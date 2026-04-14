namespace Pixario.Ingest.Application.Ports.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}