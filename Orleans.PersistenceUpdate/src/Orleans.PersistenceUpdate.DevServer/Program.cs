
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

var builder = Host.CreateDefaultBuilder(args);
builder.UseOrleans((hostContext, siloBuilder) =>
{
	siloBuilder
		.UseAdoNetClustering(o =>
		{
			o.Invariant = "MySql.Data.MySqlClient";
			o.ConnectionString = "server=localhost;database=orleansIterator;user=root;password=unsecure1Admin";
		})
		.AddAdoNetGrainStorage("STORE_NAME", o =>
		{
			o.Invariant = "MySql.Data.MySqlClient";
			o.ConnectionString = "server=localhost;database=orleansIterator;user=root;password=unsecure1Admin";
		})
		;

	siloBuilder
		.Configure<ClusterOptions>(o =>
		{
			o.ClusterId = "updater";
			o.ServiceId = "tester";
		});

});

var host = builder.Build();
await host.RunAsync();
