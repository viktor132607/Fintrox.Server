using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Purchases;

public sealed record CancelPurchaseDocumentRequest(
    [param: Required, MaxLength(500)] string Reason);
