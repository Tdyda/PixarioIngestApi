using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using Pixario.Ingest.Infrastructure.Persistence;
using Pixario.Ingest.Worker;
using Pixario.Ingest.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var cs = builder.Configuration.GetConnectionString("IngestDb");
builder.Services.AddDbContext<IngestDbContext>(opt => { opt.UseMySql(cs, ServerVersion.AutoDetect(cs)); });

builder.Services.ConfigureServices(builder.Configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<RabbitMqOptions>>();
    var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqConnection>();

    await using var channel = await rabbit.Connection.CreateChannelAsync();
    await RabbitMqTopology.EnsureCreatedAsync(channel, options.CurrentValue);
}

host.Run();