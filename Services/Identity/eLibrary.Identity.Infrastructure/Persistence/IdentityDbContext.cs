using eLibrary.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace eLibrary.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Ignore(x => x.UserId);
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Username).IsRequired().HasMaxLength(100);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).IsRequired();
        });

        modelBuilder.Entity<OtpRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.Code).IsRequired();
            entity.Property(x => x.ExpiryTime).IsRequired();
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
