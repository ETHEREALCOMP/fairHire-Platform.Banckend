using FairHire.Domain;
using FairHire.Domain.CompanyRubrics;
using FairHire.Domain.Enums;
using FairHire.Domain.Infra;
using FairHire.Domain.Simulations;
using FairHire.Domain.SubmissionsAndAssessments;
using FairHire.Domain.TaskLibrary;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Infrastructure.Postgres;

public sealed class FairHireDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public FairHireDbContext(DbContextOptions<FairHireDbContext> options)
        : base(options) { }

    // IdentityDbContext вже має DbSet<User>, але це не заважає:
    public new DbSet<User> Users => Set<User>();

    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();

    public DbSet<TaskTemplate> TaskTemplates => Set<TaskTemplate>();
    public DbSet<TaskTemplateChecklist> TaskTemplateChecklist => Set<TaskTemplateChecklist>();
    public DbSet<TaskTemplateRubric> TaskTemplateRubrics => Set<TaskTemplateRubric>();

    public DbSet<Simulation> Simulations => Set<Simulation>();
    public DbSet<SimulationWorkItem> SimulationWorkItems => Set<SimulationWorkItem>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Rubric> Rubrics => Set<Rubric>();

    public DbSet<FileObject> Files => Set<FileObject>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AnalyticsSnapshot> AnalyticsSnapshots => Set<AnalyticsSnapshot>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // 1. Обов'язково спочатку
        base.OnModelCreating(b);

        // 2. Enum-и (Npgsql)
        b.HasPostgresEnum<UserRole>();
        b.HasPostgresEnum<TemplateStatus>();
        b.HasPostgresEnum<SimulationStatus>();
        b.HasPostgresEnum<WorkItemStatus>();
        b.HasPostgresEnum<SubmissionStatus>();
        b.HasPostgresEnum<AssessmentDecision>();
        b.HasPostgresEnum<NotificationChannel>();
        b.HasPostgresEnum<NotificationStatus>();
        b.HasPostgresEnum<AnalyticsScope>();
        b.HasPostgresEnum<AnalyticsPeriod>();

        // 3. Identity таблиці (кастомні назви, але ключі залишаємо з base)
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
        });

        b.Entity<IdentityRole<Guid>>().ToTable("roles");
        b.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        b.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        b.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        b.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        b.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");

        // 4. CompanyProfile
        b.Entity<CompanyProfile>(e =>
        {
            e.ToTable("company_profiles");
            e.HasKey(x => x.UserId);
            e.HasOne(x => x.User)
                .WithOne(x => x.CompanyProfile)
                .HasForeignKey<CompanyProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.LogoFile)
                .WithMany()
                .HasForeignKey(x => x.LogoFileId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.Name);
        });

        // 5. CandidateProfile
        b.Entity<CandidateProfile>(e =>
        {
            e.ToTable("candidate_profiles");
            e.HasKey(x => x.UserId);

            e.HasOne(x => x.User)
                .WithOne(x => x.CandidateProfile)
                .HasForeignKey<CandidateProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.Stacks).HasColumnType("text[]");
            e.HasIndex(x => x.OpenToWork);
            e.HasIndex(x => x.Level);
        });

        // 6. TaskTemplate
        b.Entity<TaskTemplate>(e =>
        {
            e.ToTable("task_templates");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.CreatedByCompany)
                .WithMany(x => x.TaskTemplates)
                .HasForeignKey(x => x.CreatedByCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Tags).HasColumnType("text[]");

            e.HasIndex(x => new { x.CreatedByCompanyId, x.NormalizedTitle }).IsUnique();
            e.HasIndex(x => x.Status);
        });

        b.Entity<TaskTemplateChecklist>(e =>
        {
            e.ToTable("task_template_checklist");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.TaskTemplate)
                .WithMany(x => x.Checklist)
                .HasForeignKey(x => x.TaskTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<TaskTemplateRubric>(e =>
        {
            e.ToTable("task_template_rubrics");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.TaskTemplate)
                .WithOne(x => x.DefaultRubric)
                .HasForeignKey<TaskTemplateRubric>(x => x.TaskTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.CriteriaJson).HasColumnType("jsonb");
        });

        // 7. Simulation
        b.Entity<Simulation>(e =>
        {
            e.ToTable("simulations");
            e.HasKey(x => x.Id);

            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.MetricsJson).HasColumnType("jsonb");

            e.HasOne(x => x.Company)
                .WithMany(x => x.Simulations)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Candidate)
                .WithMany()
                .HasForeignKey(x => x.CandidateUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.CompanyId);
            e.HasIndex(x => x.CandidateUserId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.StartUtc);
            e.HasIndex(x => x.EndUtc);

            e.HasIndex(x => new { x.CompanyId, x.CandidateUserId, x.Status })
                .HasDatabaseName("ux_sim_active_per_pair")
                .IsUnique()
                .HasFilter("\"Status\" = 'Active'");
        });

        b.Entity<SimulationWorkItem>(e =>
        {
            e.ToTable("simulation_work_items");
            e.HasKey(x => x.Id);

            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Tags).HasColumnType("text[]");
            e.Property(x => x.ChecklistJson).HasColumnType("jsonb");

            e.HasOne(x => x.Simulation)
                .WithMany(x => x.WorkItems)
                .HasForeignKey(x => x.SimulationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.SourceTaskTemplate)
                .WithMany()
                .HasForeignKey(x => x.SourceTaskTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.SimulationId);
            e.HasIndex(x => x.Status);
        });

        // 8. Submission
        b.Entity<Submission>(e =>
        {
            e.ToTable("submissions");
            e.HasKey(x => x.Id);

            e.Property(x => x.Status).HasConversion<string>();

            e.HasOne(x => x.Simulation)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.SimulationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.WorkItem)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.WorkItemId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.Candidate)
                .WithMany()
                .HasForeignKey(x => x.CandidateUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.File)
                .WithMany()
                .HasForeignKey(x => x.FileId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.SimulationId);
            e.HasIndex(x => x.WorkItemId);
            e.HasIndex(x => x.CandidateUserId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CreatedAt);
        });

        // 9. Assessment
        b.Entity<Assessment>(e =>
        {
            e.ToTable("assessments");
            e.HasKey(x => x.Id);

            e.Property(x => x.Decision).HasConversion<string>();
            e.Property(x => x.ScoresJson).HasColumnType("jsonb");

            e.HasOne(x => x.Submission)
                .WithOne(x => x.Assessment)
                .HasForeignKey<Assessment>(x => x.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Reviewer)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.ReviewerUserId);
            e.HasIndex(x => x.Decision);
            e.HasIndex(x => x.DecidedAt);
            e.HasIndex(x => x.SubmissionId).IsUnique();
        });

        // 10. Rubric
        b.Entity<Rubric>(e =>
        {
            e.ToTable("rubrics");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Company)
                .WithMany(x => x.Rubrics)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.CriteriaJson).HasColumnType("jsonb");

            e.HasIndex(x => x.CompanyId);
            e.HasIndex(x => x.IsDefault);
        });

        // 11. FileObject
        b.Entity<FileObject>(e =>
        {
            e.ToTable("files");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.OwnerUserId);
            e.HasIndex(x => x.CompanyId);
            e.HasIndex(x => x.CreatedAt);
        });

        // 12. Notification
        b.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(x => x.Id);

            e.Property(x => x.Channel).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.PayloadJson).HasColumnType("jsonb");

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.SentAt);
        });

        // 13. Outbox
        b.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox");
            e.HasKey(x => x.Id);

            e.Property(x => x.PayloadJson).HasColumnType("jsonb");

            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.ProcessedAt);
            e.HasIndex(x => x.Type);
        });

        // 14. AuditLog
        b.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);

            e.Property(x => x.BeforeJson).HasColumnType("jsonb");
            e.Property(x => x.AfterJson).HasColumnType("jsonb");

            e.HasIndex(x => new { x.EntityType, x.EntityId });
            e.HasIndex(x => x.ActorUserId);
            e.HasIndex(x => x.CreatedAt);
        });

        // 15. AnalyticsSnapshot
        b.Entity<AnalyticsSnapshot>(e =>
        {
            e.ToTable("analytics_snapshots");
            e.HasKey(x => x.Id);

            e.Property(x => x.Scope).HasConversion<string>();
            e.Property(x => x.Period).HasConversion<string>();
            e.Property(x => x.PayloadJson).HasColumnType("jsonb");

            e.HasIndex(x => new { x.Scope, x.ScopeId, x.Period, x.Date }).IsUnique();
        });
    }
}
