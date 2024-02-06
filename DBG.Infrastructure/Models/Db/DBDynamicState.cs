#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[PrimaryKey(nameof(Id))]
[Index(nameof(DbSystemEntryId))]
[Table("db_dynamic_states", Schema = "db_states")]
public class DbDynamicState
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid DbSystemEntryId { get; set; }
    public int ConnectionsCount { get; set; }
    [StringLength(2048)]
    public string? DbAndTableSizesAsJson { get; set; }
}