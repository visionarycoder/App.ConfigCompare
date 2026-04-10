namespace ConfigCompare.Session.Resources;

/// <summary>
/// Data transfer object representing a snapshot of the application state at a point in time.
/// </summary>
/// <param name="SavedAt">The timestamp when the snapshot was saved.</param>
/// <param name="OpenModules">List of module names that were open.</param>
/// <param name="ActiveModule">The name of the currently active module, if any.</param>
/// <param name="SelectedResourceGroupNames">List of selected resource group names.</param>
/// <param name="WindowState">Application window state (e.g., size, position) as key-value pairs.</param>
/// <param name="ModuleState">Freeform module-specific state (e.g., scroll positions, column widths) as key-value pairs.</param>
public record SessionSnapshotDto(
    DateTimeOffset SavedAt,
    IReadOnlyList<string> OpenModules,
    string? ActiveModule,
    IReadOnlyList<string> SelectedResourceGroupNames,
    IReadOnlyDictionary<string, string> WindowState,
    IReadOnlyDictionary<string, string> ModuleState);
