using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwarmUI.Accounts;
using SwarmUI.WebAPI;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.WebAPI;

/// <summary>Serves the declared model capabilities to the browser, so parameter visibility is driven by
/// the same data the request builders use instead of by model-name pattern matching in JavaScript.</summary>
public static class APIBackendsAPI
{
    public static void Register()
    {
        API.RegisterAPICall(APIBackendsListModelCapabilities, false, APIBackendsPermissions.PermViewCapabilities);
    }

    /// <summary>Returns every API model's param family, modality, feature flags and image-input support.</summary>
    public static async Task<JObject> APIBackendsListModelCapabilities(Session session)
    {
        return await Task.FromResult(ModelCapabilities.BuildClientMap());
    }
}
