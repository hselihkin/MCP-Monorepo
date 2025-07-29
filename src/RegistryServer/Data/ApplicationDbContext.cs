using Microsoft.EntityFrameworkCore;
using RegistryServer.Models;

namespace RegistryServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Server> Servers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table name explicitly (optional since we used [Table] attribute)
            modelBuilder.Entity<Server>()
                .ToTable("Server");

            modelBuilder.Entity<Server>()
                .Property(s => s.ToolsJson)
                .HasColumnName("Tools");

            modelBuilder.Entity<Server>()
                .Property(s => s.ArgumentsJson)
                .HasColumnName("Arguments");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
