using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Grok (xAI) API provider for image generation.</summary>
public sealed class GrokProvider : IProviderSource
{
    public static GrokProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("grok_api")
        .WithName("Grok")
        .WithModelPrefix("Grok")
        .WithBaseUrl("https://api.x.ai/v1/images/generations")
        .WithAuthHeader("Bearer")
        .WithModelClass("grok_api", "Grok")
        .AddModels(Models)
        .Build();

    private static IEnumerable<ModelDefinition> Models =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("grok-2-image")
            .WithTitle("Grok 2 Image")
            .WithDescription("xAI's Grok 2 image generation model")
            .WithAuthor("xAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Grok/grok-2-image.png")
            .WithDate("2024")
            .WithUsageHint("High-quality image generation from xAI")
            .WithTags("grok", "xai", "generative")
            .WithFeatureFlag("grok_2_image_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("grok-imagine-image")
            .WithTitle("Grok Imagine Image")
            .WithDescription("xAI's Grok Imagine image generation model, version 1.0")
            .WithAuthor("xAI")
            .WithDimensions(1024, 1024)
            .WithDate("2026")
            .WithUsageHint("Lower-cost Grok Imagine image generation")
            .WithTags("grok", "xai", "imagine", "generative")
            .WithFeatureFlag("grok_2_image_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("grok-imagine-image-2.0")
            .WithTitle("Grok Imagine Image 2.0")
            .WithDescription("xAI's recommended image generation and editing model")
            .WithAuthor("xAI")
            .WithDimensions(1024, 1024)
            .WithDate("2026")
            .WithUsageHint("Recommended Grok model for image generation and editing")
            .WithTags("grok", "xai", "imagine", "generative")
            .WithFeatureFlag("grok_2_image_params")
            .WithInitImage()
            .Build()
    ];
}
