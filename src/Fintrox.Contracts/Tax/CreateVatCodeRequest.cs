using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Tax;

public sealed record CreateVatCodeRequest(
    [param: Required, MaxLength(32)] string Code,
    [param: Required, MaxLength(160)] string Name,
    [param: Required] string Kind,
    [param: Range(typeof(decimal), "0", "100")] decimal RatePercent,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool AppliesToSales,
    bool AppliesToPurchases);
