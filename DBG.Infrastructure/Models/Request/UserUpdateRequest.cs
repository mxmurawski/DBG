#region

using DBG.Infrastructure.Enums;

#endregion

namespace DBG.Infrastructure.Models.Request;

public class UserUpdateRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
    public UserRole? Role { get; set; }
}