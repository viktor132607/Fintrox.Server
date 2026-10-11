namespace Fintrox.Modules.HumanResources.Domain.Entities;

public sealed class JobOpening : OrganizationEntity
{
    public JobOpening(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string title,
        Guid departmentId,
        DateOnly opensOn,
        DateOnly closesOn)
        : base(id, organizationId, createdAtUtc)
    {
        Title = DomainRules.Text(title, nameof(title));
        DepartmentId = DomainRules.Id(departmentId, nameof(departmentId));
        OpensOn = DomainRules.Date(opensOn, nameof(opensOn));
        ClosesOn = DomainRules.Date(closesOn, nameof(closesOn));
        if (ClosesOn < OpensOn) throw new ArgumentException("The end cannot precede the start.", nameof(closesOn));
    }

    public string Title { get; }
    public Guid DepartmentId { get; }
    public DateOnly OpensOn { get; }
    public DateOnly ClosesOn { get; }
}
