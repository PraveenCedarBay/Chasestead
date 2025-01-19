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
    public class POClockINEntry : PXGraph<POClockINEntry>
    {
        #region Views
        public PXSave<POClockINFilter> Save;

        public PXFilter<POClockINFilter> Filter;
        public PXCancel<POClockINFilter> Cancel;
        public PXSelect<AMProdOper, Where<AMProdOper.prodOrdID, Equal<Current<POClockINFilter.prodOrderNbr>>>,
            OrderBy<Asc<AMProdOper.operationCD>>> ProdOperRecords;

        public AMOrderedMatlSelect<AMProdItem, AMProdMatl,
          Where<AMProdMatl.orderType, Equal<Current<AMProdOper.orderType>>, 
              And<AMProdMatl.prodOrdID,Equal<Current<AMProdOper.prodOrdID>>, 
                  And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>,
          OrderBy<Asc<AMProdMatl.sortOrder, Asc<AMProdMatl.lineID>>>> ProdMatlRecords;

        public PXSelect<AMClockTran, Where<AMClockTran.prodOrdID, Equal<Required<AMClockTran.prodOrdID>>,
            And<AMClockTran.orderType, Equal<Required<AMClockTran.orderType>>,
                And<AMClockTran.employeeID, Equal<Required<AMClockTran.employeeID>>,
                And<AMClockTran.operationID, Equal<Required<AMClockTran.operationID>>,
            And<AMClockTran.startTime, GreaterEqual<Required<AMClockTran.startTime>>,
                And<AMClockTran.startTime, LessEqual<Required<AMClockTran.startTime>>>>>>>>> AMClockEntryRecords;

        public IEnumerable prodOperRecords()
        {
            PXSelectBase<AMProdOper> GetEntiriesData = new PXSelect<AMProdOper, Where<AMProdOper.prodOrdID, Equal<Current<POClockINFilter.prodOrderNbr>>>,
            OrderBy<Asc<AMProdOper.operationCD>>>(this);
            foreach (AMProdOper rec in GetEntiriesData.Select())
            {
                AMClockTran ObjAMClockTran = AMClockEntryRecords.Select(rec.ProdOrdID, rec.OrderType, Filter.Current?.EmployeeID,
                                             rec.OperationID, Filter.Current?.FromDate, Filter.Current?.TODate.Value.AddDays(1).AddTicks(-1))
                                              .FirstTableItems.ToList().OrderByDescending(c => c.StartTime).FirstOrDefault();
                //rec.GetExtension<AMProdOperExtension>().UsrStartTime = ObjAMClockTran != null ? ObjAMClockTran.StartTime : null;
                ProdOperRecords.Cache.SetValueExt<AMProdOperExtension.usrStartTime>(rec, ObjAMClockTran?.StartTime);
                ProdOperRecords.Cache.SetValueExt<AMProdOperExtension.usrStatus>(rec, ObjAMClockTran?.Status);
                yield return rec;
            }
        }

        //[PXMergeAttributes(Method = MergeMethod.Merge)]
        //[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))] 
        //protected virtual void POClockINFilter_Status_CacheAttached(PXCache cache) { }



        public PXAction<POClockINFilter> ClockIN;
        [PXUIField(DisplayName = "Clock ON", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable clockIN(PXAdapter adapter)
        {
            try
            {
                List<AMProdOper> AMProdOperList = ProdOperRecords.Select().FirstTableItems.ToList().Where(x => x.Selected == true).ToList();
                ProdOperRecords.View.RequestRefresh();

                if (AMProdOperList.ToList().Count > 0)
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

                    AMClockTran objClockTran = PXSelect<AMClockTran,
                                                Where<AMClockTran.employeeID, Equal<Required<AMClockTran.employeeID>>,
                                                // And<AMClockTran.prodOrdID,Equal<Required<AMClockTran.prodOrdID>>,
                                                And<AMClockTran.status, Equal<ClockTranStatus.clockedIn>>>>
                                                .Select(this, Filter.Current?.EmployeeID);//, Filter.Current?.ProdOrderNbr);
                    if (objClockTran != null)
                    {

                        ProdOperRecords.Cache.Clear();
                        throw new PXException("Employee already clocked-in for another Operation. Please Review");
                    }
                    else
                    {
                        PartialGraph partialGraph = PXGraph.CreateInstance<PartialGraph>();
                        AMClockTran newTran = new AMClockTran();
                        foreach (AMProdOper rec in AMProdOperList)
                        {
                            newTran = (AMClockTran)ProdClockEntrtyGraph.Transactions.Cache.Insert();
                            newTran.EmployeeID = Filter.Current?.EmployeeID;
                            ProdClockEntrtyGraph.Transactions.Cache.Update(newTran);
                            newTran.OrderType = rec.OrderType;
                            newTran.ProdOrdID = rec.ProdOrdID;
                            ProdClockEntrtyGraph.Transactions.Cache.Update(newTran);
                            newTran.OperationID = rec.OperationID;
                            newTran.WcID = rec.WcID;
                            //newTran.Qty = 0m;
                           // newTran.QtyScrapped = 0m;
                            ProdClockEntrtyGraph.Transactions.Cache.Update(newTran);

                            newTran.Selected = true;
                            ProdClockEntrtyGraph.Transactions.Cache.Update(newTran);


                          //  rec.QtyComplete = 0m;
                           // rec.QtyScrapped = 0m;
                            //partialGraph.ProdOperRecords.Cache.Update(rec);
                            //partialGraph.Actions.PressSave();

                        }
                        ProdClockEntrtyGraph.Save.Press();
                        ProdClockEntrtyGraph.clockEntriesClockIn.Press();
                       // this.Save.Press();
                         ProdOperRecords.Cache.Clear();

                        //foreach (AMProdOper rec in AMProdOperList)
                        //{
                        //    rec.Selected = false;
                        //    ProdClockEntrtyGraph.Transactions.Cache.Update(newTran);

                        //}
                        //ProdClockEntrtyGraph.Save.Press();

                        ProdOperRecords.Ask(ProdOperRecords.Current, "", "Employee Clocked In Successfully", MessageButtons.OK, MessageIcon.Information);
                    }
                }

                ProdOperRecords.View.RequestRefresh();
            }
            catch (Exception ex)
            {
                throw new PXException(ex.Message);
            }
            return adapter.Get();
        }


        public PXAction<POClockINFilter> GoToClockOut;
        [PXUIField(DisplayName = "Go To Clock Off", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable goToClockOut(PXAdapter adapter)
        {
            //POClockOutEntry ClockOutGraph = PXGraph.CreateInstance<POClockOutEntry>();
            //var EmployeeID = ClockOutGraph.Filter.Current.EmployeeID;
            //ClockOutGraph.Filter.Cache.Clear();
            //ClockOutGraph.Filter.Current.EmployeeID = EmployeeID;
            Filter.Cache.Clear();
            ProdOperRecords.Cache.Clear();
            //Cancel.Press();
            //PXRedirectHelper.TryRedirect(ClockOutGraph, PXRedirectHelper.WindowMode.Same);
            throw new PXRedirectByScreenIDException("AN102000", PXBaseRedirectException.WindowMode.Base, true);
            //Filter.View.RequestRefresh();
            // return adapter.Get();
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
        protected virtual void POClockINFilter_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            POClockINFilter row = e.Row as POClockINFilter;
            if (row != null)
            {
                row.ProdOrderNbr = null;
            }
        }


        protected virtual void AMProdOper_UsrMaterialsCount_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {
            AMProdOper row = e.Row as AMProdOper;
            if (row != null)
            {
                //AMProdOperExtension rowExt = row.GetExtension<AMProdOperExtension>();
                e.ReturnValue = PXSelect<AMProdMatl,
                                 Where<AMProdMatl.orderType, Equal<Required<AMProdOper.orderType>>,
                                 And<AMProdMatl.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                                 And<AMProdMatl.operationID, Equal<Required<AMProdOper.operationID>>>>>,
                                 OrderBy<Asc<AMProdMatl.sortOrder, Asc<AMProdMatl.lineID>>>>.Select(this, row.OrderType, row.ProdOrdID, row.OperationID)
                                 .FirstTableItems.ToList().Count;

            }
        }
    }

    public class PartialGraph : PXGraph<PartialGraph>
    {
        public PXSelect<AMProdOper> ProdOperRecords;
    }
}
