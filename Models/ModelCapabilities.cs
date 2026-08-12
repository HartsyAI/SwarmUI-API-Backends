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
        ["image.standard"] = ["fal_img_common", "fal_t2i_params"],
        // FLUX.2 takes size plus safety only - no batch, steps, guidance or negative prompt.
        ["image.flux2"] = ["fal_img_common", "fal_flux2_params"],
        // Qwen 2.0 has no steps or guidance; Z-Image has steps but no guidance.
        ["image.qwen2"] = ["fal_img_common", "fal_prompt_expansion"],
        ["image.zimage"] = ["fal_img_common", "fal_img_steps", "fal_prompt_expansion"],
        ["image.nanobanana2"] = ["fal_nb2_params"],
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
        ["video.seedance1"] = ["fal_seedance1_video_params"],
        ["video.seedance2"] = ["fal_seedance2_video_params", "fal_seedance2_duration"],
        // Per-family video enums, verified against fal's published schemas 2026-08-12.
        ["video.wan22"] = ["fal_wan22_params", "fal_video_negative"],
        ["video.pixverse"] = ["fal_pixverse_params", "fal_video_negative"],
        ["video.ltx2"] = ["fal_ltx2_params", "fal_video_audio", "fal_video_negative"],
        ["video.ltx13b"] = ["fal_ltx13b_params", "fal_video_negative"],
        ["video.vidu"] = ["fal_vidu_params", "fal_video_audio"],
        ["video.pika"] = ["fal_pika_params", "fal_video_negative"],
        ["video.kandinsky"] = ["fal_kandinsky_params"],
        ["video.cogvideox"] = ["fal_video_negative"],
        ["video.wan25"] = ["fal_wan25_params", "fal_wan_audio", "fal_wan_expansion", "fal_video_negative"],
        ["video.wan26"] = ["fal_wan26_params", "fal_wan2x_resolution", "fal_wan_audio", "fal_wan_expansion", "fal_wan_multishot", "fal_video_negative"],
        ["video.wan27"] = ["fal_wan27_params", "fal_wan27_aspect", "fal_wan2x_resolution", "fal_wan_audio", "fal_wan_expansion", "fal_video_negative"],
        // Image-to-video takes its aspect ratio from the input image, so no aspect param.
        ["video.wan27_i2v"] = ["fal_wan27_params", "fal_wan2x_resolution", "fal_wan_audio", "fal_wan_expansion", "fal_end_image_url", "fal_video_negative"],
        ["video.wan27_ref"] = ["fal_wan27ref_params", "fal_wan27_aspect", "fal_wan2x_resolution", "fal_wan_multishot", "fal_video_negative"],
        // Kling V3 Turbo takes prompt, image and duration only.
        ["video.kling_turbo"] = ["fal_kling_turbo_params"],
        // Seedance 2.5 runs to 30s and takes no seed input; 2.0 (incl. Mini) stays capped at 15s.
        ["video.seedance25"] = ["fal_seedance25_params", "fal_seedance2_video_params"],
        ["video.seedance25_i2v"] = ["fal_seedance25_params", "fal_seedance2_video_params", "fal_end_image_url"],
        // FLUX 3: no seed input, safety_tolerance instead of a negative prompt. i2v shares the same params.
        ["video.flux3"] = ["fal_flux3_params", "fal_video_audio"],
        // Same model on BFL's own API: duration/aspect/safety match fal, but resolution is hd/fhd.
        ["video.bfl_flux3"] = ["bfl_flux3_params", "fal_flux3_params", "fal_video_audio"],
        // MiniMax H3: 768P/2K/4K, no seed input. i2v derives aspect from the image and adds a last frame.
        ["video.h3"] = ["fal_h3_params", "fal_h3_aspect", "fal_prompt_expansion"],
        ["video.h3_i2v"] = ["fal_h3_params", "fal_prompt_expansion", "fal_end_image_url"],
        ["video.h3_ref"] = ["fal_h3_params", "fal_h3_ref_aspect", "fal_prompt_expansion", "fal_ref_images", "fal_ref_videos", "fal_ref_audio"],
        ["video.grok"] = ["fal_video_params", "fal_video_audio", "fal_video_negative"],

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
