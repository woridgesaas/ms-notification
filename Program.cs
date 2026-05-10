using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ms_notification.Config;
using ms_notification.Data;
using ms_notification.Exceptions;
using ms_notification.Extensions;
using ms_notification.Factories;
using ms_notification.Hubs;
using ms_notification.Messaging.Consumers;
using ms_notification.Middleware;
using ms_notification.Options;
using ms_notification.Repositories;
using ms_notification.Services;
using Serilog;
using System.Text;

// * Load environment variables
Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// * Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// * Serilog configuration
builder.Host.UseSerilog((hostingContext, configuration) =>
{
    configuration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .WriteTo.Console();
});

// * Database configuration
builder.Services.AddDbContext<NotificationDbContext>((serviceProvider , optionsBuilder) =>
{
    var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var tenantService = serviceProvider.GetRequiredService<ITenantService>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("[EF] Configuring DbContext with connection string for subdomain: {Subdomain}",
        tenantService.Subdomain);
    optionsBuilder.UseSqlServer(tenantService.GetConnectionString());
});

// * Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);
// * Custom  authentication
builder.Services.AddAuthentication("HeaderAuthentication")
    .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>("HeaderAuthentication", null);

//builder.Services.WoridgeAddAuthenticationSchema(builder.Configuration);

// * Dependency injection
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITemplateRendererService, TemplateRendererService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IAppService, AppService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<INotificationRequestFactory, NotificationRequestFactory>();
builder.Services.AddSingleton<IUserIdProvider, CinUserIdProvider>();
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
builder.Services.AddScoped<ITemplateProvider, TemplateProvider>();
builder.Services.AddHostedService<CreateUserConsumer>();//démarre automatiquement avec l'application.
builder.Services.AddHostedService<ResignationConsumer>();
builder.Services.AddHostedService<AbsenceAlertRhConsumer>();
builder.Services.AddHostedService<AbsenceAlertCeoConsumer>();
builder.Services.AddHostedService<ChangePasswordOtpRequestedConsumer>();
builder.Services.AddHostedService<OvertimeHandleConsumer>();
builder.Services.AddHostedService<LeaveHandleConsumer>();
builder.Services.AddHostedService<UserSyncConsumer>();
builder.Services.AddHostedService<DocumentRequestCreatedConsumer>();
builder.Services.AddHostedService<InterviewCreatedConsumer>();
builder.Services.AddHostedService<InterviewStatusChangedConsumer>();
builder.Services.AddHostedService<EvaluationSubmittedConsumer>();
builder.Services.AddHostedService<TenantReadyConsumer>();
builder.Services.AddScoped<TenantConnectionFactory>();
builder.Services.AddHostedService<ContractGeneratedConsumer>();
builder.Services.AddSingleton<IPdfGeneratorService, PdfGeneratorService>();

// * SignalR
builder.Services.AddSignalR();

// * SMTP settings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// * RabbitMQ settings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
Console.WriteLine(builder.Configuration["Cors:AllowedOrigins"]);
// * CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("WoridgeCorsPolicy", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())  
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---- Build ----
var app = builder.Build();
app.UseCors("WoridgeCorsPolicy");
app.UseWebSockets();
// * Middleware
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<GlobalExceptionHandler>();


// * Endpoints
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapGet("/health-API", () => "Healthcheck, MS-notification is UP");
app.MapControllers();
app.MapHub<NotificationHub>("/api/notification/notificationHub");

// * Migrations
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
db.Database.Migrate();

app.Run();