using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MimeKit;
using ms_notification.Hubs;
using ms_notification.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
//Mapping Section in .json to the object c#
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

//Add to service : instanciation
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("WoridgeCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200") //Angular client
              .AllowAnyHeader()                     // Headers
              .AllowAnyMethod()                   
              .AllowCredentials(); //like jwts or cookies                 
    });
});
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
var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

try
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(smtpSettings.SenderName, smtpSettings.SenderEmail));
    message.To.Add(new MailboxAddress("Test Health", "test@example.com"));
    message.Subject = "Health Check : Notification Service";

    message.Body = new TextPart("plain")
    {
        Text = "This is just healthy check :)"
    };
    using var client = new SmtpClient();
    
    client.Connect(smtpSettings.Server, smtpSettings.Port, false);
    client.Send(message);
    client.Disconnect(true);
    Console.WriteLine($"Success : Connected to SMTP : {smtpSettings.Server}");
}
catch (Exception ex)
{
    
    Console.WriteLine($" Error : cannot connect to {smtpSettings.Server}. : {ex.Message}");
}
app.UseCors("WoridgeCorsPolicy");
app.MapHub<NotificationHub>("/notificationHub");
app.Run();
