using Fintrox.Domain.Common;

namespace Fintrox.Domain.Organizations;

public sealed class OrganizationMembership : IOrganizationScopedEntity
{
    private OrganizationMembership()
    {
    }

    private OrganizationMembership(
        Guid id,
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset now)
    {
        Id = id;
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        IsActive = true;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid UserId { get; private set; }

    public OrganizationRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static OrganizationMembership Create(
        Guid organizationId,
        Guid userId,
        OrganizationRole role,
        DateTimeOffset now)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id is required.", nameof(organizationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
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
        UpdatedAtUtc = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAtUtc = now;
    }

    public void Activate(DateTimeOffset now)
    {
        IsActive = true;
        UpdatedAtUtc = now;
    }
}
