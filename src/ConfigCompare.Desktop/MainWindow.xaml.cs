using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ConfigCompare.AppConfig;
using ConfigCompare.AppConfig.Resources;
using ConfigCompare.Settings;
using ConfigCompare.Settings.Resources;
using ConfigCompare.Desktop.Security;
using ConfigCompare.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigCompare.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IAppConfigService _appConfigService;
    private readonly ISettingsService _settingsService;
    private readonly ShellViewModel _shell = new();
    private AzureConnectionDto? _currentConnection;

    public MainWindow()
    {
        InitializeComponent();
        _appConfigService = App.ServiceProvider.GetRequiredService<IAppConfigService>();
        _settingsService  = App.ServiceProvider.GetRequiredService<ISettingsService>();

        Loaded += MainWindow_Loaded;
        ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
        RegisterButtonHandlers();
    }

    // -------------------------------------------------------------------------
    // Startup
    // -------------------------------------------------------------------------

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        DisplayVersionInfo();
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var result = await _settingsService.GetSettingsAsync();
        if (result.IsSuccess && result.Data is { } settings)
        {
            _currentConnection = settings.AzureConnection;
            UpdateConnectionStatus(_currentConnection);
            PopulateSettingsPopupConnection(_currentConnection);

            if (_currentConnection is null || string.IsNullOrWhiteSpace(_currentConnection.Endpoint))
                ShowConnectionDialog();
        }
        else
        {
            ShowConnectionDialog();
        }
    }

    private void DisplayVersionInfo()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        var versionString = version is not null
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : "1.0.0";

        if (VersionNumberTextBlock is not null)
            VersionNumberTextBlock.Text = $"Version: {versionString}";

        if (BuildDateTextBlock is not null)
            BuildDateTextBlock.Text = $"Build Date: {DateTime.Now:MMMM d, yyyy}";

        if (StatusVersionTextBlock is not null)
            StatusVersionTextBlock.Text = $"v{versionString}";

        if (FindName("PopupVersionNumberTextBlock") is TextBlock popupVer)
            popupVer.Text = $"Version: {versionString}";

        if (FindName("PopupBuildDateTextBlock") is TextBlock popupDate)
            popupDate.Text = $"Build Date: {DateTime.Now:MMMM d, yyyy}";
    }

    // -------------------------------------------------------------------------
    // Button handler registration
    // -------------------------------------------------------------------------

    private void RegisterButtonHandlers()
    {
        MinimizeButton.Click        += (_, _) => WindowState = WindowState.Minimized;
        MaximizeRestoreButton.Click += MaximizeRestoreButton_Click;
        CloseWindowButton.Click     += (_, _) => Close();

        NavCompareButton.Click      += (_, _) => MainTabControl.SelectedIndex = 0;
        NavEditConfigButton.Click   += (_, _) => MainTabControl.SelectedIndex = 3;
        NavFindReplaceButton.Click  += (_, _) => MainTabControl.SelectedIndex = 4;
        NavCopySettingsButton.Click += (_, _) => MainTabControl.SelectedIndex = 5;

        SettingsIconButton.Click       += (_, _) => OpenSettingsPopup();
        CloseSettingsButton.Click      += (_, _) => CloseSettingsPopup();
        SaveSettingsPopupButton.Click  += SaveSettingsPopupButton_Click;
        ResetSettingsPopupButton.Click += ResetSettingsPopupButton_Click;

        SaveConnectionPopupButton.Click  += SaveConnectionPopupButton_Click;
        ClearConnectionPopupButton.Click += ClearConnectionPopupButton_Click;

        SettingsTabConnectButton.Click    += (_, _) => { CloseSettingsPopup(); ShowConnectionDialog(); };
        SettingsTabDisconnectButton.Click += SettingsTabDisconnectButton_Click;
        SaveSettingsButton.Click          += SaveSettingsButton_Click;
        ResetSettingsButton.Click         += ResetSettingsButton_Click;

        ConnectionDialogConnectButton.Click += ConnectionDialogConnectButton_Click;
        ConnectionDialogSkipButton.Click    += (_, _) => HideConnectionDialog();

        CompareButton.Click += CompareButton_Click;
        ClearButton.Click   += ClearButton_Click;

        SaveConfigButton.Click      += SaveConfigButton_Click;
        ClearEditConfigButton.Click += ClearEditConfigButton_Click;

        ExecuteFindReplaceButton.Click += ExecuteFindReplaceButton_Click;
        ClearFindReplaceButton.Click   += ClearFindReplaceButton_Click;

        ExecuteCopySettingsButton.Click += ExecuteCopySettingsButton_Click;
        ClearCopySettingsButton.Click   += ClearCopySettingsButton_Click;
    }

    // -------------------------------------------------------------------------
    // Window chrome
    // -------------------------------------------------------------------------

    private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        MaximizeRestoreButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    // -------------------------------------------------------------------------
    // Settings popup
    // -------------------------------------------------------------------------

    private void OpenSettingsPopup()
    {
        PopulateSettingsPopupConnection(_currentConnection);
        SettingsPopupOverlay.Visibility = Visibility.Visible;
    }

    private void CloseSettingsPopup()
        => SettingsPopupOverlay.Visibility = Visibility.Hidden;

    private void SettingsPopupOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Grid grid && grid.Name == "SettingsPopupOverlay")
        {
            var hit = VisualTreeHelper.HitTest(grid, Mouse.GetPosition(grid));
            if (hit?.VisualHit is DependencyObject hitObj)
            {
                var parent = VisualTreeHelper.GetParent(hitObj);
                while (parent is not null)
                {
                    if (parent is Border) return;
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
            CloseSettingsPopup();
        }
    }

    private void SaveSettingsPopupButton_Click(object sender, RoutedEventArgs e)
    {
        var theme = (SettingsThemeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Light";
        MessageBox.Show($"Settings saved!\nTheme: {theme}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetSettingsPopupButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsThemeComboBox.SelectedIndex = 0;
        MessageBox.Show("Settings reset to default.", "Reset", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // -------------------------------------------------------------------------
    // Connection dialog
    // -------------------------------------------------------------------------

    private void ShowConnectionDialog()
    {
        PopulateConnectionDialog(_currentConnection);
        ConnectionDialogOverlay.Visibility = Visibility.Visible;
    }

    private void HideConnectionDialog()
        => ConnectionDialogOverlay.Visibility = Visibility.Hidden;

    private void PopulateConnectionDialog(AzureConnectionDto? connection)
    {
        if (connection is null) return;
        DialogConnectionEndpointTextBox.Text = connection.Endpoint;
        DialogAuthMethodComboBox.SelectedIndex = connection.AuthMethod switch
        {
            AuthMethod.ServicePrincipal => 1,
            AuthMethod.ManagedIdentity  => 2,
            _                           => 0
        };
        if (!string.IsNullOrWhiteSpace(connection.TenantId))
            DialogTenantIdTextBox.Text = connection.TenantId;
        if (!string.IsNullOrWhiteSpace(connection.ClientId))
            DialogClientIdTextBox.Text = connection.ClientId;
        var dialogDecryptedSecret = CredentialProtection.Unprotect(connection.ClientSecret);
        if (!string.IsNullOrWhiteSpace(dialogDecryptedSecret))
            DialogClientSecretBox.Password = dialogDecryptedSecret;
        if (!string.IsNullOrWhiteSpace(connection.SubscriptionId))
            DialogSubscriptionIdTextBox.Text = connection.SubscriptionId;
    }

    private void DialogAuthMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DialogServicePrincipalPanel is null) return;
        DialogServicePrincipalPanel.Visibility =
            DialogAuthMethodComboBox.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void ConnectionDialogConnectButton_Click(object sender, RoutedEventArgs e)
    {
        var endpoint = DialogConnectionEndpointTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            MessageBox.Show("Endpoint URL is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var authMethod = DialogAuthMethodComboBox.SelectedIndex switch
        {
            1 => AuthMethod.ServicePrincipal,
            2 => AuthMethod.ManagedIdentity,
            _ => AuthMethod.DefaultAzureCredential
        };

        if (authMethod == AuthMethod.ServicePrincipal &&
            (string.IsNullOrWhiteSpace(DialogTenantIdTextBox.Text) ||
             string.IsNullOrWhiteSpace(DialogClientIdTextBox.Text)))
        {
            MessageBox.Show("Tenant ID and Client ID are required for Service Principal.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var connection = new AzureConnectionDto(
            endpoint,
            authMethod,
            string.IsNullOrWhiteSpace(DialogTenantIdTextBox.Text)      ? null : DialogTenantIdTextBox.Text.Trim(),
            string.IsNullOrWhiteSpace(DialogSubscriptionIdTextBox.Text) ? null : DialogSubscriptionIdTextBox.Text.Trim(),
            string.IsNullOrWhiteSpace(DialogClientIdTextBox.Text)       ? null : DialogClientIdTextBox.Text.Trim(),
            CredentialProtection.Protect(string.IsNullOrWhiteSpace(DialogClientSecretBox.Password) ? null : DialogClientSecretBox.Password));

        await PersistConnectionAsync(connection);
        HideConnectionDialog();
    }

    // -------------------------------------------------------------------------
    // Connection popup (settings popup > Connection tab)
    // -------------------------------------------------------------------------

    private void PopulateSettingsPopupConnection(AzureConnectionDto? connection)
    {
        if (connection is null) return;
        PopupConnectionEndpointTextBox.Text = connection.Endpoint;
        PopupAuthMethodComboBox.SelectedIndex = connection.AuthMethod switch
        {
            AuthMethod.ServicePrincipal => 1,
            AuthMethod.ManagedIdentity  => 2,
            _                           => 0
        };
        if (!string.IsNullOrWhiteSpace(connection.TenantId))
            PopupTenantIdTextBox.Text = connection.TenantId;
        if (!string.IsNullOrWhiteSpace(connection.ClientId))
            PopupClientIdTextBox.Text = connection.ClientId;
        var popupDecryptedSecret = CredentialProtection.Unprotect(connection.ClientSecret);
        if (!string.IsNullOrWhiteSpace(popupDecryptedSecret))
            PopupClientSecretBox.Password = popupDecryptedSecret;
        if (!string.IsNullOrWhiteSpace(connection.SubscriptionId))
            PopupSubscriptionIdTextBox.Text = connection.SubscriptionId;
    }

    private void PopupAuthMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PopupServicePrincipalPanel is null) return;
        PopupServicePrincipalPanel.Visibility =
            PopupAuthMethodComboBox.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void SaveConnectionPopupButton_Click(object sender, RoutedEventArgs e)
    {
        var endpoint = PopupConnectionEndpointTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            MessageBox.Show("Endpoint URL is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var authMethod = PopupAuthMethodComboBox.SelectedIndex switch
        {
            1 => AuthMethod.ServicePrincipal,
            2 => AuthMethod.ManagedIdentity,
            _ => AuthMethod.DefaultAzureCredential
        };

        if (authMethod == AuthMethod.ServicePrincipal &&
            (string.IsNullOrWhiteSpace(PopupTenantIdTextBox.Text) ||
             string.IsNullOrWhiteSpace(PopupClientIdTextBox.Text)))
        {
            MessageBox.Show("Tenant ID and Client ID are required for Service Principal.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var connection = new AzureConnectionDto(
            endpoint,
            authMethod,
            string.IsNullOrWhiteSpace(PopupTenantIdTextBox.Text)       ? null : PopupTenantIdTextBox.Text.Trim(),
            string.IsNullOrWhiteSpace(PopupSubscriptionIdTextBox.Text)  ? null : PopupSubscriptionIdTextBox.Text.Trim(),
            string.IsNullOrWhiteSpace(PopupClientIdTextBox.Text)        ? null : PopupClientIdTextBox.Text.Trim(),
            CredentialProtection.Protect(string.IsNullOrWhiteSpace(PopupClientSecretBox.Password) ? null : PopupClientSecretBox.Password));

        await PersistConnectionAsync(connection);
        CloseSettingsPopup();
        MessageBox.Show("Connection saved successfully.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ClearConnectionPopupButton_Click(object sender, RoutedEventArgs e)
    {
        PopupConnectionEndpointTextBox.Clear();
        PopupTenantIdTextBox.Clear();
        PopupClientIdTextBox.Clear();
        PopupClientSecretBox.Clear();
        PopupSubscriptionIdTextBox.Clear();
        PopupAuthMethodComboBox.SelectedIndex = 0;
    }

    // -------------------------------------------------------------------------
    // Settings tab (inline)
    // -------------------------------------------------------------------------

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var theme = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Light";
        MessageBox.Show($"Settings saved!\nTheme: {theme}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ThemeComboBox.SelectedIndex = 0;
        MessageBox.Show("Settings reset to default.", "Reset", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void SettingsTabDisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "Disconnect from Azure App Configuration?",
            "Disconnect", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;
        await PersistConnectionAsync(null);
    }

    // -------------------------------------------------------------------------
    // Connection persistence helpers
    // -------------------------------------------------------------------------

    private async Task PersistConnectionAsync(AzureConnectionDto? connection)
    {
        var existing = await _settingsService.GetSettingsAsync();
        var current = existing.IsSuccess && existing.Data is not null
            ? existing.Data
            : SettingsService.DefaultSettings;

        var updated = current with { AzureConnection = connection };
        await _settingsService.SaveSettingsAsync(updated);

        _currentConnection = connection;
        UpdateConnectionStatus(connection);
        PopulateSettingsPopupConnection(connection);
    }

    private void UpdateConnectionStatus(AzureConnectionDto? connection)
    {
        _shell.UpdateConnection(connection);

        ConnectionStatusTextBlock.Text = _shell.ConnectionDisplay;
        SettingsTabConnectionTextBlock.Text = _shell.ConnectionDisplay;

        ConnectionStatusDot.Foreground = connection is not null
            ? new SolidColorBrush(Color.FromRgb(0x13, 0xa1, 0x0e))
            : new SolidColorBrush(Color.FromRgb(0xcc, 0x44, 0x44));
    }

    // -------------------------------------------------------------------------
    // Status bar
    // -------------------------------------------------------------------------

    private void ZoomSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (ZoomLevelTextBlock is null) return;
        _shell.ZoomLevel = ZoomSlider.Value;
        ZoomLevelTextBlock.Text = $"{ZoomSlider.Value:0}%";
    }

    public void UpdateConfigCount(int count)
    {
        _shell.ConfigSettingsCount = count;
        if (ConfigCountTextBlock is not null)
            ConfigCountTextBlock.Text = count > 0 ? $"  |  Settings: {count}" : string.Empty;
    }

    // -------------------------------------------------------------------------
    // Comparison tab
    // -------------------------------------------------------------------------

    private void CompareButton_Click(object sender, RoutedEventArgs e)
    {
        if (SourceConfigTextBox.Text.Length == 0 || TargetConfigTextBox.Text.Length == 0)
        {
            ComparisonResultsTextBlock.Text = "Please enter configuration in both Source and Target fields.";
            ComparisonResultsTextBlock.Foreground = Brushes.Red;
            return;
        }

        var sourceLines = SourceConfigTextBox.Text.Split(Environment.NewLine);
        var targetLines = TargetConfigTextBox.Text.Split(Environment.NewLine);

        var sb = new StringBuilder();
        sb.AppendLine("=== Comparison Results ===");
        sb.AppendLine($"Source lines: {sourceLines.Length}");
        sb.AppendLine($"Target lines: {targetLines.Length}");
        sb.AppendLine();

        int diffCount = 0;
        int maxLines = Math.Max(sourceLines.Length, targetLines.Length);
        for (int i = 0; i < maxLines; i++)
        {
            var src = i < sourceLines.Length ? sourceLines[i] : "[MISSING]";
            var tgt = i < targetLines.Length ? targetLines[i] : "[MISSING]";
            if (src != tgt)
            {
                diffCount++;
                sb.AppendLine($"Line {i + 1}: DIFFERENT");
                sb.AppendLine($"  Source: {src}");
                sb.AppendLine($"  Target: {tgt}");
            }
        }
        sb.AppendLine();
        sb.AppendLine($"Total Differences: {diffCount}");

        ComparisonResultsTextBlock.Text = sb.ToString();
        ComparisonResultsTextBlock.Foreground = new SolidColorBrush(Colors.Black);
        UpdateConfigCount(sourceLines.Length);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        SourceConfigTextBox.Clear();
        TargetConfigTextBox.Clear();
        ComparisonResultsTextBlock.Text = "Comparison results will appear here...";
        ComparisonResultsTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(119, 119, 119));
        UpdateConfigCount(0);
    }

    // -------------------------------------------------------------------------
    // Edit Config tab
    // -------------------------------------------------------------------------

    private async void SaveConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (EditConfigEndpointTextBox.Text.Length == 0 ||
            ConfigKeyTextBox.Text.Length == 0 ||
            ConfigValueTextBox.Text.Length == 0)
        {
            SetStatus(EditConfigStatusTextBlock, "Endpoint, Key, and Value are all required.", StatusColor.Error);
            return;
        }

        try
        {
            SetStatus(EditConfigStatusTextBlock, "Saving configuration...", StatusColor.Info);
            var config = new ConfigurationItemDto
            {
                Key   = ConfigKeyTextBox.Text,
                Value = ConfigValueTextBox.Text,
                Label = ConfigLabelTextBox.Text
            };

            var response = await _appConfigService.UpdateConfigurationAsync(EditConfigEndpointTextBox.Text, config);
            if (response.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Configuration Saved Successfully!");
                sb.AppendLine();
                sb.AppendLine($"Key: {config.Key}");
                sb.AppendLine($"Value: {config.Value}");
                if (!string.IsNullOrEmpty(config.Label)) sb.AppendLine($"Label: {config.Label}");
                SetStatus(EditConfigStatusTextBlock, sb.ToString(), StatusColor.Success);
                ConfigKeyTextBox.Clear();
                ConfigValueTextBox.Clear();
                ConfigLabelTextBox.Clear();
            }
            else
            {
                SetStatus(EditConfigStatusTextBlock, $"Error: {response.ErrorMessage}", StatusColor.Error);
            }
        }
        catch (Exception ex)
        {
            SetStatus(EditConfigStatusTextBlock, $"Exception: {ex.Message}", StatusColor.Error);
        }
    }

    private void ClearEditConfigButton_Click(object sender, RoutedEventArgs e)
    {
        EditConfigEndpointTextBox.Clear();
        ConfigKeyTextBox.Clear();
        ConfigValueTextBox.Clear();
        ConfigLabelTextBox.Clear();
        SetStatus(EditConfigStatusTextBlock, "Status messages will appear here...", StatusColor.Neutral);
    }

    // -------------------------------------------------------------------------
    // Find and Replace tab
    // -------------------------------------------------------------------------

    private async void ExecuteFindReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (FindReplaceEndpointTextBox.Text.Length == 0 ||
            FindValueTextBox.Text.Length == 0 ||
            ReplaceValueTextBox.Text.Length == 0)
        {
            SetStatus(FindReplaceStatusTextBlock, "Endpoint, Find Value, and Replace Value are all required.", StatusColor.Error);
            return;
        }

        try
        {
            SetStatus(FindReplaceStatusTextBlock, "Processing find and replace...", StatusColor.Info);
            var response = await _appConfigService.FindAndReplaceAsync(
                FindReplaceEndpointTextBox.Text,
                FindValueTextBox.Text,
                ReplaceValueTextBox.Text);

            if (response.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Find and Replace Completed!");
                sb.AppendLine();
                sb.AppendLine($"Find: '{FindValueTextBox.Text}'");
                sb.AppendLine($"Replace With: '{ReplaceValueTextBox.Text}'");
                sb.AppendLine($"Total Replacements: {response.ReplacementCount}");
                SetStatus(FindReplaceStatusTextBlock, sb.ToString(), StatusColor.Success);
                AffectedKeysTextBox.Text = response.AffectedKeys.Count > 0
                    ? string.Join(Environment.NewLine, response.AffectedKeys)
                    : "(No affected keys)";
            }
            else
            {
                SetStatus(FindReplaceStatusTextBlock, $"Error: {response.ErrorMessage}", StatusColor.Error);
                AffectedKeysTextBox.Clear();
            }
        }
        catch (Exception ex)
        {
            SetStatus(FindReplaceStatusTextBlock, $"Exception: {ex.Message}", StatusColor.Error);
        }
    }

    private void ClearFindReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        FindReplaceEndpointTextBox.Clear();
        FindValueTextBox.Clear();
        ReplaceValueTextBox.Clear();
        AffectedKeysTextBox.Clear();
        SetStatus(FindReplaceStatusTextBlock, "Status messages will appear here...", StatusColor.Neutral);
    }

    // -------------------------------------------------------------------------
    // Copy Settings tab
    // -------------------------------------------------------------------------

    private async void ExecuteCopySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (CopySourceEndpointTextBox.Text.Length == 0 || CopyTargetEndpointTextBox.Text.Length == 0)
        {
            SetStatus(CopySettingsStatusTextBlock, "Both Source and Target endpoints are required.", StatusColor.Error);
            return;
        }
        if (CopySourceEndpointTextBox.Text == CopyTargetEndpointTextBox.Text)
        {
            SetStatus(CopySettingsStatusTextBlock, "Source and Target endpoints must be different.", StatusColor.Error);
            return;
        }

        try
        {
            SetStatus(CopySettingsStatusTextBlock, "Copying settings...", StatusColor.Info);
            var response = await _appConfigService.CopySettingsAsync(
                CopySourceEndpointTextBox.Text,
                CopyTargetEndpointTextBox.Text);

            if (response.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Copy Settings Completed!");
                sb.AppendLine();
                sb.AppendLine($"Total Settings Copied: {response.CopiedCount}");
                sb.AppendLine($"Source: {CopySourceEndpointTextBox.Text}");
                sb.AppendLine($"Target: {CopyTargetEndpointTextBox.Text}");
                SetStatus(CopySettingsStatusTextBlock, sb.ToString(), StatusColor.Success);

                var displayKeys = response.CopiedKeys.Take(20).ToList();
                var keysText = displayKeys.Count > 0
                    ? string.Join(Environment.NewLine, displayKeys)
                    : "(No keys)";
                if (response.CopiedKeys.Count > 20)
                    keysText += $"\n... and {response.CopiedKeys.Count - 20} more";
                CopiedKeysTextBox.Text = keysText;

                UpdateConfigCount(response.CopiedCount);
            }
            else
            {
                SetStatus(CopySettingsStatusTextBlock, $"Error: {response.ErrorMessage}", StatusColor.Error);
                CopiedKeysTextBox.Clear();
            }
        }
        catch (Exception ex)
        {
            SetStatus(CopySettingsStatusTextBlock, $"Exception: {ex.Message}", StatusColor.Error);
        }
    }

    private void ClearCopySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        CopySourceEndpointTextBox.Clear();
        CopyTargetEndpointTextBox.Clear();
        CopiedKeysTextBox.Clear();
        SetStatus(CopySettingsStatusTextBlock, "Status messages will appear here...", StatusColor.Neutral);
    }

    // -------------------------------------------------------------------------
    // Status helper
    // -------------------------------------------------------------------------

    private enum StatusColor { Success, Error, Info, Neutral }

    private static void SetStatus(TextBlock tb, string message, StatusColor color)
    {
        tb.Text = message;
        tb.Foreground = color switch
        {
            StatusColor.Success => Brushes.Green,
            StatusColor.Error   => Brushes.Red,
            StatusColor.Info    => Brushes.Blue,
            _                   => new SolidColorBrush(Color.FromRgb(119, 119, 119))
        };
    }
}
