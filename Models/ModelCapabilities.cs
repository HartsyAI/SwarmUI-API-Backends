using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Hartsy.Extensions.APIBackends.Models;

/// <summary>Maps declared model families to the UI feature flags that expose their params.
/// This is the single source of truth: the C# request builders and the browser both read from here,
/// so the UI can no longer offer a param the request builder ignores (or vice versa).</summary>
public static class ModelCapabilities
{
    /// <summary>Feature flags each param family turns on.</summary>
    private static readonly Dictionary<string, string[]> FamilyFlags = new()
    {
        ["image.standard"] = ["fal_t2i_params"],
        ["image.aspect"] = ["fal_aspect_image"],
        ["image.aspect_res"] = ["fal_aspect_image", "fal_resolution_image"],
        ["image.recraft"] = ["fal_recraft_params"],
        ["image.bria"] = ["fal_aspect_image", "fal_t2i_params"],
        ["utility.image"] = ["fal_utility_params"],
        ["utility.video"] = ["fal_utility_params", "fal_utility_video_params"],
        ["video.sora"] = ["fal_sora_video_params"],
        ["video.kling"] = ["fal_kling_video_params"],
        ["video.veo"] = ["fal_veo_video_params"],
        ["video.luma"] = ["fal_luma_video_params"],
        ["video.minimax"] = ["fal_minimax_video_params"],
        ["video.hunyuan"] = ["fal_hunyuan_video_params"],
        ["video.grok"] = ["fal_video_params"],
        ["video.seedance1"] = ["fal_seedance1_video_params"],
        ["video.seedance2"] = ["fal_seedance2_video_params"],
        ["video.generic"] = ["fal_video_params"],
        // Non-Fal providers keep their own per-model flags on the ModelDefinition.
        ["image.openai"] = [],
        ["video.openai_sora"] = ["openai_sora_params"]
    };

    /// <summary>All flags a model activates: its family's, its own declared flag, and any extras.</summary>
    public static List<string> FlagsFor(ModelDefinition model)
    {
        List<string> flags = [];
        if (FamilyFlags.TryGetValue(model.Family, out string[] familyFlags))
        {
            flags.AddRange(familyFlags);
        }
        // Edit/reference image models additionally unhide the shared image-input params.
        if (model.SupportsInitImage && model.Modality == ModelModality.Image)
        {
            flags.Add("fal_i2i_params");
        }
        flags.AddRange(model.ExtraFlags);
        return flags;
    }

    /// <summary>Whether every declared family is known. Returns the offending families, empty if all valid.</summary>
    public static List<string> UnknownFamilies(IEnumerable<ModelDefinition> models)
    {
        List<string> bad = [];
        foreach (ModelDefinition model in models)
        {
            if (!FamilyFlags.ContainsKey(model.Family) && !bad.Contains(model.Family))
            {
                bad.Add(model.Family);
            }
        }
        return bad;
    }

    /// <summary>Builds the capability map the browser uses to decide param visibility,
    /// replacing the model-name pattern matching that used to live in api-backends.js.</summary>
    public static JObject BuildClientMap()
    {
        JObject models = [];
        foreach ((string fullName, ModelDefinition model) in APIProviderRegistry.Instance.ModelsByFullName)
        {
            models[fullName] = new JObject
            {
                ["family"] = model.Family,
                ["modality"] = model.Modality.ToString().ToLowerInvariant(),
                ["init_image"] = model.SupportsInitImage,
                ["flags"] = new JArray(FlagsFor(model))
            };
        }
        return new JObject { ["models"] = models };
    }
}
