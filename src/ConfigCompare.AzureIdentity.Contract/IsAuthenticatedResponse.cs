namespace ConfigCompare.AzureIdentity.Contract;

/// <summary>
/// Response for an authentication status check.
/// </summary>
public record IsAuthenticatedResponse(bool IsSuccess, string? ErrorMessage, bool IsAuthenticated);
