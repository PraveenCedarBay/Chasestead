using PX.Data;
using PX.Objects.GL.Attributes;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.Objects.GL;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM.Standalone
{
    public class AMEstimateItemExt : PXCacheExtension<PX.Objects.AM.Standalone.AMEstimateItem>
    {
        public static bool IsActive() => true;

        #region UsrCustomer
        ////[PXDBInt()]
        [PXUIField(DisplayName = "Customer")]
        //[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD))]
        //[CustomerVendorRestrictor]
        [CustomerActive]
        public virtual int? UsrCustomer { get; set; }
        public abstract class usrCustomer : PX.Data.BQL.BqlInt.Field<usrCustomer> { }
        #endregion

        //#region UsrCustomer
        //[PXDBString(99)]
        //[PXUIField(DisplayName = "Customer")]
        //public virtual string UsrCustomer { get; set; }
        //public abstract class usrCustomer : PX.Data.BQL.BqlString.Field<usrCustomer> { }
        //#endregion

        #region UsrCustPartNo
        [PXDBString(99)]
        [PXUIField(DisplayName = "CustPartNo")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string UsrCustPartNo { get; set; }
        public abstract class usrCustPartNo : PX.Data.BQL.BqlString.Field<usrCustPartNo> { }
        #endregion

        #region UsrCADLocation
        [PXDBString(512, IsUnicode =true)]
        [PXUIField(DisplayName = "CAD Location")]

        public virtual string UsrCADLocation { get; set; }
        public abstract class usrCADLocation : PX.Data.BQL.BqlString.Field<usrCADLocation> { }
        #endregion

        #region UsrProjectID
        //[PXDBString(99)]
        [ProjectDefault(PersistingCheck =PXPersistingCheck.Nothing)]
        [ActiveProjectOrContractForProd(FieldClass = ProjectAttribute.DimensionName)]
        [PXUIField(DisplayName = "Project ID")]
        public virtual int? UsrProjectID { get; set; }
        public abstract class usrProjectID : PX.Data.BQL.BqlInt.Field<usrProjectID> { }
        #endregion
    }
}