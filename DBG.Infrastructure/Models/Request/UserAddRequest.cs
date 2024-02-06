#region

using DBG.Infrastructure.Enums;

#endregion

namespace DBG.Infrastructure.Models.Request;

public class UserAddRequest
{
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Password { get; set; }
    public required UserRole Role { get; set; }
}