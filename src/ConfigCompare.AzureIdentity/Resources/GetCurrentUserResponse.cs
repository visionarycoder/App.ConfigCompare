namespace ConfigCompare.AzureIdentity.Resources;

/// <summary>
/// Response for a current user information request.
/// </summary>
public record GetCurrentUserResponse(bool IsSuccess, string? ErrorMessage, UserInfo? Data);
