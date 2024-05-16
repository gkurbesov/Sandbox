using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.Library.JsonSerialization;

public interface ISystemTextJsonConverter
{
}

public class CustomPolymorphicSerializationConverter : JsonConverter<object>
{
    private static readonly JsonConverter<object> s_defaultConverter =
        (JsonConverter<object>)JsonSerializerOptions.Default.GetConverter(typeof(object));

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert != typeof(string);
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return s_defaultConverter.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        s_defaultConverter.Write(writer, value, options);
    }
}

public class NewAbstractTypeJsonConverterFactory : JsonConverterFactory
{
    public Type? ExpectedType { get; set; }
    private readonly IAbstractTypeSerializationResolver _typeSerializationResolver;

    public NewAbstractTypeJsonConverterFactory(IAbstractTypeSerializationResolver typeSerializationResolver)
    {
        _typeSerializationResolver = typeSerializationResolver;
    }


    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert != ExpectedType && _typeSerializationResolver.IsRegistered(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ExpectedType = typeToConvert;
        return new NewAbstractTypeJsonConverter(_typeSerializationResolver);
    }
}

public class NewAbstractTypeJsonConverter : JsonConverter<object>, ISystemTextJsonConverter
{
    private const string TypeFieldName = "$type";

    private Type? _expectedType;
    private readonly IAbstractTypeSerializationResolver _typeSerializationResolver;

    public NewAbstractTypeJsonConverter(IAbstractTypeSerializationResolver typeSerializationResolver)
    {
        _typeSerializationResolver = typeSerializationResolver;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert != _expectedType && _typeSerializationResolver.IsRegistered(typeToConvert);
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var serializedTypeName = TryGetSerializedTypeName(reader) ??
                                 throw new InvalidOperationException("Serialized type name is not found.");
        var serializedType = _typeSerializationResolver.GetTypeBySerializedTypeName(serializedTypeName);

        var document = JsonDocument.ParseValue(ref reader);
        var result = Activator.CreateInstance(serializedType);
        document.PopulateObject(result, new JsonSerializerOptions(options));
        //return document.RootElement.Deserialize(serializedType, options);
        return result;
        var converter = JsonSerializerOptions.Default.GetConverter(serializedType);
        return JsonSerializer.Deserialize(ref reader, serializedType, options);
    }

    private string? TryGetSerializedTypeName(Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token.");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return null;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token.");

            if (reader.GetString() == TypeFieldName)
            {
                if (!reader.Read())
                    throw new JsonException("Expected Value token.");

                return reader.GetString();
            }

            reader.Skip();
        }

        throw new JsonException("Expected EndObject token.");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var type = value.GetType();

        var jsonSerializerOptions = GetOptions(options, type);

        var document = JsonSerializer.SerializeToDocument(value, type, jsonSerializerOptions);
        var rootElement = document.RootElement;

        if (rootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Json element must be an object.");

        writer.WriteStartObject();

        var serializedTypeName = _typeSerializationResolver.GetSerializedTypeName(type);
        writer.WriteString(TypeFieldName, serializedTypeName);

        foreach (var property in rootElement.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    private JsonSerializerOptions GetOptions(JsonSerializerOptions options, Type expectedType)
    {
        var jsonSerializerOptions = new JsonSerializerOptions(options);
        jsonSerializerOptions.Converters.Clear();
        foreach (var converter in options.Converters.Where(x => x != this))
        {
            jsonSerializerOptions.Converters.Add(converter);
        }

        var abstractTypeJsonConverter = new NewAbstractTypeJsonConverter(_typeSerializationResolver)
        {
            _expectedType = expectedType
        };
        jsonSerializerOptions.Converters.Add(abstractTypeJsonConverter);

        return jsonSerializerOptions;
    }
}