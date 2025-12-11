using FluentAssertions;
using TransactionAggregation.API.Models;
using Xunit;

namespace TransactionAggregation.Tests.UnitTests;

public class TransactionModelTests
{
    [Fact]
    public void Transaction_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var transaction = new Transaction();

        // Assert
        transaction.Id.Should().NotBe(Guid.Empty);
        transaction.CustomerId.Should().BeEmpty();
        transaction.CustomerName.Should().BeEmpty();
        transaction.Amount.Should().Be(0);
        transaction.Description.Should().BeEmpty();
        transaction.Category.Should().BeEmpty();
        transaction.Source.Should().BeEmpty();
        transaction.Currency.Should().Be("USD");
        transaction.Type.Should().BeEmpty();
        transaction.IsDeleted.Should().BeFalse();
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Transaction_CanSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var transactionDate = DateTime.UtcNow.AddDays(-5);

        // Act
        var transaction = new Transaction
        {
            Id = id,
            CustomerId = "CUST-001",
            CustomerName = "John Doe",
            Amount = -125.50m,
            TransactionDate = transactionDate,
            Description = "Test Transaction",
            Category = TransactionCategory.Groceries,
            Source = "Bank System",
            Currency = "EUR",
            Type = "Debit"
        };

        // Assert
        transaction.Id.Should().Be(id);
        transaction.CustomerId.Should().Be("CUST-001");
        transaction.CustomerName.Should().Be("John Doe");
        transaction.Amount.Should().Be(-125.50m);
        transaction.TransactionDate.Should().Be(transactionDate);
        transaction.Description.Should().Be("Test Transaction");
        transaction.Category.Should().Be(TransactionCategory.Groceries);
        transaction.Source.Should().Be("Bank System");
        transaction.Currency.Should().Be("EUR");
        transaction.Type.Should().Be("Debit");
    }
}

public class CustomerModelTests
{
    [Fact]
    public void Customer_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.Id.Should().NotBe(Guid.Empty);
        customer.CustomerId.Should().BeEmpty();
        customer.Name.Should().BeEmpty();
        customer.Email.Should().BeEmpty();
        customer.IsDeleted.Should().BeFalse();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Customer_CanSetAllProperties()
    {
        // Arrange & Act
        var customer = new Customer
        {
            CustomerId = "CUST-001",
            Name = "John Doe",
            Email = "john.doe@example.com"
        };

        // Assert
        customer.CustomerId.Should().Be("CUST-001");
        customer.Name.Should().Be("John Doe");
        customer.Email.Should().Be("john.doe@example.com");
    }
}

public class CategoryModelTests
{
    [Fact]
    public void Category_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var category = new Category();

        // Assert
        category.Id.Should().NotBe(Guid.Empty);
        category.Name.Should().BeEmpty();
        category.Description.Should().BeEmpty();
        category.Keywords.Should().NotBeNull().And.BeEmpty();
        category.IsDeleted.Should().BeFalse();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Category_CanSetAllProperties()
    {
        // Arrange & Act
        var category = new Category
        {
            Name = TransactionCategory.Groceries,
            Description = "Grocery stores and supermarkets",
            Keywords = new List<string> { "walmart", "target", "grocery" }
        };

        // Assert
        category.Name.Should().Be(TransactionCategory.Groceries);
        category.Description.Should().Be("Grocery stores and supermarkets");
        category.Keywords.Should().HaveCount(3);
        category.Keywords.Should().Contain(new[] { "walmart", "target", "grocery" });
    }
}

public class ApiResponseTests
{
    [Fact]
    public void ApiResponse_Success_CreatesSuccessResponse()
    {
        // Arrange
        var data = new { Value = "test" };

        // Act
        var response = ApiResponse<object>.Success(data, "Success message");

        // Assert
        response.Successful.Should().BeTrue();
        response.Message.Should().Be("Success message");
        response.Data.Should().Be(data);
        response.StatusCode.Should().Be(200);
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ApiResponse_Fail_CreatesFailureResponse()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var response = ApiResponse<object>.Fail(errorMessage, 400);

        // Assert
        response.Successful.Should().BeFalse();
        response.Message.Should().Be(errorMessage);
        response.Data.Should().BeNull();
        response.StatusCode.Should().Be(400);
        response.Errors.Should().ContainSingle().Which.Should().Be(errorMessage);
    }

    [Fact]
    public void ApiResponse_FailWithMultipleErrors_CreatesFailureResponseWithErrors()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var response = ApiResponse<object>.Fail(errors, "Multiple errors occurred", 500);

        // Assert
        response.Successful.Should().BeFalse();
        response.Message.Should().Be("Multiple errors occurred");
        response.Data.Should().BeNull();
        response.StatusCode.Should().Be(500);
        response.Errors.Should().HaveCount(3);
        response.Errors.Should().BeEquivalentTo(errors);
    }
}
