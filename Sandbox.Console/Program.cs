// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

CsvReader csvReader = new CsvReader();

foreach (var line in CsvProcessor.ProcessCsvLines(csvReader))
{
    Console.WriteLine(line);
}


public static class CsvProcessor
{
    public static IEnumerable<string> ProcessCsvLines(CsvReader csvReader)
    {
        using var innerEnumerator = csvReader.GetLineEnumerator();
        while (innerEnumerator.MoveNext())
        {
            if (innerEnumerator.Current.Length > 0)
            {
                yield return innerEnumerator.Current;
            }
        }
    }
}

public class CsvReader
{
    private List<string> _lines;

    public CsvReader()
    {
        // Загрузка строк CSV из файла (для примера просто инициализируем некоторыми значениями)
        _lines = new List<string> { "Name,Age", "Alice,30", "", "Bob,25" };
    }

    public IEnumerator<string> GetLineEnumerator()
    {
        foreach (var line in _lines)
        {
            yield return line;
        }
    }
}

/*
var dto = new TestDto
{
    Entity = new MainEntity()
    {
        Name = "MainEntity",
        Salary = 1000,
        Child = new MainEntity()
         {
             Name = "Child",
             Salary = 500,
             Child = new TestSerializationEntity()
             {
                 Age = 100,
                 Name = "TestSerializationEntity"
             }
        }
    },
    Entities = new BaseEntity[]
    {
        new TestSerializationEntity()
        {
            Age = 100,
            Name = "Entity-1"
        },
        new MainEntity()
        {
            Name = "Entity-2",
            Salary = 666
        }
    }
};

var typeResolver = new AbstractTypeSerializationResolver()
    .RegisterType<MainEntity>("MainEntity")
    .RegisterType<TestSerializationEntity>("TestSerializationEntity");
var options = new JsonSerializerOptions();
options.Converters.Add(new NewAbstractTypeJsonConverter(typeResolver));

//var json = JsonSerializer.Serialize(dto, options);


var json = JsonSerializer.Serialize(dto, options);

var options2 = new JsonSerializerOptions();
options2.Converters.Add(new NewAbstractTypeJsonConverter(typeResolver));

var deserialized = JsonSerializer.Deserialize<TestDto>(json, options2);

Console.WriteLine(deserialized.GetType().Name);


public class TestDto
{
    public BaseEntity? Entity { get; set; }
    public BaseEntity? TestSubEntity { get; set; }
    public IEnumerable<BaseEntity> Entities { get; set; }
}


public interface ITestSerializationEntity
{
    string? Name { get; set; }
}

public abstract class BaseEntity : ITestSerializationEntity
{
    public string? Name { get; set; }

    public BaseEntity Child { get; set; }
}

public class MainEntity : BaseEntity
{
    public int Salary { get; set; }
}

public class TestSerializationEntity : BaseEntity
{
    public int Age { get; set; }
}

public interface IAbstractTypeSerializationResolver
{
    bool IsRegistered(Type type);

    Type GetTypeBySerializedTypeName(string serializedTypeName);

    string GetSerializedTypeName(Type type);
}

public class AbstractTypeSerializationResolver : IAbstractTypeSerializationResolver
{
    private readonly Dictionary<string, Type> _typeBySerializedTypeName = new();
    private readonly Dictionary<Type, string> _serializedTypeNameByType = new();


    public AbstractTypeSerializationResolver RegisterType<T>(string serializedTypeName) =>
        Register(typeof(T), serializedTypeName);

    public AbstractTypeSerializationResolver Register(Type type, string serializedTypeName)
    {
        _typeBySerializedTypeName[serializedTypeName] = type;
        _serializedTypeNameByType[type] = serializedTypeName;
        return this;
    }

    public bool IsRegistered(Type type)
    {
        return _serializedTypeNameByType.ContainsKey(type) || _serializedTypeNameByType.Keys.Any(type.IsAssignableFrom);
    }

    public Type GetTypeBySerializedTypeName(string serializedTypeName)
    {
        return _typeBySerializedTypeName[serializedTypeName];
    }

    public string GetSerializedTypeName(Type type)
    {
        return _serializedTypeNameByType[type];
    }
}


public class NewAbstractTypeJsonConverter : JsonConverter<object>
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

        reader.CurrentState

        //var document = JsonDocument.ParseValue(ref reader);

        //return document.RootElement.Deserialize(serializedType, subOptions);
        var result = JsonSerializer.Deserialize(ref reader, serializedType, options);
        return result;
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

*/