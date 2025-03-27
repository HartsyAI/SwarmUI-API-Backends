using Newtonsoft.Json.Linq;
using SwarmUI.Backends;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using Hartsy.Extensions.APIBackends.Models;
using FreneticUtilities.FreneticDataSyntax;
using System.Net.Http;
using SwarmUI.Accounts;

namespace Hartsy.Extensions.APIBackends.Backends;

/// <summary>A dynamic backend that supports multiple API providers for image generation.</summary>
public class DynamicAPIBackend : SwarmSwarmBackend
{
    /// <summary>Settings for the dynamic API backend.</summary>
    public class DynamicAPISettings : AutoConfiguration
    {
        [ManualSettingsOptions(Impl = null, Vals = ["BFL (Flux)", "Ideogram", "OpenAI (DALL-E)"])]
        [ConfigComment("Choose the backend API provider to use for image generation. This will add the relevant models to your model list.")]
        public string SelectedProvider = "BFL (Flux)";

        [ConfigComment("Whether the backend is allowed to revert to an 'idle' state if the API is unresponsive.")]
        public bool AllowIdle = false;

        [ConfigComment("Custom Base URL (optional) This value will override the hardcoded base URL.")]
        public string CustomBaseUrl = "";
    }

    public new DynamicAPISettings Settings => SettingsRaw as DynamicAPISettings;
    public APIProviderMetadata ActiveProvider => _providerInit.Value.Providers[NormalizeProviderKey(Settings.SelectedProvider)];
    public static readonly Lazy<APIProviderInit> _providerInit = new(() => new APIProviderInit());

    public static string NormalizeProviderKey(string provider) => provider switch
    {
        "BFL (Flux)" => "bfl",
        "OpenAI (DALL-E)" => "openai",
        "Ideogram" => "ideogram",
        _ => provider.ToLowerInvariant()
    };

    public override async Task Init()
    {
        Logs.Debug($"Initializing DynamicAPIBackend with provider: {Settings.SelectedProvider}");
        Models = new ConcurrentDictionary<string, List<string>>();
        RemoteModels = [];
        RemoteBackendTypes = [];
        string normalizedProvider = NormalizeProviderKey(Settings.SelectedProvider);
        SettingsRaw = Settings;
        RemoteFeatureCombo = new ConcurrentDictionary<string, string>();
        IsReal = false;
        Status = BackendStatus.LOADING;
        try
        {
            APIProviderMetadata provider = ActiveProvider;
            Logs.Debug($"Got provider metadata for {normalizedProvider}, with {provider.Models.Count} models");
            T2IModelHandler handler = Program.T2IModelSets["Stable-Diffusion"];
            switch (normalizedProvider)
            {
                case "bfl":
                    RemoteFeatureCombo.TryAdd("bfl-api", "bfl-api");
                    RemoteBackendTypes.Add("bfl");
                    break;
                case "openai":
                    RemoteFeatureCombo.TryAdd("openai-api", "openai-api");
                    RemoteBackendTypes.Add("dall-e");
                    break;
                case "ideogram":
                    RemoteFeatureCombo.TryAdd("ideogram-api", "ideogram-api");
                    RemoteBackendTypes.Add("ideogram");
                    break;
            }
            // Register models with the handler
            Dictionary<string, JObject> models = [];
            List<string> modelNames = [];
            foreach (var (name, model) in provider.Models)
            {
                string cleanName = name.Replace("API/", "").Replace(".safetensors", "");
                JObject modelObj = model.ToNetObject();
                modelObj["loaded"] = true;
                modelObj["local"] = false;
                modelObj["features"] = JArray.FromObject(RemoteFeatureCombo.Keys);
                modelObj["display"] = model.Title;
                modelObj["name"] = name;
                modelObj["clean_name"] = cleanName;
                modelObj["raw_name"] = cleanName;
                Logs.Debug($"Registering API model: {name} with arch={model.ModelClass?.ID}, class={model.ModelClass?.Name}, compat={model.ModelClass?.CompatClass}");
                models[name] = modelObj;
                modelNames.Add(name);
                handler.ResetMetadataFrom(model);
            }
            RemoteModels["Stable-Diffusion"] = models;
            Models.TryAdd("Stable-Diffusion", modelNames);
            Logs.Debug($"DynamicAPIBackend initialized with {modelNames.Count} models for {normalizedProvider}");
            Status = BackendStatus.RUNNING;
        }
        catch (Exception ex)
        {
            Logs.Error($"Failed to initialize DynamicAPIBackend: {ex}");
            Status = BackendStatus.DISABLED;
        }
    }

    public override async Task<bool> LoadModel(T2IModel model, T2IParamInput input)
    {
        if (!ActiveProvider.Models.ContainsKey(model.Name))
        {
            return false;
        }
        CurrentModelName = model.Name;
        return true;
    }

    public override async Task GenerateLive(T2IParamInput user_input, string batchId, Action<object> takeOutput)
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

    public override async Task<Image[]> Generate(T2IParamInput input)
    {
        string normalizedProvider = NormalizeProviderKey(Settings.SelectedProvider);
        PermInfo requiredPermission = normalizedProvider switch
        {
            "openai" => APIBackendsPermissions.PermUseOpenAI,
            "ideogram" => APIBackendsPermissions.PermUseIdeogram,
            "bfl" => APIBackendsPermissions.PermUseBlackForest,
            _ => throw new Exception($"Unsupported provider: {normalizedProvider}")
        };
        if (!input.SourceSession.User.HasPermission(requiredPermission))
        {
            throw new Exception($"You do not have permission to use {Settings.SelectedProvider}");
        }
        APIProviderMetadata provider = ActiveProvider;
        RequestConfig config = provider.RequestConfig;
        // Get API key configuration details based on provider
        (string apiKeyType, string headerName, bool useCustomHeader) = normalizedProvider switch
        {
            "bfl" => ("black_forest_api", "x-key", true),
            "openai" => ("openai_api", "Authorization", false),
            "ideogram" => ("ideogram_api", "Authorization", false),
            _ => throw new Exception($"Unsupported provider: {normalizedProvider}")
        };
        string baseUrl;
        if (normalizedProvider == "bfl")
        {
            // Extract model name from input and clean it up
            string modelName = input.Get(T2IParamTypes.Model).Name
                .Replace("API/", "")
                .Replace(".safetensors", "");
            baseUrl = $"{(!string.IsNullOrEmpty(Settings.CustomBaseUrl) ? Settings.CustomBaseUrl : config.BaseUrl)}/v1/{modelName}";
            Logs.Debug($"Using BFL API endpoint: {baseUrl}");
        }
        else
        {
            baseUrl = !string.IsNullOrEmpty(Settings.CustomBaseUrl) ? Settings.CustomBaseUrl : config.BaseUrl;
        }
        string apiKey = input.SourceSession.User.GetGenericData(apiKeyType, "key");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception($"API key not found for {normalizedProvider}. Please set up your API key in the User tab");
        }
        JObject requestBody = config.BuildRequest(input);
        using HttpRequestMessage request = new(HttpMethod.Post, baseUrl)
        {
            Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json")
        };
        if (useCustomHeader)
        {
            request.Headers.Add(headerName, apiKey);
        }
        else
        {
            request.Headers.Add(headerName, $"{config.AuthHeader} {apiKey}");
        }
        // Send request and process response
        HttpResponseMessage response = await HttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"API request failed: {error}");
        }
        JObject responseJson = JObject.Parse(await response.Content.ReadAsStringAsync());
        byte[] imageData = await config.ProcessResponse(responseJson);
        return [new Image(imageData, Image.ImageType.IMAGE, "png")];
    }

    public override async Task Shutdown()
    {
        Status = BackendStatus.DISABLED;
    }

    /// <summary>Override FreeMemory to avoid NullReferenceException when accessing Address</summary>
    public override async Task<bool> FreeMemory(bool systemRam)
    {
        // For API backends, we don't actually need to free memory
        return true;
    }
}