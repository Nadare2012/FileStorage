using FileStorage.RestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.RestApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected AppDbContext()
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<File> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<User>().Property(u => u.UserName).IsRequired();
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.Password).IsRequired();

            modelBuilder.Entity<File>().HasKey(p => p.FileId);
            modelBuilder.Entity<File>().Property(p => p.FilePath).IsRequired();
            modelBuilder.Entity<File>().HasOne(p => p.User).WithMany(u => u.Files).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
