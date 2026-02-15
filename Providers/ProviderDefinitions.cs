using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Static definitions for all API providers using the type-safe factory pattern.</summary>
public static class ProviderDefinitions
{
    /// <summary>Gets all provider definitions.</summary>
    public static IReadOnlyList<ProviderDefinition> All =>
    [
        OpenAIProvider.Instance.GetProvider(),
        IdeogramProvider.Instance.GetProvider(),
        BlackForestLabsProvider.Instance.GetProvider(),
        GrokProvider.Instance.GetProvider(),
        GoogleProvider.Instance.GetProvider(),
        FalProvider.Instance.GetProvider()
    ];
}
