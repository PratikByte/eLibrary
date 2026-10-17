using BookEntity = eLibrary.Book.Domain.Entities.Book;
using ActivityEventEntity = eLibrary.Book.Domain.Entities.ActivityEvent;
using BorrowRecordEntity = eLibrary.Book.Domain.Entities.BorrowRecord;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using eLibrary.Book.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace eLibrary.Book.Infrastructure.Persistence;

/// <summary>
/// Database context owned by the Book Service.
/// It manages only the Book table and keeps the service isolated from the rest of the monolith.
/// </summary>
public class BookDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BookDbContext(DbContextOptions<BookDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<BookEntity> Books => Set<BookEntity>();
    public DbSet<ActivityEventEntity> ActivityEvents => Set<ActivityEventEntity>();
    public DbSet<BorrowRecordEntity> BorrowRecords => Set<BorrowRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Title).IsRequired(false);
            entity.Property(x => x.Author).IsRequired(false);
            entity.Property(x => x.Price).IsRequired();
            entity.Property(x => x.AvailableCount).IsRequired();
            entity.Property(x => x.CoverImageMongoId).HasMaxLength(100);
        });

        modelBuilder.Entity<BorrowRecordEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Ignore(x => x.UserName);
            entity.Ignore(x => x.BookName);
            entity.Property(x => x.Fine).HasPrecision(10, 2);
            entity.HasIndex(x => new { x.UserId, x.BookId, x.IsReturned });
        });

        modelBuilder.Entity<ActivityEventEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BookName).IsRequired();
            entity.HasIndex(x => new { x.EventType, x.SubjectId }).IsUnique();
            entity.HasIndex(x => x.OccurredAt);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;
        var actor = Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId : (Guid?)null;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
                entry.Entity.CreatedBy = actor;
                entry.Entity.UpdatedBy = actor;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Property(entity => entity.CreatedBy).IsModified = false;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = actor;
                if (entry.Entity.IsDeleted && !entry.OriginalValues.GetValue<bool>(nameof(BaseEntity.IsDeleted)))
                {
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = actor;
                }
            }
        }
    }
}
