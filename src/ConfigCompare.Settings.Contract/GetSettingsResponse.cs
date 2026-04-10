namespace ConfigCompare.Settings.Contract;

/// <summary>
/// Response from GetSettingsAsync operation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="ErrorMessage">Error message if the operation failed; otherwise, null.</param>
/// <param name="Data">The retrieved settings; null if an error occurred.</param>
public record GetSettingsResponse(bool IsSuccess, string? ErrorMessage, UserSettingsDto? Data);
