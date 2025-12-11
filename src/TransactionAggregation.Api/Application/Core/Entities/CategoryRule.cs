namespace TransactionAggregation.API.Models;

public class CategoryRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
