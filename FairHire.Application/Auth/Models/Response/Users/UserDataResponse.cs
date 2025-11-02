using FairHire.Domain;

namespace FairHire.Application.Auth.Models.Response.Users;

public sealed record UserDataResponse
{
    public required string Email { get; set; }

    public required string Name { get; set; }

    public required List<string> Skills { get; set; }

    public required List<TestTask> TestTasks { get; set; }
}
