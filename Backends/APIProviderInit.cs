using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwarmUI.Backends;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using Hartsy.Extensions.APIBackends.Models;
using Hartsy.Extensions.APIBackends.Providers;

namespace Hartsy.Extensions.APIBackends;

/// <summary>Initializes and manages API providers using the modular factory pattern.</summary>
public class APIProviderInit
{
    /// <summary>Shared HttpClient for all API requests.</summary>
    public static readonly HttpClient HttpClient = NetworkBackendUtils.MakeHttpClient();

    /// <summary>Dictionary of available providers by ID.</summary>
    public Dictionary<string, APIProviderMetadata> Providers { get; private set; }

    /// <summary>Provider definitions for type-safe access.</summary>
    public IReadOnlyDictionary<string, ProviderDefinition> ProviderDefs { get; private set; }

    public APIProviderInit()
    {
        InitializeProviders();
    }

    /// <summary>Initializes all API providers from definitions.</summary>
    public void InitializeProviders()
    {
        Providers = [];
        Dictionary<string, ProviderDefinition> defs = [];
        foreach (ProviderDefinition providerDef in ProviderDefinitions.All)
        {
            try
            {
                APIProviderMetadata metadata = CreateProviderMetadata(providerDef);
                Providers[providerDef.Id] = metadata;
                defs[providerDef.Id] = providerDef;
                Logs.Info($"[APIProviderInit] Initialized provider: {providerDef.Name} with {providerDef.Models.Count} models");
            }
            catch (Exception ex)
            {
                Logs.Error($"[APIProviderInit] Failed to initialize provider {providerDef.Name}: {ex.Message}");
            }
        }
        ProviderDefs = defs;
    }

    /// <summary>Creates APIProviderMetadata from a ProviderDefinition.</summary>
    private static APIProviderMetadata CreateProviderMetadata(ProviderDefinition providerDef)
    {
        Dictionary<string, T2IModel> models = ModelFactory.CreateAllModels(providerDef);
        IRequestBuilder requestBuilder = RequestBuilderFactory.GetBuilder(providerDef.Id);
        return new APIProviderMetadata
        {
            Name = providerDef.Name,
            Models = models,
            RequestConfig = new RequestConfig
            {
                BaseUrl = providerDef.BaseUrl,
                AuthHeader = providerDef.AuthHeaderType,
                BuildRequest = input =>
                {
                    string modelName = input.Get(T2IParamTypes.Model).Name;
                    ModelDefinition modelDef = FindModelDefinition(providerDef, modelName);
                    return requestBuilder.BuildRequest(input, modelDef, providerDef);
                },
                ProcessResponse = async (response, apiKey) =>
                {
                    return await requestBuilder.ProcessResponse(response, providerDef, apiKey);
                }
            }
        };
    }

    /// <summary>Finds the ModelDefinition for a given model name.</summary>
    private static ModelDefinition FindModelDefinition(ProviderDefinition provider, string fullModelName)
    {
        string normalized = fullModelName;
        string prefix = $"API Models/{provider.ModelPrefix}/";
        if (normalized.StartsWith(prefix, StringComparison.Ordinal))
        {
            normalized = normalized[prefix.Length..];
        }
        else
        {
            normalized = normalized.Replace($"{provider.ModelPrefix}/", "");
        }
        string modelId = normalized;
        foreach (ModelDefinition model in provider.Models)
        {
            if (model.Id == modelId || model.GetFullName(provider.ModelPrefix) == fullModelName)
            {
                return model;
            }
        }
        Logs.Warning($"[APIProviderInit] Model definition not found for: {fullModelName}, using first model as fallback");
        return provider.Models[0];
    }

    /// <summary>Gets the endpoint URL for a specific model and input.</summary>
    public static string GetEndpointUrl(ProviderDefinition provider, ModelDefinition model, T2IParamInput input)
    {
        IRequestBuilder builder = RequestBuilderFactory.GetBuilder(provider.Id);
        return builder.GetEndpointUrl(model, provider, input);
    }

    /// <summary>Adds authentication headers to a request.</summary>
    public static void AddAuthHeaders(HttpRequestMessage request, string apiKey, ProviderDefinition provider)
    {
        IRequestBuilder builder = RequestBuilderFactory.GetBuilder(provider.Id);
        builder.AddAuthHeaders(request, apiKey, provider);
    }
}
