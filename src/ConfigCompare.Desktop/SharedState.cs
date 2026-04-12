using ConfigCompare.Desktop.ViewModels;

namespace ConfigCompare.Desktop;

/// <summary>Application-wide shared state for cross-page communication.</summary>
public static class SharedState
{
    /// <summary>The shared ConfigurationsViewModel instance used across pages.</summary>
    public static ConfigurationsViewModel Configurations { get; } = new();
}
