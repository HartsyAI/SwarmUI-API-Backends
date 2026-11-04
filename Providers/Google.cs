using System.Collections.Generic;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends.Providers;

/// <summary>Google Gemini API provider for image generation models.</summary>
public sealed class GoogleProvider : IProviderSource
{
    public static GoogleProvider Instance { get; } = new();

    public ProviderDefinition GetProvider() => ProviderDefinitionBuilder.Create()
        .WithId("google_api")
        .WithName("Google")
        .WithModelPrefix("Google")
        .WithBaseUrl("https://generativelanguage.googleapis.com/v1beta/models")
        .WithAuthHeader("Bearer")
        .WithModelClass("google_api", "Google Gemini")
        .AddModels(Models)
        .Build();

    private static IEnumerable<ModelDefinition> Models =>
    [
        ModelDefinitionBuilder.Create()
            .WithId("gemini-2.5-flash-image")
            .WithTitle("Gemini 2.5 Flash Image (Nano Banana)")
            .WithDescription("Gemini 2.5 Flash with native image generation (Nano Banana). Legacy model, scheduled to shut down March 15, 2027.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithDate("2025")
            .WithUsageHint("Fast, efficient image generation. Legacy: migrate to Nano Banana 2 Lite before March 15, 2027.")
            .WithTags("gemini", "google", "nano-banana", "multimodal", "generative")
            .WithFeatureFlag("google_gemini_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-3.1-flash-image")
            .WithTitle("Gemini 3.1 Flash Image (Nano Banana 2)")
            .WithDescription("Gemini 3.1 Flash Image (Nano Banana 2). Supports 512px to 4K resolution. Deprecated by Nano Banana 2.1; no shutdown date announced.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithDate("2026")
            .WithUsageHint("Previous Nano Banana 2 model. Nano Banana 2.1 is recommended for new work.")
            .WithTags("gemini", "google", "nano-banana-2", "multimodal", "generative", "thinking", "4k")
            .WithFeatureFlag("google_gemini3_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-3.1-flash-lite-image")
            .WithTitle("Gemini 3.1 Flash Lite Image (Nano Banana 2 Lite)")
            .WithDescription("Gemini 3.1 Flash Lite Image (Nano Banana 2 Lite). The fastest and lowest-cost Gemini image model. Outputs 1K only and is not optimized for multiple reference images or multi-turn editing.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithDate("2026")
            .WithUsageHint("Fastest, lowest-cost Google image model. 1K output only.")
            .WithTags("gemini", "google", "nano-banana-2-lite", "multimodal", "generative", "thinking")
            .WithFeatureFlag("google_gemini3_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-nano-banana-2.1")
            .WithTitle("Gemini Nano Banana 2.1")
            .WithDescription("Gemini Nano Banana 2.1, the update to Nano Banana 2. Configurable thinking, 1K to 4K resolution, and up to 14 reference images.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithDate("2026")
            .WithUsageHint("Recommended Google image model for new work. Up to 4K output and 14 reference images.")
            .WithTags("gemini", "google", "nano-banana-2.1", "multimodal", "generative", "thinking", "4k")
            .WithFeatureFlag("google_gemini3_params")
            .Build(),

        ModelDefinitionBuilder.Create()
            .WithId("gemini-3-pro-image")
            .WithTitle("Gemini 3 Pro Image (Nano Banana Pro)")
            .WithDescription("Gemini 3 Pro Image (Nano Banana Pro). Advanced reasoning ('Thinking') for professional asset production, with 1K, 2K and 4K output.")
            .WithAuthor("Google")
            .WithDimensions(1024, 1024)
            .WithDate("2025")
            .WithUsageHint("Professional asset production. Advanced reasoning, high-fidelity text, up to 4K resolution.")
            .WithTags("gemini", "google", "nano-banana-pro", "multimodal", "generative", "thinking", "4k", "professional")
            .WithFeatureFlag("google_gemini3_params")
            .Build()
    ];
}
