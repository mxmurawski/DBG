#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[Table("os_static_states", Schema = "os_states")]
[PrimaryKey(nameof(Id))]
[Index(nameof(OsSystemEntryId))]
public class OsStaticState
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid OsSystemEntryId { get; set; }
    [StringLength(512)]
    public string? Version { get; set; }
    public int CpuCount { get; set; }
    public int RamCount { get; set; }
    [StringLength(255)]
    public string? Uptime { get; set; }
}