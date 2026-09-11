using BookEntity = eLibrary.Book.Domain.Entities.Book;
using BorrowRecordEntity = eLibrary.Book.Domain.Entities.BorrowRecord;
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
    public DbSet<BorrowRecordEntity> BorrowRecords => Set<BorrowRecordEntity>();

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

        modelBuilder.Entity<BorrowRecordEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Ignore(x => x.UserName);
            entity.Ignore(x => x.BookName);
            entity.Property(x => x.Fine).HasPrecision(10, 2);
            entity.HasIndex(x => new { x.UserId, x.BookId, x.IsReturned });
        });
    }
}
