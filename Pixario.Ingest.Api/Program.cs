using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Api.Security;
using Pixario.Ingest.Application.Ports;
using Pixario.Ingest.Application.UseCases;
using Pixario.Ingest.Infrastructure;
using Pixario.Ingest.Infrastructure.Messaging;
using Pixario.Ingest.Infrastructure.Persistence;
using Pixario.Ingest.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var cs = builder.Configuration.GetConnectionString("IngestDb");
builder.Services.AddDbContext<IngestDbContext>(opt =>
{
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

builder.Services.Configure<ApiKeyOptions>(
    builder.Configuration.GetSection(ApiKeyOptions.SectionName));

builder.Services.AddScoped<IJobRepository, EfJobRepository>();
builder.Services.AddSingleton<IFileStorage>(_ => new LocalFileStorage("uploads"));
builder.Services.AddScoped<CreateUploadJob>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IQueuePublisher, RabbitMqPublisher>();
builder.Services.AddTransient<ApiKeyMiddleware>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapPost("/v1/uploads/images", async (
    HttpRequest request,
    CreateUploadJob useCase,
    CancellationToken ct) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Expected multipart/form-data");

    var files = request.Form.Files
        .Select(f => (f.OpenReadStream(), f.FileName, f.Length));

    var jobId = await useCase.ExecuteAsync(files, ct);

    return Results.Accepted($"/v1/jobs/{jobId}", new { jobId });
});

app.MapGet("/v1/jobs/{jobId:guid}", async (
    Guid jobId,
    IJobRepository repo,
    CancellationToken ct) =>
{
    var job = await repo.GetAsync(jobId, ct);
    if (job is null) return Results.NotFound();

    return Results.Ok(new
    {
        jobId = job.JobId,
        status = job.Status.ToString().ToLowerInvariant(),
        images = job.Images.Select(i => new
        {
            imageId = i.ImageId,
            fileName = i.FileName,
            storagePath = i.StoragePath,
            size = i.Size
        })
    });
});

app.Run();
