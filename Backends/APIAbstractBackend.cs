using Newtonsoft.Json.Linq;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using SwarmUI.Media;
using SwarmUI.Accounts;
using SwarmUI.Backends;
using Hartsy.Extensions.APIBackends.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text;

namespace Hartsy.Extensions.APIBackends.Backends;

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

    /// <summary>Get the base URL for the API request</summary>
    protected abstract string GetBaseUrl(T2IParamInput input);

    /// <summary>Create an HTTP request for the specified API</summary>
    protected abstract HttpRequestMessage CreateHttpRequest(string baseUrl, JObject requestBody, T2IParamInput input);

    /// <summary>Gets the API key for the current provider from the user session</summary>
    protected abstract string GetApiKey(T2IParamInput input);

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

    /// <summary>Max length a string value may reach in a log before it's replaced by a size marker.</summary>
    private const int LogElideThreshold = 192;

    /// <summary>Max length of a response body in a log.</summary>
    private const int LogResponseLimit = 4096;

    /// <summary>Copy of a request body safe to log: long strings (base64 images/videos) become size markers,
    /// so the schema-relevant fields stay readable.</summary>
    protected static string SummarizeForLog(JToken token)
    {
        return Elide(token.DeepClone()).ToString(Newtonsoft.Json.Formatting.None);
    }

    private static JToken Elide(JToken token)
    {
        if (token is JObject obj)
        {
            foreach (JProperty prop in obj.Properties())
            {
                prop.Value = Elide(prop.Value);
            }
        }
        else if (token is JArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                arr[i] = Elide(arr[i]);
            }
        }
        else if (token.Type == JTokenType.String)
        {
            string str = token.ToString();
            if (str.Length > LogElideThreshold)
            {
                string kind = str.StartsWith("data:") ? str[..Math.Min(str.IndexOf(',') + 1, 64)] : "<long string>";
                return $"{kind} …elided, {str.Length} chars";
            }
        }
        return token;
    }

    /// <summary>Build the request body for the API call</summary>
    protected virtual JObject BuildRequestBody(T2IParamInput input)
    {
        try
        {
            JObject requestBody = ActiveProvider.RequestConfig.BuildRequest(input);
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Built request body: {SummarizeForLog(requestBody)}");
            return requestBody;
        }
        catch (Exception ex)
        {
            Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error building request body: {ex.Message}");
            throw;
        }
    }

    /// <summary>Process the API response to extract image data</summary>
    protected virtual async Task<byte[]> ProcessResponse(JObject responseJson, string apiKey)
    {
        try
        {
            return await ActiveProvider.RequestConfig.ProcessResponse(responseJson, apiKey);
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

    /// <summary>Determines the media type of a response. The model's declared modality is authoritative:
    /// providers that return the video out-of-band (OpenAI Sora responds with a job envelope, not a "video" key)
    /// would otherwise have their mp4 bytes mislabelled as an image.</summary>
    protected virtual MediaType DetermineResponseMediaType(JObject responseJson, T2IParamInput input)
    {
        string modelName = input?.Get(T2IParamTypes.Model)?.Name ?? "";
        if (APIProviderRegistry.TryGetModel(modelName, out ModelDefinition model) && model.Modality == ModelModality.Video)
        {
            return MediaType.VideoMp4;
        }
        if (responseJson["video"] is not null)
        {
            return MediaType.VideoMp4;
        }
        return MediaType.ImagePng;
    }

    /// <summary>Generate images with the API</summary>
    public override async Task<Image[]> Generate(T2IParamInput input)
    {
        try
        {
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Starting generation with model: {CurrentModelName}");
            CheckPermissions(input);
            string baseUrl = GetBaseUrl(input);
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Using base URL: {baseUrl}");
            JObject requestBody = BuildRequestBody(input);
            using HttpRequestMessage request = CreateHttpRequest(baseUrl, requestBody, input);
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - {request.Method} {request.RequestUri}");
            if (request.Content is not null)
            {
                try
                {
                    string sent = await request.Content.ReadAsStringAsync();
                    // Reparse rather than logging requestBody: this is what actually goes on the wire, after any builder post-processing.
                    Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Request body: {SummarizeForLog(JToken.Parse(sent))}");
                }
                catch (Exception ex)
                {
                    Logs.Error($"[APIAbstractBackend] {GetType().Name} - Failed to read request content for logging: {ex.Message}");
                }
            }
            HttpResponseMessage response = await HttpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Logs.Error($"[APIAbstractBackend] {GetType().Name} - API request failed: {response.StatusCode} - {responseText}");
                throw new Exception($"API request failed: {responseText}");
            }
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Received successful response: {response.StatusCode}");
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Response body: {(responseText.Length > LogResponseLimit ? $"{responseText[..LogResponseLimit]}… ({responseText.Length} chars)" : responseText)}");
            JObject responseJson = JObject.Parse(responseText);
            string apiKey = GetApiKey(input);
            byte[] data = await ProcessResponse(responseJson, apiKey);
            MediaType mediaType = DetermineResponseMediaType(responseJson, input);
            Logs.Verbose($"[APIAbstractBackend] {GetType().Name} - Response media type: {mediaType.Extension}");
            return [new Image(data, mediaType)];
        }
        catch (Exception ex)
        {
            Logs.Error($"[APIAbstractBackend] {GetType().Name} - Error during generation: {ex.Message}");
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