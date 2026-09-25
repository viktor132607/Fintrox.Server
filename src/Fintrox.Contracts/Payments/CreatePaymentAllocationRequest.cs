using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Payments;

public sealed record CreatePaymentAllocationRequest(
    [property: Required] string TargetType,
    Guid TargetDocumentId,
    [property: Range(typeof(decimal), "0.0001", "999999999999999.9999")] decimal DocumentAmount);
