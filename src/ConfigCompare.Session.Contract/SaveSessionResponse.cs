namespace ConfigCompare.Session.Contract;

/// <summary>
/// Response from SaveSessionAsync operation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="ErrorMessage">Error message if the operation failed; otherwise, null.</param>
public record SaveSessionResponse(bool IsSuccess, string? ErrorMessage);
