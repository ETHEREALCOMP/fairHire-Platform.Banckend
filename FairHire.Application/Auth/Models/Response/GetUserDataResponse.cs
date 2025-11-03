using FairHire.Domain;

namespace FairHire.Application.Auth.Models.Response;

public sealed record GetUserDataResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string[] Roles { get; init; } = Array.Empty<string>();

    public Company? CompanyProfile { get; init; }
    public Developer? DeveloperProfile { get; init; }

    public sealed class Company
    {
        public string Name { get; init; } = default!;
        public string? Address { get; init; }
        public string? Website { get; init; }
    }

    public sealed class Developer
    {
        public List<string> Skills { get; init; } = [];
    }
}
