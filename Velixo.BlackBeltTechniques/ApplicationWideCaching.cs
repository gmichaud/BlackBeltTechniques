using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace Velixo.BlackBeltTechniques
{
    public class ItemCategoriesDefinition : IPrefetchable
    {
        private readonly Dictionary<int?, string> _categories = new Dictionary<int?, string>();
        private readonly Dictionary<int?, string> _itemCategories = new Dictionary<int?, string>();
        
        public void Prefetch()
        {
            _categories.Clear();
            _itemCategories.Clear();

            foreach (PXDataRecord record in PXDatabase.SelectMulti<INCategory>(
                new PXDataField<INCategory.categoryID>(),
                new PXDataField<INCategory.description>()))
            {
                int? categoryID = record.GetInt32(0);
                string description = record.GetString(1);
                
                _categories.Add(categoryID, description);
            }

            foreach (PXDataRecord record in PXDatabase.SelectMulti<INItemCategory>(
                new PXDataField<INItemCategory.inventoryID>(),
                new PXDataField<INItemCategory.categoryID>()))
            {
                int? inventoryID = record.GetInt32(0);
                int? categoryID = record.GetInt32(1);

                string categoryName = String.Empty;
                string categoryList = String.Empty;

                if (_categories.TryGetValue(categoryID, out categoryName))
                {
                    if (!_itemCategories.TryGetValue(inventoryID, out categoryList))
                    {
                        _itemCategories.Add(inventoryID, categoryName);
                    }
                    else
                    {
                        _itemCategories[inventoryID] = categoryList + ", " + categoryName;
                    }
                }
            }
        }

        public Dictionary<int?, string> Categories
        {
            get
            {
                return _categories;
            }
        }

        public Dictionary<int?, string> ItemCategories
        {
            get
            {
                return _itemCategories;
            }
        }
        
        public static string GetItemCategories(int? inventoryID)
        {
            var def = GetSlot();

            string itemCategories = String.Empty;
            if (def.ItemCategories.TryGetValue(inventoryID, out itemCategories))
            {
                return itemCategories;
            }
            else
            {
                return String.Empty;
            }
        }

        private static ItemCategoriesDefinition GetSlot()
        {
            return PXDatabase.GetSlot<ItemCategoriesDefinition>("ItemCategories", typeof(INCategory), typeof(INItemCategory));
        }
    }

    [PXString]
    public class SalesCategoriesAttribute : PXAggregateAttribute, IPXFieldSelectingSubscriber
    {
        private readonly Type _inventoryIDField;

        public SalesCategoriesAttribute(Type inventoryIDField)
        {
            _inventoryIDField = inventoryIDField;
        }

        public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            int? inventoryID = (int?)sender.GetValue(e.Row, _inventoryIDField.Name);
            if (inventoryID != null)
            {
                e.ReturnValue = ItemCategoriesDefinition.GetItemCategories(inventoryID);
            }
        }
    }

    public class InventoryItemExt : PXCacheExtension<InventoryItem>
    {
        public abstract class salesCategories : IBqlField { }
        [PXUIField(DisplayName = "Sales Categories", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [SalesCategories(typeof(InventoryItem.inventoryID))]
        public decimal? SalesCategories { get; set; }
    }
}