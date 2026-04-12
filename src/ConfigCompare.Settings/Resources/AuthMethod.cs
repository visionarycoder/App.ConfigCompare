namespace ConfigCompare.Settings.Resources;

/// <summary>
/// Represents the Azure authentication method to use for connecting to App Configuration.
/// </summary>
public enum AuthMethod
{
    /// <summary>Uses DefaultAzureCredential (Azure CLI, environment variables, managed identity chain).</summary>
    DefaultAzureCredential,

    /// <summary>Uses a Service Principal with tenant ID, client ID, and client secret.</summary>
    ServicePrincipal,

    /// <summary>Uses a system-assigned or user-assigned managed identity.</summary>
    ManagedIdentity
}
