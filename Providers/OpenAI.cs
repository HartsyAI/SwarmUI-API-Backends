using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>OpenAI API provider for GPT Image models.</summary>
public sealed class OpenAIProvider : IProviderSource
{
    public static OpenAIProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("openai_api")
        .WithName("OpenAI")
        .WithModelPrefix("OpenAI")
        .WithBaseUrl("https://api.openai.com/v1/images/generations")
        .WithAuthHeader("Bearer")
        .WithModelClass("openai_api", "GPT Image")
        .AddModels(Models)
        .Build();

    private static ModelDefinition[] Models =>
    [
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
            .WithFamily("image.openai")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gpt-image-2")
            .WithTitle("GPT Image 2")
            .WithDescription("State-of-the-art image generation with exceptional photorealism, precise text rendering, and up to 2K resolution")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/OpenAI/gpt-image-2.png")
            .WithDate("2026")
            .WithUsageHint("Best for photorealistic images, accurate text, product photography, and complex visual layouts")
            .WithTags("gpt-image", "high-quality", "text-accurate", "photorealistic")
            .WithFeatureFlag("gpt-image-2_params")
            .WithFamily("image.openai")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gpt-image-2.5-sunburst")
            .WithTitle("GPT Image 2.5 Sunburst")
            .WithDescription("OpenAI's most capable model for image generation and editing")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithDate("2026")
            .WithUsageHint("Best for workflows where editing precision matters most")
            .WithTags("gpt-image", "high-quality", "editing")
            .WithFeatureFlag("gpt-image-2_params")
            .WithFamily("image.openai")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gpt-image-2.5-flare")
            .WithTitle("GPT Image 2.5 Flare")
            .WithDescription("OpenAI's fastest model for high-quality everyday image generation")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithDate("2026")
            .WithUsageHint("Best for fast, high-quality everyday image generation")
            .WithTags("gpt-image", "high-quality", "fast")
            .WithFeatureFlag("gpt-image-2_params")
            .WithFamily("image.openai")
            .Build()
    ];
}
