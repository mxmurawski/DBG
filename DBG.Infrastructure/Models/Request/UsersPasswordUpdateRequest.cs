namespace DBG.Infrastructure.Models.Request;

public class UsersPasswordUpdateRequest
{
    public required string Password { get; set; } = string.Empty;
}