using FairHire.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace FairHire.Infrastructure.Postgres;

public sealed class AppDbContext: IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<DeveloperProfile> DeveloperProfiles => Set<DeveloperProfile>();
    public DbSet<TestTask> TestTasks => Set<TestTask>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ---------- User ----------
        b.Entity<User>(e =>
        {
            e.Property(u => u.Name).IsRequired().HasMaxLength(256);

            // 1:1 на CompanyProfile
            e.HasOne(u => u.CompanyProfile)
             .WithOne(cp => cp.User)
             .HasForeignKey<CompanyProfile>(cp => cp.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // 1:1 на DeveloperProfile
            e.HasOne(u => u.DeveloperProfile)
             .WithOne(dp => dp.User)
             .HasForeignKey<DeveloperProfile>(dp => dp.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // 1:many AssignedTasks — зворотня в TestTask
        });

        // ---------- CompanyProfile ----------
        b.Entity<CompanyProfile>(e =>
        {
            e.HasKey(cp => cp.UserId);
            e.Property(cp => cp.Name).IsRequired().HasMaxLength(256);
            e.Property(cp => cp.Address).HasMaxLength(512);
            e.Property(cp => cp.Website).HasMaxLength(512);

            // Індекс на Name (опціонально унікальний)
            e.HasIndex(cp => cp.Name).IsUnique(false);
        });

        // ---------- DeveloperProfile ----------
        b.Entity<DeveloperProfile>(e =>
        {
            e.HasKey(dp => dp.UserId);

            // Skills як JSONB у Postgres
            var converter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            var comparer = new ValueComparer<List<string>>(
                (a, b) => a!.SequenceEqual(b!),
                v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                v => v.ToList());

            e.Property(dp => dp.Skills)
             .HasConversion(converter, comparer)
             .HasColumnType("jsonb")
             .HasDefaultValueSql("'[]'::jsonb");
        });

        // ---------- TestTask ----------
        b.Entity<TestTask>(e =>
        {
            e.Property(t => t.Title).IsRequired().HasMaxLength(256);
            e.Property(t => t.Status).IsRequired().HasMaxLength(64);

            // FK на компанію (через CompanyProfile.UserId)
            e.HasOne(t => t.CreatedByCompany)
             .WithMany(c => c.CreatedTasks)
             .HasForeignKey(t => t.CreatedByCompanyId)
             .OnDelete(DeleteBehavior.Restrict);

            // FK на дев-юзера
            e.HasOne(t => t.AssignedToUser)
             .WithMany(u => u.AssignedTasks)
             .HasForeignKey(t => t.AssignedToUserId)
             .OnDelete(DeleteBehavior.Restrict);

            // Корисні індекси
            e.HasIndex(t => t.CreatedByCompanyId);
            e.HasIndex(t => t.AssignedToUserId);
            e.HasIndex(t => new { t.Status, t.DueDateUtc });
        });


        b.Entity<TestTask>(e => {e.HasQueryFilter(t => !t.IsDeleted);}); //soft delete global filter
    }
}
