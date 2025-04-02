/**
 * API Backends Feature Handler
 * 
 * This script controls the visibility of parameters based on the currently selected API model.
 * It uses a comprehensive approach to find and manage all feature flags.
 */

// Function to log the current visible structure of parameters and groups
function logVisibleParams() {
    console.log('===== VISIBLE UI ELEMENTS =====');

    // Get all parameter groups
    document.querySelectorAll('.input-group').forEach(group => {
        // Check if the group is visible
        const style = window.getComputedStyle(group);
        if (style.display !== 'none') {
            const groupId = group.id;
            const groupTitle = group.querySelector('.header-label')?.textContent || 'Unknown Group';

            console.log(`GROUP: ${groupTitle} (${groupId})`);

            // Find visible parameters in this group
            const groupContent = group.querySelector('.input-group-content');
            if (groupContent) {
                const params = groupContent.querySelectorAll('.auto-input');

                params.forEach(param => {
                    const paramStyle = window.getComputedStyle(param);

                    if (paramStyle.display !== 'none') {
                        const paramName = param.querySelector('.auto-input-name')?.textContent || 'Unknown Parameter';
                        const paramId = param.id || 'unknown-id';
                        const featureFlag = param.getAttribute('data-feature-require') || 'none';

                        console.log(`  PARAM: ${paramName} (${paramId}) [Feature: ${featureFlag}]`);
                    }
                });
            }

            console.log('---');
        }
    });

    console.log('==============================');
}

// Our main feature setter
featureSetChangers.push(() => {
    if (!gen_param_types) {
        return [[], []];
    }

    // Check if current model is an API model
    const isOpenAIModel = curModelArch === 'openai_api';
    const isIdeogramModel = curModelArch === 'ideogram_api';
    const isBlackForestModel = curModelArch === 'bfl_api';

    // If not an API model, just remove API feature flags
    if (!isOpenAIModel && !isIdeogramModel && !isBlackForestModel) {
        return [[], ['openai_api', 'ideogram_api', 'bfl_api']];
    }

    // Get all feature flags from the DOM
    const allFeatureFlags = new Set();

    // Find feature flags in groups
    document.querySelectorAll('.input-group').forEach(group => {
        const featureFlag = group.getAttribute('data-feature-require');
        if (featureFlag && featureFlag !== 'none') {
            allFeatureFlags.add(featureFlag);
        }
    });

    // Find feature flags in parameters
    document.querySelectorAll('.auto-input').forEach(param => {
        const featureFlag = param.getAttribute('data-feature-require');
        if (featureFlag && featureFlag !== 'none') {
            allFeatureFlags.add(featureFlag);
        }
    });

    // Convert set to array
    const removeFlags = Array.from(allFeatureFlags);

    // Now determine which flags to add
    let addFlags = ['prompt', 'images'];

    if (isOpenAIModel) {
        addFlags.push('openai_api');
        // Remove this flag from removal list
        const index = removeFlags.indexOf('openai_api');
        if (index > -1) {
            removeFlags.splice(index, 1);
        }
    }
    else if (isIdeogramModel) {
        addFlags.push('ideogram_api');
        // Remove this flag from removal list
        const index = removeFlags.indexOf('ideogram_api');
        if (index > -1) {
            removeFlags.splice(index, 1);
        }
    }
    else if (isBlackForestModel) {
        addFlags.push('bfl_api');
        // Remove this flag from removal list
        const index = removeFlags.indexOf('bfl_api');
        if (index > -1) {
            removeFlags.splice(index, 1);
        }
    }

    console.log(`[api-backends] Found ${removeFlags.length} feature flags to remove`);
    console.log(`[api-backends] Remove flags: ${removeFlags.join(', ')}`);
    console.log(`[api-backends] Add flags: ${addFlags.join(', ')}`);

    return [addFlags, removeFlags];
});

// Helper to manually hide specific elements
hideParamCallbacks.push(groups => {
    // Only run this logic if we have a valid model
    if (!curModelArch) return;

    // Check if current model is from one of our API providers
    const isOpenAIModel = curModelArch === 'openai_api';
    const isIdeogramModel = curModelArch === 'ideogram_api';
    const isBlackForestModel = curModelArch === 'bfl_api';

    // If this isn't an API model, don't modify anything
    if (!isOpenAIModel && !isIdeogramModel && !isBlackForestModel) return;

    // List of groups we want to keep
    const keepGroups = [];

    // Core Parameters is always shown
    keepGroups.push('auto-group-coreparameters');

    // Add provider-specific groups
    if (isOpenAIModel) {
        keepGroups.push('auto-group-dallesettings');
    }
    else if (isIdeogramModel) {
        keepGroups.push('auto-group-ideogrambasicsettings', 'auto-group-ideogramadvancedsettings');
    }
    else if (isBlackForestModel) {
        keepGroups.push('auto-group-fluxcoresettings', 'auto-group-fluxadvancedsettings');
    }

    // Hide all input groups except the ones we want to keep
    document.querySelectorAll('.input-group').forEach(group => {
        if (!keepGroups.includes(group.id)) {
            group.style.display = 'none';
        } else {
            group.style.display = 'block';
        }
    });

    // In Core Parameters, keep only the essential ones
    if (document.getElementById('input_group_content_coreparameters')) {
        // Get all parameters in the Core Parameters group
        const coreParams = document.querySelectorAll('#input_group_content_coreparameters > .auto-input');

        // List of parameter IDs to keep
        const keepParams = ['input_prompt', 'input_seed', 'input_images'];

        // Hide parameters that are not in our keepParams list
        coreParams.forEach(param => {
            let shouldKeep = false;

            // Check all inputs in this parameter
            const inputs = param.querySelectorAll('input, textarea, select');
            for (const input of inputs) {
                if (keepParams.includes(input.id)) {
                    shouldKeep = true;
                    break;
                }
            }

            // Hide if we shouldn't keep it
            if (!shouldKeep) {
                param.style.display = 'none';
            }
        });
    }

    // Hide any advanced settings toggle
    const advancedToggle = document.querySelector('#display_advanced_options_span');
    if (advancedToggle) {
        advancedToggle.style.display = 'none';
    }

    // Log the current visible structure for debugging
    setTimeout(logVisibleParams, 300);
});

// Trigger updates when model changes
if (typeof addModelChangeCallback === 'function') {
    addModelChangeCallback(() => {
        console.log(`[api-backends] Model changed to: ${curModelArch}`);
        setTimeout(() => {
            reviseBackendFeatureSet();
            hideUnsupportableParams();
        }, 100);
    });
}

// Run the initial setup when the script loads
setTimeout(() => {
    hideUnsupportableParams();
    console.log('[api-backends] Initial parameter setup complete');
}, 500);