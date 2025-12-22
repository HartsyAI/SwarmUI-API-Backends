/**
 * API Backends Feature Handler for SwarmUI
 * This script integrates various API-based image generation services
 * with SwarmUI's feature system, controlling parameter visibility.
 */

// 1. Feature set changer - Add required features for API models and remove unsupported ones
// Also modifies param objects to add synthetic feature flags for proper visibility control
featureSetChangers.push(() => {
    if (!gen_param_types) {
        return [[], []];
    }

    // Determine current API backend type
    const isOpenAIModel = curModelArch === 'openai_api';
    const isIdeogramModel = curModelArch === 'ideogram_api';
    const isBlackForestModel = curModelArch === 'bfl_api';
    const isGrokModel = curModelArch === 'grok_api';
    const isGoogleImagenModel = curModelArch === 'google_imagen_api';
    const isApiModel = isOpenAIModel || isIdeogramModel || isBlackForestModel || isGrokModel || isGoogleImagenModel;

    // List of core parameters that don't have feature flags but shouldn't show for API models
    const coreParamsToHideForAPI = [
        'steps', 'cfgscale', 'width', 'height', 'sidelength', 'aspectratio',
        'seed', 'batchsize', 'initimagecreativity', 'initimageresettonorm',
        'initimagenoise', 'maskblur', 'maskgrow', 'maskshrinkgrow',
        'useinpaintingencode', 'initimagerecompositemask', 'zeronegative',
        'seamlesstileable', 'cascadelatentcompression', 'sd3textencs',
        'fluxguidancescale', 'fluxdisableguidance', 'clipstopatlayer',
        'vaetilesize', 'vaetileoverlap', 'removebackground', 'automaticvae',
        'modelspecificenhancements'
    ];

    // Modify param objects to add/remove synthetic feature flag
    // This ensures SwarmUI's core visibility system properly handles these params
    for (let param of gen_param_types) {
        if (coreParamsToHideForAPI.includes(param.id)) {
            if (isApiModel) {
                // Store original feature_flag if we haven't already
                if (!param.hasOwnProperty('original_feature_flag_api')) {
                    param.original_feature_flag_api = param.feature_flag;
                }
                // Add synthetic feature flag that won't be in currentBackendFeatureSet
                // This makes SwarmUI's core system think this param is unsupported
                param.feature_flag = param.original_feature_flag_api
                    ? `${param.original_feature_flag_api},__api_incompatible__`
                    : '__api_incompatible__';
            } else if (param.hasOwnProperty('original_feature_flag_api')) {
                // Restore original feature flag when switching away from API models
                param.feature_flag = param.original_feature_flag_api;
                delete param.original_feature_flag_api;
            }
        }
    }

    // If not using any API model, just remove API-specific feature flags
    if (!isApiModel) {
        return [[], ['openai_api', 'ideogram_api', 'bfl_api', 'grok_api', 'google_imagen_api',
            'dalle2_params', 'dalle3_params', 'gpt-image-1_params', 'gpt-image-1.5_params',
            'ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params',
            'flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_kontext_pro_params', 'flux_kontext_max_params', 'flux_2_max_params', 'flux_2_pro_params',
            'grok_2_image_params', 'imagen_4_0_params']];
    }

    // These features should be REMOVED for all API backends as they're incompatible
    const commonRemoveFlags = [
        'sampling', 'refiners', 'controlnet', 'variation_seed',
        'video', 'autowebui', 'comfyui', 'frameinterps', 'ipadapter',
        'sdxl', 'cascade', 'sd3', 'seamless', 'freeu', 'teacache',
        'text2video', 'yolov8', 'aitemplate', 'endstepsearly',
        'dynamic_thresholding', 'flux-dev', 'zero_negative',
        // Also remove model-specific feature flags when switching away
        'dalle2_params', 'dalle3_params', 'gpt-image-1_params', 'gpt-image-1.5_params',
        'ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params',
        'flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_2_pro_params', 'flux_2_max_params',
        'flux_kontext_pro_params', 'flux_kontext_max_params',
        'grok_2_image_params', 'imagen_4_0_params'
    ];

    // These are features that should always be enabled for API models
    const addFlags = ['prompt', 'images'];

    // Add provider-specific features only
    if (isOpenAIModel) {
        addFlags.push('openai_api');
        addFlags.push('dalle2_params', 'dalle3_params', 'gpt-image-1_params', 'gpt-image-1.5_params');
    } else if (isIdeogramModel) {
        addFlags.push('ideogram_api');
        addFlags.push('ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params');
    } else if (isBlackForestModel) {
        addFlags.push('bfl_api');
        addFlags.push('flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_kontext_pro_params', 'flux_kontext_max_params', 'flux_2_max_params', 'flux_2_pro_params');
    } else if (isGrokModel) {
        addFlags.push('grok_api');
        addFlags.push('grok_2_image_params');
    } else if(isGoogleImagenModel) {
        addFlags.push('google_imagen_api');
        addFlags.push('imagen_4_0_params');
    }

    console.log(`[api-backends] Adding feature flags: ${addFlags.join(', ')}`);
    console.log(`[api-backends] Removing feature flags: ${commonRemoveFlags.join(', ')}`);

    return [addFlags, commonRemoveFlags];
});

// 2. Run setup when model changes
if (typeof addModelChangeCallback === 'function') {
    addModelChangeCallback(() => {
        console.log(`[api-backends] Model changed to: ${curModelArch}`);

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
