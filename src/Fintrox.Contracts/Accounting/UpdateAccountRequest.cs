using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateAccountRequest(
    [property: Required, MaxLength(32), RegularExpression("^[A-Za-z0-9][A-Za-z0-9./-]*$")] string Code,
    [property: Required, MaxLength(200)] string Name,
    [property: Required, MaxLength(32)] string Type,
    Guid? ParentAccountId,
    bool IsAnalytical,
    bool AllowManualPosting);
