#nullable enable

using Azure;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using CommunityToolkit.Diagnostics;
using ConfigCompare.AppConfig.Resources;

namespace ConfigCompare.AppConfig;

/// <summary>
/// Implementation of App Configuration service using Azure SDK
/// </summary>
public class AppConfigService : IAppConfigService
{
    public async Task<GetConfigurationsResponse> GetConfigurationsAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(endpoint, nameof(endpoint));

        try
        {
            var client = new ConfigurationClient(new Uri(endpoint), new DefaultAzureCredential());
            var configurations = new List<ConfigurationItemDto>();

            var selector = new SettingSelector();
            await foreach (var setting in client.GetConfigurationSettingsAsync(selector, cancellationToken))
            {
                configurations.Add(new ConfigurationItemDto
                {
                    Key = setting.Key,
                    Value = setting.Value ?? string.Empty,
                    Label = setting.Label,
                    Tags = setting.Tags?.ToDictionary(t => t.Key, t => t.Value) ?? [],
                    ContentType = setting.ContentType ?? string.Empty
                });
            }

            return new GetConfigurationsResponse
            {
                Configurations = configurations,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new GetConfigurationsResponse
            {
                Success = false,
                ErrorMessage = $"Failed to retrieve configurations: {ex.Message}"
            };
        }
    }

    public async Task<UpdateConfigurationResponse> UpdateConfigurationAsync(
        string endpoint,
        ConfigurationItemDto configuration,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(endpoint, nameof(endpoint));
        Guard.IsNotNull(configuration, nameof(configuration));

        try
        {
            var client = new ConfigurationClient(new Uri(endpoint), new DefaultAzureCredential());

            var setting = new ConfigurationSetting(configuration.Key, configuration.Value)
            {
                Label = configuration.Label,
                ContentType = configuration.ContentType
            };

            if (configuration.Tags.Count > 0)
            {
                foreach (var tag in configuration.Tags)
                {
                    setting.Tags[tag.Key] = tag.Value;
                }
            }

            var response = await client.SetConfigurationSettingAsync(setting, cancellationToken: cancellationToken);

            return new UpdateConfigurationResponse
            {
                UpdatedConfiguration = new ConfigurationItemDto
                {
                    Key = response.Value.Key,
                    Value = response.Value.Value ?? string.Empty,
                    Label = response.Value.Label,
                    Tags = response.Value.Tags?.ToDictionary(t => t.Key, t => t.Value) ?? [],
                    ContentType = response.Value.ContentType ?? string.Empty
                },
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new UpdateConfigurationResponse
            {
                Success = false,
                ErrorMessage = $"Failed to update configuration: {ex.Message}"
            };
        }
    }

    public async Task<UpdateConfigurationResponse> DeleteConfigurationAsync(
        string endpoint,
        string key,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(endpoint, nameof(endpoint));
        Guard.IsNotNull(key, nameof(key));

        try
        {
            var client = new ConfigurationClient(new Uri(endpoint), new DefaultAzureCredential());
            await client.DeleteConfigurationSettingAsync(key, cancellationToken: cancellationToken);

            return new UpdateConfigurationResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new UpdateConfigurationResponse
            {
                Success = false,
                ErrorMessage = $"Failed to delete configuration: {ex.Message}"
            };
        }
    }

    public async Task<FindReplaceResponse> FindAndReplaceAsync(
        string endpoint,
        string findValue,
        string replaceValue,
        List<string>? keysToUpdate = null,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(endpoint, nameof(endpoint));
        Guard.IsNotNull(findValue, nameof(findValue));
        Guard.IsNotNull(replaceValue, nameof(replaceValue));

        try
        {
            var client = new ConfigurationClient(new Uri(endpoint), new DefaultAzureCredential());
            var affectedKeys = new List<string>();
            int replacementCount = 0;

            // Retrieve all configurations
            var configurations = new List<ConfigurationSetting>();
            var selector = new SettingSelector();
            await foreach (var setting in client.GetConfigurationSettingsAsync(selector, cancellationToken))
            {
                // Filter by keys if specified
                if (keysToUpdate != null && !keysToUpdate.Contains(setting.Key))
                {
                    continue;
                }

                configurations.Add(setting);
            }

            // Find and replace
            foreach (var config in configurations)
            {
                if (config.Value?.Contains(findValue) == true)
                {
                    var newValue = config.Value.Replace(findValue, replaceValue);
                    config.Value = newValue;

                    var updated = await client.SetConfigurationSettingAsync(config, cancellationToken: cancellationToken);
                    affectedKeys.Add(config.Key);
                    replacementCount++;
                }
            }

            return new FindReplaceResponse
            {
                ReplacementCount = replacementCount,
                AffectedKeys = affectedKeys,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new FindReplaceResponse
            {
                Success = false,
                ErrorMessage = $"Failed to perform find and replace: {ex.Message}"
            };
        }
    }

    public async Task<CopySettingsResponse> CopySettingsAsync(
        string sourceEndpoint,
        string targetEndpoint,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(sourceEndpoint, nameof(sourceEndpoint));
        Guard.IsNotNull(targetEndpoint, nameof(targetEndpoint));

        try
        {
            var sourceClient = new ConfigurationClient(new Uri(sourceEndpoint), new DefaultAzureCredential());
            var targetClient = new ConfigurationClient(new Uri(targetEndpoint), new DefaultAzureCredential());

            var copiedKeys = new List<string>();
            int copiedCount = 0;

            // Retrieve all configurations from source
            var selector = new SettingSelector();
            await foreach (var setting in sourceClient.GetConfigurationSettingsAsync(selector, cancellationToken))
            {
                // Copy to target
                var newSetting = new ConfigurationSetting(setting.Key, setting.Value)
                {
                    Label = setting.Label,
                    ContentType = setting.ContentType
                };

                if (setting.Tags?.Count > 0)
                {
                    foreach (var tag in setting.Tags)
                    {
                        newSetting.Tags[tag.Key] = tag.Value;
                    }
                }

                await targetClient.SetConfigurationSettingAsync(newSetting, cancellationToken: cancellationToken);
                copiedKeys.Add(setting.Key);
                copiedCount++;
            }

            return new CopySettingsResponse
            {
                CopiedCount = copiedCount,
                CopiedKeys = copiedKeys,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new CopySettingsResponse
            {
                Success = false,
                ErrorMessage = $"Failed to copy settings: {ex.Message}"
            };
        }
    }
}
