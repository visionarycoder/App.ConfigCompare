namespace ConfigCompare.AzureIdentity.Resources;

/// <summary>
/// Represents the currently authenticated user's identity information.
/// </summary>
public record UserInfo(string DisplayName, string UserPrincipalName, string TenantId, string ObjectId);
