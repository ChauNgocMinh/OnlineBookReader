using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineBookReader.Models;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<ChapterContent> ChapterContents { get; set; }
    public DbSet<ChapterImage> ChapterImages { get; set; }
    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.Now;
                entry.Entity.CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                entry.Entity.IsDeleted = false; 
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.Now;
                entry.Entity.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.Entity.IsDeleted = true;
                entry.State = EntityState.Modified; 
            }
        }
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.LastRead)
                  .WithMany() 
                  .HasForeignKey(u => u.LastReadId)
                  .IsRequired(false) 
                  .OnDelete(DeleteBehavior.SetNull); 
        });

        modelBuilder.Entity<Book>().HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<Author>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasMany(c => c.ChapterImages)
                .WithOne(c => c.Chapter)
                .HasForeignKey(c => c.ChapterId);

            entity.HasMany(c => c.ChapterContents)
                .WithOne(c => c.Chapter)
                .HasForeignKey(c => c.ChapterId);

            entity.HasOne(c => c.Book)
                .WithMany(c => c.Chapters)
                .HasForeignKey(c => c.BookId);
        });
        
        modelBuilder.Entity<ChapterContent>(entity =>
        {
            entity.HasKey(b => b.Id);
        });
        
        modelBuilder.Entity<ChapterImage>(entity =>
        {
            entity.HasKey(b => b.Id);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
        });
    }
}
