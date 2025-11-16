using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using FairHire.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;

namespace FairHire.Tests.Application.Auth;

public sealed class UserSignUpCommandTests
{
    private static (FairHireDbContext db, UserManager<User> userManager) CreateContextAndUserManager()
    {
        var db = InMemoryFairHireDbContext.CreateDbContext();

        var store = new UserStore<User, IdentityRole<Guid>, FairHireDbContext, Guid>(db);
        var userManager = new UserManager<User>(
            store,
            null,
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            null
        );

        db.Roles.AddRange(
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = "Candidate",
                NormalizedName = "CANDIDATE"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = "Company",
                NormalizedName = "COMPANY"
            }
        );

        db.SaveChanges();

        return (db, userManager);
    }

    [Fact]
    public async Task SignUp_Company_CreatesUserAndCompanyProfile()
    {
        // ARRANGE
        var (db, userManager) = CreateContextAndUserManager();

        var command = new UserSignUpCommand(userManager, db);

        var request = new SignUpRequest
        {
            Email = "company@example.com",
            Password = "String123!",
            ConfPassword = "String123!",
            Name = "Test Company Owner",
            Role = "Company",
            CompanyName = "Awesome Co",
            Address = "Somewhere",
            Website = "https://awesome.co"
        };

        // ACT
        var result = await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        var user = await userManager.FindByIdAsync(result.Id.ToString());
        user.Should().NotBeNull();
        user!.Email.Should().Be("company@example.com");

        var roles = await userManager.GetRolesAsync(user);
        roles.Should().ContainSingle().Which.Should().Be("Company");

        var companyProfile = await db.CompanyProfiles
            .FirstOrDefaultAsync(x => x.UserId == user.Id, CancellationToken.None);
        companyProfile.Should().NotBeNull();
        companyProfile!.Name.Should().Be("Awesome Co");
    }

    [Fact]
    public async Task SignUp_Fails_WhenUserWithSameEmailExists()
    {
        // ARRANGE
        var (db, userManager) = CreateContextAndUserManager();

        var existing = new User
        {
            Id = Guid.NewGuid(),
            Email = "dup@example.com",
            NormalizedEmail = "DUP@EXAMPLE.COM",
            UserName = "dup@example.com",
            NormalizedUserName = "DUP@EXAMPLE.COM",
            Name = "Existing User"
        };

        db.Users.Add(existing);
        await db.SaveChangesAsync();

        var command = new UserSignUpCommand(userManager, db);

        var request = new SignUpRequest
        {
            Email = "dup@example.com",
            Password = "String123!",
            ConfPassword = "String123!",
            Name = "Test",
            Role = "Company"
        };

        // ACT
        var act = async () => await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateNameException>();
    }

    [Fact]
    public async Task SignUp_Fails_WhenPasswordMissing() 
    {
        var (db, userManager) = CreateContextAndUserManager();

        var command = new UserSignUpCommand(userManager, db);

        var request = new SignUpRequest// <- 
        {
            Email = "company@example.com",
            Name = "Test Company Owner",
            Role = "Company",
            CompanyName = "Awesome Co",
            Address = "Somewhere",
            Website = "https://awesome.co"
        };

        // ACT
        var act = async () => await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Password is required."); ;
    }

    [Fact]
    public async Task SignUp_Fails_WhenPasswordsDoNotMatch() 
    {
        var (db, userManager) = CreateContextAndUserManager();

        var command = new UserSignUpCommand(userManager, db);

        var request = new SignUpRequest
        {
            Email = "company@example.com",
            Name = "Test Company Owner",
            Password = "String123!",
            ConfPassword = "DifferentString321!",// <-
            Role = "Company",
            CompanyName = "Awesome Co",
            Address = "Somewhere",
            Website = "https://awesome.co"
        };

        // ACT
        var act = async () => await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Password and Confirm Password do not match."); ;
    }

    [Fact]
    public async Task SignUp_Fails_WhenRoleInvalid()
    {
        var (db, userManager) = CreateContextAndUserManager();

        var command = new UserSignUpCommand(userManager, db);

        var request = new SignUpRequest
        {
            Email = "company@example.com",
            Name = "Test Company Owner",
            Password = "String123!",
            ConfPassword = "String123!",
            Role = "Tester", //incorrect role
            CompanyName = "Awesome Co",
            Address = "Somewhere",
            Website = "https://awesome.co"
        };

        // ACT
        var act = async () => await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        await act
            .Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Role must be either 'Company' or 'Developer'."); ;
    }

    [Fact]
    public async Task SignUp_Developer_CreatesCandidateProfile_AndAssignsDeveloperRole()
    {
        var (db, userManager) = CreateContextAndUserManager();

        var command = new UserSignUpCommand(userManager, db);

        var request = new SignUpRequest
        {
            Email = "candidate@example.com",
            Name = "Developer",
            Password = "String123!",
            ConfPassword = "String123!",
            Role = "Candidate",
            Timezone = "UTC+01:00",
            Level = "Junior",
            About = "I am backend developer!",
            OpenToWork = true,
            Stacks = new List<string> {"C++", "C#"}
        };

        // ACT
        var result = await command.ExecuteAsync(request, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        // Check user was created
        var user = await userManager.FindByIdAsync(result.Id.ToString());
        user.Should().NotBeNull();
        user!.Email.Should().Be("candidate@example.com");

        // Check role is Candidate
        var roles = await userManager.GetRolesAsync(user);
        roles.Should().ContainSingle().Which.Should().Be("Candidate");

        // CandidateProfile should exist
        var candidateProfile = await db.CandidateProfiles
            .FirstOrDefaultAsync(x => x.UserId == user.Id, CancellationToken.None);

        candidateProfile.Should().NotBeNull();
        candidateProfile.Timezone.Should().Be("UTC+01:00");
        candidateProfile.Level.Should().Be("Junior");
        candidateProfile.About.Should().Be("I am backend developer!");
        candidateProfile.OpenToWork.Should().BeTrue();

        // Check stacks/skills
        candidateProfile.Stacks.Should().NotBeNull();
        candidateProfile.Stacks.Should().BeEquivalentTo(new[] { "C++", "C#" });

        // No CompanyProfile should be created
        var companyProfile = await db.CompanyProfiles
            .FirstOrDefaultAsync(x => x.UserId == user.Id, CancellationToken.None);

        companyProfile.Should().BeNull();
    }

    [Fact]
    public async Task NormalizeRole_Allows_lowercase_input()
    {
        var (db, userManager) = CreateContextAndUserManager();

        var command = new UserSignUpCommand(userManager, db);

        // Candidate signup
        var candidateResult = await command.ExecuteAsync(new SignUpRequest
        {
            Email = "candidate@example.com",
            Name = "Developer",
            Password = "String123!",
            ConfPassword = "String123!",
            Role = "candidate"// <- lower case scenario
        }, CancellationToken.None);

        // Company signup
        var companyResult = await command.ExecuteAsync(new SignUpRequest
        {
            Email = "company@example.com",
            Name = "Company",
            Password = "String123!",
            ConfPassword = "String123!",
            Role = "company"// <- lower case scenario
        }, CancellationToken.None);

        // ASSERT
        candidateResult.Should().NotBeNull();
        candidateResult.Id.Should().NotBe(Guid.Empty);

        companyResult.Should().NotBeNull();
        companyResult.Id.Should().NotBe(Guid.Empty);

        // Check candidate role
        var candidateUser = await userManager.FindByIdAsync(candidateResult.Id.ToString());
        (candidateUser != null).Should().BeTrue();
        var candidateRoles = await userManager.GetRolesAsync(candidateUser!);
        candidateRoles.Should().ContainSingle().Which.Should().Be("Candidate");

        // Check company role
        var companyUser = await userManager.FindByIdAsync(companyResult.Id.ToString());
        (companyUser != null).Should().BeTrue();
        var companyRoles = await userManager.GetRolesAsync(companyUser!);
        companyRoles.Should().ContainSingle().Which.Should().Be("Company");
    }

    [Fact]
    public async Task CurrentUserFromHttp_ParsesClaimsCorrectly()
    {
        var userId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "candidate@example.com"),
            new Claim(ClaimTypes.Role, "Candidate")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = claimsPrincipal;

        // Отримати Id користувача
        var userIdFromClaims = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Отримати email
        var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

        // Отримати роль
        var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        userIdFromClaims.Should().Be(userId.ToString());
        email.Should().Be("candidate@example.com");
        role.Should().Be("Candidate");
    }
}