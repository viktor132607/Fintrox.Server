namespace Fintrox.Domain.Accounting;

public enum PostingRuleMatchKind
{
    Default = 0,
    ItemCode = 10,
    VatCode = 20,
    PaymentMethod = 30,
    ProductCategory = 40
}
