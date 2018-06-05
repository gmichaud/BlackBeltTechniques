using System;
using System.Collections;
using PX.Data;

namespace Velixo.BlackBeltTechniques
{
    public class GlobalGraphExtension : PXGraphExtension<PXGraph>
    {
        public override void Initialize()
        {
            if (!String.IsNullOrEmpty(Base.PrimaryView))
            { 
                Type primaryViewItemType = Base.Views[Base.PrimaryView].Cache.GetItemType();
                PXAction action = PXNamedAction.AddAction(Base, primaryViewItemType, "Test", "Test", TestClick);
            }
        }

        public IEnumerable TestClick(PXAdapter adapter)
        {
            throw new PXException("Test button clicked from graph " + Base.GetType().Name);
        }
    }
}
