using System.Text.Json;

namespace Sandbox.Configuration.JsonTenantedConfiguration;

public class TenantedJsonConfigurationSource : FileConfigurationSource
{
    public string? Tenant { get; set; }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new TenantedJsonConfigurationProvider(this);
    }
}

public class TenantedJsonConfigurationProvider : FileConfigurationProvider
{
    private readonly TenantedJsonConfigurationSource _source;

    public TenantedJsonConfigurationProvider(TenantedJsonConfigurationSource source) : base(source)
    {
        _source = source;
    }

    public override void Load(Stream stream)
    {
        try
        {
            Data = TenantedJsonConfigurationFileParser.Parse(stream,
                _source.Tenant ?? throw new InvalidOperationException("Tenant is not set"));
        }
        catch (JsonException e)
        {
            throw new FormatException("JSONParseError", e);
        }
    }
}