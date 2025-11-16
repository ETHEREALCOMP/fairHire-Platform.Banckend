using FairHire.Application.Auth.Commnad;
using FairHire.Application.Auth.Models.Request;
using FairHire.Domain;
using FairHire.Infrastructure.Postgres;
using FairHire.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace FairHire.Tests.Application;

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

        // 👇 Сіємо роль з NormalizedName, інакше AddToRoleAsync її "не бачить"
        db.Roles.Add(new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = "Company",
            NormalizedName = "COMPANY"
        });

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
}