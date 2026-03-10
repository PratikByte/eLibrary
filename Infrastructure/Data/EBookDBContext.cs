using eLibrary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Infrastructure.Data;

public class EBookDBContext : DbContext
{

    public EBookDBContext(DbContextOptions<EBookDBContext> options) : base(options) { }
    public DbSet<Book> Books { get; set; } // Book table (Books plural)
    public DbSet<User> Users { get; set; }
    public DbSet<BorrowRecords> BorrowRecords { get; set; }

    // DbSet for OTP records
    public DbSet<OtpRecord> OtpRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User -> BorrowRecord (1-to-many)
        modelBuilder.Entity<BorrowRecords>()
            .HasOne(br => br.User)
            .WithMany(u => u.BorrowRecords)
            .HasForeignKey(br => br.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // BookId is just a reference, no FK to Books table
        modelBuilder.Entity<BorrowRecords>()
            .Property(br => br.BookId)
            .IsRequired();

        // Decimal precision for Fine
        modelBuilder.Entity<BorrowRecords>()
            .Property(br => br.Fine)
            .HasPrecision(10, 2);


        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId); // Every Avenger must have a unique ID (Primary Key)

            entity.HasMany(u => u.BorrowRecords)          // Tony owns many suits
                  .WithOne(t => t.User!)          // Each suit belongs to one Tony
                  .HasForeignKey(t => t.UserId)   // The link: UserId in suits table
                  .OnDelete(DeleteBehavior.Cascade); // If Tony dies, his suits are destroyed
        });

    }



}

