using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Infrastructure.Persistence.Models;

namespace Pixario.Ingest.Infrastructure.Persistence;

public class IngestDbContext(DbContextOptions<IngestDbContext> options) : DbContext(options)
{
    public DbSet<ImageRetouchBatch> ImageRetouchBatches => Set<ImageRetouchBatch>();
    public DbSet<ImageAsset> ImageAssets => Set<ImageAsset>();
    public DbSet<ImageRetouchJob> ImageRetouchJobs => Set<ImageRetouchJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImageRetouchBatch>(b =>
        {
            b.ToTable("retouch_batch");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasMany(x => x.Images)
                .WithOne()
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Jobs)
                .WithOne()
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ImageAsset>(b =>
        {
            b.ToTable("image_assets");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(255);
            b.Property(x => x.StoredFileName).HasColumnName("stored_file_name").HasMaxLength(255);
            b.Property(x => x.StoragePath).HasColumnName("storage_path").HasMaxLength(1024);
            b.Property(x => x.Size).HasColumnName("size");
            b.Property(x => x.BatchId).HasColumnName("batch_id");
        });

        modelBuilder.Entity<ImageRetouchJob>(b =>
        {
            b.ToTable("retouch_jobs");

            b.Property(x => x.BatchId).HasColumnName("batch_id");

            b.Property(e => e.Status).HasConversion<string>();

            b.HasOne(x => x.Image)
                .WithOne()
                .HasForeignKey<ImageRetouchJob>(x => x.ImageId);
        });
    }
}