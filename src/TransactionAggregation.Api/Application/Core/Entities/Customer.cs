namespace TransactionAggregation.API.Application.Core.Entities;

public class Customer : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
