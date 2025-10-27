using FairHire.Domain;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Infrastructure.Postgres;

public sealed class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<TestTask> TestTasks { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
