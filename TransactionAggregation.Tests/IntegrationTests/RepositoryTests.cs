using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using TransactionAggregation.API.Models;
using Xunit;

namespace TransactionAggregation.Tests.IntegrationTests;

[Collection("Database collection")]
public class CustomerRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public CustomerRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanStoreAndRetrieveCustomer()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST-TEST-001",
            Name = "Test Customer",
            Email = "test@example.com"
        };

        // Act
        await using (var session = documentStore.LightweightSession())
        {
            session.Store(customer);
            await session.SaveChangesAsync();
        }

        // Assert
        await using (var session = documentStore.LightweightSession())
        {
            var retrieved = await session.LoadAsync<Customer>(customer.Id);
            retrieved.Should().NotBeNull();
            retrieved!.CustomerId.Should().Be("CUST-TEST-001");
            retrieved.Name.Should().Be("Test Customer");
            retrieved.Email.Should().Be("test@example.com");
        }
    }

    [Fact]
    public async Task CanQueryCustomerByCustomerId()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST-QUERY-001",
            Name = "Query Test",
            Email = "query@example.com"
        };

        await using (var session = documentStore.LightweightSession())
        {
            session.Store(customer);
            await session.SaveChangesAsync();
        }

        // Act
        await using (var session = documentStore.LightweightSession())
        {
            var results = await session.Query<Customer>()
                .Where(c => c.CustomerId == "CUST-QUERY-001" && !c.IsDeleted)
                .ToListAsync();

            // Assert
            results.Should().ContainSingle();
            results[0].Name.Should().Be("Query Test");
        }
    }
}

[Collection("Database collection")]
public class CategoryRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public CategoryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanStoreAndRetrieveCategory()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Description = "Test Description",
            Keywords = new List<string> { "test", "keyword" }
        };

        // Act
        await using (var session = documentStore.LightweightSession())
        {
            session.Store(category);
            await session.SaveChangesAsync();
        }

        // Assert
        await using (var session = documentStore.LightweightSession())
        {
            var retrieved = await session.LoadAsync<Category>(category.Id);
            retrieved.Should().NotBeNull();
            retrieved!.Name.Should().Be("Test Category");
            retrieved.Keywords.Should().Contain(new[] { "test", "keyword" });
        }
    }

    [Fact]
    public async Task CanQueryCategoriesByName()
    {
        // Arrange
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();

        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = TransactionCategory.Groceries, Description = "Groceries" },
            new Category { Id = Guid.NewGuid(), Name = TransactionCategory.Entertainment, Description = "Entertainment" }
        };

        await using (var session = documentStore.LightweightSession())
        {
            session.Store(categories);
            await session.SaveChangesAsync();
        }

        // Act
        await using (var session = documentStore.LightweightSession())
        {
            var results = await session.Query<Category>()
                .Where(c => c.Name == TransactionCategory.Groceries && !c.IsDeleted)
                .ToListAsync();

            // Assert
            results.Should().ContainSingle();
            results[0].Description.Should().Be("Groceries");
        }
    }
}
