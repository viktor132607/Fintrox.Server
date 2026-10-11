namespace Fintrox.Modules.Budgeting.Domain.Entities;

public sealed class BudgetVersion : OrganizationEntity
{
    public BudgetVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid budgetId,
        int versionNumber,
        string name)
        : base(id, organizationId, createdAtUtc)
    {
        BudgetId = DomainRules.Id(budgetId, nameof(budgetId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        Name = DomainRules.Text(name, nameof(name));
    }

    public Guid BudgetId { get; }
    public int VersionNumber { get; }
    public string Name { get; }
}
