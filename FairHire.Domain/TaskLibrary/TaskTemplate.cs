using FairHire.Domain.Enums;

namespace FairHire.Domain.TaskLibrary;

public sealed class TaskTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CreatedByCompanyId { get; set; } // FK -> CompanyProfile.UserId
    public required string Title { get; set; }
    public required string NormalizedTitle { get; set; }
    public string? Description { get; set; }

    public string? Level { get; set; } // optional level label
    public List<string> Tags { get; set; } = []; // text[]
    public int? EstimatedHours { get; set; }
    public List<string> Attachments { get; set; } = []; // urls/keys
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public CompanyProfile CreatedByCompany { get; set; } = default!;
    public List<TaskTemplateChecklist> Checklist { get; set; } = new();
    public TaskTemplateRubric? DefaultRubric { get; set; }

    public bool IsDeleted { get; set; } = false;
}
