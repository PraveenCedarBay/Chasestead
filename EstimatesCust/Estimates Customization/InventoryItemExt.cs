using PX.Data;
using PX.Objects.CR;
using System;
using PX.Objects.GL.Attributes;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.IN
{

    [Serializable]
    public class InventoryItemExt : PXCacheExtension<InventoryItem>
    {
        public static bool IsActive() => true;

        #region UsrCertRequired
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)] 
        [PXUIField(DisplayName = "Certificate required")]

        public virtual bool? UsrCertRequired { get; set; }
        public abstract class usrCertRequired : PX.Data.BQL.BqlBool.Field<usrCertRequired> { }
        #endregion

        #region UsrCertReceived
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)] 
        [PXUIField(DisplayName = "Certificate received")]

        public virtual bool? UsrCertReceived { get; set; }
        public abstract class usrCertReceived : PX.Data.BQL.BqlBool.Field<usrCertReceived> { }
        #endregion

        #region UsrCertRenewal
        [PXDBDate]
       
        [PXUIField(DisplayName = "Certificate renewal")]

        public virtual DateTime? UsrCertRenewal { get; set; }
        public abstract class usrCertRenewal : PX.Data.BQL.BqlDateTime.Field<usrCertRenewal> { }
        #endregion

        #region UsrCertLocation
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Certificate location")]

        public virtual string UsrCertLocation { get; set; }
        public abstract class usrCertLocation : PX.Data.BQL.BqlString.Field<usrCertLocation> { }
        #endregion

        #region UsrCADLocation
        [PXDBString(512, IsUnicode = true)]
        [PXUIField(DisplayName = "CAD Location")]

        public virtual string UsrCADLocation { get; set; }
        public abstract class usrCADLocation : PX.Data.BQL.BqlString.Field<usrCADLocation> { }
        #endregion

        #region UsrAerospaceApproved
        [PXDBBool]
        [PXUIField(DisplayName = "Aerospace Approved")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]

        public virtual bool? UsrAerospaceApproved { get; set; }
        public abstract class usrAerospaceApproved : PX.Data.BQL.BqlBool.Field<usrAerospaceApproved> { }
        #endregion 

        #region UsrCustomer
        // [PXDBInt()]
        [PXUIField(DisplayName = "Customer")]
        //[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD))]
        //[CustomerVendorRestrictor]
        [CustomerActive]
        public virtual int? UsrCustomer { get; set; }
        public abstract class usrCustomer : PX.Data.BQL.BqlInt.Field<usrCustomer> { }
        #endregion

        #region UsrCustPartNo
        [PXDBString(99)]
        [PXUIField(DisplayName = "Customer Part No")]

        public virtual string UsrCustPartNo { get; set; }
        public abstract class usrCustPartNo : PX.Data.BQL.BqlString.Field<usrCustPartNo> { }
        #endregion 

        #region UsrProjectID 
        [ProjectDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [ActiveProjectOrContractForProd(FieldClass = ProjectAttribute.DimensionName)]
        [PXUIField(DisplayName = "Project ID")]

        public virtual int? UsrProjectID { get; set; }
        public abstract class usrProjectID : PX.Data.BQL.BqlInt.Field<usrProjectID> { }
        #endregion

        #region UsrImportLastUseDate
        [PXDBDate]
        [PXUIField(DisplayName = "Imported Last Use Date")]

        public virtual DateTime? UsrImportLastUseDate { get; set; }
        public abstract class usrImportLastUseDate : PX.Data.BQL.BqlDateTime.Field<usrImportLastUseDate> { }
        #endregion

        #region UsrCommodity
        [PXDBString(12, IsUnicode = true)]
        [PXUIField(DisplayName = "Commodity Code")]
        public virtual string UsrCommodity { get; set; }
        public abstract class usrCommodity : PX.Data.BQL.BqlString.Field<usrCommodity> { }
        #endregion


        #region UsrCertReceived
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "CAD Check")]

        public virtual bool? UsrCADCheck { get; set; }
        public abstract class usrCADCheck : PX.Data.BQL.BqlBool.Field<usrCADCheck> { }
        #endregion
    }
}