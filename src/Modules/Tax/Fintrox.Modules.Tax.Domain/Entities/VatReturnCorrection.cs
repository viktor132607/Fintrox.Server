namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned VatReturnCorrection; tenant scope and input fields validated at creation.</summary>
public sealed class VatReturnCorrection : OrganizationEntity
{
    public VatReturnCorrection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid originalVatReturnId,
        int correctionNumber,
        string reason,
        DateOnly correctedOn)
        : base(id, organizationId, createdAtUtc)
    {
        OriginalVatReturnId = DomainRules.Id(originalVatReturnId, nameof(originalVatReturnId));
        CorrectionNumber = DomainRules.PositiveInt(correctionNumber, nameof(correctionNumber));
        Reason = DomainRules.Text(reason, nameof(reason));
        CorrectedOn = DomainRules.Date(correctedOn, nameof(correctedOn));
    }

    public Guid OriginalVatReturnId { get; }
    public int CorrectionNumber { get; }
    public string Reason { get; }
    public DateOnly CorrectedOn { get; }
}
