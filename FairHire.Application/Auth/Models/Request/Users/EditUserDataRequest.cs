namespace FairHire.Application.Auth.Models.Request.Users;

public sealed record EditUserDataRequest(string? Email, string? Password,
    string? Name, List<string>? Skills);
