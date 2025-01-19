using PX.Data;
using PX.Objects.GL.Attributes;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{

    public class AMEstimateItemExt : PXCacheExtension<AMEstimateItem>
    {
        public static bool IsActive() => true;

        #region UsrCustomer
        //[PXDBInt( )]
        [PXUIField(DisplayName = "Customer")]
        //[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD))]
        //[CustomerProspectVendor]
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

        #region UsrCADLocation
        [PXDBString(512, IsUnicode =true)]
        [PXUIField(DisplayName = "CAD Location")]

        public virtual string UsrCADLocation { get; set; }
        public abstract class usrCADLocation : PX.Data.BQL.BqlString.Field<usrCADLocation> { }
        #endregion

        #region UsrCustPartNo
        [PXDBString(99)]
        [PXUIField(DisplayName = "Customer Part No")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string UsrCustPartNo { get; set; }
        public abstract class usrCustPartNo : PX.Data.BQL.BqlString.Field<usrCustPartNo> { }
        #endregion

        //#region UsrProjectID
        //[PXDBString(99)]
        //[PXUIField(DisplayName = "Project ID")]

        //public virtual string UsrProjectID { get; set; }
        //public abstract class usrProjectID : PX.Data.BQL.BqlString.Field<usrProjectID> { }
        //#endregion

        #region UsrProjectID
        //[PXDBString(99)]
        [ProjectDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [ActiveProjectOrContractForProd(FieldClass = ProjectAttribute.DimensionName)]
        [PXUIField(DisplayName = "Project ID")]

        public virtual int? UsrProjectID { get; set; }
        public abstract class usrProjectID : PX.Data.BQL.BqlInt.Field<usrProjectID> { }
        #endregion
    }
}