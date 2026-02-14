using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    
}
app.UseHttpsRedirection();

app.UseAuthorization();

//Healthcheck for database connectivity
app.MapHealthChecks("/health", new HealthCheckOptions());

//Healthcheck for applciation API
app.MapGet("/health-API", () => "Healthcheck , MS-notification is UP");

app.MapControllers();

app.Run();
