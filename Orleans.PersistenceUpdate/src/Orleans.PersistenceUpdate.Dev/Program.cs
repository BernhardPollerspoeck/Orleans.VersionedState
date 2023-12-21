


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.PersistenceUpdate.DevGrain;

var builder = Host.CreateDefaultBuilder(args);
builder.UseOrleansClient(clientBuilder =>
{
	clientBuilder.UseAdoNetClustering(options =>
	{
		options.Invariant = "MySql.Data.MySqlClient";
		options.ConnectionString = "server=localhost;database=orleansIterator;user=root;password=unsecure1Admin";
	});
	clientBuilder
	.Configure<ClusterOptions>(o =>
	{
		o.ClusterId = "updater";
		o.ServiceId = "tester";
	});
});


var host = builder.Build();
await host.StartAsync();

var grain = host
	.Services
	.GetRequiredService<IClusterClient>().GetGrain<ITestStateGrain>("1");

await grain.SetState("first", "last");


Console.WriteLine("Hello, World!");

