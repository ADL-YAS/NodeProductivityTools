using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeRevitAddins.Helpers
{
    public static class WorkSharingHelper
    {
         /// <summary>
        /// Tries to checkout the given element before editing.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>True if the element is ready to be edited, false if the element cannot be checked out or edited due to being out of date.</returns>
        public static bool AttemptToCheckoutInAdvance(Document doc,List<Element> elements)
        {

            List<ElementId> ids = elements.Select(x => x.Id).ToList();
            // Checkout attempt
            ICollection<ElementId> checkedOutIds = WorksharingUtils.CheckoutElements(doc, ids);

            // Confirm checkout
            bool checkedOutSuccessfully = elements.Count == checkedOutIds.Count;
            var result = ids.Where(p => checkedOutIds.All(p2 => p2 != p));

            if (!checkedOutSuccessfully)
            {
                WorksharingTooltipInfo tooltipInfo = WorksharingUtils.GetWorksharingTooltipInfo(doc, result.FirstOrDefault());
                throw new Exception($"Some Doors/Windows are Checkout to {tooltipInfo.Owner}.\nChanges from other users needs to sync with central to execute this command.");
            }

            // If element is updated in central or deleted in central, it is not editable
            foreach (var id in checkedOutIds)
            {
                ModelUpdatesStatus updatesStatus = WorksharingUtils.GetModelUpdatesStatus(doc, id);
                if (updatesStatus == ModelUpdatesStatus.DeletedInCentral || updatesStatus == ModelUpdatesStatus.UpdatedInCentral)
                {
                    throw new Exception("Your file is out of date from central please reload latest");
                }
            }
           
            return true;
        }

    }
}
