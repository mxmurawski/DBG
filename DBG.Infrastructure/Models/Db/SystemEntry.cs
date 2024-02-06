#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure.Models.Db;

[Table("system_entries", Schema = "core")]
[PrimaryKey(nameof(Id))]
public class SystemEntry
{
    public required Guid Id { get; set; }
    [StringLength(255)]
    public string? Name { get; set; }
    public required DbSystemEntry DbEntry { get; set; }
    public required OsSystemEntry OsEntry { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}