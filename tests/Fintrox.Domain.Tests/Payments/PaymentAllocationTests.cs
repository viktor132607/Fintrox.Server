using Fintrox.Domain.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Payments;

[TestClass]
public sealed class PaymentAllocationTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PaymentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SalesInvoiceId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid PurchaseDocumentId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid CurrencyId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 20, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void CreateSalesInvoiceAllocationNormalizesSnapshot()
    {
        var allocation = CreateSalesAllocation();

        Assert.AreEqual(PaymentAllocationTargetType.SalesInvoice, allocation.TargetType);
        Assert.AreEqual(SalesInvoiceId, allocation.SalesInvoiceId);
        Assert.IsNull(allocation.PurchaseDocumentId);
        Assert.AreEqual("INV-2026-1", allocation.DocumentNumber);
        Assert.AreEqual("USD", allocation.DocumentCurrencyCode);
        Assert.AreEqual(50m, allocation.DocumentAmount);
        Assert.AreEqual(45m, allocation.PaymentAmount);
    }

    [TestMethod]
    public void CreateRejectsTargetMismatch()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            PaymentAllocation.Create(
                OrganizationId,
                PaymentId,
                1,
                PaymentAllocationTargetType.SalesInvoice,
                SalesInvoiceId,
                PurchaseDocumentId,
                "INV-1",
                CurrencyId,
                "USD",
                1.2m,
                50m,
                45m,
                Now));
    }

    [TestMethod]
    public void CreateRejectsNonPositiveAmounts()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            PaymentAllocation.Create(
                OrganizationId,
                PaymentId,
                1,
                PaymentAllocationTargetType.SalesInvoice,
                SalesInvoiceId,
                null,
                "INV-1",
                CurrencyId,
                "USD",
                1.2m,
                0m,
                45m,
                Now));
    }

    [TestMethod]
    public void UpdateCanSwitchTargetAtomically()
    {
        var allocation = CreateSalesAllocation();

        allocation.Update(
            PaymentAllocationTargetType.PurchaseDocument,
            null,
            PurchaseDocumentId,
            " BILL-2026-5 ",
            CurrencyId,
            "bgn",
            1m,
            30m,
            30m,
            Now.AddMinutes(1));

        Assert.AreEqual(PaymentAllocationTargetType.PurchaseDocument, allocation.TargetType);
        Assert.IsNull(allocation.SalesInvoiceId);
        Assert.AreEqual(PurchaseDocumentId, allocation.PurchaseDocumentId);
        Assert.AreEqual("BILL-2026-5", allocation.DocumentNumber);
        Assert.AreEqual("BGN", allocation.DocumentCurrencyCode);
        Assert.AreEqual(30m, allocation.DocumentAmount);
        Assert.AreEqual(30m, allocation.PaymentAmount);
    }

    [TestMethod]
    public void UpdateInvalidCurrencyDoesNotPartiallyMutateAllocation()
    {
        var allocation = CreateSalesAllocation();

        Assert.ThrowsExactly<ArgumentException>(() =>
            allocation.Update(
                PaymentAllocationTargetType.PurchaseDocument,
                null,
                PurchaseDocumentId,
                "BILL-2026-5",
                CurrencyId,
                "BGNN",
                1m,
                30m,
                30m,
                Now.AddMinutes(1)));

        Assert.AreEqual(PaymentAllocationTargetType.SalesInvoice, allocation.TargetType);
        Assert.AreEqual(SalesInvoiceId, allocation.SalesInvoiceId);
        Assert.IsNull(allocation.PurchaseDocumentId);
        Assert.AreEqual("INV-2026-1", allocation.DocumentNumber);
        Assert.AreEqual("USD", allocation.DocumentCurrencyCode);
        Assert.AreEqual(50m, allocation.DocumentAmount);
        Assert.AreEqual(45m, allocation.PaymentAmount);
    }

    private static PaymentAllocation CreateSalesAllocation() =>
        PaymentAllocation.Create(
            OrganizationId,
            PaymentId,
            1,
            PaymentAllocationTargetType.SalesInvoice,
            SalesInvoiceId,
            null,
            " INV-2026-1 ",
            CurrencyId,
            " usd ",
            1.2m,
            50m,
            45m,
            Now);
}
