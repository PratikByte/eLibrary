using BookEntity = eLibrary.Book.Domain.Entities.Book;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Book.Infrastructure.Persistence;

/// <summary>
/// Database context owned by the Book Service.
/// It manages only the Book table and keeps the service isolated from the rest of the monolith.
/// </summary>
public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
    {
    }

    public DbSet<BookEntity> Books => Set<BookEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired(false);
            entity.Property(x => x.Author).IsRequired(false);
            entity.Property(x => x.Price).IsRequired();
            entity.Property(x => x.AvailableCount).IsRequired();
        });
    }
}
