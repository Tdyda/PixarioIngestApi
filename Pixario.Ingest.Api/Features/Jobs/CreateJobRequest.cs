namespace Pixario.Ingest.Api.Features.Jobs;

public sealed record CreateJobRequest(IEnumerable<UploadFile> Files)
{
    public static async ValueTask<CreateJobRequest?> BindAsync(HttpContext context)
    {
        var form = await context.Request.ReadFormAsync();

        var files = form.Files.Select(f => new UploadFile(
            f.OpenReadStream(),
            f.FileName,
            f.Length
        ));

        return new CreateJobRequest(files);
    }
}