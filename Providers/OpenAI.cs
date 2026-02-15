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

    private static ModelDefinition[] Models =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("dall-e-2")
            .WithTitle("DALL-E 2")
            .WithDescription("OpenAI's DALL-E 2 model for image generation")
            .WithAuthor("OpenAI")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/OpenAI/dall-e-2.png")
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
            .WithPreviewImage("Images/ModelPreviews/OpenAI/dall-e-3.png")
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
            .WithPreviewImage("Images/ModelPreviews/OpenAI/gpt-image-1.png")
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
            .WithPreviewImage("Images/ModelPreviews/OpenAI/gpt-image-1.5.png")
            .WithDate("2025")
            .WithUsageHint("Best for context-aware image generation requiring strong instruction following")
            .WithTags("gpt-image", "high-quality", "text-accurate")
            .WithFeatureFlag("gpt-image-1.5_params")
            .Build(),

        // Sora Video Models
        ModelDefinitionBuilder.Create()
            .WithId("sora-2-t2v")
            .WithTitle("Sora 2 (T2V)")
            .WithDescription("OpenAI's state-of-the-art video generation model for creating richly detailed dynamic clips")
            .WithAuthor("OpenAI")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/OpenAI/sora-2.png")
            .WithDate("2025")
            .WithUsageHint("High-quality text-to-video generation directly from OpenAI")
            .WithTags("sora", "video", "text-to-video")
            .WithFeatureFlag("openai_sora_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("sora-2-pro-t2v")
            .WithTitle("Sora 2 Pro (T2V)")
            .WithDescription("Premium tier of OpenAI's Sora 2 video generation with enhanced quality")
            .WithAuthor("OpenAI")
            .WithDimensions(1920, 1080)
            .WithPreviewImage("Images/ModelPreviews/OpenAI/sora-2-pro.png")
            .WithDate("2025")
            .WithUsageHint("Premium text-to-video quality directly from OpenAI")
            .WithTags("sora", "video", "text-to-video", "pro")
            .WithFeatureFlag("openai_sora_params")
            .Build()
    ];
}
