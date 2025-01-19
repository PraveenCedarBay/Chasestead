using PX.Data;
using PX.Objects.AM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockOnOffCustomization
{
    public class AMOrderTypeMaintExt : PXGraphExtension<AMOrderTypeMaint>
    {
        public static bool IsActive() => true;

        protected virtual void AMOrderType_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            AMOrderType row = e.Row as AMOrderType;
            if (row != null)
            {
                PXUIFieldAttribute.SetVisible<AMOrderTypeExt.usrUnderIssueMaterialPerc>(cache, row, row.UnderIssueMaterial == "X");
                PXUIFieldAttribute.SetVisible<AMOrderTypeExt.usrOverIssueMaterialPerc>(cache, row, row.OverIssueMaterial == "X");
            }
        }
    }
}
