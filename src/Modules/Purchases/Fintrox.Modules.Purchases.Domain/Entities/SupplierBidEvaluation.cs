namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierBidEvaluation model. References require application validation.</summary>
public sealed class SupplierBidEvaluation : OrganizationEntity
{
    public SupplierBidEvaluation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierBidId,
        Guid evaluatorUserId,
        int evaluationScore,
        DateOnly evaluatedOn,
        string notes)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierBidId = DomainRules.Id(supplierBidId, nameof(supplierBidId));
        EvaluatorUserId = DomainRules.Id(evaluatorUserId, nameof(evaluatorUserId));
        EvaluationScore = DomainRules.NonnegativeInt(evaluationScore, nameof(evaluationScore));
        EvaluatedOn = DomainRules.Date(evaluatedOn, nameof(evaluatedOn));
        Notes = DomainRules.Text(notes, nameof(notes));
    }

    public Guid SupplierBidId { get; }
    public Guid EvaluatorUserId { get; }
    public int EvaluationScore { get; }
    public DateOnly EvaluatedOn { get; }
    public string Notes { get; }
}
