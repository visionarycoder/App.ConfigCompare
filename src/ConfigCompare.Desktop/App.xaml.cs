using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using ConfigCompare.AppConfig;
using ConfigCompare.AzureIdentity;
using ConfigCompare.ResourceGroup;
using ConfigCompare.Session;
using ConfigCompare.Settings;

namespace ConfigCompare.Desktop;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public static Window? MainWindowInstance { get; private set; }

    public static string SettingsFilePath { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConfigCompare",
            "settings.json");

    public static string SessionDbPath { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConfigCompare",
            "sessions.db");

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            var settingsDir = Path.GetDirectoryName(SettingsFilePath)!;
            if (!Directory.Exists(settingsDir))
                Directory.CreateDirectory(settingsDir);

            var services = new ServiceCollection();
            services.AddLogging(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));
            services.AddAzureIdentityServices();
            services.AddAppConfigService();
            services.AddSettingsServices(SettingsFilePath);
            services.AddResourceGroupServices();
            services.AddSessionServices(SessionDbPath);
            ServiceProvider = services.BuildServiceProvider();
        }
        catch
        {
            ServiceProvider = new ServiceCollection().BuildServiceProvider();
        }

        MainWindowInstance = new MainWindow();
        MainWindowInstance.Activate();
    }
}
