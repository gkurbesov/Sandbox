using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Sandbox.Library.JsonSerialization;

public static class JsonSerializerExtensions
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> _populateMap = new();

    public static void PopulateObject(
        this JsonDocument jsonDocument,
        object destination,
        JsonSerializerOptions? options = null)
    {
        options = GetOptionsWithPopulateResolver(options);
        PopulateTypeInfoResolver.t_populateObject = destination;
        try
        {
            object? result = jsonDocument.Deserialize(destination.GetType(), options);
            Debug.Assert(ReferenceEquals(result, destination));
        }
        finally
        {
            PopulateTypeInfoResolver.t_populateObject = null;
        }
    }

    private static JsonSerializerOptions GetOptionsWithPopulateResolver(JsonSerializerOptions? options)
    {
        options ??= JsonSerializerOptions.Default;

        if (!_populateMap.TryGetValue(options, out var populateResolverOptions))
        {
            Debug.Assert(options.TypeInfoResolver != null);

            populateResolverOptions = new JsonSerializerOptions(options)
            {
                TypeInfoResolver = new PopulateTypeInfoResolver(options.TypeInfoResolver)
            };

            _populateMap.TryAdd(options, populateResolverOptions);
        }

        return populateResolverOptions;
    }

    private class PopulateTypeInfoResolver : IJsonTypeInfoResolver
    {
        [ThreadStatic] internal static object? t_populateObject;
        private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;

        public PopulateTypeInfoResolver(IJsonTypeInfoResolver jsonTypeInfoResolver)
        {
            _jsonTypeInfoResolver = jsonTypeInfoResolver;
        }

        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = _jsonTypeInfoResolver.GetTypeInfo(type, options);
            if (typeInfo != null && typeInfo.Kind != JsonTypeInfoKind.None)
            {
                Func<object>? defaultCreateObjectDelegate = typeInfo.CreateObject;
                typeInfo.CreateObject = () =>
                {
                    object? result = t_populateObject;
                    if (result != null)
                    {
                        t_populateObject = null;
                    }
                    else
                    {
                        result = defaultCreateObjectDelegate?.Invoke();
                    }

                    return result!;
                };
            }

            return typeInfo;
        }
    }
}