#region

using DBG.Infrastructure.Interfaces;

#endregion

namespace DBG.Infrastructure.Models.Helper;

public class JwtConfiguration : IJwtConfiguration
{
    public string Secret { get; set; } = string.Empty;
}