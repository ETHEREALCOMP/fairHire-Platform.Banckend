namespace FairHire.Application.Auth.Models.Request;

public sealed record SignUpRequest
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string ConfPassword { get; init; } = default!;
    public string? Name { get; init; }

    // "company" або "developer"
    public string Role { get; init; } = default!;

    // Поля для Company-профілю (ігноруються, якщо роль Developer)
    public string? CompanyName { get; init; }
    public string? Address { get; init; }
    public string? Website { get; init; }

    // Поля для Developer-профілю (ігноруються, якщо роль Company)
    public List<string>? Skills { get; init; }
}
