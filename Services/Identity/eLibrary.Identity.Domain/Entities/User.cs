using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Identity.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    // PostgreSQL stores only this MongoDB GridFS document id; image bytes stay in MongoDB.
    public string? ProfileImageMongoId { get; set; }
}
