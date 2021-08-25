using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace NodeRevitAddins.Ribbon
{
    class NodeRibbon
    {
        internal static void AddRibbonPanel(UIControlledApplication application)
        {
            string TabName = "NODE";

            application.CreateRibbonTab(TabName);

            string AssemblyPath = Assembly.GetExecutingAssembly().Location;

            #region CreatePanel for button
            RibbonPanel Calcs_Panel = application.CreateRibbonPanel(TabName, "Calculation");

            #endregion



            #region Create Image for button
            BitmapImage LV_Image = new BitmapImage(new Uri("pack://application:,,,/NodeRevitAddins;component/Resources/Vent32x32.png"));

            #endregion

            #region Create button data
            //LV command
            PushButtonData LVButtonData = new PushButtonData("LV Button", "Light and Vent", AssemblyPath, "NodeRevitAddins.ExternalCommands.LightingVentCalcs.LightingAndVentCalcsCommand");
            #endregion

            #region Create push button from Pushbuttondata
            PushButton LVPushButton = Calcs_Panel.AddItem(LVButtonData) as PushButton;
            #endregion

            #region ToolTips
            LVPushButton.ToolTip = "Calculate Lighting and Vent building requirements\n to be format in Schedule View";
            #endregion


            #region Sett button image
            LVPushButton.LargeImage = LV_Image;
            #endregion

        }
    }
}
