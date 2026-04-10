namespace ConfigCompare.Settings.Contract;

/// <summary>
/// Response from SaveSettingsAsync operation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="ErrorMessage">Error message if the operation failed; otherwise, null.</param>
public record SaveSettingsResponse(bool IsSuccess, string? ErrorMessage);
