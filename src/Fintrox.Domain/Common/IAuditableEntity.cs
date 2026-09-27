namespace Fintrox.Domain.Common;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; }

    Guid? CreatedByUserId { get; }

    DateTimeOffset UpdatedAtUtc { get; }

    Guid? UpdatedByUserId { get; }

    void SetCreationAudit(DateTimeOffset timestampUtc, Guid? userId);

    void SetModificationAudit(DateTimeOffset timestampUtc, Guid? userId);
}
