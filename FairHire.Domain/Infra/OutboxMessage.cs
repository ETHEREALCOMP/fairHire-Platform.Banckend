namespace FairHire.Domain.Infra;

public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string AggregateType { get; set; }
    public required string AggregateId { get; set; }
    public required string Type { get; set; }
    public required string PayloadJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
}
