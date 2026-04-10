#nullable enable

namespace ConfigCompare.AppConfig.Resources;

/// <summary>
/// Response for getting all configuration items
/// </summary>
public class GetConfigurationsResponse
{
    public List<ConfigurationItemDto> Configurations { get; set; } = [];
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response for updating a configuration item
/// </summary>
public class UpdateConfigurationResponse
{
    public ConfigurationItemDto? UpdatedConfiguration { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response for finding and replacing values
/// </summary>
public class FindReplaceResponse
{
    public int ReplacementCount { get; set; }
    public List<string> AffectedKeys { get; set; } = [];
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response for copying settings between instances
/// </summary>
public class CopySettingsResponse
{
    public int CopiedCount { get; set; }
    public List<string> CopiedKeys { get; set; } = [];
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
