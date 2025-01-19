using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.IN;
using System.Collections.Generic;

namespace POCustomization
{
    public class POOrderEntryExt : PXGraphExtension<POOrderEntry>
    {

        [PXViewName(PX.Objects.PO.Messages.POLine)]
        [PXImport(typeof(POOrder))]
        [PXCopyPasteHiddenFields(typeof(POLine.closed), typeof(POLine.cancelled), typeof(POLine.completed), typeof(POLineExt.usrRequireQualityDocuments), typeof(POLineExt.usrDocumentsRequired))]
        public PXOrderedSelect<POOrder, POLine,
            Where<POLine.orderType, Equal<Current<POOrder.orderType>>,
                And<POLine.orderNbr, Equal<Optional<POOrder.orderNbr>>>>,
            OrderBy<Asc<POLine.orderType, Asc<POLine.orderNbr, Asc<POLine.sortOrder, Asc<POLine.lineNbr>>>>>> Transactions;


        protected virtual void POLine_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            POLine row = e.Row as POLine;
            if (row != null && Base.Document.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<POLineExt.usrDocumentsRequired>(cache, row, (row.GetExtension<POLineExt>()?.UsrRequireQualityDocuments ?? false));
            }
        }



        protected virtual void POLine_UsrRequireQualityDocuments_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            POLine row = e.Row as POLine;
            if (row != null && Base.Document.Current != null && !(row.GetExtension<POLineExt>()?.UsrRequireQualityDocuments ?? false))
            {
                row.GetExtension<POLineExt>().UsrDocumentsRequired = string.Empty;
            }
        }
        protected virtual void POLine_UsrDocumentsRequired_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            POLine row = e.Row as POLine;
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

        protected virtual void POLine_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            POLine row = e.Row as POLine;
            if (row != null)
            {
                InventoryItem objItem = InventoryItem.PK.Find(Base, row?.InventoryID);
                if (objItem != null)
                {
                    InventoryItemExt itemExt = objItem.GetExtension<InventoryItemExt>();
                    POLineExt rowExt = row.GetExtension<POLineExt>();
                    rowExt.UsrDocumentsRequired = itemExt.UsrDocumentsRequired;
                    rowExt.UsrRequireQualityDocuments = itemExt.UsrRequireQualityDocuments;
                }
            }
        }
    }
}