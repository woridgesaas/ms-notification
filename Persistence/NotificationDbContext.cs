using Microsoft.EntityFrameworkCore;
using ms_notification.Models;

namespace ms_notification.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    // 🔹 DbSets
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationReceive> NotificationReceives { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🔹 Clé composite pour table pivot
        modelBuilder.Entity<NotificationReceive>()
            .HasKey(nr => new { nr.NotificationId, nr.EmployeeId });

        // 🔹 Relation NotificationReceive → Notification
        modelBuilder.Entity<NotificationReceive>()
            .HasOne(nr => nr.Notification)
            .WithMany(n => n.Receivers)
            .HasForeignKey(nr => nr.NotificationId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 Relation NotificationReceive → Employee
        modelBuilder.Entity<NotificationReceive>()
            .HasOne(nr => nr.Employee)
            .WithMany(e => e.ReceivedNotifications)
            .HasForeignKey(nr => nr.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);

        // 🔹 Relation Notification → CreatedByEmployee
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.CreatedByEmployee)
            .WithMany(e => e.CreatedNotifications)
            .HasForeignKey(n => n.CreatedByEmployeeId);
    }
}