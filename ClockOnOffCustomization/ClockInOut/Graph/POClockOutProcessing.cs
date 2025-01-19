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
using static PX.Data.Events;
using PX.SM;
using PX.Objects.CN.Common.Extensions;
using static PX.Objects.FA.FABookSettings.midMonthType;


namespace PX.Objects.AM
{
    [PXProjection(typeof(Select2<AMProdOper, InnerJoin<AMClockTran, On<AMProdOper.prodOrdID, Equal<AMClockTran.prodOrdID>,
                                                              And<AMProdOper.orderType, Equal<AMClockTran.orderType>,
                                                              And<AMProdOper.operationID, Equal<AMClockTran.operationID>>>>>>))]

    [Serializable]
    [PXCacheName("ProjectionPOClockOutDAC")]
    public class ProjectionPOClockOutDAC : PXBqlTable, IBqlTable
    {
        #region Selected
        public abstract class selected : IBqlField
        {
        }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        [AMOrderTypeField(IsKey = true, Enabled = false, BqlField = typeof(AMProdOper.orderType))]
        [PXDBDefault(typeof(AMProdItem.orderType))]
        public virtual string OrderType { get; set; }
        #endregion


        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }
        [ProductionNbr(IsKey = true, Enabled = false, BqlField = typeof(AMProdOper.prodOrdID))]
        [PXDBDefault(typeof(AMProdItem.prodOrdID))]
        public virtual string ProdOrdID { set; get; }
        #endregion

        #region OperationCD
        public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [OperationCDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(AMProdOper.operationCD))]
        public virtual string OperationCD { get; set; }
        #endregion

        #region EmployeeID
        [PXDBInt(BqlField = typeof(AMClockTran.employeeID))]
        [ProductionEmployeeSelector]
        [PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? EmployeeID { get; set; }
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        #endregion 
 
        #region Status
        [PXDefault(typeof(ClockTranStatus.newStatus))]
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMClockTran.status))]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [ClockTranStatus.List]
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion

        #region StartTime 
        public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }
        [PXDBDateAndTime(UseTimeZone = true, DisplayNameDate = "Start Date", DisplayNameTime = "Start Time", BqlField = typeof(AMClockTran.startTime))]
        [PXUIField(DisplayName = "Start Time", Required = true, Enabled = false)]
        public virtual DateTime? StartTime { set; get; }
        #endregion

    }


    public class POClockOutProcessing : PXGraph<POClockOutProcessing>
    {
        public PXFilter<POClockINFilter> Filter;
        public PXCancel<POClockINFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessing<ProjectionPOClockOutDAC, POClockINFilter, Where<ProjectionPOClockOutDAC.status, Equal<ClockTranStatus.clockedIn>>> ProdOperRecords;
      
        #region Constructor 
        public POClockOutProcessing()
        {
            ProdOperRecords.SetProcessCaption("PROCESS");
            ProdOperRecords.SetProcessAllCaption("PROCESS ALL");
            POClockINFilter currentFilter = this.Filter.Current;
            ProdOperRecords.SetProcessDelegate(
                delegate (List<ProjectionPOClockOutDAC> list)
                {
                    ProcessRecords(list);
                });
        }
        #endregion

        public static void ProcessRecords(List<ProjectionPOClockOutDAC> list)
        {
            POClockOutProcessing graph = PXGraph.CreateInstance<POClockOutProcessing>();
            graph.ClockOutProcess(list);
        }
        public virtual void ClockOutProcess(List<ProjectionPOClockOutDAC> list)
        {
            AMClockTran newTran = new AMClockTran();
            MultipleProductionClockEntry ProdClockEntrtyGraph = PXGraph.CreateInstance<MultipleProductionClockEntry>();

            foreach (ProjectionPOClockOutDAC records in list)
            {
                try
                {
                    ProdClockEntrtyGraph.Clear();

                    var currentClockItm = ProdClockEntrtyGraph.Document.Select(records.EmployeeID);
                    if (((AMClockItem)currentClockItm) != null)
                    {
                        ProdClockEntrtyGraph.Document.Current = currentClockItm;
                    }
                    else
                    {
                        ProdClockEntrtyGraph.Document.Current = (AMClockItem)ProdClockEntrtyGraph.Document.Cache.Insert();
                        ProdClockEntrtyGraph.Document.Current.EmployeeID = records.EmployeeID;
                    }

                    AMProdOper objAMProdOper = PXSelectJoin<AMProdOper, InnerJoin<AMClockTran, On<AMProdOper.prodOrdID, Equal<AMClockTran.prodOrdID>,
                                                                   And<AMProdOper.orderType, Equal<AMClockTran.orderType>,
                                                                   And<AMProdOper.operationID, Equal<AMClockTran.operationID>>>>>,
                                                                   Where<AMClockTran.employeeID, Equal<Required<POClockINFilter.employeeID>>,
                                                                   And<AMClockTran.prodOrdID, Equal<Required<POClockINFilter.prodOrderNbr>>,
                                                                   And<AMClockTran.status, Equal<ClockTranStatus.clockedIn>>>>>
                                                                    .Select(this, records.EmployeeID, records.ProdOrdID);
                    if(objAMProdOper!=null)
                    { 

                        newTran = ProdClockEntrtyGraph.Transactions.Select().FirstTableItems.ToList().Where(x => x.OrderType == objAMProdOper.OrderType &&
                                                   x.ProdOrdID == objAMProdOper.ProdOrdID && x.OperationID == objAMProdOper.OperationID).FirstOrDefault();


                        int? LaborTime = AMDateInfo.GetDateMinutes(AMDateInfo.RemoveSeconds(records.StartTime.GetValueOrDefault()),
                             AMDateInfo.RemoveSeconds(Convert.ToDateTime(PX.Common.PXTimeZoneInfo.Now)));

                        newTran.Selected = true;
                        if (LaborTime == 0)
                        {
                            newTran.LaborTime = 1;
                        }

                        AMProdOperExtension prodOrderExt = objAMProdOper.GetExtension<AMProdOperExtension>();

                        newTran.Qty = prodOrderExt.UsrQtyComplete ?? 0m;
                        newTran.QtyScrapped = prodOrderExt.UsrQtyScrapped ?? 0m;
                        ProdClockEntrtyGraph.Transactions.Cache.Update(newTran);
                        ProdClockEntrtyGraph.Save.Press();

                        ProdClockEntrtyGraph.clockEntriesClockOut.Press();
                        //PXProcessing.SetInfo(list.IndexOf(records), "Employee Clocked Out Successfully"); 
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("is less that 1 minute and therefore cannot be recorded.Would you like to set labor time to 1 minute? Otherwise the labor time record will be deleted."))
                    {
                        ProdClockEntrtyGraph.Document.View.Answer = WebDialogResult.Yes;
                     //   PXProcessing.SetInfo(list.IndexOf(records), "Employee Clocked Out Successfully");
                    }
                    else
                    {
                        PXProcessing.SetError(list.IndexOf(records), ex.Message);
                    }
                    
                }
            }

        }


    }
}