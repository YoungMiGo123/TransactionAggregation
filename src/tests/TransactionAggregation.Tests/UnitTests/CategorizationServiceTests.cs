using FluentAssertions;
using TransactionAggregation.API.Application.Core.Entities;
using TransactionAggregation.API.Application.Core.Models;
using TransactionAggregation.API.Services;
using Xunit;

namespace TransactionAggregation.Tests.UnitTests;

public class CategorizationServiceTests
{
    private readonly ICategorizationService _service = new CategorizationService();

    [Fact]
    public void CategorizeTransaction_WithGroceryKeyword_ReturnsGroceriesCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Walmart Supercenter Purchase",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Groceries);
    }

    [Fact]
    public void CategorizeTransaction_WithEntertainmentKeyword_ReturnsEntertainmentCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Netflix Monthly Subscription",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Entertainment);
    }

    [Fact]
    public void CategorizeTransaction_WithUtilitiesKeyword_ReturnsUtilitiesCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Electric Company Bill Payment",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Utilities);
    }

    [Fact]
    public void CategorizeTransaction_WithTransportationKeyword_ReturnsTransportationCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Uber Ride to Airport",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Transportation);
    }

    [Fact]
    public void CategorizeTransaction_WithHealthcareKeyword_ReturnsHealthcareCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "CVS Pharmacy Purchase",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Healthcare);
    }

    [Fact]
    public void CategorizeTransaction_WithShoppingKeyword_ReturnsShoppingCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Amazon.com Online Purchase",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Shopping);
    }

    [Fact]
    public void CategorizeTransaction_WithDiningKeyword_ReturnsDiningCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Restaurant - Italian Bistro",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Dining);
    }

    [Fact]
    public void CategorizeTransaction_WithTravelKeyword_ReturnsTravelCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Hotel Reservation - Marriott",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Travel);
    }

    [Fact]
    public void CategorizeTransaction_WithEducationKeyword_ReturnsEducationCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Online Course - Udemy",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Education);
    }

    [Fact]
    public void CategorizeTransaction_WithUnknownKeyword_ReturnsOtherCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Unknown Merchant ABC123XYZ Purchase",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Other);
    }

    [Fact]
    public void CategorizeTransaction_WithExistingCategory_ReturnsSameCategory()
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = "Some Description",
            Category = TransactionCategory.Shopping
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(TransactionCategory.Shopping);
    }

    [Theory]
    [InlineData("walmart", TransactionCategory.Groceries)]
    [InlineData("WALMART", TransactionCategory.Groceries)]
    [InlineData("WaLmArT", TransactionCategory.Groceries)]
    public void CategorizeTransaction_IsCaseInsensitive(string keyword, string expectedCategory)
    {
        // Arrange
        var transaction = new Transaction
        {
            Description = $"{keyword} purchase",
            Category = string.Empty
        };

        // Act
        var result = _service.CategorizeTransaction(transaction);

        // Assert
        result.Should().Be(expectedCategory);
    }
}
