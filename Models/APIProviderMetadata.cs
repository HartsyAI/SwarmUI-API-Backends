using SwarmUI.Text2Image;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hartsy.Extensions.APIBackends.Models;

/// <summary>Interface to handle parameter values regardless of their type.</summary>
public interface IParameterHandler
{
    /// <summary>Gets the value from a T2IParamInput.</summary>
    object GetValue(T2IParamInput input);
}

/// <summary>Generic implementation of parameter handler.</summary>
public class ParameterHandler<T>(T2IRegisteredParam<T> param) : IParameterHandler
{
    private readonly T2IRegisteredParam<T> _param = param;

    public object GetValue(T2IParamInput input)
    {
        return input.Get(_param);
    }
}

/// <summary>Metadata about an API provider including its models, parameters, and request configuration.</summary>
public class APIProviderMetadata
{
    /// <summary>Display name of the provider.</summary>
    public string Name { get; set; }

    /// <summary>All models available from this provider.</summary>
    public Dictionary<string, T2IModel> Models { get; set; }

    /// <summary>Parameters specific to each model.</summary>
    public Dictionary<string, Dictionary<string, IParameterHandler>> ModelParameters { get; private set; } = new Dictionary<string, Dictionary<string, IParameterHandler>>();

    /// <summary>Configuration for making API requests.</summary>
    public RequestConfig RequestConfig { get; set; }

    /// <summary>Get the parameters registered for a specific model.</summary>
    public Dictionary<string, IParameterHandler> GetParametersForModel(string modelName)
    {
        if (!ModelParameters.TryGetValue(modelName, out Dictionary<string, IParameterHandler> parameters))
        {
            parameters = new Dictionary<string, IParameterHandler>();
            ModelParameters[modelName] = parameters;
        }
        return parameters;
    }

    /// <summary>Register a parameter for a specific model.</summary>
    public void AddParameterToModel<T>(string modelName, string paramName, T2IRegisteredParam<T> param)
    {
        Dictionary<string, IParameterHandler> parameters = GetParametersForModel(modelName);
        parameters[paramName] = new ParameterHandler<T>(param);
    }
}

/// <summary>Configuration data for making API requests.</summary>
public class RequestConfig
{
    /// <summary>Base URL for the API.</summary>
    public string BaseUrl { get; set; }

    /// <summary>Authorization header prefix (e.g. "Bearer" or "x-api-key").</summary>
    public string AuthHeader { get; set; }

    /// <summary>Function to build the API request body from the input parameters.</summary>
    public Func<T2IParamInput, JObject> BuildRequest { get; set; }

    /// <summary>Function to process the API response into a byte array of image data.</summary>
    public Func<JObject, string, Task<byte[]>> ProcessResponse { get; set; }
}
