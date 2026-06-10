using Microsoft.EntityFrameworkCore;
using Persistence.Models;

namespace Persistence.Context;

public class AppDbContext : DbContext
{
      public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<PostItem> PostItems => Set<PostItem>();
    
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Color> Colors => Set<Color>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.Embedding)
                .HasColumnType("vector(768)");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasIndex(x => x.Value)
                .IsUnique();
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Description);
            
            entity.HasOne(x => x.Brand)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.BrandId)  
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Color)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ColorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.ToTable("PostTags");

            entity.HasKey(x => new { x.PostId, x.TagId });

            entity.HasOne(x => x.Post)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Tag)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostItem>(entity =>
        {
            entity.ToTable("PostItems");

            entity.HasKey(x => new { x.PostId, x.ItemId });

            entity.HasOne(x => x.Post)
                .WithMany(x => x.PostItems)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Item)
                .WithMany(x => x.PostItems)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.ToTable("Colors");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Value).IsRequired().HasMaxLength(100);
        });
    }
}