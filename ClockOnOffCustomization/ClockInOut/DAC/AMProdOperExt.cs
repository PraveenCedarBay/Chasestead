using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    public class AMProdOperExtension : PXCacheExtension<AMProdOper>
    {
        #region Selected
        [PXBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region UsrEmployeeID
        [PXDBInt()]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Employee ID", Enabled = false)]
        public virtual int? UsrEmployeeID { get; set; }
        public abstract class usrEmployeeID : BqlInt.Field<usrEmployeeID> { }
        #endregion

        #region UsrStartTime
        [PXDBDateAndTime(DisplayNameDate = "Start Date", DisplayNameTime = "Start Time", UseTimeZone = false)]
        [PXUIField(Enabled = false)]
        public virtual DateTime? UsrStartTime { get; set; }
        public abstract class usrStartTime : BqlDateTime.Field<usrStartTime> { }
        #endregion

        #region UsrPOClockOut
        [PXDBDateAndTime(UseTimeZone = false)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Clock-Out", Enabled = false)]
        public virtual DateTime? UsrPOClockOut { get; set; }
        public abstract class usrPOClockOut : BqlDateTime.Field<usrPOClockOut> { }
        #endregion

        #region UsrDuration
        [PXDBString(1, IsFixed = true)]
        [ClockTranStatus.List]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        public virtual string UsrStatus { get; set; }
        public abstract class usrStatus : BqlString.Field<usrStatus> { }
        #endregion 

        #region RelatedItems
        [PXInt]
        [PXUIField(DisplayName = "Materials Count")]
        public int? UsrMaterialsCount { get; set; }
        public abstract class usrMaterialsCount : BqlInt.Field<usrMaterialsCount> { }
        #endregion

        #region QtyComplete

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Qty Complete")]
        public virtual Decimal? UsrQtyComplete { get; set; }
        public abstract class usrQtyComplete : PX.Data.BQL.BqlDecimal.Field<usrQtyComplete> { }
        #endregion

        #region QtyScrapped

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Qty Scrapped")]
        public virtual Decimal? UsrQtyScrapped { get; set; }
        public abstract class usrQtyScrapped : PX.Data.BQL.BqlDecimal.Field<usrQtyScrapped> { }

        #endregion
    }


}

