using PX.Data;
using System.Collections.Generic;
using PX.Objects.IN;
using PX.Objects.CS;

namespace POCustomization
{
    public class InventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
    {
        protected virtual void InventoryItem_UsrDocumentsRequired_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            InventoryItem row = e.Row as InventoryItem;
            if (row != null)
            {
                List<string> allowedValues = new List<string>();
                List<string> allowedLabels = new List<string>();

                foreach (CSAttributeDetail rec in PXSelect<CSAttributeDetail, Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>>.
                                         Select(Base, "PODOCS"))
                {
                    allowedValues.Add(rec.ValueID);
                    allowedLabels.Add(rec.Description);
                }
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 10, true, typeof(POLineExt.usrDocumentsRequired).Name, false, -1, string.Empty, allowedValues.ToArray(), allowedLabels.ToArray(), false, null);

                ((PXStringState)e.ReturnState).MultiSelect = true;
            }
        }
    }
}
