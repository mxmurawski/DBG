#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DBG.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[Table("db_system_entries", Schema = "core")]
[PrimaryKey(nameof(Id))]
public class DbSystemEntry
{
    public required Guid Id { get; set; }
    public required DbType DbType { get; set; }
    [StringLength(255)]
    public required string Address { get; set; }
    public required int Port { get; set; }

    [NotMapped] public string? Username { get; set; }

    [NotMapped] public string? Password { get; set; }
}