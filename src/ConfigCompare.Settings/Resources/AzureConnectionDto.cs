namespace ConfigCompare.Settings.Resources;

/// <summary>
/// Represents the saved Azure App Configuration connection details.
/// </summary>
/// <param name="Endpoint">Full HTTPS endpoint URL of the Azure App Configuration store (e.g. https://mystore.azconfig.io).</param>
/// <param name="AuthMethod">Authentication method to use when connecting.</param>
/// <param name="TenantId">Azure AD tenant ID. Required for <see cref="AuthMethod.ServicePrincipal"/>.</param>
/// <param name="SubscriptionId">Azure subscription ID (optional; used for resource group listing).</param>
/// <param name="ClientId">Service principal or managed identity client ID.</param>
/// <param name="ClientSecret">Service principal client secret stored as a DPAPI-encrypted Base64 string. Use <c>CredentialProtection.Protect/Unprotect</c> (Desktop layer) for encryption/decryption.</param>
public record AzureConnectionDto(
    string Endpoint,
    AuthMethod AuthMethod,
    string? TenantId,
    string? SubscriptionId,
    string? ClientId,
    string? ClientSecret);
