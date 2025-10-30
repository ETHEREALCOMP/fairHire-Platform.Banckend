namespace FairHire.Application.Auth.Models.Responsess;

public sealed record SignInResponse 
{
    public required string Token { get; set; }

    public Guid Id { get; set; }
}
