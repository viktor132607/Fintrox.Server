using Fintrox.Domain.Organizations;

namespace Fintrox.Application.Authorization;

public static class RolePermissions
{
    private static readonly Dictionary<OrganizationRole, IReadOnlyCollection<string>> Map =
        new()
        {
            [OrganizationRole.Owner] = Permissions.All,
            [OrganizationRole.Administrator] =
            [
                Permissions.OrganizationsRead,
                Permissions.OrganizationsManage,
                Permissions.MembersManage,
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
                Permissions.ReportsRead,
                Permissions.IntegrationsManage
            ],
            [OrganizationRole.Accountant] =
            [
                Permissions.OrganizationsRead,
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
            ],
            [OrganizationRole.Operator] =
            [
                Permissions.OrganizationsRead,
                Permissions.AccountingRead,
                Permissions.CounterpartiesRead,
                Permissions.CounterpartiesWrite,
                Permissions.TaxRead,
                Permissions.CurrenciesRead,
                Permissions.SalesRead,
                Permissions.SalesWrite,
                Permissions.PurchasesRead,
                Permissions.PurchasesWrite,
                Permissions.PaymentsRead,
                Permissions.PaymentsWrite,
                Permissions.ReportsRead
            ],
            [OrganizationRole.Viewer] =
            [
                Permissions.OrganizationsRead,
                Permissions.AccountingRead,
                Permissions.CounterpartiesRead,
                Permissions.TaxRead,
                Permissions.CurrenciesRead,
                Permissions.SalesRead,
                Permissions.PurchasesRead,
                Permissions.PaymentsRead,
                Permissions.ReportsRead
            ]
        };

    public static IReadOnlyCollection<string> For(OrganizationRole role) => Map[role];

    public static bool Has(OrganizationRole role, string permission)
    {
        return Map.TryGetValue(role, out var permissions) &&
               permissions.Contains(permission, StringComparer.Ordinal);
    }
}
