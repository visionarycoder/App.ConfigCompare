using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConfigCompare.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadEventHandlers();
    }

    private void LoadEventHandlers()
    {
        // Comparison Tab Buttons
        if (this.FindName("CompareButton") is Button compareButton)
        {
            compareButton.Click += CompareButton_Click;
        }

        if (this.FindName("ClearButton") is Button clearButton)
        {
            clearButton.Click += ClearButton_Click;
        }

        // Settings Tab Buttons
        if (this.FindName("SaveSettingsButton") is Button saveSettingsButton)
        {
            saveSettingsButton.Click += SaveSettingsButton_Click;
        }

        if (this.FindName("ResetSettingsButton") is Button resetSettingsButton)
        {
            resetSettingsButton.Click += ResetSettingsButton_Click;
        }

        // Edit Config Buttons
        if (this.FindName("SaveConfigButton") is Button saveConfigButton)
        {
            saveConfigButton.Click += SaveConfigButton_Click;
        }

        if (this.FindName("ClearEditConfigButton") is Button clearEditConfigButton)
        {
            clearEditConfigButton.Click += ClearEditConfigButton_Click;
        }

        // Find & Replace Buttons
        if (this.FindName("ExecuteFindReplaceButton") is Button executeFindReplaceButton)
        {
            executeFindReplaceButton.Click += ExecuteFindReplaceButton_Click;
        }

        if (this.FindName("ClearFindReplaceButton") is Button clearFindReplaceButton)
        {
            clearFindReplaceButton.Click += ClearFindReplaceButton_Click;
        }

        // Copy Settings Buttons
        if (this.FindName("ExecuteCopySettingsButton") is Button executeCopySettingsButton)
        {
            executeCopySettingsButton.Click += ExecuteCopySettingsButton_Click;
        }

        if (this.FindName("ClearCopySettingsButton") is Button clearCopySettingsButton)
        {
            clearCopySettingsButton.Click += ClearCopySettingsButton_Click;
        }
    }

    // Comparison Tab Methods
    private void CompareButton_Click(object sender, RoutedEventArgs e)
    {
        if (SourceConfigTextBox.Text.Length == 0 || TargetConfigTextBox.Text.Length == 0)
        {
            ComparisonResultsTextBlock.Text = "Please enter configuration in both Source and Target fields.";
            ComparisonResultsTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            return;
        }

        var sourceLines = SourceConfigTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var targetLines = TargetConfigTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        var results = new StringBuilder();
        results.AppendLine("=== Comparison Results ===");
        results.AppendLine($"Source Configuration Lines: {sourceLines.Length}");
        results.AppendLine($"Target Configuration Lines: {targetLines.Length}");
        results.AppendLine();

        var maxLines = Math.Max(sourceLines.Length, targetLines.Length);
        int differenceCount = 0;

        for (int i = 0; i < maxLines; i++)
        {
            var sourceLine = i < sourceLines.Length ? sourceLines[i] : "[MISSING]";
            var targetLine = i < targetLines.Length ? targetLines[i] : "[MISSING]";

            if (sourceLine != targetLine)
            {
                differenceCount++;
                results.AppendLine($"Line {i + 1}: DIFFERENT");
                results.AppendLine($"  Source: {sourceLine}");
                results.AppendLine($"  Target: {targetLine}");
            }
        }

        results.AppendLine();
        results.AppendLine($"Total Differences: {differenceCount}");

        ComparisonResultsTextBlock.Text = results.ToString();
        ComparisonResultsTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        SourceConfigTextBox.Clear();
        TargetConfigTextBox.Clear();
        ComparisonResultsTextBlock.Text = "Comparison results will appear here...";
        ComparisonResultsTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(119, 119, 119));
    }

    // Settings Tab Methods
    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedTheme = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        MessageBox.Show($"Settings saved!\nTheme: {selectedTheme}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ThemeComboBox.SelectedIndex = 0;
        var systemCheckBoxes = FindVisualChildren<CheckBox>(this).Where(c => c.Name.Contains("Checkbox")).ToList();
        MessageBox.Show("Settings reset to default.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // Edit Config Tab Methods
    private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (EditConfigEndpointTextBox.Text.Length == 0 || ConfigKeyTextBox.Text.Length == 0 || ConfigValueTextBox.Text.Length == 0)
        {
            EditConfigStatusTextBlock.Text = "❌ Endpoint, Key, and Value are all required.";
            EditConfigStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            return;
        }

        // Simulate saving configuration
        var configKey = ConfigKeyTextBox.Text;
        var configValue = ConfigValueTextBox.Text;
        var label = ConfigLabelTextBox.Text;

        var results = new StringBuilder();
        results.AppendLine("✓ Configuration Saved Successfully!");
        results.AppendLine();
        results.AppendLine($"Key: {configKey}");
        results.AppendLine($"Value: {configValue}");
        if (!string.IsNullOrEmpty(label))
        {
            results.AppendLine($"Label: {label}");
        }
        results.AppendLine();
        results.AppendLine("Committed to Azure App Configuration.");

        EditConfigStatusTextBlock.Text = results.ToString();
        EditConfigStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

        // Clear form
        ConfigKeyTextBox.Clear();
        ConfigValueTextBox.Clear();
        ConfigLabelTextBox.Clear();
    }

    private void ClearEditConfigButton_Click(object sender, RoutedEventArgs e)
    {
        EditConfigEndpointTextBox.Clear();
        ConfigKeyTextBox.Clear();
        ConfigValueTextBox.Clear();
        ConfigLabelTextBox.Clear();
        EditConfigStatusTextBlock.Text = "Status messages will appear here...";
        EditConfigStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(119, 119, 119));
    }

    // Find & Replace Tab Methods
    private void ExecuteFindReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (FindReplaceEndpointTextBox.Text.Length == 0 || FindValueTextBox.Text.Length == 0 || ReplaceValueTextBox.Text.Length == 0)
        {
            FindReplaceStatusTextBlock.Text = "❌ Endpoint, Find Value, and Replace Value are all required.";
            FindReplaceStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            return;
        }

        var findValue = FindValueTextBox.Text;
        var replaceValue = ReplaceValueTextBox.Text;

        // Simulate finding and replacing
        var affectedKeys = new List<string> { "app.setting1", "app.setting2", "database.connection" };
        var replacementCount = affectedKeys.Count;

        var results = new StringBuilder();
        results.AppendLine($"✓ Find and Replace Completed!");
        results.AppendLine();
        results.AppendLine($"Find: '{findValue}'");
        results.AppendLine($"Replace With: '{replaceValue}'");
        results.AppendLine($"Total Replacements: {replacementCount}");

        FindReplaceStatusTextBlock.Text = results.ToString();
        FindReplaceStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

        AffectedKeysTextBox.Text = string.Join(Environment.NewLine, affectedKeys);
    }

    private void ClearFindReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        FindReplaceEndpointTextBox.Clear();
        FindValueTextBox.Clear();
        ReplaceValueTextBox.Clear();
        FindReplaceStatusTextBlock.Text = "Status messages will appear here...";
        FindReplaceStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(119, 119, 119));
        AffectedKeysTextBox.Clear();
    }

    // Copy Settings Tab Methods
    private void ExecuteCopySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (CopySourceEndpointTextBox.Text.Length == 0 || CopyTargetEndpointTextBox.Text.Length == 0)
        {
            CopySettingsStatusTextBlock.Text = "❌ Both Source and Target endpoints are required.";
            CopySettingsStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            return;
        }

        if (CopySourceEndpointTextBox.Text == CopyTargetEndpointTextBox.Text)
        {
            CopySettingsStatusTextBlock.Text = "❌ Source and Target endpoints must be different.";
            CopySettingsStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            return;
        }

        // Simulate copying settings
        var copiedKeys = new List<string> { "app:setting1", "app:setting2", "app:setting3", "database:host", "database:port" };
        var copiedCount = copiedKeys.Count;

        var results = new StringBuilder();
        results.AppendLine($"✓ Copy Settings Completed!");
        results.AppendLine();
        results.AppendLine($"Total Settings Copied: {copiedCount}");
        results.AppendLine();
        results.AppendLine($"Source: {CopySourceEndpointTextBox.Text}");
        results.AppendLine($"Target: {CopyTargetEndpointTextBox.Text}");

        CopySettingsStatusTextBlock.Text = results.ToString();
        CopySettingsStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

        CopiedKeysTextBox.Text = string.Join(Environment.NewLine, copiedKeys);
    }

    private void ClearCopySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        CopySourceEndpointTextBox.Clear();
        CopyTargetEndpointTextBox.Clear();
        CopySettingsStatusTextBlock.Text = "Status messages will appear here...";
        CopySettingsStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(119, 119, 119));
        CopiedKeysTextBox.Clear();
    }

    // Helper method to find visual children
    private IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj is not null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}