using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.SO;

namespace Velixo.BlackBeltTechniques
{
    public class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
    {
        public override void Initialize()
        {
            TestScopes.AddMenuAction(DisableSelectorValidationScopePart1);
            TestScopes.AddMenuAction(DisableSelectorValidationScopePart2);
        }

        public PXAction<SOOrder> TestScopes;
        //[PXButton(MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Test Scopes")]
        protected void testScopes()
        {
        }

        public PXAction<SOOrder> DisableSelectorValidationScopePart1;
        [PXButton]
        [PXUIField(DisplayName = "DisableSelectorValidationScope (without)")]
        protected void disableSelectorValidationScopePart1()
        {
            // Retrieve an inactive income account from the chart of accounts and try to set it as sales account -- you'll receive an 
            int? prospectID = GetProspectID();
            SOOrder doc = Base.Document.Current;
            doc.CustomerID = prospectID;
            Base.Document.Update(doc);
        }

        public PXAction<SOOrder> DisableSelectorValidationScopePart2;
        [PXButton]
        [PXUIField(DisplayName = "DisableSelectorValidationScope (with)")]
        protected void disableSelectorValidationScopePart2()
        {
            // Retrieve an inactive income account from the chart of accounts and try to set it as sales account, this time using DisableSelectorValidationScope
            using (new DisableSelectorValidationScope(Base.Document.Cache))
            {
                int? prospectID = GetProspectID();
                SOOrder doc = Base.Document.Current;
                doc.CustomerID = prospectID;
                Base.Document.Update(doc);
            }
        }
        
        private int? GetProspectID()
        {
            BAccount prospect = (BAccount) PXSelect<BAccount, Where<BAccount.type, Equal<BAccountType.prospectType>>>.SelectWindowed(Base, 0, 1);
            if (prospect == null) throw new PXException("No prospect found!");
            return prospect.BAccountID;

        }
    }
}
