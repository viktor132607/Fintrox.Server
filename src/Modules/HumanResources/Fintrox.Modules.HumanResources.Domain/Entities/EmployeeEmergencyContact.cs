namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeEmergencyContact domain model.</summary>
public sealed class EmployeeEmergencyContact : OrganizationEntity
{
    public EmployeeEmergencyContact(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string contactName,
        string relationship,
        string phoneNumber,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        ContactName = DomainRules.Text(contactName, nameof(contactName));
        Relationship = DomainRules.Text(relationship, nameof(relationship));
        PhoneNumber = DomainRules.Text(phoneNumber, nameof(phoneNumber));
        IsPrimary = isPrimary;
    }

    public Guid EmployeeId { get; }
    public string ContactName { get; }
    public string Relationship { get; }
    public string PhoneNumber { get; }
    public bool IsPrimary { get; }
}
