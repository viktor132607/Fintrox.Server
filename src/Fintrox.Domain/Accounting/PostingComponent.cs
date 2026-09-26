namespace Fintrox.Domain.Accounting;

public enum PostingComponent
{
    AccountsReceivable = 0,
    AccountsPayable = 10,
    Revenue = 20,
    Expense = 30,
    OutputVat = 40,
    InputVat = 50,
    PaymentAsset = 60,
    CustomerAdvance = 70,
    SupplierAdvance = 80,
    FxGain = 90,
    FxLoss = 100
}
