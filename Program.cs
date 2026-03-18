using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ms_notification.Config;
using ms_notification.Data;
using ms_notification.Hubs;
using ms_notification.Messaging.Consumers;
using ms_notification.Options;
using ms_notification.Repositories;
using ms_notification.Services;
using Serilog;
using System.Text;

//Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Charge aussi les variables d'environnement (apr�s Env.Load)
builder.Configuration.AddEnvironmentVariables();
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"DEBUG CONNECTION STRING: {connString}");

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITemplateRendererService, TemplateRendererService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IAppService, AppService>();
builder.Services.AddSingleton<IUserIdProvider, CinUserIdProvider>();
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>(); 
builder.Services.AddScoped<ITemplateProvider, TemplateProvider>();
builder.Services.AddHostedService<CreateUserConsumer>();//démarre automatiquement avec l'application.
// Serilog (avant Build)
builder.Host.UseSerilog((context, configuration) =>
 configuration.ReadFrom.Configuration(context.Configuration));
/*builder.Host.UseSerilog((hostingContext, configuration) =>
{
configuration.ReadFrom.Configuration(hostingContext.Configuration)
.WriteTo.Console();
});*/
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


        policy.SetIsOriginAllowed(_ => true)
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
        /** 
            1)Authorize header in websocket are not allowed so we sent token in queryparam
            2)Only for trafic camme to /notificationHub
         */
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"].ToString();
                var isHub = context.HttpContext.Request.Path.StartsWithSegments("/notificationHub");

                if (isHub && !string.IsNullOrEmpty(token))
                    context.Token = token;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
   

}
// Apply migrations
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
db.Database.Migrate();
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

app.Run();