namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned VatReturn; tenant scope and input fields validated at creation.</summary>
public sealed class VatReturn : OrganizationEntity
{
    public VatReturn(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid vatReturnPeriodId,
        string returnReference,
        DateTimeOffset preparedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        VatReturnPeriodId = DomainRules.Id(vatReturnPeriodId, nameof(vatReturnPeriodId));
        ReturnReference = DomainRules.Text(returnReference, nameof(returnReference));
        PreparedAtUtc = DomainRules.Instant(preparedAtUtc, nameof(preparedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid VatReturnPeriodId { get; }
    public string ReturnReference { get; }
    public DateTimeOffset PreparedAtUtc { get; }
    public string Status { get; }
}
