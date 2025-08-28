using SwarmUI.Accounts;
using SwarmUI.Core;
using SwarmUI.Utils;
using SwarmUI.WebAPI;
using Hartsy.Extensions.APIBackends.Backends;
using SwarmUI.Text2Image;
using Microsoft.AspNetCore.Html;
using Hartsy.Extensions.APIBackends.Models;

namespace Hartsy.Extensions.APIBackends;

/// <summary>Permissions for the APIBackends extension.</summary>
public static class APIBackendsPermissions
{
    public static readonly PermInfoGroup APIBackendsPermGroup = new("APIBackends",
        "Permissions related to API-based image generation backends.");

    public static readonly PermInfo PermUseOpenAI = Permissions.Register(new("use_openai", "Use OpenAI APIs",
        "Allows using OpenAI's DALL-E models for image generation.",
        PermissionDefault.POWERUSERS, APIBackendsPermGroup));

    public static readonly PermInfo PermUseIdeogram = Permissions.Register(new("use_ideogram", "Use Ideogram API",
        "Allows using Ideogram's API for image generation.",
        PermissionDefault.POWERUSERS, APIBackendsPermGroup));

    public static readonly PermInfo PermUseBlackForest = Permissions.Register(new("use_blackforest", "Use Black Forest Labs API",
        "Allows using Black Forest Labs' Flux model APIs for image generation.",
        PermissionDefault.POWERUSERS, APIBackendsPermGroup));

    public static readonly PermInfo PermUseGrok = Permissions.Register(new("use_grok", "Use Grok API",
        "Allows using Grok's API for image generation.",
        PermissionDefault.POWERUSERS, APIBackendsPermGroup));
    public static readonly PermInfo PermUseGoogleImagen = Permissions.Register(new("use_google_imagen", "Use Google Imagen API",
        "Allows using Google's Imagen model API for image generation.",
        PermissionDefault.POWERUSERS, APIBackendsPermGroup));
}

/// <summary>Extension that adds support for various API-based image generation services.</summary>
public class SwarmUIAPIBackends : Extension
{
    /// <summary>API provider initialization instance.</summary>
    private static APIProviderInit _providerInit;

    // Parameter Groups for each API service
    public static T2IParamGroup OpenAIParamGroup;
    public static T2IParamGroup DallE2Group;
    public static T2IParamGroup DallE3Group;
    public static T2IParamGroup GPTImage1Group;
    public static T2IParamGroup GrokParamGroup;

    public static T2IParamGroup Grok2ImageGroup;
    public static T2IParamGroup IdeogramParamGroup;
    public static T2IParamGroup IdeogramGeneralGroup;
    public static T2IParamGroup IdeogramAdvancedGroup;

    public static T2IParamGroup BlackForestGroup;
    public static T2IParamGroup BlackForestGeneralGroup;
    public static T2IParamGroup BlackForestAdvancedGroup;

    // OpenAI Parameters
    public static T2IRegisteredParam<string> SizeParam_OpenAI;
    public static T2IRegisteredParam<string> QualityParam_OpenAI;
    public static T2IRegisteredParam<string> StyleParam_OpenAI;
    public static T2IRegisteredParam<string> ResponseFormatParam_OpenAI;

    // GPT Image 1 Parameters
    public static T2IRegisteredParam<string> QualityParam_GPTImage1;
    public static T2IRegisteredParam<string> BackgroundParam_GPTImage1;
    public static T2IRegisteredParam<string> ModerationParam_GPTImage1;
    public static T2IRegisteredParam<string> OutputFormatParam_GPTImage1;
    public static T2IRegisteredParam<int> OutputCompressionParam_GPTImage1;
    // Ideogram Parameters
    public static T2IRegisteredParam<string> StyleParam_Ideogram;
    public static T2IRegisteredParam<string> MagicPromptParam_Ideogram;
    public static T2IRegisteredParam<int> SeedParam_Ideogram;
    public static T2IRegisteredParam<string> ColorPaletteParam_Ideogram;

    // Black Forest Labs API Parameters
    public static T2IRegisteredParam<double> GuidanceParam_BlackForest;
    public static T2IRegisteredParam<int> SafetyParam_BlackForest;
    public static T2IRegisteredParam<double> IntervalParam_BlackForest;
    public static T2IRegisteredParam<bool> PromptEnhanceParam_BlackForest;
    public static T2IRegisteredParam<string> OutputFormatParam_BlackForest;
    public static T2IRegisteredParam<bool> RawModeParam_BlackForest;
    public static T2IRegisteredParam<Image> ImagePromptParam_BlackForest;
    public static T2IRegisteredParam<double> ImagePromptStrengthParam_BlackForest;

    public override void OnPreInit()
    {
        ScriptFiles.Add("Assets/api-backends.js");
    }

    public override void OnInit()
    {
        // Initialize Parameter Groups for all APIs
        OpenAIParamGroup = new("DALL-E API", Toggles: false, Open: true, OrderPriority: 10);
        DallE2Group = new("DALL-E 2 Settings", Toggles: false, Open: true, OrderPriority: 10,
            Description: "Parameters specific to OpenAI's DALL-E 2 model generation.");
        DallE3Group = new("DALL-E 3 Settings", Toggles: false, Open: true, OrderPriority: 11,
            Description: "Parameters specific to OpenAI's DALL-E 3 model, featuring enhanced quality and style options.");
        GPTImage1Group = new("GPT Image 1 Settings", Toggles: false, Open: true, OrderPriority: 12,
            Description: "Able to generate images with stronger instruction following, contextual awareness, and world knowledge.");

        IdeogramParamGroup = new("Ideogram API", Toggles: false, Open: true, OrderPriority: 20);
        IdeogramGeneralGroup = new("Ideogram Basic Settings", Toggles: false, Open: true, OrderPriority: 20,
            Description: "Core parameters for Ideogram image generation.");
        IdeogramAdvancedGroup = new("Ideogram Advanced Settings", Toggles: true, Open: false, OrderPriority: 21,
            Description: "Additional options for fine-tuning Ideogram generations.");

        GrokParamGroup = new("Grok API", Toggles: false, Open: true, OrderPriority: 30,
            Description: "API access to Grok's image generation models.");

        Grok2ImageGroup = new("Grok 2 Image Settings", Toggles: false, Open: true, OrderPriority: 37,
            Description: "Parameters specific to Grok's 2 Image model generation.");

        BlackForestGroup = new("Black Forest Labs API", Toggles: false, Open: true, OrderPriority: 40,
            Description: "API access to Black Forest Labs' high quality Flux image generation models.");
        BlackForestGeneralGroup = new("Flux Core Settings", Toggles: false, Open: true, OrderPriority: 40,
            Description: "Core parameters for Flux image generation.\nFlux models excel at high-quality image generation with strong artistic control.");
        BlackForestAdvancedGroup = new("Flux Advanced Settings", Toggles: true, Open: false, OrderPriority: 41,
            Description: "Additional options for fine-tuning Flux generations and output processing.");

        // Core Parameters for both models
        SizeParam_OpenAI = T2IParamTypes.Register<string>(new("Output Resolution",
            "Controls the dimensions of the generated image.\n" +
            "DALL-E 2: 256x256, 512x512, or 1024x1024\n" +
            "DALL-E 3: 1024x1024, 1792x1024, or 1024x1792\n" +
            "GPT Image 1: auto, 1024x1024, 1536x1024, or 1024x1536",
            "1024x1024", GetValues: model =>
            {
                if (model.ID.Contains("dall-e-2"))
                    return ["256x256", "512x512", "1024x1024"];
                else if (model.ID.Contains("gpt-image-1"))
                    return ["auto///Auto (Recommended)", "1024x1024///Square", "1536x1024///Landscape", "1024x1536///Portrait"];
                else
                    return ["1024x1024", "1792x1024", "1024x1792"];
            },
            OrderPriority: -10, ViewType: ParamViewType.POT_SLIDER,
            Group: DallE3Group, FeatureFlag: "openai_api"));

        QualityParam_OpenAI = T2IParamTypes.Register<string>(new("Generation Quality",
            "Controls the level of detail and consistency in DALL-E 3 images.\n" +
            "'Standard' - Balanced quality suitable for most uses, generates faster\n" +
            "'HD' - Enhanced detail and better consistency across the entire image, takes longer to generate\n" +
            "Note: HD mode costs more credits but can be worth it for complex scenes or when fine details matter.",
            "standard", GetValues: _ => ["standard///Standard (Faster)", "hd///HD (Higher Quality)"],
            OrderPriority: -8, Group: DallE3Group, FeatureFlag: "openai_api"));

        StyleParam_OpenAI = T2IParamTypes.Register<string>(new("Visual Style",
            "Determines the artistic approach for DALL-E 3 generations:\n" +
            "'Vivid' - Creates hyper-realistic, dramatic images with enhanced contrast and details\n" +
            "'Natural' - Produces more photorealistic results with subtle lighting and natural composition\n" +
            "Choose 'Vivid' for artistic or commercial work, 'Natural' for documentary or product photos.",
            "vivid", GetValues: _ => ["vivid///Vivid (Dramatic)", "natural///Natural (Realistic)"],
            OrderPriority: -7, Group: DallE3Group, FeatureFlag: "openai_api"));

        ResponseFormatParam_OpenAI = T2IParamTypes.Register<string>(new("Response Format",
            "Determines how the generated image is returned from the API.\n" +
            "'URL' - Returns a temporary URL valid for 60 minutes\n" +
            "'Base64' - Returns the image data directly encoded in base64\n" +
            "Base64 is preferred for immediate use, URLs for deferred processing.",
            "b64_json", GetValues: _ => ["url", "b64_json"], OrderPriority: -6,
            Group: DallE3Group, FeatureFlag: "openai_api"));

        // gpt-image-1 parameters
        QualityParam_GPTImage1 = T2IParamTypes.Register<string>(new("Quality",
            "Controls the quality of the generated image.\n" +
            "'Auto' - Automatically selects the best quality for the given model\n" +
            "'High' - Highest quality with maximum detail and clarity\n" +
            "'Medium' - Balanced quality suitable for most uses\n" +
            "'Low' - Faster generation with reduced quality\n" +
            "Higher quality levels may increase generation time and cost.",
            "auto", GetValues: _ => ["auto///Auto (Recommended)", "high///High Quality", "medium///Medium Quality", "low///Low Quality"],
            OrderPriority: -8, Group: GPTImage1Group, FeatureFlag: "gpt-image-1_params"));

        BackgroundParam_GPTImage1 = T2IParamTypes.Register<string>(new("Background",
            "Controls the background transparency of the generated image.\n" +
            "'Auto' - Automatically determines the best background for the image\n" +
            "'Transparent' - Creates a transparent background (requires PNG or WebP format)\n" +
            "'Opaque' - Creates a solid background with no transparency\n" +
            "Note: Transparent backgrounds require PNG or WebP output format.",
            "auto", GetValues: _ => ["auto///Auto (Recommended)", "transparent///Transparent", "opaque///Opaque"],
            OrderPriority: -7, Group: GPTImage1Group, FeatureFlag: "gpt-image-1_params"));

        ModerationParam_GPTImage1 = T2IParamTypes.Register<string>(new("Content Moderation",
            "Controls the content moderation level for generated images.\n" +
            "'Auto' - Uses default moderation settings\n" +
            "'Low' - Less restrictive content filtering (use with caution)\n" +
            "Auto setting provides balanced content safety.",
            "auto", GetValues: _ => ["auto///Auto (Recommended)", "low///Low Filtering"],
            OrderPriority: -6, Group: GPTImage1Group, FeatureFlag: "gpt-image-1_params"));

        OutputFormatParam_GPTImage1 = T2IParamTypes.Register<string>(new("Image Output Format",
            "The format for the generated image.\n" +
            "'PNG' - Lossless format, supports transparency\n" +
            "'JPEG' - Compressed format, smaller file size\n" +
            "'WebP' - Modern format, good compression and transparency support\n" +
            "PNG is recommended for images with transparency.",
            "png", GetValues: _ => ["png///PNG (Lossless)", "jpeg///JPEG (Compressed)", "webp///WebP (Modern)"],
            OrderPriority: -5, Group: GPTImage1Group, FeatureFlag: "gpt-image-1_params"));

        OutputCompressionParam_GPTImage1 = T2IParamTypes.Register<int>(new("Output Compression",
            "The compression level (0-100%) for JPEG and WebP formats.\n" +
            "Higher values mean better quality but larger file sizes.\n" +
            "Only applies to JPEG and WebP output formats.\n" +
            "Default is 100% (maximum quality).",
            "100", Min: 0, Max: 100, ViewType: ParamViewType.SLIDER,
            OrderPriority: -4, Group: GPTImage1Group, FeatureFlag: "gpt-image-1_params"));

        // Ideogram Parameters
        StyleParam_Ideogram = T2IParamTypes.Register<string>(new("Generation Style",
            "Determines the artistic approach for image creation (V2+ models only):\n" +
            "'Auto' - Automatically selects style based on prompt\n" +
            "'General' - Balanced style suitable for most prompts\n" +
            "'Realistic' - Photorealistic style with natural lighting and details\n" +
            "'Design' - Clean, graphic design aesthetic\n" +
            "'Render_3D' - 3D rendered style with depth and lighting\n" +
            "'Anime' - Anime/manga inspired artistic style",
            "AUTO",
            GetValues: _ => ["AUTO///Auto (Recommended, V2+)", "GENERAL///General Purpose (V2+)",
                "REALISTIC///Photorealistic (V2+)", "DESIGN///Graphic Design (V2+)",
                "RENDER_3D///3D Rendered (V2+)", "ANIME///Anime Style (V2+)"],
            OrderPriority: -10, Group: IdeogramGeneralGroup, FeatureFlag: "ideogram_api"
            ));

        // Advanced Parameters
        MagicPromptParam_Ideogram = T2IParamTypes.Register<string>(new("Magic Prompt Enhancement",
            "Controls Ideogram's prompt optimization system:\n" +
            "'Auto' - Let the system decide when to enhance prompts\n" +
            "'On' - Always enhance prompts for better results\n" +
            "'Off' - Use exact prompts without enhancement\n" +
            "Magic Prompt can help improve results but may deviate from exact prompt wording.", "AUTO",
            GetValues: _ => ["AUTO///Auto (Recommended)", "ON///Always Enhance", "OFF///Exact Prompts"],
            OrderPriority: -7, Group: IdeogramAdvancedGroup, FeatureFlag: "ideogram_api"));

        SeedParam_Ideogram = T2IParamTypes.Register<int>(new("Generation Seed",
            "Set a specific seed for reproducible results.\n" +
            "Same seed + same settings = same image.\n" +
            "Useful for iterating on specific images or sharing exact settings.", "-1",
            Min: -1, Max: 2147483647, ViewType: ParamViewType.SEED, OrderPriority: -4,
            Group: IdeogramAdvancedGroup, FeatureFlag: "ideogram_api"));

        ColorPaletteParam_Ideogram = T2IParamTypes.Register<string>(new("Color Theme",
            "Apply a predefined color palette to influence image colors (V2 & V2_TURBO only):\n" +
            "'None' - No color preference\n" +
            "'Ember' - Warm, fiery tones\n" +
            "'Fresh' - Bright, clean colors\n" +
            "'Jungle' - Natural, organic greens\n" +
            "'Magic' - Mystical purples and blues\n" +
            "'Melon' - Sweet pastels\n" +
            "'Mosaic' - Vibrant mix\n" +
            "'Pastel' - Soft, muted tones\n" +
            "'Ultramarine' - Ocean blues", "None",
            GetValues: _ => [
                "None///No Color Theme",
                "EMBER///Ember - Warm Tones",
                "FRESH///Fresh - Bright & Clean",
                "JUNGLE///Jungle - Natural Greens",
                "MAGIC///Magic - Mystical",
                "MELON///Melon - Sweet Pastels",
                "MOSAIC///Mosaic - Vibrant Mix",
                "PASTEL///Pastel - Soft Tones",
                "ULTRAMARINE///Ultramarine - Ocean Blues"
            ],
            OrderPriority: -3, Group: IdeogramAdvancedGroup, FeatureFlag: "ideogram_api"));

        // Black Forest Labs API Parameters
        GuidanceParam_BlackForest = T2IParamTypes.Register<double>(new("Prompt Guidance",
            "Controls how strictly the generation follows the prompt.\n" +
            "Flux Pro/1.1: Default 2.5 for balanced results\n" +
            "Flux Dev: Default 3.0 for increased prompt adherence\n" +
            "Lower values (1.5-2.5): More creative but less controlled\n" +
            "Higher values (3.0-5.0): Stricter prompt following but may reduce realism.", "2.5",
            Min: 1.5, Max: 5.0, Step: 0.1, ViewType: ParamViewType.SLIDER, OrderPriority: -6,
            Group: BlackForestGeneralGroup, FeatureFlag: "bfl_api"));

        SafetyParam_BlackForest = T2IParamTypes.Register<int>(new("Safety Filter Level",
            "Controls content filtering strictness for both input and output.\n" +
            "0: Most restrictive - blocks most potentially unsafe content\n" +
            "6: Least restrictive - allows most content through\n" +
            "Default 2 provides balanced filtering suitable for most use cases.", "2", Min: 0, Max: 6,
            ViewType: ParamViewType.SLIDER, OrderPriority: -5, Group: BlackForestGeneralGroup, FeatureFlag: "bfl_api"));

        IntervalParam_BlackForest = T2IParamTypes.Register<double>(new("Guidance Interval",
            "Fine-tunes how guidance is applied during generation.\n" +
            "Only available in Flux Pro and Pro 1.1 models.\n" +
            "Lower values (1.0-2.0): More precise, controlled results\n" +
            "Higher values (2.0-4.0): More creative freedom and variation\n" +
            "Default 2.0 provides good balance of control and creativity.", "2.0", Min: 1.0, Max: 4.0, Step: 0.1,
            ViewType: ParamViewType.SLIDER, OrderPriority: -5, Group: BlackForestAdvancedGroup,
            FeatureFlag: "bfl_api"));

        PromptEnhanceParam_BlackForest = T2IParamTypes.Register<bool>(new("Prompt Enhancement",
            "Enables Flux's automatic prompt enhancement system.\n" +
            "When enabled: Automatically expands prompts with additional details\n" +
            "Can help achieve more artistic results but may reduce prompt precision\n" +
            "Recommended for creative/artistic work, disable for exact prompt following.", "false",
            OrderPriority: -4, IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl_api"));

        RawModeParam_BlackForest = T2IParamTypes.Register<bool>(new("Raw Mode",
            "Enables raw generation mode in Flux Pro 1.1.\n" +
            "When enabled: Generates less processed, more natural-looking images\n" +
            "Raw mode can produce more authentic results but may be less polished\n" +
            "Only available in Flux Pro 1.1 ultra mode.", "false", OrderPriority: -3,
            IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl_api"));

        ImagePromptParam_BlackForest = T2IParamTypes.Register<Image>(new("Image Prompt",
            "Optional image to use as a starting point or reference.\n" +
            "Acts as a visual guide for the generation process.\n" +
            "Useful for variations, style matching, or guided compositions.\n" +
            "Use with Image Prompt Strength to control influence.", null, OrderPriority: -2,
            IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl_api"));

        ImagePromptStrengthParam_BlackForest = T2IParamTypes.Register<double>(new("Image Prompt Strength",
            "Controls how much the Image Prompt influences the generation.\n" +
            "0.0: Ignore image prompt completely\n" +
            "1.0: Follow image prompt very closely\n" +
            "Default 0.1 provides subtle guidance while allowing creativity.",
            "0.1", Min: 0.0, Max: 1.0, Step: 0.05, ViewType: ParamViewType.SLIDER,
            OrderPriority: -1, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl_api"));

        OutputFormatParam_BlackForest = T2IParamTypes.Register<string>(new("Output Format",
            "Choose the file format for saving generated images:\n" +
            "JPEG: Smaller files, slight quality loss, good for sharing\n" +
            "PNG: Lossless quality, larger files, best for editing.",
            "jpeg", GetValues: _ => ["jpeg///JPEG (Smaller)", "png///PNG (Lossless)"],
            OrderPriority: 0, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl_api"));

        // Register all API feature flags
        T2IEngine.DisregardedFeatureFlags.Add("openai_api");
        T2IEngine.DisregardedFeatureFlags.Add("ideogram_api");
        T2IEngine.DisregardedFeatureFlags.Add("bfl_api");
        T2IEngine.DisregardedFeatureFlags.Add("grok_api");
        T2IEngine.DisregardedFeatureFlags.Add("google_imagen_api");

        // Register model-specific feature flags
        T2IEngine.DisregardedFeatureFlags.Add("dalle2_params");
        T2IEngine.DisregardedFeatureFlags.Add("dalle3_params");
        T2IEngine.DisregardedFeatureFlags.Add("gpt-image-1_params");
        T2IEngine.DisregardedFeatureFlags.Add("ideogram_v1_params");
        T2IEngine.DisregardedFeatureFlags.Add("ideogram_v2_params");
        T2IEngine.DisregardedFeatureFlags.Add("ideogram_v3_params");
        T2IEngine.DisregardedFeatureFlags.Add("flux_ultra_params");
        T2IEngine.DisregardedFeatureFlags.Add("flux_pro_params");
        T2IEngine.DisregardedFeatureFlags.Add("flux_dev_params");
        T2IEngine.DisregardedFeatureFlags.Add("flux_kontext_pro_params");
        T2IEngine.DisregardedFeatureFlags.Add("flux_kontext_max_params");
        T2IEngine.DisregardedFeatureFlags.Add("grok_2_image_params");

        // Hard to remove parameters from the global registry, so we keep them in memory
        // Basic feature flags for all API backends - disable anything not needed
        T2IEngine.DisregardedFeatureFlags.Add("sampling");
        T2IEngine.DisregardedFeatureFlags.Add("zero_negative");
        T2IEngine.DisregardedFeatureFlags.Add("refiners");
        T2IEngine.DisregardedFeatureFlags.Add("controlnet");
        T2IEngine.DisregardedFeatureFlags.Add("variation_seed");
        T2IEngine.DisregardedFeatureFlags.Add("video");
        T2IEngine.DisregardedFeatureFlags.Add("autowebui");
        T2IEngine.DisregardedFeatureFlags.Add("comfyui");
        T2IEngine.DisregardedFeatureFlags.Add("frameinterps");
        T2IEngine.DisregardedFeatureFlags.Add("ipadapter");
        T2IEngine.DisregardedFeatureFlags.Add("sdxl");
        T2IEngine.DisregardedFeatureFlags.Add("dynamic_thresholding");
        T2IEngine.DisregardedFeatureFlags.Add("cascade");
        T2IEngine.DisregardedFeatureFlags.Add("sd3");
        T2IEngine.DisregardedFeatureFlags.Add("flux-dev");
        T2IEngine.DisregardedFeatureFlags.Add("seamless");
        T2IEngine.DisregardedFeatureFlags.Add("freeu");
        T2IEngine.DisregardedFeatureFlags.Add("teacache");
        T2IEngine.DisregardedFeatureFlags.Add("text2video");
        T2IEngine.DisregardedFeatureFlags.Add("yolov8");
        T2IEngine.DisregardedFeatureFlags.Add("aitemplate");

        // Register the dynamic API backend type
        Program.Backends.RegisterBackendType<DynamicAPIBackend>("dynamic_api_backend", "3rd Party Paid API Backends",
            "Generate images using various API services (OpenAI, Ideogram, Black Forest Labs)", true);
        // All key types must be added to the accepted list first
        string[] keyTypes = ["openai_api", "bfl_api", "ideogram_api", "grok_api", "google_imagen_api"];
        foreach (string keyType in keyTypes)
        {
            BasicAPIFeatures.AcceptedAPIKeyTypes.Add(keyType);
        }
        _providerInit = new APIProviderInit();
        RegisterApiModelsWithGlobalRegistry();
        // Register API Key tables for each backend - safely handle if already registered
        RegisterApiKeyIfNeeded("openai_api", "openai", "OpenAI (ChatGPT)", "https://platform.openai.com/api-keys",
            new HtmlString("To use OpenAI models in SwarmUI (via Hartsy extensions), you must set your OpenAI API key."));
        RegisterApiKeyIfNeeded("bfl_api", "black_forest", "Black Forest Labs (FLUX)", "https://dashboard.bfl.ai/",
            new HtmlString("To use Black Forest in SwarmUI (via Hartsy extensions), you must set your Black Forest API key."));
        RegisterApiKeyIfNeeded("ideogram_api", "ideogram", "Ideogram", "https://developer.ideogram.ai/ideogram-api/api-setup",
            new HtmlString("To use Ideogram in SwarmUI (via Hartsy extensions), you must set your Ideogram API key."));
        RegisterApiKeyIfNeeded("grok_api", "grok", "Grok", "https://accounts.x.ai/sign-up?redirect=grok-com",
            new HtmlString("To use Grok in SwarmUI (via Hartsy extensions), you must set your Grok API key."));
        RegisterApiKeyIfNeeded("google_imagen_api", "google_imagen", "Google Imagen", "https://ai.google.dev/gemini-api/docs/api-key",
            new HtmlString("To use Google Imagen in SwarmUI (via Hartsy extensions), you must set your Google Gemini API key."));
        Logs.Init("Hartsy's APIBackends extension V1.0 has successfully started.");
    }

    /// <summary>Safely registers an API key if it's not already registered</summary>
    private static void RegisterApiKeyIfNeeded(string keyType, string jsPrefix, string title, string createLink, HtmlString infoHtml)
    {
        try
        {
            if (!UserUpstreamApiKeys.KeysByType.ContainsKey(keyType))
            {
                UserUpstreamApiKeys.Register(new(keyType, jsPrefix, title, createLink, infoHtml));
                Logs.Verbose($"Registered API key type: {keyType}");
            }
            else
            {
                Logs.Verbose($"API key type '{keyType}' already registered, skipping registration");
            }
        }
        catch (Exception ex)
        {
            Logs.Warning($"Failed to register API key type '{keyType}': {ex.Message}");
        }
    }

    /// <summary>Registers models from API providers to the global SwarmUI model registry, making them visible in the Models tab.
    /// This ensures API models can be browsed, selected, and modified like standard local models.</summary>
    public static void RegisterApiModelsWithGlobalRegistry()
    {
        foreach (APIProviderMetadata provider in _providerInit.Providers.Values)
        {
            foreach (KeyValuePair<string, T2IModel> entry in provider.Models)
            {
                string modelName = entry.Key;
                T2IModel model = entry.Value;
                // Skip if model already exists (avoid duplicates)
                if (Program.MainSDModels.Models.ContainsKey(modelName))
                {
                    continue;
                }
                // Create a copy of the model for the global registry
                T2IModel globalModel = new(Program.MainSDModels, null, model.RawFilePath, modelName)
                {
                    Title = model.Title,
                    Description = model.Description,
                    PreviewImage = model.PreviewImage,
                    ModelClass = model.ModelClass,
                    StandardWidth = model.StandardWidth,
                    StandardHeight = model.StandardHeight,
                    Metadata = model.Metadata,
                    IsSupportedModelType = true
                };
                // Register the model with the global registry so it shows up in the Models tab
                Program.MainSDModels.Models[modelName] = globalModel;
                Logs.Verbose($"Registered API model in global registry: {modelName}");
            }
        }
    }
}
