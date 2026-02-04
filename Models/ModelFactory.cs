using System;
using System.Collections.Generic;
using System.IO;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace Hartsy.Extensions.APIBackends.Models
{
    /// <summary>Factory for creating T2IModel instances from ModelDefinition objects.</summary>
    public static class ModelFactory
    {
        private static readonly Dictionary<string, T2IModelClass> _modelClasses = [];
        private const string ExtensionRoot = "src/Extensions/SwarmUI-API-Backends";

        /// <summary>Creates a T2IModel from a ModelDefinition and ProviderDefinition.</summary>
        public static T2IModel Create(ModelDefinition model, ProviderDefinition provider)
        {
            string fullName = model.GetFullName(provider.ModelPrefix);
            string previewImage = LoadPreviewImage(model.PreviewImagePath);
            T2IModelClass modelClass = GetOrCreateModelClass(provider.ModelClassId, provider.ModelClassName);

            // Build tags including provider and feature flag
            List<string> allTags = [provider.Id, provider.Name.ToLowerInvariant(), .. model.Tags];
            if (!string.IsNullOrEmpty(model.FeatureFlag))
            {
                allTags.Add(model.FeatureFlag);
            }

            return new T2IModel(null, null, null, fullName)
            {
                Title = model.Title,
                Description = model.Description,
                ModelClass = modelClass,
                StandardWidth = model.StandardWidth,
                StandardHeight = model.StandardHeight,
                IsSupportedModelType = true,
                PreviewImage = previewImage,
                Metadata = new T2IModelHandler.ModelMetadataStore
                {
                    ModelName = fullName,
                    Title = model.Title,
                    Author = string.IsNullOrEmpty(model.Author) ? provider.Name : model.Author,
                    Description = model.Description,
                    PreviewImage = previewImage,
                    StandardWidth = model.StandardWidth,
                    StandardHeight = model.StandardHeight,
                    License = model.License,
                    UsageHint = string.IsNullOrEmpty(model.UsageHint) 
                        ? $"API-based generation via {provider.Name}" 
                        : model.UsageHint,
                    Date = model.Date,
                    ModelClassType = provider.ModelClassId,
                    Tags = [.. allTags],
                    TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TimeModified = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };
        }

        /// <summary>Creates all T2IModel instances for a provider.</summary>
        public static Dictionary<string, T2IModel> CreateAllModels(ProviderDefinition provider)
        {
            Dictionary<string, T2IModel> models = [];
            foreach (ModelDefinition modelDef in provider.Models)
            {
                string fullName = modelDef.GetFullName(provider.ModelPrefix);
                models[fullName] = Create(modelDef, provider);
                Logs.Debug($"[ModelFactory] Created model: {fullName}");
            }
            return models;
        }

        /// <summary>Gets or creates a T2IModelClass for the provider.</summary>
        public static T2IModelClass GetOrCreateModelClass(string id, string name)
        {
            if (!_modelClasses.TryGetValue(id, out T2IModelClass modelClass))
            {
                modelClass = new T2IModelClass
                {
                    ID = id,
                    Name = name,
                    CompatClass = default,
                    StandardWidth = 1024,
                    StandardHeight = 1024,
                    IsThisModelOfClass = (model, header) => true
                };
                _modelClasses[id] = modelClass;
                T2IModelClassSorter.Register(modelClass);
                Logs.Debug($"[ModelFactory] Registered model class: {id} ({name})");
            }
            return modelClass;
        }

        /// <summary>Loads a preview image from the specified path and returns as base64 data URI.</summary>
        private static string LoadPreviewImage(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return "";
            }

            string fullPath = Path.Combine(ExtensionRoot, relativePath);
            if (!File.Exists(fullPath))
            {
                Logs.Warning($"[ModelFactory] Preview image not found: {fullPath}");
                return "";
            }

            try
            {
                byte[] imageBytes = File.ReadAllBytes(fullPath);
                string extension = Path.GetExtension(relativePath).ToLowerInvariant();
                string mimeType = extension switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "image/png"
                };
                return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
            }
            catch (Exception ex)
            {
                Logs.Warning($"[ModelFactory] Failed to load preview image {fullPath}: {ex.Message}");
                return "";
            }
        }
    }
}
