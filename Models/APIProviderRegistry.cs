using System;
using System.Collections.Generic;
using SwarmUI.Utils;

namespace Hartsy.Extensions.APIBackends.Models
{
    /// <summary>Singleton registry for API provider metadata. Ensures only one instance of provider initialization exists.</summary>
    public sealed class APIProviderRegistry
    {
        private static readonly Lazy<APIProviderRegistry> _instance = new(() => new APIProviderRegistry());

        /// <summary>Gets the singleton instance of the provider registry.</summary>
        public static APIProviderRegistry Instance => _instance.Value;

        /// <summary>Dictionary of available providers by their ID.</summary>
        public IReadOnlyDictionary<string, APIProviderMetadata> Providers { get; }

        private APIProviderRegistry()
        {
            APIProviderInit init = new();
            Providers = init.Providers;
            Logs.Debug($"[APIProviderRegistry] Initialized with {Providers.Count} providers: {string.Join(", ", Providers.Keys)}");
        }
    }
}
