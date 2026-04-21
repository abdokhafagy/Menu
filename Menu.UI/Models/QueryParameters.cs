namespace Menu.UI.Models;

public sealed class QueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }
    public Dictionary<string, string>? Filters { get; set; }
}
