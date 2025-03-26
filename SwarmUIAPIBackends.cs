using SwarmUI.Accounts;
using SwarmUI.Core;
using SwarmUI.Utils;
using SwarmUI.WebAPI;
using Hartsy.Extensions.APIBackends.Backends;
using SwarmUI.Text2Image;
using Microsoft.AspNetCore.Html;

namespace Hartsy.Extensions.APIBackends;

/// <summary>Permissions for the APIBackends extension.</summary>
public static class APIBackendsPermissions
{
    public static readonly PermInfoGroup APIBackendsPermGroup = new("APIBackends",
        "Permissions related to API-based image generation backends.");

    public static readonly PermInfo PermUseOpenAI = Permissions.Register(new("use_openai", "Use OpenAI APIs",
        "Allows using OpenAI's DALL-E models for image generation.",
        PermissionDefault.GUEST, APIBackendsPermGroup));

    public static readonly PermInfo PermUseIdeogram = Permissions.Register(new("use_ideogram", "Use Ideogram API",
        "Allows using Ideogram's API for image generation.",
        PermissionDefault.GUEST, APIBackendsPermGroup));

    public static readonly PermInfo PermUseBlackForest = Permissions.Register(new("use_blackforest", "Use Black Forest Labs API",
        "Allows using Black Forest Labs' Flux model APIs for image generation.",
        PermissionDefault.GUEST, APIBackendsPermGroup));
}

/// <summary>Extension that adds support for various API-based image generation services.</summary>
public class SwarmUIAPIBackends : Extension
{
    // Parameter Groups for each API service
    public static T2IParamGroup OpenAIParamGroup;
    public static T2IParamGroup DallE2Group;
    public static T2IParamGroup DallE3Group;

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

        IdeogramParamGroup = new("Ideogram API", Toggles: false, Open: true, OrderPriority: 20);
        IdeogramGeneralGroup = new("Ideogram Basic Settings", Toggles: false, Open: true, OrderPriority: 20,
            Description: "Core parameters for Ideogram image generation.");
        IdeogramAdvancedGroup = new("Ideogram Advanced Settings", Toggles: true, Open: false, OrderPriority: 21,
            Description: "Additional options for fine-tuning Ideogram generations.");

        BlackForestGroup = new("Black Forest Labs API", Toggles: false, Open: true, OrderPriority: 40,
            Description: "API access to Black Forest Labs' high quality Flux image generation models.");
        BlackForestGeneralGroup = new("Flux Core Settings", Toggles: false, Open: true, OrderPriority: 40,
            Description: "Core parameters for Flux image generation.\nFlux models excel at high-quality image generation with strong artistic control.");
        BlackForestAdvancedGroup = new("Flux Advanced Settings", Toggles: true, Open: false, OrderPriority: 41,
            Description: "Additional options for fine-tuning Flux generations and output processing.");

        // Core Parameters for both models
        SizeParam_OpenAI = T2IParamTypes.Register<string>(new("Output Resolution",
            "Controls the dimensions of the generated image.\n" +
            "DALL-E 2: Smaller sizes (256x256, 512x512) generate faster but with less detail.\n" +
            "DALL-E 3: Larger sizes (1792x1024, 1024x1792) better for wide or tall compositions, 1024x1024 for balanced images.",
            "1024x1024", GetValues: model => model.ID.Contains("dall-e-2") ? ["256x256", "512x512", "1024x1024"] : ["1024x1024", 
            "1792x1024", "1024x1792"], OrderPriority: -10, ViewType: ParamViewType.DROPDOWN, 
            Group: DallE3Group, FeatureFlag: "openai-api"));

        // DALL-E 3 Specific Parameters
        QualityParam_OpenAI = T2IParamTypes.Register<string>(new("Generation Quality",
            "Controls the level of detail and consistency in DALL-E 3 images.\n" +
            "'Standard' - Balanced quality suitable for most uses, generates faster\n" +
            "'HD' - Enhanced detail and better consistency across the entire image, takes longer to generate\n" +
            "Note: HD mode costs more credits but can be worth it for complex scenes or when fine details matter.",
            "standard", GetValues: _ => ["standard///Standard (Faster)", "hd///HD (Higher Quality)"],
            OrderPriority: -8, Group: DallE3Group, FeatureFlag: "openai-api"));

        StyleParam_OpenAI = T2IParamTypes.Register<string>(new("Visual Style",
            "Determines the artistic approach for DALL-E 3 generations:\n" +
            "'Vivid' - Creates hyper-realistic, dramatic images with enhanced contrast and details\n" +
            "'Natural' - Produces more photorealistic results with subtle lighting and natural composition\n" +
            "Choose 'Vivid' for artistic or commercial work, 'Natural' for documentary or product photos.",
            "vivid", GetValues: _ => ["vivid///Vivid (Dramatic)", "natural///Natural (Realistic)"],
            OrderPriority: -7, Group: DallE3Group, FeatureFlag: "openai-api"));

        ResponseFormatParam_OpenAI = T2IParamTypes.Register<string>(new("Response Format",
            "Determines how the generated image is returned from the API.\n" +
            "'URL' - Returns a temporary URL valid for 60 minutes\n" +
            "'Base64' - Returns the image data directly encoded in base64\n" +
            "Base64 is preferred for immediate use, URLs for deferred processing.",
            "b64_json", GetValues: _ => ["url", "b64_json"], OrderPriority: -6, 
            Group: DallE3Group, FeatureFlag: "openai-api", IsAdvanced: true));

        StyleParam_Ideogram = T2IParamTypes.Register<string>(new("Generation Style",
            "Determines the artistic approach for image creation:\n" +
            "'Auto' - Automatically selects style based on prompt\n" +
            "'General' - Balanced style suitable for most prompts\n" +
            "'Realistic' - Photorealistic style with natural lighting and details\n" +
            "'Design' - Clean, graphic design aesthetic\n" +
            "'Render_3D' - 3D rendered style with depth and lighting\n" +
            "'Anime' - Anime/manga inspired artistic style",
            "AUTO",
            GetValues: _ => ["AUTO///Auto (Recommended)", "GENERAL///General Purpose",
                "REALISTIC///Photorealistic", "DESIGN///Graphic Design",
                "RENDER_3D///3D Rendered", "ANIME///Anime Style"],
            OrderPriority: -10, Group: IdeogramGeneralGroup, FeatureFlag: "ideogram-api"));

        // Advanced Parameters
        MagicPromptParam_Ideogram = T2IParamTypes.Register<string>(new("Magic Prompt Enhancement",
            "Controls Ideogram's prompt optimization system:\n" +
            "'Auto' - Let the system decide when to enhance prompts\n" +
            "'On' - Always enhance prompts for better results\n" +
            "'Off' - Use exact prompts without enhancement\n" +
            "Magic Prompt can help improve results but may deviate from exact prompt wording.", "AUTO",
            GetValues: _ => ["AUTO///Auto (Recommended)", "ON///Always Enhance", "OFF///Exact Prompts"],
            OrderPriority: -7, Group: IdeogramAdvancedGroup, FeatureFlag: "ideogram-api"));

        SeedParam_Ideogram = T2IParamTypes.Register<int>(new("Generation Seed",
            "Set a specific seed for reproducible results.\n" +
            "Same seed + same settings = same image.\n" +
            "Useful for iterating on specific images or sharing exact settings.", "-1",
            Min: -1, Max: 2147483647, ViewType: ParamViewType.SEED, OrderPriority: -4,
            Group: IdeogramAdvancedGroup, FeatureFlag: "ideogram-api"));

        ColorPaletteParam_Ideogram = T2IParamTypes.Register<string>(new("Color Theme",
            "Apply a predefined color palette to influence image colors:\n" +
            "'None' - No color preference\n" +
            "'Ember' - Warm, fiery tones\n" +
            "'Fresh' - Bright, clean colors\n" +
            "'Jungle' - Natural, organic greens\n" +
            "'Magic' - Mystical purples and blues\n" +
            "'Pastel' - Soft, muted tones", "None",
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
            OrderPriority: -3, Group: IdeogramAdvancedGroup, FeatureFlag: "ideogram-api"));

        GuidanceParam_BlackForest = T2IParamTypes.Register<double>(new("Prompt Guidance",
            "Controls how strictly the generation follows the prompt.\n" +
            "Flux Pro/1.1: Default 2.5 for balanced results\n" +
            "Flux Dev: Default 3.0 for increased prompt adherence\n" +
            "Lower values (1.5-2.5): More creative but less controlled\n" +
            "Higher values (3.0-5.0): Stricter prompt following but may reduce realism.", "2.5",
            Min: 1.5, Max: 5.0, Step: 0.1, ViewType: ParamViewType.SLIDER, OrderPriority: -6,
            Group: BlackForestGeneralGroup, FeatureFlag: "bfl-api"));

        SafetyParam_BlackForest = T2IParamTypes.Register<int>(new("Safety Filter Level",
            "Controls content filtering strictness for both input and output.\n" +
            "0: Most restrictive - blocks most potentially unsafe content\n" +
            "6: Least restrictive - allows most content through\n" +
            "Default 2 provides balanced filtering suitable for most use cases.", "2", Min: 0, Max: 6,
            ViewType: ParamViewType.SLIDER, OrderPriority: -5, Group: BlackForestGeneralGroup, FeatureFlag: "bfl-api"));

        IntervalParam_BlackForest = T2IParamTypes.Register<double>(new("Guidance Interval",
            "Fine-tunes how guidance is applied during generation.\n" +
            "Only available in Flux Pro and Pro 1.1 models.\n" +
            "Lower values (1.0-2.0): More precise, controlled results\n" +
            "Higher values (2.0-4.0): More creative freedom and variation\n" +
            "Default 2.0 provides good balance of control and creativity.", "2.0", Min: 1.0, Max: 4.0, Step: 0.1,
            ViewType: ParamViewType.SLIDER, OrderPriority: -5, IsAdvanced: true, Group: BlackForestAdvancedGroup,
            FeatureFlag: "bfl-api"));

        PromptEnhanceParam_BlackForest = T2IParamTypes.Register<bool>(new("Prompt Enhancement",
            "Enables Flux's automatic prompt enhancement system.\n" +
            "When enabled: Automatically expands prompts with additional details\n" +
            "Can help achieve more artistic results but may reduce prompt precision\n" +
            "Recommended for creative/artistic work, disable for exact prompt following.", "false", 
            OrderPriority: -4, IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl-api"));

        RawModeParam_BlackForest = T2IParamTypes.Register<bool>(new("Raw Mode",
            "Enables raw generation mode in Flux Pro 1.1.\n" +
            "When enabled: Generates less processed, more natural-looking images\n" +
            "Raw mode can produce more authentic results but may be less polished\n" +
            "Only available in Flux Pro 1.1 ultra mode.", "false", OrderPriority: -3,
            IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl-api"));

        ImagePromptParam_BlackForest = T2IParamTypes.Register<Image>(new("Image Prompt",
            "Optional image to use as a starting point or reference.\n" +
            "Acts as a visual guide for the generation process.\n" +
            "Useful for variations, style matching, or guided compositions.\n" +
            "Use with Image Prompt Strength to control influence.", null, OrderPriority: -2, 
            IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl-api"));

        ImagePromptStrengthParam_BlackForest = T2IParamTypes.Register<double>(new("Image Prompt Strength",
            "Controls how much the Image Prompt influences the generation.\n" +
            "0.0: Ignore image prompt completely\n" +
            "1.0: Follow image prompt very closely\n" +
            "Default 0.1 provides subtle guidance while allowing creativity.",
            "0.1", Min: 0.0, Max: 1.0, Step: 0.05, ViewType: ParamViewType.SLIDER,
            OrderPriority: -1, IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl-api"));

        OutputFormatParam_BlackForest = T2IParamTypes.Register<string>(new("Output Format",
            "Choose the file format for saving generated images:\n" +
            "JPEG: Smaller files, slight quality loss, good for sharing\n" +
            "PNG: Lossless quality, larger files, best for editing.",
            "jpeg", GetValues: _ => ["jpeg///JPEG (Smaller)", "png///PNG (Lossless)"],
            OrderPriority: 0, IsAdvanced: true, Group: BlackForestAdvancedGroup, FeatureFlag: "bfl-api"));

        // TODO: Rename this to something that displays better in the UI
        T2IEngine.DisregardedFeatureFlags.Add("bfl-api");
        T2IEngine.DisregardedFeatureFlags.Add("openai-api");
        T2IEngine.DisregardedFeatureFlags.Add("ideogram-api");

        // Register the dynamic API backend type
        Program.Backends.RegisterBackendType<DynamicAPIBackend>("dynamic_api_backend", "3rd Party Paid API Backends",
            "Generate images using various API services (OpenAI, Ideogram, Black Forest Labs)", true);
        // All key types must be added to the accepted list first
        string[] keyTypes = ["openai_api", "black_forest_api", "ideogram_api"]; 
        foreach (string keyType in keyTypes)
        {
            BasicAPIFeatures.AcceptedAPIKeyTypes.Add(keyType);
        }
        // Register API Key tables for each backend - safely handle if already registered
        RegisterApiKeyIfNeeded("openai_api", "openai", "OpenAI (ChatGPT)", "https://platform.openai.com/api-keys", 
            new HtmlString("To use OpenAI models in SwarmUI (via the MagicPrompt extension), you must set your OpenAI API key."));
        RegisterApiKeyIfNeeded("black_forest_api", "black_forest", "Black Forest (AI)", "https://blackforestlabs.com/api-keys", 
            new HtmlString("To use Black Forest in SwarmUI (via the MagicPrompt extension), you must set your Black Forest API key."));
        RegisterApiKeyIfNeeded("ideogram_api", "ideogram", "Ideogram", "https://ideogram.com/api-keys", 
            new HtmlString("To use Ideogram in SwarmUI (via the MagicPrompt extension), you must set your Ideogram API key.")); 
        Logs.Init("Hartsy's APIBackends extension has successfully started.");
    }
    
    /// <summary>Safely registers an API key if it's not already registered</summary>
    private static void RegisterApiKeyIfNeeded(string keyType, string jsPrefix, string title, string createLink, HtmlString infoHtml)
    {
        try
        {
            if (!UserUpstreamApiKeys.KeysByType.ContainsKey(keyType))
            {
                UserUpstreamApiKeys.Register(new(keyType, jsPrefix, title, createLink, infoHtml));
                Logs.Debug($"Registered API key type: {keyType}");
            }
            else
            {
                Logs.Debug($"API key type '{keyType}' already registered, skipping registration");
            }
        }
        catch (Exception ex)
        {
            Logs.Warning($"Failed to register API key type '{keyType}': {ex.Message}");
        }
    }
}
