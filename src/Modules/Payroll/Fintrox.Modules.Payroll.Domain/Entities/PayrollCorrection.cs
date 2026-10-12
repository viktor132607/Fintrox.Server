namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollCorrection; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollCorrection : OrganizationEntity
{
    public PayrollCorrection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid originalPayslipId,
        Guid correctedPayslipId,
        string reason,
        DateOnly correctedOn)
        : base(id, organizationId, createdAtUtc)
    {
        OriginalPayslipId = DomainRules.Id(originalPayslipId, nameof(originalPayslipId));
        CorrectedPayslipId = DomainRules.Id(correctedPayslipId, nameof(correctedPayslipId));
        Reason = DomainRules.Text(reason, nameof(reason));
        CorrectedOn = DomainRules.Date(correctedOn, nameof(correctedOn));
    }

    public Guid OriginalPayslipId { get; }
    public Guid CorrectedPayslipId { get; }
    public string Reason { get; }
    public DateOnly CorrectedOn { get; }
}
