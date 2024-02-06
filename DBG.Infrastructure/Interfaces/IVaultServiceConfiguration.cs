namespace DBG.Infrastructure.Interfaces;

public interface IVaultServiceConfiguration
{
    string Address { get; set; }
    string Token { get; set; }
    string MountPoint { get; set; }
}