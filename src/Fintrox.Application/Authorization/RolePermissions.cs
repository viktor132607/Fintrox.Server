using Fintrox.Domain.Organizations;

namespace Fintrox.Application.Authorization;

public static class RolePermissions
{
    private static readonly IReadOnlyDictionary<OrganizationRole, IReadOnlyCollection<string>> Map =
        new Dictionary<OrganizationRole, IReadOnlyCollection<string>>
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
