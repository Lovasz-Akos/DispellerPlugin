namespace Dispeller.Services
{
    using System.Collections.Generic;
    using Lumina.Excel.Sheets;

    public class ModelDetectionService
    {
        public static (ushort, ushort, ushort, ushort) ExtractModelInfo(ulong raw, bool ignoreVariant)
        {
            ushort primaryKey = (ushort)(raw & 0xFFFF);
            ushort secondaryKey = (ushort)((raw >> 16) & 0xFFFF);
            ushort weaponVariant = (ushort)((raw >> 32) & 0xFFFF);

            bool isWeapon = weaponVariant != 0;

            if (ignoreVariant)
            {
                if (isWeapon)
                {
                    return (primaryKey, secondaryKey, 0, 0);
                }

                return (primaryKey, 0, 0, 0);
            }

            if (isWeapon)
            {
                return (primaryKey, secondaryKey, weaponVariant, 0);
            }

            return (primaryKey, secondaryKey, 0, 0);
        }

        public static bool ShareModel(Item item1, Item item2, bool ignoreVariant)
        {
            (ushort, ushort, ushort, ushort) model1 = ExtractModelInfo(item1.ModelMain, ignoreVariant);
            (ushort, ushort, ushort, ushort) model2 = ExtractModelInfo(item2.ModelMain, ignoreVariant);

            return model1 == model2;
        }

        public static List<Item> FindSharedModelItems(Item targetItem, bool ignoreVariant)
        {
            (ushort, ushort, ushort, ushort) targetModel = ExtractModelInfo(targetItem.ModelMain, ignoreVariant);
            List<Item> sharedItems = new List<Item>();

            foreach (Item item in Plugin.DataManager.GetExcelSheet<Item>()!)
            {
                if (item.EquipSlotCategory.RowId != targetItem.EquipSlotCategory.RowId)
                {
                    continue;
                }

                if (ExtractModelInfo(item.ModelMain, ignoreVariant) == targetModel)
                {
                    sharedItems.Add(item);
                }
            }

            return sharedItems;
        }

        public static string GetModelIdString((ushort, ushort, ushort, ushort) model) => $"{model.Item1}-{model.Item2}-{model.Item3}-{model.Item4}";
    }
}
