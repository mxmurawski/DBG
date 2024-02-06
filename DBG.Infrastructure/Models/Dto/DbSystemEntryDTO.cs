#region

using DBG.Infrastructure.Enums;

#endregion

namespace DBG.Infrastructure.Models.Dto;

public class DbSystemEntryDto
{
    public required Guid Id { get; set; }
    public required DbType DbType { get; set; }
    public required string Address { get; set; }
    public required int Port { get; set; }
}