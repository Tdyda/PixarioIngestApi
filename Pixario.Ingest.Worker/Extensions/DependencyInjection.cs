using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Application.Ports.Integrations;
using Pixario.Ingest.Application.Ports.Messaging;
using Pixario.Ingest.Application.Ports.Repositories;
using Pixario.Ingest.Application.Ports.Storage;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Clients;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Http;
using Pixario.Ingest.Infrastructure.Integrations.ComfyUi.Workflows;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Builders;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Clients;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.Outbound.Http;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Publishing;
using Pixario.Ingest.Infrastructure.Integrations.Storage;
using Pixario.Ingest.Infrastructure.Integrations.Storage.Configuration;
using Pixario.Ingest.Infrastructure.Persistence.Repositories;
using Pixario.Ingest.Worker.Messaging;

namespace Pixario.Ingest.Worker.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<ProcessBatchHandler>();
        services.AddScoped<CheckImageStatusHandler>();
        services.AddScoped<BatchProcessedHandler>();

        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<ComfyUiConfig>(configuration.GetSection("ComfyUi"));
        services.Configure<PixarioOptions>(configuration.GetSection("Callbacks"));
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));

        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddSingleton<IImageProcessingGateway, ComfyUiProcessBatchGateway>();
        services.AddSingleton<ComfyUiWorkflowLoader>();
        services.AddSingleton<ComfyUiPromptPayloadBuilder>();
        services.AddSingleton<ComfyUiWorkflowImageBinder>();
        services.AddSingleton<ComfyUiPromptSender>();
        services.AddSingleton<ICheckImageProcessingStatusPublisher, CheckImageProcessingStatusPublisher>();
        services.AddSingleton<RabbitMqPublisherSupport>();
        services.AddSingleton<IProcessBatchPublisher, ProcessBatchPublisher>();
        services.AddSingleton<IBatchProcessedNotifier, BatchProcessedNotifier>();
        services.AddSingleton<IComfyUiCheckImageStatusGateway, ComfyUiCheckImageStatusGateway>();
        services.AddSingleton<ComfyUiPromptStatusSender>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<PixarioBatchProcessedPayloadBuilder>();
        services.AddSingleton<IPixarioBatchProcessedGateway, PixarioBatchProcessedGateway>();
        services.AddSingleton<PixarioBatchProcessedCallbackSender>();

        services.AddHostedService<ProcessBatchConsumer>();
        services.AddHostedService<CheckImageStatusConsumer>();
        services.AddHostedService<BatchProcessedConsumer>();

        services.AddHttpClient("callbacks");

        return services;
    }
}