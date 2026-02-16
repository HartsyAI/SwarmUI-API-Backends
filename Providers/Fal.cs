using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Fal.ai API provider for a wide variety of image and video generation models.</summary>
public sealed class FalProvider : IProviderSource
{
    public static FalProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("fal_api")
        .WithName("Fal.ai")
        .WithModelPrefix("Fal")
        .WithBaseUrl("https://fal.run")
        .WithAuthHeader("Key", "Authorization")
        .WithModelClass("fal_api", "Fal.ai")
        .AddModels(FluxModels)
        .AddModels(RecraftModels)
        .AddModels(IdeogramModels)
        .AddModels(StabilityAIModels)
        .AddModels(GrokModels)
        .AddModels(GoogleModels)
        .AddModels(KlingModels)
        .AddModels(QwenModels)
        .AddModels(BriaModels)
        .AddModels(ByteDanceModels)
        .AddModels(ReveModels)
        .AddModels(ImagineArtModels)
        .AddModels(FLiteModels)
        .AddModels(HiDreamModels)
        .AddModels(OmniGenModels)
        .AddModels(AuraFlowModels)
        .AddModels(LuminaModels)
        .AddModels(SanaModels)
        .AddModels(PlaygroundModels)
        .AddModels(KolorsModels)
        .AddModels(MiniMaxImageModels)
        .AddModels(Step1XModels)
        .AddModels(HunyuanImageModels)
        .AddModels(UNOModels)
        .AddModels(InstantCharacterModels)
        .AddModels(SoraModels)
        .AddModels(MiniMaxVideoModels)
        .AddModels(PixVerseModels)
        .AddModels(WanModels)
        .AddModels(LTXModels)
        .AddModels(ViduModels)
        .AddModels(HunyuanVideoModels)
        .AddModels(MochiModels)
        .AddModels(LumaModels)
        .AddModels(PikaModels)
        .AddModels(KandinskyModels)
        .AddModels(MagiModels)
        .AddModels(CogVideoXModels)
        .AddModels(SkyReelsModels)
        .AddModels(DecartModels)
        .AddModels(UtilityModels)
        .Build();

    #region FLUX (Black Forest Labs)

    private static IEnumerable<ModelDefinition> FluxModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-dev")
            .WithEndpointOverride("fal-ai/flux/dev")
            .WithTitle("FLUX.1 [dev]")
            .WithDescription("12B parameter flow transformer for high-quality text-to-image generation")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-dev.jpg")
            .WithDate("2024")
            .WithUsageHint("High-quality text-to-image with 28 inference steps")
            .WithTags("flux", "text-to-image", "high-quality")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-schnell")
            .WithEndpointOverride("fal-ai/flux/schnell")
            .WithTitle("FLUX.1 [schnell]")
            .WithDescription("Fast FLUX model generating high-quality images in 1-4 steps")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-schnell.jpg")
            .WithDate("2024")
            .WithUsageHint("Ultra-fast generation, great for rapid iteration")
            .WithTags("flux", "text-to-image", "fast")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-pro-ultra")
            .WithEndpointOverride("fal-ai/flux-pro/v1.1-ultra")
            .WithTitle("FLUX 1.1 [pro] ultra")
            .WithDescription("Professional FLUX with up to 2K resolution and improved photo realism")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(2048, 2048)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-pro-ultra.jpg")
            .WithDate("2024")
            .WithUsageHint("Best for high-resolution professional images")
            .WithTags("flux", "text-to-image", "2k", "realism", "professional")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-kontext-pro")
            .WithEndpointOverride("fal-ai/flux-pro/kontext")
            .WithTitle("FLUX.1 Kontext [pro]")
            .WithDescription("Context-aware FLUX for targeted edits and complex scene transformations")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-kontext-pro.jpg")
            .WithDate("2024")
            .WithUsageHint("Best for image editing with text and reference images")
            .WithTags("flux", "image-to-image", "editing", "kontext")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-kontext-max")
            .WithEndpointOverride("fal-ai/flux-pro/kontext/max")
            .WithTitle("FLUX Kontext [max]")
            .WithDescription("Maximum capability context-aware editing model")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-kontext-max.jpg")
            .WithDate("2024")
            .WithUsageHint("Premium image editing with maximum quality")
            .WithTags("flux", "image-to-image", "editing", "kontext", "premium")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-2-flex")
            .WithEndpointOverride("fal-ai/flux-2-flex")
            .WithTitle("FLUX.2 [flex]")
            .WithDescription("FLUX.2 with adjustable inference steps, guidance scale, and enhanced typography")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-2-flex.jpg")
            .WithDate("2025")
            .WithUsageHint("Fine-tuned control with excellent text rendering")
            .WithTags("flux", "text-to-image", "typography", "flux2")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-pro")
            .WithEndpointOverride("fal-ai/flux-pro")
            .WithTitle("FLUX.1 [pro]")
            .WithDescription("Professional-grade FLUX model with enhanced quality")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-pro.jpg")
            .WithDate("2024")
            .WithUsageHint("High-quality professional image generation")
            .WithTags("flux", "text-to-image", "professional")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-pro-v1.1")
            .WithEndpointOverride("fal-ai/flux-pro/v1.1")
            .WithTitle("FLUX 1.1 [pro]")
            .WithDescription("Enhanced FLUX Pro with improved quality and speed")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-pro-v1.1.jpg")
            .WithDate("2024")
            .WithUsageHint("Best balance of speed and quality")
            .WithTags("flux", "text-to-image", "professional", "v1.1")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-kontext-dev-lora")
            .WithEndpointOverride("fal-ai/flux-kontext-lora")
            .WithTitle("FLUX Kontext [dev] LoRA")
            .WithDescription("Kontext dev model with LoRA support for personalized editing")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-kontext-dev-lora.jpg")
            .WithDate("2025")
            .WithUsageHint("Image editing with LoRA personalization")
            .WithTags("flux", "image-to-image", "editing", "kontext", "lora")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-2-pro-edit")
            .WithEndpointOverride("fal-ai/flux-2-pro/edit")
            .WithTitle("FLUX.2 [pro] Edit")
            .WithDescription("FLUX 2 Pro optimized for maximum quality editing with exceptional photorealism")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-2-pro-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Premium image editing with photorealism")
            .WithTags("flux", "image-to-image", "editing", "flux2", "professional")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-2-max-edit")
            .WithEndpointOverride("fal-ai/flux-2-max/edit")
            .WithTitle("FLUX.2 [max] Edit")
            .WithDescription("FLUX 2 Max state-of-the-art editing with exceptional realism and precision")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-2-max-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Maximum quality image editing")
            .WithTags("flux", "image-to-image", "editing", "flux2", "premium")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-2-dev-edit")
            .WithEndpointOverride("fal-ai/flux-2/edit")
            .WithTitle("FLUX.2 [dev] Edit")
            .WithDescription("FLUX 2 Dev editing with natural language descriptions and hex color control")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-2-dev-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Precise image editing with color control")
            .WithTags("flux", "image-to-image", "editing", "flux2")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-2-flex-edit")
            .WithEndpointOverride("fal-ai/flux-2-flex/edit")
            .WithTitle("FLUX.2 [flex] Edit")
            .WithDescription("FLUX 2 Flex multi-reference editing with enhanced text rendering")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-2-flex-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Multi-reference editing with typography support")
            .WithTags("flux", "image-to-image", "editing", "flux2", "typography")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-general")
            .WithEndpointOverride("fal-ai/flux-general")
            .WithTitle("FLUX General (ControlNet)")
            .WithDescription("FLUX with ControlNet support for guided image generation")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-general.jpg")
            .WithDate("2024")
            .WithUsageHint("Controlled image generation with ControlNet")
            .WithTags("flux", "text-to-image", "controlnet")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLUX/flux-lora")
            .WithEndpointOverride("fal-ai/flux-lora")
            .WithTitle("FLUX.1 [dev] LoRA")
            .WithDescription("FLUX dev with LoRA support for personalized styles and outputs")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLUX/flux-lora.jpg")
            .WithDate("2024")
            .WithUsageHint("Personalized generation with custom LoRA adapters")
            .WithTags("flux", "text-to-image", "lora", "personalization")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Recraft

    private static IEnumerable<ModelDefinition> RecraftModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Recraft/recraft-v3")
            .WithEndpointOverride("fal-ai/recraft/v3/text-to-image")
            .WithTitle("Recraft V3")
            .WithDescription("SOTA text-to-image with long text generation, vector art, and brand style support")
            .WithAuthor("Recraft")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Recraft/recraft-v3.jpg")
            .WithDate("2024")
            .WithUsageHint("Best for vector art, typography, and brand-consistent images")
            .WithTags("recraft", "text-to-image", "vector", "typography", "style")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Ideogram

    private static IEnumerable<ModelDefinition> IdeogramModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Ideogram/ideogram-v2-turbo")
            .WithEndpointOverride("fal-ai/ideogram/v2/turbo")
            .WithTitle("Ideogram V2 Turbo")
            .WithDescription("Fast Ideogram V2 with excellent text rendering via Fal.ai")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Ideogram/ideogram-v2-turbo.jpg")
            .WithDate("2024")
            .WithUsageHint("Fast generation with superior text rendering")
            .WithTags("ideogram", "text-to-image", "text-rendering", "fast")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Ideogram/ideogram-v3")
            .WithEndpointOverride("fal-ai/ideogram/v3")
            .WithTitle("Ideogram V3")
            .WithDescription("Latest Ideogram model with turbo, balanced, and quality modes")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Ideogram/ideogram-v3.jpg")
            .WithDate("2025")
            .WithUsageHint("Highest quality text rendering and image generation")
            .WithTags("ideogram", "text-to-image", "text-rendering", "v3")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Ideogram/ideogram-v3-edit")
            .WithEndpointOverride("fal-ai/ideogram/v3/edit")
            .WithTitle("Ideogram V3 Edit")
            .WithDescription("Ideogram V3 image editing with precision control")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Ideogram/ideogram-v3-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Precise image editing with text rendering")
            .WithTags("ideogram", "image-to-image", "image-editing", "v3")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Ideogram/ideogram-v2a")
            .WithEndpointOverride("fal-ai/ideogram/v2a")
            .WithTitle("Ideogram V2A")
            .WithDescription("Ideogram V2A generation model")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Ideogram/ideogram-v2a.jpg")
            .WithDate("2024")
            .WithUsageHint("High-quality image generation with text rendering")
            .WithTags("ideogram", "text-to-image", "text-rendering", "v2a")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Ideogram/ideogram-v2a-turbo")
            .WithEndpointOverride("fal-ai/ideogram/v2a/turbo")
            .WithTitle("Ideogram V2A Turbo")
            .WithDescription("Fast Ideogram V2A model for rapid generation")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Ideogram/ideogram-v2a-turbo.jpg")
            .WithDate("2024")
            .WithUsageHint("Fast generation with good text rendering")
            .WithTags("ideogram", "text-to-image", "text-rendering", "v2a", "fast")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Stability AI

    private static IEnumerable<ModelDefinition> StabilityAIModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("StabilityAI/sd-v35-large")
            .WithEndpointOverride("fal-ai/stable-diffusion-v35-large")
            .WithTitle("Stable Diffusion 3.5 Large")
            .WithDescription("Stability AI's SD 3.5 Large model with 8B parameters")
            .WithAuthor("Stability AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/StabilityAI/sd-v35-large.jpg")
            .WithDate("2024")
            .WithUsageHint("High-quality general purpose image generation")
            .WithTags("stable-diffusion", "text-to-image", "sd35")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("StabilityAI/sd-v35-medium")
            .WithEndpointOverride("fal-ai/stable-diffusion-v35-medium")
            .WithTitle("Stable Diffusion 3.5 Medium")
            .WithDescription("Balanced SD 3.5 model with 2.5B parameters")
            .WithAuthor("Stability AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/StabilityAI/sd-v35-medium.jpg")
            .WithDate("2024")
            .WithUsageHint("Good balance of quality and speed")
            .WithTags("stable-diffusion", "text-to-image", "sd35")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("StabilityAI/sdxl-lightning")
            .WithEndpointOverride("fal-ai/fast-lightning-sdxl")
            .WithTitle("SDXL Lightning")
            .WithDescription("Ultra-fast SDXL model generating high-quality images in minimal steps")
            .WithAuthor("Stability AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/StabilityAI/sdxl-lightning.jpg")
            .WithDate("2024")
            .WithUsageHint("Fastest SDXL generation")
            .WithTags("stable-diffusion", "text-to-image", "sdxl", "fast")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("StabilityAI/sd-v3-medium")
            .WithEndpointOverride("fal-ai/stable-diffusion-v3-medium")
            .WithTitle("Stable Diffusion 3.0 Medium")
            .WithDescription("SD 3.0 Medium model with improved text rendering")
            .WithAuthor("Stability AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/StabilityAI/sd-v3-medium.jpg")
            .WithDate("2024")
            .WithUsageHint("Good text rendering with balanced quality")
            .WithTags("stable-diffusion", "text-to-image", "sd3")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("StabilityAI/stable-cascade")
            .WithEndpointOverride("fal-ai/stable-cascade")
            .WithTitle("Stable Cascade")
            .WithDescription("Würstchen-based cascaded diffusion model with efficient generation")
            .WithAuthor("Stability AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/StabilityAI/stable-cascade.jpg")
            .WithDate("2024")
            .WithUsageHint("Efficient high-quality image generation")
            .WithTags("stable-diffusion", "text-to-image", "cascade")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region xAI (Grok)

    private static IEnumerable<ModelDefinition> GrokModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Grok/grok-imagine-image")
            .WithEndpointOverride("xai/grok-imagine-image")
            .WithTitle("Grok Imagine Image")
            .WithDescription("Generate highly aesthetic images with xAI's Grok Imagine model")
            .WithAuthor("xAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Grok/grok-imagine-image.jpg")
            .WithDate("2024")
            .WithUsageHint("High aesthetic quality image generation")
            .WithTags("grok", "xai", "text-to-image", "aesthetic")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Grok/grok-imagine-video-t2v")
            .WithEndpointOverride("xai/grok-imagine-video/text-to-video")
            .WithTitle("Grok Imagine Video (T2V)")
            .WithDescription("Generate videos with audio from text using xAI's Grok Imagine")
            .WithAuthor("xAI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Grok/grok-imagine-video-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Text-to-video with audio generation")
            .WithTags("grok", "xai", "text-to-video", "video", "audio")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Grok/grok-imagine-video-i2v")
            .WithEndpointOverride("xai/grok-imagine-video/image-to-video")
            .WithTitle("Grok Imagine Video (I2V)")
            .WithDescription("Generate videos with audio from images using xAI's Grok Imagine")
            .WithAuthor("xAI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Grok/grok-imagine-video-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video with audio generation")
            .WithTags("grok", "xai", "image-to-video", "video", "audio")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Grok/grok-imagine-image-edit")
            .WithEndpointOverride("xai/grok-imagine-image/edit")
            .WithTitle("Grok Imagine Image Edit")
            .WithDescription("Edit images precisely with xAI's Grok Imagine model")
            .WithAuthor("xAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Grok/grok-imagine-image-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Precise image editing with text guidance")
            .WithTags("grok", "xai", "image-to-image", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Grok/grok-imagine-video-edit")
            .WithEndpointOverride("xai/grok-imagine-video/edit-video")
            .WithTitle("Grok Imagine Video Edit")
            .WithDescription("Edit videos using xAI's Grok Imagine model")
            .WithAuthor("xAI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Grok/grok-imagine-video-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Video editing with text guidance")
            .WithTags("grok", "xai", "video-to-video", "video", "editing")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Google

    private static IEnumerable<ModelDefinition> GoogleModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Google/nano-banana-pro")
            .WithEndpointOverride("fal-ai/nano-banana-pro")
            .WithTitle("Nano Banana Pro")
            .WithDescription("Google's state-of-the-art image generation and editing model")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/nano-banana-pro.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality image generation from Google")
            .WithTags("google", "text-to-image", "high-quality")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-3.1-t2v")
            .WithEndpointOverride("fal-ai/veo3.1")
            .WithTitle("Veo 3.1 (T2V)")
            .WithDescription("Google DeepMind's most advanced AI video generation model with sound")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-3.1-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Highest quality text-to-video with audio")
            .WithTags("google", "veo", "text-to-video", "video", "audio")
            .WithFeatureFlag("fal_veo_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-3.1-fast-t2v")
            .WithEndpointOverride("fal-ai/veo3.1/fast")
            .WithTitle("Veo 3.1 Fast (T2V)")
            .WithDescription("Faster and more cost-effective version of Google's Veo 3.1")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-3.1-fast-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast text-to-video with audio")
            .WithTags("google", "veo", "text-to-video", "video", "fast")
            .WithFeatureFlag("fal_veo_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-3.1-i2v")
            .WithEndpointOverride("fal-ai/veo3.1/image-to-video")
            .WithTitle("Veo 3.1 (I2V)")
            .WithDescription("Google Veo 3.1 image-to-video generation with audio")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-3.1-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video with audio")
            .WithTags("google", "veo", "image-to-video", "video", "audio")
            .WithFeatureFlag("fal_veo_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-3.1-fast-i2v")
            .WithEndpointOverride("fal-ai/veo3.1/fast/image-to-video")
            .WithTitle("Veo 3.1 Fast (I2V)")
            .WithDescription("Fast Veo 3.1 image-to-video generation")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-3.1-fast-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast image-to-video generation")
            .WithTags("google", "veo", "image-to-video", "video", "fast")
            .WithFeatureFlag("fal_veo_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/nano-banana-pro-edit")
            .WithEndpointOverride("fal-ai/nano-banana-pro/edit")
            .WithTitle("Nano Banana Pro Edit")
            .WithDescription("Google's state-of-the-art image editing model with realism and typography")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/nano-banana-pro-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality image editing from Google")
            .WithTags("google", "image-to-image", "image-editing", "realism", "typography")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/imagen-3")
            .WithEndpointOverride("fal-ai/imagen3")
            .WithTitle("Imagen 3")
            .WithDescription("Google's Imagen 3 text-to-image generation model")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/imagen-3.jpg")
            .WithDate("2024")
            .WithUsageHint("Premium image generation from Google")
            .WithTags("google", "text-to-image", "imagen")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/imagen-3-fast")
            .WithEndpointOverride("fal-ai/imagen3/fast")
            .WithTitle("Imagen 3 Fast")
            .WithDescription("Fast version of Google's Imagen 3 model")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/imagen-3-fast.jpg")
            .WithDate("2024")
            .WithUsageHint("Quick image generation from Google")
            .WithTags("google", "text-to-image", "imagen", "fast")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/gemini-flash-edit")
            .WithEndpointOverride("fal-ai/gemini-flash-edit")
            .WithTitle("Gemini Flash Edit")
            .WithDescription("Google Gemini-powered image editing")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/gemini-flash-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("AI-powered image editing with Gemini")
            .WithTags("google", "image-to-image", "image-editing", "gemini")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-3-t2v")
            .WithEndpointOverride("fal-ai/veo3")
            .WithTitle("Veo 3 (T2V)")
            .WithDescription("Google's Veo 3 - the most advanced AI video generation model with sound")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-3-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Highest quality text-to-video with audio from Google")
            .WithTags("google", "veo", "text-to-video", "video", "audio")
            .WithFeatureFlag("fal_veo_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-2-t2v")
            .WithEndpointOverride("fal-ai/veo2")
            .WithTitle("Veo 2 (T2V)")
            .WithDescription("Google's Veo 2 high-quality video generation")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-2-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality text-to-video from Google")
            .WithTags("google", "veo", "text-to-video", "video")
            .WithFeatureFlag("fal_veo_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-2-i2v")
            .WithEndpointOverride("fal-ai/veo2/image-to-video")
            .WithTitle("Veo 2 (I2V)")
            .WithDescription("Google Veo 2 image-to-video with realistic motion")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-2-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Realistic motion from images")
            .WithTags("google", "veo", "image-to-video", "video")
            .WithFeatureFlag("fal_veo_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Google/veo-3.1-ref-t2v")
            .WithEndpointOverride("fal-ai/veo3.1/reference-to-video")
            .WithTitle("Veo 3.1 Reference (T2V)")
            .WithDescription("Generate videos from reference images using Google's Veo 3.1")
            .WithAuthor("Google DeepMind")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Google/veo-3.1-ref-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Reference-guided video generation")
            .WithTags("google", "veo", "image-to-video", "video", "reference")
            .WithFeatureFlag("fal_veo_video_params")
            .Build()
    ];

    #endregion

    #region Kling

    private static IEnumerable<ModelDefinition> KlingModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-image-v3")
            .WithEndpointOverride("fal-ai/kling-image/v3/text-to-image")
            .WithTitle("Kling Image V3")
            .WithDescription("Kling's V3 text-to-image generation model")
            .WithAuthor("Kling")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-image-v3.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality image generation")
            .WithTags("kling", "text-to-image")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-image-o3")
            .WithEndpointOverride("fal-ai/kling-image/o3/text-to-image")
            .WithTitle("Kling Omni 3")
            .WithDescription("Kling Omni 3 text-to-image generation model")
            .WithAuthor("Kling")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-image-o3.jpg")
            .WithDate("2025")
            .WithUsageHint("Latest Kling image generation")
            .WithTags("kling", "text-to-image", "omni")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-image-v3-i2i")
            .WithEndpointOverride("fal-ai/kling-image/v3/image-to-image")
            .WithTitle("Kling Image V3 (I2I)")
            .WithDescription("Kling V3 image-to-image generation")
            .WithAuthor("Kling")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-image-v3-i2i.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-image editing")
            .WithTags("kling", "image-to-image", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-image-o3-i2i")
            .WithEndpointOverride("fal-ai/kling-image/o3/image-to-image")
            .WithTitle("Kling Omni 3 (I2I)")
            .WithDescription("Kling Omni 3 image-to-image with flawless consistency")
            .WithAuthor("Kling")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-image-o3-i2i.jpg")
            .WithDate("2025")
            .WithUsageHint("High-consistency image editing")
            .WithTags("kling", "image-to-image", "omni", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-o3-pro-t2v")
            .WithEndpointOverride("fal-ai/kling-video/o3/pro/text-to-video")
            .WithTitle("Kling O3 Pro (T2V)")
            .WithDescription("Generate realistic videos using Kling O3 Pro")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-o3-pro-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Professional text-to-video generation")
            .WithTags("kling", "text-to-video", "video", "o3", "pro")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-o3-pro-i2v")
            .WithEndpointOverride("fal-ai/kling-video/o3/pro/image-to-video")
            .WithTitle("Kling O3 Pro (I2V)")
            .WithDescription("Generate realistic videos from images using Kling O3 Pro")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-o3-pro-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Professional image-to-video generation")
            .WithTags("kling", "image-to-video", "video", "o3", "pro")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-o3-std-t2v")
            .WithEndpointOverride("fal-ai/kling-video/o3/standard/text-to-video")
            .WithTitle("Kling O3 Standard (T2V)")
            .WithDescription("Generate videos using Kling O3 Standard")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-o3-std-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Cost-effective text-to-video")
            .WithTags("kling", "text-to-video", "video", "o3", "standard")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-o3-std-i2v")
            .WithEndpointOverride("fal-ai/kling-video/o3/standard/image-to-video")
            .WithTitle("Kling O3 Standard (I2V)")
            .WithDescription("Generate videos from images using Kling O3 Standard")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-o3-std-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Cost-effective image-to-video")
            .WithTags("kling", "image-to-video", "video", "o3", "standard")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-v3-pro-t2v")
            .WithEndpointOverride("fal-ai/kling-video/v3/pro/text-to-video")
            .WithTitle("Kling V3 Pro (T2V)")
            .WithDescription("Top-tier text-to-video with cinematic visuals and fluid motion")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-v3-pro-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Cinematic text-to-video generation")
            .WithTags("kling", "text-to-video", "video", "v3", "pro")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-v3-pro-i2v")
            .WithEndpointOverride("fal-ai/kling-video/v3/pro/image-to-video")
            .WithTitle("Kling V3 Pro (I2V)")
            .WithDescription("Top-tier image-to-video with cinematic visuals and fluid motion")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-v3-pro-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Cinematic image-to-video generation")
            .WithTags("kling", "image-to-video", "video", "v3", "pro")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-v3-std-t2v")
            .WithEndpointOverride("fal-ai/kling-video/v3/standard/text-to-video")
            .WithTitle("Kling V3 Standard (T2V)")
            .WithDescription("Kling 3.0 Standard text-to-video with cinematic visuals and audio")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-v3-std-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Cost-effective cinematic text-to-video")
            .WithTags("kling", "text-to-video", "video", "v3", "standard")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-v3-std-i2v")
            .WithEndpointOverride("fal-ai/kling-video/v3/standard/image-to-video")
            .WithTitle("Kling V3 Standard (I2V)")
            .WithDescription("Kling 3.0 Standard image-to-video with cinematic visuals and audio")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-v3-std-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Cost-effective cinematic image-to-video")
            .WithTags("kling", "image-to-video", "video", "v3", "standard")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-v2.5-turbo-pro-t2v")
            .WithEndpointOverride("fal-ai/kling-video/v2.5-turbo/pro/text-to-video")
            .WithTitle("Kling V2.5 Turbo Pro (T2V)")
            .WithDescription("Top-tier text-to-video with unparalleled motion fluidity and cinematic visuals")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-v2.5-turbo-pro-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast professional text-to-video")
            .WithTags("kling", "text-to-video", "video", "v2.5", "turbo", "pro")
            .WithFeatureFlag("fal_kling_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Kling/kling-v2.5-turbo-pro-i2v")
            .WithEndpointOverride("fal-ai/kling-video/v2.5-turbo/pro/image-to-video")
            .WithTitle("Kling V2.5 Turbo Pro (I2V)")
            .WithDescription("Top-tier image-to-video with unparalleled motion fluidity and cinematic visuals")
            .WithAuthor("Kling")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kling/kling-v2.5-turbo-pro-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast professional image-to-video")
            .WithTags("kling", "image-to-video", "video", "v2.5", "turbo", "pro")
            .WithFeatureFlag("fal_kling_video_params")
            .Build()
    ];

    #endregion

    #region Qwen

    private static IEnumerable<ModelDefinition> QwenModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Qwen/qwen-image")
            .WithEndpointOverride("fal-ai/qwen-image")
            .WithTitle("Qwen Image")
            .WithDescription("Image generation foundation model with complex text rendering and precise editing")
            .WithAuthor("Alibaba")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Qwen/qwen-image.jpg")
            .WithDate("2025")
            .WithUsageHint("Strong text rendering and image editing")
            .WithTags("qwen", "text-to-image", "text-rendering")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Bria

    private static IEnumerable<ModelDefinition> BriaModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Bria/bria-fibo")
            .WithEndpointOverride("bria/fibo/generate")
            .WithTitle("Bria FIBO Generate")
            .WithDescription("Enterprise-grade image generation trained on licensed data")
            .WithAuthor("Bria")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Bria/bria-fibo.jpg")
            .WithDate("2025")
            .WithUsageHint("Commercially safe image generation")
            .WithTags("bria", "text-to-image", "enterprise", "licensed")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Bria/bria-fibo-edit")
            .WithEndpointOverride("bria/fibo-edit/edit")
            .WithTitle("Bria FIBO Edit")
            .WithDescription("High-quality editing with maximum controllability using JSON + Mask + Image")
            .WithAuthor("Bria")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Bria/bria-fibo-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Commercially safe precise image editing")
            .WithTags("bria", "image-to-image", "enterprise", "licensed", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build()
    ];

    #endregion

    #region ByteDance

    private static IEnumerable<ModelDefinition> ByteDanceModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("ByteDance/seedream-v4")
            .WithEndpointOverride("fal-ai/bytedance/seedream/v4/edit")
            .WithTitle("Seedream 4.0")
            .WithDescription("ByteDance's image generation and editing model with unified architecture")
            .WithAuthor("ByteDance")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/ByteDance/seedream-v4.jpg")
            .WithDate("2025")
            .WithUsageHint("Unified generation and editing")
            .WithTags("bytedance", "seedream", "text-to-image", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build()
    ];

    #endregion

    #region Reve

    private static IEnumerable<ModelDefinition> ReveModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Reve/reve-edit")
            .WithEndpointOverride("fal-ai/reve/edit")
            .WithTitle("Reve Edit")
            .WithDescription("High-quality image editing model")
            .WithAuthor("Reve")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Reve/reve-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Precise image editing")
            .WithTags("reve", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build()
    ];

    #endregion

    #region ImagineArt

    private static IEnumerable<ModelDefinition> ImagineArtModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("ImagineArt/imagineart-1.5")
            .WithEndpointOverride("imagineart/imagineart-1.5-preview/text-to-image")
            .WithTitle("ImagineArt 1.5")
            .WithDescription("ImagineArt's text-to-image generation model")
            .WithAuthor("ImagineArt")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/ImagineArt/imagineart-1.5.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality artistic image generation")
            .WithTags("imagineart", "text-to-image")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region F-Lite (Fal x Freepik)

    private static IEnumerable<ModelDefinition> FLiteModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("FLite/f-lite-standard")
            .WithEndpointOverride("fal-ai/f-lite/standard")
            .WithTitle("F-Lite Standard")
            .WithDescription("10B parameter model trained on 80M copyright-safe images by Fal and Freepik")
            .WithAuthor("Fal x Freepik")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLite/f-lite-standard.jpg")
            .WithDate("2025")
            .WithUsageHint("Commercial-safe image generation")
            .WithTags("f-lite", "text-to-image", "commercial-safe")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("FLite/f-lite-texture")
            .WithEndpointOverride("fal-ai/f-lite/texture")
            .WithTitle("F-Lite Texture")
            .WithDescription("F-Lite texture generation mode for materials and patterns")
            .WithAuthor("Fal x Freepik")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/FLite/f-lite-texture.jpg")
            .WithDate("2025")
            .WithUsageHint("Texture and pattern generation")
            .WithTags("f-lite", "text-to-image", "texture", "commercial-safe")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region HiDream

    private static IEnumerable<ModelDefinition> HiDreamModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("HiDream/hidream-i1-full")
            .WithEndpointOverride("fal-ai/hidream-i1-full")
            .WithTitle("HiDream I1 Full")
            .WithDescription("17B parameter open-source model with state-of-the-art image quality")
            .WithAuthor("HiDream")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/HiDream/hidream-i1-full.jpg")
            .WithDate("2025")
            .WithUsageHint("Maximum quality open-source generation")
            .WithTags("hidream", "text-to-image", "open-source", "17b")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("HiDream/hidream-i1-dev")
            .WithEndpointOverride("fal-ai/hidream-i1-dev")
            .WithTitle("HiDream I1 Dev")
            .WithDescription("HiDream I1 developer model with 17B parameters")
            .WithAuthor("HiDream")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/HiDream/hidream-i1-dev.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality developer-focused generation")
            .WithTags("hidream", "text-to-image", "open-source")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("HiDream/hidream-i1-fast")
            .WithEndpointOverride("fal-ai/hidream-i1-fast")
            .WithTitle("HiDream I1 Fast")
            .WithDescription("Fast HiDream I1 model achieving SOTA quality in 16 steps")
            .WithAuthor("HiDream")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/HiDream/hidream-i1-fast.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast high-quality generation")
            .WithTags("hidream", "text-to-image", "open-source", "fast")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("HiDream/hidream-e1-full")
            .WithEndpointOverride("fal-ai/hidream-e1-full")
            .WithTitle("HiDream E1 Full")
            .WithDescription("HiDream E1 Full editing model")
            .WithAuthor("HiDream")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/HiDream/hidream-e1-full.jpg")
            .WithDate("2025")
            .WithUsageHint("Image editing with HiDream")
            .WithTags("hidream", "image-to-image", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build()
    ];

    #endregion

    #region OmniGen

    private static IEnumerable<ModelDefinition> OmniGenModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("OmniGen/omnigen-v1")
            .WithEndpointOverride("fal-ai/omnigen-v1")
            .WithTitle("OmniGen V1")
            .WithDescription("Unified model for multi-modal generation, editing, virtual try-on, and more")
            .WithAuthor("OmniGen")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/OmniGen/omnigen-v1.jpg")
            .WithDate("2024")
            .WithUsageHint("Multi-task image generation and editing")
            .WithTags("omnigen", "text-to-image", "image-editing", "multi-modal")
            .WithFeatureFlag("fal_i2i_params")
            .Build()
    ];

    #endregion

    #region AuraFlow

    private static IEnumerable<ModelDefinition> AuraFlowModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("AuraFlow/aura-flow")
            .WithEndpointOverride("fal-ai/aura-flow")
            .WithTitle("AuraFlow v0.3")
            .WithDescription("Open-source flow-based text-to-image with SOTA GenEval results")
            .WithAuthor("Fal.ai")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/AuraFlow/aura-flow.jpg")
            .WithDate("2024")
            .WithUsageHint("Flow-based generation with excellent prompt following")
            .WithTags("auraflow", "text-to-image", "open-source", "flow")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Lumina

    private static IEnumerable<ModelDefinition> LuminaModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Lumina/lumina-image-v2")
            .WithEndpointOverride("fal-ai/lumina-image/v2")
            .WithTitle("Lumina Image V2")
            .WithDescription("Lumina Image V2 text-to-image generation model")
            .WithAuthor("Lumina")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Lumina/lumina-image-v2.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality image generation")
            .WithTags("lumina", "text-to-image")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Sana

    private static IEnumerable<ModelDefinition> SanaModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Sana/sana-sprint")
            .WithEndpointOverride("fal-ai/sana/sprint")
            .WithTitle("Sana Sprint")
            .WithDescription("Ultra-fast Sana model for rapid image generation")
            .WithAuthor("Sana")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Sana/sana-sprint.jpg")
            .WithDate("2025")
            .WithUsageHint("Fastest possible image generation")
            .WithTags("sana", "text-to-image", "fast")
            .WithFeatureFlag("fal_t2i_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Sana/sana-v1.5-4.8b")
            .WithEndpointOverride("fal-ai/sana/v1.5/4.8b")
            .WithTitle("Sana v1.5 4.8B")
            .WithDescription("Sana v1.5 with 4.8B parameters for high-quality generation")
            .WithAuthor("Sana")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Sana/sana-v1.5-4.8b.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality fast generation")
            .WithTags("sana", "text-to-image")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Playground

    private static IEnumerable<ModelDefinition> PlaygroundModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Playground/playground-v2.5")
            .WithEndpointOverride("fal-ai/playground-v25")
            .WithTitle("Playground v2.5")
            .WithDescription("Playground AI's v2.5 text-to-image model with aesthetic focus")
            .WithAuthor("Playground AI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Playground/playground-v2.5.jpg")
            .WithDate("2024")
            .WithUsageHint("Aesthetic-focused image generation")
            .WithTags("playground", "text-to-image", "aesthetic")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Kolors

    private static IEnumerable<ModelDefinition> KolorsModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Kolors/kolors")
            .WithEndpointOverride("fal-ai/kolors")
            .WithTitle("Kolors")
            .WithDescription("Kuaishou's text-to-image model with bilingual support")
            .WithAuthor("Kuaishou")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kolors/kolors.jpg")
            .WithDate("2024")
            .WithUsageHint("Bilingual English/Chinese image generation")
            .WithTags("kolors", "text-to-image", "bilingual")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region MiniMax (Image)

    private static IEnumerable<ModelDefinition> MiniMaxImageModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("MiniMax/minimax-image-01")
            .WithEndpointOverride("fal-ai/minimax/image-01")
            .WithTitle("MiniMax Image 01")
            .WithDescription("MiniMax text-to-image generation model")
            .WithAuthor("MiniMax")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/MiniMax/minimax-image-01.jpg")
            .WithDate("2025")
            .WithUsageHint("Text-to-image from MiniMax")
            .WithTags("minimax", "text-to-image")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Step1X

    private static IEnumerable<ModelDefinition> Step1XModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Step1X/step1x-edit")
            .WithEndpointOverride("fal-ai/step1x-edit")
            .WithTitle("Step1X Edit")
            .WithDescription("Step1X image editing model")
            .WithAuthor("Step1X")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Step1X/step1x-edit.jpg")
            .WithDate("2025")
            .WithUsageHint("Precise image editing")
            .WithTags("step1x", "image-to-image", "image-editing")
            .WithFeatureFlag("fal_i2i_params")
            .Build()
    ];

    #endregion

    #region Hunyuan (Image)

    private static IEnumerable<ModelDefinition> HunyuanImageModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Hunyuan/hunyuan-image-v3")
            .WithEndpointOverride("fal-ai/hunyuan-image/v3/text-to-image")
            .WithTitle("Hunyuan Image V3")
            .WithDescription("Tencent's 80B parameter MoE image generation model")
            .WithAuthor("Tencent")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Hunyuan/hunyuan-image-v3.jpg")
            .WithDate("2025")
            .WithUsageHint("Large-scale open-source image generation")
            .WithTags("hunyuan", "text-to-image", "tencent", "open-source")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region UNO

    private static IEnumerable<ModelDefinition> UNOModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("UNO/uno")
            .WithEndpointOverride("fal-ai/uno")
            .WithTitle("UNO")
            .WithDescription("UNO style transfer and image generation model")
            .WithAuthor("Fal.ai")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/UNO/uno.jpg")
            .WithDate("2025")
            .WithUsageHint("Style transfer and generation")
            .WithTags("uno", "text-to-image", "style-transfer")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region Instant Character

    private static IEnumerable<ModelDefinition> InstantCharacterModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("InstantCharacter/instant-character")
            .WithEndpointOverride("fal-ai/instant-character")
            .WithTitle("Instant Character")
            .WithDescription("Generate consistent character images from reference photos")
            .WithAuthor("Fal.ai")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/InstantCharacter/instant-character.jpg")
            .WithDate("2025")
            .WithUsageHint("Character-consistent image generation")
            .WithTags("instant-character", "text-to-image", "character")
            .WithFeatureFlag("fal_t2i_params")
            .Build()
    ];

    #endregion

    #region OpenAI (Sora)

    private static IEnumerable<ModelDefinition> SoraModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Sora/sora-2-t2v")
            .WithEndpointOverride("fal-ai/sora-2/text-to-video")
            .WithTitle("Sora 2 (T2V)")
            .WithDescription("OpenAI's state-of-the-art video model for richly detailed dynamic clips with audio")
            .WithAuthor("OpenAI")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Sora/sora-2-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Highest quality text-to-video from OpenAI")
            .WithTags("sora", "openai", "text-to-video", "video", "audio")
            .WithFeatureFlag("fal_sora_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Sora/sora-2-pro-t2v")
            .WithEndpointOverride("fal-ai/sora-2/text-to-video/pro")
            .WithTitle("Sora 2 Pro (T2V)")
            .WithDescription("Professional tier of OpenAI's Sora 2 video generation")
            .WithAuthor("OpenAI")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Sora/sora-2-pro-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Premium text-to-video quality")
            .WithTags("sora", "openai", "text-to-video", "video", "pro")
            .WithFeatureFlag("fal_sora_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Sora/sora-2-i2v")
            .WithEndpointOverride("fal-ai/sora-2/image-to-video")
            .WithTitle("Sora 2 (I2V)")
            .WithDescription("Create richly detailed dynamic clips with audio from images")
            .WithAuthor("OpenAI")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Sora/sora-2-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video with audio from OpenAI")
            .WithTags("sora", "openai", "image-to-video", "video", "audio")
            .WithFeatureFlag("fal_sora_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Sora/sora-2-pro-i2v")
            .WithEndpointOverride("fal-ai/sora-2/image-to-video/pro")
            .WithTitle("Sora 2 Pro (I2V)")
            .WithDescription("Professional tier image-to-video from OpenAI's Sora 2")
            .WithAuthor("OpenAI")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Sora/sora-2-pro-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Premium image-to-video quality")
            .WithTags("sora", "openai", "image-to-video", "video", "pro")
            .WithFeatureFlag("fal_sora_video_params")
            .Build()
    ];

    #endregion

    #region MiniMax (Video)

    private static IEnumerable<ModelDefinition> MiniMaxVideoModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("MiniMax/hailuo-02-t2v")
            .WithEndpointOverride("fal-ai/minimax/hailuo-02/standard/text-to-video")
            .WithTitle("Hailuo-02 (T2V)")
            .WithDescription("MiniMax's Hailuo-02 text-to-video generation model")
            .WithAuthor("MiniMax")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/MiniMax/hailuo-02-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Text-to-video generation")
            .WithTags("minimax", "hailuo", "text-to-video", "video")
            .WithFeatureFlag("fal_minimax_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("MiniMax/hailuo-02-i2v")
            .WithEndpointOverride("fal-ai/minimax/hailuo-02/standard/image-to-video")
            .WithTitle("Hailuo-02 (I2V)")
            .WithDescription("MiniMax's advanced image-to-video generation model")
            .WithAuthor("MiniMax")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/MiniMax/hailuo-02-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video generation")
            .WithTags("minimax", "hailuo", "image-to-video", "video")
            .WithFeatureFlag("fal_minimax_video_params")
            .Build()
    ];

    #endregion

    #region PixVerse

    private static IEnumerable<ModelDefinition> PixVerseModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("PixVerse/pixverse-v5-t2v")
            .WithEndpointOverride("fal-ai/pixverse/v5/text-to-video")
            .WithTitle("PixVerse V5 (T2V)")
            .WithDescription("PixVerse V5 text-to-video generation")
            .WithAuthor("PixVerse")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/PixVerse/pixverse-v5-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Text-to-video generation")
            .WithTags("pixverse", "text-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("PixVerse/pixverse-v5-i2v")
            .WithEndpointOverride("fal-ai/pixverse/v5/image-to-video")
            .WithTitle("PixVerse V5 (I2V)")
            .WithDescription("PixVerse V5 image-to-video generation")
            .WithAuthor("PixVerse")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/PixVerse/pixverse-v5-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video generation")
            .WithTags("pixverse", "image-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Wan

    private static IEnumerable<ModelDefinition> WanModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Wan/wan-2.2-t2v")
            .WithEndpointOverride("fal-ai/wan/v2.2-a14b/text-to-video")
            .WithTitle("Wan 2.2 (T2V)")
            .WithDescription("High-quality video generation with motion diversity from text prompts")
            .WithAuthor("Wan")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Wan/wan-2.2-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Text-to-video with LoRA support")
            .WithTags("wan", "text-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Wan/wan-2.2-i2v")
            .WithEndpointOverride("fal-ai/wan/v2.2-a14b/image-to-video")
            .WithTitle("Wan 2.2 (I2V)")
            .WithDescription("High-quality video generation from images with LoRA support")
            .WithAuthor("Wan")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Wan/wan-2.2-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video with LoRA support")
            .WithTags("wan", "image-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Lightricks (LTX)

    private static IEnumerable<ModelDefinition> LTXModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("LTX/ltx-2-19b-i2v")
            .WithEndpointOverride("fal-ai/ltx-2-19b/image-to-video")
            .WithTitle("LTX-2 19B (I2V)")
            .WithDescription("Generate video with audio from images using LTX-2")
            .WithAuthor("Lightricks")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/LTX/ltx-2-19b-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video with audio")
            .WithTags("ltx", "image-to-video", "video", "audio")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("LTX/ltx-13b-distilled-i2v")
            .WithEndpointOverride("fal-ai/ltx-video-13b-distilled/image-to-video")
            .WithTitle("LTX 13B Distilled (I2V)")
            .WithDescription("Generate videos from prompts and images using custom LoRA")
            .WithAuthor("Lightricks")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/LTX/ltx-13b-distilled-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast image-to-video with LoRA support")
            .WithTags("ltx", "image-to-video", "video", "lora")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Vidu

    private static IEnumerable<ModelDefinition> ViduModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Vidu/vidu-q3-t2v")
            .WithEndpointOverride("fal-ai/vidu/q3/text-to-video")
            .WithTitle("Vidu Q3 (T2V)")
            .WithDescription("Vidu's latest Q3 pro text-to-video model")
            .WithAuthor("Vidu")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Vidu/vidu-q3-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Text-to-video generation")
            .WithTags("vidu", "text-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Vidu/vidu-q3-i2v")
            .WithEndpointOverride("fal-ai/vidu/q3/image-to-video")
            .WithTitle("Vidu Q3 (I2V)")
            .WithDescription("Vidu's latest Q3 pro image-to-video model")
            .WithAuthor("Vidu")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Vidu/vidu-q3-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video generation")
            .WithTags("vidu", "image-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Hunyuan (Video)

    private static IEnumerable<ModelDefinition> HunyuanVideoModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Hunyuan/hunyuan-video-t2v")
            .WithEndpointOverride("fal-ai/hunyuan-video")
            .WithTitle("Hunyuan Video (T2V)")
            .WithDescription("Tencent's open video generation model with high visual quality and motion diversity")
            .WithAuthor("Tencent")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Hunyuan/hunyuan-video-t2v.jpg")
            .WithDate("2024")
            .WithUsageHint("Open-source text-to-video generation")
            .WithTags("hunyuan", "tencent", "text-to-video", "video", "open-source")
            .WithFeatureFlag("fal_hunyuan_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Hunyuan/hunyuan-video-i2v")
            .WithEndpointOverride("fal-ai/hunyuan-video-image-to-video")
            .WithTitle("Hunyuan Video (I2V)")
            .WithDescription("Tencent's image-to-video generation with high visual quality")
            .WithAuthor("Tencent")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Hunyuan/hunyuan-video-i2v.jpg")
            .WithDate("2024")
            .WithUsageHint("Open-source image-to-video generation")
            .WithTags("hunyuan", "tencent", "image-to-video", "video", "open-source")
            .WithFeatureFlag("fal_hunyuan_video_params")
            .Build()
    ];

    #endregion

    #region Mochi

    private static IEnumerable<ModelDefinition> MochiModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Mochi/mochi-v1")
            .WithEndpointOverride("fal-ai/mochi-v1")
            .WithTitle("Mochi 1")
            .WithDescription("Open state-of-the-art video model with high-fidelity motion and strong prompt adherence")
            .WithAuthor("Genmo")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Mochi/mochi-v1.jpg")
            .WithDate("2024")
            .WithUsageHint("High-fidelity motion video generation")
            .WithTags("mochi", "text-to-video", "video", "open-source")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Luma (Dream Machine)

    private static IEnumerable<ModelDefinition> LumaModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Luma/ray-2-t2v")
            .WithEndpointOverride("fal-ai/luma-dream-machine/ray-2")
            .WithTitle("Luma Ray 2 (T2V)")
            .WithDescription("Luma Dream Machine Ray 2 text-to-video generation")
            .WithAuthor("Luma AI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Luma/ray-2-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Premium text-to-video generation")
            .WithTags("luma", "ray2", "text-to-video", "video")
            .WithFeatureFlag("fal_luma_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Luma/ray-2-i2v")
            .WithEndpointOverride("fal-ai/luma-dream-machine/ray-2/image-to-video")
            .WithTitle("Luma Ray 2 (I2V)")
            .WithDescription("Luma Dream Machine Ray 2 image-to-video generation")
            .WithAuthor("Luma AI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Luma/ray-2-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Premium image-to-video generation")
            .WithTags("luma", "ray2", "image-to-video", "video")
            .WithFeatureFlag("fal_luma_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Luma/ray-2-flash-t2v")
            .WithEndpointOverride("fal-ai/luma-dream-machine/ray-2-flash")
            .WithTitle("Luma Ray 2 Flash (T2V)")
            .WithDescription("Fast Luma Dream Machine Ray 2 text-to-video")
            .WithAuthor("Luma AI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Luma/ray-2-flash-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast text-to-video generation")
            .WithTags("luma", "ray2", "text-to-video", "video", "fast")
            .WithFeatureFlag("fal_luma_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Luma/ray-2-flash-i2v")
            .WithEndpointOverride("fal-ai/luma-dream-machine/ray-2-flash/image-to-video")
            .WithTitle("Luma Ray 2 Flash (I2V)")
            .WithDescription("Fast Luma Dream Machine Ray 2 image-to-video")
            .WithAuthor("Luma AI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Luma/ray-2-flash-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast image-to-video generation")
            .WithTags("luma", "ray2", "image-to-video", "video", "fast")
            .WithFeatureFlag("fal_luma_video_params")
            .Build()
    ];

    #endregion

    #region Pika

    private static IEnumerable<ModelDefinition> PikaModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Pika/pika-v2.2-t2v")
            .WithEndpointOverride("fal-ai/pika/v2.2/text-to-video")
            .WithTitle("Pika 2.2 (T2V)")
            .WithDescription("Pika v2.2 text-to-video generation with HD support")
            .WithAuthor("Pika")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Pika/pika-v2.2-t2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Creative text-to-video generation")
            .WithTags("pika", "text-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Pika/pika-v2.2-i2v")
            .WithEndpointOverride("fal-ai/pika/v2.2/image-to-video")
            .WithTitle("Pika 2.2 (I2V)")
            .WithDescription("Pika v2.2 image-to-video generation with HD support")
            .WithAuthor("Pika")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Pika/pika-v2.2-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Creative image-to-video generation")
            .WithTags("pika", "image-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Kandinsky

    private static IEnumerable<ModelDefinition> KandinskyModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Kandinsky/kandinsky-5-pro-i2v")
            .WithEndpointOverride("fal-ai/kandinsky5-pro/image-to-video")
            .WithTitle("Kandinsky 5.0 Pro (I2V)")
            .WithDescription("Fast, high-quality image-to-video generation from Kandinsky")
            .WithAuthor("Kandinsky")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Kandinsky/kandinsky-5-pro-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast image-to-video generation")
            .WithTags("kandinsky", "image-to-video", "video", "fast")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Magi

    private static IEnumerable<ModelDefinition> MagiModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Magi/magi-i2v")
            .WithEndpointOverride("fal-ai/magi/image-to-video")
            .WithTitle("Magi (I2V)")
            .WithDescription("Magi image-to-video generation model")
            .WithAuthor("Sand AI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Magi/magi-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality image-to-video")
            .WithTags("magi", "image-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Magi/magi-distilled-i2v")
            .WithEndpointOverride("fal-ai/magi-distilled/image-to-video")
            .WithTitle("Magi Distilled (I2V)")
            .WithDescription("Faster distilled version of Magi image-to-video")
            .WithAuthor("Sand AI")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Magi/magi-distilled-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast image-to-video generation")
            .WithTags("magi", "image-to-video", "video", "fast")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region CogVideoX

    private static IEnumerable<ModelDefinition> CogVideoXModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("CogVideoX/cogvideox-5b")
            .WithEndpointOverride("fal-ai/cogvideox-5b")
            .WithTitle("CogVideoX 5B")
            .WithDescription("CogVideoX 5B text-to-video generation model")
            .WithAuthor("THUDM")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/CogVideoX/cogvideox-5b.jpg")
            .WithDate("2024")
            .WithUsageHint("Open-source text-to-video generation")
            .WithTags("cogvideox", "text-to-video", "video", "open-source")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region SkyReels

    private static IEnumerable<ModelDefinition> SkyReelsModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("SkyReels/skyreels-i2v")
            .WithEndpointOverride("fal-ai/skyreels-i2v")
            .WithTitle("SkyReels (I2V)")
            .WithDescription("SkyReels image-to-video generation")
            .WithAuthor("SkyReels")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/SkyReels/skyreels-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Image-to-video generation")
            .WithTags("skyreels", "image-to-video", "video")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Decart

    private static IEnumerable<ModelDefinition> DecartModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Decart/lucy-14b-i2v")
            .WithEndpointOverride("decart/lucy-14b/image-to-video")
            .WithTitle("Lucy-14B (I2V)")
            .WithDescription("Lightning fast image-to-video generation")
            .WithAuthor("Decart")
            .WithDimensions(1280, 720)
            .WithPreviewImage("Images/ModelPreviews/Fal/Decart/lucy-14b-i2v.jpg")
            .WithDate("2025")
            .WithUsageHint("Ultra-fast image-to-video")
            .WithTags("decart", "lucy", "image-to-video", "video", "fast")
            .WithFeatureFlag("fal_video_params")
            .Build()
    ];

    #endregion

    #region Utility

    private static IEnumerable<ModelDefinition> UtilityModels =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("Utility/rembg")
            .WithEndpointOverride("fal-ai/imageutils/rembg")
            .WithTitle("Remove Background")
            .WithDescription("Remove backgrounds from images automatically")
            .WithAuthor("Fal.ai")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/rembg.jpg")
            .WithDate("2024")
            .WithUsageHint("Quick background removal for any image")
            .WithTags("utility", "background-removal")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/clarity-upscaler")
            .WithEndpointOverride("fal-ai/clarity-upscaler")
            .WithTitle("Clarity Upscaler")
            .WithDescription("AI-powered image upscaling with detail enhancement")
            .WithAuthor("Fal.ai")
            .WithDimensions(4096, 4096)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/clarity-upscaler.jpg")
            .WithDate("2024")
            .WithUsageHint("Upscale images up to 4x with enhanced details")
            .WithTags("utility", "upscaler", "enhancement")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/topaz-upscale")
            .WithEndpointOverride("fal-ai/topaz/upscale/image")
            .WithTitle("Topaz Upscale")
            .WithDescription("Topaz-powered image upscaling")
            .WithAuthor("Topaz")
            .WithDimensions(4096, 4096)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/topaz-upscale.jpg")
            .WithDate("2025")
            .WithUsageHint("Professional image upscaling")
            .WithTags("utility", "upscaler", "topaz")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/bria-rmbg")
            .WithEndpointOverride("fal-ai/bria/background/remove")
            .WithTitle("Bria RMBG 2.0")
            .WithDescription("Bria's background removal model")
            .WithAuthor("Bria")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/bria-rmbg.jpg")
            .WithDate("2025")
            .WithUsageHint("High-quality background removal")
            .WithTags("utility", "background-removal", "bria")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/creative-upscaler")
            .WithEndpointOverride("fal-ai/creative-upscaler")
            .WithTitle("Creative Upscaler")
            .WithDescription("AI-powered creative upscaling with detail enhancement")
            .WithAuthor("Fal.ai")
            .WithDimensions(4096, 4096)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/creative-upscaler.jpg")
            .WithDate("2024")
            .WithUsageHint("Creative upscaling with artistic enhancement")
            .WithTags("utility", "upscaler", "creative")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/esrgan")
            .WithEndpointOverride("fal-ai/esrgan")
            .WithTitle("ESRGAN")
            .WithDescription("Enhanced Super-Resolution GAN for image upscaling")
            .WithAuthor("Fal.ai")
            .WithDimensions(4096, 4096)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/esrgan.jpg")
            .WithDate("2024")
            .WithUsageHint("Fast image upscaling")
            .WithTags("utility", "upscaler", "esrgan")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/ben-v2-bg-remove")
            .WithEndpointOverride("fal-ai/ben/v2/image")
            .WithTitle("BEN V2 Background Removal")
            .WithDescription("BEN V2 high-quality image background removal")
            .WithAuthor("Fal.ai")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/ben-v2-bg-remove.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast background removal")
            .WithTags("utility", "background-removal")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/codeformer")
            .WithEndpointOverride("fal-ai/codeformer")
            .WithTitle("CodeFormer")
            .WithDescription("AI face restoration and enhancement model")
            .WithAuthor("Fal.ai")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/codeformer.jpg")
            .WithDate("2024")
            .WithUsageHint("Face restoration and enhancement")
            .WithTags("utility", "face-restoration", "enhancement")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/topaz-video-upscale")
            .WithEndpointOverride("fal-ai/topaz/upscale/video")
            .WithTitle("Topaz Video Upscale")
            .WithDescription("Professional-grade video upscaling using Topaz technology")
            .WithAuthor("Topaz")
            .WithDimensions(3840, 2160)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/topaz-video-upscale.jpg")
            .WithDate("2025")
            .WithUsageHint("Upscale videos with high-quality enhancement")
            .WithTags("utility", "upscaler", "topaz", "video")
            .WithFeatureFlag("fal_utility_params")
            .Build(),
        ModelDefinitionBuilder.Create()
            .WithId("Utility/bria-video-bg-removal")
            .WithEndpointOverride("bria/video/background-removal")
            .WithTitle("Bria Video BG Removal")
            .WithDescription("Automatically remove backgrounds from videos for clean professional content")
            .WithAuthor("Bria")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/Fal/Utility/bria-video-bg-removal.jpg")
            .WithDate("2025")
            .WithUsageHint("Video background removal without green screen")
            .WithTags("utility", "background-removal", "bria", "video")
            .WithFeatureFlag("fal_utility_params")
            .Build()
    ];

    #endregion
}
