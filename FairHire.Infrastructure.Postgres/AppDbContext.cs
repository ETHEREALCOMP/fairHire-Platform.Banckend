using FairHire.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Infrastructure.Postgres;

public sealed class AppDbContext: IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<TestTask> TestTasks { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(b =>
        {
            b.Property(u => u.Name).HasMaxLength(100);
            b.Property(u => u.UserName).HasMaxLength(100);
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("IX_User_NormalizedEmail");
        });

    }
}
