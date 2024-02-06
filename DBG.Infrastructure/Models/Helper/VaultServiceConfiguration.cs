#region

using DBG.Infrastructure.Interfaces;

#endregion

namespace DBG.Infrastructure.Models.Helper;

public class VaultServiceConfiguration : IVaultServiceConfiguration
{
    public string Address { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string MountPoint { get; set; } = string.Empty;
}