namespace ConfigCompare.AzureIdentity.Contract;

/// <summary>
/// Represents an Azure access token and its metadata.
/// </summary>
public record TokenInfo(string AccessToken, DateTimeOffset ExpiresOn, string[] Scopes);
