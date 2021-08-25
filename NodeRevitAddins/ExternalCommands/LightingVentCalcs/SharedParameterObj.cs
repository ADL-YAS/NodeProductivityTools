using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeRevitAddins.ExternalCommands.LightingVentCalcs
{
    public class SharedParameterObj
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public ParameterType Type { get; set; }
    }
}
