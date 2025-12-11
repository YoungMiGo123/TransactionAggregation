namespace TransactionAggregation.API.Application.Core.Entities;

public class CategoryRule : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;

}
