using Newtonsoft.Json.Linq;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using SwarmUI.Accounts;
using SwarmUI.Backends;
using Hartsy.Extensions.APIBackends.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hartsy.Extensions.APIBackends.Backends
{
    /// <summary>Abstract base class for all API-based backends in the system.
    /// Provides common functionality for connecting to external APIs.</summary>
    public abstract class APIAbstractBackend : AbstractT2IBackend
    {
        /// <summary>Shared HttpClient for all API requests</summary>
        protected static readonly HttpClient HttpClient = NetworkBackendUtils.MakeHttpClient();

        /// <summary>Collection of supported features for this backend</summary>
        protected readonly HashSet<string> SupportedFeatureSet = [];

        /// <summary>Gets the active provider metadata</summary>
        protected abstract APIProviderMetadata ActiveProvider { get; }

        /// <summary>Gets a custom base URL for the provider, if specified</summary>
        protected abstract string CustomBaseUrl { get; }

        /// <summary>Get the permission required for the current provider</summary>
        protected abstract PermInfo GetRequiredPermission();

        /// <summary>Initialize models from provider metadata</summary>
        protected async Task RegisterProviderModels()
        {
            try
            {   
                List<string> modelNames = [];
                foreach (var (name, model) in ActiveProvider.Models)
                {
                    string cleanName = name.Replace("API/", "").Replace(".safetensors", "");
                    Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Processing model: {name} (clean name: {cleanName})");
                    // Set additional properties for better integration
                    model.Handler = Program.MainSDModels;  // Associate with the global handler
                    // Ensure metadata is properly set
                    model.Metadata ??= new T2IModelHandler.ModelMetadataStore
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
                    Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Registering API model: {name} with arch={model.ModelClass?.ID}, class={model.ModelClass?.Name}, compat={model.ModelClass?.CompatClass}");
                    modelNames.Add(name);
                    // Add the model directly to the Models dictionary as well
                    // This is critical because GetModel() method looks for models directly in the Models dictionary
                    if (!Program.MainSDModels.Models.ContainsKey(name))
                    {
                        Program.MainSDModels.Models[name] = model;
                    }
                }
                // Check if Models dictionary contains "Stable-Diffusion" key
                if (Models.TryGetValue("Stable-Diffusion", out List<string> value))
                {
                    value.AddRange(modelNames);
                }
                else
                {
                    Models.TryAdd("Stable-Diffusion", modelNames);
                }
                Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Successfully registered {modelNames.Count} models from provider {ActiveProvider.Name}");
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error registering provider models: {ex.Message}");
                throw;
            }
        }

        /// <summary>Get the base URL for the API request</summary>
        protected abstract string GetBaseUrl(T2IParamInput input);

        /// <summary>Create an HTTP request for the specified API</summary>
        protected abstract HttpRequestMessage CreateHttpRequest(string baseUrl, JObject requestBody, T2IParamInput input);

        /// <summary>Check if the user has permission to use this provider</summary>
        protected void CheckPermissions(T2IParamInput input)
        {
            try
            {
                PermInfo requiredPermission = GetRequiredPermission();
                if (!input.SourceSession.User.HasPermission(requiredPermission))
                {
                    Logs.Warning($"[APIAbstractBackend] {GetType().Name} - User lacks required permission");
                    throw new Exception($"You do not have permission to use this API provider");
                }
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error checking permissions: {ex.Message}");
                throw;
            }
        }

        /// <summary>Build the request body for the API call</summary>
        protected virtual JObject BuildRequestBody(T2IParamInput input)
        {
            try
            {
                JObject requestBody = ActiveProvider.RequestConfig.BuildRequest(input);
                Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Built request body: {requestBody}");
                return requestBody;
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error building request body: {ex.Message}");
                throw;
            }
        }

        /// <summary>Process the API response to extract image data</summary>
        protected virtual async Task<byte[]> ProcessResponse(JObject responseJson)
        {
            try
            {
                return await ActiveProvider.RequestConfig.ProcessResponse(responseJson);
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error processing API response: {ex.Message}");
                throw;
            }
        }

        /// <summary>Load a model</summary>
        public override async Task<bool> LoadModel(T2IModel model, T2IParamInput input)
        {
            try
            {   
                CurrentModelName = model.Name;
                Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Successfully loaded model: {model.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error loading model: {ex.Message}");
                return false;
            }
        }

        /// <summary>Generate images with the API</summary>
        public override async Task<Image[]> Generate(T2IParamInput input)
        {
            try
            {
                Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Starting image generation with model: {CurrentModelName}");
                CheckPermissions(input);
                // Get base URL and build request body
                string baseUrl = GetBaseUrl(input);
                Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Using base URL: {baseUrl}");
                JObject requestBody = BuildRequestBody(input);
                // Create and send request
                using HttpRequestMessage request = CreateHttpRequest(baseUrl, requestBody, input);
                if (request.Content != null)
                {
                    try
                    {
                        await request.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        Logs.Error($"[APIAbstractBackend] {GetType().Name} - Failed to read request content for logging: {ex.Message}");
                    }
                }
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Logs.Error($"[APIAbstractBackend] {GetType().Name} - API request failed: {response.StatusCode} - {error}");
                    throw new Exception($"API request failed: {error}");
                }
                Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Received successful response: {response.StatusCode}");
                JObject responseJson = JObject.Parse(await response.Content.ReadAsStringAsync());
                byte[] imageData = await ProcessResponse(responseJson);
                return [new Image(imageData, Image.ImageType.IMAGE, "png")];
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error during image generation: {ex.Message}");
                throw;
            }
        }

        /// <summary>Generate images with live feedback</summary>
        public override async Task GenerateLive(T2IParamInput user_input, string batchId, Action<object> takeOutput)
        {
            try
            {
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
            catch (Exception ex)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error during live generation: {ex.Message}");
                throw;
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
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Shutting down backend");
            Status = BackendStatus.DISABLED;
        }
    }
}