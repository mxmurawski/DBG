#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DBG.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[Table("users", Schema = "core")]
[Index(nameof(Email), IsUnique = true)]
[PrimaryKey(nameof(Id))]
public class User
{
    public required Guid Id { get; set; }
    [StringLength(255)]
    public required string Email { get; set; }
    [StringLength(255)]
    public string? FirstName { get; set; }
    [StringLength(255)]
    public string? LastName { get; set; }
    [StringLength(255)]
    public required string Password { get; set; }
    public required UserRole Role { get; set; }
    [StringLength(512)]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}