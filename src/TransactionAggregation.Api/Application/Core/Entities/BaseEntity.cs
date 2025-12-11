namespace TransactionAggregation.API.Application.Core.Entities;

public class BaseEntity
{
    public Guid Id { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}