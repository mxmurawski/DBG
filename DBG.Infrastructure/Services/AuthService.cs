#region

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using Microsoft.IdentityModel.Tokens;

#endregion

namespace DBG.Infrastructure.Services;

public class AuthService
    : IAuthService
{
    private readonly IJwtConfiguration jwtConfiguration;
    private readonly IPersistenceService persistenceService;

    public AuthService(
        IPersistenceService persistenceService,
        IJwtConfiguration jwtConfiguration)
    {
        this.persistenceService = persistenceService;
        this.jwtConfiguration = jwtConfiguration;
    }

    public async Task<AuthenticatedResponse> AuthenticateAsync(Login login)
    {
        var user = await persistenceService.GetUserAsync(login);
        return user is not null
            ? await GenerateAuthenticatedResponseAsync(user)
            : throw new Exception("Invalid credentials or user not found.");
    }

    public async Task<AuthenticatedResponse> GenerateAuthenticatedResponseAsync(User user)
    {
        SymmetricSecurityKey key = new(Encoding.ASCII.GetBytes(jwtConfiguration.Secret));
        SigningCredentials cred = new(key, SecurityAlgorithms.HmacSha512);
        Claim[] claims =
        {
            new("Id", user.Id.ToString()),
            new("Name", user.FirstName + " " + user.LastName),
            new("Email", user.Email),
            new("Role", user.Role.ToString())
        };
        JwtSecurityToken token = new(claims: claims, expires: DateTime.Now.AddMinutes(5), signingCredentials: cred);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(30);
        user.UpdatedOn = DateTime.Now;
        _ = await persistenceService.UpdateUserAsync(user);
        return new AuthenticatedResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }


    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public Guid GetUserIdFromExpiredToken(string token)
    {
        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration.Secret)),
            ValidateLifetime = false
        };
        JwtSecurityTokenHandler tokenHandler = new();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        return securityToken is not JwtSecurityToken
            ? throw new Exception("Invalid token")
            : Guid.Parse(principal.FindFirst("Id")?.Value ?? "");
    }

    public async Task<AuthenticatedResponse> RefreshAsync(AuthenticatedResponse tokenModel)
    {
        var g = GetUserIdFromExpiredToken(tokenModel.AccessToken);
        var u = await persistenceService.GetUserAsync(g);
        if (u is null || u.RefreshToken != tokenModel.RefreshToken || u.RefreshTokenExpiryTime <= DateTime.Now)
            throw new Exception("Unable to refresh token.");
        var newAuthenticatedResponse = await GenerateAuthenticatedResponseAsync(u);
        return newAuthenticatedResponse;
    }

    public async Task<bool> UpdateMyPasswordAsync(string token, string password)
    {
        var g = GetUserIdFromExpiredToken(token);
        var u = await persistenceService.GetUserAsync(g);
        if (u is null) throw new Exception("User not found.");
        if (!PersistenceService.IsPasswordComplex(password)) throw new Exception("Password is not complex enough.");

        u.Password = PersistenceService.ComputeSha512Hash(password);
        u.UpdatedOn = DateTime.Now;
        _ = await persistenceService.UpdateUserAsync(u);
        return true;

    }
}