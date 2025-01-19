using PX.Data;
using PX.Objects.PO;

namespace POCustomization
{
    public class POReceiptExt : PXCacheExtension<POReceipt>
    {
        public static bool IsActive() => true;

        #region UsrRequestCofCCounter
        [PXDBInt()]
        [PXUIField(DisplayName = "CofC Counter", Visible =false)]
        public int? UsrRequestCofCCounter { get; set; }
        public abstract class usrRequestCofCCounter : PX.Data.BQL.BqlInt.Field<usrRequestCofCCounter> { }
        #endregion

        #region UsrEnableReleaseButton
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Enable Release")]
        public bool? UsrEnableReleaseButton { get; set; }
        public abstract class usrEnableReleaseButton : PX.Data.BQL.BqlBool.Field<usrEnableReleaseButton> { }
        #endregion 
    }
}