using PX.Data;
using PX.Objects.AM;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.AM.AMProdMatl;
using static PX.Objects.AM.ProductionTransactionHelper;
using static PX.Objects.PO.POOrderEntry;
using static PX.Objects.SO.SalesAllocationStatus;
using static PX.Objects.TX.CSTaxCalcType;

namespace ClockOnOffCustomization
{
    public class MaterialEntryExt : PXGraphExtension<MaterialEntry>
    {

        protected virtual void AMMTranSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying BaseMethod)
        {
            var row = (AMMTranSplit)e.Row;
            var parent = Base.transactions.Current;
            var isByProductReceipt = parent != null && parent.DocType == AMDocType.Material && parent.IsByproduct.GetValueOrDefault() && parent.TranType == INTranType.Receipt;

            if (row?.InventoryID == null || row.TranType == INTranType.Return || isByProductReceipt)
            {
                return;
            }

            if (!CheckOverIssueMaterialOnEntry(sender, row, Convert.ToDecimal(e.NewValue)))
            {
                return;
            }
            e.NewValue = row.Qty.GetValueOrDefault();
            e.Cancel = true;
        }


        protected virtual void AMMTran_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying BaseMethod)
        {
            //BaseMethod?.Invoke(sender, e);
            var row = (AMMTran)e.Row;
            if (row?.ProdOrdID == null || row.OperationID == null || row.InventoryID == null || row.Qty < 0)
            {
                return;
            }
            var totalQty = Convert.ToDecimal(e.NewValue) + GetOverIssuedQtyForSameMaterial(sender, row, false);
            if (!CheckOverIssueMaterialOnEntry(sender, row, totalQty))
            {
                return;
            }
            e.NewValue = row.Qty.GetValueOrDefault();
            e.Cancel = true;
        }

        private decimal GetOverIssuedQtyForSameMaterial(PXCache sender, AMMTran ammTran, bool useBaseQty)
        {
            var returnQty = 0m;

            var rows = sender.Cached.RowCast<AMMTran>().Where(r => r.OrderType == ammTran.OrderType
                                                    && r.ProdOrdID == ammTran.ProdOrdID
                                                    && r.OperationID == ammTran.OperationID
                                                    && r.MatlLineId == ammTran.MatlLineId
                                                    && r.LineNbr != ammTran.LineNbr).ToList();
            foreach (var row in rows)
            {
                returnQty += useBaseQty ? row.BaseQty.GetValueOrDefault() : row.Qty.GetValueOrDefault();
            }

            return returnQty;
        }

        protected virtual bool CheckOverIssueMaterialOnEntry(PXCache sender, AMMTran ammTran, decimal? newQty)
        {
            if (ammTran?.ProdOrdID == null || ammTran.OperationID == null || ammTran.InventoryID == null ||
                !TryCheckOverIssueMaterialSetPropertyException(sender, ammTran,
                    newQty, out var setPropertyException))
            {
                return false;
            }

            // if (!IsInternalQtySet)
            {
                sender.RaiseExceptionHandling<AMMTran.qty>(
                    ammTran,
                    ammTran.Qty,
                    setPropertyException);
            }

            return setPropertyException?.ErrorLevel == PXErrorLevel.Error || setPropertyException?.ErrorLevel == PXErrorLevel.RowError;
        }

        private decimal GetOverIssuedQtyForSameMaterial(PXCache sender, AMMTranSplit ammTranSplit)
        {
            var returnQty = 0m;

            var cachedSplitRows = sender.Cached.RowCast<AMMTranSplit>().Where(r => r.DocType == ammTranSplit.DocType
                                                            && r.BatNbr == ammTranSplit.BatNbr
                                            && r.LineNbr == ammTranSplit.LineNbr
                                            && r.SplitLineNbr != ammTranSplit.SplitLineNbr).ToList();
            foreach (var row in cachedSplitRows)
            {
                returnQty += row.BaseQty.GetValueOrDefault();
            }

            return returnQty;
        }

        public static bool TryCheckOverIssueMaterialSetPropertyException(PXCache sender, AMMTran ammTran, decimal? newQty, out PXSetPropertyException exception)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            exception = null;
            if (ammTran == null || ammTran.DocType == null || ammTran.OrderType == null || ammTran.ProdOrdID == null || newQty.GetValueOrDefault() == 0m || ammTran.DocType != "M")
            {
                return false;
            }

            AMOrderType aMOrderType = PXSelectBase<AMOrderType, PXSelect<AMOrderType, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>.Config>.Select(sender.Graph, ammTran.OrderType);
            if (CanSkipOrderTypeCheck<AMOrderType.overIssueMaterial>(sender.Graph, aMOrderType))
            {
                AMProdMatl aMProdMatl1 = PXSelectBase<AMProdMatl, PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>, And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>, And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>, And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>>>>>>.Config>.Select(sender.Graph, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.MatlLineId);
                if (aMProdMatl1 == null)
                {
                    return false;
                }

                decimal valueOrDefault1 = aMProdMatl1.QtyRemaining.GetValueOrDefault();
                if (ammTran.UOM != null && !aMProdMatl1.UOM.EqualsWithTrim(ammTran.UOM) && UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(sender.Graph.Caches<AMProdMatl>(), aMProdMatl1, aMProdMatl1.UOM, ammTran.UOM, valueOrDefault1, out var result1))
                {
                    valueOrDefault1 = result1.GetValueOrDefault();
                }

                decimal num1 = (aMOrderType.IncludeUnreleasedOverIssueMaterial.GetValueOrDefault() ? GetUnreleasedMaterialQty(sender, aMProdMatl1, ammTran.UOM, ammTran) : 0m);
                valueOrDefault1 -= num1;



                AMOrderTypeExt orderTypeExt = aMOrderType.GetExtension<AMOrderTypeExt>();
                var overIssuPerc = orderTypeExt.UsrOverIssueMaterialPerc;
                
                
                //var val = (valueOrDefault1 * overIssuPerc) / 100m; 
                var val = aMProdMatl1.TotalQtyRequired * (overIssuPerc / 100m) - aMProdMatl1.QtyActual - valueOrDefault1;

                //(BaseTotalQtyRequired * (OverIssuepct / 100) - BaseQtyActual - num1(in the code) 

                if (newQty.GetValueOrDefault() > val)
                {
                    object stateExt1 = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.inventoryID>(ammTran);
                    object stateExt22 = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.operationID>(ammTran);
                    List<object> list1 = new List<object>
                   {
                       ammTran.UOM,
                       UomHelper.FormatQty(newQty.GetValueOrDefault()),
                       UomHelper.FormatQty(valueOrDefault1.NotLessZero()),
                       ammTran.OrderType + " " + ammTran.ProdOrdID.TrimIfNotNullEmpty(),
                       stateExt22 ?? ((object)ammTran.OperationID),
                       (stateExt1 == null) ? string.Empty : Convert.ToString(stateExt1).TrimIfNotNullEmpty(),
                       aMProdMatl1.LineNbr
                   };

                    bool num21 = num1 != 0m;
                    if (num21)
                    {
                        list1.Add(UomHelper.FormatQty(num1));
                    }


                    string local1 = PX.Objects.AM.Messages.GetLocal(num21 ? "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued (includes the unreleased batch quantity of {7} {0}) in the {6} line for the {5} item of the {4} operation in the {3} order." : "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued in the {6} line for the {5} item of the {4} operation in the {3} order.", list1.ToArray());

                    exception = new PXSetPropertyException(local1, PXErrorLevel.Error);
                    return true;

                    //return exception != null;
                }

                return false;
            }

            AMProdMatl aMProdMatl = PXSelectBase<AMProdMatl, PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>, And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>, And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>, And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>>>>>>.Config>.Select(sender.Graph, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.MatlLineId);
            if (aMProdMatl == null)
            {
                return false;
            }

            decimal valueOrDefault = aMProdMatl.QtyRemaining.GetValueOrDefault();
            if (ammTran.UOM != null && !aMProdMatl.UOM.EqualsWithTrim(ammTran.UOM) && UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(sender.Graph.Caches<AMProdMatl>(), aMProdMatl, aMProdMatl.UOM, ammTran.UOM, valueOrDefault, out var result))
            {
                valueOrDefault = result.GetValueOrDefault();
            }

            decimal num = (aMOrderType.IncludeUnreleasedOverIssueMaterial.GetValueOrDefault() ? GetUnreleasedMaterialQty(sender, aMProdMatl, ammTran.UOM, ammTran) : 0m);
            valueOrDefault -= num;
            if (newQty.GetValueOrDefault() <= valueOrDefault)
            {
                return exception != null;
            }

            object stateExt = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.inventoryID>(ammTran);
            object stateExt2 = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.operationID>(ammTran);
            List<object> list = new List<object>
        {
            ammTran.UOM,
            UomHelper.FormatQty(newQty.GetValueOrDefault()),
            UomHelper.FormatQty(valueOrDefault.NotLessZero()),
            ammTran.OrderType + " " + ammTran.ProdOrdID.TrimIfNotNullEmpty(),
            stateExt2 ?? ((object)ammTran.OperationID),
            (stateExt == null) ? string.Empty : Convert.ToString(stateExt).TrimIfNotNullEmpty(),
            aMProdMatl.LineNbr
        };
            bool num2 = num != 0m;
            if (num2)
            {
                list.Add(UomHelper.FormatQty(num));
            }

            string local = PX.Objects.AM.Messages.GetLocal(num2 ? "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued (includes the unreleased batch quantity of {7} {0}) in the {6} line for the {5} item of the {4} operation in the {3} order." : "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued in the {6} line for the {5} item of the {4} operation in the {3} order.", list.ToArray());
            exception = new PXSetPropertyException(local, (aMOrderType.OverIssueMaterial == "W") ? PXErrorLevel.Warning : PXErrorLevel.Error);
            return true;
        }

        protected static decimal GetUnreleasedMaterialQty(PXCache cache, AMProdMatl prodMatl, string tranUom, AMMTran excludingTran)
        {
            decimal unreleasedMaterialBaseQty = GetUnreleasedMaterialBaseQty(cache.Graph, prodMatl, excludingTran);
            if (unreleasedMaterialBaseQty != 0m && UomHelper.TryConvertFromBaseQty<AMMTran.inventoryID>(cache, excludingTran, tranUom, unreleasedMaterialBaseQty, out var result))
            {
                return result.GetValueOrDefault();
            }

            return unreleasedMaterialBaseQty;
        }

        public static bool CanSkipOrderTypeCheck<TField>(PXGraph graph, AMOrderType orderType) where TField : class, IBqlField
        {
            if (orderType == null || graph == null)
            {
                return false;
            }

            string text = (string)graph.Caches<AMOrderType>()?.GetValue<TField>(orderType);
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            if (text == "X")
            {
                text = "A";
            }

            if (!(text == "A"))
            {
                if (text == "W")
                {
                    if (!graph.IsImport)
                    {
                        return graph.IsContractBasedAPI;
                    }

                    return true;
                }

                return false;
            }

            return true;
        }

        protected static decimal GetUnreleasedMaterialBaseQty(PXGraph graph, AMProdMatl prodMatl, AMMTran excludingTran)
        {
            Common.Cache.AddCache<AMMTran>(graph);
            PXSelectBase<UnreleasedMaterialTran> pXSelectBase = new PXSelectGroupBy<UnreleasedMaterialTran, Where<UnreleasedMaterialTran.orderType, Equal<Required<UnreleasedMaterialTran.orderType>>, And<UnreleasedMaterialTran.prodOrdID, Equal<Required<UnreleasedMaterialTran.prodOrdID>>, And<UnreleasedMaterialTran.operationID, Equal<Required<UnreleasedMaterialTran.operationID>>, And<UnreleasedMaterialTran.matlLineId, Equal<Required<UnreleasedMaterialTran.matlLineId>>>>>>, Aggregate<Sum<UnreleasedMaterialTran.baseQty, Sum<UnreleasedMaterialTran.qty, Sum<UnreleasedMaterialTran.tranAmt>>>>>(graph);
            List<object> list = new List<object>
        {
            prodMatl?.OrderType,
            prodMatl?.ProdOrdID,
            prodMatl?.OperationID,
            prodMatl?.LineID
        };
            if (excludingTran != null && excludingTran.LineNbr.HasValue && excludingTran.DocType == "M")
            {
                pXSelectBase.WhereAnd<Where<UnreleasedMaterialTran.batNbr, NotEqual<Required<UnreleasedMaterialTran.batNbr>>>>();
                list.Add(excludingTran.BatNbr);
            }

            return (((UnreleasedMaterialTran)pXSelectBase.Select(list.ToArray()))?.BaseQty).GetValueOrDefault();
        } 

    } 



    [PXProtectedAccess]
    public abstract class AMReleaseProcess_ProtectedExtension : PXGraphExtension<AMReleaseProcessExt, AMReleaseProcess>
    {
        [PXProtectedAccess(typeof(AMReleaseProcess))]
        // protected abstract void LinkPOLineToSOLineSplit(POOrderEntry docgraph, SOLineSplit3 soline, POLine line);

        protected abstract void CheckOverIssuedMaterial(AMMTran ammTran, PXResultset<AMMTran> allTrans);
    }


    [PXProtectedAccess(typeof(AMReleaseProcess))]

    public class AMReleaseProcessExt : PXGraphExtension<AMReleaseProcess>
    {
        #region Event Handlers
        
        #endregion
        public delegate void CheckOverIssuedMaterialDelegate(AMMTran ammTran, PXResultset<AMMTran> allTrans);
        [PXOverride]
        public virtual void CheckOverIssuedMaterial(AMMTran ammTran, PXResultset<AMMTran> allTrans, CheckOverIssuedMaterialDelegate BaseMethod)
        {
            var totalQty = ammTran.Qty;
            var rows = allTrans.RowCast<AMMTran>().Where(r => r.OrderType == ammTran.OrderType
                                                    && r.ProdOrdID == ammTran.ProdOrdID
                                                    && r.OperationID == ammTran.OperationID
                                                    && r.MatlLineId == ammTran.MatlLineId
                                                    && r.LineNbr != ammTran.LineNbr).ToList();
            foreach (var row in rows)
            {
                totalQty += row.Qty.GetValueOrDefault();
            }
            if (TryCheckOverIssueMaterialSetPropertyException(Base.TranRecs.Cache, ammTran, totalQty, out var exception)
                && exception.ErrorLevel == PXErrorLevel.Error)
            {
                throw exception;
            }
        }

        public static bool TryCheckOverIssueMaterialSetPropertyException(PXCache sender, AMMTran ammTran, decimal? newQty, out PXSetPropertyException exception)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            exception = null;
            if (ammTran == null || ammTran.DocType == null || ammTran.OrderType == null || ammTran.ProdOrdID == null || newQty.GetValueOrDefault() == 0m || ammTran.DocType != "M")
            {
                return false;
            }

            AMOrderType aMOrderType = PXSelectBase<AMOrderType, PXSelect<AMOrderType, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>.Config>.Select(sender.Graph, ammTran.OrderType);
            if (CanSkipOrderTypeCheck<AMOrderType.overIssueMaterial>(sender.Graph, aMOrderType))
            {
                AMProdMatl aMProdMatl1 = PXSelectBase<AMProdMatl, PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>, And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>, And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>, And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>>>>>>.Config>.Select(sender.Graph, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.MatlLineId);
                if (aMProdMatl1 == null)
                {
                    return false;
                }

                decimal valueOrDefault1 = aMProdMatl1.QtyRemaining.GetValueOrDefault();
                if (ammTran.UOM != null && !aMProdMatl1.UOM.EqualsWithTrim(ammTran.UOM) && UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(sender.Graph.Caches<AMProdMatl>(), aMProdMatl1, aMProdMatl1.UOM, ammTran.UOM, valueOrDefault1, out var result1))
                {
                    valueOrDefault1 = result1.GetValueOrDefault();
                }

                decimal num1 = (aMOrderType.IncludeUnreleasedOverIssueMaterial.GetValueOrDefault() ? GetUnreleasedMaterialQty(sender, aMProdMatl1, ammTran.UOM, ammTran) : 0m);
                valueOrDefault1 -= num1;

                AMOrderTypeExt orderTypeExt = aMOrderType.GetExtension<AMOrderTypeExt>();
                var overIssuPerc = orderTypeExt.UsrOverIssueMaterialPerc;


                //var val = (valueOrDefault1 * overIssuPerc) / 100m;

                var val = aMProdMatl1.TotalQtyRequired * (overIssuPerc / 100m) - aMProdMatl1.QtyActual - valueOrDefault1;


                if (newQty.GetValueOrDefault() > val)
                {
                    object stateExt1 = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.inventoryID>(ammTran);
                    object stateExt22 = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.operationID>(ammTran);
                    List<object> list1 = new List<object>
                   {
                       ammTran.UOM,
                       UomHelper.FormatQty(newQty.GetValueOrDefault()),
                       UomHelper.FormatQty(valueOrDefault1.NotLessZero()),
                       ammTran.OrderType + " " + ammTran.ProdOrdID.TrimIfNotNullEmpty(),
                       stateExt22 ?? ((object)ammTran.OperationID),
                       (stateExt1 == null) ? string.Empty : Convert.ToString(stateExt1).TrimIfNotNullEmpty(),
                       aMProdMatl1.LineNbr
                   };

                    bool num21 = num1 != 0m;
                    if (num21)
                    {
                        list1.Add(UomHelper.FormatQty(num1));
                    }


                    string local1 = PX.Objects.AM.Messages.GetLocal(num21 ? "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued (includes the unreleased batch quantity of {7} {0}) in the {6} line for the {5} item of the {4} operation in the {3} order." : "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued in the {6} line for the {5} item of the {4} operation in the {3} order.", list1.ToArray());

                    exception = new PXSetPropertyException(local1, PXErrorLevel.Error);
                    return true;

                    //return exception != null;
                }

                return false;
            }

            AMProdMatl aMProdMatl = PXSelectBase<AMProdMatl, PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>, And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>, And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>, And<AMProdMatl.lineID, Equal<Required<AMProdMatl.lineID>>>>>>>.Config>.Select(sender.Graph, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.MatlLineId);
            if (aMProdMatl == null)
            {
                return false;
            }

            decimal valueOrDefault = aMProdMatl.QtyRemaining.GetValueOrDefault();
            if (ammTran.UOM != null && !aMProdMatl.UOM.EqualsWithTrim(ammTran.UOM) && UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(sender.Graph.Caches<AMProdMatl>(), aMProdMatl, aMProdMatl.UOM, ammTran.UOM, valueOrDefault, out var result))
            {
                valueOrDefault = result.GetValueOrDefault();
            }

            decimal num = (aMOrderType.IncludeUnreleasedOverIssueMaterial.GetValueOrDefault() ? GetUnreleasedMaterialQty(sender, aMProdMatl, ammTran.UOM, ammTran) : 0m);
            valueOrDefault -= num;
            if (newQty.GetValueOrDefault() <= valueOrDefault)
            {
                return exception != null;
            }

            object stateExt = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.inventoryID>(ammTran);
            object stateExt2 = sender.Graph.Caches<AMMTran>().GetStateExt<AMMTran.operationID>(ammTran);
            List<object> list = new List<object>
        {
            ammTran.UOM,
            UomHelper.FormatQty(newQty.GetValueOrDefault()),
            UomHelper.FormatQty(valueOrDefault.NotLessZero()),
            ammTran.OrderType + " " + ammTran.ProdOrdID.TrimIfNotNullEmpty(),
            stateExt2 ?? ((object)ammTran.OperationID),
            (stateExt == null) ? string.Empty : Convert.ToString(stateExt).TrimIfNotNullEmpty(),
            aMProdMatl.LineNbr
        };
            bool num2 = num != 0m;
            if (num2)
            {
                list.Add(UomHelper.FormatQty(num));
            }

            string local = PX.Objects.AM.Messages.GetLocal(num2 ? "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued (includes the unreleased batch quantity of {7} {0}) in the {6} line for the {5} item of the {4} operation in the {3} order." : "The material quantity of {1} {0} to be issued is greater than the remaining quantity of {2} {0} to be issued in the {6} line for the {5} item of the {4} operation in the {3} order.", list.ToArray());
            exception = new PXSetPropertyException(local, (aMOrderType.OverIssueMaterial == "W") ? PXErrorLevel.Warning : PXErrorLevel.Error);
            return true;
        }
        protected static decimal GetUnreleasedMaterialQty(PXCache cache, AMProdMatl prodMatl, string tranUom, AMMTran excludingTran)
        {
            decimal unreleasedMaterialBaseQty = GetUnreleasedMaterialBaseQty(cache.Graph, prodMatl, excludingTran);
            if (unreleasedMaterialBaseQty != 0m && UomHelper.TryConvertFromBaseQty<AMMTran.inventoryID>(cache, excludingTran, tranUom, unreleasedMaterialBaseQty, out var result))
            {
                return result.GetValueOrDefault();
            }

            return unreleasedMaterialBaseQty;
        }

        public static bool CanSkipOrderTypeCheck<TField>(PXGraph graph, AMOrderType orderType) where TField : class, IBqlField
        {
            if (orderType == null || graph == null)
            {
                return false;
            }

            string text = (string)graph.Caches<AMOrderType>()?.GetValue<TField>(orderType);
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            if (text == "X")
            {
                text = "A";
            }

            if (!(text == "A"))
            {
                if (text == "W")
                {
                    if (!graph.IsImport)
                    {
                        return graph.IsContractBasedAPI;
                    }

                    return true;
                }

                return false;
            }

            return true;
        }

        protected static decimal GetUnreleasedMaterialBaseQty(PXGraph graph, AMProdMatl prodMatl, AMMTran excludingTran)
        {
            Common.Cache.AddCache<AMMTran>(graph);
            PXSelectBase<UnreleasedMaterialTran> pXSelectBase = new PXSelectGroupBy<UnreleasedMaterialTran, Where<UnreleasedMaterialTran.orderType, Equal<Required<UnreleasedMaterialTran.orderType>>, And<UnreleasedMaterialTran.prodOrdID, Equal<Required<UnreleasedMaterialTran.prodOrdID>>, And<UnreleasedMaterialTran.operationID, Equal<Required<UnreleasedMaterialTran.operationID>>, And<UnreleasedMaterialTran.matlLineId, Equal<Required<UnreleasedMaterialTran.matlLineId>>>>>>, Aggregate<Sum<UnreleasedMaterialTran.baseQty, Sum<UnreleasedMaterialTran.qty, Sum<UnreleasedMaterialTran.tranAmt>>>>>(graph);
            List<object> list = new List<object>
        {
            prodMatl?.OrderType,
            prodMatl?.ProdOrdID,
            prodMatl?.OperationID,
            prodMatl?.LineID
        };
            if (excludingTran != null && excludingTran.LineNbr.HasValue && excludingTran.DocType == "M")
            {
                pXSelectBase.WhereAnd<Where<UnreleasedMaterialTran.batNbr, NotEqual<Required<UnreleasedMaterialTran.batNbr>>>>();
                list.Add(excludingTran.BatNbr);
            }

            return (((UnreleasedMaterialTran)pXSelectBase.Select(list.ToArray()))?.BaseQty).GetValueOrDefault();
        }

    }
}
