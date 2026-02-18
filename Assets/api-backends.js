/**
 * API Backends Feature Handler for SwarmUI
 * Integrates API-based image generation services with SwarmUI's feature system.
 */

const APIBackendsConfig = {
    // Provider IDs mapped to their model-specific feature flags
    providers: {
        openai_api: ['dalle2_params', 'dalle3_params', 'gpt-image-1_params', 'gpt-image-1.5_params', 'openai_sora_params'],
        ideogram_api: ['ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params', 'ideogram_style'],
        bfl_api: ['flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_kontext_pro_params', 'flux_kontext_max_params', 'flux_2_max_params', 'flux_2_pro_params', 'bfl_prompt_enhance', 'bfl_image_prompt'],
        grok_api: ['grok_2_image_params'],
        google_api: ['google_imagen_params', 'google_gemini_params'],
        fal_api: ['fal_t2i_params', 'fal_i2i_params', 'fal_video_params', 'fal_utility_params', 'fal_sora_video_params', 'fal_kling_video_params', 'fal_veo_video_params', 'fal_minimax_video_params', 'fal_luma_video_params', 'fal_hunyuan_video_params']
    },

    // Model name patterns to feature flags (order matters)
    modelPatterns: {
        openai_api: [
            { pattern: 'sora-2-pro', flag: 'openai_sora_params' },
            { pattern: 'sora-2', flag: 'openai_sora_params' },
            { pattern: 'gpt-image-1.5', flag: 'gpt-image-1.5_params' },
            { pattern: 'gpt-image-1', flag: 'gpt-image-1_params' },
            { pattern: 'dall-e-3', flag: 'dalle3_params' },
            { pattern: 'dall-e-2', flag: 'dalle2_params' }
        ],
        grok_api: [
            { pattern: 'grok-2-image', flag: 'grok_2_image_params' }
        ],
        google_api: [
            { pattern: 'imagen-3.0', flag: 'google_imagen_params' },
            { pattern: 'gemini-2.0', flag: 'google_gemini_params' }
        ]
    },

    // Core Swarm params to SHOW (unhide) for specific API models
    coreParamsToShow: {
        bfl_api: {
            'flux-pro-1.1-ultra': ['seed', 'aspectratio', 'initimage'],
            'flux-pro-1.1': ['seed', 'width', 'height', 'initimage'],
            'flux-dev': ['seed', 'width', 'height', 'steps', 'initimage'],
            'flux-kontext-pro': ['seed', 'aspectratio', 'initimage'],
            'flux-kontext-max': ['seed', 'aspectratio', 'initimage'],
            'flux-2-pro': ['seed', 'width', 'height', 'initimage'],
            'flux-2-max': ['seed', 'width', 'height', 'initimage']
        },
        ideogram_api: {
            '*': ['seed', 'initimage', 'maskimage']
        },
        grok_api: {
            '*': ['seed', 'initimage']
        },
        google_api: {
            'gemini-': ['seed', 'initimage'],
            '*': ['seed']
        },
        fal_api: { '*': ['seed'] }
    },

    // All core params to potentially hide for API models
    coreParamsToHide: [
        'steps', 'cfgscale', 'width', 'height', 'sidelength', 'aspectratio',
        'seed', 'batchsize', 'initimage', 'initimagecreativity', 'initimageresettonorm',
        'initimagenoise', 'maskimage', 'maskblur', 'maskgrow', 'maskshrinkgrow',
        'useinpaintingencode', 'initimagerecompositemask', 'unsamplepprompt', 'zeronegative',
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
        'flux-dev', 'zero_negative', 'sdcpp'
    ],

    // Core parameter groups that are local-only and should be hidden for all API models
    // Child groups are caught automatically by the parent-chain walk (e.g. controlnettwo via controlnet)
    // NOTE: 'resolution' is intentionally NOT here - BFL models need width/height from that group
    coreGroupsToHide: [
        'refineupscale',        // Refine / Upscale (+ child refinerparamoverrides)
        'controlnet',           // ControlNet (+ children controlnettwo, controlnetthree)
        'swarminternal',        // Swarm Internal (BatchSize, VAE format, wildcards, etc.)
        'advancedvideo',        // Advanced Video (+ child videoobscureoptions)
        'videoextend',          // Video Extend
        'advancedmodeladdons',  // Advanced Model Addons (VAE, LoRAs, CLIP models, etc.)
        'magicpromptautoenable',// Magic Prompt Auto Enable (extension)
        'dynamicthresholding',  // Dynamic Thresholding (extension)
        'regionalprompting',    // Regional Prompting (+ child segments)
        'segmentrefining',      // Segment Refining
        'segmentparamoverrides',// Segment Param Overrides
        'advancedsampling',     // Advanced Sampling
        // NOTE: 'initimage' intentionally NOT here - BFL models use Init Image for image_prompt/input_image
        'freeu',                // FreeU
        'texttovideo',          // Text to Video (local video gen)
        'texttoaudio',          // Text to Audio
        'alternateguidance'     // Alternate Guidance
        // NOTE: 'sampling', 'variation', 'resolution' are NOT here - they auto-hide via
        // incompatibleFlags, and individual params (steps, width, height) need to be
        // selectively shown for some BFL models via coreParamsToShow.
    ],

    get providerIds() {
        return Object.keys(this.providers);
    },

    get allModelFlags() {
        return Object.values(this.providers).flat();
    },

    get allApiFlags() {
        return [...this.providerIds, ...this.allModelFlags];
    },

    // Determine the active model-specific feature flag based on the selected model
    getActiveModelFlags(curArch, modelName) {
        if (curArch === 'fal_api') return this.getFalModelFlags(modelName);
        if (curArch === 'bfl_api') return this.getBflModelFlags(modelName);
        if (curArch === 'ideogram_api') return this.getIdeogramModelFlags(modelName);
        const patterns = this.modelPatterns[curArch];
        if (!patterns) return this.providers[curArch] || [];
        for (const { pattern, flag } of patterns) {
            if (modelName.includes(pattern)) return [flag];
        }
        return this.providers[curArch] || [];
    },

    // Ideogram capability-based flags per model
    getIdeogramModelFlags(modelName) {
        let flags = [];
        // Model-specific flag
        if (modelName.includes('V_3')) flags.push('ideogram_v3_params');
        else if (modelName.includes('V_2_TURBO')) flags.push('ideogram_v2_params');
        else if (modelName.includes('V_2')) flags.push('ideogram_v2_params');
        else if (modelName.includes('V_1')) flags.push('ideogram_v1_params');
        // Style and color palette: V2+ only (not V1)
        if (!modelName.includes('V_1')) flags.push('ideogram_style');
        return flags;
    },

    // BFL capability-based flags per model (returns multiple flags)
    getBflModelFlags(modelName) {
        let flags = [];
        // Model-specific flag
        if (modelName.includes('flux-pro-1.1-ultra')) flags.push('flux_ultra_params');
        else if (modelName.includes('flux-pro-1.1')) flags.push('flux_pro_params');
        else if (modelName.includes('flux-dev')) flags.push('flux_dev_params');
        else if (modelName.includes('flux-kontext-pro')) flags.push('flux_kontext_pro_params');
        else if (modelName.includes('flux-kontext-max')) flags.push('flux_kontext_max_params');
        else if (modelName.includes('flux-2-pro')) flags.push('flux_2_pro_params');
        else if (modelName.includes('flux-2-max')) flags.push('flux_2_max_params');
        // Prompt enhancement: all except flux-2-* models
        if (!modelName.includes('flux-2-')) flags.push('bfl_prompt_enhance');
        // Image prompt (single): flux-dev, flux-pro-1.1, flux-ultra
        if (modelName.includes('flux-dev') || modelName.includes('flux-pro-1.1')) flags.push('bfl_image_prompt');
        return flags;
    },

    getFalModelFlags(modelName) {
        if (!modelName || !modelName.startsWith('API Models/Fal/')) {
            return ['fal_t2i_params'];
        }
        if (modelName.includes('/Utility/')) {
            return ['fal_utility_params'];
        }
        // Model-specific video flags based on provider prefix
        if (modelName.endsWith('-t2v') || modelName.endsWith('-i2v')) {
            if (modelName.includes('/Sora/')) return ['fal_sora_video_params'];
            if (modelName.includes('/Kling/')) return ['fal_kling_video_params'];
            if (modelName.includes('/Google/') && modelName.includes('veo')) return ['fal_veo_video_params'];
            if (modelName.includes('/MiniMax/')) return ['fal_minimax_video_params'];
            if (modelName.includes('/Luma/')) return ['fal_luma_video_params'];
            if (modelName.includes('/Hunyuan/')) return ['fal_hunyuan_video_params'];
            return ['fal_video_params'];
        }
        // Image editing / image-to-image models: show both t2i and i2i params
        if (this.isFalEditModel(modelName)) {
            return ['fal_t2i_params', 'fal_i2i_params'];
        }
        return ['fal_t2i_params'];
    },

    // Check if a Fal model is an image editing / image-to-image model
    isFalEditModel(modelName) {
        // Explicit edit model patterns
        if (modelName.endsWith('-edit')) return true;
        if (modelName.endsWith('-i2i')) return true;
        if (modelName.includes('kontext')) return true;
        if (modelName.includes('hidream-e1')) return true;
        // Dual models that support both t2i and editing (image input optional)
        if (modelName.includes('seedream')) return true;
        if (modelName.includes('omnigen')) return true;
        return false;
    },

    // Check if a core Swarm param should be shown for the current API model
    shouldShowCoreParam(curArch, modelName, paramId) {
        const providerConfig = this.coreParamsToShow[curArch];
        if (!providerConfig) return false;
        if (providerConfig['*'] && providerConfig['*'].includes(paramId)) {
            return true;
        }
        const sortedPatterns = Object.keys(providerConfig)
            .filter(p => p !== '*')
            .sort((a, b) => b.length - a.length);
        for (const pattern of sortedPatterns) {
            if (modelName.includes(pattern)) {
                return providerConfig[pattern].includes(paramId);
            }
        }
        return false;
    }
};

// Feature set changer - controls parameter visibility for API models
featureSetChangers.push(() => {
    if (!gen_param_types) {
        return [[], []];
    }

    const curArch = currentModelHelper.curArch;
    const modelName = currentModelHelper.curModel || '';
    const isApiModel = APIBackendsConfig.providerIds.includes(curArch);

    // Handle core Swarm param visibility for API models
    for (let param of gen_param_types) {
        // Hide individual core params that don't apply to API models
        if (APIBackendsConfig.coreParamsToHide.includes(param.id)) {
            if (isApiModel) {
                // Save original feature flag on first encounter
                if (!param.hasOwnProperty('original_feature_flag_api')) {
                    param.original_feature_flag_api = param.feature_flag;
                }
                const shouldShow = APIBackendsConfig.shouldShowCoreParam(curArch, modelName, param.id);
                if (shouldShow) {
                    // Clear feature flag entirely so the param is always visible
                    // (restoring original would fail if original flag like "sampling" is incompatible)
                    param.feature_flag = null;
                } else {
                    param.feature_flag = '__api_incompatible__';
                }
            } else if (param.hasOwnProperty('original_feature_flag_api')) {
                // Restore original when switching back to a local model
                param.feature_flag = param.original_feature_flag_api;
                delete param.original_feature_flag_api;
            }
        }
        // Hide params belonging to local-only groups (Regional Prompting, Segment Refining, etc.)
        let inHiddenGroup = false;
        let currentGroup = param.group;
        while (currentGroup) {
            if (APIBackendsConfig.coreGroupsToHide.includes(currentGroup.id)) {
                inHiddenGroup = true;
                break;
            }
            currentGroup = currentGroup.parent;
        }
        if (inHiddenGroup) {
            if (isApiModel) {
                if (!param.hasOwnProperty('original_feature_flag_api_group')) {
                    param.original_feature_flag_api_group = param.feature_flag;
                }
                param.feature_flag = '__api_incompatible__';
            } else if (param.hasOwnProperty('original_feature_flag_api_group')) {
                param.feature_flag = param.original_feature_flag_api_group;
                delete param.original_feature_flag_api_group;
            }
        }
    }
    // Not using API model - remove all API-specific flags
    if (!isApiModel) {
        return [[], APIBackendsConfig.allApiFlags];
    }
    // Determine which model-specific flags should be active
    const activeModelFlags = APIBackendsConfig.getActiveModelFlags(curArch, modelName);
    // Remove: other providers + their model flags + inactive model flags from current provider
    const otherProviderIds = APIBackendsConfig.providerIds.filter(id => id !== curArch);
    const otherProviderModelFlags = otherProviderIds.flatMap(id => APIBackendsConfig.providers[id] || []);
    const currentProviderInactiveFlags = (APIBackendsConfig.providers[curArch] || []).filter(f => !activeModelFlags.includes(f));
    const removeFlags = [
        ...APIBackendsConfig.incompatibleFlags,
        ...otherProviderIds,
        ...otherProviderModelFlags,
        ...currentProviderInactiveFlags
    ];
    const addFlags = ['prompt', 'images', curArch, ...activeModelFlags];
    return [addFlags, removeFlags];
});

// Run setup when model changes
if (typeof addModelChangeCallback === 'function') {
    addModelChangeCallback(() => {
        console.log(`[api-backends] Model changed to: ${currentModelHelper.curArch} (${currentModelHelper.curModel})`);
        reviseBackendFeatureSet();
        hideUnsupportableParams();
    });
}

// Initial setup
setTimeout(() => {
    console.log('[api-backends] Initial parameter setup starting');
    reviseBackendFeatureSet();
    hideUnsupportableParams();
}, 500);
