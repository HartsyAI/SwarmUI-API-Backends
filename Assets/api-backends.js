featureSetChangers.push(() => {
    if (!gen_param_types) {
        return [[], []];
    }

    // Get the model class from curModelArch
    const isAPIModel = curModelArch?.startsWith('api-');

    if (isAPIModel) {
        // For any API model, enable basic features and hide standard SD features
        let addFlags = ['prompt', 'images'];
        let removeFlags = [
            'steps', 'cfg_scale', 'seed', 'resolution', 'sampling', 'variation_seed',
            'init_image', 'refine_upscale', 'controlnet', 'image_to_video', 'advanced_video',
            'comfyui', 'controlnet_two', 'controlnet_three', 'swarm_internal', 'video_extend',
            'advanced_model_addons', 'negative_prompt', 'seamless', 'cascade', 'sd3',
            'sdxl', 'refiners', 'video', 'text2video', 'regional_promptin', 'advanced_sampling'
        ];

        // Add provider-specific flags based on model class
        if (curModelArch?.startsWith('openai_api')) {
            addFlags.push('openai_api');
        }
        else if (curModelArch?.startsWith('ideogram_api')) {
            addFlags.push('ideogram_api');
        }
        else if (curModelArch?.startsWith('bfl_api')) {
            addFlags.push('bfl_api');
        }

        return [addFlags, removeFlags];
    }
    else {
        // For non-API models, add standard features and remove API-specific ones
        return [
            ['prompt', 'steps', 'cfg_scale', 'seed', 'images', 'resolution', 'sampling', 'negative_prompt'],
            ['openai_api', 'ideogram_api', 'bfl_api']
        ];
    }
});