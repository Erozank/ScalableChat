using Microsoft.EntityFrameworkCore;

namespace ChatWithSignalR.UsersDb
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nickname).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Nickname).IsUnique();
            });
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public required string Nickname { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}
