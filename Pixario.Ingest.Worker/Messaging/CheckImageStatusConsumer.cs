using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Exceptions;
using Pixario.Ingest.Application.Features.Worker.ImageProcessing;
using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Configuration;
using Pixario.Ingest.Infrastructure.Integrations.RabbitMq.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pixario.Ingest.Worker.Messaging;

public sealed class CheckImageStatusConsumer(
    IServiceScopeFactory scopeFactory,
    IRabbitMqConnection conn,
    IOptionsMonitor<RabbitMqOptions> opt,
    ILogger<CheckImageStatusConsumer> log)
    : BackgroundService
{
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _channel = await conn.Connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.BasicQosAsync(0, 1, false, ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<CheckImageStatusMessage>(json);

                if (msg is null)
                    throw new PermanentProcessingException("Invalid completed message payload");

                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<CheckImageStatusHandler>();
                var isSuccess = await handler.Handle(msg, ct);

                if (!isSuccess)
                {
                    log.LogWarning("Job is still processing...");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, ct);
                    return;
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (PermanentProcessingException ex)
            {
                log.LogError(ex, "Permanent failure (notifier) -> DLQ");

                await PublishDlqAsync(ea.Body, ct);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Transient failure (notifier) -> retry");

                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, ct);
            }
        };

        await _channel.BasicConsumeAsync(
            opt.CurrentValue.ImageStatusCheckQueue,
            false,
            consumer,
            ct);
    }

    // private async Task HandleAsync(CheckImageStatusMessage msg, CancellationToken ct,
    //     IJobRepository jobRepository, IProcessBatchPublisher publisher)
    // {
    //     if (string.IsNullOrWhiteSpace(cb.CurrentValue.BaseUrl))
    //         throw new PermanentProcessingException("Callbacks:BaseUrl is not configured");
    //
    //     if (string.IsNullOrWhiteSpace(cb.CurrentValue.EndpointPath))
    //         throw new PermanentProcessingException("Callbacks:EndpointPath is not configured");
    //
    //     msg.Job.MarkProcessing();
    //     await jobRepository.UpdateAsync(msg.Job, ct);
    //
    //     var historyClient = httpClientFactory.CreateClient("history");
    //
    //     JsonDocument? historyDoc = null;
    //
    //     while (!historyDoc.HasNonEmptyOutputs())
    //     {
    //         await Task.Delay(10000, ct);
    //         historyDoc = await GetHistoryAsync(msg.PromptId, historyClient, ct);
    //     }
    //
    //     var root = historyDoc!.RootElement;
    //
    //     var outputs = root.GetSection($"{msg.PromptId}__outputs");
    //
    //     var firstOutput = outputs?.EnumerateObject().First().Value;
    //
    //     var fileName = firstOutput?
    //         .GetSection("images__0__filename")
    //         ?.GetString();
    //
    //     msg.Job.MarkDone();
    //     await jobRepository.UpdateAsync(msg.Job, ct);
    //
    //     await publisher.PublishAsync(new ProcessBatchMessage
    //     {
    //         BatchId = msg.BatchId
    //     }, ct);
    //
    //
    //     
    //     historyDoc!.RootElement.TryGetProperty("outputs", out var output);
    //     log.LogInformation("OUTPUT: {Output}", output);
    //
    //
    //     foreach (var p in msg.OutputFiles)
    //     {
    //         if (!File.Exists(p))
    //             throw new PermanentProcessingException($"Output file not found: {p}");
    //     }
    //     
    //     var url = $"{cb.BaseUrl.TrimEnd('/')}/{cb.EndpointPath.TrimStart('/')}";
    //     
    //     using var form = new MultipartFormDataContent();
    //     
    //     form.Add(new StringContent(StaticUrlToken), "url");
    //     
    //     var streams = new List<Stream>();
    //     try
    //     {
    //         foreach (var filePath in msg.OutputFiles)
    //         {
    //             var stream = File.OpenRead(filePath);
    //             streams.Add(stream);
    //     
    //             var fileContent = new StreamContent(stream);
    //             form.Add(fileContent, "photo[]", Path.GetFileName(filePath));
    //         }
    //     
    //         var client = httpClientFactory.CreateClient("callbacks");
    //     
    //         using var req = new HttpRequestMessage(HttpMethod.Post, url);
    //         req.Content = form;
    //     
    //         if (!string.IsNullOrWhiteSpace(cb.ApiKey))
    //             // req.Headers.TryAddWithoutValidation("X-API-KEY", _cb.ApiKey);
    //             req.Headers.Add("Cookie", $"x-api-key={cb.ApiKey}");
    //     
    //         log.LogInformation(
    //             "Sending callback (photos={Count}) -> {Url}",
    //             msg.OutputFiles.Count,
    //             url);
    //     
    //         using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
    //     
    //         if (resp.IsSuccessStatusCode)
    //         {
    //             log.LogInformation("Callback OK. Status={StatusCode}", (int)resp.StatusCode);
    //             return;
    //         }
    //     
    //         var body = await SafeReadBodyAsync(resp, ct);
    //         
    //         if (IsPermanentHttpFailure(resp.StatusCode))
    //             throw new PermanentProcessingException(
    //                 $"Callback returned {(int)resp.StatusCode} {resp.ReasonPhrase}. Body={body}");
    //         
    //         throw new Exception(
    //             $"Callback returned {(int)resp.StatusCode} {resp.ReasonPhrase}. Body={body}");
    //     }
    //     finally
    //     {
    //         foreach (var s in streams)
    //             await s.DisposeAsync();
    //     }
    // }

    private static bool IsPermanentHttpFailure(HttpStatusCode code)
    {
        var n = (int)code;

        if (n is >= 400 and <= 499)
            return code is not HttpStatusCode.RequestTimeout
                   && code is not HttpStatusCode.TooManyRequests;

        return false;
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var text = await resp.Content.ReadAsStringAsync(ct);
            if (text.Length > 2000) text = text[..2000] + "...";
            return text;
        }
        catch
        {
            return "<unreadable>";
        }
    }

    private async Task PublishDlqAsync(ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        await using var ch = await conn.Connection.CreateChannelAsync(cancellationToken: ct);

        var props = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>
            {
                ["dlq_reason"] = "notifier_permanent_failure"
            }
        };

        await ch.BasicPublishAsync(
            opt.CurrentValue.Exchange,
            opt.CurrentValue.ImageStatusCheckDlqRoutingKey,
            false,
            props,
            body,
            ct);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}