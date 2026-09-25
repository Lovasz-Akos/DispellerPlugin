namespace Dispeller.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using Dalamud.Bindings.ImGui;
    using Dalamud.Interface.Textures.TextureWraps;
    using Dalamud.Interface.Utility.Raii;
    using Dalamud.Interface.Windowing;
    using Dispeller.Services;
    using Lumina.Excel;
    using Lumina.Excel.Sheets;

    public class MainWindow : Window, IDisposable
    {
        private readonly Plugin plugin;
        private List<SharedModelGroup>? sharedGroups;
        private bool isScanning = false;
        private string statusMessage = "Ready to scan!";

        // Darker purple + soft magenta + text colors
        private static readonly Vector4 DarkerPurple = new(0.28f, 0.20f, 0.45f, 1.00f);  // deep purple
        private static readonly Vector4 SoftMagenta = new(0.78f, 0.37f, 0.64f, 1.00f);  // soft magenta
        private static readonly Vector4 HeaderEdge = new(0.20f, 0.15f, 0.35f, 1.00f);  // even darker edge
        private static readonly Vector4 BrightWhite = new(1.00f, 1.00f, 1.00f, 1.00f);  // white for most text
        private static readonly Vector4 AshBlack = new(0.10f, 0.10f, 0.10f, 1.00f);  // ash black for dropdown header text only

        // Light variants for UI elements
        private static readonly Vector4 LightMagenta = new(0.88f, 0.47f, 0.74f, 1.00f);  // lighter magenta (pink) for main gear
        private static readonly Vector4 LightPurple = new(0.65f, 0.60f, 0.80f, 1.00f);  // pastel purple for accessories (lighter, softer)
        private static readonly Vector4 LightMintGreen = new(0.50f, 0.85f, 0.75f, 1.00f);  // minty green for weapons

        public MainWindow(Plugin plugin)
            : base("Dispeller - Shared Model Analyzer", ImGuiWindowFlags.NoScrollbar)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(600, 400),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            this.plugin = plugin;
        }

        public void Dispose() { }

        public override void Draw()
        {
            // Pink gradient header
            this.DrawHeader();

            ImGui.Spacing();

            // Scan button
            this.DrawScanButton();

            ImGui.Spacing();

            // Status message
            this.DrawStatus();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Results display
            this.DrawResults();

            // Footer
            this.DrawFooter();
        }

        private void DrawHeader()
        {
            float windowWidth = ImGui.GetWindowSize().X;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            Vector2 cursorPos = ImGui.GetCursorScreenPos();

            // Dark purple/magenta gradient background
            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(windowWidth, 60),
                ImGui.ColorConvertFloat4ToU32(SoftMagenta),
                ImGui.ColorConvertFloat4ToU32(DarkerPurple),
                ImGui.ColorConvertFloat4ToU32(DarkerPurple),
                ImGui.ColorConvertFloat4ToU32(SoftMagenta)
            );

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
            ImGui.SetCursorPosX(20);

            // Title
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            ImGui.SetWindowFontScale(1.2f);
            ImGui.TextUnformatted("Dispeller Revived");
            ImGui.SetWindowFontScale(1.0f);
            ImGui.PopStyleColor();

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5);
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 50);
        }

        private void DrawScanButton()
        {
            float contentWidth = ImGui.GetContentRegionAvail().X;
            float buttonWidth = contentWidth * 0.5f;
            float centerPos = (contentWidth - buttonWidth - 90) / 2;
            if (centerPos < 0)
            {
                centerPos = 0;
            }

            ImGui.SetCursorPosX(centerPos);

            if (this.isScanning)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, LightPurple);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, LightPurple);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, LightPurple);
                ImGui.PushStyleColor(ImGuiCol.Text, BrightWhite);

                if (ImGui.Button("Scanning...", new Vector2(buttonWidth, 40)))
                {
                    // Cancelled during scan
                }

                ImGui.PopStyleColor(4);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, SoftMagenta);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(SoftMagenta.X, SoftMagenta.Y, SoftMagenta.Z, 0.8f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, DarkerPurple);
                ImGui.PushStyleColor(ImGuiCol.Text, BrightWhite);

                if (ImGui.Button("Scan Glamour Dresser", new Vector2(buttonWidth, 40)))
                {
                    this.ScanDresser();
                }

                ImGui.PopStyleColor(4);
            }

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);

            ImGui.PushStyleColor(ImGuiCol.Button, HeaderEdge);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, SoftMagenta);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, DarkerPurple);
            ImGui.PushStyleColor(ImGuiCol.Text, BrightWhite);

            if (ImGui.Button("Settings", new Vector2(80, 40)))
            {
                this.plugin.ToggleConfigUi();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Open Dispeller Revived Settings");
            }

            ImGui.PopStyleColor(4);
        }

        private void DrawStatus()
        {
            float centerPos = (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(this.statusMessage).X) / 2;
            ImGui.SetCursorPosX(centerPos);

            ImGui.PushStyleColor(ImGuiCol.Text, this.isScanning ? SoftMagenta : BrightWhite);
            ImGui.TextUnformatted(this.statusMessage);
            ImGui.PopStyleColor();
        }

        private void DrawResults()
        {
            if (this.sharedGroups == null || this.sharedGroups.Count == 0)
            {
                string message = "Click Scan to analyze your glamour dresser!";
                float centerPos = (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(message).X) / 2;
                ImGui.SetCursorPosX(centerPos);

                ImGui.PushStyleColor(ImGuiCol.Text, BrightWhite);
                ImGui.TextUnformatted(message);
                ImGui.PopStyleColor();
                return;
            }

            if (!ImGui.BeginChild("Results", Vector2.Zero, false))
            {
                return;
            }

            foreach (SharedModelGroup? group in this.sharedGroups.Where(g => g.Items.Count > 0))
            {
                this.DrawSharedGroup(group);
                ImGui.Spacing();
            }

            ImGui.EndChild();
        }

        private void DrawSharedGroup(SharedModelGroup group)
        {
            using ImRaii.IdDisposable id = ImRaii.PushId($"{group.SlotCategory}-{group.Items.Count}");

            // Get color based on slot category
            Vector4 groupColor = this.GetColorForSlot(group.SlotCategory);
            ImGui.PushStyleColor(ImGuiCol.Header, groupColor);
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, groupColor);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, groupColor);
            ImGui.PushStyleColor(ImGuiCol.Text, AshBlack);

            string headerText = $"{group.SlotCategory} ({group.Items.Count} items)";

            if (ImGui.CollapsingHeader(headerText))
            {
                ImGui.PopStyleColor(4);

                string? previousModelId = null;
                foreach (SharedModelItem item in group.Items)
                {
                    // Visual separator if model changes (items with matching models will be adjacent)
                    if (previousModelId != null && previousModelId != item.ModelId)
                    {
                        ImGui.Spacing();
                    }

                    previousModelId = item.ModelId;

                    this.DrawItem(item, group.Items);
                }
            }
            else
            {
                ImGui.PopStyleColor(4);
            }
        }

        private void DrawItem(SharedModelItem item, List<SharedModelItem> allItemsInSlot)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 20);

            // Check if this item has matching models (more than one item with same model ID)
            int matchingModelCount = allItemsInSlot.Count(i => i.ModelId == item.ModelId);
            bool hasMatchingModels = matchingModelCount > 1;

            // Try to get icon
            IDalamudTextureWrap? icon = this.GetIcon((uint)item.IconId);
            if (icon == null)
            {
                uint luminaIconId = this.GetItemIconFromLumina(item.ItemId);
                if (luminaIconId != 0 && luminaIconId != item.IconId)
                {
                    icon = this.GetIcon(luminaIconId);
                }
            }

            if (icon != null)
            {
                ImGui.Image(icon.Handle, new Vector2(32, 32));
                ImGui.SameLine();
            }
            else
            {
                // Draw a placeholder if icon is missing
                ImGui.Dummy(new Vector2(32, 32));
                ImGui.SameLine();
            }

            // Get display name - fallback if empty
            string displayName = string.IsNullOrWhiteSpace(item.Name) ? $"Item #{item.ItemId}" : item.Name;

            // Add indicator for matching models
            if (hasMatchingModels)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, SoftMagenta);
                ImGui.TextUnformatted($"🔗 {displayName}");
                ImGui.PopStyleColor();

                // Tooltip showing matching items
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted($"Matches {matchingModelCount} items with model: {item.ModelId}");
                    ImGui.EndTooltip();
                }
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, BrightWhite);
                ImGui.TextUnformatted(displayName);
                ImGui.PopStyleColor();
            }

            // Draw dye slot indicators (circles) - similar to Glamaholic
            if (item.DyeCount > 0)
            {
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                Vector2 basePos = ImGui.GetCursorScreenPos();
                float circleRadius = 4.0f;
                float circleSpacing = 8.0f;
                // Use white/light gray for empty circles (visible on dark background)
                uint circleColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.85f, 0.85f, 0.85f, 1.0f));

                // Draw circles for each dye slot (1 or 2)
                for (int i = 0; i < item.DyeCount; i++)
                {
                    Vector2 circleCenter = basePos + new Vector2(circleRadius + 2, circleRadius + 2) + new Vector2(i * circleSpacing, 0);
                    // Draw empty circle outline (similar to Glamaholic - empty circles indicate available dye slots)
                    drawList.AddCircle(circleCenter, circleRadius + 1, circleColor);
                }

                // Add spacing after circles and create invisible button for tooltip
                float circlesWidth = (item.DyeCount * circleSpacing) + 4;
                _ = ImGui.InvisibleButton($"dye_{item.ItemId}", new Vector2(circlesWidth, (circleRadius * 2) + 4));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted($"{item.DyeCount} dye slot{(item.DyeCount > 1 ? "s" : "")} available");
                    ImGui.EndTooltip();
                }

                // Move cursor past the circles
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + circlesWidth);
            }

            // Draw Armoire marker if item can be stored in Armoire
            if (item.CanGoInArmoire)
            {
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);

                ImGui.PushStyleColor(ImGuiCol.Text, SoftMagenta);
                ImGui.TextUnformatted("[Armoire]");
                ImGui.PopStyleColor();

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("This item can be stored in your Armoire instead of the Glamour Dresser!");
                    ImGui.EndTooltip();
                }
            }

            // Draw Copy Name button
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);

            ImGui.PushStyleColor(ImGuiCol.Button, HeaderEdge);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, SoftMagenta);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, DarkerPurple);
            ImGui.PushStyleColor(ImGuiCol.Text, BrightWhite);

            if (ImGui.Button($"Copy Name##{item.ItemId}_{item.Slot}", new Vector2(80, 22)))
            {
                ImGui.SetClipboardText(displayName);
                this.statusMessage = $"Copied '{displayName}' to clipboard!";
            }

            ImGui.PopStyleColor(4);

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Copy '{displayName}' to clipboard so you can paste it into the search bar!");
            }
        }

        private IDalamudTextureWrap? GetIcon(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            try
            {
                return Plugin.TextureProvider.GetFromGameIcon(new Dalamud.Interface.Textures.GameIconLookup(id)).GetWrapOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private void DrawFooter()
        {
            ImGui.Separator();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);

            string message = "Find shared models in your glamour dresser!";
            float centerPos = (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(message).X) / 2;
            ImGui.SetCursorPosX(centerPos);

            ImGui.PushStyleColor(ImGuiCol.Text, BrightWhite);
            ImGui.TextUnformatted(message);
            ImGui.PopStyleColor();
        }

        private void ScanDresser()
        {
            this.isScanning = true;
            this.statusMessage = "Scanning your glamour dresser...";

            try
            {
                // Check if we have cached data first (before trying to refresh)
                int cachedCountBefore = DresserScanner.GetCachedItemCount();
                bool hasCachedData = cachedCountBefore > 0;

                Plugin.Log.Information($"ScanDresser: Starting scan - cached items before refresh: {cachedCountBefore}");

                // Try to refresh the dresser data if it's currently open (to get latest data)
                bool refreshed = false;
                unsafe
                {
                    refreshed = DresserScanner.TryRefresh();
                }

                int cachedCountAfter = DresserScanner.GetCachedItemCount();
                List<PrismBoxItem> dresserItems = DresserScanner.GetDresserItems();

                Plugin.Log.Information($"Dresser scan: Found {dresserItems.Count} items (cached before: {cachedCountBefore}, cached after: {cachedCountAfter}, refreshed: {refreshed})");

                if (dresserItems.Count == 0)
                {
                    if (!hasCachedData && !refreshed)
                    {
                        this.statusMessage = "Your glamour dresser hasn't been opened yet! Please open your Glamour Dresser at least once, then try again.";
                        Plugin.Log.Warning("Dresser scan: No cached data and dresser not open");
                    }
                    else if (hasCachedData && !refreshed)
                    {
                        this.statusMessage = "Using cached data, but dresser appears empty. Try opening the dresser to refresh.";
                        Plugin.Log.Warning($"Dresser scan: Had {cachedCountBefore} cached items but got 0 after refresh attempt");
                    }
                    else
                    {
                        this.statusMessage = "Your glamour dresser appears to be empty!";
                        Plugin.Log.Warning("Dresser scan: Dresser is open but empty");
                    }

                    this.sharedGroups = null;
                    return;
                }

                // Deduplicate by Slot + ItemId to prevent duplicates from race conditions
                List<PrismBoxItem> uniqueItems = dresserItems
                    .GroupBy(item => new { item.Slot, item.ItemId })
                    .Select(g => g.First())
                    .ToList();

                // Filter out items with unknown slots
                List<PrismBoxItem> validItems = uniqueItems
                    .Where(item =>
                    {
                        string slotName = this.GetSlotName(item.ItemId);
                        return !string.IsNullOrEmpty(slotName) && slotName != "Unknown Slot";
                    })
                    .ToList();

                // Identify items with shared models OR items that can be stored in the Armoire
                List<PrismBoxItem> itemsToDisplay = validItems
                    .GroupBy(item =>
                    {
                        string slotName = this.GetSlotName(item.ItemId);
                        string modelId = this.GetItemModel(item.ItemId);
                        return $"{slotName}-{modelId}";
                    })
                    .Where(g => g.Count() > 1 || g.Any(item => this.CanGoInArmoire(item.ItemId))) // Include duplicate models OR Armoire items
                    .SelectMany(g => g) // Flatten back to individual items
                    .ToList();

                // Now group by slot category only
                List<SharedModelGroup> grouped = itemsToDisplay
                    .GroupBy(item => this.GetSlotName(item.ItemId))
                    .Select(g =>
                    {
                        // Sort items within this slot by model ID so matching models are adjacent
                        List<SharedModelItem> sortedItems = g
                            .OrderBy(item => this.GetItemModel(item.ItemId))
                            .Select(item =>
                            {
                                // Always get item name from Lumina for accuracy
                                // Dresser name can be incorrect/outdated when dresser updates
                                string itemName = this.GetItemNameFromLumina(item.ItemId);

                                // Get icon from Lumina for accuracy (handles HQ and NQ item IDs)
                                uint iconId = this.GetItemIconFromLumina(item.ItemId);
                                if (iconId == 0)
                                {
                                    iconId = item.IconId;
                                }

                                // Get dye count from Lumina
                                byte dyeCount = this.GetItemDyeCount(item.ItemId);

                                // Check if item can be stored in Armoire
                                bool canGoInArmoire = this.CanGoInArmoire(item.ItemId);

                                return new SharedModelItem
                                {
                                    Name = itemName,
                                    ItemId = item.ItemId,
                                    IconId = (int)iconId,
                                    Slot = item.Slot,
                                    ModelId = this.GetItemModel(item.ItemId),
                                    DyeCount = dyeCount,
                                    CanGoInArmoire = canGoInArmoire
                                };
                            })
                            .ToList();

                        return new SharedModelGroup
                        {
                            ModelId = "", // Not used for slot-based grouping
                            SlotCategory = g.Key,
                            Items = sortedItems
                        };
                    })
                    .OrderBy(g => this.GetSlotOrder(g.SlotCategory)) // Sort slots in logical order
                    .ToList();

                if (this.plugin.Configuration.ShowOnlyWeapons)
                {
                    grouped = grouped.Where(g => g.SlotCategory is "Main Hand" or "Off Hand").ToList();
                }
                else if (this.plugin.Configuration.ShowOnlyClothing)
                {
                    grouped = grouped.Where(g => g.SlotCategory is not "Main Hand" and not "Off Hand").ToList();
                }

                this.sharedGroups = grouped;
                int totalItems = grouped.Sum(g => g.Items.Count);
                this.statusMessage = $"Found {totalItems} items (shared models & Armoire items) across {grouped.Count} slot categories!";
            }
            catch (Exception ex)
            {
                this.statusMessage = $"Error: {ex.Message}";
                this.sharedGroups = null;
                Plugin.Log.Error(ex, "Error during dresser scan");
            }
            finally
            {
                this.isScanning = false;
            }
        }

        private readonly Lazy<HashSet<uint>> armoireItemIds = new(() =>
        {
            ExcelSheet<Cabinet> cabinetSheet = Plugin.DataManager.GetExcelSheet<Cabinet>();
            if (cabinetSheet == null)
            {
                return [];
            }

            return cabinetSheet
                .Where(row => row.Item.RowId > 0)
                .Select(row => row.Item.RowId)
                .ToHashSet();
        });

        private static uint GetBaseItemId(uint itemId)
        {
            if (itemId == 0)
            {
                return 0;
            }

            return itemId > 500000 ? itemId % 500000 : (itemId > 100000 ? itemId % 100000 : itemId);
        }

        private string GetItemModel(uint itemId)
        {
            uint baseId = GetBaseItemId(itemId);
            ExcelSheet<Item> sheet = Plugin.DataManager.GetExcelSheet<Item>()!;
            if (!sheet.TryGetRow(baseId, out Item item))
            {
                return "Unknown";
            }

            (ushort, ushort, ushort, ushort) model = ModelDetectionService.ExtractModelInfo(item.ModelMain);
            return ModelDetectionService.GetModelIdString(model);
        }

        private string GetSlotName(uint itemId)
        {
            uint baseId = GetBaseItemId(itemId);
            if (baseId == 0)
            {
                return "Unknown Slot";
            }

            ExcelSheet<Item> sheet = Plugin.DataManager.GetExcelSheet<Item>()!;
            if (!sheet.TryGetRow(baseId, out Item item))
            {
                return "Unknown Slot";
            }

            if (!item.EquipSlotCategory.IsValid)
            {
                return "Unknown Slot";
            }

            EquipSlotCategory category = item.EquipSlotCategory.Value;

            // Check each slot category in priority order
            if (category.MainHand > 0)
            {
                return "Main Hand";
            }

            if (category.OffHand > 0)
            {
                return "Off Hand";
            }

            if (category.Head > 0)
            {
                return "Head";
            }

            if (category.Body > 0)
            {
                return "Body";
            }

            if (category.Gloves > 0)
            {
                return "Gloves";
            }

            if (category.Legs > 0)
            {
                return "Legs";
            }

            if (category.Feet > 0)
            {
                return "Feet";
            }

            if (category.Ears > 0)
            {
                return "Ears";
            }

            if (category.Neck > 0)
            {
                return "Neck";
            }

            if (category.Wrists > 0)
            {
                return "Wrists";
            }

            if (category.FingerR > 0 || category.FingerL > 0)
            {
                return "Ring";
            }

            return "Unknown Slot";
        }

        private string GetItemNameFromLumina(uint itemId)
        {
            uint baseId = GetBaseItemId(itemId);
            if (baseId == 0)
            {
                return "Unknown Item";
            }

            ExcelSheet<Item> sheet = Plugin.DataManager.GetExcelSheet<Item>()!;
            if (!sheet.TryGetRow(baseId, out Item item))
            {
                return "Unknown Item";
            }

            return item.Name.ExtractText();
        }

        private uint GetItemIconFromLumina(uint itemId)
        {
            uint baseId = GetBaseItemId(itemId);
            if (baseId == 0)
            {
                return 0;
            }

            ExcelSheet<Item> sheet = Plugin.DataManager.GetExcelSheet<Item>()!;
            if (!sheet.TryGetRow(baseId, out Item item))
            {
                return 0;
            }

            return item.Icon;
        }

        private byte GetItemDyeCount(uint itemId)
        {
            uint baseId = GetBaseItemId(itemId);
            if (baseId == 0)
            {
                return 0;
            }

            ExcelSheet<Item> sheet = Plugin.DataManager.GetExcelSheet<Item>()!;
            if (!sheet.TryGetRow(baseId, out Item item))
            {
                return 0;
            }

            return item.DyeCount;
        }

        private bool CanGoInArmoire(uint itemId)
        {
            uint baseId = GetBaseItemId(itemId);
            return baseId != 0 && this.armoireItemIds.Value.Contains(baseId);
        }

        private int GetSlotOrder(string slotName)
        {
            // Return order value for slot sorting (lower = appears first)
            return slotName switch
            {
                "Main Hand" => 1,
                "Off Hand" => 2,
                "Head" => 3,
                "Body" => 4,
                "Gloves" => 5,
                "Legs" => 6,
                "Feet" => 7,
                "Ears" => 8,
                "Neck" => 9,
                "Wrists" => 10,
                "Ring" => 11,
                _ => 99
            };
        }

        private Vector4 GetColorForSlot(string slotName)
        {
            // Accessories - purple
            if (slotName is "Ears" or "Neck" or "Wrists" or "Ring")
            {
                return LightPurple;
            }

            // Main gear - pink/magenta
            if (slotName is "Head" or "Body" or "Gloves" or "Legs" or "Feet")
            {
                return LightMagenta;
            }

            // Weapons - minty green
            if (slotName is "Main Hand" or "Off Hand")
            {
                return LightMintGreen;
            }

            // Default to purple if unknown
            return LightPurple;
        }
    }

    public class SharedModelGroup
    {
        public string ModelId { get; set; } = string.Empty;
        public string SlotCategory { get; set; } = string.Empty;
        public List<SharedModelItem> Items { get; set; } = [];
    }

    public class SharedModelItem
    {
        public string Name { get; set; } = string.Empty;
        public uint ItemId { get; set; }
        public int IconId { get; set; }
        public uint Slot { get; set; }
        public string ModelId { get; set; } = string.Empty;
        public byte DyeCount { get; set; }
        public bool CanGoInArmoire { get; set; }
    }
}
