# TransactionAggregation

A .NET-based system that aggregates customer financial transaction data from multiple mock data sources and categorizes the transactions. It provides an extensive API for retrieving aggregated information.

## Features

- **Multiple Data Sources**: Integrates with mock Bank System, Credit Card System, and Payment Processor
- **Data Persistence**: Uses Marten with PostgreSQL for document storage
- **Auto-Seeding**: Configurable automatic data generation using Bogus library
- **Transaction Categorization**: Intelligent categorization of transactions into predefined categories
- **Rich Query API**: Extensive API endpoints for querying and aggregating transaction data
- **Wolverine Mediator**: Uses Wolverine for CQRS pattern implementation
- **Consistent Response Model**: All API responses wrapped in `ApiResponse<T>` for consistent error handling

## Tech Stack

- **.NET 10.0**
- **Marten** - Document database on PostgreSQL
- **Wolverine** - Mediator/Message Bus for CQRS
- **Bogus** - Fake data generation
- **PostgreSQL** - Database
- **ASP.NET Core Web API**
- **Docker & Docker Compose** - Containerization

## Quick Start with Docker Compose

The easiest way to run the application is using Docker Compose, which will spin up both the PostgreSQL database and the API.

### Prerequisites

- Docker Desktop (or Docker Engine + Docker Compose)
- No other services running on ports 5000 and 5432

### Running the Application

1. **Clone the repository**
   ```bash
   git clone https://github.com/YoungMiGo123/TransactionAggregation.git
   cd TransactionAggregation
   ```

2. **Start the application**
   ```bash
   docker-compose up -d
   ```

   This command will:
   - Pull the required Docker images (PostgreSQL 16 and .NET 10)
   - Build the API container
   - Start PostgreSQL database on port 5432
   - Start the API on port 5000
   - Automatically seed the database with 1,250+ transactions

3. **Check the logs**
   ```bash
   docker-compose logs -f api
   ```

4. **Access the API**
   - API: `http://localhost:5000`
   - OpenAPI/Swagger: `http://localhost:5000/openapi/v1.json`

5. **Stop the application**
   ```bash
   docker-compose down
   ```

6. **Stop and remove all data**
   ```bash
   docker-compose down -v
   ```

### Docker Compose Services

- **postgres**: PostgreSQL 16 database
  - Port: 5432
  - Database: `transactiondb`
  - User: `postgres`
  - Password: `postgres123`
  - Volume: `postgres_data` (persisted)

- **api**: Transaction Aggregation API
  - Port: 5000
  - Auto-seeding: Enabled by default
  - Health check: Waits for PostgreSQL to be ready

### Environment Variables

You can customize the application by modifying environment variables in `docker-compose.yml`:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__Postgres=Host=postgres;Port=5432;Database=transactiondb;Username=postgres;Password=postgres123
  - DataSeeding__AutoSeed=true  # Set to false to disable auto-seeding
```

## Running Locally (Without Docker)

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 12+ (optional - will use default if not configured)

### Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=yourpassword"
  },
  "DataSeeding": {
    "AutoSeed": true
  }
}
```

- **AutoSeed**: Set to `true` to automatically generate mock transaction data on startup

### Running the Application

```bash
dotnet restore
dotnet build
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

## API Endpoints

### Get All Transactions
```http
GET /api/transactions
```
Returns all transactions from all data sources with categorization applied.

**Response**: `ApiResponse<TransactionsResponse>`

### Get Transactions by Customer
```http
GET /api/transactions/customer/{customerId}
```
Returns all transactions for a specific customer.

**Response**: `ApiResponse<TransactionsResponse>`

### Get Transactions by Category
```http
GET /api/transactions/category/{category}
```
Returns all transactions in a specific category.

**Response**: `ApiResponse<TransactionsResponse>`

### Get Transactions by Date Range
```http
GET /api/transactions/date-range?startDate=2024-01-01&endDate=2024-12-31
```
Returns transactions within the specified date range.

**Response**: `ApiResponse<TransactionsResponse>`

### Get Transactions by Source
```http
GET /api/transactions/source/{source}
```
Returns transactions from a specific data source.

**Response**: `ApiResponse<TransactionsResponse>`

### Get Customer Summary
```http
GET /api/transactions/summary/customer/{customerId}
```
Returns comprehensive summary statistics for a customer including:
- Total amount
- Transaction count
- Average transaction amount
- Category breakdowns
- Source list
- Date range

**Response**: `ApiResponse<TransactionSummary>`

### Get Category Summary
```http
GET /api/transactions/summary/categories
```
Returns aggregated data by category with breakdowns including total amount, count, and averages.

**Response**: `ApiResponse<CategorySummaryResponse>`

### Get Source Summary
```http
GET /api/transactions/summary/sources
```
Returns transaction counts and totals by data source with percentages.

**Response**: `ApiResponse<SourceSummaryResponse>`

### Get Available Categories
```http
GET /api/transactions/categories
```
Returns list of all available transaction categories from the database.

**Response**: `ApiResponse<CategoriesResponse>`

## Testing the API

### Using cURL

```bash
# Get all transactions
curl http://localhost:5000/api/transactions

# Get transactions for a specific customer
curl http://localhost:5000/api/transactions/customer/CUST-001

# Get customer summary
curl http://localhost:5000/api/transactions/summary/customer/CUST-001

# Get all categories
curl http://localhost:5000/api/transactions/categories

# Get category summary
curl http://localhost:5000/api/transactions/summary/categories
```

### Using PowerShell

```powershell
# Get all transactions
Invoke-RestMethod -Uri http://localhost:5000/api/transactions

# Get customer summary
Invoke-RestMethod -Uri http://localhost:5000/api/transactions/summary/customer/CUST-001
```

## Transaction Categories

- **Groceries**: Supermarkets, grocery stores
- **Entertainment**: Streaming services, movies, concerts
- **Utilities**: Electric, water, gas, internet, phone
- **Transportation**: Uber, gas stations, parking
- **Healthcare**: Pharmacies, medical services
- **Shopping**: Online and retail stores
- **Dining**: Restaurants, cafes
- **Travel**: Flights, hotels, travel bookings
- **Education**: Online courses, tuition
- **Other**: Uncategorized transactions

## Response Model

All API responses are wrapped in the `ApiResponse<T>` model:

```csharp
{
  "successful": true,
  "message": "Operation successful",
  "data": { ... },
  "statusCode": 200,
  "errors": []
}
```

## Data Models

### Transaction
```csharp
{
  "id": "guid",
  "customerId": "string",
  "customerName": "string",
  "amount": 0.00,
  "transactionDate": "datetime",
  "description": "string",
  "category": "string",
  "source": "string",
  "currency": "USD",
  "type": "Debit/Credit",
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "isDeleted": false
}
```

### TransactionsResponse
```csharp
{
  "transactions": [],
  "totalCount": 0
}
```

### TransactionSummary
```csharp
{
  "customerId": "string",
  "customerName": "string",
  "totalAmount": 0.00,
  "transactionCount": 0,
  "averageTransactionAmount": 0.00,
  "categoryTotals": {},
  "categoryCounts": {},
  "firstTransactionDate": "datetime",
  "lastTransactionDate": "datetime",
  "sources": []
}
```

## Architecture

The system follows CQRS pattern using Wolverine:

1. **Controllers**: Handle HTTP requests and responses
2. **Queries**: Define query requests
3. **Query Handlers**: Process queries and return data from Marten
4. **Models**: Domain models and response DTOs
5. **Services**: Business logic (categorization)
6. **Background Services**: Auto-seeding on startup and transaction categorization
7. **Data Sources**: Mock data generators using Bogus

## Database Schema

The system uses Marten document database with the following collections:

- **Transactions**: Financial transaction records
- **Customers**: Customer information
- **Categories**: Transaction category definitions with keywords
- **CategoryRules**: Priority-based keyword matching rules for categorization

## Background Services

1. **DataSeedingService**: 
   - Runs once on application startup
   - Seeds 10 customers
   - Seeds 10 categories with keyword rules
   - Generates 1,250 transactions across 3 data sources

2. **TransactionCategorizationService**: 
   - Runs every 5 minutes
   - Automatically categorizes uncategorized transactions
   - Uses priority-based keyword matching from database rules

## Development

### Running Tests

The project includes comprehensive unit and integration tests.

**Run all tests:**
```bash
cd TransactionAggregation.Tests
dotnet test
```

**Run with detailed output:**
```bash
dotnet test --logger "console;verbosity=detailed"
```

**Run only unit tests:**
```bash
dotnet test --filter FullyQualifiedName~UnitTests
```

**Run only integration tests (requires Docker):**
```bash
dotnet test --filter FullyQualifiedName~IntegrationTests
```

**Test Coverage:**
- 21 unit tests covering categorization logic and models
- 18 integration tests with real PostgreSQL database
- Full API endpoint testing
- Total: 39 tests, ~32-40s execution time

See [TransactionAggregation.Tests/README.md](TransactionAggregation.Tests/README.md) for detailed test documentation.

### Adding New Categories

Update `TransactionCategory.cs` and add keyword mappings via the database by seeding new `Category` and `CategoryRule` entities.

### Configuring Data Generation

Modify `DataSeedingService.cs` to adjust:
- Number of transactions per source
- Customer IDs
- Date ranges
- Transaction descriptions and amounts

### Docker Development

To rebuild the API after making changes:

```bash
docker-compose build api
docker-compose up -d api
```

To view real-time logs:

```bash
docker-compose logs -f
```

## Troubleshooting

### Port Already in Use

If ports 5000 or 5432 are already in use, modify the port mappings in `docker-compose.yml`:

```yaml
ports:
  - "5001:8080"  # Change API port
  # or
  - "5433:5432"  # Change PostgreSQL port
```

### Database Connection Issues

Check that PostgreSQL is healthy:

```bash
docker-compose ps
```

View PostgreSQL logs:

```bash
docker-compose logs postgres
```

### API Not Starting

Check API logs for errors:

```bash
docker-compose logs api
```

Common issues:
- PostgreSQL not ready (wait a few seconds)
- Port conflicts
- Connection string misconfiguration

## License

MIT
