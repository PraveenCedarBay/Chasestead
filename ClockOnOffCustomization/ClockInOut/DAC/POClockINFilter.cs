using System;
using PX.Data;
using PX.Data.BQL;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Objects.AM.Attributes;
using PX.Objects.EP;
using PX.Objects.SO;
using System.Collections.Generic;

namespace PX.Objects.AM
{
    [PXHidden]
    public class POClockINFilter : PXBqlTable, IBqlTable
    {
        #region EmployeeID
        [PXInt] 
        [ProductionEmployeeSelector]
        //[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? EmployeeID { get; set; }
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        #endregion 

        #region ProdOrderNbr   
        [PXString(30, IsUnicode = true)]
        [ProductionNbr] 
        [PXSelector(typeof(Search<
            AMProdItem.prodOrdID,
            Where<AMProdItem.statusID, NotEqual<ProductionOrderStatus.closed>,
                And<AMProdItem.statusID, NotEqual<ProductionOrderStatus.cancel>>>>), ValidateValue = false)]

        [PXUIField(DisplayName = "Produciton Nbr.")]
        public virtual string ProdOrderNbr { get; set; }
        public abstract class prodOrderNbr : PX.Data.BQL.BqlString.Field<prodOrderNbr> { }
        #endregion

        #region Status

        [PXDefault(typeof(ClockTranStatus.clockedIn))]
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status" )]
        [ClockTranStatus.List]
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion

        //#region ToolID1
        //[PXSelector(typeof(Search<AMToolMst.toolID>))]
        //[PXUIField(DisplayName = "Tool ID 1")]
        //[ToolIDField]

        //public virtual string UsrToolID1 { get; set; }
        //public abstract class usrToolID1 : BqlString.Field<usrToolID1> { }
        //#endregion 

        //#region ToolID2
        //[PXSelector(typeof(Search<AMToolMst.toolID>))]
        //[ToolIDField]

        //[PXUIField(DisplayName = "Tool ID 2")]
        //public virtual string UsrToolID2 { get; set; }
        //public abstract class usrToolID2 : BqlString.Field<usrToolID2> { }
        //#endregion

        //#region ToolID3
        //[PXSelector(typeof(Search<AMToolMst.toolID>))]
        //[ToolIDField]

        //[PXUIField(DisplayName = "Tool ID 3")]
        //public virtual string UsrToolID3 { get; set; }
        //public abstract class usrToolID3 : BqlString.Field<usrToolID3> { }
        //#endregion 

        #region FromDate
        [PXDate()]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "From Date")]
        public virtual DateTime? FromDate { get; set; }
        public abstract class fromDate : IBqlField { }
        #endregion

        #region TODate
        [PXDate()]
        // [PXFormula(typeof(IIf<Where<SOImportFilter.actions, NotEqual<AMIConstants.ActionsList.sASSDATEFROMEXCEL>>, AccessInfo.businessDate, Null>))]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? TODate { get; set; }
        public abstract class tODate : IBqlField { }
        #endregion
    }
}
