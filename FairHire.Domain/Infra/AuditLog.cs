namespace FairHire.Domain.Infra;

public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActorUserId { get; set; }
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public required string Action { get; set; } // Create/Update/Delete/StateChange
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
}
