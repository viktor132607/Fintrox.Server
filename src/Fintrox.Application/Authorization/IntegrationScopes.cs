namespace Fintrox.Application.Authorization;

public static class IntegrationScopes
{
    public static readonly IReadOnlyCollection<string> Allowed =
    [
        Permissions.AccountingRead,
        Permissions.AccountingWrite,
        Permissions.AccountingPost,
        Permissions.CounterpartiesRead,
        Permissions.CounterpartiesWrite,
        Permissions.TaxRead,
        Permissions.TaxWrite,
        Permissions.CurrenciesRead,
        Permissions.CurrenciesWrite,
        Permissions.SalesRead,
        Permissions.SalesWrite,
        Permissions.PurchasesRead,
        Permissions.PurchasesWrite,
        Permissions.PaymentsRead,
        Permissions.PaymentsWrite,
        Permissions.ReportsRead
    ];

    public static IReadOnlyList<string> Normalize(
        IEnumerable<string>? scopes)
    {
        var normalized = (scopes ?? [])
            .Select(scope => scope?.Trim())
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Select(scope => scope!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(scope => scope, StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "At least one integration scope is required.",
                nameof(scopes));
        }

        var invalid = normalized
            .Where(scope => !Allowed.Contains(scope, StringComparer.Ordinal))
            .ToArray();

        if (invalid.Length > 0)
        {
            throw new ArgumentException(
                $"Unsupported integration scope(s): {string.Join(", ", invalid)}.",
                nameof(scopes));
        }

        return normalized;
    }

    public static string Serialize(IEnumerable<string> scopes) =>
        string.Join(' ', Normalize(scopes));

    public static IReadOnlyList<string> Parse(string value) =>
        Normalize(value.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries));
}
