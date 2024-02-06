#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DBG.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[Table("os_system_entries", Schema = "core")]
[PrimaryKey(nameof(Id))]
public class OsSystemEntry
{
    public required Guid Id { get; set; }
    public required OsType OsType { get; set; }
    [StringLength(255)]
    public required string Address { get; set; }
    public required int Port { get; set; }

    [NotMapped] public string? Username { get; set; }

    [NotMapped] public string? Password { get; set; }
}