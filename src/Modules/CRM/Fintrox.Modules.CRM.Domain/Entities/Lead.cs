namespace Fintrox.Modules.CRM.Domain.Entities;

public sealed class Lead : OrganizationEntity
{
    public Lead(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string subject,
        string contactName,
        string email)
        : base(id, organizationId, createdAtUtc)
    {
        Subject = DomainRules.Text(subject, nameof(subject));
        ContactName = DomainRules.Text(contactName, nameof(contactName));
        Email = DomainRules.Text(email, nameof(email));
    }

    public string Subject { get; }
    public string ContactName { get; }
    public string Email { get; }
}
