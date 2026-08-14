using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Black Forest Labs API provider for FLUX models.</summary>
public sealed class BlackForestLabsProvider : IProviderSource
{
    public static BlackForestLabsProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("bfl_api")
        .WithName("Black Forest Labs")
        .WithModelPrefix("BFL")
        .WithBaseUrl("https://api.bfl.ai")
        .WithAuthHeader("Bearer", "x-key")
        .WithModelClass("bfl_api", "FLUX")
        .AddModels(Models)
        .Build();

    private static IEnumerable<ModelDefinition> Models =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("flux-pro-1.1")
            .WithTitle("FLUX Pro 1.1")
            .WithDescription("Professional-grade FLUX model with enhanced capabilities")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-pro-1.1.png")
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
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-pro-1.1-ultra.png")
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
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-dev.png")
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
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-kontext-pro.png")
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
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-kontext-max.png")
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
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-2-pro.png")
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
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-2-max.png")
            .WithDate("2025")
            .WithUsageHint("Premium FLUX 2 generation with maximum quality")
            .WithTags("flux", "flux2", "premium")
            .WithFeatureFlag("flux_2_max_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-2-flex")
            .WithTitle("FLUX 2 Flex")
            .WithDescription("FLUX.2 tuned for typography and text rendering, with step and guidance control")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-2-flex.png")
            .WithDate("2026")
            .WithUsageHint("Best for posters, packaging and dense text")
            .WithTags("flux", "flux2", "typography")
            .WithFeatureFlag("flux_2_flex_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-2-klein-9b")
            .WithTitle("FLUX 2 Klein 9B")
            .WithDescription("Compact 9B FLUX.2 variant")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-2-klein-9b.png")
            .WithDate("2026")
            .WithUsageHint("Faster, cheaper FLUX.2 generation")
            .WithTags("flux", "flux2", "klein", "compact")
            .WithFeatureFlag("flux_2_pro_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-2-klein-4b")
            .WithTitle("FLUX 2 Klein 4B")
            .WithDescription("Smallest FLUX.2 variant")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-2-klein-4b.png")
            .WithDate("2026")
            .WithUsageHint("Fastest, cheapest FLUX.2 generation")
            .WithTags("flux", "flux2", "klein", "compact")
            .WithFeatureFlag("flux_2_pro_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("flux-3-video")
            .WithTitle("FLUX 3 Video")
            .WithDescription("Multimodal video generation with audio produced in the same pass, 5-20 seconds at HD or Full HD")
            .WithAuthor("Black Forest Labs")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/BFL/flux-3-video.png")
            .WithDate("2026")
            .WithUsageHint("Text-to-video, or supply an Init Image to animate from it")
            .WithTags("flux", "flux3", "video", "audio")
            .WithFamily("video.bfl_flux3")
            .WithInitImage()
            .Build()
    ];
}
