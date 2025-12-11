using TransactionAggregation.API.Application.Extensions;
using TransactionAggregation.API.Application.Middleware;
using TransactionAggregation.API.Services;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Marten for data persistence
builder.Services.AddMartenConfiguration(builder.Configuration);

// Configure Wolverine for mediator pattern
builder.Host.UseWolverine();

// Register services
builder.Services.AddSingleton<ICategorizationService, CategorizationService>();
builder.Services.AddSingleton<IRuleBasedCategorizer, RuleBasedCategorizer>();

// Register background services for data seeding and categorization
//builder.Services.AddHostedService<DataSeedingService>();
//builder.Services.AddHostedService<TransactionCategorizationService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Add correlation ID middleware first to ensure all requests have a correlation ID
app.UseCorrelationId();

// Add global exception handler middleware early in the pipeline
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
