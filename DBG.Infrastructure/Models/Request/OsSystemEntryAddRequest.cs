#region

using DBG.Infrastructure.Enums;

#endregion

namespace DBG.Infrastructure.Models.Request;

public class OsSystemEntryAddRequest
{
    public required OsType OsType { get; set; }
    public required string Address { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}