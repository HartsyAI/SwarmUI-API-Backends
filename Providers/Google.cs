using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Google API provider for Imagen and Gemini image models.</summary>
public sealed class GoogleProvider : IProviderSource
{
    public static GoogleProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("google_api")
        .WithName("Google")
        .WithModelPrefix("Google")
        .WithBaseUrl("https://generativelanguage.googleapis.com/v1beta/models")
        .WithAuthHeader("Bearer")
        .WithModelClass("google_api", "Google Imagen")
        .AddModels(Models)
        .Build();

    private static IEnumerable<ModelDefinition> Models =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("imagen-3.0-generate-002")
            .WithTitle("Imagen 3.0")
            .WithDescription("Google's Imagen 3.0 image generation model")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Google/imagen-3.0.jpg")
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
            .WithPreviewImage("Images/ModelPreviews/Google/gemini-2.0-flash.jpg")
            .WithDate("2024")
            .WithUsageHint("Fast multimodal image generation")
            .WithTags("gemini", "google", "multimodal")
            .WithFeatureFlag("google_gemini_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-2.5-flash-image")
            .WithTitle("Gemini 2.5 Flash Image (Nano Banana)")
            .WithDescription("Gemini 2.5 Flash with native image generation (Nano Banana). Optimized for speed and efficiency with high-volume, low-latency tasks. Generates images at 1024px resolution.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Google/gemini-2.5-flash-image.jpg")
            .WithDate("2025")
            .WithUsageHint("Fast, efficient image generation. Best for high-volume workflows.")
            .WithTags("gemini", "google", "nano-banana", "multimodal", "generative")
            .WithFeatureFlag("google_gemini_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-3.1-flash-image-preview")
            .WithTitle("Gemini 3.1 Flash Image (Nano Banana 2)")
            .WithDescription("Gemini 3.1 Flash Image Preview (Nano Banana 2). Best all-around image generation model with excellent intelligence-to-cost balance. Supports 512px to 4K resolution, up to 14 reference images, Google Search grounding, Google Image Search, and thinking mode.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Google/gemini-3.1-flash-image.jpg")
            .WithDate("2026")
            .WithUsageHint("Best all-around Google image model. Supports 512px-4K, thinking, search grounding, and 14 reference images.")
            .WithTags("gemini", "google", "nano-banana-2", "multimodal", "generative", "thinking", "4k")
            .WithFeatureFlag("google_gemini3_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-3-pro-image-preview")
            .WithTitle("Gemini 3 Pro Image (Nano Banana Pro)")
            .WithDescription("Gemini 3 Pro Image Preview (Nano Banana Pro). Designed for professional asset production with advanced reasoning ('Thinking'). Excels at complex multi-turn creation, high-fidelity text rendering, and can generate up to 4K resolution images.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithPreviewImage("Images/ModelPreviews/Google/gemini-3-pro-image.jpg")
            .WithDate("2026")
            .WithUsageHint("Professional asset production. Advanced reasoning, high-fidelity text, up to 4K resolution.")
            .WithTags("gemini", "google", "nano-banana-pro", "multimodal", "generative", "thinking", "4k", "professional")
            .WithFeatureFlag("google_gemini3_params")
            .Build()
    ];
}
