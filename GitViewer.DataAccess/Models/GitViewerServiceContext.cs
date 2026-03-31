using Microsoft.EntityFrameworkCore;
namespace GitViewer.DataAccess.Models
{
    public class GitViewerServiceContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Repository> Repository { get; set; }
        public DbSet<ShareLink> ShareLinks { get; set; }


        public GitViewerServiceContext(DbContextOptions<GitViewerServiceContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(user =>
            {
                user.Property(e => e.UserName).IsRequired();
                user.Property(e => e.Email).IsRequired(false);
                user.Property(e => e.PasswordHash).IsRequired();
                user.Property(e => e.Role).IsRequired();
            });

            modelBuilder.Entity<ShareLink>(shareLink =>
            {
                shareLink.Property(e => e.Name).IsRequired(false);
                shareLink.Property(e => e.Created).IsRequired();
                shareLink.Property(e => e.UserId).IsRequired(false);
                shareLink.Property(e => e.ExpiryTime).IsRequired(false);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

    }
}