//using PX.Data;
//using PX.Objects.AM;
//using PX.Objects.IN;


//namespace EstimatesCust
//{
//    public class BOMMaintExt : PXGraphExtension<BOMMaint>
//    {
//        public static bool IsActive() => true;

//        protected virtual void AMBomItem_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
//        {
//            InvokeBaseHandler?.Invoke(cache, e);
//            AMBomItem row = e.Row as AMBomItem;
//            if (row == null) return;

//            InventoryItem objItem = InventoryItem.PK.Find(Base, row?.InventoryID);
//            if (objItem != null)
//            {
//                AMBomItemExt rowExt = row.GetExtension<AMBomItemExt>();
//                InventoryItemExt itemExt = objItem.GetExtension<InventoryItemExt>();
//                rowExt.UsrCADLocation = itemExt.UsrCADLocation;
//                rowExt.UsrCustomer = itemExt.UsrCustomer;
//                rowExt.UsrCustPartNo = itemExt.UsrCustPartNo;
//                rowExt.UsrProjectID = itemExt.UsrProjectID;
//            }
//        }
//        public delegate void PersistDelegate();
//        [PXOverride]
//        public void Persist(PersistDelegate del)
//        {
//            if (Base.Documents.Current != null)
//            {
//                InventoryItem objItem = InventoryItem.PK.Find(Base, Base.Documents.Current?.InventoryID);
//                if (objItem != null)
//                {
//                    InventoryItemMaint itemMaint = PXGraph.CreateInstance<InventoryItemMaint>();
//                    itemMaint.Item.Current = objItem;
//                    InventoryItemExt itemExt = itemMaint.Item.Current?.GetExtension<InventoryItemExt>();
//                    AMBomItemExt rowExt = Base.Documents.Current?.GetExtension<AMBomItemExt>();
//                    itemExt.UsrCADLocation = rowExt?.UsrCADLocation;
//                    itemExt.UsrCustomer = rowExt?.UsrCustomer;
//                    itemExt.UsrCustPartNo = rowExt?.UsrCustPartNo;
//                    itemExt.UsrProjectID = rowExt?.UsrProjectID;

//                    itemMaint.Item.Cache.Update(itemMaint.Item.Current);
//                    itemMaint.Save.Press();
//                }
//            }
//            del.Invoke();
//        }
//    }
//}