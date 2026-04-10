namespace ConfigCompare.AzureIdentity.Contract;

/// <summary>
/// Response for a token acquisition request.
/// </summary>
public record GetTokenResponse(bool IsSuccess, string? ErrorMessage, TokenInfo? Data);
