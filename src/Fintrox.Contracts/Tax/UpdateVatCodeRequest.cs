using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Tax;

public sealed record UpdateVatCodeRequest(
    [property: Required, MaxLength(32)] string Code,
    [property: Required, MaxLength(160)] string Name,
    [property: Required] string Kind,
    [property: Range(typeof(decimal), "0", "100")] decimal RatePercent,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool AppliesToSales,
    bool AppliesToPurchases);
