/**
 * API Backends Feature Handler for SwarmUI
 * Integrates API-based image generation services with SwarmUI's feature system.
 */

const APIBackendsConfig = {
    // Provider IDs mapped to their model-specific feature flags
    providers: {
        openai_api: ['dalle2_params', 'dalle3_params', 'gpt-image-1_params', 'gpt-image-1.5_params'],
        ideogram_api: ['ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params'],
        bfl_api: ['flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_kontext_pro_params', 'flux_kontext_max_params', 'flux_2_max_params', 'flux_2_pro_params'],
        grok_api: ['grok_2_image_params'],
        google_api: ['google_imagen_params', 'google_gemini_params'],
        fal_api: ['fal_t2i_params', 'fal_video_params', 'fal_utility_params']
    },

    // Model name patterns to feature flags (order matters - more specific patterns first)
    modelPatterns: {
        openai_api: [
            { pattern: 'gpt-image-1.5', flag: 'gpt-image-1.5_params' },
            { pattern: 'gpt-image-1', flag: 'gpt-image-1_params' },
            { pattern: 'dall-e-3', flag: 'dalle3_params' },
            { pattern: 'dall-e-2', flag: 'dalle2_params' }
        ],
        ideogram_api: [
            { pattern: 'V_3', flag: 'ideogram_v3_params' },
            { pattern: 'V_2_TURBO', flag: 'ideogram_v2_params' },
            { pattern: 'V_2', flag: 'ideogram_v2_params' },
            { pattern: 'V_1', flag: 'ideogram_v1_params' }
        ],
        bfl_api: [
            { pattern: 'flux-pro-1.1-ultra', flag: 'flux_ultra_params' },
            { pattern: 'flux-pro-1.1', flag: 'flux_pro_params' },
            { pattern: 'flux-dev', flag: 'flux_dev_params' },
            { pattern: 'flux-kontext-pro', flag: 'flux_kontext_pro_params' },
            { pattern: 'flux-kontext-max', flag: 'flux_kontext_max_params' },
            { pattern: 'flux-2-pro', flag: 'flux_2_pro_params' },
            { pattern: 'flux-2-max', flag: 'flux_2_max_params' }
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
    // '*' matches all models for that provider
    coreParamsToShow: {
        bfl_api: {
            'flux-pro-1.1-ultra': ['seed'],
            'flux-pro-1.1': ['seed', 'width', 'height'],
            'flux-dev': ['seed', 'width', 'height', 'steps'],
            'flux-kontext-pro': ['seed'],
            'flux-kontext-max': ['seed'],
            'flux-2-pro': ['seed', 'width', 'height'],
            'flux-2-max': ['seed', 'width', 'height']
        },
        ideogram_api: { '*': ['seed'] },
        fal_api: { '*': ['seed'] }
    },

    // All core params to potentially hide for API models
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
        'flux-dev', 'zero_negative', 'sdcpp'
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
        if (curArch === 'fal_api') {
            return this.getFalModelFlags(modelName);
        }
        const patterns = this.modelPatterns[curArch];
        if (!patterns) return this.providers[curArch] || [];
        for (const { pattern, flag } of patterns) {
            if (modelName.includes(pattern)) return [flag];
        }
        return this.providers[curArch] || [];
    },

    getFalModelFlags(modelName) {
        if (!modelName || !modelName.startsWith('API Models/Fal/')) {
            return ['fal_t2i_params'];
        }
        if (modelName.includes('/Utility/')) {
            return ['fal_utility_params'];
        }
        if (modelName.endsWith('-t2v') || modelName.endsWith('-i2v')) {
            return ['fal_video_params'];
        }
        return ['fal_t2i_params'];
    },

    // Check if a core Swarm param should be shown for the current API model
    shouldShowCoreParam(curArch, modelName, paramId) {
        const providerConfig = this.coreParamsToShow[curArch];
        if (!providerConfig) return false;
        if (providerConfig['*'] && providerConfig['*'].includes(paramId)) {
            return true;
        }
        // Check model-specific patterns (longer patterns first for specificity)
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
        if (APIBackendsConfig.coreParamsToHide.includes(param.id)) {
            const shouldShow = isApiModel && APIBackendsConfig.shouldShowCoreParam(curArch, modelName, param.id);
            if (isApiModel && !shouldShow) {
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

    // Determine which model-specific flags should be active
    const activeModelFlags = APIBackendsConfig.getActiveModelFlags(curArch, modelName);

    // Remove: other providers + their model flags + inactive model flags from current provider
    const otherProviderIds = APIBackendsConfig.providerIds.filter(id => id !== curArch);
    const otherProviderModelFlags = otherProviderIds.flatMap(id => APIBackendsConfig.providers[id] || []);
    const currentProviderInactiveFlags = (APIBackendsConfig.providers[curArch] || [])
        .filter(f => !activeModelFlags.includes(f));

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
