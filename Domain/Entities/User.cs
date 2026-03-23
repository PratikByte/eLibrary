using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Domain.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string? Email { get; set; } 

    [Required, MaxLength(50)]
    public string? Username { get; set; }

    [Required]
    public string PasswordHash { get; set; } 

    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ✅ Optional profile image
    public string? ProfileImagePath { get; set; }

    // Navigation property - user can have many borrow records
    public ICollection<BorrowRecords> BorrowRecords { get; set; } = new List<BorrowRecords>();


}

