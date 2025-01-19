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
using ClockOnOffCustomization;
using static PX.Data.BQL.BqlPlaceholder;

namespace PX.Objects.AM
{
    public class POClockOutEntry : PXGraph<POClockINEntry>
    {
        #region Views

        public PXFilter<POClockINFilter> Filter;
        public PXCancel<POClockINFilter> Cancel;



        public PXSelectJoin<AMProdOper, InnerJoin<AMClockTran, On<AMProdOper.prodOrdID, Equal<AMClockTran.prodOrdID>,
                                                          And<AMProdOper.orderType, Equal<AMClockTran.orderType>,
                                                          And<AMProdOper.operationID, Equal<AMClockTran.operationID>>>>>,
                                                         Where<AMClockTran.employeeID, Equal<Required<POClockINFilter.employeeID>>,
                                                         And<AMClockTran.prodOrdID, Equal<Required<POClockINFilter.prodOrderNbr>>,
                                                         And<AMClockTran.startTime, GreaterEqual<Required<AMProdOper.startDate>>,
                                                         And<AMClockTran.startTime, LessEqual<Required<AMProdOper.endDate>>,
                                                            And<AMClockTran.status, Equal<Required<POClockINFilter.status>>>

                                                             // And<Where<AMClockTran.status, Equal<ClockTranStatus.clockedIn>,
                                                             //Or<AMClockTran.status, Equal<ClockTranStatus.clockedOut>>>>

                                                             >>>>> ProdOperRecords;
        public IEnumerable prodOperRecords()
        {
            if (Filter.Current != null)
            {
                PXSelectBase<AMProdOper> GetEntiriesData = new PXSelectJoin<AMProdOper, InnerJoin<AMClockTran, On<AMProdOper.prodOrdID, Equal<AMClockTran.prodOrdID>,
                                                              And<AMProdOper.orderType, Equal<AMClockTran.orderType>,
                                                              And<AMProdOper.operationID, Equal<AMClockTran.operationID>>>>>,
                                                             Where<AMClockTran.employeeID, Equal<Required<POClockINFilter.employeeID>>,
                                                             //  And<AMClockTran.prodOrdID, Equal<Required<POClockINFilter.prodOrderNbr>>,
                                                             And<AMClockTran.startTime, GreaterEqual<Required<AMProdOper.startDate>>,
                                                             And<AMClockTran.startTime, LessEqual<Required<AMProdOper.endDate>>,

                                                             And<AMClockTran.status, Equal<Required<POClockINFilter.status>>>
                >>>>(this);
                if (Filter.Current.ProdOrderNbr != null)
                {
                    GetEntiriesData.WhereAnd<Where<AMClockTran.prodOrdID, Equal<Current<POClockINFilter.prodOrderNbr>>>>();
                }
                return GetEntiriesData.Select(Filter.Current?.EmployeeID, Filter.Current?.FromDate, Filter.Current?.TODate.Value.AddDays(1).AddTicks(-1), Filter.Current?.Status);

            }
            return null;
        }


        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
        [PXUIField(DisplayName = "Qty Complete", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void AMProdOper_QtyComplete_CacheAttached(PXCache cache) { }


        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
        [PXUIField(DisplayName = "Qty Scrapped")]
        protected virtual void AMProdOper_QtyScrapped_CacheAttached(PXCache cache) { }



        public PXAction<POClockINFilter> ClockOut;
        [PXUIField(DisplayName = "Clock Off", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable clockOut(PXAdapter adapter)
        {
            if (ProdOperRecords.Select(Filter.Current?.EmployeeID, Filter.Current?.ProdOrderNbr, Filter.Current?.FromDate, Filter.Current?.TODate.Value.AddDays(1).AddTicks(-1)).FirstTableItems.ToList().Where(x => x.Selected == true).ToList().Count > 0)
            {
                MultipleProductionClockEntry ProdClockEntrtyGraph = PXGraph.CreateInstance<MultipleProductionClockEntry>();

                var currentClockItm = ProdClockEntrtyGraph.Document.Select(Filter.Current?.EmployeeID);
                if (((AMClockItem)currentClockItm) != null)
                {
                    ProdClockEntrtyGraph.Document.Current = currentClockItm;
                }
                else
                {
                    ProdClockEntrtyGraph.Document.Current = (AMClockItem)ProdClockEntrtyGraph.Document.Cache.Insert();
                    ProdClockEntrtyGraph.Document.Current.EmployeeID = Filter.Current?.EmployeeID;
                }

                AMClockTran newTran = new AMClockTran();

                foreach (AMProdOper records in ProdOperRecords.Select(Filter.Current?.EmployeeID, Filter.Current?.ProdOrderNbr, Filter.Current?.FromDate, Filter.Current?.TODate.Value.AddDays(1).AddTicks(-1)).FirstTableItems.ToList().Where(x => x.Selected == true))
                {

                    foreach (PXResult<AMProdOper, AMClockTran> res in PXSelectJoin<AMProdOper, InnerJoin<AMClockTran, On<AMProdOper.prodOrdID, Equal<AMClockTran.prodOrdID>,
                                                                        And<AMProdOper.orderType, Equal<AMClockTran.orderType>,
                                                                        And<AMProdOper.operationID, Equal<AMClockTran.operationID>>>>>,
                                                                        Where<AMClockTran.employeeID, Equal<Required<POClockINFilter.employeeID>>,
                                                                        And<AMClockTran.prodOrdID, Equal<Required<POClockINFilter.prodOrderNbr>>,
                                                                        And<AMClockTran.startTime, GreaterEqual<Required<AMProdOper.startDate>>,
                                                                        And<AMClockTran.startTime, LessEqual<Required<AMProdOper.endDate>>,
                                                                        And<AMClockTran.status, Equal<ClockTranStatus.clockedIn>>>>>>>
                                                                         .Select(this, Filter.Current?.EmployeeID,
                                                                         records?.ProdOrdID, Filter.Current?.FromDate,
                                                               Filter.Current?.TODate.Value.AddDays(1).AddTicks(-1)))
                    {

                        AMProdOper objAMProdOper = (AMProdOper)res;
                        AMClockTran objAMClockTran = (AMClockTran)res;

                        if (objAMProdOper.Selected == true)
                        {
                            AMProdOperExtension prodOrderExt = objAMProdOper.GetExtension<AMProdOperExtension>();

                            //UnderAndOverIssueMaterialTolerenceValidation(objAMProdOper, (objAMProdOper.QtyComplete ?? 0m), (objAMProdOper.QtyScrapped ?? 0m));

                            //Matt and Nathan suggested
                           // UnderAndOverIssueMaterialTolerenceValidation(objAMProdOper, (prodOrderExt.UsrQtyComplete ?? 0m), (prodOrderExt.UsrQtyScrapped ?? 0m));



                            UnderAndOverIssueMaterialTolerenceValidation(objAMProdOper, (prodOrderExt.UsrQtyComplete ?? 0m), ((objAMProdOper.QtyComplete ?? 0m) + (prodOrderExt.UsrQtyComplete ?? 0m)), ((objAMProdOper.QtyScrapped ?? 0m) + (prodOrderExt.UsrQtyScrapped ?? 0m)));




                            newTran = ProdClockEntrtyGraph.Transactions.Select().FirstTableItems.ToList().Where(x => x.OrderType == objAMProdOper.OrderType &&
                            x.ProdOrdID == objAMProdOper.ProdOrdID && x.OperationID == objAMProdOper.OperationID).FirstOrDefault();


                            int? LaborTime = AMDateInfo.GetDateMinutes(AMDateInfo.RemoveSeconds(objAMClockTran.StartTime.GetValueOrDefault()),
                                 AMDateInfo.RemoveSeconds(Convert.ToDateTime(PX.Common.PXTimeZoneInfo.Now)));

                            newTran.Selected = true;
                            if (LaborTime == 0)
                            {
                                newTran.LaborTime = 1;
                            }



                            newTran.Qty = prodOrderExt.UsrQtyComplete ?? 0m;
                            newTran.QtyScrapped = prodOrderExt.UsrQtyScrapped ?? 0m;
                            ProdClockEntrtyGraph.Transactions.Cache.Update(newTran);
                        }
                        ProdClockEntrtyGraph.Save.Press();
                        try
                        {
                            ProdClockEntrtyGraph.clockEntriesClockOut.Press();
                        }
                        catch (Exception ex)
                        {

                            if (ex.Message.Contains("is less that 1 minute and therefore cannot be recorded.Would you like to set labor time to 1 minute? Otherwise the labor time record will be deleted."))
                            {
                                ProdClockEntrtyGraph.Document.View.Answer = WebDialogResult.Yes;
                            }
                            else
                            {
                                throw new PXException(ex.Message);
                            }

                        }

                        ProdOperRecords.View.RequestRefresh();


                        ProdOperRecords.Ask(ProdOperRecords.Current, "", "Employee Clocked Out Successfully", MessageButtons.OK, MessageIcon.Information);
                    }
                }
            }
            return adapter.Get();
        }

        private void UnderAndOverIssueMaterialTolerenceValidation(AMProdOper objAMProdOper, decimal? UsrQtyComplete, decimal? TotalQtyComplete, decimal? TotalQtyScrapped)
        {
            AMOrderType objAMOrderType = PXSelect<AMOrderType, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>.Select(this, objAMProdOper?.OrderType);


            if (objAMOrderType != null)
            {
                if (UsrQtyComplete > 0)
                {
                    foreach (AMProdMatl objAMProdMatl in PXSelect<AMProdMatl,
                                           Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                                           And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                                           And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>>>>>.Select(this,
                                               objAMProdOper?.OrderType, objAMProdOper?.ProdOrdID, objAMProdOper?.OperationID))
                    {

                        if (objAMProdMatl != null && objAMProdMatl?.BFlush != true)
                        {
                            AMOrderTypeExt prodTypeExt = objAMOrderType.GetExtension<AMOrderTypeExt>();


                            //// Under Issue Logic 
                            //if (objAMOrderType.UnderIssueMaterial == "X" && prodTypeExt.UsrUnderIssueMaterialPerc > 0)
                            //{
                            //    decimal? UnderIssueValue = ((QtyComplete * objAMProdMatl.QtyReq) - (QtyComplete - (prodTypeExt.UsrUnderIssueMaterialPerc / 100)));

                            //    if (objAMProdMatl.QtyActual < UnderIssueValue)
                            //    {
                            //        throw new PXException("Insufficient Material Issued. Please Review.");
                            //    }
                            //}

                            ////Over Issue Logic
                            //if (objAMOrderType.OverIssueMaterial == "X" && prodTypeExt.UsrOverIssueMaterialPerc > 0)
                            //{

                            //    decimal? OverIssueValue = ((QtyComplete * objAMProdMatl.QtyReq) - (QtyComplete - (prodTypeExt.UsrOverIssueMaterialPerc / 100)));

                            //    if (objAMProdMatl.QtyActual > OverIssueValue)
                            //    {
                            //        throw new PXException("Excess Material Issued. Please Review.");
                            //    }
                            //}

                            if (objAMOrderType.UnderIssueMaterial == "X" && objAMOrderType.OverIssueMaterial == "X")
                            {
                                //decimal? UnderIssueValue = (((QtyComplete + QtyScrapped) * objAMProdMatl.QtyReq) - ((QtyComplete * objAMProdMatl.QtyReq) * (prodTypeExt.UsrOverIssueMaterialPerc / 100)));
                              //  decimal? UnderIssueValue = (((TotalQtyComplete + TotalQtyScrapped) * objAMProdMatl.QtyReq) * (prodTypeExt.UsrUnderIssueMaterialPerc / 100)); // Provided by Matt
                                //decimal? OverIssueValue = (((QtyComplete + QtyScrapped) * objAMProdMatl.QtyReq) + ((QtyComplete * objAMProdMatl.QtyReq) * (prodTypeExt.UsrOverIssueMaterialPerc / 100)));
                              //  decimal? OverIssueValue = (((TotalQtyComplete + TotalQtyScrapped) * objAMProdMatl.QtyReq) * (prodTypeExt.UsrOverIssueMaterialPerc / 100));// Provided by Matt

                                 
                                 

                                decimal? UnderIssueValue = (((TotalQtyComplete + TotalQtyScrapped) * objAMProdMatl.QtyReq) * (prodTypeExt.UsrUnderIssueMaterialPerc / 100)); // Provided by Matt 
 
                                decimal? OverIssueValue = (((TotalQtyScrapped * objAMProdMatl.QtyReq) * (prodTypeExt.UsrOverIssueMaterialPerc / 100)) + (objAMProdMatl.TotalQtyRequired * (prodTypeExt.UsrOverIssueMaterialPerc / 100)));// Provided by Matt

                                 



                                if (objAMProdMatl.QtyActual > UnderIssueValue && objAMProdMatl.QtyActual < OverIssueValue)
                                {
                                    // No Error
                                }
                                else
                                {
                                    throw new PXException("Excess/Insufficient Material Issued. Please Review.");
                                }
                            }
                        }
                    }
                }
            }
        }

        public PXAction<POClockINFilter> GoToClockIN;
        [PXUIField(DisplayName = "Go To Clock ON", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable goToClockIN(PXAdapter adapter)
        {
            //POClockINEntry ClockInGraph = PXGraph.CreateInstance<POClockINEntry>();
            //var EmployeeID = ClockInGraph.Filter.Current.EmployeeID;

            //ClockInGraph.Filter.Cache.Clear();
            //ClockInGraph.Filter.Current.EmployeeID = EmployeeID;
            //Cancel.Press();
            Filter.Cache.Clear();
            ProdOperRecords.Cache.Clear();
            //PXRedirectHelper.TryRedirect(ClockInGraph, PXRedirectHelper.WindowMode.Same);
            throw new PXRedirectByScreenIDException("AN101000", PXBaseRedirectException.WindowMode.Base, true);
            //Filter.View.RequestRefresh();
            //return adapter.Get();
        }
        #endregion


        protected virtual void POClockINFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            POClockINFilter row = e.Row as POClockINFilter;
            if (row != null)
            {
                PXUIFieldAttribute.SetEnabled<POClockINFilter.employeeID>(cache, row);
                PXUIFieldAttribute.SetEnabled<POClockINFilter.prodOrderNbr>(cache, row);
            }
        }
    }
}
