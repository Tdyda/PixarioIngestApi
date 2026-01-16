using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Application.Ports;
using Pixario.Ingest.Infrastructure.Messaging;
using Pixario.Ingest.Infrastructure.Persistence;
using Pixario.Ingest.Infrastructure.Pipeline;
using Pixario.Ingest.Worker;
using Pixario.Ingest.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var cs = builder.Configuration.GetConnectionString("IngestDb");
builder.Services.AddDbContext<IngestDbContext>(opt =>
{
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

builder.Services.AddScoped<IJobRepository, EfJobRepository>();

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

builder.Services.AddHostedService<ImageProcessingConsumer>();

builder.Services.Configure<PythonPipelineOptions>(builder.Configuration.GetSection("Pipeline"));
builder.Services.AddSingleton<IImageProcessingPipeline, PythonScriptPipeline>();


var host = builder.Build();
host.Run();
