using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using Hartsy.Extensions.APIBackends.Models;
using Newtonsoft.Json.Linq;
using SwarmUI.Accounts;
using SwarmUI.Backends;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using SwarmUI.WebAPI;

namespace Hartsy.Extensions.APIBackends.Backends
{
    /// <summary>A dynamic backend that supports multiple API providers for image generation.</summary>
    public class DynamicAPIBackend : APIAbstractBackend
    {
        /// <summary>Static constructor to register our model provider with ModelsAPI</summary>
        static DynamicAPIBackend()
        {
            // Register our API models provider with ModelsAPI.ExtraModelProviders
            ModelsAPI.ExtraModelProviders["dynamic_api_backends"] = GetApiModels;
        }

        /// <summary>Static method to provide API models from all DynamicAPIBackend instances</summary>
        private static Dictionary<string, JObject> GetApiModels(string subtype)
        {
            // Get all running DynamicAPIBackend instances
            IEnumerable<DynamicAPIBackend> apiBackends = Program.Backends.RunningBackendsOfType<DynamicAPIBackend>().Where(b => b.RemoteModels != null);
            // Special handling based on subtype
            if (subtype == "Stable-Diffusion" || string.IsNullOrEmpty(subtype))
            {
                IEnumerable<Dictionary<string, JObject>> modelSets = apiBackends.Select(b => b.RemoteModels.GetValueOrDefault("Stable-Diffusion")).Where(m => m != null);
                Dictionary<string, JObject> result = [];
                foreach (Dictionary<string, JObject> modelSet in modelSets)
                {
                    foreach (KeyValuePair<string, JObject> kvp in modelSet)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
                Logs.Verbose($"[DynamicAPIBackend] Returned {result.Count} models for subtype: {subtype}");
                return result;
            }
            // TODO: Handle other subtypes if needed (e.g., "TextualInversion", "Lora", etc.)
            // For other subtypes, return an empty dictionary for now
            return [];
        }

        /// <summary>Settings for the dynamic API backend.</summary>
        public class DynamicAPISettings : AutoConfiguration
        {
            [ConfigComment("Enable Black Forest Labs (Flux) API models.")]
            public bool EnableBlackForest = false;

            [ConfigComment("Enable Ideogram API models.")]
            public bool EnableIdeogram = false;

            [ConfigComment("Enable OpenAI (DALL-E, GPT Image) API models.")]
            public bool EnableOpenAI = false;

            [ConfigComment("Enable Grok API models.")]
            public bool EnableGrok = false;

            [ConfigComment("Enable Google (Imagen, Gemini) API models.")]
            public bool EnableGoogle = false;

            [ConfigComment("Enable Fal.ai API models (600+ models including FLUX, SD, Recraft, etc.).")]
            public bool EnableFal = false;

            [ConfigComment("Custom Base URL (optional). Overrides the default base URL for the selected provider during generation.")]
            public string CustomBaseUrl = "";
        }

        /// <summary>Maps setting properties to provider IDs.</summary>
        private static readonly Dictionary<string, Func<DynamicAPISettings, bool>> ProviderSettingsMap = new()
        {
            ["bfl_api"] = s => s.EnableBlackForest,
            ["ideogram_api"] = s => s.EnableIdeogram,
            ["openai_api"] = s => s.EnableOpenAI,
            ["grok_api"] = s => s.EnableGrok,
            ["google_api"] = s => s.EnableGoogle,
            ["fal_api"] = s => s.EnableFal
        };

        /// <summary>Gets the list of currently enabled provider IDs based on settings.</summary>
        private List<string> GetEnabledProviderIds()
        {
            List<string> enabled = new List<string>();
            foreach (KeyValuePair<string, Func<DynamicAPISettings, bool>> kvp in ProviderSettingsMap)
            {
                string providerId = kvp.Key;
                Func<DynamicAPISettings, bool> isEnabled = kvp.Value;
                if (isEnabled(Settings))
                {
                    enabled.Add(providerId);
                }
            }
            return enabled;
        }

        /// <summary>Current backend settings</summary>
        public DynamicAPISettings Settings => SettingsRaw as DynamicAPISettings;

        /// <summary>Gets the active provider metadata based on the model being used for generation.</summary>
        protected override APIProviderMetadata ActiveProvider => _currentGenerationProvider;

        /// <summary>Stores the provider being used for the current generation request.</summary>
        private APIProviderMetadata _currentGenerationProvider;

        /// <summary>Gets the custom base URL if specified</summary>
        protected override string CustomBaseUrl => Settings.CustomBaseUrl;

        /// <summary>Gets the supported features for this backend</summary>
        public override IEnumerable<string> SupportedFeatures => SupportedFeatureSet;

        /// <summary>Dictionary of remote models this backend provides, by type</summary>
        public Dictionary<string, Dictionary<string, JObject>> RemoteModels { get; set; } = new Dictionary<string, Dictionary<string, JObject>>();

        /// <summary>Collection of all registered models, keyed by model name</summary>
        private Dictionary<string, T2IModel> RegisteredApiModels { get; set; } = new Dictionary<string, T2IModel>();

        /// <summary>Constructor</summary>
        public DynamicAPIBackend()
        {
            SettingsRaw = new DynamicAPISettings();
            Status = BackendStatus.LOADING;
        }

        private static readonly Dictionary<string, PermInfo> _providerToPermission = new()
        {
            ["bfl_api"] = APIBackendsPermissions.PermUseBlackForest,
            ["openai_api"] = APIBackendsPermissions.PermUseOpenAI,
            ["ideogram_api"] = APIBackendsPermissions.PermUseIdeogram,
            ["grok_api"] = APIBackendsPermissions.PermUseGrok,
            ["google_api"] = APIBackendsPermissions.PermUseGoogleImagen,
            ["fal_api"] = APIBackendsPermissions.PermUseFal
        };

        private bool CheckIdeogramEdit(T2IParamInput input)
        {
            return input.TryGet(SwarmUIAPIBackends.ImagePromptParam_Ideogram, out Image inputImg) && inputImg?.RawData != null;
        }

        private static bool IsIdeogramV3Model(string modelName)
        {
            if (string.IsNullOrEmpty(modelName)) return false;
            string lower = modelName.ToLowerInvariant();
            return lower.Contains("/v_3") || lower.Contains("/v3") || lower.Contains("v_3") || lower.Contains("v3");
        }

        /// <summary>Determines the provider ID from a model name.</summary>
        private string GetProviderIdFromModel(string modelName)
        {
            if (modelName.StartsWith("API Models/BFL/")) return "bfl_api";
            if (modelName.StartsWith("API Models/OpenAI/")) return "openai_api";
            if (modelName.StartsWith("API Models/Ideogram/")) return "ideogram_api";
            if (modelName.StartsWith("API Models/Grok/")) return "grok_api";
            if (modelName.StartsWith("API Models/Google/")) return "google_api";
            if (modelName.StartsWith("API Models/Fal/")) return "fal_api";
            return null;
        }

        /// <summary>Sets the current provider based on the model being used.</summary>
        private void SetCurrentProviderFromModel(T2IParamInput input)
        {
            string modelName = input.Get(T2IParamTypes.Model)?.Name;
            string providerId = GetProviderIdFromModel(modelName);
            if (providerId != null && APIProviderRegistry.Instance.Providers.TryGetValue(providerId, out APIProviderMetadata provider))
            {
                _currentGenerationProvider = provider;
            }
            else
            {
                throw new Exception($"Could not determine provider for model: {modelName}");
            }
        }

        protected override PermInfo GetRequiredPermission()
        {
            string providerId = GetProviderIdFromModel(CurrentModelName);
            if (providerId != null && _providerToPermission.TryGetValue(providerId, out PermInfo permission))
            {
                return permission;
            }
            throw new Exception($"Unknown provider for model: {CurrentModelName}");
        }

        /// <summary>Get the base URL for the API request</summary>
        protected override string GetBaseUrl(T2IParamInput input)
        {
            SetCurrentProviderFromModel(input);
            string baseUrl = !string.IsNullOrEmpty(CustomBaseUrl) ? CustomBaseUrl : ActiveProvider.RequestConfig.BaseUrl;
            string modelName = input.Get(T2IParamTypes.Model).Name;
            string providerId = GetProviderIdFromModel(modelName);

            if (providerId == "bfl_api")
            {
                string cleanName = modelName.Replace("API Models/BFL/", "").Replace(".safetensors", "");
                return $"{baseUrl}/v1/{cleanName}";
            }
            else if (providerId == "ideogram_api")
            {
                bool hasInputImage = CheckIdeogramEdit(input);
                bool isV3 = IsIdeogramV3Model(modelName);
                string baseUrlForIdeogram = isV3 ? 
                    hasInputImage ? "https://api.ideogram.ai/v1/ideogram-v3/edit" : "https://api.ideogram.ai/v1/ideogram-v3/generate" 
                    : hasInputImage ? "https://api.ideogram.ai/edit" : baseUrl;
                return baseUrlForIdeogram;
            }
            else if (providerId == "google_api")
            {
                string cleanName = modelName.Replace("API Models/Google/", "");
                string baseUrlForGoogle = cleanName.StartsWith("gemini-") ? 
                    $"{baseUrl}/{cleanName}:generateContent" : $"{baseUrl}/{cleanName}:predict";
                return baseUrlForGoogle;
            }
            Logs.Verbose($"[DynamicAPIBackend] Using base URL: {baseUrl}");
            return baseUrl;
        }

        /// <summary>Create an HTTP request for the specified API</summary>
        protected override HttpRequestMessage CreateHttpRequest(string baseUrl, JObject requestBody, T2IParamInput input)
        {
            HttpRequestMessage request = new(HttpMethod.Post, baseUrl);
            string modelName = input.Get(T2IParamTypes.Model).Name;
            string providerId = GetProviderIdFromModel(modelName);
            if (CheckIdeogramEdit(input))
            {
                MultipartFormDataContent formData = new MultipartFormDataContent();
                foreach (JProperty property in requestBody.Properties())
                {
                    if (property.Value != null &&
                        property.Name != "image" &&
                        property.Name != "mask" && property.Name != "image_file")
                    {
                        formData.Add(new StringContent(property.Value.ToString()), property.Name);
                    }
                }

                if (input.TryGet(SwarmUIAPIBackends.ImagePromptParam_Ideogram, out Image inputImg) && inputImg?.RawData != null)
                {

                    string requestImageType = "image_file";
                    if (IsIdeogramV3Model(modelName))
                    {
                        requestImageType = "image";
                    }
                    ByteArrayContent imageContent = new ByteArrayContent(inputImg.RawData);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    formData.Add(imageContent, requestImageType, "input.png");
                }

                if (input.TryGet(SwarmUIAPIBackends.ImageMaskPromptParam_Ideogram, out Image maskImg) && maskImg?.RawData != null)
                {
                    ByteArrayContent maskContent = new ByteArrayContent(maskImg.RawData);
                    maskContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    formData.Add(maskContent, "mask", "mask.png");
                }

                request.Content = formData;
            }
            else
            {
                request.Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            }
            string apiKey = input.SourceSession.User.GetGenericData(providerId, "key")?.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception($"API key not found for {providerId}. Please set up your API key in the User tab");
            }
            AddAuthHeader(request, providerId, apiKey);
            return request;
        }

        /// <summary>Adds the appropriate authentication header for the provider.</summary>
        private static void AddAuthHeader(HttpRequestMessage request, string providerId, string apiKey)
        {
            switch (providerId)
            {
                case "bfl_api":
                    request.Headers.Add("x-key", apiKey);
                    break;
                case "ideogram_api":
                    request.Headers.Add("Api-Key", apiKey);
                    break;
                case "google_api":
                    request.Headers.Add("x-goog-api-key", apiKey);
                    break;
                default:
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    break;
            }
        }

        /// <summary>Validates API keys for all enabled providers using the local user.</summary>
        protected bool ValidateApiKeysForEnabledProviders(List<string> enabledProviders)
        {
            // Use local user for backend initialization validation
            User user = Program.Sessions.GetUser(SessionHandler.LocalUserID);
            List<string> missingKeys = new List<string>();
            foreach (string providerId in enabledProviders)
            {
                string apiKey = user.GetGenericData(providerId, "key");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    missingKeys.Add(providerId);
                }
            }
            if (missingKeys.Count > 0)
            {
                string missing = string.Join(", ", missingKeys);
                Logs.Warning($"[DynamicAPIBackend] Missing API keys for: {missing}. Add them in the User tab.");
                AddLoadStatus($"Missing API keys for: {missing}. Add them in the User tab.");
                return false;
            }
            return true;
        }

        /// <summary>Initializes this backend, setting up client and configuration.</summary>
        public override async Task Init()
        {
            Status = BackendStatus.LOADING;
            Models = new ConcurrentDictionary<string, List<string>>();
            SupportedFeatureSet.Clear();
            RegisteredApiModels.Clear();
            RemoteModels.Clear();

            List<string> enabledProviders = GetEnabledProviderIds();
            if (enabledProviders.Count == 0)
            {
                Logs.Warning("[DynamicAPIBackend] No API providers enabled. Please enable at least one provider and save settings.");
                Status = BackendStatus.DISABLED;
                AddLoadStatus("Please enable at least one API provider checkbox and click Save.");
                return;
            }

            if (!ValidateApiKeysForEnabledProviders(enabledProviders))
            {
                Status = BackendStatus.ERRORED;
                return;
            }

            try
            {
                // Register models for ALL enabled providers
                foreach (string providerId in enabledProviders)
                {
                    if (!APIProviderRegistry.Instance.Providers.TryGetValue(providerId, out APIProviderMetadata providerMeta))
                    {
                        Logs.Warning($"[DynamicAPIBackend] Provider '{providerId}' not found in registry, skipping.");
                        continue;
                    }
                    // Add feature flag for this provider so params are displayed in the UI
                    SupportedFeatureSet.Add(providerId);
                    // Register models from this provider
                    await RegisterModelsForProvider(providerMeta);
                    Logs.Verbose($"[DynamicAPIBackend] Registered models for provider: {providerId}");
                }
                UpdateRemoteModels();
                Status = BackendStatus.RUNNING;
                Logs.Info($"[DynamicAPIBackend] Initialized with {enabledProviders.Count} provider(s): {string.Join(", ", enabledProviders)}");
            }
            catch (Exception ex)
            {
                Logs.Error($"[DynamicAPIBackend] Failed to initialize: {ex}");
                Status = BackendStatus.ERRORED;
            }
        }

        /// <summary>Registers models for a specific provider.</summary>
        private async Task RegisterModelsForProvider(APIProviderMetadata provider)
        {
            Logs.Debug($"[DynamicAPIBackend] RegisterModelsForProvider called for {provider.Name}, provider.Models.Count = {provider.Models?.Count ?? 0}");
            List<string> modelNames = new List<string>();
            foreach (KeyValuePair<string, T2IModel> kvp in provider.Models)
            {
                string name = kvp.Key;
                T2IModel model = kvp.Value;
                Logs.Debug($"[DynamicAPIBackend] Processing model: {name}");
                model.Handler = Program.MainSDModels;
                model.Metadata ??= new T2IModelHandler.ModelMetadataStore
                {
                    ModelName = name,
                    Title = model.Title,
                    Author = provider.Name,
                    Description = model.Description,
                    PreviewImage = model.PreviewImage,
                    StandardWidth = model.StandardWidth,
                    StandardHeight = model.StandardHeight,
                    License = "Commercial",
                    UsageHint = $"API-based generation via {provider.Name}",
                    ModelClassType = model.ModelClass?.ID,
                    Tags = new string[] { "api", provider.Name.ToLowerInvariant() },
                    TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                modelNames.Add(name);
                if (!Program.MainSDModels.Models.ContainsKey(name))
                {
                    Program.MainSDModels.Models[name] = model;
                    Logs.Debug($"[DynamicAPIBackend] Added model to MainSDModels: {name}");
                }
                RegisteredApiModels[name] = model;
            }
            Logs.Debug($"[DynamicAPIBackend] Total modelNames collected: {modelNames.Count}");

            if (Models.TryGetValue("Stable-Diffusion", out List<string> existingModels))
            {
                existingModels.AddRange(modelNames);
                Logs.Debug($"[DynamicAPIBackend] Added {modelNames.Count} models to existing SD list, total now: {existingModels.Count}");
            }
            else
            {
                Models.TryAdd("Stable-Diffusion", modelNames);
                Logs.Debug($"[DynamicAPIBackend] Created new SD model list with {modelNames.Count} models");
            }
        }

        private void UpdateRemoteModels()
        {
            if (!Models.TryGetValue("Stable-Diffusion", out List<string> sdModels) || !sdModels.Any())
            {
                Logs.Warning("[DynamicAPIBackend] No models found to register");
                return;
            }
            Dictionary<string, JObject> remoteSD = RemoteModels.GetOrCreate("Stable-Diffusion", () => []);
            remoteSD.Clear();
            foreach (string modelName in sdModels)
            {
                if (Program.MainSDModels.Models.TryGetValue(modelName, out T2IModel model))
                {
                    RegisteredApiModels[modelName] = model;
                    remoteSD[modelName] = CreateModelMetadata(model, modelName);
                }
            }
            Logs.Verbose($"[DynamicAPIBackend] Registered {RegisteredApiModels.Count} total API models");
        }

        private JObject CreateModelMetadata(T2IModel model, string modelName)
        {
            string providerId = GetProviderIdFromModel(modelName) ?? "unknown";
            return new JObject
            {
                ["name"] = modelName,
                ["title"] = model.Title ?? modelName,
                ["description"] = model.Description ?? $"API model",
                ["preview_image"] = model.PreviewImage ?? "",
                ["loaded"] = true,
                ["architecture"] = model.ModelClass?.ID ?? "stable-diffusion",
                ["class"] = model.ModelClass?.Name ?? providerId,
                ["compat_class"] = model.ModelClass?.ID ?? "stable-diffusion",
                ["standard_width"] = model.StandardWidth > 0 ? model.StandardWidth : 1024,
                ["standard_height"] = model.StandardHeight > 0 ? model.StandardHeight : 1024,
                ["is_supported_model_format"] = true,
                ["tags"] = new JArray("api", providerId),
                ["local"] = false,
                ["api_source"] = providerId
            };
        }

        /// <summary>Shutdown the backend</summary>
        public override async Task Shutdown()
        {
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Shutting down backend");
            // Properly clean up model registrations
            foreach (string modelName in RegisteredApiModels.Keys)
            {
                if (Program.MainSDModels.Models.ContainsKey(modelName))
                {
                    Logs.Verbose($"[DynamicAPIBackend] Removing API model from global registry: {modelName}");
                    Program.MainSDModels.Models.Remove(modelName, out _);
                }
            }
            RegisteredApiModels.Clear();
            Status = BackendStatus.DISABLED;
        }
    }
}