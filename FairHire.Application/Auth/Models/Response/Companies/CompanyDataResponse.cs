using FairHire.Domain;

namespace FairHire.Application.Auth.Models.Response.Companies;

public sealed record CompanyDataResponse
{
    public required string Email { get; set; }

    public required string Name { get; set; }

    public required List<string> Skills { get; set; }

    public required List<TestTask> TestTasks { get; set; }
}
