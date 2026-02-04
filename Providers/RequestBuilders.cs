using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers
{
    /// <summary>Interface for building provider-specific API requests.</summary>
    public interface IRequestBuilder
    {
        /// <summary>Builds the JSON request body for the API call.</summary>
        JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider);

        /// <summary>Processes the API response and extracts image data.</summary>
        Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider);

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
        public abstract Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider);

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
                if (input.TryGet(SwarmUIAPIBackends.QualityParam_GPTImage1, out string quality))
                    request["quality"] = quality;
                if (input.TryGet(SwarmUIAPIBackends.BackgroundParam_GPTImage1, out string bg))
                    request["background"] = bg;
                if (input.TryGet(SwarmUIAPIBackends.ModerationParam_GPTImage1, out string mod))
                    request["moderation"] = mod;
                if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_GPTImage1, out string format))
                    request["output_format"] = format;
                if (input.TryGet(SwarmUIAPIBackends.OutputCompressionParam_GPTImage1, out int compression))
                    request["output_compression"] = compression;
            }
            else if (modelName == "dall-e-3")
            {
                if (input.TryGet(SwarmUIAPIBackends.QualityParam_OpenAI, out string quality))
                    request["quality"] = quality;
                if (input.TryGet(SwarmUIAPIBackends.StyleParam_OpenAI, out string style))
                    request["style"] = style;
                request["response_format"] = "b64_json";
            }
            else
            {
                request["response_format"] = "b64_json";
            }

            return request;
        }

        public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider)
        {
            JArray data = response["data"] as JArray;
            if (data == null || data.Count == 0)
            {
                throw new Exception("No image data in OpenAI response");
            }

            JToken firstImage = data[0];
            if (firstImage["b64_json"] != null)
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
        public override JObject BuildRequest(T2IParamInput input, ModelDefinition model, ProviderDefinition provider)
        {
            bool hasInputImage = input.TryGet(SwarmUIAPIBackends.ImagePromptParam_Ideogram, out Image inputImg) 
                && inputImg?.RawData != null;

            JObject imageRequest = new()
            {
                ["prompt"] = input.Get(T2IParamTypes.Prompt),
                ["model"] = model.Id,
                ["magic_prompt_option"] = input.TryGet(SwarmUIAPIBackends.MagicPromptParam_Ideogram, out string magic) ? magic : "AUTO"
            };

            if (input.TryGet(SwarmUIAPIBackends.AspectRatioParam_Ideogram, out string aspect))
                imageRequest["aspect_ratio"] = aspect;
            if (input.TryGet(SwarmUIAPIBackends.StyleTypeParam_Ideogram, out string style))
                imageRequest["style_type"] = style;
            if (input.TryGet(SwarmUIAPIBackends.NegativePromptParam_Ideogram, out string negPrompt) && !string.IsNullOrEmpty(negPrompt))
                imageRequest["negative_prompt"] = negPrompt;

            if (hasInputImage)
            {
                string base64Image = Convert.ToBase64String(inputImg.RawData);
                imageRequest["image"] = base64Image;
                if (input.TryGet(SwarmUIAPIBackends.ImageWeightParam_Ideogram, out double weight))
                    imageRequest["image_weight"] = weight;
            }

            return new JObject { ["image_request"] = imageRequest };
        }

        public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
        {
            bool hasInputImage = input.TryGet(SwarmUIAPIBackends.ImagePromptParam_Ideogram, out Image inputImg) 
                && inputImg?.RawData != null;

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

        public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider)
        {
            JArray data = response["data"] as JArray;
            if (data == null || data.Count == 0)
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

            if (input.TryGet(SwarmUIAPIBackends.WidthParam_BlackForest, out int width))
                request["width"] = width;
            if (input.TryGet(SwarmUIAPIBackends.HeightParam_BlackForest, out int height))
                request["height"] = height;
            if (input.TryGet(SwarmUIAPIBackends.PromptUpsampling_BlackForest, out bool upsample))
                request["prompt_upsampling"] = upsample;
            if (input.TryGet(SwarmUIAPIBackends.SafetyTolerance_BlackForest, out int safety))
                request["safety_tolerance"] = safety;
            if (input.TryGet(SwarmUIAPIBackends.SeedParam_BlackForest, out long seed) && seed >= 0)
                request["seed"] = seed;
            if (input.TryGet(SwarmUIAPIBackends.GuidanceParam_BlackForest, out double guidance))
                request["guidance"] = guidance;
            if (input.TryGet(SwarmUIAPIBackends.StepsParam_BlackForest, out int steps))
                request["steps"] = steps;
            if (input.TryGet(SwarmUIAPIBackends.IntervalParam_BlackForest, out double interval))
                request["interval"] = interval;
            if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_BlackForest, out string format))
                request["output_format"] = format;

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

        public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider)
        {
            // BFL uses async polling - check for result URL
            string resultUrl = response["result"]?["sample"]?.ToString();
            if (!string.IsNullOrEmpty(resultUrl))
            {
                return await DownloadImageFromUrl(resultUrl);
            }

            // Check for direct image data
            if (response["sample"] != null)
            {
                string sampleUrl = response["sample"].ToString();
                return await DownloadImageFromUrl(sampleUrl);
            }

            throw new Exception("Black Forest Labs response missing image data");
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

        public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider)
        {
            JArray data = response["data"] as JArray;
            if (data == null || data.Count == 0)
            {
                throw new Exception("No image data in Grok response");
            }

            JToken firstImage = data[0];
            if (firstImage["b64_json"] != null)
            {
                return DecodeBase64Image(firstImage["b64_json"].ToString());
            }
            else if (firstImage["url"] != null)
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

            // Imagen format
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
            return isGemini 
                ? $"{provider.BaseUrl}/{model.Id}:generateContent" 
                : $"{provider.BaseUrl}/{model.Id}:predict";
        }

        public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider)
        {
            // Try Gemini format first
            JArray candidates = response["candidates"] as JArray;
            if (candidates != null && candidates.Count > 0)
            {
                JArray parts = candidates[0]["content"]?["parts"] as JArray;
                if (parts != null)
                {
                    foreach (JToken part in parts)
                    {
                        if (part["inlineData"] != null)
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

            // Try Imagen format
            JArray predictions = response["predictions"] as JArray;
            if (predictions != null && predictions.Count > 0)
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
            JObject request = new()
            {
                ["prompt"] = input.Get(T2IParamTypes.Prompt)
            };

            // Common Fal parameters
            if (input.TryGet(SwarmUIAPIBackends.ImageSizeParam_Fal, out string imageSize))
                request["image_size"] = imageSize;
            else
                request["image_size"] = "landscape_4_3";

            request["num_images"] = GetNumImages(input);

            if (input.TryGet(SwarmUIAPIBackends.SeedParam_Fal, out int seed) && seed >= 0)
                request["seed"] = seed;

            if (input.TryGet(SwarmUIAPIBackends.GuidanceScaleParam_Fal, out double guidance))
                request["guidance_scale"] = guidance;

            if (input.TryGet(SwarmUIAPIBackends.NumInferenceStepsParam_Fal, out int steps))
                request["num_inference_steps"] = steps;

            if (input.TryGet(SwarmUIAPIBackends.OutputFormatParam_Fal, out string format))
                request["output_format"] = format;

            if (input.TryGet(SwarmUIAPIBackends.SafetyCheckerParam_Fal, out bool safety))
                request["enable_safety_checker"] = safety;

            // Sync mode returns base64 directly
            request["sync_mode"] = true;

            return request;
        }

        public override string GetEndpointUrl(ModelDefinition model, ProviderDefinition provider, T2IParamInput input)
        {
            // Fal.ai endpoint is: https://fal.run/{model-id}
            return $"{provider.BaseUrl}/{model.Id}";
        }

        public override async Task<byte[]> ProcessResponse(JObject response, ProviderDefinition provider)
        {
            JArray images = response["images"] as JArray;
            if (images == null || images.Count == 0)
            {
                throw new Exception("No image data in Fal.ai response");
            }

            JToken firstImage = images[0];
            
            // Check for URL (most common)
            string url = firstImage["url"]?.ToString();
            if (!string.IsNullOrEmpty(url))
            {
                // If it's a data URI, decode it
                if (url.StartsWith("data:"))
                {
                    return DecodeBase64Image(url);
                }
                return await DownloadImageFromUrl(url);
            }

            // Check for direct base64
            string base64 = firstImage["base64"]?.ToString();
            if (!string.IsNullOrEmpty(base64))
            {
                return DecodeBase64Image(base64);
            }

            throw new Exception("Fal.ai response missing image data");
        }
    }

    #endregion
}
