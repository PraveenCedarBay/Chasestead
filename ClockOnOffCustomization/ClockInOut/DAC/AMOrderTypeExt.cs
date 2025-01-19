using PX.Data;
using PX.Objects.AM;
using System;

namespace ClockOnOffCustomization
{
    public class AMOrderTypeExt : PXCacheExtension<AMOrderType>
    {

        #region UsrUnderIssueMaterialPerc

        [PXDBDecimal(MinValue = 1, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Under Issue Material by %")]
        public virtual Decimal? UsrUnderIssueMaterialPerc { get; set; }
        public abstract class usrUnderIssueMaterialPerc : PX.Data.BQL.BqlDecimal.Field<usrUnderIssueMaterialPerc> { }
        #endregion

        #region UsrOverIssueMaterialPerc

        [PXDBDecimal(MinValue = 100, MaxValue = 1000)]
        [PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Over Issue Material by %")]
        public virtual Decimal? UsrOverIssueMaterialPerc { get; set; }
        public abstract class usrOverIssueMaterialPerc : PX.Data.BQL.BqlDecimal.Field<usrOverIssueMaterialPerc> { }
        #endregion
    }
}
