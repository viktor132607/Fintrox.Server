namespace Fintrox.Modules.Experience.Domain.Entities;

public sealed class UserExperiencePreference : OrganizationEntity
{
    public UserExperiencePreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        Guid profileId)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        ProfileId = DomainRules.Id(profileId, nameof(profileId));
    }

    public Guid UserId { get; }
    public Guid ProfileId { get; }
}
