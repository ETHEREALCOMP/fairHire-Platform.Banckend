namespace FairHire.Application.Auth.Models.Request;

public sealed record SignInRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }

    public string? DesiredRole { get; set; }
}
