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

namespace Hartsy.Extensions.APIBackends.Backends;

/// <summary>Initializes and manages API providers for image generation.</summary>
public class APIProviderInit : IDisposable
{
    private readonly HttpClient _httpClient;
    public HttpClient HttpClient => _httpClient;
    private bool _disposed;
    private static readonly Dictionary<string, T2IModelClass> _modelClasses = [];
    public Dictionary<string, APIProviderMetadata> Providers { get; private set; }

    public APIProviderInit()
    {
        _httpClient = new HttpClient();
        InitializeProviders();
    }

    private static T2IModelClass CreateModelClass(string id, string name)
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

    private void InitializeProviders()
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

    private static APIProviderMetadata InitializeOpenAIProvider()
    {
        T2IModel dallE2 = new(null, null, null, "dall-e-2")
        {
            Title = "DALL-E 2",
            Description = "OpenAI's DALL-E 2 model",
            ModelClass = CreateModelClass("dall-e", "DALL-E"),
            StandardWidth = 1024,
            StandardHeight = 1024
        };

        T2IModel dallE3 = new(null, null, null, "dall-e-3")
        {
            Title = "DALL-E 3",
            Description = "OpenAI's DALL-E 3 model",
            ModelClass = CreateModelClass("dall-e", "DALL-E"),
            StandardWidth = 1024,
            StandardHeight = 1024
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

    private APIProviderMetadata InitializeIdeogramProvider()
    {
        var provider = new APIProviderMetadata
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
                    StandardWidth = 1024
                },
                ["v2"] = new T2IModel(null, "Ideogram", "v2", "ideogram")
                {
                    Title = "Ideogram v2",
                    ModelClass = CreateModelClass("ideogram", "Ideogram"),
                    Description = "Ideogram's image generation model",
                    StandardHeight = 1024,
                    StandardWidth = 1024
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
                    return await _httpClient.GetByteArrayAsync(url);
                }
            }
        };
        // Set model-specific parameters
        foreach (string version in new[] { "v1", "v2" })
        {
            provider.AddParameterToModel(version, "style_type", SwarmUIAPIBackends.StyleParam_Ideogram);
            provider.AddParameterToModel(version, "magic_prompt", SwarmUIAPIBackends.MagicPromptParam_Ideogram);
        }
        return provider;
    }

    private APIProviderMetadata InitializeBlackForestProvider()
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
                    ModelClass = CreateModelClass("flux", "Flux"),
                    StandardWidth = 1024,
                    StandardHeight = 1024,
                    IsSupportedModelType = true,
                    PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-APIBackends/Images/ModelPreviews/flux-pro-ultra.png"))}",
                    Metadata = new T2IModelHandler.ModelMetadataStore
                    {
                        ModelName = "API/flux-pro-1.1-ultra.safetensors",
                        Title = "FLUX 1.1 Ultra",
                        Author = "Black Forest Labs",
                        Description = "Professional Flux model optimized for high-quality image generation with ultra-refined outputs",
                        PreviewImage = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes("src/Extensions/SwarmUI-APIBackends/Images/ModelPreviews/flux-pro-ultra.png"))}",
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
                    ModelClass = CreateModelClass("flux", "Flux"),
                    IsSupportedModelType = true,
                    StandardWidth = 1024,
                    StandardHeight = 768
                },
                ["API/flux-dev"] = new T2IModel(null, "API", "API/flux-dev.safetensors", "flux-dev")
                {
                    Title = "FLUX.1 Dev",
                    Description = "Black Forest Labs' Flux model",
                    ModelClass = CreateModelClass("flux", "Flux"),
                    IsSupportedModelType = true,
                    StandardWidth = 1024,
                    StandardHeight = 768
                }
            },
            RequestConfig = new RequestConfig
            {
                BaseUrl = "https://api.us1.bfl.ai",
                AuthHeader = "x-key",
                BuildRequest = input => new JObject
                {
                    ["prompt"] = input.Get(T2IParamTypes.Prompt),
                    ["width"] = input.Get(T2IParamTypes.Width),
                    ["height"] = input.Get(T2IParamTypes.Height),
                    ["prompt_upsampling"] = input.Get(SwarmUIAPIBackends.PromptEnhanceParam_BlackForest),
                    ["output_format"] = input.Get(SwarmUIAPIBackends.OutputFormatParam_BlackForest),
                    ["steps"] = input.Get(T2IParamTypes.Steps),
                    ["guidance"] = input.Get(SwarmUIAPIBackends.GuidanceParam_BlackForest),
                    ["interval"] = input.TryGet(SwarmUIAPIBackends.IntervalParam_BlackForest, out var interval) ? interval : null
                },
                ProcessResponse = async response =>
                {
                    string taskId = (string)response["id"];
                    Logs.Debug($"BFL Task ID: {taskId}");
                    while (true)
                    {
                        HttpResponseMessage result = await _httpClient.GetAsync($"https://api.us1.bfl.ai/v1/get_result?id={taskId}");
                        JObject resultJson = JObject.Parse(await result.Content.ReadAsStringAsync());
                        string status = (string)resultJson["status"];
                        if (status == "Ready" || status == "completed")
                        {
                            string url = (string)resultJson["result"]["sample"];
                            return await _httpClient.GetByteArrayAsync(url);
                        }
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

    public void Dispose()
    {
        if (!_disposed)
        {
            Logs.Debug("Disposing APIProviderInit...");
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}