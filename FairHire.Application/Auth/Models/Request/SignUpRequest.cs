namespace FairHire.Application.Auth.Models.Request;

public sealed record SignUpRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfPassword { get; set; } = default!;
    public string? Name { get; set; }

    // "company" або "developer"
    public string Role { get; set; } = default!;

    // Поля для Company-профілю (ігноруються, якщо роль Developer)
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }

    // Поля для Developer-профілю (ігноруються, якщо роль Company)
    public string? Timezone { get; set; }
    public string? Level { get; set; }// "Junior/Middle/Senior"
    public List<string>? Stacks { get; set; }
    public string? About { get; set; }
    public bool OpenToWork { get; set; } = true;
}
