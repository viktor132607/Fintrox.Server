namespace Fintrox.Domain.Common;

public abstract class AuditableEntity : Entity, IAuditableEntity
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id, DateTimeOffset now) : base(id)
    {
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public void SetCreationAudit(DateTimeOffset timestampUtc, Guid? userId)
    {
        CreatedAtUtc = timestampUtc;
        CreatedByUserId = userId;
        UpdatedAtUtc = timestampUtc;
        UpdatedByUserId = userId;
    }

    public void SetModificationAudit(DateTimeOffset timestampUtc, Guid? userId)
    {
        UpdatedAtUtc = timestampUtc;
        UpdatedByUserId = userId;
    }

    protected void Touch(DateTimeOffset now)
    {
        UpdatedAtUtc = now;
    }
}
