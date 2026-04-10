namespace ConfigCompare.ResourceGroup.Resources;

/// <summary>
/// Response from GetServicesInGroupAsync containing discovered services or an error.
/// </summary>
public record GetServicesInGroupResponse(
    bool IsSuccess,
    string? ErrorMessage,
    ResourceGroupServicesDto? Data);
