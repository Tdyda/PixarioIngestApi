using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Core.Entities;

namespace Pixario.Ingest.Infrastructure.Persistence;

public class IngestDbContext : DbContext
{
    public IngestDbContext(DbContextOptions<IngestDbContext> options) : base(options) { }
    
    public DbSet<UploadJob> UploadJobs => Set<UploadJob>();
    public DbSet<ImageAsset> ImageAssets => Set<ImageAsset>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UploadJob>(b =>
        {
            b.ToTable("upload_jobs");
            b.HasKey(x => x.JobId);

            b.Property(x => x.JobId).HasColumnName("job_id");
            b.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            
            b.HasMany(x => x.Images)
                .WithOne()
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.Images)
                .HasField("_images")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ImageAsset>(b =>
        {
            b.ToTable("image_assets");
            b.HasKey(x => x.ImageId);

            b.Property(x => x.ImageId).HasColumnName("image_id");
            b.Property(x => x.JobId).HasColumnName("job_id");
            b.Property(x => x.FileName).HasColumnName("file_name");
            b.Property(x => x.StoragePath).HasColumnName("storage_path");
            b.Property(x => x.Size).HasColumnName("size");
        });
    }
}