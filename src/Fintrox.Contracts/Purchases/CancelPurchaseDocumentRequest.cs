using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Purchases;

public sealed record CancelPurchaseDocumentRequest(
    [property: Required, MaxLength(500)] string Reason);
