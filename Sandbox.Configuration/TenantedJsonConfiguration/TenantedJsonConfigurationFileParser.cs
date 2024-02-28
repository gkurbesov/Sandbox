using System.Diagnostics;
using System.Text.Json;

namespace Sandbox.Configuration.JsonTenantedConfiguration;

public class TenantedJsonConfigurationFileParser
{
    private const string TenantRootKey = "Tenants";

    private TenantedJsonConfigurationFileParser()
    {
    }

    private readonly Dictionary<string, string?> _data =
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    private readonly Stack<string> _paths = new Stack<string>();

    public static IDictionary<string, string?> Parse(Stream input, string tenant)
        => new TenantedJsonConfigurationFileParser().ParseStream(input, tenant);

    private Dictionary<string, string?> ParseStream(Stream input, string tenant)
    {
        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        using (var reader = new StreamReader(input))
        using (JsonDocument doc = JsonDocument.Parse(reader.ReadToEnd(), jsonDocumentOptions))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("The top-level JSON element must be an object.");
            }
            
            if (!doc.RootElement.TryGetProperty(TenantRootKey, out JsonElement tenantElement))
            {
                throw new FormatException("The top-level JSON element must contain a `Tenants` property.");
            }
            
            if (!tenantElement.TryGetProperty(tenant, out JsonElement tenantRoot))
            {
                throw new FormatException($"The `Tenants` property must contain a `{tenant}` property.");
            }

            VisitObjectElement(tenantRoot);
        }

        return _data;
    }

    private void VisitObjectElement(JsonElement element)
    {
        var isEmpty = true;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            isEmpty = false;
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }

        SetNullIfElementIsEmpty(isEmpty);
    }

    private void VisitArrayElement(JsonElement element)
    {
        int index = 0;

        foreach (JsonElement arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToString());
            VisitValue(arrayElement);
            ExitContext();
            index++;
        }

        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }

    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && _paths.Count > 0)
        {
            _data[_paths.Peek()] = null;
        }
    }

    private void VisitValue(JsonElement value)
    {
        Debug.Assert(_paths.Count > 0);

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitObjectElement(value);
                break;

            case JsonValueKind.Array:
                VisitArrayElement(value);
                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                string key = _paths.Peek();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException("A duplicate key is not allowed.");
                }

                _data[key] = value.ToString();
                break;

            default:
                throw new FormatException("`JsonValueKind` is not supported.");
        }
    }

    private void EnterContext(string context) =>
        _paths.Push(_paths.Count > 0 ? _paths.Peek() + ConfigurationPath.KeyDelimiter + context : context);

    private void ExitContext() => _paths.Pop();
}