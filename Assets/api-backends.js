/**
 * API Backends Feature Handler for SwarmUI
 * This script integrates various API-based image generation services
 * with SwarmUI's feature system, controlling parameter visibility.
 */

// 1. Feature set changer - Add required features for API models and remove unsupported ones
featureSetChangers.push(() => {
    if (!gen_param_types) {
        return [[], []];
    }

    // Determine current API backend type
    const isOpenAIModel = curModelArch === 'openai_api';
    const isIdeogramModel = curModelArch === 'ideogram_api';
    const isBlackForestModel = curModelArch === 'bfl_api';
    const isApiModel = isOpenAIModel || isIdeogramModel || isBlackForestModel;

    // If not using any API model, just remove API-specific feature flags
    if (!isApiModel) {
        return [[], ['openai_api', 'ideogram_api', 'bfl_api',
            'dalle2_params', 'dalle3_params', 'gpt-image-1_params',
            'ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params',
            'flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_kontext_pro_params', 'flux_kontext_max_params']];
    }

    // These features should be REMOVED for all API backends as they're incompatible
    const commonRemoveFlags = [
        'sampling', 'refiners', 'controlnet', 'variation_seed',
        'video', 'autowebui', 'comfyui', 'frameinterps', 'ipadapter',
        'sdxl', 'cascade', 'sd3', 'seamless', 'freeu', 'teacache',
        'text2video', 'yolov8', 'aitemplate', 'endstepsearly'
    ];

    // These are features that should always be enabled for API models
    const addFlags = ['prompt', 'images'];

    // Add provider-specific features only
    if (isOpenAIModel) {
        addFlags.push('openai_api');
        addFlags.push('dalle2_params', 'dalle3_params', 'gpt-image-1_params');
    } else if (isIdeogramModel) {
        addFlags.push('ideogram_api');
        addFlags.push('ideogram_v1_params', 'ideogram_v2_params', 'ideogram_v3_params');
    } else if (isBlackForestModel) {
        addFlags.push('bfl_api');
        addFlags.push('flux_ultra_params', 'flux_pro_params', 'flux_dev_params', 'flux_kontext_pro_params', 'flux_kontext_max_params');
    }

    console.log(`[api-backends] Adding feature flags: ${addFlags.join(', ')}`);
    console.log(`[api-backends] Removing feature flags: ${commonRemoveFlags.join(', ')}`);

    return [addFlags, commonRemoveFlags];
});

// 2. Additional parameter visibility handling
hideParamCallbacks.push(() => {
    // Only run this logic if we have a valid model
    if (!curModelArch) return;

    // Check if current model is from one of our API providers
    const isApiModel = ['openai_api', 'ideogram_api', 'bfl_api'].includes(curModelArch);

    // If this isn't an API model, don't modify anything
    if (!isApiModel) return;

    console.log('[api-backends] Performing additional UI cleanups for API models');

    // Parameters to hide for ALL API models, regardless of type
    const paramsToHide = [
        // Specific params without feature flags that don't apply to API models
        'zeronegative',
        'initimage', 'initimagecreativity', 'initimageresettonorm', 'initimagenoise',
        'maskimage', 'maskshrinkgrow', 'maskblur', 'maskgrow', 'maskbehavior',
        'initimagerecompositemask', 'useinpaintingencode', 'unsamplerprompt',

        // Advanced sampling params that are irrelevant for APIs
        'samplersigmamin', 'samplersigmamax', 'samplerrho', 'sigmashift',
        'ip2pcfg2', 'clipstopatlayer', 'vaetilesize', 'vaetileoverlap',
        'colorationcorrectionbehavior', 'removebackground'
    ];

    // Hide specific parameters
    paramsToHide.forEach(paramId => {
        const elem = document.getElementById(`input_${paramId}`);
        if (elem) {
            const box = findParentOfClass(elem, 'auto-input');
            if (box) {
                box.style.display = 'none';
                box.dataset.visible_controlled = 'true';

                // Mark param as hidden for group visibility check
                box.dataset.api_hidden = 'true';
            }
        }
    });

    // Groups to hide completely for ALL API models
    const groupsToHide = [
        'sampling', 'initimage', 'variationseed', 'controlnet', 'video',
        'texttovideo', 'advancedvideo', 'videoextend', 'advancedsampling',
        'freeu', 'regionalobject', 'regionalprompting'
    ];

    // Hide entire groups
    groupsToHide.forEach(groupId => {
        const group = document.getElementById(`auto-group-${groupId}`);
        if (group) {
            group.style.display = 'none';
            group.dataset.visible_controlled = 'true';
        }
    });

    // Provider-specific hiding
    const isOpenAIModel = curModelArch === 'openai_api';
    const isIdeogramModel = curModelArch === 'ideogram_api';
    const isBlackForestModel = curModelArch === 'bfl_api';

    // Hide provider-specific groups and parameters
    document.querySelectorAll('.input-group').forEach(group => {
        const headerLabel = group.querySelector('.header-label');
        if (!headerLabel) return;

        const groupName = headerLabel.textContent.trim();

        if (isOpenAIModel) {
            // When using OpenAI, hide Ideogram and Flux groups
            if (groupName.includes('Ideogram') || groupName.includes('Flux')) {
                group.style.display = 'none';
                group.dataset.visible_controlled = 'true';
            }
        } else if (isIdeogramModel) {
            // When using Ideogram, hide OpenAI and Flux groups
            if (groupName.includes('DALL-E') || groupName.includes('Flux')) {
                group.style.display = 'none';
                group.dataset.visible_controlled = 'true';
            }
        } else if (isBlackForestModel) {
            // When using Black Forest, hide OpenAI and Ideogram groups
            if (groupName.includes('DALL-E') || groupName.includes('Ideogram')) {
                group.style.display = 'none';
                group.dataset.visible_controlled = 'true';
            }
        }
    });

    // Now check for groups with all parameters hidden and hide those groups too
    document.querySelectorAll('.input-group').forEach(group => {
        // Skip groups we've already explicitly hidden
        if (group.style.display === 'none' || group.dataset.visible_controlled === 'true') {
            return;
        }

        // Check if the group has any visible parameters
        const groupContent = group.querySelector('.input-group-content');
        if (groupContent) {
            const autoInputs = groupContent.querySelectorAll('.auto-input');
            let allHidden = true;

            // Check each parameter in the group
            for (const autoInput of autoInputs) {
                // If it's not hidden by our script or by SwarmUI's normal mechanism, the group has visible content
                if (autoInput.style.display !== 'none' && !autoInput.dataset.api_hidden && !autoInput.dataset.visible_controlled) {
                    allHidden = false;
                    break;
                }
            }

            // If all parameters are hidden, hide the whole group
            if (allHidden && autoInputs.length > 0) {
                group.style.display = 'none';
                group.dataset.visible_controlled = 'true';
                console.log(`[api-backends] Hiding empty group: ${group.querySelector('.header-label')?.textContent || group.id}`);
            }
        }
    });
    // Force another visibility update to catch any changes
    scheduleParamUnsupportUpdate();
});

// 3. Run setup when model changes
if (typeof addModelChangeCallback === 'function') {
    addModelChangeCallback(() => {
        console.log(`[api-backends] Model changed to: ${curModelArch}`);

        // Update the feature set and parameter visibility
        reviseBackendFeatureSet();
        hideUnsupportableParams();
    });
}

// 4. Initial setup
setTimeout(() => {
    console.log('[api-backends] Initial parameter setup starting');
    reviseBackendFeatureSet();
    hideUnsupportableParams();

    // Run multiple cleanup passes to ensure everything is properly hidden
    setTimeout(() => {
        hideUnsupportableParams();
    }, 1000);
}, 500);
