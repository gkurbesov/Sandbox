using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using Sandbox.Library.JsonSerialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Sandbox.Tests.Serialization;

[TestClass]
public class JsonSerializationTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var entity = new TestSerializationEntity { Name = "Test", Age = 10 };
        var dto = new TestDto { Entity = entity };

        var serializeOptions = new JsonSerializerOptions();
        
        serializeOptions.Converters.Add(new UniversalJsonConverter());

        var json = JsonSerializer.Serialize((object)dto);
        var actual = JsonSerializer.Deserialize<TestDto>(json);

        //var json = JsonConvert.SerializeObject(dto);
        // var actual = JsonConvert.DeserializeObject<TestDto>(json);

        Assert.IsNotNull(actual);
        Assert.IsNotNull(actual.Entity);
        Assert.IsInstanceOfType(actual, typeof(TestSerializationEntity));
       // Assert.AreEqual(entity.Name, actual.Entity.Name);
        Assert.AreEqual(entity.Age, ((TestSerializationEntity)actual.Entity).Age);
    }

    private class TestDto
    {
        public object Entity { get; set; }
    }
}

// Универсальный конвертер для сериализации объектов любого типа, включая производные классы
public class UniversalJsonConverter : System.Text.Json.Serialization.JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Можно ограничить применение конвертера, если это необходимо
        return true; // Применять конвертер ко всем типам
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Реализуйте десериализацию, если это необходимо
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        Type type = value.GetType();

        writer.WriteStartObject();

        foreach (PropertyInfo propertyInfo in type.GetProperties())
        {
            // Проверяем, можно ли читать свойство и нет ли у него атрибута JsonIgnore
            if (propertyInfo.CanRead && propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            {
                object propValue = propertyInfo.GetValue(value);
                writer.WritePropertyName(propertyInfo.Name);
                JsonSerializer.Serialize(writer, propValue, propertyInfo.PropertyType, options);
            }
        }

        writer.WriteEndObject();
    }
}