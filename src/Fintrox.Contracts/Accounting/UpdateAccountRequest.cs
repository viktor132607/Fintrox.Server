using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateAccountRequest(
    [param: Required, MaxLength(32), RegularExpression("^[A-Za-z0-9][A-Za-z0-9./-]*$")] string Code,
    [param: Required, MaxLength(200)] string Name,
    [param: Required, MaxLength(32)] string Type,
    Guid? ParentAccountId,
    bool IsAnalytical,
    bool AllowManualPosting);
