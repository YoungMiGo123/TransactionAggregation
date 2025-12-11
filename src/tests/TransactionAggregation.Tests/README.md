# TransactionAggregation.Tests

Comprehensive test suite for the Transaction Aggregation API including unit tests and integration tests.

## Test Structure

```
TransactionAggregation.Tests/
├── UnitTests/
│   ├── CategorizationServiceTests.cs    # Tests for transaction categorization logic
│   └── ModelTests.cs                     # Tests for domain models and API response models
├── IntegrationTests/
│   ├── DatabaseFixture.cs                # Test database setup with Testcontainers
│   ├── TransactionRepositoryTests.cs    # Marten document store tests
│   ├── RepositoryTests.cs                # Customer and Category repository tests
│   └── TransactionApiTests.cs            # End-to-end API tests
```

## Test Categories

### Unit Tests (21 tests)
Tests individual components in isolation without external dependencies:

**CategorizationServiceTests**
- Category matching for different transaction types (Groceries, Entertainment, Utilities, etc.)
- Case-insensitive keyword matching
- Handling of pre-categorized transactions
- Default "Other" category for unknown transactions

**ModelTests**
- Transaction model validation
- Customer model validation
- Category model validation
- ApiResponse success and failure scenarios

### Integration Tests (18 tests)
Tests with real database using PostgreSQL Testcontainers:

**TransactionRepositoryTests**
- Store and retrieve transactions
- Query by customer ID
- Query by category
- Soft delete functionality

**CustomerRepositoryTests**
- Store and retrieve customers
- Query by customer ID

**CategoryRepositoryTests**
- Store and retrieve categories
- Query by category name

**TransactionApiTests**
- Full end-to-end API testing with real HTTP requests
- Tests all 9 API endpoints
- Validates response models and status codes
- Tests error scenarios (404 for non-existent customers)

## Running Tests

### Run All Tests
```bash
cd TransactionAggregation.Tests
dotnet test
```

### Run With Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Category
```bash
# Run only unit tests
dotnet test --filter FullyQualifiedName~UnitTests

# Run only integration tests
dotnet test --filter FullyQualifiedName~IntegrationTests
```

### Run Specific Test Class
```bash
dotnet test --filter FullyQualifiedName~CategorizationServiceTests
```

### Run With Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Dependencies

- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library for readable test assertions
- **Moq** - Mocking framework (available for future use)
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing with WebApplicationFactory
- **Testcontainers.PostgreSql** - Docker-based PostgreSQL for integration tests

## Integration Test Setup

Integration tests use **Testcontainers** to spin up a real PostgreSQL database in Docker:

- Each test collection gets a fresh database instance
- Database is automatically cleaned up after tests complete
- No manual database setup required
- Tests run in isolation

### Requirements for Integration Tests

- Docker must be running on your machine
- Docker daemon must be accessible
- Sufficient permissions to create and run containers

### Troubleshooting Integration Tests

**Docker not running:**
```
Error: Cannot connect to the Docker daemon
```
Solution: Start Docker Desktop or Docker daemon

**Permission denied:**
```
Error: Permission denied while trying to connect to Docker
```
Solution: Add your user to the docker group or run with appropriate permissions

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
```csharp
[Fact]
public void TestName()
{
    // Arrange - Setup test data and dependencies
    var transaction = new Transaction { ... };

    // Act - Execute the method being tested
    var result = _service.CategorizeTransaction(transaction);

    // Assert - Verify the expected outcome
    result.Should().Be(TransactionCategory.Groceries);
}
```

### FluentAssertions
Tests use FluentAssertions for readable assertions:
```csharp
// Instead of: Assert.Equal(expected, actual)
result.Should().Be(expected);

// Instead of: Assert.True(collection.Count > 0)
collection.Should().NotBeEmpty();

// Instead of: Assert.Contains(item, collection)
collection.Should().Contain(item);
```

## Continuous Integration

Tests are designed to run in CI/CD pipelines:
- No manual setup required
- Testcontainers handles database provisioning
- All tests complete in ~40 seconds
- Zero external dependencies (besides Docker)

## Test Coverage

Current test coverage includes:
- ✅ Transaction categorization logic
- ✅ All domain models (Transaction, Customer, Category, CategoryRule)
- ✅ API response models
- ✅ Marten document store operations
- ✅ All 9 API endpoints
- ✅ Query filtering and sorting
- ✅ Soft delete functionality
- ✅ Error handling (404, 500)

## Adding New Tests

### Unit Test Example
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = "test";

    // Act
    var result = _service.Process(input);

    // Assert
    result.Should().NotBeNull();
}
```

### Integration Test Example
```csharp
[Collection("Database collection")]
public class MyIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public MyIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestName()
    {
        var serviceProvider = _fixture.CreateServiceProvider();
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();
        
        // ... test code
    }
}
```

## Best Practices

1. **Test Naming**: Use descriptive names following `MethodName_Scenario_ExpectedBehavior` pattern
2. **Isolation**: Each test should be independent and not rely on other tests
3. **AAA Pattern**: Always structure tests with Arrange, Act, Assert sections
4. **Assertions**: Use FluentAssertions for readable and maintainable assertions
5. **Test Data**: Keep test data minimal and focused on the scenario being tested
6. **Async/Await**: Always await async operations in tests
7. **Cleanup**: Testcontainers handles cleanup automatically for integration tests

## Performance

- Unit tests: ~0.1s each
- Integration tests with database: ~1-2s each
- API tests with full stack: ~2-3s each
- Total test suite: ~32-40s

## Future Enhancements

- [ ] Add performance/load tests
- [ ] Add tests for background services
- [ ] Add tests for RuleBasedCategorizer
- [ ] Add mutation testing
- [ ] Add code coverage reporting
- [ ] Add benchmark tests
