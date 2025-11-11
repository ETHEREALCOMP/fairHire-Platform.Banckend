using FairHire.Domain.Enums;

namespace FairHire.Domain.Infra;

public sealed class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.Email;
    public required string TemplateKey { get; set; }
    public required string PayloadJson { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Queued;
    public DateTime? SentAt { get; set; }
    public string? Error { get; set; }

    public User User { get; set; } = default!;
}
