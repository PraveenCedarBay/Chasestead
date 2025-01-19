using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;
using System.Collections.Generic;
using System.Linq;

namespace POCustomization
{
    public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
    {
        public static bool IsActive() => true;
        protected virtual void APTran_POLineNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            APTran row = e.Row as APTran;
            if (row != null)
            {
                POLine pOLine = PXSelect<POLine,
                              Where<POLine.orderType, Equal<Required<POLine.orderType>>,
                              And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
                              And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>.
                              Select(Base, row.POOrderType, row.PONbr, row.POLineNbr);
                APTranExt rowExt = row.GetExtension<APTranExt>();

                rowExt.UsrPOQualityRequirement = (pOLine?.GetExtension<POLineExt>()?.UsrRequireQualityDocuments ?? false);
                rowExt.UsrDocumentsRequired = pOLine?.GetExtension<POLineExt>()?.UsrDocumentsRequired;

                if (row != null && Base.Document.Current != null)
                {
                    //APTranExt rowExt = row.GetExtension<APTranExt>();

                    //if (rowExt?.UsrPOQualityRequirement == true && rowExt?.UsrReceiptQualityDocsReceived == false && rowExt?.UsrLateDocumentSubmission == false)
                    if (rowExt?.UsrPOQualityRequirement != false && rowExt?.UsrReceiptQualityDocsReceived != true && rowExt?.UsrLateDocumentSubmission != true)
                    {
                        rowExt.UsrNonComplianceHold = true;
                    }
                    else
                    {
                        rowExt.UsrNonComplianceHold = false;
                    }
                }

            }
        }
        protected virtual void APTran_ReceiptLineNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            APTran row = e.Row as APTran;
            if (row != null)
            {
                POReceiptLine pOReceiptLine = PXSelect<POReceiptLine,
                                 Where<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
                                 And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
                                 And<POReceiptLine.lineNbr, Equal<Required<POReceiptLine.lineNbr>>>>>>.
                                 Select(Base, row?.ReceiptType, row?.ReceiptNbr, row?.ReceiptLineNbr);

                APTranExt rowExt = row.GetExtension<APTranExt>();
                rowExt.UsrPOQualityRequirement = (pOReceiptLine?.GetExtension<POReceiptLineExt>()?.UsrRequireQualityDocuments ?? false);
                rowExt.UsrDocumentsRequired = pOReceiptLine?.GetExtension<POReceiptLineExt>()?.UsrDocumentsRequired;
                rowExt.UsrReceiptQualityDocsReceived = (pOReceiptLine?.GetExtension<POReceiptLineExt>()?.UsrQualityDocumentsReceived ?? false);


                if (row != null && Base.Document.Current != null)
                {
                    //APTranExt rowExt = row.GetExtension<APTranExt>();

                    // if (rowExt?.UsrPOQualityRequirement == true && rowExt?.UsrReceiptQualityDocsReceived == false && rowExt?.UsrLateDocumentSubmission == false)
                    if (rowExt?.UsrPOQualityRequirement != false && rowExt?.UsrReceiptQualityDocsReceived != true && rowExt?.UsrLateDocumentSubmission != true)
                    {
                        rowExt.UsrNonComplianceHold = true;
                    }
                    else
                    {
                        rowExt.UsrNonComplianceHold = false;
                    }
                }
            }
        }
        protected virtual void APTran_UsrDocumentsRequired_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            APTran row = e.Row as APTran;
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
        protected virtual void APTran_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            APTran row = e.Row as APTran;
            if (row != null && Base.Document.Current != null)
            {
                APTranExt rowExt = row.GetExtension<APTranExt>();

                if (rowExt?.UsrPOQualityRequirement == true && rowExt?.UsrReceiptQualityDocsReceived == false && rowExt?.UsrLateDocumentSubmission == false)
                {
                    rowExt.UsrNonComplianceHold = true;
                }
                else
                {
                    rowExt.UsrNonComplianceHold = false;
                }
            }
        }
        protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            APInvoice row = e.Row as APInvoice;
            if (row != null)
            {
                if (Base.Transactions.Select().FirstTableItems.ToList().Where(x => x.GetExtension<APTranExt>().UsrNonComplianceHold == true).Any())
                {
                    Base.release.SetVisible(false);
                }
                else
                {
                    Base.release.SetVisible(true);
                }
            }
        }
    }
}