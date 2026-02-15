using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwarmUI.Backends;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Interface for building provider-specific API requests.</summary>
public interface IRequestBuilder
{
    /// <summary>Builds the JSON request body for the API call.</summary>
    JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider);

    /// <summary>Processes the API response and extracts image data.</summary>
    Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null);

    /// <summary>Gets the endpoint URL for the specific model.</summary>
    string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input);

    /// <summary>Adds authentication headers to the request.</summary>
    void AddAuthHeaders(HttpRequestMessage request, string apiKey, ProviderDefinition provider);
}

/// <summary>Factory for getting the appropriate request builder for a provider.</summary>
public static class RequestBuilderFactory
{
    private static readonly Dictionary<string, IRequestBuilder> _builders = new()
    {
        ["openai_api"] = new OpenAIRequestBuilder(),
        ["ideogram_api"] = new IdeogramRequestBuilder(),
        ["bfl_api"] = new BlackForestRequestBuilder(),
        ["grok_api"] = new GrokRequestBuilder(),
        ["google_api"] = new GoogleRequestBuilder(),
        ["fal_api"] = new FalRequestBuilder()
    };

    /// <summary>Gets the request builder for the specified provider.</summary>
    public static IRequestBuilder GetBuilder(string providerId)
    {
        if (_builders.TryGetValue(providerId, out IRequestBuilder builder))
        {
            return builder;
        }
        throw new ArgumentException($"No request builder found for provider: {providerId}");
    }

    /// <summary>Registers a custom request builder for a provider.</summary>
    public static void RegisterBuilder(string providerId, IRequestBuilder builder)
    {
        _builders[providerId] = builder;
    }
}

/// <summary>Base class with common request building functionality.</summary>
public abstract class BaseRequestBuilder : IRequestBuilder
{
    protected static readonly HttpClient HttpClient = NetworkBackendUtils.MakeHttpClient();
    public abstract JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider);
    public abstract Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null);
    public virtual string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        if (!string.IsNullOrEmpty(model.EndpointOverride))
        {
            return model.EndpointOverride;
        }
        return provider.BaseUrl;
    }

    public virtual void AddAuthHeaders(HttpRequestMessage request, string apiKey, ProviderDefinition provider)
    {
        if (!string.IsNullOrEmpty(provider.CustomAuthHeader))
        {
            request.Headers.TryAddWithoutValidation(provider.CustomAuthHeader, apiKey);
        }
        else
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                provider.AuthHeaderType, apiKey);
        }
    }

    protected static int GetNumImages(T2IParamInput input)
    {
        return input.TryGet(T2IParamTypes.Images, out int num) && num > 0 ? num : 1;
    }

    protected static async Task<byte[]> DownloadImageFromUrl(string url)
    {
        return await HttpClient.GetByteArrayAsync(url);
    }

    protected static byte[] DecodeBase64Image(string base64Data)
    {
        if (base64Data.Contains(','))
        {
            base64Data = base64Data[(base64Data.IndexOf(',') + 1)..];
        }
        return Convert.FromBase64String(base64Data);
    }
}

#region OpenAI Request Builder

public sealed class OpenAIRequestBuilder : BaseRequestBuilder
{
    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        string modelName = model.Id;
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt),
            ["model"] = modelName,
            ["n"] = GetNumImages(input),
            ["size"] = input.TryGet(SwarmUIAPIBackends.SizeParam_OpenAI, out string size) ? size : "1024x1024"
        };
        if (modelName is "gpt-image-1" or "gpt-image-1.5")
        {
            if (input.TryGet(SwarmUIAPIBackends.QualityParam_GPTImage1, out string quality)) request["quality"] = quality;
            if (input.TryGet(SwarmUIAPIBackends.BackgroundParam_GPTImage1, out string bg)) request["background"] = bg;
            if (input.TryGet(SwarmUIAPIBackends.ModerationParam_GPTImage1, out string mod)) request["moderation"] = mod;
            if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_GPTImage1, out string format)) request["output_format"] = format;
            if (input.TryGet(SwarmUIAPIBackends.OutputCompressionParam_GPTImage1, out int compression)) request["output_compression"] = compression;
        }
        else if (modelName is "dall-e-3")
        {
            if (input.TryGet(SwarmUIAPIBackends.QualityParam_OpenAI, out string quality)) request["quality"] = quality;
            if (input.TryGet(SwarmUIAPIBackends.StyleParam_OpenAI, out string style)) request["style"] = style;
            request["response_format"] = "b64_json";
        }
        else
        {
            request["response_format"] = "b64_json";
        }
        return request;
    }

    public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        JArray data = response["data"] as JArray;
        if (data is null || data.Count is 0)
        {
            throw new Exception("No image data in OpenAI response");
        }
        JToken firstImage = data[0];
        if (firstImage["b64_json"] is not null)
        {
            return DecodeBase64Image(firstImage["b64_json"].ToString());
        }
        else if (firstImage["url"] != null)
        {
            return await DownloadImageFromUrl(firstImage["url"].ToString());
        }
        throw new Exception("OpenAI response missing image data");
    }
}

#endregion

#region Ideogram Request Builder

public sealed class IdeogramRequestBuilder : BaseRequestBuilder
{
    private static readonly Dictionary<string, string> LegacyAspectRatioMap = new()
    {
        ["1:1"] = "ASPECT_1_1",
        ["10:16"] = "ASPECT_10_16",
        ["16:10"] = "ASPECT_16_10",
        ["9:16"] = "ASPECT_9_16",
        ["16:9"] = "ASPECT_16_9",
        ["3:2"] = "ASPECT_3_2",
        ["2:3"] = "ASPECT_2_3",
        ["4:3"] = "ASPECT_4_3",
        ["3:4"] = "ASPECT_3_4",
        ["1:3"] = "ASPECT_1_3",
        ["3:1"] = "ASPECT_3_1"
    };

    private static readonly Dictionary<string, string> V3AspectRatioMap = new()
    {
        ["1:1"] = "1x1",
        ["10:16"] = "10x16",
        ["16:10"] = "16x10",
        ["9:16"] = "9x16",
        ["16:9"] = "16x9",
        ["3:2"] = "3x2",
        ["2:3"] = "2x3",
        ["4:3"] = "4x3",
        ["3:4"] = "3x4",
        ["1:3"] = "1x3",
        ["3:1"] = "3x1",
        ["1:2"] = "1x2",
        ["2:1"] = "2x1",
        ["4:5"] = "4x5",
        ["5:4"] = "5x4"
    };

    private static string MapAspectRatioLegacy(string aspect)
    {
        if (string.IsNullOrEmpty(aspect)) return "ASPECT_1_1";
        if (aspect.StartsWith("ASPECT_")) return aspect;
        return LegacyAspectRatioMap.TryGetValue(aspect, out string mapped) ? mapped : "ASPECT_1_1";
    }

    private static string MapAspectRatioV3(string aspect)
    {
        if (string.IsNullOrEmpty(aspect)) return "1x1";
        if (aspect.Contains("x")) return aspect;
        return V3AspectRatioMap.TryGetValue(aspect, out string mapped) ? mapped : "1x1";
    }

    private static bool IsV3Model(ModelDefinition model)
    {
        string id = model.Id?.ToLowerInvariant() ?? "";
        return id.Contains("v_3") || id.Contains("v3");
    }

    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        bool hasInputImage = input.TryGet(SwarmUIAPIBackends.ImagePromptParam_Ideogram, out Image inputImg) && inputImg?.RawData is not null;
        bool isV3 = IsV3Model(model);
        JObject requestBody = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt)
        };
        if (!isV3)
        {
            requestBody["model"] = model.Id;
        }
        if (input.TryGet(SwarmUIAPIBackends.MagicPromptParam_Ideogram, out string magic)) requestBody["magic_prompt_option"] = magic;
        else requestBody["magic_prompt_option"] = "AUTO";
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Ideogram, out string aspect)) requestBody["aspect_ratio"] = isV3 ? MapAspectRatioV3(aspect) : MapAspectRatioLegacy(aspect);
        if (input.TryGet(SwarmUIAPIBackends.StyleTypeParam_Ideogram, out string style) && !string.IsNullOrEmpty(style)) requestBody["style_type"] = style;
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_Ideogram, out string negPrompt) && !string.IsNullOrEmpty(negPrompt)) requestBody["negative_prompt"] = negPrompt;
        if (input.TryGet(T2IParamTypes.Seed, out long seed) && seed >= 0) requestBody["seed"] = (int)seed;
        if (hasInputImage)
        {
            string base64Image = Convert.ToBase64String(inputImg.RawData);
            requestBody["image"] = base64Image;
            if (input.TryGet(SwarmUIAPIBackends.ImageWeightParam_Ideogram, out double weight)) requestBody["image_weight"] = weight;
        }
        if (isV3)
        {
            return requestBody;
        }
        return new JObject { ["image_request"] = requestBody };
    }

    public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        bool hasInputImage = input.TryGet(SwarmUIAPIBackends.ImagePromptParam_Ideogram, out Image inputImg) && inputImg?.RawData != null;
        if (model.Id.Contains("V_3") || model.Id.Contains("v3"))
        {
            return hasInputImage ? "https://api.ideogram.ai/v1/ideogram-v3/edit" : "https://api.ideogram.ai/v1/ideogram-v3/generate";
        }
        return hasInputImage ? "https://api.ideogram.ai/edit" : provider.BaseUrl;
    }

    public override void AddAuthHeaders(HttpRequestMessage request, string apiKey, ProviderDefinition provider)
    {
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
    }

    public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        JArray data = response["data"] as JArray;
        if (data is null || data.Count == 0)
        {
            throw new Exception("No image data in Ideogram response");
        }
        string url = data[0]["url"]?.ToString();
        if (string.IsNullOrEmpty(url))
        {
            throw new Exception("Ideogram response missing image URL");
        }
        return await DownloadImageFromUrl(url);
    }
}

#endregion

#region Black Forest Labs Request Builder

public sealed class BlackForestRequestBuilder : BaseRequestBuilder
{
    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt)
        };
        if (input.TryGet(SwarmUIAPIBackends.WidthParam_BlackForest, out int width)) request["width"] = width;
        if (input.TryGet(SwarmUIAPIBackends.HeightParam_BlackForest, out int height)) request["height"] = height;
        if (input.TryGet(SwarmUIAPIBackends.PromptUpsampling_BlackForest, out bool upsample)) request["prompt_upsampling"] = upsample;
        if (input.TryGet(SwarmUIAPIBackends.SafetyTolerance_BlackForest, out int safety)) request["safety_tolerance"] = safety;
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_BlackForest, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.GuidanceParam_BlackForest, out double guidance)) request["guidance"] = guidance;
        if (input.TryGet(SwarmUIAPIBackends.StepsParam_BlackForest, out int steps)) request["steps"] = steps;
        if (input.TryGet(SwarmUIAPIBackends.IntervalParam_BlackForest, out double interval)) request["interval"] = interval;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_BlackForest, out string format)) request["output_format"] = format;
        return request;
    }

    public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        return $"{provider.BaseUrl}/v1/{model.Id}";
    }

    public override void AddAuthHeaders(HttpRequestMessage request, string apiKey, ProviderDefinition provider)
    {
        request.Headers.TryAddWithoutValidation("x-key", apiKey);
    }

    public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        string pollingUrl = response["polling_url"]?.ToString();
        if (!string.IsNullOrEmpty(pollingUrl) && !string.IsNullOrEmpty(apiKey))
        {
            return await PollForResult(pollingUrl, apiKey);
        }
        string resultUrl = response["result"]?["sample"]?.ToString();
        if (!string.IsNullOrEmpty(resultUrl))
        {
            return await DownloadImageFromUrl(resultUrl);
        }
        if (response["sample"] is not null)
        {
            string sampleUrl = response["sample"].ToString();
            return await DownloadImageFromUrl(sampleUrl);
        }
        throw new Exception($"Black Forest Labs response missing image data. Response: {response}");
    }

    private async Task<byte[]> PollForResult(string pollingUrl, string apiKey)
    {
        int maxAttempts = 120;
        int delayMs = 1000;
        for (int i = 0; i < maxAttempts; i++)
        {
            await Task.Delay(delayMs);
            using HttpRequestMessage pollRequest = new(HttpMethod.Get, pollingUrl);
            pollRequest.Headers.TryAddWithoutValidation("x-key", apiKey);
            pollRequest.Headers.TryAddWithoutValidation("accept", "application/json");
            HttpResponseMessage pollResponse = await HttpClient.SendAsync(pollRequest);
            string content = await pollResponse.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(content);
            string status = result["status"]?.ToString();
            Logs.Verbose($"[BFL] Polling status: {status}");
            if (status is "Ready")
            {
                string sampleUrl = result["result"]?["sample"]?.ToString();
                if (!string.IsNullOrEmpty(sampleUrl))
                {
                    return await DownloadImageFromUrl(sampleUrl);
                }
                throw new Exception("BFL result ready but missing sample URL");
            }
            else if (status is "Error" || status is "Failed")
            {
                throw new Exception($"BFL generation failed: {result}");
            }
        }
        throw new Exception("BFL polling timed out after 120 seconds");
    }
}

#endregion

#region Grok Request Builder

public sealed class GrokRequestBuilder : BaseRequestBuilder
{
    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        return new JObject
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt),
            ["model"] = "grok-2-image",
            ["n"] = GetNumImages(input),
            ["response_format"] = "b64_json"
        };
    }

    public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        JArray data = response["data"] as JArray;
        if (data is null || data.Count is 0)
        {
            throw new Exception("No image data in Grok response");
        }
        JToken firstImage = data[0];
        if (firstImage["b64_json"] is not null)
        {
            return DecodeBase64Image(firstImage["b64_json"].ToString());
        }
        else if (firstImage["url"] is not null)
        {
            return await DownloadImageFromUrl(firstImage["url"].ToString());
        }
        throw new Exception("Grok response missing image data");
    }
}

#endregion

#region Google Request Builder

public sealed class GoogleRequestBuilder : BaseRequestBuilder
{
    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        bool isGemini = model.Id.StartsWith("gemini-");
        if (isGemini)
        {
            return new JObject
            {
                ["contents"] = new JArray
                {
                    new JObject
                    {
                        ["parts"] = new JArray
                        {
                            new JObject { ["text"] = input.Get(T2IParamTypes.Prompt) }
                        }
                    }
                },
                ["generationConfig"] = new JObject
                {
                    ["responseModalities"] = new JArray { "TEXT", "IMAGE" }
                }
            };
        }
        return new JObject
        {
            ["instances"] = new JArray
            {
                new JObject { ["prompt"] = input.Get(T2IParamTypes.Prompt) }
            },
            ["parameters"] = new JObject
            {
                ["sampleCount"] = GetNumImages(input)
            }
        };
    }

    public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        bool isGemini = model.Id.StartsWith("gemini-");
        return isGemini ? $"{provider.BaseUrl}/{model.Id}:generateContent" : $"{provider.BaseUrl}/{model.Id}:predict";
    }

    public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        JArray candidates = response["candidates"] as JArray;
        if (candidates is not null && candidates.Count > 0)
        {
            JArray parts = candidates[0]["content"]?["parts"] as JArray;
            if (parts is not null)
            {
                foreach (JToken part in parts)
                {
                    if (part["inlineData"] is not null)
                    {
                        string base64 = part["inlineData"]["data"]?.ToString();
                        if (!string.IsNullOrEmpty(base64))
                        {
                            return DecodeBase64Image(base64);
                        }
                    }
                }
            }
        }
        JArray predictions = response["predictions"] as JArray;
        if (predictions is not null && predictions.Count > 0)
        {
            string base64 = predictions[0]["bytesBase64Encoded"]?.ToString();
            if (!string.IsNullOrEmpty(base64))
            {
                return DecodeBase64Image(base64);
            }
        }
        throw new Exception("Google response missing image data");
    }
}

#endregion

#region Fal.ai Request Builder

public sealed class FalRequestBuilder : BaseRequestBuilder
{
    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        bool isVideo = model.Id.EndsWith("-t2v") || model.Id.EndsWith("-i2v");
        bool isUtility = model.Id.StartsWith("Utility/");
        if (isVideo)
        {
            return BuildVideoRequest(input, model);
        }
        if (isUtility)
        {
            return BuildUtilityRequest(input, model);
        }
        return BuildImageRequest(input, model);
    }

    private static JObject BuildImageRequest(T2IParamInput input, ModelDefinition model)
    {
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt)
        };
        if (input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Fal, out string imageSize)) request["image_size"] = imageSize;
        else request["image_size"] = "landscape_4_3";
        request["num_images"] = GetNumImages(input);
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.GuidanceScaleParam_Fal, out double guidance)) request["guidance_scale"] = guidance;
        if (input.TryGet(SwarmUIAPIBackends.NumInferenceStepsParam_Fal, out int steps)) request["num_inference_steps"] = steps;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_Fal, out string format)) request["output_format"] = format;
        if (input.TryGet(SwarmUIAPIBackends.SafetyCheckerParam_Fal, out bool safety)) request["enable_safety_checker"] = safety;
        request["sync_mode"] = true;
        return request;
    }

    private static JObject BuildVideoRequest(T2IParamInput input, ModelDefinition model)
    {
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt)
        };
        string modelId = model.Id;
        // Determine model family for parameter handling
        if (modelId.StartsWith("Sora/"))
        {
            BuildSoraVideoParams(input, request);
        }
        else if (modelId.StartsWith("Kling/"))
        {
            BuildKlingVideoParams(input, request);
        }
        else if (modelId.StartsWith("Google/") && modelId.Contains("veo"))
        {
            BuildVeoVideoParams(input, request);
        }
        else if (modelId.StartsWith("Luma/"))
        {
            BuildLumaVideoParams(input, request);
        }
        else if (modelId.StartsWith("MiniMax/"))
        {
            BuildMiniMaxVideoParams(input, request);
        }
        else if (modelId.StartsWith("Hunyuan/") && modelId.Contains("video"))
        {
            BuildHunyuanVideoParams(input, request);
        }
        else if (modelId.StartsWith("Grok/") && modelId.Contains("video"))
        {
            BuildGrokVideoParams(input, request);
        }
        else
        {
            // Generic video params for other models (Wan, Pika, PixVerse, Vidu, LTX, Mochi, etc.)
            BuildGenericVideoParams(input, request);
        }
        return request;
    }

    /// <summary>Sora 2: duration (int: 4,8,12), aspect_ratio (16:9,9:16), resolution (720p,1080p). NO: generate_audio, negative_prompt, seed</summary>
    private static void BuildSoraVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_Sora, out string duration) && int.TryParse(duration, out int durationInt))
        {
            request["duration"] = durationInt;
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Sora, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Sora, out string resolution))
        {
            request["resolution"] = resolution;
        }
    }

    /// <summary>Kling: duration (string: 3-15), aspect_ratio (16:9,9:16,1:1), generate_audio, negative_prompt. NO: seed, resolution</summary>
    private static void BuildKlingVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_Kling, out string duration))
        {
            request["duration"] = duration;
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Kling, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_Kling, out bool genAudio))
        {
            request["generate_audio"] = genAudio;
        }
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_Kling, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
        {
            request["negative_prompt"] = negPrompt;
        }
    }

    /// <summary>Veo 3: duration (string: 4s,6s,8s), aspect_ratio (16:9,9:16), resolution (720p,1080p), generate_audio, negative_prompt, seed</summary>
    private static void BuildVeoVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_Veo, out string duration))
        {
            request["duration"] = duration;
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Veo, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Veo, out string resolution))
        {
            request["resolution"] = resolution;
        }
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_Veo, out bool genAudio))
        {
            request["generate_audio"] = genAudio;
        }
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_Veo, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
        {
            request["negative_prompt"] = negPrompt;
        }
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0)
        {
            request["seed"] = seed;
        }
    }

    /// <summary>Luma Ray 2: duration (string: 5s,9s), aspect_ratio (many), resolution (540p,720p,1080p). NO: generate_audio, negative_prompt, seed</summary>
    private static void BuildLumaVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_Luma, out string duration))
        {
            request["duration"] = duration;
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Luma, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Luma, out string resolution))
        {
            request["resolution"] = resolution;
        }
    }

    /// <summary>MiniMax Hailuo: duration (string: 6,10). NO: aspect_ratio, resolution, generate_audio, negative_prompt, seed</summary>
    private static void BuildMiniMaxVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_MiniMax, out string duration))
        {
            request["duration"] = duration;
        }
    }

    /// <summary>Hunyuan Video: aspect_ratio (16:9,9:16), resolution (480p,580p,720p), seed. NO: duration, generate_audio, negative_prompt</summary>
    private static void BuildHunyuanVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Hunyuan, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Hunyuan, out string resolution))
        {
            request["resolution"] = resolution;
        }
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0)
        {
            request["seed"] = seed;
        }
    }

    /// <summary>Grok Imagine Video: duration, aspect_ratio, generate_audio, negative_prompt, seed</summary>
    private static void BuildGrokVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_FalVideo, out string duration))
        {
            if (int.TryParse(duration, out int durationInt))
            {
                request["duration"] = durationInt;
            }
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_FalVideo, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_FalVideo, out bool genAudio))
        {
            request["generate_audio"] = genAudio;
        }
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_FalVideo, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
        {
            request["negative_prompt"] = negPrompt;
        }
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0)
        {
            request["seed"] = seed;
        }
    }

    /// <summary>Generic video params for models without specific handling (Wan, Pika, PixVerse, Vidu, LTX, Mochi, CogVideoX, etc.)</summary>
    private static void BuildGenericVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_FalVideo, out string duration))
        {
            // Try to parse as int first, otherwise send as string
            if (int.TryParse(duration, out int durationInt))
            {
                request["duration"] = durationInt;
            }
            else
            {
                request["duration"] = duration;
            }
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_FalVideo, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_FalVideo, out string resolution))
        {
            request["resolution"] = resolution;
        }
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_FalVideo, out bool genAudio))
        {
            request["generate_audio"] = genAudio;
        }
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_FalVideo, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
        {
            request["negative_prompt"] = negPrompt;
        }
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0)
        {
            request["seed"] = seed;
        }
    }

    private static JObject BuildUtilityRequest(T2IParamInput input, ModelDefinition model)
    {
        JObject request = new();
        request["sync_mode"] = true;
        return request;
    }

    public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        string path = !string.IsNullOrEmpty(model.EndpointOverride) ? model.EndpointOverride : model.Id;
        return $"{provider.BaseUrl}/{path}";
    }

    public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        // Handle image responses (most text-to-image models)
        JArray images = response["images"] as JArray;
        if (images is not null && images.Count > 0)
        {
            JToken firstImage = images[0];
            string url = firstImage["url"]?.ToString();
            if (!string.IsNullOrEmpty(url))
            {
                if (url.StartsWith("data:"))
                {
                    return DecodeBase64Image(url);
                }
                return await DownloadImageFromUrl(url);
            }
            string base64 = firstImage["base64"]?.ToString();
            if (!string.IsNullOrEmpty(base64))
            {
                return DecodeBase64Image(base64);
            }
        }
        // Handle single image response (some models return {image: {url: ...}})
        JToken imageObj = response["image"];
        if (imageObj is not null)
        {
            string imageUrl = imageObj["url"]?.ToString();
            if (!string.IsNullOrEmpty(imageUrl))
            {
                return await DownloadImageFromUrl(imageUrl);
            }
        }
        // Handle video responses (video generation models)
        JToken video = response["video"];
        if (video is not null)
        {
            string videoUrl = video["url"]?.ToString();
            if (!string.IsNullOrEmpty(videoUrl))
            {
                return await DownloadImageFromUrl(videoUrl);
            }
        }
        // Handle output array (some utility models)
        JToken output = response["output"];
        if (output is JArray outputArr && outputArr.Count > 0)
        {
            string outputUrl = outputArr[0]["url"]?.ToString();
            if (!string.IsNullOrEmpty(outputUrl))
            {
                return await DownloadImageFromUrl(outputUrl);
            }
        }
        throw new Exception($"Fal.ai response missing image/video data. Response keys: {string.Join(", ", response.Properties().Select(p => p.Name))}");
    }
}

#endregion
