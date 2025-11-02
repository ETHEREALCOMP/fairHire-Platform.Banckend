using FairHire.Domain;

namespace FairHire.Application.Auth.Models.Request.Companies;

public sealed record CompanySignUpRequest
{
    public required string Name { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string ConfPassword { get; set; }

    public required string Role { get; set; }
}
