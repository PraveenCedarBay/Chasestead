using PX.Data;
using PX.Objects.AM;
using PX.Objects.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.TX.CSTaxType;
using static PX.SM.EMailAccount;

namespace ClockOnOffCustomization.ClockInOut.Graph
{
    public class MatlWizard2Ext : PXGraphExtension<MatlWizard2>
    {

        // protected virtual void _(Events.FieldVerifying<AMWrkMatl, AMWrkMatl.matlQty> e)

        protected virtual void AMWrkMatl_matlQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying BaseMethod)
        {
            AMWrkMatl row = e.Row as AMWrkMatl;

            //BaseMethod?.Invoke(sender, e);
            if (IsMaterialQtyAllowed(row, (decimal?)e.NewValue, out var ex))
            {
                sender.ClearFieldErrors<AMWrkMatl.matlQty>(e.Row);
                return;
            }

            if (ex.ErrorLevel == PXErrorLevel.Error)
            {
                e.Cancel = true;
                e.NewValue = row?.MatlQty;
            }

            sender.RaiseExceptionHandling<AMWrkMatl.matlQty>(row, row?.MatlQty, ex);
        }
        protected virtual bool IsMaterialQtyAllowed(AMWrkMatl row, decimal? qty, out PXSetPropertyException exceptionMsg)
        {
            exceptionMsg = null;
            if (row == null)
            {
                return true;
            }

            // UnreleasedBatchQty only set if settings require this field to lookup such qty
            decimal? maxQtyToIssue = (row.QtyRemaining.GetValueOrDefault() - row.UnreleasedBatchQty.GetValueOrDefault()).NotLessZero();

            AMOrderType aMOrderType = PXSelectBase<AMOrderType, PXSelect<AMOrderType, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>.Config>.Select(Base, row.OrderType);

            AMOrderTypeExt orderTypeExt = aMOrderType?.GetExtension<AMOrderTypeExt>();
            var overIssuPerc = orderTypeExt.UsrOverIssueMaterialPerc;

            maxQtyToIssue = (maxQtyToIssue * overIssuPerc) / 100m;

            if (row.OverIssueMaterial == "X")
            {
                row.OverIssueMaterial = "A";
            }

            if (qty.GetValueOrDefault() > maxQtyToIssue)
            {
                exceptionMsg = new PXSetPropertyException(PX.Objects.AM.Messages.GetLocal(PX.Objects.AM.Messages.MaterialQuantityOverIssueShortMsg,
              row.UOM, qty.GetValueOrDefault(), maxQtyToIssue), PXErrorLevel.Error);
                return false;
            }


            if (row == null || row.OverIssueMaterial == PX.Objects.AM.Attributes.SetupMessage.AllowMsg || qty.GetValueOrDefault() <= maxQtyToIssue)
            {
                return true;
            }

            exceptionMsg = new PXSetPropertyException(PX.Objects.AM.Messages.GetLocal(PX.Objects.AM.Messages.MaterialQuantityOverIssueShortMsg,
                row.UOM, qty.GetValueOrDefault(), maxQtyToIssue), row.OverIssueMaterial == PX.Objects.AM.Attributes.SetupMessage.WarningMsg ? PXErrorLevel.Warning : PXErrorLevel.Error);

            return false;
        }
    }
}
