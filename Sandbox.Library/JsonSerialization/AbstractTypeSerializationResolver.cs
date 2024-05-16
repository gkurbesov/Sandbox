namespace Sandbox.Library.JsonSerialization;

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