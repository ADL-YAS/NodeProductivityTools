using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeRevitAddins
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class TestingClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {
                UIDocument uIDocument = commandData.Application.ActiveUIDocument;
                Document doc = uIDocument.Document;
                var re = uIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element).ElementId;


                Element ele = doc.GetElement(re);
                using(Transaction tr = new Transaction(doc, "test"))
                {
                    tr.Start();
                    ElementTransformUtils.MoveElement(doc, ele.Id, new XYZ(0, 5, 0));

                    tr.Commit();
                }
            }
            catch (Exception)
            {

                throw;
            }


            return Result.Succeeded;
        }
    }
}
