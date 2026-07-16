using Microsoft.EntityFrameworkCore;
using UserRegistration.Domain.Models;

namespace UserRegistration.Infrastructure.Users;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
        {
            // SQLite stores the table as "Users" and the primary key
            // as the rowid (autoincrement) so EF can generate ids on
            // Add without the domain having to track them.
            user.ToTable("Users");
            user.HasKey(u => u.Id);

            user.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            user.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(254);

            user.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(256);

            user.Property(u => u.Phone)
                .HasMaxLength(32);

            user.Property(u => u.AddressLine)
                .IsRequired()
                .HasMaxLength(200);

            user.Property(u => u.AddressComplement)
                .HasMaxLength(200);

            user.Property(u => u.City)
                .IsRequired()
                .HasMaxLength(100);

            user.Property(u => u.State)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength();

            user.Property(u => u.ZipCode)
                .IsRequired()
                .HasMaxLength(9);
        });
    }
}