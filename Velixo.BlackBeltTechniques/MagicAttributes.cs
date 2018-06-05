using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.PM;

namespace Velixo.BlackBeltTechniques
{
    public class SOOrderExt : PXCacheExtension<SOOrder>
    {
        //Example 1 - PXUIVerify: Request date should be 2+ days from order
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIVerify(typeof(Where<DateDiff<SOOrder.orderDate, SOOrder.requestDate, DateDiff.day>, GreaterEqual<int2>>), PXErrorLevel.Warning, "Requested date is too close to order date!")]
        public DateTime? RequestDate { get; set; }

        //Example 2 - PXUIRequired: Make CustomerOrderNbr required for everyone except "key" customers
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault]
        [PXUIRequired(typeof(Where<Selector<SOOrder.customerID, Customer.customerClassID>, NotEqual<keyCustomersClass>>))]
        public string CustomerOrderNbr { get; set; }

        //Example 3 - PXUIEnabled+PXDefault+PXFormula trigger: Order description defaulted from Project and only editable for "X" (non-project code)
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIEnabled(typeof(Where<Selector<SOOrder.projectID, PMProject.nonProject>, Equal<True>>))]
        [PXDefault(typeof(Search<PMTask.description,
            Where<PMTask.projectID, Equal<Current<SOOrder.projectID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<SOOrder.projectID>))] //To force update of default value when projectID changes - replaces SetDefaultEx call
        public string OrderDesc { get; set; }
    }

    public class SOLineExt : PXCacheExtension<SOLine>
    {
        //Example 4 - PXFormula+Selector: Populate field on SOLine with MSRP value of currently selected item
        [PXDBPriceCost]
        [PXUIField(DisplayName = "MSRP", Enabled = false)]
        [PXFormula(typeof(Selector<SOLine.inventoryID, InventoryItem.recPrice>))]
        public decimal? UsrRecPrice { get; set; }
    }

}
