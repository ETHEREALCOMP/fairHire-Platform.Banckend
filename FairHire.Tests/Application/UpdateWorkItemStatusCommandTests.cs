using FairHire.Application.Feature.WorkItemFeature.Command;
using FairHire.Domain.Enums;
using FairHire.Domain.Simulations;
using FairHire.Infrastructure.Postgres;
using FairHire.Tests.Domain;
using FairHire.Tests.Infrastructure;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace FairHire.Tests.Application;

public sealed class UpdateWorkItemStatusCommandTests
{
    private static FairHireDbContext CreateDb() => InMemoryFairHireDbContext.CreateDbContext();

    [Fact]
    public async Task ExecuteAsync_ChangesStatus_ForOwnerCompany()
    {
        // ARRANGE
        var db = CreateDb();
        var companyId = Guid.NewGuid();

        var sim = new Simulation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            CandidateUserId = Guid.NewGuid(),
            Name = "Sim",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddDays(5),
            Status = SimulationStatus.Active,
            CreatedByUserId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        var workItem = new SimulationWorkItem
        {
            Id = Guid.NewGuid(),
            SimulationId = sim.Id,
            Title = "Task 1",
            Status = WorkItemStatus.Backlog
        };

        sim.WorkItems.Add(workItem);
        db.Simulations.Add(sim);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new UpdateWorkItemStatusCommand(db, currentUser);

        // ACT
        await command.ExecuteAsync(workItem.Id, "InProgress", CancellationToken.None);

        // ASSERT
        var updated = await db.SimulationWorkItems.FindAsync(workItem.Id);
        updated!.Status.Should().Be(WorkItemStatus.InProgress);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenStatusInvalid()
    {
        // ARRANGE
        var db = CreateDb();
        var companyId = Guid.NewGuid();

        var sim = new Simulation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            CandidateUserId = Guid.NewGuid(),
            Name = "Sim",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddDays(5),
            Status = SimulationStatus.Active,
            CreatedByUserId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        var workItem = new SimulationWorkItem
        {
            Id = Guid.NewGuid(),
            SimulationId = sim.Id,
            Title = "Task 1",
            Status = WorkItemStatus.Backlog
        };

        sim.WorkItems.Add(workItem);
        db.Simulations.Add(sim);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new UpdateWorkItemStatusCommand(db, currentUser);

        // ACT
        var act = async () => await command.ExecuteAsync(workItem.Id, "INVALID", CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<ValidationException>();
    }
}
