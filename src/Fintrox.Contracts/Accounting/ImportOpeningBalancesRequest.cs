using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

/// <summary>
/// One balanced, initial general-ledger position for a single fiscal year.
/// Posting is all-or-nothing and exactly one import is allowed per organization/year.
/// </summary>
public sealed record ImportOpeningBalancesRequest(
    Guid FiscalYearId,
    [param: Required, MinLength(2), MaxLength(1000)]
    IReadOnlyList<CreateJournalLineRequest> Lines);
