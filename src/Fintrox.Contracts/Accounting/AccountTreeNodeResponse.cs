namespace Fintrox.Contracts.Accounting;

public sealed record AccountTreeNodeResponse(
    AccountResponse Account,
    IReadOnlyList<AccountTreeNodeResponse> Children);
