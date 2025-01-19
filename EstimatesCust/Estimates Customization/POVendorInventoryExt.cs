using PX.Data;

namespace PX.Objects.PO
{
    public class POVendorInventoryExt : PXCacheExtension<PX.Objects.PO.POVendorInventory>
	{
		public static bool IsActive() => true;

		#region UsrApprovalSupplier
		[PXDBBool]
		[PXUIField(DisplayName="Approval Supplier")]

		public virtual bool? UsrApprovalSupplier { get; set; }
		public abstract class usrApprovalSupplier : PX.Data.BQL.BqlBool.Field<usrApprovalSupplier> { }
		#endregion
	}
}