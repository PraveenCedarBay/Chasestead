using PX.Data;
using PX.Objects.IN;
using PX.Objects.CR;
using static PX.Objects.AM.CreateInventoryItemProcess;

namespace PX.Objects.AM.Standalone
{
    [PXProtectedAccess]
    public abstract class CreateInventoryItemProcessProtectedExtension : PXGraphExtension<CreateInventoryItemProcessExt, CreateInventoryItemProcess>
    {
        [PXProtectedAccess()]
        protected abstract InventoryItem CreateInventoryItem(InventoryItemMaintBase graph, NonInventoryItem row);

    }
    public class CreateInventoryItemProcessExt : PXGraphExtension<CreateInventoryItemProcess>
    {
        public static bool IsActive() => true;

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXUnboundDefault(true)]
        protected virtual void CreateInventoryItemFilter_CopyFiles_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXUnboundDefault(true)]
        protected virtual void CreateInventoryItemFilter_CopyDetailedDescription_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXUnboundDefault(true)]
        protected virtual void CreateInventoryItemFilter_CopyNotes_CacheAttached(PXCache cache)
        {
        }

        public delegate InventoryItem CreateInventoryItemDelegate(InventoryItemMaintBase graph, NonInventoryItem row);
        [PXOverride]
        public virtual InventoryItem CreateInventoryItem(InventoryItemMaintBase graph, NonInventoryItem row, CreateInventoryItemDelegate del)
        {
            var delmethod = del?.Invoke(graph, row);

            AMEstimateItem estimateItem = PXSelect<AMEstimateItem,
                       Where<AMEstimateItem.estimateID, Equal<Required<AMEstimateItem.estimateID>>,
                           And<AMEstimateItem.revisionID, Equal<Required<AMEstimateItem.revisionID>>
                           >>>.Select(Base, row.EstimateID, row.RevisionID);

            if (estimateItem != null)
            {
                InventoryItem inventoryItem = PXSelect<InventoryItem,
                      Where<InventoryItem.inventoryCD, Equal<Required<AMEstimateItem.inventoryCD>>
                          >>.Select(Base, row.InventoryCD.Trim());
                if (inventoryItem != null)
                {
                    InventoryItemMaint Invgraph = PXGraph.CreateInstance<InventoryItemMaint>();
                    Invgraph.Item.Current = inventoryItem;

                    InventoryItemExt rowExt = Invgraph.Item.Current.GetExtension<InventoryItemExt>();
                    AMEstimateItemExt rowEstimateItemExt = estimateItem.GetExtension<AMEstimateItemExt>();
                    Invgraph.Item.Cache.SetValueExt<InventoryItemExt.usrCustomer>(Invgraph.Item.Current, rowEstimateItemExt?.UsrCustomer);
                    Invgraph.Item.Cache.SetValueExt<InventoryItemExt.usrCustPartNo>(Invgraph.Item.Current, rowEstimateItemExt?.UsrCustPartNo);
                    Invgraph.Item.Cache.SetValueExt<InventoryItemExt.usrCADLocation>(Invgraph.Item.Current, rowEstimateItemExt?.UsrCADLocation);
                    Invgraph.Item.Cache.SetValueExt<InventoryItemExt.usrProjectID>(Invgraph.Item.Current, rowEstimateItemExt?.UsrProjectID);
                    Invgraph.Item.Cache.Update(Invgraph.Item.Current);

                    Invgraph.ItemSettings.Current.ImageUrl = estimateItem.ImageURL;
                    Invgraph.ItemSettings.Cache.Update(Invgraph.ItemSettings.Current);

                    //if (!string.IsNullOrEmpty(rowExt?.UsrCustomer))
                    if (rowExt?.UsrCustomer != null)
                    {
                        //BAccount objBAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(Invgraph, rowExt?.UsrCustomer.Trim());
                        // if (objBAccount != null)
                        // {
                        INItemXRef newCrossRef = new INItemXRef();
                        newCrossRef.InventoryID = Invgraph.Item.Current.InventoryID;
                        newCrossRef.AlternateType = INAlternateType.CPN;
                        //newCrossRef.SubItemID = Base.itemxrefrecords.Current.SubItemID;                            
                        newCrossRef.AlternateID = rowExt?.UsrCustPartNo;
                        newCrossRef.BAccountID = rowExt?.UsrCustomer;
                        newCrossRef.Descr = estimateItem?.ItemDesc;

                        Invgraph.itemxrefrecords.Cache.Insert(newCrossRef);
                        // }
                    }
                    Invgraph.Actions.PressSave();
                }
            }
            return delmethod;
        }

    }

}