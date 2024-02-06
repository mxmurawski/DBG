#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[Table("os_dynamic_states", Schema = "os_states")]
[PrimaryKey(nameof(Id))]
[Index(nameof(OsSystemEntryId))]
public class OsDynamicState
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid OsSystemEntryId { get; set; }
    public double CpuUsage { get; set; }
    public double RamUsage { get; set; }
    [StringLength(2048)]
    public string? DiskUsageAsJson { get; set; }
}