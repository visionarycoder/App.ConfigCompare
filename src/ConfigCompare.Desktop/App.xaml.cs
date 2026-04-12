using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConfigCompare.AppConfig;
using ConfigCompare.Settings;

namespace ConfigCompare.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>Path to the user settings JSON file in the local app-data folder.</summary>
    public static string SettingsFilePath { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConfigCompare",
            "settings.json");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Logging
        services.AddLogging(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));

        // App services
        services.AddAppConfigService();
        services.AddSettingsServices(SettingsFilePath);

        ServiceProvider = services.BuildServiceProvider();
    }
}

