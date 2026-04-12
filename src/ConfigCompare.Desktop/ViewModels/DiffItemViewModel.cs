#nullable enable

namespace ConfigCompare.Desktop.ViewModels;

public sealed class DiffItemViewModel
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}
