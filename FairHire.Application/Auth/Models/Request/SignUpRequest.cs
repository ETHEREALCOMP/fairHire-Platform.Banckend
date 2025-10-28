namespace FairHire.Application.Auth.Models.Request;

public sealed record SignUpRequest
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string ConfPassword { get; set; }

    public required string Name { get; set; }
}
