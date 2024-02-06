namespace DBG.Infrastructure.Models.Request;

public class SystemEntryAddRequest
{
    public string? Name { get; set; }
    public required DbSystemEntryAddRequest DbEntry { get; set; }
    public required OsSystemEntryAddRequest OsEntry { get; set; }
}