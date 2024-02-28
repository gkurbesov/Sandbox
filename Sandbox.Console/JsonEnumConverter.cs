using System;
using System.Runtime.Serialization;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sandbox.Console;


public class TestJsonConverter
{
    public static void Run()
    {
        var serializer = new NewtonsoftJsonSerializer();

        var input = new Dictionary<string, object?>();
        input.Add("num", 123);
        input.Add("enum", Test.MotherFucker);

        var request = new GraphQLRequest
        {
            Query = "query",
            Variables = input
        };

        var json = serializer.SerializeToString(request);
        System.Console.WriteLine(json);
    }
}


[JsonConverter(typeof(StringEnumConverter))]
public enum Test
{
    [EnumMember(Value = "one")]
    One,
    [EnumMember(Value = "two")]
    Two,
    [EnumMember(Value = "motherFucker")]
    MotherFucker
}