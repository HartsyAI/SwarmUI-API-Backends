using System;
using System.Collections.Generic;
using SwarmUI.Utils;

namespace Hartsy.Extensions.APIBackends.Models;

/// <summary>Singleton registry for API provider metadata. Ensures only one instance of provider initialization exists.</summary>
public sealed class APIProviderRegistry
{
    private static readonly Lazy<APIProviderRegistry> _instance = new(() => new APIProviderRegistry());

    /// <summary>Gets the singleton instance of the provider registry.</summary>
    public static APIProviderRegistry Instance => _instance.Value;

    /// <summary>Dictionary of available providers by their ID.</summary>
    public IReadOnlyDictionary<string, APIProviderMetadata> Providers { get; }

    /// <summary>Provider definitions by ID, carrying the declared model capabilities.</summary>
    public IReadOnlyDictionary<string, ProviderDefinition> ProviderDefs { get; }

    /// <summary>Every model by its full SwarmUI name ("API Models/Fal/BFL/FLUX/flux-dev"). The one place model
    /// capabilities are resolved from, so request building, output handling and the UI cannot drift apart.</summary>
    public IReadOnlyDictionary<string, ModelDefinition> ModelsByFullName { get; }

    private APIProviderRegistry()
    {
        APIProviderInit init = new();
        Providers = init.Providers;
        ProviderDefs = init.ProviderDefs;
        Dictionary<string, ModelDefinition> byName = [];
        foreach (ProviderDefinition provider in init.ProviderDefs.Values)
        {
            foreach (ModelDefinition model in provider.Models)
            {
                byName[model.GetFullName(provider.ModelPrefix)] = model;
            }
        }
        ModelsByFullName = byName;
        Logs.Debug($"[APIProviderRegistry] Initialized with {Providers.Count} providers, {ModelsByFullName.Count} models: {string.Join(", ", Providers.Keys)}");
    }

    /// <summary>Looks up a model's declared capabilities by its full SwarmUI name.</summary>
    public static bool TryGetModel(string fullModelName, out ModelDefinition model)
    {
        if (string.IsNullOrEmpty(fullModelName))
        {
            model = null;
            return false;
        }
        return Instance.ModelsByFullName.TryGetValue(fullModelName, out model);
    }
}
