using System.ComponentModel.DataAnnotations;
using FairHire.Application.Feature.TaskFeature.Command;
using FairHire.Application.Feature.TaskFeature.Models.Request;
using FairHire.Domain;
using FairHire.Domain.Enums;
using FairHire.Domain.TaskLibrary;
using FairHire.Infrastructure.Postgres;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FairHire.Tests;

public sealed class CreateTaskTemplateCommandTests
{
    private static FairHireDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FairHireDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FairHireDbContext(options);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCompanyHasProfile_CreatesTemplateSuccessfully()
    {
        // ARRANGE
        var db = CreateDbContext();

        var companyId = Guid.NewGuid();

        db.CompanyProfiles.Add(new CompanyProfile
        {
            UserId = companyId,
            Name = "Test Company"
        });

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new CreateTaskTemplateCommand(db, currentUser);

        var request = new TaskTemplateCreateRequest(
            Title: "Junior Backend Task",
            Description: "Implement small API endpoint.",
            Level: "Junior",
            Tags: new List<string> { "C#", "ASP.NET Core" },
            EstimatedHours: 4,
            Attachments: new List<string> { "https://example.com/spec" }
        );

        // ACT
        var result = await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        var template = await db.TaskTemplates.FindAsync(result.Id);
        template.Should().NotBeNull();
        template!.CreatedByCompanyId.Should().Be(companyId);
        template.Title.Should().Be("Junior Backend Task");
        template.NormalizedTitle.Should().Be("JUNIOR BACKEND TASK");
        template.Tags.Should().BeEquivalentTo(new[] { "C#", "ASP.NET Core" });
        template.EstimatedHours.Should().Be(4);
        template.Status.Should().Be(TemplateStatus.Draft);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTemplateWithSameTitleExists_ThrowsValidationException()
    {
        // ARRANGE
        var db = CreateDbContext();

        var companyId = Guid.NewGuid();

        db.CompanyProfiles.Add(new CompanyProfile
        {
            UserId = companyId,
            Name = "Test Company"
        });

        db.TaskTemplates.Add(new TaskTemplate
        {
            Id = Guid.NewGuid(),
            CreatedByCompanyId = companyId,
            Title = "Duplicate title",
            NormalizedTitle = "DUPLICATE TITLE",
            Status = TemplateStatus.Draft,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUser
        {
            UserId = companyId,
            IsCompany = true
        };

        var command = new CreateTaskTemplateCommand(db, currentUser);

        var request = new TaskTemplateCreateRequest(
            Title: " Duplicate title ",   // з пробілами, щоб перевірити нормалізацію
            Description: "Some desc",
            Level: null,
            Tags: null,
            EstimatedHours: null,
            Attachments: null
        );

        // ACT
        Func<Task> act = async () =>
        {
            await command.ExecuteAsync(request, CancellationToken.None);
        };

        // ASSERT
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("*already exists*");
    }
}
