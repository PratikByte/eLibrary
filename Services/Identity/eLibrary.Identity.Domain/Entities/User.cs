using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Identity.Domain.Entities;

public class User : BaseEntity
{
    [NotMapped]
    public Guid UserId
    {
        get => Id;
        set => Id = value;
    }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? ProfileImagePath { get; set; }
}
