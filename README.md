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

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 12+ (optional - will use default if not configured)

### Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres"
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
Returns list of all available transaction categories.

**Response**: `ApiResponse<CategoriesResponse>`

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
6. **Background Services**: Auto-seeding on startup
7. **Data Sources**: Mock data generators using Bogus

## Development

### Adding New Categories

Update `TransactionCategory.cs` and add keyword mappings in `CategorizationService.cs`.

### Configuring Data Generation

Modify `DataSeedingService.cs` to adjust:
- Number of transactions per source
- Customer IDs
- Date ranges
- Transaction descriptions and amounts

## License

MIT
