using SwarmUI.Text2Image;
using Newtonsoft.Json.Linq;
using SwarmUI.Utils;
using Hartsy.Extensions.APIBackends.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using SwarmUI.Backends;

namespace Hartsy.Extensions.APIBackends
{
    /// <summary>Initializes and manages API providers for image generation.</summary>
    public class APIProviderInit
    {
        private static readonly Dictionary<string, T2IModelClass> _modelClasses = [];

        /// <summary>Dictionary of available providers</summary>
        public Dictionary<string, APIProviderMetadata> Providers { get; private set; }

        /// <summary>Shared HttpClient for all API requests</summary>
        protected static readonly HttpClient HttpClient = NetworkBackendUtils.MakeHttpClient();

        public APIProviderInit()
        {
            InitializeProviders();
        }

        /// <summary>Create and register a model class</summary>
        public static T2IModelClass CreateModelClass(string id, string name)
        {
            if (!_modelClasses.TryGetValue(id, out T2IModelClass modelClass))
            {
                modelClass = new T2IModelClass
                {
                    ID = id,
                    Name = name,
                    CompatClass = id,
                    StandardWidth = 1024,
                    StandardHeight = 1024,
                    IsThisModelOfClass = (model, header) => true
                };
                _modelClasses[id] = modelClass;
                T2IModelClassSorter.Register(modelClass);
                Logs.Debug($"[APIProviderInit] Registered API model class: {id} ({name})");
            }
            return modelClass;
        }

        /// <summary>Initializes all API providers.</summary>
        public void InitializeProviders()
        {
            Providers = [];
            Providers["bfl_api"] = InitializeBlackForestProvider();
            Providers["openai_api"] = InitializeOpenAIProvider();
            Providers["ideogram_api"] = InitializeIdeogramProvider();
        }

        public APIProviderMetadata InitializeOpenAIProvider()
        {
            try
            {
                T2IModel dallE2 = new(null, null, null, "OpenAI/dall-e-2")
                {
                    Title = "DALL-E 2",
                    Description = "OpenAI's DALL-E 2 model",
                    ModelClass = CreateModelClass("openai_api", "DALL-E"),
                    StandardWidth = 1024,
                    StandardHeight = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle2.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "OpenAI/dall-e-2",
                        Title = "DALL-E 2",
                        Author = "OpenAI",
                        Description = "Generative model capable of creating realistic images from text descriptions",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle2.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        License = "Commercial",
                        UsageHint = "Good for general image generation via API",
                        Date = "2022",
                        ModelClassType = "openai_api",
                        Tags = ["openai", "dall-e", "generative", "dalle2_params"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                };
                T2IModel dallE3 = new(null, null, null, "OpenAI/dall-e-3")
                {
                    Title = "DALL-E 3",
                    Description = "OpenAI's DALL-E 3 model",
                    ModelClass = CreateModelClass("openai_api", "DALL-E"),
                    StandardWidth = 1024,
                    StandardHeight = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "OpenAI/dall-e-3",
                        Title = "DALL-E 3",
                        Author = "OpenAI",
                        Description = "Advanced generative model for creating highly detailed and accurate images from text descriptions",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        License = "Commercial",
                        UsageHint = "Excellent for high-quality image generation with accurate text representation",
                        Date = "2023",
                        ModelClassType = "openai_api",
                        Tags = ["openai", "dall-e", "high-quality", "text-accurate", "dalle3_params"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                };
                T2IModel image1 = new(null, null, null, "OpenAI/gpt-image-1")
                {
                    Title = "GPT Image 1",
                    Description = "OpenAI's GPT Image 1 model",
                    ModelClass = CreateModelClass("openai_api", "DALL-E"),
                    StandardWidth = 1024,
                    StandardHeight = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/gpt-image-1.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "OpenAI/gpt-image-1",
                        Title = "GPT Image 1",
                        Author = "OpenAI",
                        Description = "Able to generate images with stronger instruction following, contextual awareness, and world knowledge",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/gpt-image-1.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        License = "Commercial",
                        UsageHint = "Best for context-aware image generation and edits requiring strong instruction following",
                        Date = "2025",
                        ModelClassType = "openai_api",
                        Tags = ["openai", "high-quality", "text-accurate", "gpt-image-1_params"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                };
                APIProviderMetadata provider = new()
                {
                    Name = "OpenAI",
                    Models = new Dictionary<string, T2IModel>
                    {
                        ["OpenAI/dall-e-2"] = dallE2,
                        ["OpenAI/dall-e-3"] = dallE3,
                        ["OpenAI/gpt-image-1"] = image1
                    },
                    RequestConfig = new RequestConfig
                    {
                        BaseUrl = "https://api.openai.com/v1/images/generations",
                        AuthHeader = "Bearer",
                        BuildRequest = input =>
                        {
                            string modelName = input.Get(T2IParamTypes.Model).Name.Replace("OpenAI/", "");
                            JObject request = new()
                            {
                                ["prompt"] = input.Get(T2IParamTypes.Prompt),
                                ["model"] = modelName,
                                ["n"] = input.TryGet(T2IParamTypes.Images, out int numImages) && numImages > 0 ? numImages : 1,
                                ["size"] = input.Get(SwarmUIAPIBackends.SizeParam_OpenAI)
                            };

                            if (modelName == "gpt-image-1")
                            {
                                request["quality"] = input.Get(SwarmUIAPIBackends.QualityParam_GPTImage1);
                                request["background"] = input.Get(SwarmUIAPIBackends.BackgroundParam_GPTImage1);
                                request["moderation"] = input.Get(SwarmUIAPIBackends.ModerationParam_GPTImage1);
                                request["output_format"] = input.Get(SwarmUIAPIBackends.OutputFormatParam_GPTImage1);
                                request["output_compression"] = input.Get(SwarmUIAPIBackends.OutputCompressionParam_GPTImage1);
                            }
                            else if (modelName == "dall-e-3")
                            {
                                request["quality"] = input.Get(SwarmUIAPIBackends.QualityParam_OpenAI);
                                request["style"] = input.Get(SwarmUIAPIBackends.StyleParam_OpenAI);
                                request["response_format"] = "b64_json";
                            }
                            else if (modelName == "dall-e-2")
                            {
                                request["response_format"] = "b64_json";
                            }

                            return request;
                        },
                        ProcessResponse = async response =>
                        {
                            // Check if there's a data array
                            JArray dataArray = response["data"] as JArray;
                            if (dataArray == null || dataArray.Count == 0)
                            {
                                Logs.Error("[APIProviderInit] OpenAI API response missing 'data' array or array is empty");
                                throw new Exception("OpenAI API response missing image data");
                            }
                            JToken firstImage = dataArray[0];
                            // Check if we have a b64_json response
                            if (firstImage["b64_json"] != null)
                            {
                                string b64 = firstImage["b64_json"].ToString();
                                return Convert.FromBase64String(b64);
                            }
                            // Check if we have a URL response
                            else if (firstImage["url"] != null)
                            {
                                string imageUrl = firstImage["url"].ToString();
                                // Download the image bytes from the URL
                                try
                                {
                                    byte[] imageBytes = await HttpClient.GetByteArrayAsync(imageUrl);
                                    return imageBytes;
                                }
                                catch (Exception ex)
                                {
                                    Logs.Error($"[APIProviderInit] OpenAI ProcessResponse - Failed to download image: {ex.Message}");
                                    throw new Exception($"Failed to download OpenAI image: {ex.Message}", ex);
                                }
                            }
                            else
                            {
                                Logs.Error("[APIProviderInit] OpenAI API response missing both 'b64_json' and 'url' properties");
                                throw new Exception("OpenAI API response in unexpected format");
                            }
                        }
                    }
                };
                // Register parameters
                provider.AddParameterToModel("OpenAI/dall-e-2", "size", SwarmUIAPIBackends.SizeParam_OpenAI);
                provider.AddParameterToModel("OpenAI/dall-e-2", "response_format", SwarmUIAPIBackends.ResponseFormatParam_OpenAI);
                provider.AddParameterToModel("OpenAI/dall-e-3", "size", SwarmUIAPIBackends.SizeParam_OpenAI);
                provider.AddParameterToModel("OpenAI/dall-e-3", "quality", SwarmUIAPIBackends.QualityParam_OpenAI);
                provider.AddParameterToModel("OpenAI/dall-e-3", "style", SwarmUIAPIBackends.StyleParam_OpenAI);
                provider.AddParameterToModel("OpenAI/dall-e-3", "response_format", SwarmUIAPIBackends.ResponseFormatParam_OpenAI);
                provider.AddParameterToModel("OpenAI/gpt-image-1", "size", SwarmUIAPIBackends.SizeParam_OpenAI);
                provider.AddParameterToModel("OpenAI/gpt-image-1", "quality", SwarmUIAPIBackends.QualityParam_GPTImage1);
                provider.AddParameterToModel("OpenAI/gpt-image-1", "background", SwarmUIAPIBackends.BackgroundParam_GPTImage1);
                provider.AddParameterToModel("OpenAI/gpt-image-1", "moderation", SwarmUIAPIBackends.ModerationParam_GPTImage1);
                provider.AddParameterToModel("OpenAI/gpt-image-1", "output_format", SwarmUIAPIBackends.OutputFormatParam_GPTImage1);
                provider.AddParameterToModel("OpenAI/gpt-image-1", "output_compression", SwarmUIAPIBackends.OutputCompressionParam_GPTImage1);
                return provider;
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIProviderInit] Failed to initialize OpenAI provider: {ex.Message}");
                throw;
            }
        }

        public APIProviderMetadata InitializeIdeogramProvider()
        {
            try
            {
                // Define all known Ideogram models
                var models = new Dictionary<string, (string Title, string Description, string ImagePath, string FeatureFlag)>
                {
                    ["v1"] = ("Ideogram v1", "Ideogram's v1 model", "ideogram.png", "ideogram_v1_params"),
                    ["v1-turbo"] = ("Ideogram v1 Turbo", "Ideogram's v1 Turbo model", "ideogram_turbo.png", "ideogram_v1_params"),
                    ["v2"] = ("Ideogram v2", "Ideogram's v2 model", "ideogram2.png", "ideogram_v2_params"),
                    ["v2-turbo"] = ("Ideogram v2 Turbo", "Ideogram's v2 Turbo model", "ideogram2_turbo.png", "ideogram_v2_params"),
                    ["v2a"] = ("Ideogram v2a", "Ideogram's v2a model", "ideogram2a.png", "ideogram_v2_params"),
                    ["v2a-turbo"] = ("Ideogram v2a Turbo", "Ideogram's v2a Turbo model", "ideogram2a_turbo.png", "ideogram_v2_params"),
                    ["v3"] = ("Ideogram v3", "Ideogram's v3 model", "ideogram3.png", "ideogram_v3_params") // Placeholder for the unreleased V3 model
                };
                Dictionary<string, T2IModel> providerModels = [];
                foreach (var kvp in models)
                {
                    string modelKey = kvp.Key;
                    (string title, string description, string imageName, string featureFlag) = kvp.Value;
                    string imagePath = "src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/ideogram.png";
                    string base64Image = File.Exists(imagePath) ? Convert.ToBase64String(File.ReadAllBytes(imagePath)) : "";
                    T2IModel model = new(null, null, null, $"Ideogram/{modelKey}")
                    {
                        Title = title,
                        Description = description,
                        ModelClass = CreateModelClass("ideogram_api", "Ideogram"),
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        IsSupportedModelType = true,
                        PreviewImage = $"data:image/png;base64,{base64Image}",
                        Metadata = new T2IModelHandler.ModelMetadataStore
                        {
                            ModelName = $"Ideogram/{modelKey}",
                            Title = title,
                            Author = "Ideogram",
                            Description = description,
                            PreviewImage = $"data:image/png;base64,{base64Image}",
                            StandardWidth = 1024,
                            StandardHeight = 1024,
                            License = "Commercial",
                            UsageHint = "Ideogram API text-to-image model",
                            Date = modelKey.Contains("v3") ? "2024" : (modelKey.Contains("v2") ? "2024" : "2023"),
                            ModelClassType = "ideogram_api",
                            Tags = ["ideogram", "generative", modelKey, featureFlag],
                            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        },
                    };
                    providerModels[$"Ideogram/{modelKey}"] = model;
                }
                APIProviderMetadata provider = new()
                {
                    Name = "Ideogram",
                    Models = providerModels,
                    RequestConfig = new RequestConfig
                    {
                        BaseUrl = "https://api.ideogram.ai/generate",
                        AuthHeader = "Api-Key",
                        BuildRequest = input =>
                        {
                            // Helper function to convert SwarmUI aspect ratio to Ideogram format
                            static string ConvertToIdeogramAspectRatio(string swarmAspectRatio)
                            {
                                if (string.IsNullOrEmpty(swarmAspectRatio) || !swarmAspectRatio.Contains(':'))
                                {
                                    return "ASPECT_1_1"; // Default or fallback
                                }
                                return swarmAspectRatio switch
                                {
                                    "10:16" => "ASPECT_10_16",
                                    "16:10" => "ASPECT_16_10",
                                    "9:16" => "ASPECT_9_16",
                                    "16:9" => "ASPECT_16_9",
                                    "3:2" => "ASPECT_3_2",
                                    "2:3" => "ASPECT_2_3",
                                    "4:3" => "ASPECT_4_3",
                                    "3:4" => "ASPECT_3_4",
                                    "1:1" => "ASPECT_1_1",
                                    "1:3" => "ASPECT_1_3",
                                    "3:1" => "ASPECT_3_1",
                                    _ => "ASPECT_1_1" // Fallback for unsupported ratios
                                };
                            }
                            string modelName = input.Get(T2IParamTypes.Model).Name;
                            // Extract the actual model name without the folder path
                            string baseModelName = modelName;
                            if (modelName.Contains('/'))
                            {
                                baseModelName = modelName[(modelName.IndexOf('/') + 1)..];
                            }

                            // Convert model name to Ideogram format (uppercase, underscores)
                            string ideogramModelName = baseModelName.ToLowerInvariant() switch
                            {
                                "v1" => "V_1",
                                "v1-turbo" => "V_1_TURBO",
                                "v2" => "V_2",
                                "v2-turbo" => "V_2_TURBO",
                                "v2a" => "V_2A",
                                "v2a-turbo" => "V_2A_TURBO",
                                "v3" => "V_3",
                                // Add other known mappings here if needed
                                _ => baseModelName.Replace('-', '_').ToUpperInvariant() // Fallback attempt
                            };
                            string standard_aspect_ratio = input.Get(T2IParamTypes.AspectRatio)?.ToString();
                            string aspectRatioForRequest;
                            if (ideogramModelName == "V_3")
                            {
                                // v3 uses x format (4x3, 1x1, etc.)
                                aspectRatioForRequest = standard_aspect_ratio switch
                                {
                                    "1:3" => "1x3",
                                    "3:1" => "3x1", 
                                    "1:2" => "1x2",
                                    "2:1" => "2x1",
                                    "9:16" => "9x16",
                                    "16:9" => "16x9",
                                    "10:16" => "10x16", 
                                    "16:10" => "16x10",
                                    "2:3" => "2x3",
                                    "3:2" => "3x2",
                                    "3:4" => "3x4",
                                    "4:3" => "4x3",
                                    "4:5" => "4x5",
                                    "5:4" => "5x4",
                                    "1:1" => "1x1",
                                    _ => "1x1" // Default fallback
                                };
                            }
                            else
                            {
                                // for v1, v2 versions
                                aspectRatioForRequest = ConvertToIdeogramAspectRatio(standard_aspect_ratio);
                            }
                            
                            JObject requestBody = new()
                            {
                                ["prompt"] = input.Get(T2IParamTypes.Prompt).ToString(),
                                // ["negative_prompt"] = input.GetNegativePrompt(), // Optional
                                ["model"] = ideogramModelName, // Use the converted name
                                ["aspect_ratio"] = aspectRatioForRequest,
                                ["seed"] = input.Get(T2IParamTypes.Seed),
                                ["output_format"] = "png" // Request PNG format
                            };
                            // Add number of images to generate (default to 1 if not specified)
                            if (input.TryGet(T2IParamTypes.Images, out int numImages) && numImages > 0)
                            {
                                requestBody["num_images"] = numImages;
                            }
                            else
                            {
                                requestBody["num_images"] = 1; // Default to 1 image
                            }
                            // Optional parameters based on input and model version
                            if (input.TryGet(T2IParamTypes.Seed, out long seed) && seed != -1)
                            {
                                requestBody["seed"] = seed;
                            }
                            if (input.TryGet(SwarmUIAPIBackends.MagicPromptParam_Ideogram, out string magic) && magic != "AUTO")
                            {
                                requestBody["magic_prompt_option"] = magic;
                            }
                            // Style type only for V2+
                            if ((ideogramModelName.StartsWith("V2") || ideogramModelName.StartsWith("V3")) && input.TryGet(SwarmUIAPIBackends.StyleParam_Ideogram, out string style) && style != "AUTO")
                            {
                                requestBody["style_type"] = style;
                            }
                            // Color palette only for V2 & V2_TURBO
                            if ((ideogramModelName == "V2" || ideogramModelName == "V2_TURBO") && input.TryGet(SwarmUIAPIBackends.ColorPaletteParam_Ideogram, out string palette) && palette != "None")
                            {
                                requestBody["color_palette"] = new JObject { ["name"] = palette };
                            }
                            if (ideogramModelName == "V_3")
                            {
                                requestBody.Remove("model");
                                return requestBody;
                            }
                            JObject finalPayload = new()
                            {
                                ["aspect_ratio"] = aspectRatioForRequest,
                                ["image_request"] = requestBody,
                            };
                            return finalPayload;
                        },
                        ProcessResponse = async responseJson =>
                        {
                            // Check for error response with 'error' property (e.g., safety check failure)
                            if (responseJson["error"] != null)
                            {
                                string errorMessage = responseJson["error"].ToString();
                                Logs.Error($"[APIProviderInit] Ideogram API returned error: {errorMessage}");
                                throw new Exception($"Ideogram API error: {errorMessage}");
                            }
                            // Parse response to get image URLs from the 'data' array
                            JArray dataArray = responseJson["data"] as JArray;
                            if (dataArray == null || dataArray.Count == 0)
                            {
                                Logs.Error("[APIProviderInit] Ideogram API response missing 'data' array or array is empty");
                                throw new Exception("Ideogram API response missing image data");
                            }
                            // Get first image data
                            JToken imageData = dataArray[0];
                            string imageUrl = imageData["url"]?.ToString();
                            if (string.IsNullOrEmpty(imageUrl))
                            {
                                Logs.Error("[APIProviderInit] Ideogram API response missing image URL");
                                throw new Exception("Ideogram API response missing image URL");
                            }
                            try
                            {
                                // Download the image bytes
                                byte[] imageBytes = await HttpClient.GetByteArrayAsync(imageUrl);
                                return imageBytes;
                            }
                            catch (Exception ex)
                            {
                                Logs.Error($"[APIProviderInit] Ideogram ProcessResponse - Failed to download image: {ex.Message}");
                                throw new Exception($"Failed to download Ideogram image: {ex.Message}", ex);
                            }
                        }
                    }
                };
                Logs.Verbose($"[APIProviderInit] Ideogram provider initialized successfully with {providerModels.Count} models.");
                return provider;
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIProviderInit] Failed to initialize Ideogram provider: {ex.Message}");
                return null; // Return null to indicate failure
            }
        }

        public APIProviderMetadata InitializeBlackForestProvider()
        {
            try
            {
                // Define model-specific feature flags
                Dictionary<string, string> fluxModelFlags = new()
                {
                    ["flux-pro-1.1-ultra"] = "flux_ultra_params",
                    ["flux-pro-1.1"] = "flux_pro_params",
                    ["flux-dev"] = "flux_dev_params",
                    ["flux-kontext-pro"] = "flux_kontext_pro_params",
                    ["flux-kontext-max"] = "flux_kontext_max_params"
                };
                APIProviderMetadata provider = new()
                {
                    Name = "Black Forest Labs",
                    Models = new Dictionary<string, T2IModel>
                    {
                        ["BFL/flux-pro-1.1-ultra"] = new T2IModel(null, "BFL", "BFL/flux-pro-1.1-ultra", "flux-pro-1.1-ultra")
                        {
                            Title = "FLUX 1.1 Ultra",
                            Description = "Black Forest Labs' professional Flux model optimized for high-quality image generation with ultra-refined outputs",
                            ModelClass = CreateModelClass("bfl_api", "Flux"),
                            StandardWidth = 1024,
                            StandardHeight = 1024,
                            IsSupportedModelType = true,
                            PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro-ultra.png"))}",
                            Metadata = new T2IModelHandler.ModelMetadataStore
                            {
                                ModelName = "BFL/flux-pro-1.1-ultra",
                                Title = "FLUX 1.1 Ultra",
                                Author = "Black Forest Labs",
                                Description = "Professional Flux model optimized for high-quality image generation with ultra-refined outputs",
                                PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro-ultra.png"))}",
                                StandardWidth = 1024,
                                StandardHeight = 1024,
                                License = "Commercial",
                                UsageHint = "Best for high-quality professional image generation",
                                Date = "2024",
                                ModelClassType = "bfl_api",
                                Tags = ["flux", "professional", "high-quality", "ultra", fluxModelFlags["flux-pro-1.1-ultra"]],
                                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            },
                        },
                        ["BFL/flux-pro-1.1"] = new T2IModel(null, "BFL", "BFL/flux-pro-1.1", "flux-pro-1.1")
                        {
                            Title = "FLUX 1.1 Pro",
                            Description = "Black Forest Labs' Flux model",
                            ModelClass = CreateModelClass("bfl_api", "Flux"),
                            IsSupportedModelType = true,
                            StandardWidth = 1024,
                            StandardHeight = 768,
                            PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro.png"))}",
                            Metadata = new T2IModelHandler.ModelMetadataStore
                            {
                                ModelName = "BFL/flux-pro-1.1",
                                Title = "FLUX 1.1 Pro",
                                Author = "Black Forest Labs",
                                Description = "Professional Flux model for high-quality image generation",
                                PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro.png"))}",
                                StandardWidth = 1024,
                                StandardHeight = 768,
                                License = "Commercial",
                                UsageHint = "Great for professional image generation with detailed control",
                                Date = "2023",
                                ModelClassType = "bfl_api",
                                Tags = ["flux", "professional", "high-quality", fluxModelFlags["flux-pro-1.1"]],
                                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            },
                        },
                        ["BFL/flux-dev"] = new T2IModel(null, "BFL", "BFL/flux-dev", "flux-dev")
                        {
                            Title = "FLUX.1 Dev",
                            Description = "Black Forest Labs' Flux model",
                            ModelClass = CreateModelClass("bfl_api", "Flux"),
                            IsSupportedModelType = true,
                            StandardWidth = 1024,
                            StandardHeight = 768,
                            PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-dev.png"))}",
                            Metadata = new T2IModelHandler.ModelMetadataStore
                            {
                                ModelName = "BFL/flux-dev",
                                Title = "FLUX.1 Dev",
                                Author = "Black Forest Labs",
                                Description = "Developmental Flux model for general-purpose image generation",
                                PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-dev.png"))}",
                                StandardWidth = 1024,
                                StandardHeight = 768,
                                License = "Commercial",
                                UsageHint = "Good for general-purpose image generation",
                                Date = "2023",
                                ModelClassType = "bfl_api",
                                Tags = ["flux", "development", fluxModelFlags["flux-dev"]],
                                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            },
                        },
                        ["BFL/flux-kontext-pro"] = new T2IModel(null, "BFL", "BFL/flux-kontext-pro", "flux-kontext-pro")
                        {
                            Title = "FLUX.1 Kontext Pro",
                            Description = "Black Forest Labs' Flux Kontext Pro model",
                            ModelClass = CreateModelClass("bfl_api", "Flux"),
                            IsSupportedModelType = true,
                            StandardWidth = 1024,
                            StandardHeight = 768,
                            PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-kontext-pro.png"))}",
                            Metadata = new T2IModelHandler.ModelMetadataStore
                            {
                                ModelName = "BFL/flux-kontext-pro",
                                Title = "FLUX.1 Kontext Pro",
                                Author = "Black Forest Labs",
                                Description = "A unified model for fast editing, generation, and text-to-image with FLUX.1 quality, preserving character consistency across iterations",
                                PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-kontext-pro.png"))}",
                                StandardWidth = 1024,
                                StandardHeight = 768,
                                License = "Commercial",
                                UsageHint = "Ideal for fast, consistent image editing and generation with context-aware prompts.",
                                Date = "2025",
                                ModelClassType = "bfl_api",
                                Tags = ["flux", "high-quality", fluxModelFlags["flux-kontext-pro"]],
                                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            },
                        },
                        ["BFL/flux-kontext-max"] = new T2IModel(null, "BFL", "BFL/flux-kontext-max", "flux-kontext-max")
                        {
                            Title = "FLUX.1 Kontext Max",
                            Description = "Black Forest Labs' Flux Kontext Max model",
                            ModelClass = CreateModelClass("bfl_api", "Flux"),
                            IsSupportedModelType = true,
                            StandardWidth = 1024,
                            StandardHeight = 768,
                            PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-kontext-max.png"))}",
                            Metadata = new T2IModelHandler.ModelMetadataStore
                            {
                                ModelName = "BFL/flux-kontext-max",
                                Title = "FLUX.1 Kontext Max",
                                Author = "Black Forest Labs",
                                Description = "Delivers maximum speed, precision, and consistency for image editing and generation, maintaining character fidelity across multiple iterations",
                                PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-kontext-max.png"))}",
                                StandardWidth = 1024,
                                StandardHeight = 768,
                                License = "Commercial",
                                UsageHint = "Ideal for fast, consistent image editing and generation with context-aware prompts.",
                                Date = "2025",
                                ModelClassType = "bfl_api",
                                Tags = ["flux", "high-quality", fluxModelFlags["flux-kontext-max"]],
                                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            },
                        }
                    },
                    RequestConfig = new RequestConfig
                    {
                        BaseUrl = "https://api.us1.bfl.ai",
                        AuthHeader = "x-key",
                        BuildRequest = input => BuildBlackForestRequest(input),
                        ProcessResponse = async response =>
                        {
                            string taskId = (string)response["id"];
                            Logs.Verbose($"BFL Task ID: {taskId}");
                            while (true)
                            {
                                HttpResponseMessage result = await HttpClient.GetAsync($"https://api.us1.bfl.ai/v1/get_result?id={taskId}");
                                JObject resultJson = JObject.Parse(await result.Content.ReadAsStringAsync());
                                string status = (string)resultJson["status"];
                                if (status == "Ready" || status == "completed")
                                {
                                    string url = (string)resultJson["result"]["sample"];
                                    return await HttpClient.GetByteArrayAsync(url);
                                }
                                Logs.Verbose($"BFL Task status: {status}");
                                await Task.Delay(500);
                            }
                        }
                    }
                };
                // Register parameters for all Flux models supported by Black Forest Labs API
                foreach (string modelName in new[] { "BFL/flux-pro-1.1-ultra", "BFL/flux-pro-1.1", "BFL/flux-dev", "BFL/flux-kontext-pro", "BFL/flux-kontext-max" })
                {
                    provider.AddParameterToModel(modelName, "prompt_upsampling", SwarmUIAPIBackends.PromptEnhanceParam_BlackForest);
                    provider.AddParameterToModel(modelName, "safety_tolerance", SwarmUIAPIBackends.SafetyParam_BlackForest);
                    provider.AddParameterToModel(modelName, "output_format", SwarmUIAPIBackends.OutputFormatParam_BlackForest);
                }
                // guidance paramter not supported on flux-kontext models
                foreach (string modelName in new[] { "BFL/flux-pro-1.1-ultra", "BFL/flux-pro-1.1", "BFL/flux-dev" })
                {
                    provider.AddParameterToModel(modelName, "guidance", SwarmUIAPIBackends.GuidanceParam_BlackForest);
                }
                // Interval is only supported on some models
                foreach (string modelName in new[] { "BFL/flux-pro-1.1-ultra", "BFL/flux-pro-1.1" })
                {
                    provider.AddParameterToModel(modelName, "interval", SwarmUIAPIBackends.IntervalParam_BlackForest);
                }
                // Raw mode is only supported on ultra model
                provider.AddParameterToModel("BFL/flux-pro-1.1-ultra", "raw", SwarmUIAPIBackends.RawModeParam_BlackForest);
                Logs.Verbose("[APIProviderInit] Black Forest Labs provider successfully initialized with 3 models");
                return provider;
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIProviderInit] Failed to initialize Black Forest Labs provider: {ex.Message}");
                throw;
            }
        }

        private static JObject BuildBlackForestRequest(T2IParamInput input)
        {
            string modelName = input.Get(T2IParamTypes.Model).Name;
            // Extract base model name without folder path
            string baseModelName = modelName;
            if (modelName.Contains('/'))
            {
                baseModelName = modelName[(modelName.IndexOf('/') + 1)..];
            }
            JObject request = new()
            {
                // Common parameters for all Flux models
                ["prompt"] = input.Get(T2IParamTypes.Prompt),
                ["prompt_upsampling"] = input.Get(SwarmUIAPIBackends.PromptEnhanceParam_BlackForest),
                ["output_format"] = input.Get(SwarmUIAPIBackends.OutputFormatParam_BlackForest)
            };
            // Add safety parameter default is 2. Higher = more risky, lower = safer
            if (input.TryGet(SwarmUIAPIBackends.SafetyParam_BlackForest, out int safetyLevel))
            {
                request["safety_tolerance"] = safetyLevel;
            }
            else
            {
                request["safety_tolerance"] = 2;
            }
            if (input.TryGet(T2IParamTypes.Seed, out long seed) && seed != -1)
            {
                request["seed"] = seed;
            }
            else
            {
                request["seed"] = new Random().Next(1, 2147483647);
            }
            // Model-specific parameters
            if (baseModelName.Contains("flux-pro-1.1-ultra"))
            {
                // Ultra model uses aspect_ratio instead of width/height
                if (input.TryGet(T2IParamTypes.AspectRatio, out string aspectRatio))
                {
                    request["aspect_ratio"] = aspectRatio;
                }
                else
                {
                    request["aspect_ratio"] = "16:9";
                }

                if (input.TryGet(SwarmUIAPIBackends.GuidanceParam_BlackForest, out double guidance))
                {
                    request["guidance"] = guidance;
                }
                else
                {
                    request["guidance"] = 2.5;
                }

                if (input.TryGet(SwarmUIAPIBackends.RawModeParam_BlackForest, out bool rawMode))
                {
                    request["raw"] = rawMode;
                }

                if (input.TryGet(T2IParamTypes.Steps, out int steps))
                {
                    request["steps"] = steps;
                }
                else
                {
                    request["steps"] = 25;
                }
                // For Ultra model, don't include image_prompt if not provided
                request["image_prompt"] = null;
            }
            else
            {
                // Other models require explicit width/height that are multiples of 32
                if (input.TryGet(T2IParamTypes.Width, out int width))
                {
                    width = (width + 31) / 32 * 32;  // Round up to nearest multiple of 32
                }
                else
                {
                    width = 1024;
                }

                if (input.TryGet(T2IParamTypes.Height, out int height))
                {
                    height = (height + 31) / 32 * 32; // Round up to nearest multiple of 32
                }
                else
                {
                    height = 768;
                }
                request["width"] = width;
                request["height"] = height;
                if (input.TryGet(SwarmUIAPIBackends.GuidanceParam_BlackForest, out double guidance))
                {
                    request["guidance"] = guidance;
                }
                else
                {
                    request["guidance"] = baseModelName.Contains("flux-dev") ? 3.0 : 2.5;
                }
                if (input.TryGet(T2IParamTypes.Steps, out int steps))
                {
                    request["steps"] = steps;
                }
                else
                {
                    request["steps"] = baseModelName.Contains("flux-dev") ? 28 : 40;
                }
                // Add interval parameter for flux-pro-1.1 (non-ultra)
                if (baseModelName.Contains("flux-pro-1.1") && !baseModelName.Contains("ultra"))
                {
                    if (input.TryGet(SwarmUIAPIBackends.IntervalParam_BlackForest, out double interval))
                    {
                        request["interval"] = interval;
                    }
                    else
                    {
                        request["interval"] = 2.0;
                    }
                }
            }
            return request;
        }
    }
}