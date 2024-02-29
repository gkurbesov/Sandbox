using Microsoft.Extensions.DependencyInjection.Extensions;
using Sandbox.Configuration;
using Sandbox.Configuration.JsonTenantedConfiguration;
using Sandbox.Configuration.Multitenancy;
using Sandbox.Configuration.Multitenancy.Services;

/*
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("");
    })
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .Build();

host.Run();
*/
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

var globalServices = new ServiceCollection();

globalServices.AddSingleton<IGlobalService, GlobalService>();
globalServices.AddSingleton<ITenantContainerFactory>(sp =>
    new TenantContainerFactory(globalServices, sp, new TenantServicesConfigurator()));

var mainServiceProvider = globalServices.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true
});
var tenantContainerFactory = mainServiceProvider.GetRequiredService<ITenantContainerFactory>();



var testTenant = tenantContainerFactory.GetTenantContainer("Test");

var testService = testTenant.GetTenantProvider().GetRequiredService<ITenantedService>();
Console.WriteLine(testService.GetStateValue());

var globalservoceForTest = testTenant.GetTenantProvider().GetRequiredService<IGlobalService>();
Console.WriteLine($"{testTenant.TenantId}: " + globalservoceForTest.GetGlobalValue() + "\r\r\r\n");



var stagingTenant = tenantContainerFactory.GetTenantContainer("Staging");

var stagingService = stagingTenant.GetTenantProvider().GetRequiredService<ITenantedService>();
Console.WriteLine(stagingService.GetStateValue());

var globalservoceForStaging = stagingTenant.GetTenantProvider().GetRequiredService<IGlobalService>();
Console.WriteLine($"{stagingTenant.TenantId}: " + globalservoceForStaging.GetGlobalValue());

