namespace Dispeller.Windows
{
    using System;
    using System.Numerics;
    using Dalamud.Bindings.ImGui;
    using Dalamud.Interface.Windowing;

    public class ConfigWindow : Window, IDisposable
    {
        private readonly Plugin plugin;

        public ConfigWindow(Plugin plugin)
            : base("Dispeller Configuration", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(320, 160),
                MaximumSize = new Vector2(600, 400)
            };

            this.plugin = plugin;
        }

        public void Dispose() { }

        public override void Draw()
        {
            Configuration config = this.plugin.Configuration;
            bool changed = false;

            ImGui.TextUnformatted("Filter Settings");
            ImGui.Separator();
            ImGui.Spacing();

            bool showOnlyWeapons = config.ShowOnlyWeapons;
            if (ImGui.Checkbox("Show only weapons", ref showOnlyWeapons))
            {
                config.ShowOnlyWeapons = showOnlyWeapons;
                if (showOnlyWeapons)
                {
                    config.ShowOnlyClothing = false;
                }

                changed = true;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Filter results to only display Main Hand and Off Hand items.");
            }

            bool showOnlyClothing = config.ShowOnlyClothing;
            if (ImGui.Checkbox("Show only clothing / armor", ref showOnlyClothing))
            {
                config.ShowOnlyClothing = showOnlyClothing;
                if (showOnlyClothing)
                {
                    config.ShowOnlyWeapons = false;
                }

                changed = true;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Filter results to only display non-weapon armor and gear pieces.");
            }

            if (changed)
            {
                config.Save();
            }
        }
    }
}
