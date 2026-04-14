using Pixario.Ingest.Core.Enums;

namespace Pixario.Ingest.Infrastructure.Persistence.Models;

public class LogLevelEntity
{
    public int Id { get; set; }
    public LogLevelValue LogLevel{ get; init; }
    public bool IsActive { get; set; }
}