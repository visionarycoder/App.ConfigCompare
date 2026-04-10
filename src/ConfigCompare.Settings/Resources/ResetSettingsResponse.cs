namespace ConfigCompare.Settings.Resources;

/// <summary>
/// Response from ResetSettingsAsync operation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="ErrorMessage">Error message if the operation failed; otherwise, null.</param>
public record ResetSettingsResponse(bool IsSuccess, string? ErrorMessage);
