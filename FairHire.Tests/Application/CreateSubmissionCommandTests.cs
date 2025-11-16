using FairHire.Application.Feature.SubmissionFeature.Command;
using FairHire.Application.Feature.SubmissionFeature.Models.Request;
using FairHire.Domain.Enums;
using FairHire.Domain.Simulations;
using FairHire.Infrastructure.Postgres;
using FairHire.Tests.Domain;
using FairHire.Tests.Infrastructure;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Tests.Application;

public sealed class CreateSubmissionCommandTests
{
    private static FairHireDbContext CreateDb() => InMemoryFairHireDbContext.CreateDbContext();

    [Fact]
    public async Task ExecuteAsync_Succeeds_ForCandidateOfSimulation()
    {
        // ARRANGE
        var db = CreateDb();
        var companyId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();

        var sim = new Simulation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            CandidateUserId = candidateId,
            Name = "Sim",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddDays(5),
            Status = SimulationStatus.Active,
            CreatedByUserId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        db.Simulations.Add(sim);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = candidateId,
            IsCandidate = true
        };

        var command = new CreateSubmissionCommand(db, currentUser);

        var request = new SubmissionCreateRequest(
            SimulationId: sim.Id,
            WorkItemId: null,
            RepoUrl: " https://github.com/user/repo ",
            PullRequestUrl: null,
            CommitSha: " abc123 ",
            FileId: null
        );

        // ACT
        var result = await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        var submission = await db.Submissions.FindAsync(result.Id);
        submission.Should().NotBeNull();
        submission!.CandidateUserId.Should().Be(candidateId);
        submission.RepoUrl.Should().Be("https://github.com/user/repo");
        submission.CommitSha.Should().Be("abc123");
        submission.Status.Should().Be(SubmissionStatus.Pending);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenNoRepoUrlAndNoFileId()
    {
        // ARRANGE
        var db = CreateDb();
        var candidateId = Guid.NewGuid();

        var sim = new Simulation
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            CandidateUserId = candidateId,
            Name = "Sim",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddDays(5),
            Status = SimulationStatus.Active,
            CreatedByUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        db.Simulations.Add(sim);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = candidateId,
            IsCandidate = true
        };

        var command = new CreateSubmissionCommand(db, currentUser);

        var request = new SubmissionCreateRequest(
            SimulationId: sim.Id,
            WorkItemId: null,
            RepoUrl: null,
            PullRequestUrl: null,
            CommitSha: null,
            FileId: null
        );

        // ACT
        var act = async () => await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<ValidationException>();
    }
}
