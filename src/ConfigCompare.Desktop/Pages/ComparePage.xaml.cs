using System.Linq;
using ConfigCompare.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ConfigCompare.Desktop.Pages;

public sealed partial class ComparePage : Page
{
    public ComparePage()
    {
        this.InitializeComponent();
        Loaded += ComparePage_Loaded;
    }

    private void ComparePage_Loaded(object sender, RoutedEventArgs e)
    {
        var configs = SharedState.Configurations.Configurations;
        LeftComboBox.ItemsSource = configs;
        RightComboBox.ItemsSource = configs;
        LeftComboBox.DisplayMemberPath = "DisplayName";
        RightComboBox.DisplayMemberPath = "DisplayName";
    }

    private void Compare_Click(object sender, RoutedEventArgs e)
    {
        var left = LeftComboBox.SelectedItem as ConfigurationTabViewModel;
        var right = RightComboBox.SelectedItem as ConfigurationTabViewModel;

        if (left is null || right is null)
            return;

        var leftKeys = left.Items.ToDictionary(i => i.Key, i => i);
        var rightKeys = right.Items.ToDictionary(i => i.Key, i => i);

        LeftOnlyList.ItemsSource  = leftKeys.Keys.Except(rightKeys.Keys).Select(k => ToDiffItem(k, leftKeys)).ToList();
        RightOnlyList.ItemsSource = rightKeys.Keys.Except(leftKeys.Keys).Select(k => ToDiffItem(k, rightKeys)).ToList();
        MatchingList.ItemsSource  = leftKeys.Keys.Intersect(rightKeys.Keys).Select(k => ToDiffItem(k, leftKeys)).ToList();
    }

    private static DiffItemViewModel ToDiffItem(
        string key,
        System.Collections.Generic.Dictionary<string, ConfigCompare.AppConfig.Resources.ConfigurationItemDto> source)
        => new() { Key = key, Value = source[key].Value, Label = source[key].Label ?? "" };
}
