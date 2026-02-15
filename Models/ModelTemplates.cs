#nullable enable

namespace Hartsy.Extensions.APIBackends.Models;

/// <summary>
/// Shared model templates for models that appear across multiple providers.
/// These templates define the base properties; providers customize Id, EndpointOverride, and FeatureFlag.
/// </summary>
public static class ModelTemplates
{
    #region OpenAI Models

    public static ModelDefinition DallE2(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "DALL-E 2",
            Description = "OpenAI's DALL-E 2 model for image generation",
            Author = "OpenAI",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/OpenAI/dall-e-2.png",
            Date = "2022",
            UsageHint = "Good for general image generation via API",
            Tags = ["dall-e", "generative"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    public static ModelDefinition DallE3(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "DALL-E 3",
            Description = "Advanced generative model for creating highly detailed and accurate images",
            Author = "OpenAI",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/OpenAI/dall-e-3.png",
            Date = "2023",
            UsageHint = "Excellent for high-quality image generation with accurate text representation",
            Tags = ["dall-e", "high-quality", "text-accurate"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    public static ModelDefinition GptImage1(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "GPT Image 1",
            Description = "Generates images with strong instruction following, contextual awareness, and world knowledge",
            Author = "OpenAI",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/OpenAI/gpt-image-1.png",
            Date = "2025",
            UsageHint = "Best for context-aware image generation and edits",
            Tags = ["gpt-image", "high-quality", "text-accurate"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    #endregion

    #region Black Forest Labs Models

    public static ModelDefinition FluxPro(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "FLUX.1 Pro",
            Description = "Black Forest Labs' flagship model with state-of-the-art image quality and prompt following",
            Author = "Black Forest Labs",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/BFL/flux-pro.jpg",
            Date = "2024",
            UsageHint = "Best for highest quality generation with excellent prompt adherence",
            Tags = ["flux", "professional", "high-quality"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    public static ModelDefinition FluxDev(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "FLUX.1 Dev",
            Description = "Open-weight model for non-commercial development with quality comparable to Pro",
            Author = "Black Forest Labs",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/BFL/flux-dev.jpg",
            Date = "2024",
            License = "Non-Commercial",
            UsageHint = "Great for development and testing, similar quality to Pro",
            Tags = ["flux", "development", "open-weight"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    public static ModelDefinition FluxSchnell(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "FLUX.1 Schnell",
            Description = "Fastest FLUX model optimized for speed while maintaining good quality",
            Author = "Black Forest Labs",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/BFL/flux-schnell.jpg",
            Date = "2024",
            License = "Apache 2.0",
            UsageHint = "Best for rapid iteration and testing",
            Tags = ["flux", "fast", "open-source"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    #endregion

    #region Ideogram Models

    public static ModelDefinition IdeogramV2(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "Ideogram V2",
            Description = "Second generation model with improved image quality and text rendering",
            Author = "Ideogram",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/Ideogram/ideogram-v2.jpg",
            Date = "2024",
            UsageHint = "Excellent for accurate text in images",
            Tags = ["ideogram", "text-rendering", "high-quality"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    public static ModelDefinition IdeogramV3(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "Ideogram V3",
            Description = "Latest Ideogram model with significantly improved text rendering and photorealism",
            Author = "Ideogram",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/Ideogram/ideogram-v3.jpg",
            Date = "2025",
            UsageHint = "Best for photorealistic images with perfect text",
            Tags = ["ideogram", "text-rendering", "photorealistic"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    #endregion

    #region Recraft Models

    public static ModelDefinition RecraftV3(string id, string featureFlag, string? endpointOverride = null) =>
        new()
        {
            Id = id,
            Title = "Recraft V3",
            Description = "High-quality image generation with excellent style control",
            Author = "Recraft",
            StandardWidth = 1024,
            StandardHeight = 1024,
            PreviewImagePath = "Images/ModelPreviews/Recraft/recraft-v3.jpg",
            Date = "2024",
            UsageHint = "Great for stylized and artistic images",
            Tags = ["recraft", "artistic", "style-control"],
            FeatureFlag = featureFlag,
            EndpointOverride = endpointOverride ?? ""
        };

    #endregion
}
