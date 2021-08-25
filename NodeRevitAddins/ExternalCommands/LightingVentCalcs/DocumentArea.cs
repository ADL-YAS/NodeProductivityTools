using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;


namespace NodeRevitAddins.ExternalCommands.LightingVentCalcs
{
    public class AreaInfo
    {
        public Area Area { get; set; }
        public List<FamilyInstance> AreaDoors { get; set; }
        public List<FamilyInstance> AreaWindows { get; set; }
        //public List<Room> AreaRooms { get; private set; }

        public void SetWindowParameters(Document doc, List<SharedParameterObj> sharedParameterObjs)
        {
            foreach (FamilyInstance instance in AreaWindows)
            {
                Room room = instance.FromRoom;
                if (room != null)
                {
                    foreach (SharedParameterObj item in sharedParameterObjs)
                    {

                        Parameter par1 = instance.get_Parameter(item.Guid);
                        if (par1 != null && par1.Definition.Name == "Appartment_Unit")
                        {
                            par1.Set($"{Area.Name.Replace(Area.Number, " ")}");
                        }
                        Parameter par2 = instance.get_Parameter(item.Guid);
                        if (par2 != null && par2.Definition.Name == "LV_Building_Code")
                        {
                            double glaze = Math.Round(room.Area * 0.10, 1);
                            double vent = Math.Round(room.Area * 0.05, 1);
                            par2.Set($"REQ: LIGHT: {glaze} SF (10% FL AREA); VENT: {vent} SF (5% FL AREA)");
                        }
                        Parameter par3 = instance.get_Parameter(item.Guid);
                        if (par3 != null && par3.Definition.Name == "Room_Name + Area")
                        {
                            par2.Set($"{room.Name.Replace(room.Number," ")}AREA: {Math.Round(room.Area)} SF");
                        }
                    }
                }
            }
        }
        public void SetDoorParameters(Document doc, List<SharedParameterObj> sharedParameterObjs)
        {
            foreach (FamilyInstance instance in AreaDoors)
            {
                Room room = instance.FromRoom;
                if (room != null)
                {
                    foreach (SharedParameterObj item in sharedParameterObjs)
                    {

                        Parameter par1 = instance.get_Parameter(item.Guid);
                        if (par1 != null && par1.Definition.Name == "Appartment_Unit")
                        {
                            par1.Set($"{Area.Name.Replace(Area.Number, " ")}");
                        }
                        Parameter par2 = instance.get_Parameter(item.Guid);
                        if (par2 != null && par2.Definition.Name == "LV_Building_Code")
                        {
                            double glaze = Math.Round(room.Area * 0.10, 1);
                            double vent = Math.Round(room.Area * 0.05, 1);
                            par2.Set($"REQ: LIGHT: {glaze} SF (10% FL AREA); VENT: {vent} SF (5% FL AREA)");
                        }
                        Parameter par3 = instance.get_Parameter(item.Guid);
                        if (par3 != null && par3.Definition.Name == "Room_Name + Area")
                        {
                            par2.Set($"{room.Name.Replace(room.Number, " ")}AREA: {Math.Round(room.Area)} SF");
                        }
                    }
                }
            }
        }
    }
}
