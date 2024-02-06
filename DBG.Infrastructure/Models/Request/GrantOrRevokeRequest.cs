namespace DBG.Infrastructure.Models.Request;

public class GrantOrRevokeRequest
{
    public required string What { get; set; }
    public string Where { get; set; }
    public required string Who { get; set; }
    public required string Database { get; set; }
}