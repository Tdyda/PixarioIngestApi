using Pixario.Ingest.Application.Messages;
using Pixario.Ingest.Application.Ports;
using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Application.UseCases;

public class CreateUploadJob(IFileStorage fileStorage, IQueuePublisher publisher, IJobRepository jobs)
{
    public async Task<Guid> ExecuteAsync(IEnumerable<(Stream Content, string FileName, long Size)> files, CancellationToken ct)
    {
        var jobId = Guid.NewGuid();
        var job = new UploadJob(jobId);

        var storedFiles = new List<string>();

        foreach (var file in files)
        {
            var path = await fileStorage.SaveAsync(file.Content, file.FileName, jobId, ct);

            job.AddImage(new ImageAsset(Guid.NewGuid(), file.FileName, path, file.Size));
            
            storedFiles.Add(path);
        }
        
        await jobs.SaveAsync(job, ct);
        
        await publisher.PublishAsync(new ImageProcessingMessage
        {
            JobId = jobId,
            Files = storedFiles
        }, ct);
        
        return jobId;
    }
}