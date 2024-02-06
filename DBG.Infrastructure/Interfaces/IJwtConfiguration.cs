namespace DBG.Infrastructure.Interfaces;

public interface IJwtConfiguration
{
    string Secret { get; set; }
}