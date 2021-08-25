using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using NodeRevitAddins.Ribbon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIFramework;

namespace NodeRevitAddins
{
    class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            NodeRibbon.AddRibbonPanel(application);
            application.ViewActivated += new EventHandler<Autodesk.Revit.UI.Events.ViewActivatedEventArgs>(OnViewActivated_Tab);
            return Result.Succeeded;
        }

        private void OnViewActivated_Tab(object sender, ViewActivatedEventArgs e)
        {

            var rc = RevitRibbonControl.RibbonControl;
            var tab = rc.FindTab("NODE");
            tab.IsVisible = false; // or true

            tab.IsVisible = !e.Document.IsFamilyDocument;
        }

    }
}
