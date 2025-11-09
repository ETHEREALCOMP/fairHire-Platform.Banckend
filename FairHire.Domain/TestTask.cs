namespace FairHire.Domain;

public sealed class TestTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public string? NormalizedTitle { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDateUtc { get; set; } // Термін виконання завдання
    public string Status { get; set; } = "New"; // New/InProgress/Done/Rejected/Updated

    // Хто створив (компанія)
    public Guid CreatedByCompanyId { get; set; }      // FK -> CompanyProfile.UserId
    public CompanyProfile? CreatedByCompany { get; set; }

    // Кому призначено (дев-юзер)
    public Guid? AssignedToUserId { get; set; }        // FK -> AspNetUsers.Id
    public User? AssignedToUser { get; set; }
}
