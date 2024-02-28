using Sandbox.Configuration;
using Sandbox.Configuration.JsonTenantedConfiguration;

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

var builder = new ConfigurationBuilder();

var configuration = builder
    .AddJsonFile("appsettings.json")
    .AddTenantedJsonFile("tenants.appsettings.json", "Test")
    .Build();

var options = configuration.GetSection(ApplicationOptions.SectionName).Get<ApplicationOptions>();

Console.WriteLine($"ProjectId: {options.ProjectId}");
Console.WriteLine($"Project name: {options.SystemName}");
Console.WriteLine($"Deployment: {options.Deployment}");


