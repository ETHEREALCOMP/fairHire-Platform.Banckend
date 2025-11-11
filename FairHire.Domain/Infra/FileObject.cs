namespace FairHire.Domain.Infra;

public sealed class FileObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public Guid? CompanyId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long Size { get; set; }
    public required string StorageKey { get; set; }
    public string? Checksum { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Owner { get; set; } = default!;
    public CompanyProfile? Company { get; set; }
}
