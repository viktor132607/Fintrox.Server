namespace Fintrox.Application.Integrations;

public static class IntegrationBusinessOperations
{
    public const string Sales = "sales";
    public const string Payment = "payment";
    public const string Expense = "expense";
    public const string Counterparty = "counterparty";

    public static readonly IReadOnlyCollection<string> All =
    [
        Sales,
        Payment,
        Expense,
        Counterparty
    ];
}
