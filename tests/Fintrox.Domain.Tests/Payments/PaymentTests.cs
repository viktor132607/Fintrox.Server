using Fintrox.Domain.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Payments;

[TestClass]
public sealed class PaymentTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CounterpartyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid CurrencyId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid BaseCurrencyId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 20, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void CreateDraftNormalizesValuesAndStartsUnallocated()
    {
        var payment = CreateDraft();

        Assert.AreEqual(PaymentStatus.Draft, payment.Status);
        Assert.AreEqual("EUR", payment.CurrencyCode);
        Assert.AreEqual("ACME", payment.CounterpartyName);
        Assert.AreEqual("REG-1", payment.CounterpartyRegistrationNumber);
        Assert.AreEqual("BG123", payment.CounterpartyVatNumber);
        Assert.AreEqual("REF-1", payment.Reference);
        Assert.AreEqual("Notes", payment.Notes);
        Assert.AreEqual(100m, payment.Amount);
        Assert.AreEqual(100m, payment.UnallocatedAmount);
        Assert.IsNull(payment.InternalNumber);
    }

    [TestMethod]
    public void CreateDraftRejectsNonPositiveAmount()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CreateDraft(0m));
    }

    [TestMethod]
    public void SetAllocatedAmountRejectsOverAllocation()
    {
        var payment = CreateDraft();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => payment.SetAllocatedAmount(100.01m, Now.AddMinutes(1)));

        Assert.AreEqual(0m, payment.AllocatedAmount);
    }

    [TestMethod]
    public void UpdateDraftRejectsAmountBelowAllocatedAmountWithoutMutation()
    {
        var payment = CreateDraft();
        payment.SetAllocatedAmount(80m, Now.AddMinutes(1));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            payment.UpdateDraft(
                PaymentDirection.Outgoing,
                CounterpartyId,
                new DateOnly(2026, 9, 26),
                PaymentMethod.Cash,
                CurrencyId,
                "USD",
                79m,
                "Changed",
                null,
                null,
                null,
                null,
                Now.AddMinutes(2)));

        Assert.AreEqual(PaymentDirection.Incoming, payment.Direction);
        Assert.AreEqual("EUR", payment.CurrencyCode);
        Assert.AreEqual(100m, payment.Amount);
        Assert.AreEqual(80m, payment.AllocatedAmount);
    }

    [TestMethod]
    public void UpdateDraftInvalidCurrencyDoesNotPartiallyMutateAggregate()
    {
        var payment = CreateDraft();

        Assert.ThrowsExactly<ArgumentException>(() =>
            payment.UpdateDraft(
                PaymentDirection.Outgoing,
                CounterpartyId,
                new DateOnly(2026, 9, 26),
                PaymentMethod.Cash,
                CurrencyId,
                "EURO",
                150m,
                "Changed",
                null,
                null,
                null,
                null,
                Now.AddMinutes(1)));

        Assert.AreEqual(PaymentDirection.Incoming, payment.Direction);
        Assert.AreEqual(new DateOnly(2026, 9, 25), payment.PaymentDate);
        Assert.AreEqual(PaymentMethod.BankTransfer, payment.Method);
        Assert.AreEqual("EUR", payment.CurrencyCode);
        Assert.AreEqual(100m, payment.Amount);
        Assert.AreEqual("ACME", payment.CounterpartyName);
    }

    [TestMethod]
    public void ConfirmSnapshotsValuesAndLocksDraftMutation()
    {
        var payment = CreateDraft();
        payment.SetAllocatedAmount(40m, Now.AddMinutes(1));

        payment.Confirm(
            " RCV-2026-000001 ",
            BaseCurrencyId,
            "bgn",
            "eur",
            " ACME Snapshot ",
            " REG-2 ",
            " BG999 ",
            1.95583m,
            Now.AddMinutes(2));

        Assert.AreEqual(PaymentStatus.Confirmed, payment.Status);
        Assert.AreEqual("RCV-2026-000001", payment.InternalNumber);
        Assert.AreEqual("BGN", payment.BaseCurrencyCode);
        Assert.AreEqual("EUR", payment.CurrencyCode);
        Assert.AreEqual("ACME Snapshot", payment.CounterpartyName);
        Assert.AreEqual(1.95583m, payment.ExchangeRate);
        Assert.AreEqual(60m, payment.UnallocatedAmount);
        Assert.ThrowsExactly<InvalidOperationException>(
            () => payment.SetAllocatedAmount(50m, Now.AddMinutes(3)));
    }

    [TestMethod]
    public void ConfirmInvalidSnapshotDoesNotPartiallyConfirm()
    {
        var payment = CreateDraft();

        Assert.ThrowsExactly<ArgumentException>(() =>
            payment.Confirm(
                "RCV-2026-000001",
                BaseCurrencyId,
                "BGNN",
                "EUR",
                "ACME",
                null,
                null,
                1.95583m,
                Now.AddMinutes(1)));

        Assert.AreEqual(PaymentStatus.Draft, payment.Status);
        Assert.IsNull(payment.InternalNumber);
        Assert.IsNull(payment.BaseCurrencyId);
        Assert.IsNull(payment.ExchangeRate);
    }

    [TestMethod]
    public void CancelInvalidReasonDoesNotPartiallyCancel()
    {
        var payment = CreateDraft();
        Confirm(payment);

        Assert.ThrowsExactly<ArgumentException>(
            () => payment.Cancel("   ", Now.AddMinutes(2)));

        Assert.AreEqual(PaymentStatus.Confirmed, payment.Status);
        Assert.IsNull(payment.CancellationReason);
        Assert.IsNull(payment.CancelledAtUtc);
    }

    [TestMethod]
    public void CancelConfirmedPaymentTransitionsOnce()
    {
        var payment = CreateDraft();
        Confirm(payment);

        payment.Cancel(" duplicate receipt ", Now.AddMinutes(2));

        Assert.AreEqual(PaymentStatus.Cancelled, payment.Status);
        Assert.AreEqual("duplicate receipt", payment.CancellationReason);
        Assert.AreEqual(Now.AddMinutes(2), payment.CancelledAtUtc);
        Assert.ThrowsExactly<InvalidOperationException>(
            () => payment.Cancel("again", Now.AddMinutes(3)));
    }

    private static Payment CreateDraft(decimal amount = 100m) =>
        Payment.CreateDraft(
            OrganizationId,
            PaymentDirection.Incoming,
            CounterpartyId,
            new DateOnly(2026, 9, 25),
            PaymentMethod.BankTransfer,
            CurrencyId,
            " eur ",
            amount,
            " ACME ",
            " REG-1 ",
            " BG123 ",
            " REF-1 ",
            " Notes ",
            Now);

    private static void Confirm(Payment payment) =>
        payment.Confirm(
            "RCV-2026-000001",
            BaseCurrencyId,
            "BGN",
            "EUR",
            "ACME",
            "REG-1",
            "BG123",
            1.95583m,
            Now.AddMinutes(1));
}
