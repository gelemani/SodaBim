using Microsoft.EntityFrameworkCore;
using B.Models;

namespace B.Data
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectFile> ProjectFiles { get; set; }
        public DbSet<ProjectAccess> ProjectAccesses { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<IfcCollision> IfcCollisions { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            // Database.EnsureCreated(); // Ensure the database is created if it does not exist
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Login).IsRequired();
                entity.Property(u => u.UserName).IsRequired();
                entity.Property(u => u.UserSurname).IsRequired();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.Password).IsRequired(); // Ensure password is required
                entity.Property(u => u.CompanyName).IsRequired();
                entity.Property(u => u.CompanyPosition).IsRequired();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Title).IsRequired();
                entity.Property(p => p.CreatedAt).IsRequired();
                entity.Property(p => p.LastModified).IsRequired();
                entity.Property(p => p.CreatorId).IsRequired();

                entity.HasOne(p => p.Creator)
                    .WithMany()
                    .HasForeignKey(p => p.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProjectFile>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.FileName).IsRequired().HasMaxLength(255);
                entity.Property(f => f.FileData).IsRequired();
                entity.Property(f => f.CreatedAt).IsRequired();
                entity.Property(f => f.LastModified).IsRequired();
                entity.Property(f => f.ContentType).IsRequired().HasMaxLength(100);

                entity.HasOne(f => f.Project)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(f => f.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProjectAccess>(entity =>
            {
                entity.HasKey(pa => pa.Id);
                entity.Property(pa => pa.AccessLevel).IsRequired();
                entity.Property(pa => pa.GrantedAt).IsRequired();

                entity.HasOne(pa => pa.Project)
                    .WithMany(p => p.ProjectAccesses)
                    .HasForeignKey(pa => pa.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pa => pa.User)
                    .WithMany()
                    .HasForeignKey(pa => pa.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Text).IsRequired().HasMaxLength(1000);
                entity.Property(c => c.ElementName).IsRequired().HasMaxLength(255);
                entity.Property(c => c.ElementId).IsRequired();
                entity.Property(c => c.CreatedAt).IsRequired();
                entity.Property(c => c.LastModified).IsRequired();

                entity.HasOne(c => c.File)
                    .WithMany()
                    .HasForeignKey(c => c.FileId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<IfcCollision>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ElementA).IsRequired();
                entity.Property(c => c.ElementB).IsRequired();
                entity.Property(c => c.FileId).IsRequired();
                entity.HasOne(c => c.File)
                    .WithMany()
                    .HasForeignKey(c => c.FileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseSqlServer("YourConnectionStringHere"); // Update with your connection string

            }
        }
    }
}
