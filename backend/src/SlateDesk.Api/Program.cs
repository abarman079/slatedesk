var builder = WebApplication.CreateBuilder(args);

// Add controller support.
builder.Services.AddControllers();

// Add Swagger documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger during local development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Temporary health endpoint.
// PostgreSQL checking will be added during Phase 1.
app.MapGet("/api/v1/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        application = "SlateDesk API",
        timestampUtc = DateTime.UtcNow
    });
})
.WithName("GetHealthStatus")
.WithTags("Health");

app.Run();

// Required later for integration testing.
public partial class Program
{
}