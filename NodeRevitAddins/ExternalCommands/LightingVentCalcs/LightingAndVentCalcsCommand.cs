using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NodeRevitAddins.ExternalCommands.LightingVentCalcs
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class LightingAndVentCalcsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                
                LightingAndVentUI ui = new LightingAndVentUI(commandData.Application);
                HwndSource hwnd = HwndSource.FromHwnd(commandData.Application.MainWindowHandle);
                Window wnd = hwnd.RootVisual as Window;
                if (wnd != null)
                {
                    ui.Owner = wnd;
                    ui.ShowInTaskbar = false;
                    ui.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                TaskDialog td = new TaskDialog("Addin info");
                td.Title = "Info";
                td.MainContent = ex.Message;
                td.Show();
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }

}
