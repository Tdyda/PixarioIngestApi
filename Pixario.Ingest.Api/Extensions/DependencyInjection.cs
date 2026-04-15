using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Api.HostedServices;
using Pixario.Ingest.Api.OpenApi;
using Pixario.Ingest.Api.Security;
using Pixario.Ingest.Application.Features.Api.Jobs.CreateBatch;
using Pixario.Ingest.Application.Features.Logging.Get;
using Pixario.Ingest.Application.Ports.Logging;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Publishing;
using Pixario.Ingest.Infrastructure.Integrations.Storage;
using Pixario.Ingest.Infrastructure.Integrations.Storage.Configuration;
using Pixario.Ingest.Infrastructure.Persistence;
using Pixario.Ingest.Infrastructure.Persistence.Repositories;

namespace Pixario.Ingest.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddCustomOpenApi();

        var cs = configuration.GetConnectionString("IngestDb");
        services.AddDbContext<IngestDbContext>(opt => { opt.UseMySql(cs, ServerVersion.AutoDetect(cs)); });

        services.Configure<ApiKeyOptions>(configuration.GetSection(ApiKeyOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorageOps"));

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<CreateJobCommandHandler>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILogLevelRepository, LogLevelRepository>();
        services.AddScoped<GetLogLevelHandler>();

        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddSingleton<IProcessBatchPublisher, ProcessBatchPublisher>();
        services.AddSingleton<LogLevelService>();

        services.AddTransient<ApiKeyMiddleware>();

        services.AddHostedService<LogLevelWatcher>();

        return services;
    }
}