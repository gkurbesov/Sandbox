// See https://aka.ms/new-console-template for more information

#pragma warning disable VSTHRD200
using Ductus.FluentDocker.Builders;
using Sandbox.EfCore;

Console.WriteLine("Applicaiton started");


var unitofwork = new UnitOfWork();

var tenants = unitofwork.GetTenants();

/*
var tasks = new List<Task>();
foreach (var tenant in unitofwork.GetTenants())
{
    tasks.Add(unitofwork.DoWorkAsync(tenant));
}

await Task.WhenAll(tasks).ConfigureAwait(false);
*/

var builder = new ContainerManagerBuilder();
var port = 1430;
foreach (var tenant in tenants)
{
    builder.WithTenant(tenant, port++);
}

using var manager = builder.Build();

Console.WriteLine("Manager built");
manager.Start();


Console.ReadLine();


