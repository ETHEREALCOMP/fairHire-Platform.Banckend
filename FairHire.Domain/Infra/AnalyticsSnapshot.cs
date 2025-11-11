using FairHire.Domain.Enums;

namespace FairHire.Domain.Infra;

public sealed class AnalyticsSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AnalyticsScope Scope { get; set; }
    public string ScopeId { get; set; } = default!; // CompanyId/CandidateUserId/"system"
    public AnalyticsPeriod Period { get; set; }
    public DateOnly Date { get; set; }
    public required string PayloadJson { get; set; } // aggregated metrics
}
