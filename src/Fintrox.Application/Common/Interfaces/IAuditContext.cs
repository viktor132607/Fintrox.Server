namespace Fintrox.Application.Common.Interfaces;

public interface IAuditContext
{
    Guid? UserId { get; }

    Guid? OrganizationId { get; }

    string? CorrelationId { get; }
}
