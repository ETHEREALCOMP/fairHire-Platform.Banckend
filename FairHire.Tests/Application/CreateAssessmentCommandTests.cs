using FairHire.Application.Feature.AssessmentFeature.Command;
using FairHire.Application.Feature.AssessmentFeature.Models.Request;
using FairHire.Domain.Enums;
using FairHire.Domain.Simulations;
using FairHire.Domain.SubmissionsAndAssessments;
using FairHire.Infrastructure.Postgres;
using FairHire.Tests.Domain;
using FairHire.Tests.Infrastructure;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Tests.Application;

public sealed class CreateAssessmentCommandTests
{
    private static FairHireDbContext CreateDb() => InMemoryFairHireDbContext.CreateDbContext();

    [Fact]
    public async Task ExecuteAsync_Succeeds_ForOwnerCompany_ComputesTotalScore()
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

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            SimulationId = sim.Id,
            CandidateUserId = candidateId,
            Status = SubmissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.Simulations.Add(sim);
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new CreateAssessmentCommand(db, currentUser);

        var scores = new Dictionary<string, int>
        {
            ["codeQuality"] = 4,
            ["communication"] = 3,
            ["timeliness"] = 5
        };

        var request = new AssessmentCreateRequest(
            SubmissionId: submission.Id,
            Scores: scores,
            Decision: "Accepted",
            Comment: "Solid performance"
        );

        // ACT
        var result = await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        var assessment = await db.Assessments.FindAsync(result.Id);
        assessment.Should().NotBeNull();
        assessment!.SubmissionId.Should().Be(submission.Id);
        assessment.Decision.Should().Be(AssessmentDecision.Accepted);

        var expectedAvg = scores.Values.Average();    // 4.0
        var expectedTotal = (int)Math.Round(expectedAvg * 20, MidpointRounding.AwayFromZero); // 80
        assessment.TotalScore.Should().Be(expectedTotal);

        var updatedSubmission = await db.Submissions.FindAsync(submission.Id);
        updatedSubmission!.Status.Should().Be(SubmissionStatus.Reviewed);
        updatedSubmission.ReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenScoresEmpty()
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

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            SimulationId = sim.Id,
            CandidateUserId = candidateId,
            Status = SubmissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.Simulations.Add(sim);
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new CreateAssessmentCommand(db, currentUser);

        var request = new AssessmentCreateRequest(
            SubmissionId: submission.Id,
            Scores: new Dictionary<string, int>(),
            Decision: "Accepted",
            Comment: "ok"
        );

        // ACT
        var act = async () => await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<ValidationException>();
    }
}
