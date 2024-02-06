namespace DBG.Infrastructure.Models.Helper;

public class AuthenticatedResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}