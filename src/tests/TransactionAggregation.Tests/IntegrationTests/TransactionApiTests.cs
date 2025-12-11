using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using TransactionAggregation.API;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;
using Xunit;

namespace TransactionAggregation.Tests.IntegrationTests;

public class TransactionApiTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;
    private PostgreSqlContainer? _postgresContainer;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("transactiondb_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove auto-seeding background service for tests
                    var descriptors = services.Where(d =>
                        d.ServiceType.Name.Contains("DataSeedingService") ||
                        d.ServiceType.Name.Contains("TransactionCategorizationService"))
                        .ToList();

                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }
                });

                builder.UseSetting("ConnectionStrings:Postgres", connectionString);
                builder.UseSetting("DataSeeding:AutoSeed", "false");
            });

        _client = _factory.CreateClient();

        // Seed test data
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();

        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory!.Services.CreateScope();
        var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

        await using var session = documentStore.LightweightSession();

        // Seed customers
        var customers = new[]
        {
            new Customer { Id = Guid.NewGuid(), CustomerId = "CUST-001", Name = "John Doe", Email = "john@example.com" },
            new Customer { Id = Guid.NewGuid(), CustomerId = "CUST-002", Name = "Jane Smith", Email = "jane@example.com" }
        };
        session.Store(customers);

        // Seed categories
        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = TransactionCategory.Groceries, Description = "Groceries" },
            new Category { Id = Guid.NewGuid(), Name = TransactionCategory.Entertainment, Description = "Entertainment" }
        };
        session.Store(categories);

        // Seed transactions
        var transactions = new[]
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                CustomerId = "CUST-001",
                CustomerName = "John Doe",
                Amount = -125.50m,
                TransactionDate = DateTime.UtcNow.AddDays(-5),
                Description = "Walmart Purchase",
                Category = TransactionCategory.Groceries,
                Source = "Bank System",
                Type = "Debit"
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                CustomerId = "CUST-001",
                CustomerName = "John Doe",
                Amount = -45.00m,
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                Description = "Netflix Subscription",
                Category = TransactionCategory.Entertainment,
                Source = "Credit Card System",
                Type = "Debit"
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                CustomerId = "CUST-002",
                CustomerName = "Jane Smith",
                Amount = -85.30m,
                TransactionDate = DateTime.UtcNow.AddDays(-2),
                Description = "Target Purchase",
                Category = TransactionCategory.Groceries,
                Source = "Bank System",
                Type = "Debit"
            }
        };
        session.Store(transactions);

        await session.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllTransactions_ReturnsSuccess()
    {
        // Act
        var response = await _client!.GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionsResponse>>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Transactions.Should().NotBeEmpty();
        result.Data.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTransactionsByCustomer_ReturnsCustomerTransactions()
    {
        // Act
        var response = await _client!.GetAsync("/api/transactions/customer/CUST-001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionsResponse>>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Transactions.Should().NotBeEmpty();
        result.Data.Transactions.Should().AllSatisfy(t => t.CustomerId.Should().Be("CUST-001"));
    }

    [Fact]
    public async Task GetTransactionsByCategory_ReturnsCategoryTransactions()
    {
        // Act
        var response = await _client!.GetAsync($"/api/transactions/category/{TransactionCategory.Groceries}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionsResponse>>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Transactions.Should().NotBeEmpty();
        result.Data.Transactions.Should().AllSatisfy(t => t.Category.Should().Be(TransactionCategory.Groceries));
    }

    [Fact]
    public async Task GetCustomerSummary_ReturnsCorrectSummary()
    {
        // Act
        var response = await _client!.GetAsync("/api/transactions/summary/customer/CUST-001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionSummary>>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CustomerId.Should().Be("CUST-001");
        result.Data.TransactionCount.Should().BeGreaterThan(0);
        result.Data.TotalAmount.Should().NotBe(0);
    }

    [Fact]
    public async Task GetCategories_ReturnsCategories()
    {
        // Act
        var response = await _client!.GetAsync("/api/transactions/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoriesResponse>>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Categories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCategorySummary_ReturnsAggregatedData()
    {
        // Act
        var response = await _client!.GetAsync("/api/transactions/summary/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategorySummaryResponse>>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CategoryBreakdowns.Should().NotBeEmpty();
        result.Data.TotalTransactions.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSourceSummary_ReturnsSourceData()
    {
        // Act
        var response = await _client!.GetAsync("/api/transactions/summary/sources");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SourceSummaryResponse>>();
        result.Should().NotBeNull();
        result!.Successful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SourceBreakdowns.Should().NotBeEmpty();
        result.Data.TotalTransactions.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCustomerSummary_WithNonExistentCustomer_ReturnsNotFound()
    {
        // Act
        var response = await _client!.GetAsync("/api/transactions/summary/customer/CUST-NONEXISTENT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
