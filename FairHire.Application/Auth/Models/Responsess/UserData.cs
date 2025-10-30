using FairHire.Domain;

namespace FairHire.Application.Auth.Models.Responsess;

public sealed record UserData(string Email, string Name, 
    IList<string> Skills, IList<TestTask> TestTasks);
