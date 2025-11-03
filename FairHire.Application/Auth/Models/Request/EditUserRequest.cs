using System.ComponentModel.DataAnnotations;

namespace FairHire.Application.Auth.Models.Request;

public sealed record EditUserRequest
{
    [Required] public Guid UserId { get; init; }

    // базові поля User
    [MaxLength(256)] public string? Name { get; init; }

    // поля CompanyProfile (ігноруються, якщо користувач не Company)
    [MaxLength(256)] public string? CompanyName { get; init; }
    [MaxLength(512)] public string? Address { get; init; }
    [MaxLength(512)] public string? Website { get; init; }

    // поля DeveloperProfile (ігноруються, якщо не Developer)
    public List<string>? Skills { get; init; }
}
