using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using Sandbox.Library.JsonSerialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Sandbox.Tests.Serialization;

public class CustomTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        return base.GetTypeInfo(type, options);
    }
}

[TestClass]
public class JsonSerializationTests
{
    [TestMethod]
    public void Test()
    {
        var dto = new TestDto
        {
            Entity = new MainEntity()
            {
                Name = "MainEntity",
                Salary = 1000,
                /* Child = new MainEntity()
                 {
                     Name = "Child",
                     Salary = 500,
                     Child = new TestSerializationEntity()
                     {
                         Age = 100,
                         Name = "TestSerializationEntity"
                     }
                }*/
            },
            /*Entities = new BaseEntity[]
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
            }*/
        };

        var typeResolver = new AbstractTypeSerializationResolver()
            .RegisterType<MainEntity>("MainEntity")
            .RegisterType<TestSerializationEntity>("TestSerializationEntity");
        var options = new JsonSerializerOptions();
        options.Converters.Add(new NewAbstractTypeJsonConverter(typeResolver));
       // options.Converters.Add(new CustomPolymorphicSerializationConverter());

        var json = JsonSerializer.Serialize(dto.Entity, options);
        var deserialized = JsonSerializer.Deserialize<MainEntity>(json, options);

        deserialized.Should().BeEquivalentTo(dto);
    }


    private class TestDto
    {
        public BaseEntity? Entity { get; set; }
        public BaseEntity? TestSubEntity { get; set; }
        public IEnumerable<BaseEntity> Entities { get; set; }
    }
}