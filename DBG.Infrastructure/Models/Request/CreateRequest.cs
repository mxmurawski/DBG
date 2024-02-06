namespace DBG.Infrastructure.Models.Request;

public class CreateRequest
{
    public required string Name { get; set; }
    public string? Password { get; set; }
    public string? Database { get; set; }
}