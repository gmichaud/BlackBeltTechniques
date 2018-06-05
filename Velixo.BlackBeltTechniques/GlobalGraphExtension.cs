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
                //Add a "Test" button in the toolbar
                Type primaryViewItemType = Base.Views[Base.PrimaryView].Cache.GetItemType();
                PXAction action = PXNamedAction.AddAction(Base, primaryViewItemType, "Test", "Test", TestClick);

                //Liste to FieldUpdating event for NoteText on primary view; we could also do it on every view that has a Note field.
                PXCache primaryViewCache = Base.Caches[primaryViewItemType];
                if (primaryViewCache.Fields.Contains("NoteText"))
                {
                    Base.FieldUpdating.AddHandler(Base.PrimaryView, "NoteText", NoteFieldUpdating);
                }
            }
        }

        public IEnumerable TestClick(PXAdapter adapter)
        {
            throw new PXException("Test button clicked from graph " + Base.GetType().Name);
        }

        public void NoteFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.NewValue as string))
            {
                //The change will only be visible after reloading the record since the note panel caches it...
                e.NewValue = e.NewValue + $" (added {DateTime.Now.ToString()} by {PXAccess.GetUserLogin()})\n";
            }
        }
    }
}
