namespace FairHire.Application.Auth.Models.Request;

public sealed record EditUserDataRequest(string? Email, 
    string? Name, List<string>? Skills);
