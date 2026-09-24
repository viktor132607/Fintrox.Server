namespace Fintrox.Infrastructure.Audit;

public sealed class AuditLogEntry
{
    public Guid Id { get; set; }

    public Guid? OrganizationId { get; set; }

    public Guid? UserId { get; set; }

    public string EntityType { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public AuditAction Action { get; set; }

    public string ChangesJson { get; set; } = null!;

    public string? CorrelationId { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }
}
