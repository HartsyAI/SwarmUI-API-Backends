using System.Collections.Generic;

namespace Hartsy.Extensions.APIBackends.Models;

/// <summary>Interface for provider sources that supply model definitions.</summary>
public interface IProviderSource
{
    /// <summary>Gets the provider definition for this source.</summary>
    ProviderDefinition GetProvider();
}

/// <summary>Registry that aggregates all provider sources.</summary>
public static class ProviderRegistry
{
    private static readonly List<IProviderSource> _sources = [];

    /// <summary>Registers a provider source.</summary>
    public static void Register(IProviderSource source) => _sources.Add(source);

    /// <summary>Gets all registered provider definitions.</summary>
    public static IReadOnlyList<ProviderDefinition> All => _sources.ConvertAll(s => s.GetProvider());

    /// <summary>Clears all registered sources (for testing).</summary>
    public static void Clear() => _sources.Clear();
}
