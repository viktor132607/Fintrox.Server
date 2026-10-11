namespace Fintrox.Modules.HumanResources.Domain.Entities;

public sealed class Employee : OrganizationEntity
{
    public Employee(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string employeeNumber,
        string givenName,
        string familyName,
        DateOnly hiredOn)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeNumber = DomainRules.Text(employeeNumber, nameof(employeeNumber));
        GivenName = DomainRules.Text(givenName, nameof(givenName));
        FamilyName = DomainRules.Text(familyName, nameof(familyName));
        HiredOn = DomainRules.Date(hiredOn, nameof(hiredOn));
    }

    public string EmployeeNumber { get; }
    public string GivenName { get; }
    public string FamilyName { get; }
    public DateOnly HiredOn { get; }
}
