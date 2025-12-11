using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using TransactionAggregation.API.Models;
using Xunit;

namespace TransactionAggregation.Tests.IntegrationTests;

[Collection("Database collection")]
public class TransactionRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public TransactionRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanStoreAndRetrieveTransaction()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST-001",
            CustomerName = "John Doe",
            Amount = -125.50m,
            TransactionDate = DateTime.UtcNow,
            Description = "Test Transaction",
            Category = TransactionCategory.Groceries,
            Source = "Bank System",
            Currency = "USD",
            Type = "Debit"
        };

        // Act
        await using (var session = documentStore.LightweightSession())
        {
            session.Store(transaction);
            await session.SaveChangesAsync();
        }

        // Assert
        await using (var session = documentStore.LightweightSession())
        {
            var retrieved = await session.LoadAsync<Transaction>(transaction.Id);
            retrieved.Should().NotBeNull();
            retrieved!.CustomerId.Should().Be("CUST-001");
            retrieved.CustomerName.Should().Be("John Doe");
            retrieved.Amount.Should().Be(-125.50m);
            retrieved.Category.Should().Be(TransactionCategory.Groceries);
        }
    }

    [Fact]
    public async Task CanQueryTransactionsByCustomerId()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var transactions = new[]
        {
            new Transaction { Id = Guid.NewGuid(), CustomerId = "CUST-001", CustomerName = "John Doe", Amount = -50m, Description = "Purchase 1" },
            new Transaction { Id = Guid.NewGuid(), CustomerId = "CUST-001", CustomerName = "John Doe", Amount = -75m, Description = "Purchase 2" },
            new Transaction { Id = Guid.NewGuid(), CustomerId = "CUST-002", CustomerName = "Jane Smith", Amount = -100m, Description = "Purchase 3" }
        };

        await using (var session = documentStore.LightweightSession())
        {
            session.Store(transactions);
            await session.SaveChangesAsync();
        }

        // Act
        await using (var session = documentStore.LightweightSession())
        {
            var results = await session.Query<Transaction>()
                .Where(t => t.CustomerId == "CUST-001" && !t.IsDeleted)
                .ToListAsync();

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(t => t.CustomerId.Should().Be("CUST-001"));
        }
    }

    [Fact]
    public async Task CanQueryTransactionsByCategory()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var transactions = new[]
        {
            new Transaction { Id = Guid.NewGuid(), CustomerId = "CUST-001", Category = TransactionCategory.Groceries, Amount = -50m },
            new Transaction { Id = Guid.NewGuid(), CustomerId = "CUST-001", Category = TransactionCategory.Groceries, Amount = -75m },
            new Transaction { Id = Guid.NewGuid(), CustomerId = "CUST-002", Category = TransactionCategory.Entertainment, Amount = -100m }
        };

        await using (var session = documentStore.LightweightSession())
        {
            session.Store(transactions);
            await session.SaveChangesAsync();
        }

        // Act
        await using (var session = documentStore.LightweightSession())
        {
            var results = await session.Query<Transaction>()
                .Where(t => t.Category == TransactionCategory.Groceries && !t.IsDeleted)
                .ToListAsync();

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(t => t.Category.Should().Be(TransactionCategory.Groceries));
        }
    }

    [Fact]
    public async Task SoftDelete_ExcludesTransactionFromQueries()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST-001",
            Amount = -50m
        };

        await using (var session = documentStore.LightweightSession())
        {
            session.Store(transaction);
            await session.SaveChangesAsync();
        }

        // Act - Soft delete
        await using (var session = documentStore.LightweightSession())
        {
            var toDelete = await session.LoadAsync<Transaction>(transaction.Id);
            toDelete!.IsDeleted = true;
            session.Update(toDelete);
            await session.SaveChangesAsync();
        }

        // Assert
        await using (var session = documentStore.LightweightSession())
        {
            var results = await session.Query<Transaction>()
                .Where(t => !t.IsDeleted)
                .ToListAsync();

            results.Should().NotContain(t => t.Id == transaction.Id);
        }
    }
}
