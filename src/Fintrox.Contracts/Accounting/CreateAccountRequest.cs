using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record CreateAccountRequest(
    [property: Required, MaxLength(32)] string Code,
    [property: Required, MaxLength(200)] string Name,
    [property: Required, MaxLength(32)] string Type,
    Guid? ParentAccountId,
    bool IsAnalytical,
    bool AllowManualPosting);
