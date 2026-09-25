namespace Dispeller.Services
{
    using System.Collections.Generic;
    using Lumina.Excel.Sheets;

    public class ModelDetectionService
    {
        /// <summary>
        /// Extract model information from Item.ModelMain
        /// Based on Glamaholic's AlternativeFinder.ModelInfo
        /// </summary>
        public static (ushort, ushort, ushort, ushort) ExtractModelInfo(ulong raw)
        {
            ushort primaryKey = (ushort)(raw & 0xFFFF);
            ushort secondaryKey = (ushort)((raw >> 16) & 0xFFFF);
            ushort variant = (ushort)((raw >> 32) & 0xFFFF);
            ushort dye = (ushort)((raw >> 48) & 0xFFFF);

            if (variant != 0)
            {
                // weapon
                return (primaryKey, secondaryKey, variant, dye);
            }

            return (primaryKey, 0, 0, 0);
        }

        /// <summary>
        /// Check if two items share the same model
        /// </summary>
        public static bool ShareModel(Item item1, Item item2)
        {
            (ushort, ushort, ushort, ushort) model1 = ExtractModelInfo(item1.ModelMain);
            (ushort, ushort, ushort, ushort) model2 = ExtractModelInfo(item2.ModelMain);

            return model1 == model2;
        }

        /// <summary>
        /// Get all items that share a model with the given item
        /// </summary>
        public static List<Item> FindSharedModelItems(Item targetItem)
        {
            (ushort, ushort, ushort, ushort) targetModel = ExtractModelInfo(targetItem.ModelMain);
            List<Item> sharedItems = new List<Item>();

            foreach (Item item in Plugin.DataManager.GetExcelSheet<Item>()!)
            {
                if (item.EquipSlotCategory.RowId != targetItem.EquipSlotCategory.RowId)
                {
                    continue;
                }

                if (ExtractModelInfo(item.ModelMain) == targetModel)
                {
                    sharedItems.Add(item);
                }
            }

            return sharedItems;
        }

        /// <summary>
        /// Get model ID string for display
        /// </summary>
        public static string GetModelIdString((ushort, ushort, ushort, ushort) model) => $"{model.Item1}-{model.Item2}-{model.Item3}-{model.Item4}";
    }
}
