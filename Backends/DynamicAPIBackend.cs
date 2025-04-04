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
using SwarmUI.WebAPI;
using FreneticUtilities.FreneticExtensions;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Extensions.Hosting;

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
            [ManualSettingsOptions(Impl = null, Vals = ["", "bfl_api", "ideogram_api", "openai_api"],
                ManualNames = ["Select a provider...", "Black Forest Labs (Flux)", "Ideogram", "OpenAI (DALL-E)"])]
                        [ConfigComment("Choose the backend API provider to use for image generation.")]
            public string SelectedProvider = "";

            [ConfigComment("Custom Base URL (optional) This value will override the hardcoded base URL.")]
            public string CustomBaseUrl = "";
        }

        /// <summary>Current backend settings</summary>
        public DynamicAPISettings Settings => SettingsRaw as DynamicAPISettings;

        /// <summary>Access to the API provider service</summary>
        private static readonly APIProviderInit _providerInit = new();

        /// <summary>Gets the active provider metadata</summary>
        protected override APIProviderMetadata ActiveProvider
        {
            get
            {
                if (!_providerInit.Providers.TryGetValue(Settings.SelectedProvider, out APIProviderMetadata value))
                {
                    Logs.Error($"[DynamicAPIBackend] Provider key not found: {Settings.SelectedProvider}. Available providers: {string.Join(", ", _providerInit.Providers.Keys)}");
                    return null;
                }
                return value;
            }
        }

        /// <summary>Gets the custom base URL if specified</summary>
        protected override string CustomBaseUrl => Settings.CustomBaseUrl;

        /// <summary>Gets the supported features for this backend</summary>
        public override IEnumerable<string> SupportedFeatures => SupportedFeatureSet;

        /// <summary>Dictionary of remote models this backend provides, by type</summary>
        public Dictionary<string, Dictionary<string, JObject>> RemoteModels { get; set; } = [];

        /// <summary>Collection of all registered models, keyed by model name</summary>
        private Dictionary<string, T2IModel> RegisteredApiModels { get; set; } = [];

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
            ["ideogram_api"] = APIBackendsPermissions.PermUseIdeogram
        };

        protected override PermInfo GetRequiredPermission() =>
            _providerToPermission.TryGetValue(Settings.SelectedProvider, out PermInfo permission)
                ? permission
                : throw new Exception($"Unknown provider: {Settings.SelectedProvider}");

        /// <summary>Get the base URL for the API request</summary>
        protected override string GetBaseUrl(T2IParamInput input)
        {
            string baseUrl = !string.IsNullOrEmpty(CustomBaseUrl) ? CustomBaseUrl : ActiveProvider.RequestConfig.BaseUrl;
            if (Settings.SelectedProvider == "bfl_api")
            {
                // Extract model name from input and clean it up
                string modelName = input.Get(T2IParamTypes.Model).Name
                    .Replace("BFL/", "") //TODO: Why is this done here and why only BFL? How do the others work? 
                    .Replace(".safetensors", "");
                return $"{baseUrl}/v1/{modelName}";
            }
            Logs.Verbose($"[DynamicAPIBackend] Using base URL: {baseUrl}");
            return baseUrl;
        }

        /// <summary>Create an HTTP request for the specified API</summary>
        protected override HttpRequestMessage CreateHttpRequest(string baseUrl, JObject requestBody, T2IParamInput input)
        {
            HttpRequestMessage request = new(HttpMethod.Post, baseUrl) // TODO: Check swarm code to see if we can use a different method
            {
                Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json")
            };
            string provider = Settings.SelectedProvider;
            string apiKey = input.SourceSession.User.GetGenericData(provider, "key")?.Trim(); // Trim to remove trailing space
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception($"API key not found for {provider}. Please set up your API key in the User tab");
            }
            // TODO: Is auth header already mapped in RequestConfig? If so we can use that instead of an if else
            // BFL and Ideogram use different header names for API key
            if (provider == "bfl_api")
            {
                request.Headers.Add("x-key", apiKey);
            }
            if (provider == "ideogram_api")
            {
                request.Headers.Add("Api-Key", apiKey);
            }
            else
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
            return request;
        }

        /// <summary>Validates the API key for the current provider</summary>
        protected async Task<bool> ValidateApiKey()
        {
            string provider = Settings.SelectedProvider;
            if (string.IsNullOrEmpty(provider))
            {
                Logs.Warning("[DynamicAPIBackend] Cannot validate API key: No provider selected");
                return false;
            }
            try
            {
                string sessionId = null;
                User sessionUser = null;
                try
                {
                    //TODO: There has to be a more efficient way to get the current user session without creating a new one
                    // Create a new session to access the current user context
                    using CancellationTokenSource timeout = Utilities.TimedCancel(TimeSpan.FromSeconds(10)); 
                    string host = Program.ServerSettings.Network.Host;
                    string port = Program.ServerSettings.Network.Port.ToString();
                    JObject sessData = await HttpClient.PostJson($"http://{host}:{port}/API/GetNewSession", [], null, timeout.Token);
                    sessionId = sessData["session_id"].ToString();
                    // Use the session ID to get the session object and user then check for the API key
                    if (Program.Sessions.TryGetSession(sessionId, out Session session) && session?.User != null)
                    {
                        sessionUser = session.User;
                        string apiKey = sessionUser.GetGenericData(provider, "key");
                        // TODO: Check if the API key is valid. Maybe by making a simple test request to the API?
                        if (!string.IsNullOrEmpty(apiKey))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logs.Error($"[DynamicAPIBackend] Error creating test session: {ex.Message}");
                }
                Logs.Error($"[DynamicAPIBackend] API key validation failed: No API key found for {provider}. Have you saved your key?");
                return false;
            }
            catch (Exception ex)
            {
                Logs.Error($"[DynamicAPIBackend] API key validation error for {provider}: {ex.Message}");
                AddLoadStatus($"API key validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Initializes this backend, setting up client and configuration.</summary>
        public override async Task Init()
        {
            Status = BackendStatus.LOADING;
            string provider = Settings.SelectedProvider;
            Models = new ConcurrentDictionary<string, List<string>>();
            // Early exit if no provider selected or API key is invalid
            if (string.IsNullOrEmpty(provider))
            {
                Logs.Warning("[DynamicAPIBackend] No API provider selected. Please choose a provider and save settings.");
                Status = BackendStatus.DISABLED;
                AddLoadStatus("Please select an API provider from the dropdown and click Save.");
                return;
            }
            if (!(await ValidateApiKey()))
            {
                Status = BackendStatus.ERRORED;
                AddLoadStatus($"Please set up your {provider} API key in the User tab.");
                return;
            }
            try
            {
                // Add feature flag for this provider so params are displayed in the UI
                SupportedFeatureSet.Add(provider);
                // Register models and update remote models
                await RegisterProviderModels();
                UpdateRemoteModels();
                Status = BackendStatus.RUNNING;
            }
            catch (Exception ex)
            {
                Logs.Error($"Failed to initialize DynamicAPIBackend: {ex}");
                Status = BackendStatus.ERRORED;
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
            Logs.Verbose($"[DynamicAPIBackend] Registered {RegisteredApiModels.Count} models from {Settings.SelectedProvider}");
        }

        private JObject CreateModelMetadata(T2IModel model, string modelName)
        {
            return new JObject
            {
                ["name"] = modelName,
                ["title"] = model.Title ?? modelName,
                ["description"] = model.Description ?? $"API model from {Settings.SelectedProvider}",
                ["preview_image"] = model.PreviewImage ?? "",
                ["loaded"] = true,
                ["architecture"] = model.ModelClass?.ID ?? "stable-diffusion",
                ["class"] = model.ModelClass?.Name ?? Settings.SelectedProvider,
                ["compat_class"] = model.ModelClass?.CompatClass ?? "stable-diffusion",
                ["standard_width"] = model.StandardWidth > 0 ? model.StandardWidth : 1024,
                ["standard_height"] = model.StandardHeight > 0 ? model.StandardHeight : 1024,
                ["is_supported_model_format"] = true,
                ["tags"] = new JArray("api", ActiveProvider.Name.ToLowerInvariant()),
                ["local"] = false,
                ["api_source"] = Settings.SelectedProvider
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