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
            .WithPreviewImage("Images/ModelPreviews/Ideogram/v1.jpg")
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
            .WithPreviewImage("Images/ModelPreviews/Ideogram/v2.jpg")
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
            .WithPreviewImage("Images/ModelPreviews/Ideogram/v2-turbo.jpg")
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
            .WithPreviewImage("Images/ModelPreviews/Ideogram/v3.jpg")
            .WithDate("2025")
            .WithUsageHint("Best for professional-grade images with perfect text")
            .WithTags("text-rendering", "premium", "high-quality")
            .WithFeatureFlag("ideogram_v3_params")
            .Build()
    ];
}
