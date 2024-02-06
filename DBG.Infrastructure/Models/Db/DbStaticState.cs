#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[Table("db_static_states", Schema = "db_states")]
[PrimaryKey(nameof(Id))]
[Index(nameof(DbSystemEntryId))]
public class DbStaticState
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid DbSystemEntryId { get; set; }
    [StringLength(512)]
    public string? Version { get; set; }
    public int MaxConnectionsCount { get; set; }
}