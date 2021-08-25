using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NodeRevitAddins.Helpers;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace NodeRevitAddins.ExternalCommands.LightingVentCalcs
{
    /// <summary>
    /// Interaction logic for LightingAndVentUI.xaml
    /// </summary>
    public partial class LightingAndVentUI : Window
    {
        Document doc;
        UIDocument uidoc;
        Autodesk.Revit.ApplicationServices.Application app;
        Autodesk.Revit.Creation.Application creaTeApp;
        DefinitionGroup definitionGroup;

        List<SharedParameterObj> LVParameters;
        private readonly ObservableCollection<CustomAreaClass> customAreaClasses;
        public ICollectionView CustomAreaCollectionView { get; }
        
        public LightingAndVentUI(UIApplication uiapp)
        { 
            InitializeComponent();
            doc = uiapp.ActiveUIDocument.Document;
            app = uiapp.Application;
            creaTeApp = uiapp.Application.Create;
            uidoc = uiapp.ActiveUIDocument;
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);

            customAreaClasses = new ObservableCollection<CustomAreaClass>(GetGrossArea().OrderBy(x => x.Name).ToList());

            InitializeData();
        }


        private void InitializeData()
        {
            LVParameters = new List<SharedParameterObj>()
            {
                new SharedParameterObj(){Name = "Appartment_Unit", Type = ParameterType.Text, Guid = new Guid("83ec66e9-035e-4657-9577-d99fc3e5785c")},
                new SharedParameterObj(){Name = "Room_Name + Area", Type = ParameterType.Text, Guid = new Guid("6a3f124a-55c5-4b71-a7ff-50e2fcdb7b80")},
                new SharedParameterObj(){Name = "LV_Building_Code", Type = ParameterType.Text, Guid = new Guid("13b84a84-1d0f-465f-8168-d52da71e5fd4")}
            };

            if (customAreaClasses.Count != 0)
            {
                string selected = Properties.Settings.Default["LVpref"].ToString();
                int count = selected.Length;
                if (selected != null)
                {
                    string[] array = selected.Split('*');
                    foreach (string s in array)
                    {
                        var obj = customAreaClasses.Where(x => x.Uid == s).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.IsChecked = true;
                        }
                    }
                }
                UiAreas.ItemsSource = customAreaClasses;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(UiAreas.ItemsSource);
                view.Filter = AreasFilter;
            }
            else
            {
                TaskDialog.Show("Info", "No Gross building Area found");
            }
        }

        private bool AreasFilter(object item)
        {
            if (string.IsNullOrEmpty(ListFilter.Text))
                return true;
            else
                return (item as CustomAreaClass).Name.IndexOf(ListFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private void ListFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

            CollectionViewSource.GetDefaultView(UiAreas.ItemsSource).Refresh();
        }

        List<CustomAreaClass> GetGrossArea()
        {
            List<SpatialElement> spElements = new FilteredElementCollector(doc).OfClass(typeof(SpatialElement)).Cast<SpatialElement>().ToList();
            List<CustomAreaClass> areaList = new List<CustomAreaClass>();
            foreach (var item in spElements)
            {
                Area area = item as Area;
                if (null != area && area.AreaScheme.IsGrossBuildingArea == true)
                {
                    areaList.Add(new CustomAreaClass() { Name = area.Name.Replace(area.Number, " "), Area = area , Uid=area.UniqueId});
                }
            }
            return areaList;
        }

        void CallFunctions()
        {
            using (Transaction tr = new Transaction(doc, "Test"))
            {
                tr.Start();
                //check and bind parameters
                BindParameters();

                var li = customAreaClasses.Where(x => x.IsChecked == true).ToList();
                List<AreaInfo> docAreas = ProcessAreas(li);

                //tr.RollBack();
                foreach (AreaInfo info in docAreas)
                {
                    info.SetWindowParameters(doc, LVParameters);
                    info.SetDoorParameters(doc, LVParameters);
                }
                if (tr.Commit() == TransactionStatus.Committed)
                {
                    TaskDialog.Show("Info", "Parameters updated! Please consider Synchronize with Central");
                }
            }
            
        }

        /// <summary>
        /// bind parameters
        /// </summary>
        void BindParameters()
        {
            if (doc.IsFamilyDocument)
            {
                throw new Exception(
                  "Document can not be a family document.");
            }


            CategorySet categorySet = new CategorySet();
            categorySet.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Doors));
            categorySet.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Windows));

            Element door = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors).FirstOrDefault();
            Element window = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Windows).FirstOrDefault();

            if(door != null && window != null)
            {
                //check each parameter if already binded
                foreach (SharedParameterObj item in LVParameters)
                {
                    Parameter doorParameter = door.get_Parameter(item.Guid);
                    Parameter windowParameter = window.get_Parameter(item.Guid);
                    if (doorParameter == null && windowParameter == null)
                    {
                        InitializeSharedParameter();
                        InstanceBinding binding = app.Create.NewInstanceBinding(categorySet);
                        Definition def = definitionGroup.Definitions.get_Item(item.Name);
                        doc.ParameterBindings.Insert(def, binding);
                    }
                    if (doorParameter != null && windowParameter == null || doorParameter == null && windowParameter != null)
                    {
                        throw new Exception("Please check if LV Calculation required parameters are applied to Doors and Windows");
                    }
                }
            }
            else
            {
                throw new Exception("Unable to collect Window or Door for parameter binding check.");
            }
           

        }

        /// <summary>
        /// process Areas and get doors and windows
        /// </summary>
        /// <returns></returns>
        /// 
        List<AreaInfo> ProcessAreas(List<CustomAreaClass> areafromUi)
        {
            List<BuiltInCategory> cats = new List<BuiltInCategory>();
            cats.Add(BuiltInCategory.OST_Doors);
            cats.Add(BuiltInCategory.OST_Windows);
            ElementMulticategoryFilter multiCat = new ElementMulticategoryFilter(cats);

            //UI input
            List<Area> grossAreasFromUI = areafromUi.Where(x => x.IsChecked = true).Select(a=>a.Area).ToList();


            //function return
            List<AreaInfo> areaInfos = new List<AreaInfo>();

            //process all area boundaries to filter boundaries that are not closed
            Dictionary<Area,List<Curve>> AreaCurvesDict = new Dictionary<Area, List<Curve>>();
            List<Area> ConflictedArea = new List<Area>();
            foreach (Area area in grossAreasFromUI)
            {
                List<Curve> crvs = new List<Curve>();
                SpatialElementBoundaryOptions elementBoundaryOptions = new SpatialElementBoundaryOptions();
                foreach (var bsegmentList in area.GetBoundarySegments(elementBoundaryOptions))
                {
                    foreach (BoundarySegment segment in bsegmentList)
                    {
                        Curve curve = segment.GetCurve();
                        crvs.Add(curve);
                    }
                }
                try
                {
                    //jt sort curves
                    CurveUtils.SortCurvesContiguous(creaTeApp, crvs, true);
                    AreaCurvesDict.Add(area,crvs);
                }
                catch
                {
                    ConflictedArea.Add(area);
                }
            }

            //throw exception if there is one open area boundaries
            if(ConflictedArea.Count() != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in ConflictedArea)
                {
                    sb.Append($"Name: {item.Name.Replace(item.Number, " ")} Number: {item.Number}\n");
                }
                throw new Exception($"Please make sure the following Areas has closed boundaries:\n {sb}");
            }
            else
            {
                //check for checkout elements and get all element in the specific areas
                //this will return list of Areainfo object
                List<Element> fInstances = new FilteredElementCollector(doc).WherePasses(multiCat).WhereElementIsNotElementType().ToElements().ToList();
                bool isEditable = WorkSharingHelper.AttemptToCheckoutInAdvance(doc, fInstances);
                if(isEditable)
                {
                    foreach (var kv in AreaCurvesDict)
                    {
                        CurveLoop curveLoop = CurveLoop.Create(kv.Value);
                        Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(new CurveLoop[] { curveLoop }, XYZ.BasisZ, 7);
                        ElementIntersectsSolidFilter filter = new ElementIntersectsSolidFilter(solid);
                        List<Element> fromSolid = new FilteredElementCollector(doc).WherePasses(filter).ToList();
                        var elementInside = fromSolid.Intersect(fInstances, new RevitElementComparer()).ToList();

                        //intialize object to add to the function return
                        AreaInfo aInfo = new AreaInfo();
                        aInfo.Area = kv.Key;
                        aInfo.AreaDoors = elementInside.Where(x => isDoorWithGlazing(x)).Cast<FamilyInstance>().ToList();
                        aInfo.AreaWindows = elementInside.Where(x => x.Category.Name == "Windows").Cast<FamilyInstance>().ToList();
                        areaInfos.Add(aInfo);
                    }
                }
            }
            return areaInfos;
        }


        bool isDoorWithGlazing(Element e)
        {
            FamilyInstance instance = e as FamilyInstance ;
            if(instance != null && instance.Category.Name == "Doors")
            {
                Parameter par = instance.Symbol.LookupParameter("Window Glazing_Area_Total");
                if(par != null)
                {
                    return true;
                }
            }
            return false;
        }
        

        /// <summary>
        /// Initialized parameters
        /// </summary>
        void InitializeSharedParameter()
        {
            //gets or create shared parameter file
            DefinitionFile definitionFile = SharedParameterHelper.CreateSharedParameterFile(app);
            definitionGroup = SharedParameterHelper.GetDefinitionGroup(definitionFile, "Lighting and Vent Calcs");

            foreach (SharedParameterObj item in LVParameters)
            {
                if (null == definitionGroup.Definitions.get_Item(item.Name))
                {
                    ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(item.Name, item.Type);
                    opt.UserModifiable = false;
                    opt.Visible = true;
                    opt.GUID = item.Guid;
                    definitionGroup.Definitions.Create(opt);
                }
            }
        }


        class CustomAreaClass : INotifyPropertyChanged
        {
            public bool IsChecked { get; set; }
            public string Name { get; set; }
            public string Uid { get; set; }
            public Area Area { get; set; }
            public event PropertyChangedEventHandler PropertyChanged ;
        }


        /////////////////events //////
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                AnimateClose();
        }


        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            var list = customAreaClasses.Where(x => x.IsChecked == true).ToList();

            try
            {
                
                string[] arr = list.Select(x => x.Uid).ToArray();
                string uids = string.Join("*", arr);
                Properties.Settings.Default["LVpref"] = uids;
                Properties.Settings.Default.Save();
            }
            catch
            {
                TaskDialog.Show("Info", "Unable to save selected items in the application.");
            }

            if (list.Count() != 0)
            {
                AnimateClose();
                CallFunctions();
            }
            else
            {
                TaskDialog.Show("Info", "No item selected");
                AnimateClose();
            }

        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            AnimateClose();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            AnimateClose();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => WindowStyle = WindowStyle.None));
        }

        private void AnimateClose()
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            this.Close();
        }

        
    }
}
