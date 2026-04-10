#nullable enable

namespace ConfigCompare.AppConfig.Resources;

/// <summary>
/// Represents a configuration key-value pair from App Configuration
/// </summary>
public class ConfigurationItemDto
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Label { get; set; }
    public Dictionary<string, string> Tags { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
}
