using Microsoft.EntityFrameworkCore;
using Pixario.Ingest.Core.Entities;

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

            b.Navigation(x => x.Images)
                .HasField("_images")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ImageAsset>(b =>
        {
            b.ToTable("image_assets");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.FileName).HasColumnName("file_name");
            b.Property(x => x.StoragePath).HasColumnName("storage_path");
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