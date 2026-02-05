using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Static definitions for all API providers using the type-safe factory pattern.</summary>
public static class ProviderDefinitions
{
    /// <summary>Gets all provider definitions.</summary>
    public static IReadOnlyList<ProviderDefinition> All => [OpenAI, Ideogram, BlackForestLabs, Grok, Google, Fal];

    #region OpenAI Provider

    public static ProviderDefinition OpenAI => ProviderDefinitionBuilder.Create()
        .WithId("openai_api")
        .WithName("OpenAI")
        .WithModelPrefix("OpenAI")
        .WithBaseUrl("https://api.openai.com/v1/images/generations")
        .WithAuthHeader("Bearer")
        .WithModelClass("openai_api", "DALL-E")
        .AddModels(OpenAIModels)
        .Build();

    private static IEnumerable<ModelDefinition> OpenAIModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("dall-e-2")
            .WithTitle("DALL-E 2")
            .WithDescription("OpenAI's DALL-E 2 model for image generation")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/dalle2.png")
            .WithDate("2022")
            .WithUsageHint("Good for general image generation via API")
            .WithTags("dall-e", "generative")
            .WithFeatureFlag("dalle2_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("dall-e-3")
            .WithTitle("DALL-E 3")
            .WithDescription("Advanced generative model for creating highly detailed and accurate images")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/dalle.png")
            .WithDate("2023")
            .WithUsageHint("Excellent for high-quality image generation with accurate text representation")
            .WithTags("dall-e", "high-quality", "text-accurate")
            .WithFeatureFlag("dalle3_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gpt-image-1")
            .WithTitle("GPT Image 1")
            .WithDescription("Generates images with strong instruction following, contextual awareness, and world knowledge")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/gpt-image-1.png")
            .WithDate("2025")
            .WithUsageHint("Best for context-aware image generation and edits")
            .WithTags("gpt-image", "high-quality", "text-accurate")
            .WithFeatureFlag("gpt-image-1_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gpt-image-1.5")
            .WithTitle("GPT Image 1.5")
            .WithDescription("High-fidelity images with strong instruction following and accurate visual details")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/gpt-image-1.5.png")
            .WithDate("2025")
            .WithUsageHint("Best for context-aware image generation requiring strong instruction following")
            .WithTags("gpt-image", "high-quality", "text-accurate")
            .WithFeatureFlag("gpt-image-1.5_params")
            .Build()
    ];

    #endregion

    #region Ideogram Provider

    public static ProviderDefinition Ideogram => ProviderDefinitionBuilder.Create()
        .WithId("ideogram_api")
        .WithName("Ideogram")
        .WithModelPrefix("Ideogram")
        .WithBaseUrl("https://api.ideogram.ai/generate")
        .WithAuthHeader("Bearer", "Api-Key")
        .WithModelClass("ideogram_api", "Ideogram")
        .AddModels(IdeogramModels)
        .Build();

    private static IEnumerable<ModelDefinition> IdeogramModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("V_1")
            .WithTitle("Ideogram V1")
            .WithDescription("First generation Ideogram model with strong text rendering")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/ideogram_v1.png")
            .WithDate("2023")
            .WithUsageHint("Good for images requiring accurate text rendering")
            .WithTags("text-rendering", "generative")
            .WithFeatureFlag("ideogram_v1_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_2")
            .WithTitle("Ideogram V2")
            .WithDescription("Second generation with improved quality and text accuracy")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/ideogram_v2.png")
            .WithDate("2024")
            .WithUsageHint("Enhanced text rendering and image quality")
            .WithTags("text-rendering", "high-quality")
            .WithFeatureFlag("ideogram_v2_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_2_TURBO")
            .WithTitle("Ideogram V2 Turbo")
            .WithDescription("Faster V2 generation with optimized performance")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/ideogram_v2_turbo.png")
            .WithDate("2024")
            .WithUsageHint("Fast generation with good quality")
            .WithTags("text-rendering", "fast")
            .WithFeatureFlag("ideogram_v2_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_3")
            .WithTitle("Ideogram V3")
            .WithDescription("Latest generation with best-in-class text rendering and image quality")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/ideogram_v3.png")
            .WithDate("2025")
            .WithUsageHint("Best for professional-grade images with perfect text")
            .WithTags("text-rendering", "premium", "high-quality")
            .WithFeatureFlag("ideogram_v3_params")
            .Build()
    ];

    #endregion

    #region Black Forest Labs Provider

    public static ProviderDefinition BlackForestLabs => ProviderDefinitionBuilder.Create()
        .WithId("bfl_api")
        .WithName("Black Forest Labs")
        .WithModelPrefix("BFL")
        .WithBaseUrl("https://api.bfl.ai")
        .WithAuthHeader("Bearer", "x-key")
        .WithModelClass("bfl_api", "FLUX")
        .AddModels(BlackForestModels)
        .Build();

    private static IEnumerable<ModelDefinition> BlackForestModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("flux-pro-1.1")
            .WithTitle("FLUX Pro 1.1")
            .WithDescription("Professional-grade FLUX model with enhanced capabilities")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/flux-pro.png")
            .WithDate("2024")
            .WithUsageHint("High-quality professional image generation")
            .WithTags("flux", "professional", "high-quality")
            .WithFeatureFlag("flux_pro_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-pro-1.1-ultra")
            .WithTitle("FLUX Pro 1.1 Ultra")
            .WithDescription("Ultra high-resolution FLUX model up to 2K with improved realism")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(2048, 2048)
            .WithPreviewImage("Images/ModelPreviews/flux-ultra.png")
            .WithDate("2024")
            .WithUsageHint("Best for ultra high-resolution professional images")
            .WithTags("flux", "ultra", "2k", "realism")
            .WithFeatureFlag("flux_ultra_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-dev")
            .WithTitle("FLUX Dev")
            .WithDescription("Development version of FLUX for experimentation")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/flux-dev.png")
            .WithDate("2024")
            .WithUsageHint("Good for testing and development")
            .WithTags("flux", "dev")
            .WithFeatureFlag("flux_dev_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-kontext-pro")
            .WithTitle("FLUX Kontext Pro")
            .WithDescription("Context-aware FLUX model for targeted edits and transformations")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/flux-kontext.png")
            .WithDate("2024")
            .WithUsageHint("Best for image editing and context-aware generation")
            .WithTags("flux", "kontext", "editing")
            .WithFeatureFlag("flux_kontext_pro_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-kontext-max")
            .WithTitle("FLUX Kontext Max")
            .WithDescription("Maximum capability context-aware FLUX model")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/flux-kontext-max.png")
            .WithDate("2024")
            .WithUsageHint("Premium context-aware generation and editing")
            .WithTags("flux", "kontext", "premium")
            .WithFeatureFlag("flux_kontext_max_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-2-pro")
            .WithTitle("FLUX 2 Pro")
            .WithDescription("Second generation professional FLUX model")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/flux-2-pro.png")
            .WithDate("2025")
            .WithUsageHint("Latest professional-grade FLUX generation")
            .WithTags("flux", "flux2", "professional")
            .WithFeatureFlag("flux_2_pro_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-2-max")
            .WithTitle("FLUX 2 Max")
            .WithDescription("Maximum capability second generation FLUX model")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/flux-2-max.png")
            .WithDate("2025")
            .WithUsageHint("Premium FLUX 2 generation with maximum quality")
            .WithTags("flux", "flux2", "premium")
            .WithFeatureFlag("flux_2_max_params")
            .Build()
    ];

    #endregion

    #region Grok Provider

    public static ProviderDefinition Grok => ProviderDefinitionBuilder.Create()
        .WithId("grok_api")
        .WithName("Grok")
        .WithModelPrefix("Grok")
        .WithBaseUrl("https://api.x.ai/v1/images/generations")
        .WithAuthHeader("Bearer")
        .WithModelClass("grok_api", "Grok")
        .AddModels(GrokModels)
        .Build();

    private static IEnumerable<ModelDefinition> GrokModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("grok-2-image")
            .WithTitle("Grok 2 Image")
            .WithDescription("xAI's Grok 2 image generation model")
            .WithAuthor("xAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/grok.png")
            .WithDate("2024")
            .WithUsageHint("High-quality image generation from xAI")
            .WithTags("grok", "xai", "generative")
            .WithFeatureFlag("grok_2_image_params")
            .Build()
    ];

    #endregion

    #region Google Provider

    public static ProviderDefinition Google => ProviderDefinitionBuilder.Create()
        .WithId("google_api")
        .WithName("Google")
        .WithModelPrefix("Google")
        .WithBaseUrl("https://generativelanguage.googleapis.com/v1beta/models")
        .WithAuthHeader("Bearer")
        .WithModelClass("google_api", "Google Imagen")
        .AddModels(GoogleModels)
        .Build();

    private static IEnumerable<ModelDefinition> GoogleModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("imagen-3.0-generate-002")
            .WithTitle("Imagen 3.0")
            .WithDescription("Google's Imagen 3.0 image generation model")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/imagen.png")
            .WithDate("2024")
            .WithUsageHint("High-quality image generation from Google")
            .WithTags("imagen", "google", "generative")
            .WithFeatureFlag("google_imagen_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-2.0-flash-preview-image-generation")
            .WithTitle("Gemini 2.0 Flash Image")
            .WithDescription("Gemini 2.0 Flash with image generation capabilities")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/gemini.png")
            .WithDate("2024")
            .WithUsageHint("Fast multimodal image generation")
            .WithTags("gemini", "google", "multimodal")
            .WithFeatureFlag("google_gemini_params")
            .Build()
    ];

    #endregion

    #region Fal.ai Provider

    public static ProviderDefinition Fal => ProviderDefinitionBuilder.Create()
        .WithId("fal_api")
        .WithName("Fal.ai")
        .WithModelPrefix("Fal")
        .WithBaseUrl("https://fal.run")
        .WithAuthHeader("Key", "Authorization")
        .WithModelClass("fal_api", "Fal.ai")
        .AddModels(FalModels)
        .Build();

    private static IEnumerable<ModelDefinition> FalModels =>
    [
        // FLUX Models
        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/flux/dev")
            .WithTitle("FLUX.1 [dev]")
            .WithDescription("12B parameter flow transformer for high-quality text-to-image generation")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-flux-dev.png")
            .WithDate("2024")
            .WithUsageHint("High-quality text-to-image with 28 inference steps")
            .WithTags("flux", "text-to-image", "high-quality")
            .WithFeatureFlag("fal_flux_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/flux/schnell")
            .WithTitle("FLUX.1 [schnell]")
            .WithDescription("Fast FLUX model generating high-quality images in 1-4 steps")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-flux-schnell.png")
            .WithDate("2024")
            .WithUsageHint("Ultra-fast generation, great for rapid iteration")
            .WithTags("flux", "text-to-image", "fast")
            .WithFeatureFlag("fal_flux_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/flux-pro/v1.1-ultra")
            .WithTitle("FLUX1.1 [pro] ultra")
            .WithDescription("Professional FLUX with up to 2K resolution and improved photo realism")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(2048, 2048)
            .WithPreviewImage("Images/ModelPreviews/fal-flux-pro-ultra.png")
            .WithDate("2024")
            .WithUsageHint("Best for high-resolution professional images")
            .WithTags("flux", "text-to-image", "2k", "realism", "professional")
            .WithFeatureFlag("fal_flux_pro_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/flux-pro/kontext")
            .WithTitle("FLUX.1 Kontext [pro]")
            .WithDescription("Context-aware FLUX for targeted edits and complex scene transformations")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-flux-kontext.png")
            .WithDate("2024")
            .WithUsageHint("Best for image editing with text and reference images")
            .WithTags("flux", "image-to-image", "editing", "kontext")
            .WithFeatureFlag("fal_flux_kontext_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/flux-2-flex")
            .WithTitle("FLUX.2 [flex]")
            .WithDescription("FLUX.2 with adjustable inference steps, guidance scale, and enhanced typography")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-flux-2-flex.png")
            .WithDate("2025")
            .WithUsageHint("Fine-tuned control with excellent text rendering")
            .WithTags("flux", "text-to-image", "typography", "flux2")
            .WithFeatureFlag("fal_flux_2_params")
            .Build(),

        // Recraft Models
        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/recraft/v3/text-to-image")
            .WithTitle("Recraft V3")
            .WithDescription("SOTA text-to-image with long text generation, vector art, and brand style support")
            .WithAuthor("Recraft")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-recraft-v3.png")
            .WithDate("2024")
            .WithUsageHint("Best for vector art, typography, and brand-consistent images")
            .WithTags("recraft", "text-to-image", "vector", "typography", "style")
            .WithFeatureFlag("fal_recraft_params")
            .Build(),

        // Ideogram via Fal
        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/ideogram/v2/turbo")
            .WithTitle("Ideogram V2 Turbo (Fal)")
            .WithDescription("Fast Ideogram V2 with excellent text rendering via Fal.ai")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-ideogram-v2.png")
            .WithDate("2024")
            .WithUsageHint("Fast generation with superior text rendering")
            .WithTags("ideogram", "text-to-image", "text-rendering", "fast")
            .WithFeatureFlag("fal_ideogram_params")
            .Build(),

        // Stable Diffusion Models
        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/stable-diffusion-v35-large")
            .WithTitle("Stable Diffusion 3.5 Large")
            .WithDescription("Stability AI's SD 3.5 Large model with 8B parameters")
            .WithAuthor("Stability AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-sd35-large.png")
            .WithDate("2024")
            .WithUsageHint("High-quality general purpose image generation")
            .WithTags("stable-diffusion", "text-to-image", "sd35")
            .WithFeatureFlag("fal_sd_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/stable-diffusion-v35-medium")
            .WithTitle("Stable Diffusion 3.5 Medium")
            .WithDescription("Balanced SD 3.5 model with 2.5B parameters")
            .WithAuthor("Stability AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-sd35-medium.png")
            .WithDate("2024")
            .WithUsageHint("Good balance of quality and speed")
            .WithTags("stable-diffusion", "text-to-image", "sd35")
            .WithFeatureFlag("fal_sd_params")
            .Build(),

        // Grok via Fal
        ModelDefinitionBuilder.Create()
            .WithId("xai/grok-imagine-image")
            .WithTitle("Grok Imagine Image (Fal)")
            .WithDescription("Generate highly aesthetic images with xAI's Grok Imagine model")
            .WithAuthor("xAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-grok-imagine.png")
            .WithDate("2024")
            .WithUsageHint("High aesthetic quality image generation")
            .WithTags("grok", "xai", "text-to-image", "aesthetic")
            .WithFeatureFlag("fal_grok_params")
            .Build(),

        // Video Models (for future expansion)
        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/ltx-2-19b/image-to-video")
            .WithTitle("LTX-2 19B Image-to-Video")
            .WithDescription("Generate video with audio from images using LTX-2")
            .WithAuthor("Lightricks")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/fal-ltx-2.png")
            .WithDate("2025")
            .WithUsageHint("Create videos from static images")
            .WithTags("ltx", "image-to-video", "video", "audio")
            .WithFeatureFlag("fal_video_params")
            .Build(),

        // Image Editing Models
        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/flux-pro/kontext/max")
            .WithTitle("FLUX Kontext Max (Fal)")
            .WithDescription("Maximum capability context-aware editing model")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-flux-kontext-max.png")
            .WithDate("2024")
            .WithUsageHint("Premium image editing with maximum quality")
            .WithTags("flux", "image-to-image", "editing", "kontext", "premium")
            .WithFeatureFlag("fal_flux_kontext_params")
            .Build(),

        // Utility Models
        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/imageutils/rembg")
            .WithTitle("Remove Background")
            .WithDescription("Remove backgrounds from images automatically")
            .WithAuthor("Fal.ai")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/fal-rembg.png")
            .WithDate("2024")
            .WithUsageHint("Quick background removal for any image")
            .WithTags("utility", "background-removal")
            .WithFeatureFlag("fal_utility_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("fal-ai/clarity-upscaler")
            .WithTitle("Clarity Upscaler")
            .WithDescription("AI-powered image upscaling with detail enhancement")
            .WithAuthor("Fal.ai")
            .WithDimensions(4096, 4096)
            .WithPreviewImage("Images/ModelPreviews/fal-clarity.png")
            .WithDate("2024")
            .WithUsageHint("Upscale images up to 4x with enhanced details")
            .WithTags("utility", "upscaler", "enhancement")
            .WithFeatureFlag("fal_utility_params")
            .Build()
    ];

    #endregion
}
