#region

using DBG.Infrastructure.Enums;

#endregion

namespace DBG.Infrastructure.Models.Dto;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required UserRole Role { get; set; }
}