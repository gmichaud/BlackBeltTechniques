using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace Velixo.BlackBeltTechniques
{
    public class InventoryItemExt : PXCacheExtension<InventoryItem>
    {
        public abstract class salesCategories : IBqlField { }
        [PXUIField(DisplayName = "Sales Categories", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [SalesCategories(typeof(InventoryItem.inventoryID))]
        public decimal? SalesCategories { get; set; }
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
                //Bad!!!
                var categoryList = PXSelectJoin<INCategory, 
                    InnerJoin<INItemCategory, On<INCategory.categoryID, Equal<INItemCategory.categoryID>>>,
                    Where<INItemCategory.inventoryID, Equal<Required<INItemCategory.inventoryID>>>>.Select(sender.Graph, inventoryID);

                string itemCategories = String.Empty;
                foreach(INCategory cat in categoryList)
                {
                    if(String.IsNullOrEmpty(itemCategories))
                    {
                        itemCategories = cat.Description;
                    }
                    else
                    {
                        itemCategories = itemCategories + ", " + cat.Description;
                    }
                }

                e.ReturnValue = itemCategories;

                //Much better!!!
                //e.ReturnValue = ItemCategoriesDefinition.GetItemCategories(inventoryID);
            }
        }
    }

    public class ItemCategoriesDefinition : IPrefetchable
    {
        private readonly Dictionary<int?, string> _itemCategories = new Dictionary<int?, string>();
        
        public void Prefetch()
        {
            //The system will automatically call Prefetch() the first time we retrieve this class from our slot (via the GetSlot() function below)
            //In this example, we load some data off the DB and cache it in the dictionary, but any sort of loading/pre-computation/caching could be performed inside this function
            _itemCategories.Clear();

            //Load category names and store into a dictionary for quick retrieval
            var categoryNames = new Dictionary<int?, string>();
            foreach (PXDataRecord record in PXDatabase.SelectMulti<INCategory>(
                new PXDataField<INCategory.categoryID>(),
                new PXDataField<INCategory.description>()))
            {
                int? categoryID = record.GetInt32(0);
                string description = record.GetString(1);

                categoryNames.Add(categoryID, description);
            }

            //Build comma-delemited list of categories for each item
            foreach (PXDataRecord record in PXDatabase.SelectMulti<INItemCategory>(
                new PXDataField<INItemCategory.inventoryID>(),
                new PXDataField<INItemCategory.categoryID>()))
            {
                int? inventoryID = record.GetInt32(0);
                int? categoryID = record.GetInt32(1);
                
                string categoryName = String.Empty;
                if (categoryNames.TryGetValue(categoryID, out categoryName))
                {
                    string categoryList = String.Empty;
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
            //Returns or initializes a new instance of ItemCategoriesDefinition. System will automatically invalidate and Prefetch again if INCategory or INItemCategory tables are updated.
            return PXDatabase.GetSlot<ItemCategoriesDefinition>("ItemCategories", typeof(INCategory), typeof(INItemCategory));
        }
    }
}