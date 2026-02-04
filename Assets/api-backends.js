/**
 * API Backends Feature Handler for SwarmUI
 * Integrates API-based image generation services with SwarmUI's feature system.
 */

// Centralized configuration for API backend feature flags
const APIBackendsConfig = {
    // Provider IDs mapped to their model-specific feature flags
    providers: {
        openai_api: ['dalle2_params', 'dalle3_params', 'gpt-image-1_params', 'gpt-image-1.5_params'],
        ideogram_api: ['ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params'],
        bfl_api: ['flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_kontext_pro_params', 'flux_kontext_max_params', 'flux_2_max_params', 'flux_2_pro_params'],
        grok_api: ['grok_2_image_params'],
        google_api: ['google_imagen_params', 'google_gemini_params'],
        fal_api: ['fal_flux_params', 'fal_flux_pro_params', 'fal_flux_kontext_params', 'fal_flux_2_params', 'fal_recraft_params', 'fal_ideogram_params', 'fal_sd_params', 'fal_grok_params', 'fal_video_params', 'fal_utility_params']
    },

    // Core params to hide for all API models (no feature flags but incompatible)
    coreParamsToHide: [
        'steps', 'cfgscale', 'width', 'height', 'sidelength', 'aspectratio',
        'seed', 'batchsize', 'initimagecreativity', 'initimageresettonorm',
        'initimagenoise', 'maskblur', 'maskgrow', 'maskshrinkgrow',
        'useinpaintingencode', 'initimagerecompositemask', 'zeronegative',
        'seamlesstileable', 'cascadelatentcompression', 'sd3textencs',
        'fluxguidancescale', 'fluxdisableguidance', 'clipstopatlayer',
        'vaetilesize', 'vaetileoverlap', 'removebackground', 'automaticvae',
        'modelspecificenhancements'
    ],

    // Features incompatible with API backends (local-only)
    incompatibleFlags: [
        'sampling', 'refiners', 'controlnet', 'variation_seed', 'video',
        'autowebui', 'comfyui', 'frameinterps', 'ipadapter', 'sdxl',
        'cascade', 'sd3', 'seamless', 'freeu', 'teacache', 'text2video',
        'yolov8', 'aitemplate', 'endstepsearly', 'dynamic_thresholding',
        'flux-dev', 'zero_negative'
    ],

    // Get all provider IDs
    get providerIds() {
        return Object.keys(this.providers);
    },

    // Get all model-specific flags across all providers
    get allModelFlags() {
        return Object.values(this.providers).flat();
    },

    // Get all flags to remove when not using API models
    get allApiFlags() {
        return [...this.providerIds, ...this.allModelFlags];
    }
};

// Feature set changer - controls parameter visibility for API models
featureSetChangers.push(() => {
    if (!gen_param_types) {
        return [[], []];
    }

    const curArch = currentModelHelper.curArch;
    const isApiModel = APIBackendsConfig.providerIds.includes(curArch);

    // Modify param objects to add/remove synthetic feature flag for core params
    for (let param of gen_param_types) {
        if (APIBackendsConfig.coreParamsToHide.includes(param.id)) {
            if (isApiModel) {
                if (!param.hasOwnProperty('original_feature_flag_api')) {
                    param.original_feature_flag_api = param.feature_flag;
                }
                param.feature_flag = param.original_feature_flag_api
                    ? `${param.original_feature_flag_api},__api_incompatible__`
                    : '__api_incompatible__';
            } else if (param.hasOwnProperty('original_feature_flag_api')) {
                param.feature_flag = param.original_feature_flag_api;
                delete param.original_feature_flag_api;
            }
        }
    }

    // Not using API model - remove all API-specific flags
    if (!isApiModel) {
        return [[], APIBackendsConfig.allApiFlags];
    }

    // Build flags to remove (incompatible + other providers' model flags)
    const removeFlags = [
        ...APIBackendsConfig.incompatibleFlags,
        ...APIBackendsConfig.allModelFlags
    ];

    // Build flags to add (base + current provider + its model flags)
    const providerModelFlags = APIBackendsConfig.providers[curArch] || [];
    const addFlags = ['prompt', 'images', curArch, ...providerModelFlags];

    return [addFlags, removeFlags];
});

// 2. Run setup when model changes
if (typeof addModelChangeCallback === 'function') {
    addModelChangeCallback(() => {
        console.log(`[api-backends] Model changed to: ${currentModelHelper.curArch}`);

        // Update the feature set and parameter visibility
        reviseBackendFeatureSet();
        hideUnsupportableParams();
    });
}

// 3. Initial setup
setTimeout(() => {
    console.log('[api-backends] Initial parameter setup starting');
    reviseBackendFeatureSet();
    hideUnsupportableParams();
}, 500);
