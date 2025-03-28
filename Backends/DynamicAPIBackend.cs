using Newtonsoft.Json.Linq;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using Hartsy.Extensions.APIBackends.Models;
using FreneticUtilities.FreneticDataSyntax;
using System.Net.Http;
using System.Net.Http.Headers;
using SwarmUI.Accounts;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SwarmUI.Backends;

namespace Hartsy.Extensions.APIBackends.Backends
{
    /// <summary>A dynamic backend that supports multiple API providers for image generation.</summary>
    public class DynamicAPIBackend : APIAbstractBackend
    {
        /// <summary>Settings for the dynamic API backend.</summary>
        public class DynamicAPISettings : AutoConfiguration
        {
            [ManualSettingsOptions(Impl = null, Vals = ["BFL (Flux)", "Ideogram", "OpenAI (DALL-E)"])]
            [ConfigComment("Choose the backend API provider to use for image generation. This will add the relevant models to your model list.")]
            public string SelectedProvider = "BFL (Flux)";

            [ConfigComment("Custom Base URL (optional) This value will override the hardcoded base URL.")]
            public string CustomBaseUrl = "";
        }

        /// <summary>Current backend settings</summary>
        public DynamicAPISettings Settings => SettingsRaw as DynamicAPISettings;

        /// <summary>Access to the API provider service</summary>
        private static readonly APIProviderInit _providerInit = new();

        /// <summary>Gets the active provider metadata</summary>
        protected override APIProviderMetadata ActiveProvider => _providerInit.Providers[NormalizeProviderKey(Settings.SelectedProvider)];

        /// <summary>Gets the custom base URL if specified</summary>
        protected override string CustomBaseUrl => Settings.CustomBaseUrl;

        /// <summary>Mapping of provider keys to feature flags</summary>
        private readonly Dictionary<string, string> _providerFeatures = new()
        {
            ["bfl"] = "bfl-api",
            ["openai"] = "openai-api",
            ["ideogram"] = "ideogram-api"
        };

        /// <summary>Gets the supported features for this backend</summary>
        public override IEnumerable<string> SupportedFeatures => SupportedFeatureSet;

        /// <summary>Constructor</summary>
        public DynamicAPIBackend()
        {
            SettingsRaw = new DynamicAPISettings();
            Status = BackendStatus.LOADING;
        }

        /// <summary>Normalize a provider key from display name to internal key</summary>
        public string NormalizeProviderKey(string provider) => provider switch
        {
            "BFL (Flux)" => "bfl",
            "OpenAI (DALL-E)" => "openai",
            "Ideogram" => "ideogram",
            _ => provider.ToLowerInvariant()
        };

        /// <summary>Get the API key type for the current provider</summary>
        protected override string GetApiKeyType()
        {
            string normalizedProvider = NormalizeProviderKey(Settings.SelectedProvider);
            return normalizedProvider switch
            {
                "bfl" => "black_forest_api",
                "openai" => "openai_api",
                "ideogram" => "ideogram_api",
                _ => throw new Exception($"Unsupported provider: {normalizedProvider}")
            };
        }

        /// <summary>Get the permission required for the current provider</summary>
        protected override PermInfo GetRequiredPermission()
        {
            string normalizedProvider = NormalizeProviderKey(Settings.SelectedProvider);
            return normalizedProvider switch
            {
                "openai" => APIBackendsPermissions.PermUseOpenAI,
                "ideogram" => APIBackendsPermissions.PermUseIdeogram,
                "bfl" => APIBackendsPermissions.PermUseBlackForest,
                _ => throw new Exception($"Unsupported provider: {normalizedProvider}")
            };
        }

        /// <summary>Get the base URL for the API request</summary>
        protected override string GetBaseUrl(T2IParamInput input)
        {
            string normalizedProvider = NormalizeProviderKey(Settings.SelectedProvider);
            RequestConfig config = ActiveProvider.RequestConfig;
            if (normalizedProvider == "bfl")
            {
                // Extract model name from input and clean it up
                string modelName = input.Get(T2IParamTypes.Model).Name
                    .Replace("API/", "")
                    .Replace(".safetensors", "");
                return $"{(!string.IsNullOrEmpty(CustomBaseUrl) ? CustomBaseUrl : config.BaseUrl)}/v1/{modelName}";
            }
            return !string.IsNullOrEmpty(CustomBaseUrl) ? CustomBaseUrl : config.BaseUrl;
        }

        /// <summary>Create an HTTP request for the specified API</summary>
        protected override HttpRequestMessage CreateHttpRequest(string baseUrl, JObject requestBody, T2IParamInput input)
        {
            string normalizedProvider = NormalizeProviderKey(Settings.SelectedProvider);
            HttpRequestMessage request = new(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json")
            };
            // Get API key
            string apiKey = input.SourceSession.User.GetGenericData(GetApiKeyType(), "key");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception($"API key not found for {normalizedProvider}. Please set up your API key in the User tab");
            }
            // Add authentication header
            if (normalizedProvider == "bfl")
            {
                request.Headers.Add("x-key", apiKey);
            }
            else
            {
                request.Headers.Add("Authorization", $"{ActiveProvider.RequestConfig.AuthHeader} {apiKey}");
            }
            return request;
        }

        /// <summary>Initialize the backend</summary>
        public override async Task Init()
        {
            Logs.Debug($"Initializing DynamicAPIBackend with provider: {Settings.SelectedProvider}");
            Models = new ConcurrentDictionary<string, List<string>>();
            Status = BackendStatus.LOADING;
            // If no provider is selected, prompt the user
            if (string.IsNullOrEmpty(Settings.SelectedProvider))
            {
                Logs.Warning("No API provider selected. Please choose a provider and save settings.");
                Status = BackendStatus.LOADING;
                AddLoadStatus("Please select an API provider from the dropdown and click Save.");
                return;
            }
            string normalizedProvider = NormalizeProviderKey(Settings.SelectedProvider);
            try
            {
                // Add feature flag for this provider
                if (_providerFeatures.TryGetValue(normalizedProvider, out string feature))
                {
                    SupportedFeatureSet.Add(feature);
                }
                // Register models locally for the backend
                await RegisterProviderModels();
                // Register models with the global registry if they aren't already there
                foreach (string modelName in Models.GetValueOrDefault("Stable-Diffusion", []))
                {
                    if (ActiveProvider.Models.TryGetValue(modelName, out T2IModel model))
                    {
                        if (!Program.MainSDModels.Models.ContainsKey(modelName))
                        {
                            // Create a copy of the model for the global registry
                            T2IModel globalModel = new(Program.MainSDModels, null, model.RawFilePath, modelName)
                            {
                                Title = model.Title,
                                Description = model.Description,
                                PreviewImage = model.PreviewImage,
                                ModelClass = model.ModelClass,
                                StandardWidth = model.StandardWidth,
                                StandardHeight = model.StandardHeight,
                                Metadata = model.Metadata,
                                IsSupportedModelType = true
                            };
                            // Register the model with the global registry
                            Program.MainSDModels.Models[modelName] = globalModel;
                            Logs.Verbose($"DynamicAPIBackend registered API model in global registry: {modelName}");
                        }
                    }
                }
                Logs.Verbose($"DynamicAPIBackend initialized with {Models["Stable-Diffusion"].Count} models for {normalizedProvider}");
                Status = BackendStatus.RUNNING;
            }
            catch (Exception ex)
            {
                Logs.Error($"Failed to initialize DynamicAPIBackend: {ex}");
                Status = BackendStatus.DISABLED;
            }
        }
    }
}