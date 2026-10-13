using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Payments;

public sealed record CreatePaymentAllocationRequest(
    [param: Required] string TargetType,
    Guid TargetDocumentId,
    [param: Range(typeof(decimal), "0.0001", "999999999999999.9999")] decimal DocumentAmount);
