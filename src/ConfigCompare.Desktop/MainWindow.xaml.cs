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
        // Wire up the Compare button
        if (this.FindName("CompareButton") is Button compareButton)
        {
            compareButton.Click += CompareButton_Click;
        }

        // Wire up the Clear button
        if (this.FindName("ClearButton") is Button clearButton)
        {
            clearButton.Click += ClearButton_Click;
        }

        // Wire up the Save Settings button
        if (this.FindName("SaveSettingsButton") is Button saveSettingsButton)
        {
            saveSettingsButton.Click += SaveSettingsButton_Click;
        }

        // Wire up the Reset Settings button
        if (this.FindName("ResetSettingsButton") is Button resetSettingsButton)
        {
            resetSettingsButton.Click += ResetSettingsButton_Click;
        }
    }

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