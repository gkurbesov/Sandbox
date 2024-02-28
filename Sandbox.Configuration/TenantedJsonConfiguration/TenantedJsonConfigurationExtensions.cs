using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace Sandbox.Configuration.JsonTenantedConfiguration;

public static class TenantedJsonConfigurationExtensions
{
    public static IConfigurationBuilder AddTenantedJsonFile(this IConfigurationBuilder builder,
        string path,
        string tenant)
    {
        return AddTenantedJsonFile(builder,
            provider: null,
            path: path,
            tenant: tenant, optional: false,
            reloadOnChange: false);
    }

    public static IConfigurationBuilder AddTenantedJsonFile(this IConfigurationBuilder builder,
        string path,
        string tenant,
        bool optional)
    {
        return AddTenantedJsonFile(builder, provider: null, path: path, tenant: tenant, optional: optional,
            reloadOnChange: false);
    }

    public static IConfigurationBuilder AddTenantedJsonFile(this IConfigurationBuilder builder,
        string path,
        string tenant,
        bool optional,
        bool reloadOnChange)
    {
        return AddTenantedJsonFile(builder, provider: null, path: path, tenant: tenant, optional: optional,
            reloadOnChange: reloadOnChange);
    }

    public static IConfigurationBuilder AddTenantedJsonFile(this IConfigurationBuilder builder,
        IFileProvider? provider,
        string path,
        string tenant,
        bool optional,
        bool reloadOnChange)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Invalid file path", nameof(path));
        }

        return builder.AddTenantedJsonFile(s =>
        {
            s.Tenant = tenant;
            s.FileProvider = provider;
            s.Path = path;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
            s.ResolveFileProvider();
        });
    }

    private static IConfigurationBuilder AddTenantedJsonFile(this IConfigurationBuilder builder,
        Action<TenantedJsonConfigurationSource>? configureSource)
        => builder.Add(configureSource);

    public static IConfigurationBuilder AddJsonStream(this IConfigurationBuilder builder, Stream stream)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.Add<JsonStreamConfigurationSource>(s => s.Stream = stream);
    }
}