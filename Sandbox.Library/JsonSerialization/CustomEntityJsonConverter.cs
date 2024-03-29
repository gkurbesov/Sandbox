using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.Library.JsonSerialization;

public class CustomEntityJsonConverter : JsonConverter<ITestSerializationEntity>
{
    public override ITestSerializationEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ITestSerializationEntity value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}