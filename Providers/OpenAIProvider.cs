using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>OpenAI API provider for DALL-E and GPT Image models.</summary>
public sealed class OpenAIProvider : IProviderSource
{
    public static OpenAIProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("openai_api")
        .WithName("OpenAI")
        .WithModelPrefix("OpenAI")
        .WithBaseUrl("https://api.openai.com/v1/images/generations")
        .WithAuthHeader("Bearer")
        .WithModelClass("openai_api", "DALL-E")
        .AddModels(Models)
        .Build();

    private static IEnumerable<ModelDefinition> Models =>
    [
        ModelTemplates.DallE2("dall-e-2", "dalle2_params"),
        ModelTemplates.DallE3("dall-e-3", "dalle3_params"),
        ModelTemplates.GptImage1("gpt-image-1", "gpt-image-1_params"),

        ModelDefinitionBuilder.Create()
            .WithId("gpt-image-1.5")
            .WithTitle("GPT Image 1.5")
            .WithDescription("High-fidelity images with strong instruction following and accurate visual details")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/OpenAI/gpt-image-1.5.png")
            .WithDate("2025")
            .WithUsageHint("Best for context-aware image generation requiring strong instruction following")
            .WithTags("gpt-image", "high-quality", "text-accurate")
            .WithFeatureFlag("gpt-image-1.5_params")
            .Build()
    ];
}
