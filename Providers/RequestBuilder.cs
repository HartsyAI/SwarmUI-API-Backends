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
    Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null);

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
    public abstract Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null);
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

    /// <summary>How many images this single API call should return.
    /// Swarm's Images param is the number of separate generation calls it will make - it already loops and
    /// issues one backend call per image - so using it here would multiply the bill by asking each of those
    /// calls for that many images again. BatchSize is the per-call count.</summary>
    protected static int GetNumImages(T2IParamInput input)
    {
        return input.TryGet(T2IParamTypes.BatchSize, out int num) && num > 0 ? num : 1;
    }

    protected static async Task<byte[]> DownloadImageFromUrl(string url)
    {
        return await HttpClient.GetByteArrayAsync(url);
    }

    /// <summary>Splits a comma-separated URL list into a JSON array, or null if empty.</summary>
    protected static JArray UrlList(T2IParamInput input, T2IRegisteredParam<string> param)
    {
        if (!input.TryGet(param, out string raw) || string.IsNullOrEmpty(raw))
        {
            return null;
        }
        JArray urls = [];
        foreach (string url in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            urls.Add(url);
        }
        return urls.Count > 0 ? urls : null;
    }

    /// <summary>Pulls every image out of a provider's result array, taking base64 where offered and
    /// downloading otherwise. Providers return one entry per image requested.</summary>
    protected static async Task<byte[][]> CollectImages(JArray entries, string base64Key, string urlKey, string providerName)
    {
        List<byte[]> results = [];
        foreach (JToken entry in entries)
        {
            string base64 = base64Key is null ? null : entry[base64Key]?.ToString();
            if (!string.IsNullOrEmpty(base64))
            {
                results.Add(DecodeBase64Image(base64));
                continue;
            }
            string url = entry[urlKey]?.ToString();
            if (!string.IsNullOrEmpty(url))
            {
                results.Add(url.StartsWith("data:") ? DecodeBase64Image(url) : await DownloadImageFromUrl(url));
            }
        }
        if (results.Count == 0)
        {
            throw new Exception($"{providerName} response carried no usable image data");
        }
        return [.. results];
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
    private const string OpenAIVideoBaseUrl = "https://api.openai.com/v1/videos";

    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        string modelName = model.Id;
        // Sora video models use different request structure
        if (IsSoraModel(modelName))
        {
            return BuildSoraVideoRequest(input, modelName);
        }
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt),
            ["model"] = modelName,
            // DALL-E 3 rejects any n above 1; the others accept a real batch.
            ["n"] = modelName == "dall-e-3" ? 1 : GetNumImages(input),
            ["size"] = SizeForOpenAIModel(input, modelName)
        };
        if (modelName is "gpt-image-1" or "gpt-image-1.5" or "gpt-image-2")
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

    /// <summary>Each OpenAI image model accepts a different size list, so each has its own param.</summary>
    private static string SizeForOpenAIModel(T2IParamInput input, string modelName)
    {
        T2IRegisteredParam<string> param = modelName switch
        {
            "dall-e-2" => SwarmUIAPIBackends.SizeParam_DallE2,
            "gpt-image-2" => SwarmUIAPIBackends.SizeParam_GPTImage2,
            "gpt-image-1" or "gpt-image-1.5" => SwarmUIAPIBackends.SizeParam_GPTImage,
            _ => SwarmUIAPIBackends.SizeParam_OpenAI
        };
        return input.TryGet(param, out string size) ? size : "1024x1024";
    }

    private static bool IsSoraModel(string modelName) => modelName.StartsWith("sora-");

    private static JObject BuildSoraVideoRequest(T2IParamInput input, string modelName)
    {
        // Map model IDs to API model names (sora-2-t2v -> sora-2, sora-2-pro-t2v -> sora-2-pro)
        string apiModel = modelName.Replace("-t2v", "").Replace("-i2v", "");
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt),
            ["model"] = apiModel
        };
        if (input.TryGet(SwarmUIAPIBackends.SizeParam_OpenAISora, out string size))
        {
            request["size"] = size;
        }
        if (input.TryGet(SwarmUIAPIBackends.SecondsParam_OpenAISora, out int seconds))
        {
            request["seconds"] = seconds;
        }
        return request;
    }

    public override async Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        // Check if this is a Sora video response (has "id" and "status" fields)
        if (response["id"] != null && response["status"] != null)
        {
            return [await ProcessSoraVideoResponse(response, apiKey)];
        }
        JArray data = response["data"] as JArray;
        if (data is null || data.Count is 0)
        {
            throw new Exception("No image data in OpenAI response");
        }
        return await CollectImages(data, "b64_json", "url", "OpenAI");
    }

    private async Task<byte[]> ProcessSoraVideoResponse(JObject initialResponse, string apiKey)
    {
        string videoId = initialResponse["id"]?.ToString();
        if (string.IsNullOrEmpty(videoId))
        {
            throw new Exception("OpenAI Sora response missing video ID");
        }
        string status = initialResponse["status"]?.ToString();
        int maxAttempts = 120; // 10 minutes max (5 second intervals)
        int attempts = 0;
        // Poll for completion
        while (status is "queued" or "in_progress" && attempts < maxAttempts)
        {
            await Task.Delay(5000); // Wait 5 seconds between polls
            attempts++;
            using HttpRequestMessage pollRequest = new(HttpMethod.Get, $"{OpenAIVideoBaseUrl}/{videoId}");
            pollRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            using HttpResponseMessage pollResponse = await HttpClient.SendAsync(pollRequest);
            string pollContent = await pollResponse.Content.ReadAsStringAsync();
            if (!pollResponse.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI Sora polling failed: {pollContent}");
            }
            JObject statusResponse = JObject.Parse(pollContent);
            status = statusResponse["status"]?.ToString();
            if (status == "failed")
            {
                string error = statusResponse["error"]?.ToString() ?? "Unknown error";
                throw new Exception($"OpenAI Sora video generation failed: {error}");
            }
            Logs.Verbose($"[OpenAI Sora] Video {videoId} status: {status}, progress: {statusResponse["progress"]}%");
        }
        if (status != "completed")
        {
            throw new Exception($"OpenAI Sora video generation timed out or failed. Status: {status}");
        }
        // Download the completed video
        using HttpRequestMessage downloadRequest = new(HttpMethod.Get, $"{OpenAIVideoBaseUrl}/{videoId}/content");
        downloadRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        using HttpResponseMessage downloadResponse = await HttpClient.SendAsync(downloadRequest);
        if (!downloadResponse.IsSuccessStatusCode)
        {
            string error = await downloadResponse.Content.ReadAsStringAsync();
            throw new Exception($"OpenAI Sora video download failed: {error}");
        }
        byte[] videoData = await downloadResponse.Content.ReadAsByteArrayAsync();
        Logs.Verbose($"[OpenAI Sora] Downloaded video {videoId}, size: {videoData.Length} bytes");
        return videoData;
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

    private static bool IsV4Model(ModelDefinition model)
    {
        string id = model.Id?.ToLowerInvariant() ?? "";
        return id.Contains("v_4") || id.Contains("v4");
    }

    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        if (IsV4Model(model))
        {
            JObject v4 = new()
            {
                ["text_prompt"] = input.Get(T2IParamTypes.Prompt)
            };
            if (input.TryGet(SwarmUIAPIBackends.RenderingSpeedParam_IdeogramV4, out string v4speed) && !string.IsNullOrEmpty(v4speed))
            {
                v4["rendering_speed"] = v4speed;
            }
            if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_IdeogramV4, out string v4res) && !string.IsNullOrEmpty(v4res))
            {
                v4["resolution"] = v4res;
            }
            if (input.TryGet(SwarmUIAPIBackends.CopyrightDetectionParam_IdeogramV4, out bool v4copy))
            {
                v4["enable_copyright_detection"] = v4copy;
            }
            return v4;
        }
        bool isV3 = IsV3Model(model);
        bool hasInitImage = input.TryGet(T2IParamTypes.InitImage, out Image initImg) && initImg?.RawData is not null;
        bool hasMask = input.TryGet(T2IParamTypes.MaskImage, out Image maskImg) && maskImg?.RawData is not null;
        JObject requestBody = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt)
        };
        if (!isV3)
        {
            requestBody["model"] = model.Id;
        }
        // Common params
        if (input.TryGet(SwarmUIAPIBackends.MagicPromptParam_Ideogram, out string magic)) requestBody["magic_prompt_option"] = magic;
        else requestBody["magic_prompt_option"] = "AUTO";
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Ideogram, out string aspect)) requestBody["aspect_ratio"] = isV3 ? MapAspectRatioV3(aspect) : MapAspectRatioLegacy(aspect);
        if (input.TryGet(SwarmUIAPIBackends.StyleTypeParam_Ideogram, out string style) && !string.IsNullOrEmpty(style)) requestBody["style_type"] = style;
        if (input.TryGet(T2IParamTypes.Seed, out long seed) && seed >= 0) requestBody["seed"] = (int)seed;
        // Negative prompt: legacy models only (V3 doesn't support it)
        if (!isV3 && input.TryGet(SwarmUIAPIBackends.NegativePromptParam_Ideogram, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
            requestBody["negative_prompt"] = negPrompt;
        // Color palette (V2+ only, handled by feature flag)
        if (input.TryGet(SwarmUIAPIBackends.ColorPaletteParam_Ideogram, out string palette) && !string.IsNullOrEmpty(palette) && palette != "None")
            requestBody["color_palette"] = new JObject { ["name"] = palette };
        // V3-specific params
        if (isV3)
        {
            if (input.TryGet(SwarmUIAPIBackends.RenderingSpeedParam_Ideogram, out string speed)) requestBody["rendering_speed"] = speed;
        }
        // Image input: use core Swarm InitImage
        if (hasInitImage)
        {
            string base64Image = Convert.ToBase64String(initImg.RawData);
            requestBody["image"] = base64Image;
            if (hasMask)
            {
                requestBody["mask"] = Convert.ToBase64String(maskImg.RawData);
            }
            // Image weight: V3 remix only (scale 0-1 to 1-100)
            if (isV3 && !hasMask && input.TryGet(SwarmUIAPIBackends.ImageWeightParam_Ideogram, out double weight))
                requestBody["image_weight"] = (int)Math.Round(weight * 100);
        }
        if (isV3)
        {
            return requestBody;
        }
        return new JObject { ["image_request"] = requestBody };
    }

    public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        bool hasInitImage = input.TryGet(T2IParamTypes.InitImage, out Image initImg) && initImg?.RawData is not null;
        bool hasMask = input.TryGet(T2IParamTypes.MaskImage, out Image maskImg) && maskImg?.RawData is not null;
        if (IsV4Model(model))
        {
            return "https://api.ideogram.ai/v1/ideogram-v4/generate";
        }
        if (IsV3Model(model))
        {
            if (hasInitImage && hasMask) return "https://api.ideogram.ai/v1/ideogram-v3/edit";
            if (hasInitImage) return "https://api.ideogram.ai/v1/ideogram-v3/remix";
            return "https://api.ideogram.ai/v1/ideogram-v3/generate";
        }
        return hasInitImage ? "https://api.ideogram.ai/edit" : provider.BaseUrl;
    }

    public override void AddAuthHeaders(HttpRequestMessage request, string apiKey, ProviderDefinition provider)
    {
        request.Headers.TryAddWithoutValidation("Api-Key", apiKey);
    }

    public override async Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        JArray data = response["data"] as JArray;
        if (data is null || data.Count == 0)
        {
            throw new Exception("No image data in Ideogram response");
        }
        return await CollectImages(data, null, "url", "Ideogram");
    }
}

#endregion

#region Black Forest Labs Request Builder

public sealed class BlackForestRequestBuilder : BaseRequestBuilder
{
    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        if (model.Family == "video.bfl_flux3")
        {
            return BuildFlux3DirectRequest(input);
        }
        string modelId = model.Id;
        bool usesAspectRatio = modelId is "flux-pro-1.1-ultra" or "flux-kontext-pro" or "flux-kontext-max";
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt)
        };
        // Size: aspect_ratio for ultra/kontext, width+height for others
        if (usesAspectRatio)
        {
            if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_BlackForest, out string aspectRatio) && aspectRatio != "Custom")
                request["aspect_ratio"] = aspectRatio;
        }
        else
        {
            if (input.TryGet(SwarmUIAPIBackends.WidthParam_BlackForest, out int width)) request["width"] = width;
            if (input.TryGet(SwarmUIAPIBackends.HeightParam_BlackForest, out int height)) request["height"] = height;
        }
        // Common params
        if (input.TryGet(SwarmUIAPIBackends.SafetyTolerance_BlackForest, out int safety)) request["safety_tolerance"] = safety;
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_BlackForest, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_BlackForest, out string format)) request["output_format"] = format;
        // Guidance and steps: flux-dev and flux-2-flex both expose them
        if (modelId is "flux-dev" or "flux-2-flex")
        {
            if (input.TryGet(SwarmUIAPIBackends.GuidanceParam_BlackForest, out double guidance)) request["guidance"] = guidance;
            if (input.TryGet(SwarmUIAPIBackends.StepsParam_BlackForest, out int steps)) request["steps"] = steps;
        }
        // Prompt upsampling: all except flux-2-* models
        if (!modelId.StartsWith("flux-2-"))
        {
            if (input.TryGet(SwarmUIAPIBackends.PromptUpsampling_BlackForest, out bool upsample)) request["prompt_upsampling"] = upsample;
        }
        // Raw mode: ultra only
        if (modelId == "flux-pro-1.1-ultra")
        {
            if (input.TryGet(SwarmUIAPIBackends.RawModeParam_BlackForest, out bool raw)) request["raw"] = raw;
        }
        // Image input: use core Swarm InitImage, sent as image_prompt or input_image depending on model
        if (input.TryGet(T2IParamTypes.InitImage, out Image initImg) && initImg?.RawData is not null)
        {
            string base64Image = Convert.ToBase64String(initImg.RawData);
            if (modelId is "flux-kontext-pro" or "flux-kontext-max" or "flux-2-pro" or "flux-2-max" or "flux-2-flex")
            {
                request["input_image"] = base64Image;
                // FLUX.2 also accepts input_image_2..8; extra references come from the shared reference URL param.
                if (modelId.StartsWith("flux-2-") && UrlList(input, SwarmUIAPIBackends.RefImageUrlsParam) is JArray extras)
                {
                    for (int i = 0; i < extras.Count && i < 7; i++)
                    {
                        request[$"input_image_{i + 2}"] = extras[i];
                    }
                }
            }
            else
            {
                request["image_prompt"] = base64Image;
            }
        }
        // Image prompt strength: ultra only (controls blend between prompt and image)
        if (modelId == "flux-pro-1.1-ultra")
        {
            if (input.TryGet(SwarmUIAPIBackends.ImagePromptStrengthParam_BlackForest, out double strength)) request["image_prompt_strength"] = strength;
        }
        return request;
    }

    /// <summary>FLUX 3 on BFL's own API is a discriminated union on 'mode'. Supplying an Init Image switches
    /// from text-to-video to image-continuation, where the image becomes the first keyframe.
    /// There is no seed field in this schema.</summary>
    private static JObject BuildFlux3DirectRequest(T2IParamInput input)
    {
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt),
            ["version"] = "latest"
        };
        bool hasImage = input.TryGet(T2IParamTypes.InitImage, out Image initImg) && initImg?.RawData is not null;
        if (hasImage)
        {
            request["mode"] = "i2v";
            request["keyframes"] = new JArray($"data:image/png;base64,{Convert.ToBase64String(initImg.RawData)}");
        }
        else
        {
            request["mode"] = "t2v";
        }
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_Flux3, out string duration))
        {
            // 'auto' stays a string; a concrete length is an integer in this schema.
            request["duration"] = int.TryParse(duration, out int seconds) ? seconds : duration;
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Flux3, out string aspect)) request["aspect_ratio"] = aspect;
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Flux3Bfl, out string resolution)) request["resolution"] = resolution;
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_FalVideo, out bool audio)) request["generate_audio"] = audio;
        if (input.TryGet(SwarmUIAPIBackends.SafetyToleranceParam_Flux3, out int safety)) request["safety_tolerance"] = safety;
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

    public override async Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        string pollingUrl = response["polling_url"]?.ToString();
        if (!string.IsNullOrEmpty(pollingUrl) && !string.IsNullOrEmpty(apiKey))
        {
            return [await PollForResult(pollingUrl, apiKey)];
        }
        string resultUrl = response["result"]?["sample"]?.ToString();
        if (!string.IsNullOrEmpty(resultUrl))
        {
            return [await DownloadImageFromUrl(resultUrl)];
        }
        if (response["sample"] is not null)
        {
            string sampleUrl = response["sample"].ToString();
            return [await DownloadImageFromUrl(sampleUrl)];
        }
        throw new Exception($"Black Forest Labs response missing image data. Response: {response}");
    }

    private async Task<byte[]> PollForResult(string pollingUrl, string apiKey)
    {
        // Video jobs run for minutes, far past the 2 minutes the original image-only budget allowed.
        int maxAttempts = 450;
        int delayMs = 2000;
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
                // Images come back under 'sample'; video jobs use a video key instead.
                JToken payload = result["result"];
                string sampleUrl = (payload?["sample"] ?? payload?["video"] ?? payload?["video_url"])?.ToString();
                if (!string.IsNullOrEmpty(sampleUrl))
                {
                    return await DownloadImageFromUrl(sampleUrl);
                }
                throw new Exception($"BFL result ready but carried no downloadable URL. Result: {payload}");
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
        JObject request = new()
        {
            ["prompt"] = input.Get(T2IParamTypes.Prompt),
            ["model"] = model.Id,
            ["n"] = GetNumImages(input),
            ["response_format"] = "b64_json"
        };
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Grok, out string aspect) && !string.IsNullOrEmpty(aspect))
            request["aspect_ratio"] = aspect;
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Grok, out string resolution) && !string.IsNullOrEmpty(resolution))
            request["resolution"] = resolution;
        // Image editing: use core Swarm InitImage
        if (input.TryGet(T2IParamTypes.InitImage, out Image initImg) && initImg?.RawData is not null)
        {
            string base64Image = Convert.ToBase64String(initImg.RawData);
            request["image_url"] = $"data:image/png;base64,{base64Image}";
        }
        return request;
    }

    public override async Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        JArray data = response["data"] as JArray;
        if (data is null || data.Count is 0)
        {
            throw new Exception("No image data in Grok response");
        }
        return await CollectImages(data, "b64_json", "url", "Grok");
    }
}

#endregion

#region Google Request Builder

public sealed class GoogleRequestBuilder : BaseRequestBuilder
{
    /// <summary>Checks if a model is a Gemini 3 series model that supports image_size parameter.</summary>
    private static bool IsGemini3Model(string modelId)
    {
        return modelId is "gemini-3.1-flash-image-preview" or "gemini-3-pro-image-preview";
    }

    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        bool isGemini = model.Id.StartsWith("gemini-");
        if (isGemini)
        {
            JArray parts = new() { new JObject { ["text"] = input.Get(T2IParamTypes.Prompt) } };
            // Image editing: send init image as inline data in contents
            if (input.TryGet(T2IParamTypes.InitImage, out Image initImg) && initImg?.RawData is not null)
            {
                string base64Image = Convert.ToBase64String(initImg.RawData);
                parts.Insert(0, new JObject
                {
                    ["inlineData"] = new JObject
                    {
                        ["mimeType"] = "image/png",
                        ["data"] = base64Image
                    }
                });
            }
            JObject genConfig = new()
            {
                ["responseModalities"] = new JArray { "TEXT", "IMAGE" }
            };
            // Build image_config with aspect_ratio and optional image_size
            JObject imageConfig = new();
            bool hasImageConfig = false;
            if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Google, out string geminiAspect) && !string.IsNullOrEmpty(geminiAspect))
            {
                imageConfig["aspect_ratio"] = geminiAspect;
                hasImageConfig = true;
            }
            // image_size: only supported by Gemini 3 models 
            if (IsGemini3Model(model.Id) && input.TryGet(SwarmUIAPIBackends.ImageSizeParam_GoogleGemini3, out string imageSize) && !string.IsNullOrEmpty(imageSize))
            {
                imageConfig["image_size"] = imageSize;
                hasImageConfig = true;
            }
            if (hasImageConfig)
            {
                genConfig["image_config"] = imageConfig;
            }
            return new JObject
            {
                ["contents"] = new JArray { new JObject { ["parts"] = parts } },
                ["generationConfig"] = genConfig
            };
        }
        // Imagen
        JObject parameters = new() { ["sampleCount"] = GetNumImages(input) };
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Google, out string aspect) && !string.IsNullOrEmpty(aspect))
            parameters["aspectRatio"] = aspect;
        if (input.TryGet(SwarmUIAPIBackends.PersonGenerationParam_Google, out string person) && !string.IsNullOrEmpty(person))
            parameters["personGeneration"] = person;
        if (input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Google, out string imagenImageSize) && !string.IsNullOrEmpty(imagenImageSize))
            parameters["imageSize"] = imagenImageSize;
        return new JObject
        {
            ["instances"] = new JArray
            {
                new JObject { ["prompt"] = input.Get(T2IParamTypes.Prompt) }
            },
            ["parameters"] = parameters
        };
    }

    public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        bool isGemini = model.Id.StartsWith("gemini-");
        return isGemini ? $"{provider.BaseUrl}/{model.Id}:generateContent" : $"{provider.BaseUrl}/{model.Id}:predict";
    }

    public override async Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        JArray candidates = response["candidates"] as JArray;
        if (candidates is not null && candidates.Count > 0)
        {
            List<byte[]> parsed = [];
            foreach (JToken candidate in candidates)
            {
                if (candidate["content"]?["parts"] is not JArray parts)
                {
                    continue;
                }
                foreach (JToken part in parts)
                {
                    string base64 = part["inlineData"]?["data"]?.ToString();
                    if (!string.IsNullOrEmpty(base64))
                    {
                        parsed.Add(DecodeBase64Image(base64));
                    }
                }
            }
            if (parsed.Count > 0)
            {
                return [.. parsed];
            }
        }
        JArray predictions = response["predictions"] as JArray;
        if (predictions is not null && predictions.Count > 0)
        {
            List<byte[]> parsed = [];
            foreach (JToken prediction in predictions)
            {
                string base64 = prediction["bytesBase64Encoded"]?.ToString();
                if (!string.IsNullOrEmpty(base64))
                {
                    parsed.Add(DecodeBase64Image(base64));
                }
            }
            if (parsed.Count > 0)
            {
                return [.. parsed];
            }
        }
        throw new Exception("Google response missing image data");
    }
}

#endregion

#region Fal.ai Request Builder

public sealed class FalRequestBuilder : BaseRequestBuilder
{
    /// <summary>Fills in the params for one model family. Families are declared on the model, never inferred from its name.</summary>
    private delegate void FamilyBuilder(T2IParamInput input, JObject request, ModelDefinition model);

    private static readonly Dictionary<string, FamilyBuilder> Families = new()
    {
        ["image.standard"] = (i, r, m) => BuildStandardImageParams(i, r),
        ["image.flux2"] = (i, r, m) => BuildFlux2ImageParams(i, r),
        ["image.qwen2"] = (i, r, m) => BuildQwen2ImageParams(i, r),
        ["image.zimage"] = (i, r, m) => BuildZImageParams(i, r),
        ["image.nanobanana2"] = (i, r, m) => BuildNanoBanana2Params(i, r),
        ["image.aspect"] = (i, r, m) => BuildAspectRatioImageParams(i, r, m.Id),
        ["image.recraft"] = (i, r, m) => BuildRecraftImageParams(i, r),
        ["image.bria"] = (i, r, m) => BuildBriaImageParams(i, r),
        ["video.sora"] = (i, r, m) => BuildSoraVideoParams(i, r),
        ["video.kling"] = (i, r, m) => BuildKlingVideoParams(i, r),
        ["video.veo"] = (i, r, m) => BuildVeoVideoParams(i, r),
        ["video.luma"] = (i, r, m) => BuildLumaVideoParams(i, r),
        ["video.minimax"] = (i, r, m) => BuildMiniMaxVideoParams(i, r),
        ["video.hunyuan"] = (i, r, m) => BuildHunyuanVideoParams(i, r),
        ["video.grok"] = (i, r, m) => BuildGrokVideoParams(i, r),
        ["video.seedance1"] = (i, r, m) => BuildSeedance1VideoParams(i, r),
        ["video.seedance2"] = (i, r, m) =>
        {
            BuildSeedance2VideoParams(i, r);
            if (m.ExtraFlags.Contains("fal_seedance_ref_params"))
            {
                AddReferenceUrls(i, r, "image_urls", "video_urls", "audio_urls");
            }
        },
        ["video.wan22"] = (i, r, m) => BuildWan22VideoParams(i, r),
        ["video.pixverse"] = (i, r, m) => BuildPixVerseVideoParams(i, r),
        ["video.ltx2"] = (i, r, m) => BuildLtx2VideoParams(i, r),
        ["video.ltx13b"] = (i, r, m) => BuildLtx13bVideoParams(i, r),
        ["video.vidu"] = (i, r, m) => BuildViduVideoParams(i, r),
        ["video.pika"] = (i, r, m) => BuildPikaVideoParams(i, r),
        ["video.kandinsky"] = (i, r, m) => BuildKandinskyVideoParams(i, r),
        ["video.cogvideox"] = (i, r, m) => BuildCogVideoXParams(i, r),
        ["video.wan25"] = (i, r, m) => BuildWan25VideoParams(i, r),
        ["video.wan26"] = (i, r, m) => BuildWan26VideoParams(i, r),
        ["video.wan27"] = (i, r, m) => BuildWan27VideoParams(i, r, aspect: true, endImage: false),
        ["video.wan27_i2v"] = (i, r, m) => BuildWan27VideoParams(i, r, aspect: false, endImage: true),
        ["video.wan27_ref"] = (i, r, m) => BuildWan27RefVideoParams(i, r),
        ["video.flux3"] = (i, r, m) => BuildFlux3VideoParams(i, r),
        ["video.h3"] = (i, r, m) => BuildH3VideoParams(i, r, aspect: SwarmUIAPIBackends.AspectRatioParam_H3, endImage: false, refs: false),
        ["video.h3_i2v"] = (i, r, m) => BuildH3VideoParams(i, r, aspect: null, endImage: true, refs: false),
        ["video.h3_ref"] = (i, r, m) => BuildH3VideoParams(i, r, aspect: SwarmUIAPIBackends.AspectRatioParam_H3Ref, endImage: false, refs: true),
        ["video.kling_turbo"] = (i, r, m) => BuildKlingTurboVideoParams(i, r),
        ["video.seedance25"] = (i, r, m) => BuildSeedance25VideoParams(i, r, m, endImage: false),
        ["video.seedance25_i2v"] = (i, r, m) => BuildSeedance25VideoParams(i, r, m, endImage: true),
        ["utility.image"] = (i, r, m) => BuildUtilityImageParams(i, r),
        ["utility.video"] = (i, r, m) => BuildUtilityVideoParams(i, r)
    };

    public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
    {
        if (!Families.TryGetValue(model.Family, out FamilyBuilder buildFamily))
        {
            throw new Exception($"Fal model '{model.Id}' declares unknown param family '{model.Family}'");
        }
        JObject request = [];
        if (model.Modality != ModelModality.Utility)
        {
            request["prompt"] = input.Get(T2IParamTypes.Prompt);
        }
        AttachInputMedia(input, request, model);
        buildFamily(input, request, model);
        // Videos are polled rather than returned inline, so sync_mode only applies to image/utility results.
        if (model.Modality != ModelModality.Video)
        {
            request["sync_mode"] = true;
        }
        return request;
    }

    /// <summary>Attaches the user's init/reference image, in whichever shape the family expects.</summary>
    private static void AttachInputMedia(T2IParamInput input, JObject request, ModelDefinition model)
    {
        if (!model.SupportsInitImage || model.Family == "utility.video")
        {
            return;
        }
        if (!input.TryGet(T2IParamTypes.InitImage, out Image img) || img?.RawData is null)
        {
            return;
        }
        string dataUrl = $"data:image/png;base64,{Convert.ToBase64String(img.RawData)}";
        request["image_url"] = dataUrl;
        if (model.Modality == ModelModality.Image)
        {
            request["image_urls"] = new JArray(dataUrl);
        }
    }

    /// <summary>Standard Fal image params: image_size, guidance, steps, seed, safety_checker, output_format, negative_prompt.</summary>
    private static void BuildStandardImageParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Fal, out string imageSize)) request["image_size"] = imageSize;
        else request["image_size"] = "landscape_4_3";
        request["num_images"] = GetNumImages(input);
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.GuidanceScaleParam_Fal, out double guidance)) request["guidance_scale"] = guidance;
        if (input.TryGet(SwarmUIAPIBackends.NumInferenceStepsParam_Fal, out int steps)) request["num_inference_steps"] = steps;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_Fal, out string format)) request["output_format"] = format;
        if (input.TryGet(SwarmUIAPIBackends.SafetyCheckerParam_Fal, out bool safety)) request["enable_safety_checker"] = safety;
        // Negative prompt: SD, HiDream, Qwen, Sana, Lumina, Kolors, Playground
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_FalImage, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
            request["negative_prompt"] = negPrompt;
    }

    /// <summary>FLUX.2 image models: image_size, seed, output_format, safety. No batch, steps, guidance
    /// or negative prompt - the endpoint declares none of them.</summary>
    private static void BuildFlux2ImageParams(T2IParamInput input, JObject request)
    {
        request["image_size"] = input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Fal, out string size) ? size : "landscape_4_3";
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_Fal, out string format)) request["output_format"] = format;
        if (input.TryGet(SwarmUIAPIBackends.SafetyCheckerParam_Fal, out bool safe)) request["enable_safety_checker"] = safe;
        if (input.TryGet(SwarmUIAPIBackends.SafetyToleranceParam_Flux2, out int tolerance)) request["safety_tolerance"] = tolerance;
    }

    /// <summary>Qwen Image 2.0: image_size, batch, seed, negative prompt, prompt expansion. No steps or guidance.</summary>
    private static void BuildQwen2ImageParams(T2IParamInput input, JObject request)
    {
        request["image_size"] = input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Fal, out string size) ? size : "square_hd";
        request["num_images"] = GetNumImages(input);
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_Fal, out string format)) request["output_format"] = format;
        if (input.TryGet(SwarmUIAPIBackends.SafetyCheckerParam_Fal, out bool safe)) request["enable_safety_checker"] = safe;
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_FalImage, out string neg) && !string.IsNullOrEmpty(neg)) request["negative_prompt"] = neg;
        if (input.TryGet(SwarmUIAPIBackends.PromptExpansionParam_Wan, out bool expand)) request["enable_prompt_expansion"] = expand;
    }

    /// <summary>Z-Image Turbo: image_size, batch, steps, seed. No guidance or negative prompt.</summary>
    private static void BuildZImageParams(T2IParamInput input, JObject request)
    {
        request["image_size"] = input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Fal, out string size) ? size : "landscape_4_3";
        request["num_images"] = GetNumImages(input);
        if (input.TryGet(SwarmUIAPIBackends.NumInferenceStepsParam_Fal, out int steps)) request["num_inference_steps"] = steps;
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_Fal, out string format)) request["output_format"] = format;
        if (input.TryGet(SwarmUIAPIBackends.SafetyCheckerParam_Fal, out bool safe)) request["enable_safety_checker"] = safe;
        if (input.TryGet(SwarmUIAPIBackends.PromptExpansionParam_Wan, out bool expand)) request["enable_prompt_expansion"] = expand;
    }

    /// <summary>Nano Banana 2: extended aspect list, 0.5K-4K resolution, plus reasoning and web-search controls.</summary>
    private static void BuildNanoBanana2Params(T2IParamInput input, JObject request)
    {
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_NB2);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_NB2);
        Put(input, request, "thinking_level", SwarmUIAPIBackends.ThinkingLevelParam_NB2);
        Put(input, request, "system_prompt", SwarmUIAPIBackends.SystemPromptParam_NB2);
        request["num_images"] = GetNumImages(input);
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_FalAspect, out string format)) request["output_format"] = format;
        if (input.TryGet(SwarmUIAPIBackends.SafetyToleranceParam_NB2, out int tolerance)) request["safety_tolerance"] = tolerance;
        if (input.TryGet(SwarmUIAPIBackends.WebSearchParam_NB2, out bool search)) request["enable_web_search"] = search;
    }

    /// <summary>Aspect ratio models: FLUX Ultra, Kling Image, Nano Banana, Imagen 3. Use aspect_ratio + resolution instead of image_size.</summary>
    private static void BuildAspectRatioImageParams(T2IParamInput input, JObject request, string modelId)
    {
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_FalImage, out string aspectRatio) && aspectRatio != "auto")
            request["aspect_ratio"] = aspectRatio;
        // Resolution: Kling Image (1K/2K), Nano Banana (1K/2K/4K)
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_FalImage, out string resolution))
            request["resolution"] = resolution;
        request["num_images"] = GetNumImages(input);
        // Seed: supported by FLUX Ultra, Nano Banana, Imagen 3 (NOT Kling Image)
        if (!modelId.StartsWith("Kling/"))
        {
            if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        }
        // Output format: all aspect ratio models support it
        if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_FalAspect, out string format)) request["output_format"] = format;
        // Negative prompt: Imagen 3 only
        if (modelId.StartsWith("Google/imagen-3"))
        {
            if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_FalImage, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
                request["negative_prompt"] = negPrompt;
        }
    }

    /// <summary>Recraft V3: image_size + style, NO steps/guidance/seed.</summary>
    private static void BuildRecraftImageParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Fal, out string imageSize)) request["image_size"] = imageSize;
        else request["image_size"] = "square_hd";
        if (input.TryGet(SwarmUIAPIBackends.StyleParam_Recraft, out string style) && !string.IsNullOrEmpty(style))
            request["style"] = style;
    }

    /// <summary>Bria FIBO: aspect_ratio + steps_num + guidance_scale + negative_prompt + seed. Uses non-standard param name 'steps_num'.</summary>
    private static void BuildBriaImageParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_FalImage, out string aspectRatio) && aspectRatio != "auto")
            request["aspect_ratio"] = aspectRatio;
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
        if (input.TryGet(SwarmUIAPIBackends.GuidanceScaleParam_Fal, out double guidance)) request["guidance_scale"] = guidance;
        // Bria uses 'steps_num' instead of standard 'num_inference_steps'
        if (input.TryGet(SwarmUIAPIBackends.NumInferenceStepsParam_Fal, out int steps)) request["steps_num"] = steps;
        if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_FalImage, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
            request["negative_prompt"] = negPrompt;
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

    /// <summary>Fields common to Wan 2.5+: optional audio drive and prompt expansion.</summary>
    private static void AddWanShared(T2IParamInput input, JObject request)
    {
        Put(input, request, "audio_url", SwarmUIAPIBackends.AudioUrlParam_Wan);
        if (input.TryGet(SwarmUIAPIBackends.PromptExpansionParam_Wan, out bool expand)) request["enable_prompt_expansion"] = expand;
    }

    /// <summary>Wan 2.5 preview: aspect (16:9/9:16/1:1), resolution (480p-1080p), duration (5 or 10), audio_url, negative, seed.</summary>
    private static void BuildWan25VideoParams(T2IParamInput input, JObject request)
    {
        PutInt(input, request, "duration", SwarmUIAPIBackends.DurationParam_Wan25);
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Wan25);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Wan25);
        AddWanShared(input, request);
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>Wan 2.6 i2v: resolution (720p/1080p), duration (5/10/15), audio_url, multi_shots, negative, seed. No aspect.</summary>
    private static void BuildWan26VideoParams(T2IParamInput input, JObject request)
    {
        PutInt(input, request, "duration", SwarmUIAPIBackends.DurationParam_Wan26);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Wan2x);
        AddWanShared(input, request);
        if (input.TryGet(SwarmUIAPIBackends.MultiShotsParam_Wan, out bool multi)) request["multi_shots"] = multi;
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>Wan 2.7 t2v/i2v: duration (2-15), resolution (720p/1080p), audio_url, negative, seed.
    /// t2v takes an aspect ratio; i2v derives it from the input image and instead accepts a last frame.</summary>
    private static void BuildWan27VideoParams(T2IParamInput input, JObject request, bool aspect, bool endImage)
    {
        PutInt(input, request, "duration", SwarmUIAPIBackends.DurationParam_Wan27);
        if (aspect) Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Wan27);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Wan2x);
        if (endImage) Put(input, request, "end_image_url", SwarmUIAPIBackends.EndImageUrlParam);
        AddWanShared(input, request);
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>Wan 2.7 reference-to-video: reference_image_urls / reference_video_urls arrays, aspect, resolution,
    /// duration (2-10), multi_shots, negative, seed. No audio drive.</summary>
    private static void BuildWan27RefVideoParams(T2IParamInput input, JObject request)
    {
        PutInt(input, request, "duration", SwarmUIAPIBackends.DurationParam_Wan27Ref);
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Wan27);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Wan2x);
        AddReferenceUrls(input, request, "reference_image_urls", "reference_video_urls", null);
        if (input.TryGet(SwarmUIAPIBackends.MultiShotsParam_Wan, out bool multi)) request["multi_shots"] = multi;
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>Seedance 2.5: duration (auto or 4-30, as a string), aspect_ratio, resolution (480p/720p),
    /// generate_audio. Takes no seed input - seed is only returned in the response.</summary>
    private static void BuildSeedance25VideoParams(T2IParamInput input, JObject request, ModelDefinition model, bool endImage)
    {
        Put(input, request, "duration", SwarmUIAPIBackends.DurationParam_Seedance25);
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Seedance2);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Seedance2);
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_Seedance2, out bool audio)) request["generate_audio"] = audio;
        if (endImage) Put(input, request, "end_image_url", SwarmUIAPIBackends.EndImageUrlParam);
        if (model.ExtraFlags.Contains("fal_seedance_ref_params"))
        {
            AddReferenceUrls(input, request, "image_urls", "video_urls", "audio_urls");
        }
    }

    /// <summary>FLUX 3 on fal: duration (auto or 5-20), aspect_ratio, resolution (720p/1080p), generate_audio,
    /// safety_tolerance. Takes no seed and no negative prompt - sending either would be silently ignored.</summary>
    private static void BuildFlux3VideoParams(T2IParamInput input, JObject request)
    {
        Put(input, request, "duration", SwarmUIAPIBackends.DurationParam_Flux3);
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Flux3);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Flux3);
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_FalVideo, out bool audio)) request["generate_audio"] = audio;
        if (input.TryGet(SwarmUIAPIBackends.SafetyToleranceParam_Flux3, out int safety)) request["safety_tolerance"] = safety;
    }

    /// <summary>MiniMax H3: integer duration, resolution on its own 768P/2K/4K scale, prompt expansion.
    /// Takes no seed. Aspect ratio applies to t2v and ref2v only - i2v follows the input image.</summary>
    private static void BuildH3VideoParams(T2IParamInput input, JObject request, T2IRegisteredParam<string> aspect, bool endImage, bool refs)
    {
        PutInt(input, request, "duration", SwarmUIAPIBackends.DurationParam_H3);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_H3);
        if (aspect is not null) Put(input, request, "aspect_ratio", aspect);
        if (input.TryGet(SwarmUIAPIBackends.PromptExpansionParam_Wan, out bool expand)) request["enable_prompt_expansion"] = expand;
        if (endImage) Put(input, request, "end_image_url", SwarmUIAPIBackends.EndImageUrlParam);
        if (refs) AddReferenceUrls(input, request, "reference_image_urls", "reference_video_urls", "reference_audio_urls");
    }

    /// <summary>Kling V3 Turbo Pro: prompt, image_url and duration only. No aspect, resolution, audio, negative or seed.</summary>
    private static void BuildKlingTurboVideoParams(T2IParamInput input, JObject request)
    {
        Put(input, request, "duration", SwarmUIAPIBackends.DurationParam_KlingTurbo);
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

    /// <summary>Seedance 2.0: duration (auto/4-15), aspect_ratio (many incl auto), resolution (480p/720p), generate_audio, seed</summary>
    private static void BuildSeedance2VideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_Seedance2, out string duration))
        {
            request["duration"] = duration;
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Seedance2, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Seedance2, out string resolution))
        {
            request["resolution"] = resolution;
        }
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_Seedance2, out bool genAudio))
        {
            request["generate_audio"] = genAudio;
        }
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0)
        {
            request["seed"] = seed;
        }
    }

    /// <summary>Seedance 1.0: duration (2-12), aspect_ratio (no auto), resolution (480p/720p/1080p), camera_fixed, seed</summary>
    private static void BuildSeedance1VideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.DurationParam_Seedance1, out string duration))
        {
            request["duration"] = duration;
        }
        if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Seedance1, out string aspectRatio))
        {
            request["aspect_ratio"] = aspectRatio;
        }
        if (input.TryGet(SwarmUIAPIBackends.ResolutionParam_Seedance1, out string resolution))
        {
            request["resolution"] = resolution;
        }
        if (input.TryGet(SwarmUIAPIBackends.CameraFixedParam_Seedance1, out bool cameraFixed))
        {
            request["camera_fixed"] = cameraFixed;
        }
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0)
        {
            request["seed"] = seed;
        }
    }

    /// <summary>Attaches reference media. Endpoints disagree on the field names (Seedance uses image_urls,
    /// Wan and H3 use reference_image_urls), so the caller supplies them. Any Init Image already placed in
    /// image_url is folded in as the first reference.</summary>
    private static void AddReferenceUrls(T2IParamInput input, JObject request, string imageField, string videoField, string audioField)
    {
        JArray images = UrlList(input, SwarmUIAPIBackends.RefImageUrlsParam) ?? [];
        if (request.Remove("image_url", out JToken initImage))
        {
            images.AddFirst(initImage);
        }
        if (images.Count > 0)
        {
            request[imageField] = images;
        }
        if (videoField is not null && UrlList(input, SwarmUIAPIBackends.RefVideoUrlsParam) is JArray videos)
        {
            request[videoField] = videos;
        }
        if (audioField is not null && UrlList(input, SwarmUIAPIBackends.RefAudioUrlsParam) is JArray audio)
        {
            request[audioField] = audio;
        }
    }

    /// <summary>Image utilities (upscalers, background removal, face restoration). Input image comes from AttachInputMedia.</summary>
    private static void BuildUtilityImageParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.UpscaleFactorParam_FalUtility, out double scale))
        {
            request["upscale_factor"] = scale;
        }
    }

    /// <summary>Video utilities (video upscale, video background removal). These take video_url, not image_url.</summary>
    private static void BuildUtilityVideoParams(T2IParamInput input, JObject request)
    {
        if (input.TryGet(SwarmUIAPIBackends.VideoUrlParam_FalUtility, out string videoUrl) && !string.IsNullOrEmpty(videoUrl))
        {
            request["video_url"] = videoUrl;
        }
        if (input.TryGet(SwarmUIAPIBackends.UpscaleFactorParam_FalUtility, out double scale))
        {
            request["upscale_factor"] = scale;
        }
    }

    /// <summary>Adds the fields shared by every video family: an optional negative prompt and the seed.</summary>
    private static void AddSeedAndNegative(T2IParamInput input, JObject request, bool negative)
    {
        if (negative && input.TryGet(SwarmUIAPIBackends.NegativePromptParam_FalVideo, out string neg) && !string.IsNullOrEmpty(neg)) request["negative_prompt"] = neg;
        if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out long seed) && seed >= 0) request["seed"] = seed;
    }

    private static void Put(T2IParamInput input, JObject request, string field, T2IRegisteredParam<string> param)
    {
        if (input.TryGet(param, out string val) && !string.IsNullOrEmpty(val)) request[field] = val;
    }

    private static int Seconds(T2IParamInput input, T2IRegisteredParam<string> param)
    {
        return input.TryGet(param, out string val) && int.TryParse(val, out int seconds) ? seconds : 0;
    }

    /// <summary>Writes a numeric field. Several fal endpoints declare integer enums and reject the quoted form.</summary>
    private static void PutInt(T2IParamInput input, JObject request, string field, T2IRegisteredParam<string> param)
    {
        int value = Seconds(input, param);
        if (value > 0)
        {
            request[field] = value;
        }
    }

    /// <summary>Wan 2.2 A14B: aspect (16:9,9:16,1:1), resolution (480p/580p/720p), negative, seed.
    /// Length is num_frames at 16fps, not duration. Rejects duration and generate_audio.</summary>
    private static void BuildWan22VideoParams(T2IParamInput input, JObject request)
    {
        int seconds = Seconds(input, SwarmUIAPIBackends.DurationParam_Wan22);
        if (seconds > 0) request["num_frames"] = Math.Clamp(seconds * 16 + 1, 17, 161);
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Wan22);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Wan22);
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>PixVerse v5: duration (5 or 8), aspect, resolution (360p-1080p), negative, seed. No audio.</summary>
    private static void BuildPixVerseVideoParams(T2IParamInput input, JObject request)
    {
        int seconds = Seconds(input, SwarmUIAPIBackends.DurationParam_PixVerse);
        if (seconds > 0) request["duration"] = seconds;
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_PixVerse);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_PixVerse);
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>LTX-2 19B: video_size + num_frames at 25fps, generate_audio, negative, seed.
    /// Rejects duration, aspect_ratio and resolution.</summary>
    private static void BuildLtx2VideoParams(T2IParamInput input, JObject request)
    {
        int seconds = Seconds(input, SwarmUIAPIBackends.DurationParam_Ltx2);
        if (seconds > 0) request["num_frames"] = seconds * 25 + 1;
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_FalVideo, out bool audio)) request["generate_audio"] = audio;
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>LTX-13B distilled: resolution (480p/720p), aspect (incl auto), negative, seed, num_frames at 24fps.</summary>
    private static void BuildLtx13bVideoParams(T2IParamInput input, JObject request)
    {
        int seconds = Seconds(input, SwarmUIAPIBackends.DurationParam_Ltx13b);
        if (seconds > 0) request["num_frames"] = seconds * 24 + 1;
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Ltx13b);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Ltx13b);
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>Vidu Q3: duration (int), aspect, resolution, seed. Audio field is 'audio', not 'generate_audio'. No negative.</summary>
    private static void BuildViduVideoParams(T2IParamInput input, JObject request)
    {
        int seconds = Seconds(input, SwarmUIAPIBackends.DurationParam_Vidu);
        if (seconds > 0) request["duration"] = seconds;
        if (input.TryGet(SwarmUIAPIBackends.GenerateAudioParam_FalVideo, out bool audio)) request["audio"] = audio;
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Vidu);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Vidu);
        AddSeedAndNegative(input, request, negative: false);
    }

    /// <summary>Pika v2.2: duration (5 or 10), aspect (7 ratios), resolution (720p/1080p), negative, seed. No audio.</summary>
    private static void BuildPikaVideoParams(T2IParamInput input, JObject request)
    {
        Put(input, request, "duration", SwarmUIAPIBackends.DurationParam_Pika);
        Put(input, request, "aspect_ratio", SwarmUIAPIBackends.AspectRatioParam_Pika);
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Pika);
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>Kandinsky 5 Pro: resolution (512P/1024P), duration as a "5s" string, seed. No aspect, negative or audio.</summary>
    private static void BuildKandinskyVideoParams(T2IParamInput input, JObject request)
    {
        int seconds = Seconds(input, SwarmUIAPIBackends.DurationParam_Kandinsky);
        if (seconds > 0) request["duration"] = $"{seconds}s";
        Put(input, request, "resolution", SwarmUIAPIBackends.ResolutionParam_Kandinsky);
        AddSeedAndNegative(input, request, negative: false);
    }

    /// <summary>CogVideoX-5B: video_size, negative, seed. Rejects duration, aspect_ratio, resolution and audio.</summary>
    private static void BuildCogVideoXParams(T2IParamInput input, JObject request)
    {
        AddSeedAndNegative(input, request, negative: true);
    }

    /// <summary>Grok Imagine Video: duration, aspect_ratio, generate_audio, negative_prompt, seed</summary>
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

    public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
    {
        string path = !string.IsNullOrEmpty(model.EndpointOverride) ? model.EndpointOverride : model.Id;
        return $"{provider.BaseUrl}/{path}";
    }

    public override async Task<byte[][]> ProcessResponse(JObject response, ProviderDefinition provider, string apiKey = null)
    {
        // Most image models return an array, one entry per image requested.
        if (response["images"] is JArray images && images.Count > 0)
        {
            return await CollectImages(images, "base64", "url", "Fal.ai");
        }
        // Some models return a single object instead.
        foreach (string key in (string[])["image", "video"])
        {
            string url = response[key]?["url"]?.ToString();
            if (!string.IsNullOrEmpty(url))
            {
                return [url.StartsWith("data:") ? DecodeBase64Image(url) : await DownloadImageFromUrl(url)];
            }
        }
        if (response["output"] is JArray outputArr && outputArr.Count > 0)
        {
            return await CollectImages(outputArr, null, "url", "Fal.ai");
        }
        throw new Exception($"Fal.ai response missing image/video data. Response keys: {string.Join(", ", response.Properties().Select(p => p.Name))}");
    }
}

#endregion
