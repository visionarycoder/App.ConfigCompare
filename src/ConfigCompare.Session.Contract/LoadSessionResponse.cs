namespace ConfigCompare.Session.Contract;

/// <summary>
/// Response from LoadSessionAsync operation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation succeeded.</param>
/// <param name="ErrorMessage">Error message if the operation failed; otherwise, null.</param>
/// <param name="Data">The loaded session snapshot; null if no prior session exists (not an error).</param>
public record LoadSessionResponse(bool IsSuccess, string? ErrorMessage, SessionSnapshotDto? Data);
