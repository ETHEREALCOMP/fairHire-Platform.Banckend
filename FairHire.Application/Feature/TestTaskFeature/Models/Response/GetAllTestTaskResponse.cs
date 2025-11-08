using FairHire.Domain;

namespace FairHire.Application.Feature.TestTaskFeature.Models.Response;

public sealed class GetAllTestTaskResponse
{
    public List<TestTask>? CreatedTasks { get; set; } = [];
}
