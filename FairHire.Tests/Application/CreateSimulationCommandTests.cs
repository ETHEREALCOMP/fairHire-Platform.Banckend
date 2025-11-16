using FairHire.Application.Feature.SimulationFeature.Command;
using FairHire.Application.Feature.SimulationFeature.Modles.Request;
using FairHire.Domain;
using FairHire.Domain.Enums;
using FairHire.Domain.TaskLibrary;
using FairHire.Infrastructure.Postgres;
using FairHire.Tests.Domain;
using FairHire.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Tests.Application;

public sealed class CreateSimulationCommandTests
{
    private static FairHireDbContext CreateDb() => InMemoryFairHireDbContext.CreateDbContext();

    [Fact]
    public async Task ExecuteAsync_CreatesSimulationWithWorkItems()
    {
        // ARRANGE
        var db = CreateDb();

        var companyId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = candidateId,
            Email = "dev@example.com",
            UserName = "dev@example.com",
            Name = "Candidate"
        });

        db.CompanyProfiles.Add(new CompanyProfile
        {
            UserId = companyId,
            Name = "Test Company"
        });

        var template1 = new TaskTemplate
        {
            Id = Guid.NewGuid(),
            CreatedByCompanyId = companyId,
            Title = "Implement API",
            NormalizedTitle = "IMPLEMENT API",
            Description = "REST endpoint",
            Level = "Junior",
            Tags = new List<string> { "C#", "ASP.NET Core" },
            Status = TemplateStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var template2 = new TaskTemplate
        {
            Id = Guid.NewGuid(),
            CreatedByCompanyId = companyId,
            Title = "Fix bug",
            NormalizedTitle = "FIX BUG",
            Description = "Fix null ref",
            Level = "Junior",
            Tags = new List<string> { "C#", "Debug" },
            Status = TemplateStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        db.TaskTemplates.AddRange(template1, template2);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new CreateSimulationCommand(db, currentUser);

        var now = DateTime.UtcNow;

        var request = new SimulationCreateRequest(
            CandidateUserId: candidateId,
            Name: "Backend Simulation",
            StartUtc: now,
            EndUtc: now.AddDays(5),
            BaseRepoUrl: "https://github.com/company/base-repo",
            BaseProjectRef: "main",
            TaskTemplateIds: new List<Guid> { template1.Id, template2.Id }
        );

        // ACT
        var result = await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        result.Id.Should().NotBe(Guid.Empty);

        var sim = await db.Simulations
            .Include(s => s.WorkItems)
            .FirstOrDefaultAsync(s => s.Id == result.Id);

        sim.Should().NotBeNull();
        sim!.CompanyId.Should().Be(companyId);
        sim.CandidateUserId.Should().Be(candidateId);
        sim.Status.Should().Be(SimulationStatus.Scheduled);
        sim.WorkItems.Should().HaveCount(2);
        sim.WorkItems.Select(w => w.Title)
            .Should().BeEquivalentTo(new[] { template1.Title, template2.Title });
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenEndBeforeStart()
    {
        // ARRANGE
        var db = CreateDb();
        var companyId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = candidateId,
            Email = "dev@example.com",
            UserName = "dev@example.com",
            Name = "Candidate"
        });
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new CreateSimulationCommand(db, currentUser);

        var now = DateTime.UtcNow;

        var request = new SimulationCreateRequest(
            CandidateUserId: candidateId,
            Name: "Invalid Simulation",
            StartUtc: now,
            EndUtc: now.AddHours(-1),
            BaseRepoUrl: null,
            BaseProjectRef: null,
            TaskTemplateIds: []
        );

        // ACT
        var act = async () => await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<ValidationException>();
    }
}
