using Microsoft.EntityFrameworkCore;
using ms_notification.Models;

namespace ms_notification.Data;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{

    //* DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationReceive> NotificationReceives { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
         {
             user.ToTable("users");

             user.HasKey(x => x.Id);

             user.Property(x => x.Id).HasColumnName("id");

             user.Property(x => x.CIN).IsRequired().HasMaxLength(15);

             user.Property(x => x.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(50);

             user.Property(x => x.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(50);

             user.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(100);

             //* Unique constraints
             user.HasIndex(x => x.Email).IsUnique();

             user.HasIndex(x => x.CIN).IsUnique();
         });

        modelBuilder.Entity<Notification>(notification =>
    {
        notification.ToTable("notifications");

        notification.HasKey(x => x.Id);

        notification.Property(x => x.Id).HasColumnName("id");

        notification.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(50);

        notification.Property(x => x.Description).HasColumnName("description").HasColumnType("nvarchar(max)").IsRequired(false);

        notification.Property(x => x.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(50).IsRequired();

        notification.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(50).IsRequired();

        notification.Property(x => x.Priority).HasColumnName("priority").HasConversion<string>().HasMaxLength(50).IsRequired();

        notification.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();

        notification.Property(x => x.TemplateCode).HasColumnName("template_code").HasConversion<string>().HasMaxLength(50).IsRequired();

        notification.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        notification.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();

        //* Relationship
        notification.HasOne(x => x.User).WithMany(user => user.CreatedNotifications).HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    });

        modelBuilder.Entity<NotificationReceive>(notificationReceive =>
         {
             notificationReceive.ToTable("notification_receives");

             notificationReceive.HasKey(x => new { x.NotificationId, x.UserId });

             notificationReceive.Property(x => x.NotificationId).HasColumnName("notification_id");

             notificationReceive.Property(x => x.UserId).HasColumnName("employee_id");

             notificationReceive.Property(x => x.IsRead).HasColumnName("is_read").IsRequired();

             notificationReceive.Property(x => x.ReadDate).HasColumnName("read_date");

             //* Relationships
             notificationReceive.HasOne(x => x.Notification).WithMany(n => n.Receivers).HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);

             notificationReceive.HasOne(x => x.User).WithMany(user => user.ReceivedNotifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
         });
    }
}