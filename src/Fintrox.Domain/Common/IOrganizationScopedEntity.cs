namespace Fintrox.Domain.Common;

public interface IOrganizationScopedEntity
{
    Guid OrganizationId { get; }
}
