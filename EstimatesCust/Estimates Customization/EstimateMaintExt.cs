using PX.Data;
using PX.Objects.AM;
using PX.Objects.IN;
using PX.Objects.SO;
using System;

namespace EstimatesCust
{
    public class EstimateMaintExt : PXGraphExtension<EstimateMaint>
    {
        public static bool IsActive() => true;
        protected virtual void AMEstimateReference_BAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            AMEstimateReference row = e.Row as AMEstimateReference;
            if (row == null) return;
            if (row.BAccountID != null)
            {
                AMEstimateItem Doc = Base.Documents.Current;
                if (Doc != null)
                {
                    AMEstimateItemExt DocExt = Doc.GetExtension<AMEstimateItemExt>();
                    DocExt.UsrCustomer = row.BAccountID;
                }
            }
        }

        protected virtual void AMEstimateReference_ProjectID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            AMEstimateReference row = e.Row as AMEstimateReference;
            if (row == null) return;
            if (row.ProjectID != null)
            {
                AMEstimateItem Doc = Base.Documents.Current;
                if (Doc != null)
                {
                    AMEstimateItemExt DocExt = Doc.GetExtension<AMEstimateItemExt>();
                    DocExt.UsrProjectID = row.ProjectID;
                     
                }
            }
        }

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate del)
        {
            if (Base.Documents.Current != null)
            {
                try
                {
                    InventoryItem objItem = InventoryItem.PK.Find(Base, Base.Documents.Current?.InventoryID);
                    if (objItem != null)
                    {
                        InventoryItemMaint itemGraph = PXGraph.CreateInstance<InventoryItemMaint>();
                        itemGraph.Item.Current = objItem;
                        InventoryItemExt itemExt = itemGraph.Item.Current.GetExtension<InventoryItemExt>();
                        AMEstimateItemExt EstimateitemExt = itemGraph.Item.Current?.GetExtension<AMEstimateItemExt>();

                        if (!string.IsNullOrEmpty(EstimateitemExt?.UsrCustPartNo) && itemExt.UsrCustPartNo != EstimateitemExt.UsrCustPartNo)
                        {
                            itemExt.UsrCustPartNo = EstimateitemExt?.UsrCustPartNo;
                            itemGraph.Item.Cache.Update(itemGraph.Item.Current);
                            itemGraph.Actions.PressSave();
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw new PXException(ex.Message);
                }
            }
            del.Invoke();
        }
    }
}