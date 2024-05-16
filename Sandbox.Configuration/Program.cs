#pragma warning disable VSTHRD200
using Microsoft.Extensions.Options;
using Sandbox.Configuration;
using Sandbox.Configuration.Multitenancy;
using Sandbox.Configuration.Multitenancy.Services;

/*
var globalServices = new ServiceCollection();

var builder = new ConfigurationBuilder();
var mainConfiguration = builder
    .AddJsonFile("appsettings.json")
    .Build();

globalServices.AddSingleton<IConfiguration>(mainConfiguration);
globalServices.AddSingleton<IGlobalServiceResolver>(sp => new GlobalServiceResolver(globalServices, sp));
globalServices.AddSingleton<ITenantServicesConfigurator, TenantServicesConfigurator>();
globalServices.AddSingleton<ITenantContainerFactory, TenantContainerFactory>();
globalServices.AddSingleton<IGlobalService, GlobalService>();

var mainServiceProvider = globalServices.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true
});


var tenantContainerFactory = mainServiceProvider.GetRequiredService<ITenantContainerFactory>();

ExampleForTenant(tenantContainerFactory, "Test");
ExampleForTenant(tenantContainerFactory, "Staging");


void ExampleForTenant(ITenantContainerFactory tenantContainerFactory, string tenantId)
{
    var tenantContainer = tenantContainerFactory.GetOrCreateTenantContainer(tenantId);

    var tenantedService = tenantContainer.GetTenantProvider().GetRequiredService<ITenantedService>();
    Console.WriteLine(tenantedService.GetStateValue());

    var globalServiceForTenant = tenantContainer.GetTenantProvider().GetRequiredService<IGlobalService>();
    Console.WriteLine($"{tenantContainer.TenantId}: {globalServiceForTenant.GetGlobalValue()}");

    var tenantOptionsPrinter = tenantContainer.GetTenantProvider().GetRequiredService<ITenantOptionsPrinter>();
    tenantOptionsPrinter.PrintOptions();

    Console.WriteLine("\r\n\r\n");
}
*/
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<ApplicationOptions>()
            .Bind(context.Configuration.GetSection(ApplicationOptions.SectionName));
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();

/*
var builder = new ConfigurationBuilder();

var mainConfiguration = builder
    .AddJsonFile("appsettings.json")
    .Build();

var tenantTestConfiguration = new ConfigurationBuilder()
    .AddConfiguration(mainConfiguration)
    .AddTenantedJsonFile("tenants.appsettings.json", "Test")
    .Build();


var standardTestConfiguration = new ConfigurationBuilder()
    .AddConfiguration(mainConfiguration)
    .AddTenantedJsonFile("tenants.appsettings.json", "Standard")
    .Build();

var testOptions = tenantTestConfiguration.GetSection(ApplicationOptions.SectionName).Get<ApplicationOptions>();
var standardOptions = standardTestConfiguration.GetSection(ApplicationOptions.SectionName).Get<ApplicationOptions>();


ShowApplicationInfo(testOptions);
ShowApplicationInfo(standardOptions);

void ShowApplicationInfo(ApplicationOptions options)
{
    Console.WriteLine($"ProjectId: {options.ProjectId}");
    Console.WriteLine($"Project name: {options.SystemName}");
    Console.WriteLine($"Deployment: {options.Deployment}");
}
*/
#pragma warning restore VSTHRD200
