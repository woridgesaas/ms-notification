using DotNetEnv;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using ms_notification.Hubs;
using ms_notification.Middleware;
using ms_notification.Options;
using ms_notification.Persistence;
using ms_notification.Repositories;
using ms_notification.Services;
using Serilog;
using System.Text;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Charge aussi les variables d'environnement (après Env.Load)
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddScoped<TemplateProvider>();
builder.Services.AddScoped<TemplateRenderer>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<NotificationService>();



//
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotifService, NotifService>();
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

// Options binding (SmtpSettings)
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

        // Si null ou vide => refuse tout (évite crash et évite config dangereuse)
        if (allowedOrigins is null || allowedOrigins.Length == 0)
            return;

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    // ex: app.UseSwagger(); app.UseSwaggerUI();
}


app.UseMiddleware<ExceptionMiddleware>();

//app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("WoridgeCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Health endpoints
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapGet("/health-API", () => "Healthcheck, MS-notification is UP");

// Controllers + SignalR
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

// SMTP health ping (optionnel)
var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
if (smtpSettings is not null)
{
    try
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpSettings.SenderName, smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress("Test Health", "test@example.com"));
        message.Subject = "Health Check : Notification Service";
        message.Body = new TextPart("plain") { Text = "This is just a health check :)" };

        using var client = new SmtpClient();
        client.Connect(smtpSettings.Server, smtpSettings.Port, false);
        client.Send(message);
        client.Disconnect(true);

        Console.WriteLine($"Success: Connected to SMTP: {smtpSettings.Server}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: cannot connect to {smtpSettings.Server}: {ex.Message}");
    }
}

app.Run();