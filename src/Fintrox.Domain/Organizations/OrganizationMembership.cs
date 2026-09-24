using Fintrox.Domain.Common;

namespace Fintrox.Domain.Organizations;

public sealed class OrganizationMembership : OrganizationScopedAuditableEntity
{
    private OrganizationMembership()
    {
    }

    private OrganizationMembership(
        Guid id,
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        UserId = userId;
        Role = role;
        IsActive = true;
    }

    public Guid UserId { get; private set; }

    public OrganizationRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public static OrganizationMembership Create(
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset now)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id is required.",
                nameof(userId));
        }

        return new OrganizationMembership(
            Guid.NewGuid(),
            organizationId,
            userId,
            role,
            now);
    }

    public void ChangeRole(OrganizationRole role, DateTimeOffset now)
    {
        Role = role;
        Touch(now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(now);
    }

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(now);
    }
}
