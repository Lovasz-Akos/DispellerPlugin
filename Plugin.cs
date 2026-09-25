namespace Dispeller
{
    using Dalamud.Game.Command;
    using Dalamud.Interface.Windowing;
    using Dalamud.IoC;
    using Dalamud.Plugin;
    using Dalamud.Plugin.Services;
    using Dispeller.Services;
    using Dispeller.Windows;

    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService] internal static IFramework Framework { get; private set; } = null!;
        [PluginService] internal static IPluginLog Log { get; private set; } = null!;

        private const string CommandName = "/dispeller";

        public Configuration Configuration { get; init; }
        public DresserScanner DresserScanner { get; init; }

        public readonly WindowSystem WindowSystem = new("Dispeller");
        public MainWindow MainWindow { get; init; }
        public ConfigWindow ConfigWindow { get; init; }

        public Plugin()
        {
            this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            this.DresserScanner = new DresserScanner();

            this.MainWindow = new MainWindow(this);
            this.ConfigWindow = new ConfigWindow(this);

            this.WindowSystem.AddWindow(this.MainWindow);
            this.WindowSystem.AddWindow(this.ConfigWindow);

            _ = CommandManager.AddHandler(CommandName, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Open Dispeller - Find shared models in your glamour dresser!"
            });

            PluginInterface.UiBuilder.Draw += this.WindowSystem.Draw;
            PluginInterface.UiBuilder.OpenMainUi += this.ToggleMainUi;
            PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigUi;

            Log.Information($"===Dispeller plugin loaded! Ready to find shared models!===");
        }

        public void Dispose()
        {
            PluginInterface.UiBuilder.Draw -= this.WindowSystem.Draw;
            PluginInterface.UiBuilder.OpenMainUi -= this.ToggleMainUi;
            PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleConfigUi;

            this.WindowSystem.RemoveAllWindows();

            this.MainWindow.Dispose();
            this.ConfigWindow.Dispose();
            this.DresserScanner.Dispose();

            _ = CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args) => this.MainWindow.Toggle();

        public void ToggleMainUi() => this.MainWindow.Toggle();
        public void ToggleConfigUi() => this.ConfigWindow.Toggle();
    }
}
