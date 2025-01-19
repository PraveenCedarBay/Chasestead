using PX.Data;
using PX.Objects.AM;
using PX.Objects.AM.Attributes;
using PX.Objects.AR;
using PX.Objects.PM;

namespace EstimatesCust
{
    public class AMBomItemExt : PXCacheExtension<AMBomItem>
    {
        public static bool IsActive() => true;

        //#region UsrCADLocation
        //[PXDBString(512, IsUnicode =true)]
        //[PXUIField(DisplayName = "CAD Location")]
        //public virtual string UsrCADLocation { get; set; }
        //public abstract class usrCADLocation : PX.Data.BQL.BqlString.Field<usrCADLocation> { }
        //#endregion

        //#region UsrCustomer
       
        //[PXUIField(DisplayName = "Customer")]
        // [CustomerActive]
        //public virtual int? UsrCustomer { get; set; }
        //public abstract class usrCustomer : PX.Data.BQL.BqlInt.Field<usrCustomer> { }
        //#endregion

        //#region UsrCustPartNo
        //[PXDBString(255, IsUnicode = true)]
        //[PXUIField(DisplayName = "Customer Part No")] 
        //public virtual string UsrCustPartNo { get; set; }
        //public abstract class usrCustPartNo : PX.Data.BQL.BqlString.Field<usrCustPartNo> { }
        //#endregion 

        //#region UsrProjectID 
        //[ProjectDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        //[ActiveProjectOrContractForProd(FieldClass = ProjectAttribute.DimensionName)]
        //[PXUIField(DisplayName = "Project ID")] 
        //public virtual int? UsrProjectID { get; set; }
        //public abstract class usrProjectID : PX.Data.BQL.BqlInt.Field<usrProjectID> { }
        //#endregion
    }
}
