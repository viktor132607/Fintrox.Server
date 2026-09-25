namespace Fintrox.Application.Authorization;

public static class Permissions
{
    public const string OrganizationsRead = "organizations.read";
    public const string OrganizationsManage = "organizations.manage";
    public const string MembersManage = "members.manage";

    public const string AccountingRead = "accounting.read";
    public const string AccountingWrite = "accounting.write";
    public const string AccountingPost = "accounting.post";

    public const string CounterpartiesRead = "counterparties.read";
    public const string CounterpartiesWrite = "counterparties.write";

    public const string TaxRead = "tax.read";
    public const string TaxWrite = "tax.write";

    public const string CurrenciesRead = "currencies.read";
    public const string CurrenciesWrite = "currencies.write";

    public const string SalesRead = "sales.read";
    public const string SalesWrite = "sales.write";

    public const string PurchasesRead = "purchases.read";
    public const string PurchasesWrite = "purchases.write";

    public const string PaymentsRead = "payments.read";
    public const string PaymentsWrite = "payments.write";

    public const string ReportsRead = "reports.read";
    public const string IntegrationsManage = "integrations.manage";

    public static readonly IReadOnlyCollection<string> All =
    [
        OrganizationsRead,
        OrganizationsManage,
        MembersManage,
        AccountingRead,
        AccountingWrite,
        AccountingPost,
        CounterpartiesRead,
        CounterpartiesWrite,
        TaxRead,
        TaxWrite,
        CurrenciesRead,
        CurrenciesWrite,
        SalesRead,
        SalesWrite,
        PurchasesRead,
        PurchasesWrite,
        PaymentsRead,
        PaymentsWrite,
        ReportsRead,
        IntegrationsManage
    ];
}
