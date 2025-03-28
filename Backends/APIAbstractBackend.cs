using Newtonsoft.Json.Linq;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using SwarmUI.Accounts;
using Hartsy.Extensions.APIBackends.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using SwarmUI.Backends;

namespace Hartsy.Extensions.APIBackends.Backends
{
    /// <summary>Abstract base class for all API-based backends in the system.
    /// Provides common functionality for connecting to external APIs.</summary>
    public abstract class APIAbstractBackend : AbstractT2IBackend
    {
        /// <summary>Shared HttpClient for all API requests</summary>
        protected static readonly HttpClient HttpClient = new();

        /// <summary>Collection of supported features for this backend</summary>
        protected readonly HashSet<string> SupportedFeatureSet = [];

        /// <summary>Gets the active provider metadata</summary>
        protected abstract APIProviderMetadata ActiveProvider { get; }

        /// <summary>Gets a custom base URL for the provider, if specified</summary>
        protected abstract string CustomBaseUrl { get; }

        /// <summary>Get the API key type for the current provider</summary>
        protected abstract string GetApiKeyType();

        /// <summary>Get the permission required for the current provider</summary>
        protected abstract PermInfo GetRequiredPermission();

        /// <summary>Initialize models from provider metadata</summary>
        protected async Task RegisterProviderModels()
        {
            if (ActiveProvider == null)
            {
                throw new InvalidOperationException("No active provider configured");
            }
            List<string> modelNames = [];
            foreach (var (name, model) in ActiveProvider.Models)
            {
                string cleanName = name.Replace("API/", "").Replace(".safetensors", "");
                // Set additional properties for better integration
                model.Handler = Program.MainSDModels;  // Associate with the global handler
                // Ensure metadata is properly set
                if (model.Metadata == null)
                {
                    model.Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = name,
                        Title = model.Title,
                        Author = ActiveProvider.Name,
                        Description = model.Description,
                        PreviewImage = model.PreviewImage,
                        StandardWidth = model.StandardWidth,
                        StandardHeight = model.StandardHeight,
                        License = "Commercial",
                        UsageHint = $"API-based generation via {ActiveProvider.Name}",
                        ModelClassType = model.ModelClass?.ID,
                        Tags = ["api", ActiveProvider.Name.ToLowerInvariant()],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                }
                Logs.Verbose($"Registering API model: {name} with arch={model.ModelClass?.ID}, class={model.ModelClass?.Name}, compat={model.ModelClass?.CompatClass}");
                modelNames.Add(name);
            }
            Models.TryAdd("Stable-Diffusion", modelNames);
        }

        /// <summary>Get the base URL for the API request</summary>
        protected abstract string GetBaseUrl(T2IParamInput input);

        /// <summary>Create an HTTP request for the specified API</summary>
        protected abstract HttpRequestMessage CreateHttpRequest(string baseUrl, JObject requestBody, T2IParamInput input);

        /// <summary>Check if the user has permission to use this provider</summary>
        protected void CheckPermissions(T2IParamInput input)
        {
            PermInfo requiredPermission = GetRequiredPermission();
            if (!input.SourceSession.User.HasPermission(requiredPermission))
            {
                throw new Exception($"You do not have permission to use this API provider");
            }
        }

        /// <summary>Build the request body for the API call</summary>
        protected virtual JObject BuildRequestBody(T2IParamInput input)
        {
            return ActiveProvider.RequestConfig.BuildRequest(input);
        }

        /// <summary>Process the API response to extract image data</summary>
        protected virtual async Task<byte[]> ProcessResponse(JObject responseJson)
        {
            return await ActiveProvider.RequestConfig.ProcessResponse(responseJson);
        }

        /// <summary>Load a model</summary>
        public override async Task<bool> LoadModel(T2IModel model, T2IParamInput input)
        {
            if (ActiveProvider == null || !ActiveProvider.Models.ContainsKey(model.Name))
            {
                return false;
            }
            CurrentModelName = model.Name;
            return true;
        }

        /// <summary>Generate images with the API</summary>
        public override async Task<Image[]> Generate(T2IParamInput input)
        {
            // Check permissions
            CheckPermissions(input);
            // Get base URL and build request body
            string baseUrl = GetBaseUrl(input);
            JObject requestBody = BuildRequestBody(input);
            // Create and send request
            using HttpRequestMessage request = CreateHttpRequest(baseUrl, requestBody, input);
            HttpResponseMessage response = await HttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API request failed: {error}");
            }
            // Process response
            JObject responseJson = JObject.Parse(await response.Content.ReadAsStringAsync());
            byte[] imageData = await ProcessResponse(responseJson);
            return [new Image(imageData, Image.ImageType.IMAGE, "png")];
        }

        /// <summary>Generate images with live feedback</summary>
        public override async Task GenerateLive(T2IParamInput user_input, string batchId, Action<object> takeOutput)
        {
            // Send progress update
            takeOutput(new JObject
            {
                ["gen_progress"] = new JObject
                {
                    ["batch_index"] = batchId,
                    ["step"] = 0,
                    ["total_steps"] = 1
                }
            });
            Image[] results = await Generate(user_input);
            foreach (Image img in results)
            {
                takeOutput(img);
            }
        }

        /// <summary>Free memory (no-op for API backends)</summary>
        public override async Task<bool> FreeMemory(bool systemRam)
        {
            // API backends don't need to free memory
            return true;
        }

        /// <summary>Shutdown the backend</summary>
        public override async Task Shutdown()
        {
            Status = BackendStatus.DISABLED;
        }
    }
}