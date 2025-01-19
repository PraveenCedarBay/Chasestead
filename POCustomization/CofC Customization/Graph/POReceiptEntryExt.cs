using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POCustomization
{
    public class POReceiptEntryExt : PXGraphExtension<POReceiptEntry>
    {
        public static bool IsActive() => true;

        protected virtual void POReceipt_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            POReceipt row = e.Row as POReceipt;
            if (row != null)
            {
                POReceiptExt rowExt = row.GetExtension<POReceiptExt>(); 
                 
                if (rowExt.UsrEnableReleaseButton == false)
                {
                    if (Base.transactions.Select().FirstTableItems.ToList().Where(x => x.GetExtension<POReceiptLineExt>().UsrRequireQualityDocuments == true && 
                                                                                       x.GetExtension<POReceiptLineExt>().UsrQualityDocumentsReceived != true).Any())
                    {
                        Base.release.SetVisible(false);
                    }
                    else
                    {
                        Base.release.SetVisible(true);
                    }
                }
                else
                    Base.release.SetVisible(true);
            }
        }
        protected virtual void POReceiptLine_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        {
            InvokeBaseHandler?.Invoke(cache, e);
            POReceiptLine row = e.Row as POReceiptLine;
            if (row != null)
            {
                POReceiptLineExt rowExt = row.GetExtension<POReceiptLineExt>();
                PXUIFieldAttribute.SetEnabled<POReceiptLineExt.usrQualityDocumentsReceived>(cache, row, rowExt.UsrRequireQualityDocuments == true);
                PXUIFieldAttribute.SetEnabled<POReceiptLineExt.usrDocumentsRequired>(cache, row, rowExt.UsrRequireQualityDocuments == true);
            }
        }
        protected virtual void POReceiptLine_UsrDocumentsRequired_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            POReceiptLine row = e.Row as POReceiptLine;
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
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 10, true, typeof(POReceiptLineExt.usrDocumentsRequired).Name, false, -1, string.Empty, allowedValues.ToArray(), allowedLabels.ToArray(), false, null);

                ((PXStringState)e.ReturnState).MultiSelect = true;
            }
        }

        public PXAction<POReceipt> RequestCofC;
        [PXUIField(DisplayName = "Request C of C", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton()]
        public virtual IEnumerable requestCofC(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                PXLongOperation.StartOperation(Base, delegate ()
                {
                    POReceiptExt rowExt = Base.Document.Current.GetExtension<POReceiptExt>();
                    rowExt.UsrRequestCofCCounter = (rowExt.UsrRequestCofCCounter ?? 0) + 1;
                    Base.Document.Cache.Update(Base.Document.Current);


                    string getNoteText = PXNoteAttribute.GetNote(Base.Document.Cache, Base.Document.Current);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(getNoteText);
                    sb.Append("\n");
                    var val = PXTimeZoneInfo.Now.ToString() + " " + "Request C of C email sent.";
                    sb.Append(val);


                    PXNoteAttribute.SetNote(Base.Document.Cache, Base.Document.Current, sb.ToString());

                    Base.Actions.PressSave();
                    Base.Document.View.RequestRefresh();
                });
            } 
            return adapter.Get();
        }
    }
}
