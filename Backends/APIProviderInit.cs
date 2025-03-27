using SwarmUI.Text2Image;
using Newtonsoft.Json.Linq;
using SwarmUI.Utils;
using Hartsy.Extensions.APIBackends.Models;
using System.Net.Http;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Reflection;
using FreneticUtilities.FreneticExtensions;
using SwarmUI.Backends;

namespace Hartsy.Extensions.APIBackends.Backends;

/// <summary>Initializes and manages API providers for image generation.</summary>
public class APIProviderInit
{
    public static readonly Dictionary<string, T2IModelClass> _modelClasses = [];
    public Dictionary<string, APIProviderMetadata> Providers { get; private set; }

    public APIProviderInit()
    {
        InitializeProviders();
    }

    public static T2IModelClass CreateModelClass(string id, string name)
    {
        if (!_modelClasses.TryGetValue(id, out T2IModelClass modelClass))
        {
            string prefixedId = id switch
            {
                "flux" => "bfl-api-flux",
                "dall-e" => "openai-api-dalle",
                "ideogram" => "ideogram-api",
                _ => id
            };
            modelClass = new T2IModelClass
            {
                ID = prefixedId,
                Name = name,
                CompatClass = id,
                StandardWidth = 1024,
                StandardHeight = 1024,
                IsThisModelOfClass = (model, header) => true
            };
            _modelClasses[id] = modelClass;
            T2IModelClassSorter.Register(modelClass);
            Logs.Debug($"Registered API model class in CreateModelClass(): {prefixedId} ({name})");
        }
        return modelClass;
    }

    public void InitializeProviders()
    {
        Dictionary<string, string> providerKeys = new()
        {
            ["BFL (Flux)"] = "bfl",
            ["OpenAI (DALL-E)"] = "openai",
            ["Ideogram"] = "ideogram"
        };
        Providers = [];
        foreach (var (displayName, key) in providerKeys)
        {
            Logs.Debug($"Initializing provider: {displayName}");
            APIProviderMetadata provider = key switch
            {
                "bfl" => InitializeBlackForestProvider(),
                "openai" => InitializeOpenAIProvider(),
                "ideogram" => InitializeIdeogramProvider(),
                _ => throw new ArgumentException($"Unknown provider key: {key}")
            };
            Providers[key] = provider;
        }
    }

    public static APIProviderMetadata InitializeOpenAIProvider()
    {
        T2IModel dallE2 = new(null, null, null, "dall-e-2")
        {
            Title = "DALL-E 2",
            Description = "OpenAI's DALL-E 2 model",
            ModelClass = CreateModelClass("dall-e", "DALL-E"),
            StandardWidth = 1024,
            StandardHeight = 1024,
            IsSupportedModelType = true,
            PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle2.png"))}",
            Metadata = new T2IModelHandler.ModelMetadataStore
            {
                ModelName = "dall-e-2",
                Title = "DALL-E 2",
                Author = "OpenAI",
                Description = "Generative model capable of creating realistic images from text descriptions",
                PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle2.png"))}",
                StandardWidth = 1024,
                StandardHeight = 1024,
                License = "Commercial",
                UsageHint = "Good for general image generation via API",
                Date = "2022",
                ModelClassType = "dall-e",
                Tags = ["openai", "dall-e", "generative"],
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            },
        };
        T2IModel dallE3 = new(null, null, null, "dall-e-3")
        {
            Title = "DALL-E 3",
            Description = "OpenAI's DALL-E 3 model",
            ModelClass = CreateModelClass("dall-e", "DALL-E"),
            StandardWidth = 1024,
            StandardHeight = 1024,
            IsSupportedModelType = true,
            PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle.png"))}",
            Metadata = new T2IModelHandler.ModelMetadataStore
            {
                ModelName = "dall-e-3",
                Title = "DALL-E 3",
                Author = "OpenAI",
                Description = "Advanced generative model for creating highly detailed and accurate images from text descriptions",
                PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/dalle.png"))}",
                StandardWidth = 1024,
                StandardHeight = 1024,
                License = "Commercial",
                UsageHint = "Excellent for high-quality image generation with accurate text representation",
                Date = "2023",
                ModelClassType = "dall-e",
                Tags = ["openai", "dall-e", "high-quality", "text-accurate"],
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            },
        };
        APIProviderMetadata provider = new()
        {
            Name = "OpenAI",
            Models = new Dictionary<string, T2IModel>
            {
                ["dall-e-2"] = dallE2,
                ["dall-e-3"] = dallE3
            },
            RequestConfig = new RequestConfig
            {
                BaseUrl = "https://api.openai.com/v1/images/generations",
                AuthHeader = "Bearer",
                BuildRequest = input => new JObject
                {
                    ["prompt"] = input.Get(T2IParamTypes.Prompt),
                    ["model"] = input.Get(T2IParamTypes.Model).Name,
                    ["n"] = input.Get(T2IParamTypes.Images),
                    ["quality"] = input.Get(SwarmUIAPIBackends.QualityParam_OpenAI),
                    ["style"] = input.Get(SwarmUIAPIBackends.StyleParam_OpenAI),
                    ["size"] = input.Get(SwarmUIAPIBackends.SizeParam_OpenAI),
                    ["response_format"] = "b64_json"
                },
                ProcessResponse = async response =>
                {
                    string b64 = response["data"][0]["b64_json"].ToString();
                    return Convert.FromBase64String(b64);
                }
            }
        };
        provider.AddParameterToModel("dall-e-2", "size", SwarmUIAPIBackends.SizeParam_OpenAI);
        provider.AddParameterToModel("dall-e-3", "size", SwarmUIAPIBackends.SizeParam_OpenAI);
        provider.AddParameterToModel("dall-e-3", "quality", SwarmUIAPIBackends.QualityParam_OpenAI);
        provider.AddParameterToModel("dall-e-3", "style", SwarmUIAPIBackends.StyleParam_OpenAI);
        return provider;
    }

    public APIProviderMetadata InitializeIdeogramProvider()
    {
        APIProviderMetadata provider = new()
        {
            Name = "Ideogram",
            Models = new Dictionary<string, T2IModel>
            {
                ["v1"] = new T2IModel(null, "Ideogram", "v1", "ideogram")
                {
                    Title = "Ideogram v1",
                    ModelClass = CreateModelClass("ideogram", "Ideogram"),
                    Description = "Ideogram's image generation model",
                    StandardHeight = 1024,
                    StandardWidth = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/ideogram.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "ideogram-v1",
                        Title = "Ideogram v1",
                        Author = "Ideogram AI",
                        Description = "Ideogram's first-generation image synthesis model optimized for creative content",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/ideogram.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        License = "Commercial",
                        UsageHint = "Great for creative image generation with strong artistic style",
                        Date = "2023",
                        ModelClassType = "ideogram",
                        Tags = ["ideogram", "creative", "ai-art"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                },
                ["v2"] = new T2IModel(null, "Ideogram", "v2", "ideogram")
                {
                    Title = "Ideogram v2",
                    ModelClass = CreateModelClass("ideogram", "Ideogram"),
                    Description = "Ideogram's image generation model",
                    StandardHeight = 1024,
                    StandardWidth = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/ideogram.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "ideogram-v2",
                        Title = "Ideogram v2",
                        Author = "Ideogram AI",
                        Description = "Ideogram's second-generation image synthesis model with improved quality and control",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/ideogram.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        License = "Commercial",
                        UsageHint = "Excellent for high-quality creative images with enhanced detail",
                        Date = "2024",
                        ModelClassType = "ideogram",
                        Tags = ["ideogram", "creative", "ai-art", "professional"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                },
                ["v3"] = new T2IModel(null, "Ideogram", "v3", "ideogram")
                {
                    Title = "Ideogram v3",
                    ModelClass = CreateModelClass("ideogram", "Ideogram"),
                    Description = "Ideogram's latest and most advanced image generation model",
                    StandardHeight = 1024,
                    StandardWidth = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/ideogram.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "ideogram-v3",
                        Title = "Ideogram v3",
                        Author = "Ideogram AI",
                        Description = "Ideogram's third-generation image synthesis model with state-of-the-art quality, detail and creative control",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/ideogram.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        License = "Commercial",
                        UsageHint = "Cutting-edge image generation with exceptional quality and precise prompt following",
                        Date = "2025",
                        ModelClassType = "ideogram",
                        Tags = ["ideogram", "creative", "ai-art", "professional", "cutting-edge"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                }
            },
            RequestConfig = new RequestConfig
            {
                BaseUrl = "https://api.ideogram.ai/api/v1/images/generate",
                AuthHeader = "Bearer",
                BuildRequest = input => new JObject
                {
                    ["prompt"] = input.Get(T2IParamTypes.Prompt),
                    ["model"] = input.Get(T2IParamTypes.Model).Name.ToUpperInvariant(),
                    ["style_type"] = input.Get(SwarmUIAPIBackends.StyleParam_Ideogram),
                    ["aspect_ratio"] = input.Get(T2IParamTypes.AspectRatio),
                    ["magic_prompt_option"] = input.Get(SwarmUIAPIBackends.MagicPromptParam_Ideogram),
                    ["negative_prompt"] = input.Get(T2IParamTypes.NegativePrompt),
                    ["num_images"] = input.Get(T2IParamTypes.Images)
                },
                ProcessResponse = async response =>
                {
                    string url = response["images"][0]["url"].ToString();
                    return await SwarmSwarmBackend.HttpClient.GetByteArrayAsync(url);
                }
            }
        };
        // Set model-specific parameters
        foreach (string version in new[] { "v1", "v2", "v3" })
        {
            provider.AddParameterToModel(version, "style_type", SwarmUIAPIBackends.StyleParam_Ideogram);
            provider.AddParameterToModel(version, "magic_prompt", SwarmUIAPIBackends.MagicPromptParam_Ideogram);
        }
        return provider;
    }

    public APIProviderMetadata InitializeBlackForestProvider()
    {
        APIProviderMetadata provider = new()
        {
            Name = "Black Forest Labs",
            Models = new Dictionary<string, T2IModel>
            {
                ["API/flux-pro-1.1-ultra"] = new T2IModel(null, "API", "API/flux-pro-1.1-ultra.safetensors", "flux-pro-1.1-ultra")
                {
                    Title = "FLUX 1.1 Ultra",
                    Description = "Black Forest Labs' professional Flux model optimized for high-quality image generation with ultra-refined outputs",
                    ModelClass = CreateModelClass("bfl", "Flux"),
                    StandardWidth = 1024,
                    StandardHeight = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro-ultra.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "API/flux-pro-1.1-ultra.safetensors",
                        Title = "FLUX 1.1 Ultra",
                        Author = "Black Forest Labs",
                        Description = "Professional Flux model optimized for high-quality image generation with ultra-refined outputs",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro-ultra.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 1024,
                        License = "Commercial",
                        UsageHint = "Best for high-quality professional image generation",
                        Date = "2024",
                        ModelClassType = "flux",
                        Tags = ["flux", "professional", "high-quality", "ultra"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                },
                ["API/flux-pro-1.1"] = new T2IModel(null, "API", "API/flux-pro-1.1.safetensors", "flux-pro-1.1")
                {
                    Title = "FLUX 1.1 Pro",
                    Description = "Black Forest Labs' Flux model",
                    ModelClass = CreateModelClass("bfl", "Flux"),
                    IsSupportedModelType = true,
                    StandardWidth = 1024,
                    StandardHeight = 768,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "API/flux-pro-1.1.safetensors",
                        Title = "FLUX 1.1 Pro",
                        Author = "Black Forest Labs",
                        Description = "Professional Flux model for high-quality image generation",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-pro.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 768,
                        License = "Commercial",
                        UsageHint = "Great for professional image generation with detailed control",
                        Date = "2023",
                        ModelClassType = "flux",
                        Tags = ["flux", "professional", "high-quality"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                },
                ["API/flux-dev"] = new T2IModel(null, "API", "API/flux-dev.safetensors", "flux-dev")
                {
                    Title = "FLUX.1 Dev",
                    Description = "Black Forest Labs' Flux model",
                    ModelClass = CreateModelClass("bfl", "Flux"),
                    IsSupportedModelType = true,
                    StandardWidth = 1024,
                    StandardHeight = 768,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-dev.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "API/flux-dev.safetensors",
                        Title = "FLUX.1 Dev",
                        Author = "Black Forest Labs",
                        Description = "Developmental Flux model for general-purpose image generation",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-API-Backends/Images/ModelPreviews/flux-dev.png"))}",
                        StandardWidth = 1024,
                        StandardHeight = 768,
                        License = "Commercial",
                        UsageHint = "Good for general-purpose image generation",
                        Date = "2023",
                        ModelClassType = "flux",
                        Tags = ["flux", "development"],
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                }
            },
            RequestConfig = new RequestConfig
            {
                BaseUrl = "https://api.us1.bfl.ai",
                AuthHeader = "x-key",
                BuildRequest = input =>
                {
                    // Get the model name to determine the appropriate request format
                    string modelName = input.Get(T2IParamTypes.Model).Name;
                    JObject request = new()
                    {
                        // Common parameters for all models
                        ["prompt"] = input.Get(T2IParamTypes.Prompt),
                        ["prompt_upsampling"] = input.Get(SwarmUIAPIBackends.PromptEnhanceParam_BlackForest),
                        ["output_format"] = input.Get(SwarmUIAPIBackends.OutputFormatParam_BlackForest),
                        ["safety_tolerance"] = input.TryGet(SwarmUIAPIBackends.SafetyParam_BlackForest, out int safety) ? safety : 2
                    };
                    // Model-specific parameters
                    if (modelName.Contains("flux-pro-1.1-ultra"))
                    {
                        // Ultra model uses aspect_ratio
                        request["aspect_ratio"] = input.TryGet(T2IParamTypes.AspectRatio, out string aspect) ? aspect : "16:9";
                        request["guidance"] = input.TryGet(SwarmUIAPIBackends.GuidanceParam_BlackForest, out double guidance) ? guidance : 2.5;
                        request["raw"] = input.TryGet(SwarmUIAPIBackends.RawModeParam_BlackForest, out bool raw) && raw;
                        request["steps"] = input.TryGet(T2IParamTypes.Steps, out int steps) ? steps : 25;
                        request["seed"] = 1;
                        // Process image prompt if present
                        if (input.TryGet(SwarmUIAPIBackends.ImagePromptParam_BlackForest, out Image img) && img != null)
                        {
                            //TODO: Implement image prompt processing (Init Image)
                            request["image_prompt_strength"] = input.TryGet(SwarmUIAPIBackends.ImagePromptStrengthParam_BlackForest, out double strength) ? strength : 0.1;
                        }
                        else
                        {
                            request["image_prompt"] = "";
                            request["image_prompt_strength"] = 0.1;
                        }
                    }
                    else
                    {
                        // Other models require explicit width/height that are multiples of 32
                        int width = input.TryGet(T2IParamTypes.Width, out int w) ? w : 1024;
                        int height = input.TryGet(T2IParamTypes.Height, out int h) ? h : 768;
                        // Ensure dimensions are multiples of 32
                        width = (width + 31) / 32 * 32;  // Round up to nearest multiple of 32
                        height = (height + 31) / 32 * 32; // Round up to nearest multiple of 32
                        request["width"] = width;
                        request["height"] = height;
                        request["guidance"] = input.TryGet(SwarmUIAPIBackends.GuidanceParam_BlackForest, out double guidance) ? guidance : modelName.Contains("flux-dev") ? 3.0 : 2.5;
                        request["steps"] = input.TryGet(T2IParamTypes.Steps, out int steps) ? steps : modelName.Contains("flux-dev") ? 28 : 40;
                        request["seed"] = 1;
                        // Add interval parameter for flux-pro-1.1
                        if (modelName.Contains("flux-pro-1.1") && !modelName.Contains("ultra"))
                        {
                            request["interval"] = input.TryGet(SwarmUIAPIBackends.IntervalParam_BlackForest, out double interval) ? interval : 2.0;
                        }
                    }
                    return request;
                },
                ProcessResponse = async response =>
                {
                    string taskId = (string)response["id"];
                    Logs.Debug($"BFL Task ID: {taskId}");
                    while (true)
                    {
                        HttpResponseMessage result = await SwarmSwarmBackend.HttpClient.GetAsync($"https://api.us1.bfl.ai/v1/get_result?id={taskId}");
                        JObject resultJson = JObject.Parse(await result.Content.ReadAsStringAsync());
                        string status = (string)resultJson["status"];
                        if (status == "Ready" || status == "completed")
                        {
                            string url = (string)resultJson["result"]["sample"];
                            return await SwarmSwarmBackend.HttpClient.GetByteArrayAsync(url);
                        }
                        // TODO: Check for image getting caught in moderation and return a placeholder image
                        Logs.Debug($"BFL Task status: {status}");
                        await Task.Delay(500);
                    }
                }
            }
        };
        foreach (var (modelName, model) in provider.Models)
        {
            Logs.Debug($"Model initialization debug for {modelName}:");
            Logs.Debug($"- Raw file path: {model.RawFilePath}");
            Logs.Debug($"- File extension: {(model.RawFilePath ?? "").AfterLast('.')}");
            Logs.Debug($"- Is supported type: {model.IsSupportedModelType}");
            Logs.Debug($"- Model class: {model.ModelClass?.ID}");
        }
        foreach (string modelName in new[] { "flux-pro-1.1-ultra", "flux-pro-1.1", "flux-dev" })
        {
            provider.AddParameterToModel(modelName, "prompt_upsampling", SwarmUIAPIBackends.PromptEnhanceParam_BlackForest);
            provider.AddParameterToModel(modelName, "safety_tolerance", SwarmUIAPIBackends.SafetyParam_BlackForest);
            provider.AddParameterToModel(modelName, "output_format", SwarmUIAPIBackends.OutputFormatParam_BlackForest);
            provider.AddParameterToModel(modelName, "guidance", SwarmUIAPIBackends.GuidanceParam_BlackForest);
        }
        // Interval is only supported on some models
        foreach (string modelName in new[] { "flux-pro-1.1-ultra", "flux-pro-1.1" })
        {
            provider.AddParameterToModel(modelName, "interval", SwarmUIAPIBackends.IntervalParam_BlackForest);
        }
        return provider;
    }
}