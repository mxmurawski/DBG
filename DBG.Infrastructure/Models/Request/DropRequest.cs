namespace DBG.Infrastructure.Models.Request;

public class DropRequest
{
    public required string Name { get; set; }
    public string? Database { get; set; }
}