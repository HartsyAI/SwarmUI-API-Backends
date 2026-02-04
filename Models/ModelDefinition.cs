using System;
using System.Collections.Generic;

namespace Hartsy.Extensions.APIBackends.Models
{
    /// <summary>Type-safe definition for an API model. Used by ModelFactory to create T2IModel instances.</summary>
    public sealed class ModelDefinition
    {
        /// <summary>Unique model identifier (e.g., "dall-e-3", "flux/dev").</summary>
        public required string Id { get; init; }

        /// <summary>Display title for the model.</summary>
        public required string Title { get; init; }

        /// <summary>Description of the model's capabilities.</summary>
        public required string Description { get; init; }

        /// <summary>Author/creator of the model.</summary>
        public string Author { get; init; } = "";

        /// <summary>Standard output width in pixels.</summary>
        public int StandardWidth { get; init; } = 1024;

        /// <summary>Standard output height in pixels.</summary>
        public int StandardHeight { get; init; } = 1024;

        /// <summary>Path to preview image file (relative to extension root).</summary>
        public string PreviewImagePath { get; init; } = "";

        /// <summary>Release date/year of the model.</summary>
        public string Date { get; init; } = "";

        /// <summary>License type (e.g., "Commercial", "Apache 2.0").</summary>
        public string License { get; init; } = "Commercial";

        /// <summary>Usage hint shown to users.</summary>
        public string UsageHint { get; init; } = "";

        /// <summary>Tags for categorization and filtering.</summary>
        public string[] Tags { get; init; } = [];

        /// <summary>Feature flag for parameter visibility (e.g., "dalle3_params").</summary>
        public string FeatureFlag { get; init; } = "";

        /// <summary>Optional model-specific endpoint path override.</summary>
        public string EndpointOverride { get; init; } = "";

        /// <summary>Creates the full model name with provider prefix.</summary>
        public string GetFullName(string providerPrefix) => $"API Models/{providerPrefix}/{Id}";
    }

    /// <summary>Type-safe definition for an API provider.</summary>
    public sealed class ProviderDefinition
    {
        /// <summary>Unique provider identifier (e.g., "openai_api", "fal_api").</summary>
        public required string Id { get; init; }

        /// <summary>Display name of the provider.</summary>
        public required string Name { get; init; }

        /// <summary>Prefix used in model names (e.g., "OpenAI", "Fal").</summary>
        public required string ModelPrefix { get; init; }

        /// <summary>Base URL for API requests.</summary>
        public required string BaseUrl { get; init; }

        /// <summary>Authorization header type (e.g., "Bearer", "x-api-key").</summary>
        public string AuthHeaderType { get; init; } = "Bearer";

        /// <summary>Custom auth header name if not using standard Authorization header.</summary>
        public string CustomAuthHeader { get; init; } = "";

        /// <summary>Model class ID for SwarmUI categorization.</summary>
        public required string ModelClassId { get; init; }

        /// <summary>Model class display name.</summary>
        public required string ModelClassName { get; init; }

        /// <summary>All models available from this provider.</summary>
        public required IReadOnlyList<ModelDefinition> Models { get; init; }
    }

    /// <summary>Builder for creating ModelDefinition instances with fluent syntax.</summary>
    public sealed class ModelDefinitionBuilder
    {
        private string _id = "";
        private string _title = "";
        private string _description = "";
        private string _author = "";
        private int _width = 1024;
        private int _height = 1024;
        private string _previewPath = "";
        private string _date = "";
        private string _license = "Commercial";
        private string _usageHint = "";
        private readonly List<string> _tags = [];
        private string _featureFlag = "";
        private string _endpointOverride = "";

        public ModelDefinitionBuilder WithId(string id) { _id = id; return this; }
        public ModelDefinitionBuilder WithTitle(string title) { _title = title; return this; }
        public ModelDefinitionBuilder WithDescription(string desc) { _description = desc; return this; }
        public ModelDefinitionBuilder WithAuthor(string author) { _author = author; return this; }
        public ModelDefinitionBuilder WithDimensions(int width, int height) { _width = width; _height = height; return this; }
        public ModelDefinitionBuilder WithPreviewImage(string path) { _previewPath = path; return this; }
        public ModelDefinitionBuilder WithDate(string date) { _date = date; return this; }
        public ModelDefinitionBuilder WithLicense(string license) { _license = license; return this; }
        public ModelDefinitionBuilder WithUsageHint(string hint) { _usageHint = hint; return this; }
        public ModelDefinitionBuilder WithTags(params string[] tags) { _tags.AddRange(tags); return this; }
        public ModelDefinitionBuilder WithFeatureFlag(string flag) { _featureFlag = flag; return this; }
        public ModelDefinitionBuilder WithEndpointOverride(string endpoint) { _endpointOverride = endpoint; return this; }

        public ModelDefinition Build()
        {
            if (string.IsNullOrEmpty(_id)) throw new InvalidOperationException("Model ID is required");
            if (string.IsNullOrEmpty(_title)) throw new InvalidOperationException("Model title is required");
            if (string.IsNullOrEmpty(_description)) throw new InvalidOperationException("Model description is required");

            return new ModelDefinition
            {
                Id = _id,
                Title = _title,
                Description = _description,
                Author = _author,
                StandardWidth = _width,
                StandardHeight = _height,
                PreviewImagePath = _previewPath,
                Date = _date,
                License = _license,
                UsageHint = _usageHint,
                Tags = [.. _tags],
                FeatureFlag = _featureFlag,
                EndpointOverride = _endpointOverride
            };
        }

        /// <summary>Creates a new builder instance.</summary>
        public static ModelDefinitionBuilder Create() => new();
    }

    /// <summary>Builder for creating ProviderDefinition instances with fluent syntax.</summary>
    public sealed class ProviderDefinitionBuilder
    {
        private string _id = "";
        private string _name = "";
        private string _modelPrefix = "";
        private string _baseUrl = "";
        private string _authHeaderType = "Bearer";
        private string _customAuthHeader = "";
        private string _modelClassId = "";
        private string _modelClassName = "";
        private readonly List<ModelDefinition> _models = [];

        public ProviderDefinitionBuilder WithId(string id) { _id = id; return this; }
        public ProviderDefinitionBuilder WithName(string name) { _name = name; return this; }
        public ProviderDefinitionBuilder WithModelPrefix(string prefix) { _modelPrefix = prefix; return this; }
        public ProviderDefinitionBuilder WithBaseUrl(string url) { _baseUrl = url; return this; }
        public ProviderDefinitionBuilder WithAuthHeader(string type, string customHeader = "")
        {
            _authHeaderType = type;
            _customAuthHeader = customHeader;
            return this;
        }
        public ProviderDefinitionBuilder WithModelClass(string id, string name)
        {
            _modelClassId = id;
            _modelClassName = name;
            return this;
        }
        public ProviderDefinitionBuilder AddModel(ModelDefinition model) { _models.Add(model); return this; }
        public ProviderDefinitionBuilder AddModels(IEnumerable<ModelDefinition> models) { _models.AddRange(models); return this; }

        public ProviderDefinition Build()
        {
            if (string.IsNullOrEmpty(_id)) throw new InvalidOperationException("Provider ID is required");
            if (string.IsNullOrEmpty(_name)) throw new InvalidOperationException("Provider name is required");
            if (string.IsNullOrEmpty(_modelPrefix)) throw new InvalidOperationException("Model prefix is required");
            if (string.IsNullOrEmpty(_baseUrl)) throw new InvalidOperationException("Base URL is required");
            if (_models.Count == 0) throw new InvalidOperationException("At least one model is required");

            return new ProviderDefinition
            {
                Id = _id,
                Name = _name,
                ModelPrefix = _modelPrefix,
                BaseUrl = _baseUrl,
                AuthHeaderType = _authHeaderType,
                CustomAuthHeader = _customAuthHeader,
                ModelClassId = _modelClassId,
                ModelClassName = _modelClassName,
                Models = _models.AsReadOnly()
            };
        }

        /// <summary>Creates a new builder instance.</summary>
        public static ProviderDefinitionBuilder Create() => new();
    }
}
