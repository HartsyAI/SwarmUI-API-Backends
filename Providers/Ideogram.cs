using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Ideogram API provider for text rendering focused image generation.</summary>
public sealed class IdeogramProvider : IProviderSource
{
    public static IdeogramProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("ideogram_api")
        .WithName("Ideogram")
        .WithModelPrefix("Ideogram")
        .WithBaseUrl("https://api.ideogram.ai/generate")
        .WithAuthHeader("Bearer", "Api-Key")
        .WithModelClass("ideogram_api", "Ideogram")
        .AddModels(Models)
        .Build();

    private static IEnumerable<ModelDefinition> Models =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("V_1")
            .WithTitle("Ideogram V1")
            .WithDescription("First generation Ideogram model with strong text rendering")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Ideogram/ideogram.png")
            .WithDate("2023")
            .WithUsageHint("Good for images requiring accurate text rendering")
            .WithTags("text-rendering", "generative")
            .WithFeatureFlag("ideogram_v1_params")
            .WithInitImage()
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_2")
            .WithTitle("Ideogram V2")
            .WithDescription("Second generation with improved quality and text accuracy")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Ideogram/ideogram.png")
            .WithDate("2024")
            .WithUsageHint("Enhanced text rendering and image quality")
            .WithTags("text-rendering", "high-quality")
            .WithFeatureFlag("ideogram_v2_params")
            .WithInitImage()
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_2_TURBO")
            .WithTitle("Ideogram V2 Turbo")
            .WithDescription("Faster V2 generation with optimized performance")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Ideogram/ideogram.png")
            .WithDate("2024")
            .WithUsageHint("Fast generation with good quality")
            .WithTags("text-rendering", "fast")
            .WithFeatureFlag("ideogram_v2_params")
            .WithInitImage()
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_2A")
            .WithTitle("Ideogram V2A")
            .WithDescription("Ideogram 2a model with strong text rendering")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Ideogram/ideogram.png")
            .WithDate("2025")
            .WithUsageHint("Good for images requiring accurate text rendering")
            .WithTags("text-rendering", "generative")
            .WithFeatureFlag("ideogram_v2_params")
            .WithInitImage()
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_3")
            .WithTitle("Ideogram V3")
            .WithDescription("Ideogram 3.0 with strong text rendering; supports remix and edit with an input image")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Ideogram/ideogram.png")
            .WithDate("2025")
            .WithUsageHint("Best for professional-grade images with perfect text")
            .WithTags("text-rendering", "premium", "high-quality")
            .WithFeatureFlag("ideogram_v3_params")
            .WithInitImage()
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("V_4")
            .WithTitle("Ideogram V4")
            .WithDescription("Ideogram 4.0 high-resolution generation with rendering speed and resolution controls")
            .WithAuthor("Ideogram")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Ideogram/ideogram.png")
            .WithDate("2026")
            .WithUsageHint("Best for high-resolution images; uses fixed resolutions and a rendering-speed/quality control")
            .WithTags("text-rendering", "premium", "high-quality", "high-resolution")
            .WithFeatureFlag("ideogram_v4_params")
            .WithInitImage()
            .Build()
    ];
}
