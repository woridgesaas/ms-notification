using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using ms_notification.Data;
using ms_notification.Hubs;
using ms_notification.Options;
using ms_notification.Repositories;
using ms_notification.Services;
using Serilog;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Charge aussi les variables d'environnement (apr�s Env.Load)
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITemplateRendererService, TemplateRendererService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IAppService, AppService>();

// Serilog (avant Build)
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// DbContext
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + HealthChecks
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    }); builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Smtp settings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// SignalR
builder.Services.AddSignalR();


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("WoridgeCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        // Si null ou vide => refuse tout (�vite crash et �vite config dangereuse)
        if (allowedOrigins is null || allowedOrigins.Length == 0)
        {
            return;
        }
            

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    // Apply migrations
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();

}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("WoridgeCorsPolicy");

app.UseAuthorization();

// Health endpoints
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapGet("/health-API", () => "Healthcheck, MS-notification is UP");

// Controllers + SignalR
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();