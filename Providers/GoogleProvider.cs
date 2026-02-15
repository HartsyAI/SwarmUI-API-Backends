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
            .Build()
    ];
}
