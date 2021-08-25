using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeRevitAddins.Helpers
{
    class RevitElementComparer : IEqualityComparer<Element>
    {
        public bool Equals(Element x, Element y)
        {
            return x.UniqueId == y.UniqueId;
        }

        public int GetHashCode(Element obj)
        {
            return obj.UniqueId.GetHashCode();
        }
    }
}
