namespace DBG.Infrastructure.Models.Dto;

public class SystemEntryDto
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public required DbSystemEntryDto DbEntry { get; set; }
    public required OsSystemEntryDto OsEntry { get; set; }
}