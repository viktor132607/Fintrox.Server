namespace Fintrox.Application.Common.Interfaces;

public interface ICurrentOrganization
{
    Guid? OrganizationId { get; }

    Guid RequireOrganizationId();
}
