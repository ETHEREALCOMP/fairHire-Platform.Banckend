using FairHire.Domain;

namespace FairHire.Application.Auth.Models.Request.Users;

public sealed record UserSignUpRequest
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string ConfPassword { get; set; }

    public required string Name { get; set; }

    public required string Role { get; set; }

    public List<string>? Skills { get; set; } = [];
}
