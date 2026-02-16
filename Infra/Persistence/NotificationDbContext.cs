using Microsoft.EntityFrameworkCore;
using ms_notification.Models;

namespace ms_notification.Infra.Persistence
{
    // Cette classe représente le contexte de la base de données
    // Elle hérite de DbContext (obligatoire pour EF Core)
    public class NotificationDbContext : DbContext
    {
        // Constructeur obligatoire pour injecter la configuration
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        // Tables de la base de données
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationReceive> NotificationReceives { get; set; }

        // Configuration des relations et clés composites
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Clé primaire composite pour la table many-to-many
            modelBuilder.Entity<NotificationReceive>()
                .HasKey(nr => new { nr.NotificationId, nr.EmployeeId });

            // Relation Notification -> NotificationReceive (1 to many)
            modelBuilder.Entity<NotificationReceive>()
                .HasOne(nr => nr.Notification)
                .WithMany(n => n.Receivers)
                .HasForeignKey(nr => nr.NotificationId);

            // Relation Employee -> NotificationReceive (1 to many)
            modelBuilder.Entity<NotificationReceive>()
                .HasOne(nr => nr.Employee)
                .WithMany(e => e.ReceivedNotifications)
                .HasForeignKey(nr => nr.EmployeeId);
        }
    }
}
