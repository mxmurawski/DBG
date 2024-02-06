#region

using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;

#endregion

namespace DBG.Infrastructure.Interfaces;

public interface IAuthService
{
    Task<bool> UpdateMyPasswordAsync(string token, string password);
    Task<AuthenticatedResponse> AuthenticateAsync(Login login);
    Task<AuthenticatedResponse> GenerateAuthenticatedResponseAsync(User user);
    string GenerateRefreshToken();
    Guid GetUserIdFromExpiredToken(string token);
    Task<AuthenticatedResponse> RefreshAsync(AuthenticatedResponse tokenModel);
}