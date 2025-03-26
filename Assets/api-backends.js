featureSetChangers.push(() => {
    if (!gen_param_types) {
        return [[], []];
    }

    // Hide the entire Image To Video group for API models
    const videoGroup = document.getElementById('input_group_content_imagetovideo');
    if (videoGroup) {
        videoGroup.style.display =
            (curModelArch?.startsWith('bfl-api-flux')
                || curModelArch?.startsWith('openai-api-dalle')
                || curModelArch?.startsWith('anthropic-api-claude')
                || curModelArch?.startsWith('ideogram-api')) ? 'none' : '';
    }

    if (curModelArch?.startsWith('bfl-api-flux')
        || curModelArch?.startsWith('openai-api-dalle')
        || curModelArch?.startsWith('anthropic-api-claude')
        || curModelArch?.startsWith('ideogram-api')) {

        // For API models - remove all core and ComfyUI-related parameters
        let hideFlags = [
            'comfyui',
            'controlnet',
            'video',
            'text2video',
            'refiners',
            'init_image',
            'resolution',
            'sampling',
            'variation_seed',
            'seamless',
            'cascade',
            'sd3',
            'flux-dev',
            'steps',
            'cfg_scale',
            'seed',
            'images'
        ];

        // Add the API-specific feature flag
        if (curModelArch?.startsWith('bfl-api-flux')) {
            return [['bfl-api'], hideFlags];
        }
        else if (curModelArch?.startsWith('openai-api-dalle')) {
            return [['openai-api'], hideFlags];
        }
        else if (curModelArch?.startsWith('anthropic-api-claude')) {
            return [['anthropic-api'], hideFlags];
        }
        else if (curModelArch?.startsWith('ideogram-api')) {
            return [['ideogram-api'], hideFlags];
        }
    }

    // For non-API models, enable core parameters and hide API features
    return [[
        'steps',
        'cfg_scale',
        'seed',
        'images',
        'resolution',
        'sampling'
    ], [
        'bfl-api',
        'openai-api',
        'anthropic-api',
        'ideogram-api'
    ]];
});

// Add a function to reset feature flags
function resetFeatureFlags() {
    currentBackendFeatureSet = [];
    reviseBackendFeatureSet();
}