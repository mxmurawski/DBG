namespace DBG.Infrastructure.Models.Helper;

public class DbResult
{
    public IList<string> Columns { get; set; } = new List<string>();
    public IList<IList<string>> Rows { get; set; } = new List<IList<string>>();
}