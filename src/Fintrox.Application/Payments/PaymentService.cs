using System.Globalization;
using Fintrox.Application.Accounting;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Counterparties;
using Fintrox.Application.Currencies;
using Fintrox.Application.Purchases;
using Fintrox.Application.Sales;
using Fintrox.Contracts.Payments;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Payments;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Sales;

namespace Fintrox.Application.Payments;

public sealed class PaymentService(
    IPaymentRepository repository,
    ICounterpartyRepository counterpartyRepository,
    ICurrencyRepository currencyRepository,
    ISalesInvoiceRepository salesInvoiceRepository,
    IPurchaseDocumentRepository purchaseDocumentRepository,
    IAutoPostingEngine autoPostingEngine,
    ITransactionRunner transactionRunner,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IPaymentService
{
    public async Task<IReadOnlyList<PaymentSummaryResponse>> ListAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        string? direction,
        Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDate, toDate);
        var organizationId = currentOrganization.RequireOrganizationId();

        var payments = await repository.ListPaymentsAsync(
            organizationId,
            fromDate,
            toDate,
            ParseStatus(status),
            ParseDirection(direction),
            counterpartyId,
            cancellationToken);

        return payments.Select(MapSummary).ToArray();
    }

    public async Task<PaymentResponse?> GetAsync(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var payment = await repository.GetPaymentAsync(
            organizationId,
            paymentId,
            trackChanges: false,
            cancellationToken);

        return payment is null
            ? null
            : await BuildResponseAsync(payment, cancellationToken);
    }

    public async Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var payment = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var direction = ParseRequiredDirection(request.Direction);
                var method = ParseRequiredMethod(request.Method);
                var counterparty = await RequireCounterpartyAsync(
                    organizationId,
                    request.CounterpartyId,
                    direction,
                    ct);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    request.CurrencyId,
                    ct);

                ValidateAmountPrecision(request.Amount, currency);

                var draft = Payment.CreateDraft(
                    organizationId,
                    direction,
                    counterparty.Id,
                    request.PaymentDate,
                    method,
                    currency.Id,
                    currency.Code,
                    request.Amount,
                    counterparty.Name,
                    counterparty.RegistrationNumber,
                    counterparty.VatNumber,
                    request.Reference,
                    request.Notes,
                    timeProvider.GetUtcNow());

                await repository.AddPaymentAsync(draft, ct);
                await repository.SaveChangesAsync(ct);
                return draft;
            },
            cancellationToken);

        return await BuildResponseAsync(payment, cancellationToken);
    }

    public async Task<PaymentResponse?> UpdateAsync(
        Guid paymentId,
        UpdatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var payment = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var draft = await repository.GetPaymentAsync(
                    organizationId,
                    paymentId,
                    trackChanges: true,
                    ct);

                if (draft is null)
                {
                    return null;
                }

                EnsureDraft(draft);

                var direction = ParseRequiredDirection(request.Direction);
                var method = ParseRequiredMethod(request.Method);
                var allocations = await repository.ListAllocationsAsync(
                    organizationId,
                    paymentId,
                    trackChanges: false,
                    ct);

                if (allocations.Count > 0 &&
                    (draft.Direction != direction ||
                     draft.CounterpartyId != request.CounterpartyId ||
                     draft.PaymentDate != request.PaymentDate ||
                     draft.CurrencyId != request.CurrencyId))
                {
                    throw new PaymentConflictException(
                        "Direction, counterparty, payment date and currency cannot be changed after allocations have been added.");
                }

                var counterparty = await RequireCounterpartyAsync(
                    organizationId,
                    request.CounterpartyId,
                    direction,
                    ct);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    request.CurrencyId,
                    ct);

                ValidateAmountPrecision(request.Amount, currency);

                draft.UpdateDraft(
                    direction,
                    counterparty.Id,
                    request.PaymentDate,
                    method,
                    currency.Id,
                    currency.Code,
                    request.Amount,
                    counterparty.Name,
                    counterparty.RegistrationNumber,
                    counterparty.VatNumber,
                    request.Reference,
                    request.Notes,
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return draft;
            },
            cancellationToken);

        return payment is null
            ? null
            : await BuildResponseAsync(payment, cancellationToken);
    }

    public async Task<PaymentAllocationResponse?> AddAllocationAsync(
        Guid paymentId,
        CreatePaymentAllocationRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var payment = await repository.GetPaymentAsync(
                    organizationId,
                    paymentId,
                    trackChanges: true,
                    ct);

                if (payment is null)
                {
                    return null;
                }

                EnsureDraft(payment);

                var targetType = ParseRequiredTargetType(request.TargetType);
                ValidateTargetForDirection(payment.Direction, targetType);

                var existing = await repository.ListAllocationsAsync(
                    organizationId,
                    paymentId,
                    trackChanges: false,
                    ct);

                var prepared = await PrepareAllocationAsync(
                    organizationId,
                    payment,
                    targetType,
                    request.TargetDocumentId,
                    request.DocumentAmount,
                    excludingAllocationId: null,
                    existing,
                    ct);

                var allocation = PaymentAllocation.Create(
                    organizationId,
                    payment.Id,
                    await repository.GetNextAllocationLineNumberAsync(
                        organizationId,
                        paymentId,
                        ct),
                    targetType,
                    prepared.SalesInvoiceId,
                    prepared.PurchaseDocumentId,
                    prepared.DocumentNumber,
                    prepared.DocumentCurrency.Id,
                    prepared.DocumentCurrency.Code,
                    prepared.DocumentExchangeRate,
                    prepared.DocumentAmount,
                    prepared.PaymentAmount,
                    timeProvider.GetUtcNow());

                await repository.AddAllocationAsync(allocation, ct);

                payment.SetAllocatedAmount(
                    existing.Sum(item => item.PaymentAmount) +
                    allocation.PaymentAmount,
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return MapAllocation(allocation);
            },
            cancellationToken);
    }

    public async Task<PaymentAllocationResponse?> UpdateAllocationAsync(
        Guid paymentId,
        Guid allocationId,
        UpdatePaymentAllocationRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var payment = await repository.GetPaymentAsync(
                    organizationId,
                    paymentId,
                    trackChanges: true,
                    ct);

                if (payment is null)
                {
                    return null;
                }

                EnsureDraft(payment);

                var allocation = await repository.GetAllocationAsync(
                    organizationId,
                    paymentId,
                    allocationId,
                    trackChanges: true,
                    ct);

                if (allocation is null)
                {
                    return null;
                }

                var targetType = ParseRequiredTargetType(request.TargetType);
                ValidateTargetForDirection(payment.Direction, targetType);

                var existing = await repository.ListAllocationsAsync(
                    organizationId,
                    paymentId,
                    trackChanges: false,
                    ct);

                var prepared = await PrepareAllocationAsync(
                    organizationId,
                    payment,
                    targetType,
                    request.TargetDocumentId,
                    request.DocumentAmount,
                    allocation.Id,
                    existing,
                    ct);

                allocation.Update(
                    targetType,
                    prepared.SalesInvoiceId,
                    prepared.PurchaseDocumentId,
                    prepared.DocumentNumber,
                    prepared.DocumentCurrency.Id,
                    prepared.DocumentCurrency.Code,
                    prepared.DocumentExchangeRate,
                    prepared.DocumentAmount,
                    prepared.PaymentAmount,
                    timeProvider.GetUtcNow());

                payment.SetAllocatedAmount(
                    existing
                        .Where(item => item.Id != allocation.Id)
                        .Sum(item => item.PaymentAmount) +
                    allocation.PaymentAmount,
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return MapAllocation(allocation);
            },
            cancellationToken);
    }

    public async Task<bool> DeleteAllocationAsync(
        Guid paymentId,
        Guid allocationId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var payment = await repository.GetPaymentAsync(
                    organizationId,
                    paymentId,
                    trackChanges: true,
                    ct);

                if (payment is null)
                {
                    return false;
                }

                EnsureDraft(payment);

                var allocation = await repository.GetAllocationAsync(
                    organizationId,
                    paymentId,
                    allocationId,
                    trackChanges: true,
                    ct);

                if (allocation is null)
                {
                    return false;
                }

                var allocations = await repository.ListAllocationsAsync(
                    organizationId,
                    paymentId,
                    trackChanges: false,
                    ct);

                repository.RemoveAllocation(allocation);

                payment.SetAllocatedAmount(
                    allocations
                        .Where(item => item.Id != allocation.Id)
                        .Sum(item => item.PaymentAmount),
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return true;
            },
            cancellationToken);
    }

    public async Task<PaymentResponse?> ConfirmAsync(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var payment = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var draft = await repository.GetPaymentAsync(
                    organizationId,
                    paymentId,
                    trackChanges: true,
                    ct);

                if (draft is null)
                {
                    return null;
                }

                EnsureDraft(draft);

                var counterparty = await RequireCounterpartyAsync(
                    organizationId,
                    draft.CounterpartyId,
                    draft.Direction,
                    ct);

                var paymentCurrency = await RequireActiveCurrencyAsync(
                    organizationId,
                    draft.CurrencyId,
                    ct);

                var baseCurrency = await currencyRepository.GetBaseCurrencyAsync(
                    organizationId,
                    trackChanges: false,
                    ct);

                if (baseCurrency is null || !baseCurrency.IsActive)
                {
                    throw new PaymentConflictException(
                        "The organization must have an active base currency before a payment can be confirmed.");
                }

                var paymentExchangeRate = await ResolveRateAsync(
                    organizationId,
                    baseCurrency,
                    paymentCurrency,
                    draft.PaymentDate,
                    ct);

                var allocations = await repository.ListAllocationsAsync(
                    organizationId,
                    paymentId,
                    trackChanges: true,
                    ct);

                foreach (var target in allocations
                             .Select(GetTargetKey)
                             .Distinct())
                {
                    if (target.TargetType == PaymentAllocationTargetType.SalesInvoice)
                    {
                        await repository.LockSalesInvoiceAsync(
                            organizationId,
                            target.TargetId,
                            ct);
                    }
                    else
                    {
                        await repository.LockPurchaseDocumentAsync(
                            organizationId,
                            target.TargetId,
                            ct);
                    }
                }

                var refreshed = new List<PreparedAllocation>(allocations.Count);

                foreach (var allocation in allocations)
                {
                    var targetId = allocation.SalesInvoiceId ??
                        allocation.PurchaseDocumentId ??
                        throw new InvalidOperationException(
                            "Payment allocation target is missing.");

                    refreshed.Add(await PrepareAllocationForConfirmationAsync(
                        organizationId,
                        draft,
                        allocation.TargetType,
                        targetId,
                        allocation.DocumentAmount,
                        baseCurrency,
                        paymentCurrency,
                        paymentExchangeRate,
                        ct));
                }

                foreach (var group in refreshed.GroupBy(item =>
                             (item.TargetType, item.TargetDocumentId)))
                {
                    var first = group.First();
                    var confirmedAllocated = first.TargetType ==
                        PaymentAllocationTargetType.SalesInvoice
                        ? await repository.GetConfirmedSalesInvoiceAllocatedAmountAsync(
                            organizationId,
                            first.TargetDocumentId,
                            draft.Id,
                            ct)
                        : await repository.GetConfirmedPurchaseDocumentAllocatedAmountAsync(
                            organizationId,
                            first.TargetDocumentId,
                            draft.Id,
                            ct);

                    if (confirmedAllocated +
                        group.Sum(item => item.DocumentAmount) >
                        first.DocumentTotal)
                    {
                        throw new PaymentConflictException(
                            $"Allocations exceed the outstanding amount of document '{first.DocumentNumber}'.");
                    }
                }

                var totalPaymentAmount = refreshed.Sum(item => item.PaymentAmount);

                if (totalPaymentAmount > draft.Amount)
                {
                    throw new PaymentConflictException(
                        "Total allocations exceed the payment amount.");
                }

                var now = timeProvider.GetUtcNow();

                for (var index = 0; index < allocations.Count; index++)
                {
                    var allocation = allocations[index];
                    var item = refreshed[index];

                    allocation.Update(
                        item.TargetType,
                        item.SalesInvoiceId,
                        item.PurchaseDocumentId,
                        item.DocumentNumber,
                        item.DocumentCurrency.Id,
                        item.DocumentCurrency.Code,
                        item.DocumentExchangeRate,
                        item.DocumentAmount,
                        item.PaymentAmount,
                        now);
                }

                draft.SetAllocatedAmount(totalPaymentAmount, now);

                // Persist refreshed allocation snapshots while the parent payment
                // is still Draft. This keeps database immutability triggers
                // independent from EF update ordering.
                await repository.SaveChangesAsync(ct);

                var sequence = await repository.AllocatePaymentSequenceAsync(
                    organizationId,
                    draft.PaymentDate.Year,
                    draft.Direction,
                    ct);

                draft.Confirm(
                    FormatInternalNumber(
                        draft.Direction,
                        draft.PaymentDate.Year,
                        sequence),
                    baseCurrency.Id,
                    baseCurrency.Code,
                    paymentCurrency.Code,
                    counterparty.Name,
                    counterparty.RegistrationNumber,
                    counterparty.VatNumber,
                    paymentExchangeRate,
                    now);

                try
                {
                    await autoPostingEngine.PostPaymentAsync(
                        draft,
                        allocations,
                        ct);
                }
                catch (AutoPostingException exception)
                {
                    throw new PaymentConflictException(
                        $"Accounting auto-posting failed: {exception.Message}");
                }

                await repository.SaveChangesAsync(ct);
                return draft;
            },
            cancellationToken);

        return payment is null
            ? null
            : await BuildResponseAsync(payment, cancellationToken);
    }

    public async Task<PaymentResponse?> CancelAsync(
        Guid paymentId,
        CancelPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var payment = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var existing = await repository.GetPaymentAsync(
                    organizationId,
                    paymentId,
                    trackChanges: true,
                    ct);

                if (existing is null)
                {
                    return null;
                }

                if (existing.Status != PaymentStatus.Confirmed)
                {
                    throw new PaymentConflictException(
                        "Only a confirmed payment can be cancelled.");
                }

                existing.Cancel(
                    request.Reason,
                    timeProvider.GetUtcNow());

                try
                {
                    await autoPostingEngine.ReversePaymentAsync(
                        existing,
                        request.Reason,
                        ct);
                }
                catch (AutoPostingException exception)
                {
                    throw new PaymentConflictException(
                        $"Accounting reversal failed: {exception.Message}");
                }

                await repository.SaveChangesAsync(ct);
                return existing;
            },
            cancellationToken);

        return payment is null
            ? null
            : await BuildResponseAsync(payment, cancellationToken);
    }

    public async Task<DocumentSettlementResponse?> GetSalesInvoiceSettlementAsync(
        Guid salesInvoiceId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var invoice = await salesInvoiceRepository.GetInvoiceAsync(
            organizationId,
            salesInvoiceId,
            trackChanges: false,
            cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        var allocated = await repository.GetConfirmedSalesInvoiceAllocatedAmountAsync(
            organizationId,
            salesInvoiceId,
            excludingPaymentId: null,
            cancellationToken);

        return new DocumentSettlementResponse(
            PaymentAllocationTargetType.SalesInvoice.ToString(),
            invoice.Id,
            invoice.Number ?? invoice.Id.ToString(),
            invoice.Status.ToString(),
            invoice.CounterpartyId,
            invoice.CurrencyId,
            invoice.CurrencyCode,
            invoice.GrossTotal,
            allocated,
            Math.Max(0m, invoice.GrossTotal - allocated),
            GetSettlementStatus(invoice.GrossTotal, allocated));
    }

    public async Task<DocumentSettlementResponse?> GetPurchaseDocumentSettlementAsync(
        Guid purchaseDocumentId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var document = await purchaseDocumentRepository.GetAsync(
            organizationId,
            purchaseDocumentId,
            trackChanges: false,
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        var allocated = await repository.GetConfirmedPurchaseDocumentAllocatedAmountAsync(
            organizationId,
            purchaseDocumentId,
            excludingPaymentId: null,
            cancellationToken);

        return new DocumentSettlementResponse(
            PaymentAllocationTargetType.PurchaseDocument.ToString(),
            document.Id,
            document.InternalNumber ??
                document.SupplierDocumentNumber ??
                document.Id.ToString(),
            document.Status.ToString(),
            document.CounterpartyId,
            document.CurrencyId,
            document.CurrencyCode,
            document.GrossTotal,
            allocated,
            Math.Max(0m, document.GrossTotal - allocated),
            GetSettlementStatus(document.GrossTotal, allocated));
    }

    private async Task<PreparedAllocation> PrepareAllocationAsync(
        Guid organizationId,
        Payment payment,
        PaymentAllocationTargetType targetType,
        Guid targetDocumentId,
        decimal documentAmount,
        Guid? excludingAllocationId,
        IReadOnlyList<PaymentAllocation> existingAllocations,
        CancellationToken cancellationToken)
    {
        var paymentCurrency = await RequireActiveCurrencyAsync(
            organizationId,
            payment.CurrencyId,
            cancellationToken);

        var baseCurrency = await currencyRepository.GetBaseCurrencyAsync(
            organizationId,
            trackChanges: false,
            cancellationToken);

        if (baseCurrency is null || !baseCurrency.IsActive)
        {
            throw new PaymentConflictException(
                "The organization must have an active base currency before allocations can be calculated.");
        }

        var paymentExchangeRate = await ResolveRateAsync(
            organizationId,
            baseCurrency,
            paymentCurrency,
            payment.PaymentDate,
            cancellationToken);

        var prepared = await PrepareAllocationForConfirmationAsync(
            organizationId,
            payment,
            targetType,
            targetDocumentId,
            documentAmount,
            baseCurrency,
            paymentCurrency,
            paymentExchangeRate,
            cancellationToken);

        if (existingAllocations.Any(item =>
                item.Id != excludingAllocationId &&
                GetTargetKey(item) ==
                (targetType, targetDocumentId)))
        {
            throw new PaymentConflictException(
                "This payment already contains an allocation for the selected document. Update the existing allocation instead.");
        }

        var confirmedAllocated = targetType ==
            PaymentAllocationTargetType.SalesInvoice
            ? await repository.GetConfirmedSalesInvoiceAllocatedAmountAsync(
                organizationId,
                targetDocumentId,
                payment.Id,
                cancellationToken)
            : await repository.GetConfirmedPurchaseDocumentAllocatedAmountAsync(
                organizationId,
                targetDocumentId,
                payment.Id,
                cancellationToken);

        var currentDraftTargetAmount = existingAllocations
            .Where(item =>
                item.Id != excludingAllocationId &&
                GetTargetKey(item) ==
                (targetType, targetDocumentId))
            .Sum(item => item.DocumentAmount);

        if (confirmedAllocated +
            currentDraftTargetAmount +
            prepared.DocumentAmount >
            prepared.DocumentTotal)
        {
            throw new PaymentConflictException(
                $"Allocation exceeds the outstanding amount of document '{prepared.DocumentNumber}'.");
        }

        var otherPaymentAmount = existingAllocations
            .Where(item => item.Id != excludingAllocationId)
            .Sum(item => item.PaymentAmount);

        if (otherPaymentAmount + prepared.PaymentAmount > payment.Amount)
        {
            throw new PaymentConflictException(
                "Total allocations exceed the payment amount.");
        }

        return prepared;
    }

    private async Task<PreparedAllocation> PrepareAllocationForConfirmationAsync(
        Guid organizationId,
        Payment payment,
        PaymentAllocationTargetType targetType,
        Guid targetDocumentId,
        decimal documentAmount,
        Currency baseCurrency,
        Currency paymentCurrency,
        decimal paymentExchangeRate,
        CancellationToken cancellationToken)
    {
        ValidateTargetForDirection(payment.Direction, targetType);

        if (targetDocumentId == Guid.Empty)
        {
            throw new PaymentConflictException(
                "Target document id is required.");
        }

        if (targetType == PaymentAllocationTargetType.SalesInvoice)
        {
            var invoice = await salesInvoiceRepository.GetInvoiceAsync(
                organizationId,
                targetDocumentId,
                trackChanges: false,
                cancellationToken);

            if (invoice is null ||
                invoice.Status != SalesInvoiceStatus.Issued)
            {
                throw new PaymentConflictException(
                    "Incoming payments can only be allocated to issued sales invoices.");
            }

            if (invoice.CounterpartyId != payment.CounterpartyId)
            {
                throw new PaymentConflictException(
                    "Payment and sales invoice must belong to the same counterparty.");
            }

            var documentCurrency = await RequireCurrencyAsync(
                organizationId,
                invoice.CurrencyId,
                cancellationToken);

            ValidateDocumentAmountPrecision(
                documentAmount,
                documentCurrency);

            var documentExchangeRate = await ResolveRateAsync(
                organizationId,
                baseCurrency,
                documentCurrency,
                payment.PaymentDate,
                cancellationToken);

            var paymentAmount = ConvertDocumentToPaymentAmount(
                documentAmount,
                documentExchangeRate,
                paymentExchangeRate,
                paymentCurrency.DecimalPlaces);

            return new PreparedAllocation(
                targetType,
                invoice.Id,
                invoice.Id,
                null,
                invoice.Number ??
                    throw new InvalidOperationException(
                        "Issued sales invoice is missing a number."),
                invoice.GrossTotal,
                documentCurrency,
                documentExchangeRate,
                documentAmount,
                paymentAmount);
        }

        var document = await purchaseDocumentRepository.GetAsync(
            organizationId,
            targetDocumentId,
            trackChanges: false,
            cancellationToken);

        if (document is null ||
            document.Status != PurchaseDocumentStatus.Received)
        {
            throw new PaymentConflictException(
                "Outgoing payments can only be allocated to received purchase documents.");
        }

        if (document.CounterpartyId != payment.CounterpartyId)
        {
            throw new PaymentConflictException(
                "Payment and purchase document must belong to the same counterparty.");
        }

        var purchaseCurrency = await RequireCurrencyAsync(
            organizationId,
            document.CurrencyId,
            cancellationToken);

        ValidateDocumentAmountPrecision(
            documentAmount,
            purchaseCurrency);

        var purchaseExchangeRate = await ResolveRateAsync(
            organizationId,
            baseCurrency,
            purchaseCurrency,
            payment.PaymentDate,
            cancellationToken);

        var outgoingPaymentAmount = ConvertDocumentToPaymentAmount(
            documentAmount,
            purchaseExchangeRate,
            paymentExchangeRate,
            paymentCurrency.DecimalPlaces);

        return new PreparedAllocation(
            targetType,
            document.Id,
            null,
            document.Id,
            document.InternalNumber ??
                document.SupplierDocumentNumber ??
                throw new InvalidOperationException(
                    "Received purchase document is missing a document number."),
            document.GrossTotal,
            purchaseCurrency,
            purchaseExchangeRate,
            documentAmount,
            outgoingPaymentAmount);
    }

    private async Task<Counterparty> RequireCounterpartyAsync(
        Guid organizationId,
        Guid counterpartyId,
        PaymentDirection direction,
        CancellationToken cancellationToken)
    {
        var counterparty = await counterpartyRepository.GetAsync(
            organizationId,
            counterpartyId,
            trackChanges: false,
            cancellationToken);

        var valid = counterparty is not null &&
            counterparty.IsActive &&
            (direction == PaymentDirection.Incoming
                ? counterparty.IsCustomer
                : counterparty.IsSupplier);

        if (!valid || counterparty is null)
        {
            throw new PaymentConflictException(
                direction == PaymentDirection.Incoming
                    ? "Incoming payments require an active customer."
                    : "Outgoing payments require an active supplier.");
        }

        return counterparty;
    }

    private async Task<Currency> RequireActiveCurrencyAsync(
        Guid organizationId,
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var currency = await RequireCurrencyAsync(
            organizationId,
            currencyId,
            cancellationToken);

        if (!currency.IsActive)
        {
            throw new PaymentConflictException(
                "Payment currency must be active.");
        }

        return currency;
    }

    private async Task<Currency> RequireCurrencyAsync(
        Guid organizationId,
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var currency = await currencyRepository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: false,
            cancellationToken);

        return currency ??
            throw new PaymentConflictException(
                "Currency does not exist in this organization.");
    }

    private async Task<decimal> ResolveRateAsync(
        Guid organizationId,
        Currency baseCurrency,
        Currency currency,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        if (baseCurrency.Id == currency.Id)
        {
            return 1m;
        }

        var direct = await currencyRepository.GetLatestExchangeRateAsync(
            organizationId,
            baseCurrency.Id,
            currency.Id,
            date,
            cancellationToken);

        if (direct is not null)
        {
            return direct.Rate;
        }

        var inverse = await currencyRepository.GetLatestExchangeRateAsync(
            organizationId,
            currency.Id,
            baseCurrency.Id,
            date,
            cancellationToken);

        if (inverse is not null)
        {
            return decimal.Round(
                1m / inverse.Rate,
                10,
                MidpointRounding.AwayFromZero);
        }

        throw new PaymentConflictException(
            $"No exchange rate is available between {baseCurrency.Code} and {currency.Code} as of {date:yyyy-MM-dd}.");
    }

    private async Task<PaymentResponse> BuildResponseAsync(
        Payment payment,
        CancellationToken cancellationToken)
    {
        var allocations = await repository.ListAllocationsAsync(
            payment.OrganizationId,
            payment.Id,
            trackChanges: false,
            cancellationToken);

        return new PaymentResponse(
            payment.Id,
            payment.OrganizationId,
            payment.InternalNumber,
            payment.Direction.ToString(),
            payment.Status.ToString(),
            payment.PaymentDate,
            payment.Method.ToString(),
            payment.CounterpartyId,
            payment.CounterpartyName,
            payment.CounterpartyRegistrationNumber,
            payment.CounterpartyVatNumber,
            payment.CurrencyId,
            payment.CurrencyCode,
            payment.BaseCurrencyId,
            payment.BaseCurrencyCode,
            payment.ExchangeRate,
            payment.Amount,
            payment.AllocatedAmount,
            payment.UnallocatedAmount,
            payment.Reference,
            payment.Notes,
            payment.ConfirmedAtUtc,
            payment.CancelledAtUtc,
            payment.CancellationReason,
            allocations
                .OrderBy(item => item.LineNumber)
                .Select(MapAllocation)
                .ToArray(),
            payment.CreatedAtUtc,
            payment.CreatedByUserId,
            payment.UpdatedAtUtc,
            payment.UpdatedByUserId);
    }

    private static decimal ConvertDocumentToPaymentAmount(
        decimal documentAmount,
        decimal documentExchangeRate,
        decimal paymentExchangeRate,
        int paymentCurrencyDecimalPlaces)
    {
        var paymentAmount = decimal.Round(
            documentAmount *
            paymentExchangeRate /
            documentExchangeRate,
            paymentCurrencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        if (paymentAmount <= 0m)
        {
            throw new PaymentConflictException(
                "Allocation converts to a zero payment amount.");
        }

        return paymentAmount;
    }

    private static void ValidateAmountPrecision(
        decimal amount,
        Currency currency)
    {
        if (amount <= 0m ||
            decimal.Round(amount, currency.DecimalPlaces) != amount)
        {
            throw new PaymentConflictException(
                $"Payment amount must be positive and use at most {currency.DecimalPlaces} decimal places for {currency.Code}.");
        }
    }

    private static void ValidateDocumentAmountPrecision(
        decimal amount,
        Currency currency)
    {
        if (amount <= 0m ||
            decimal.Round(amount, currency.DecimalPlaces) != amount)
        {
            throw new PaymentConflictException(
                $"Document allocation amount must be positive and use at most {currency.DecimalPlaces} decimal places for {currency.Code}.");
        }
    }

    private static void ValidateTargetForDirection(
        PaymentDirection direction,
        PaymentAllocationTargetType targetType)
    {
        var valid =
            direction == PaymentDirection.Incoming &&
            targetType == PaymentAllocationTargetType.SalesInvoice ||
            direction == PaymentDirection.Outgoing &&
            targetType == PaymentAllocationTargetType.PurchaseDocument;

        if (!valid)
        {
            throw new PaymentConflictException(
                direction == PaymentDirection.Incoming
                    ? "Incoming payments can only allocate to sales invoices."
                    : "Outgoing payments can only allocate to purchase documents.");
        }
    }

    private static PaymentDirection ParseRequiredDirection(string value) =>
        ParseDirection(value) ??
        throw new PaymentQueryException(
            "Payment direction is required.");

    private static PaymentDirection? ParseDirection(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Enum.TryParse<PaymentDirection>(
                value,
                ignoreCase: true,
                out var direction) ||
            !Enum.IsDefined(direction))
        {
            throw new PaymentQueryException(
                $"Unknown payment direction '{value}'.");
        }

        return direction;
    }

    private static PaymentMethod ParseRequiredMethod(string value)
    {
        if (!Enum.TryParse<PaymentMethod>(
                value,
                ignoreCase: true,
                out var method) ||
            !Enum.IsDefined(method))
        {
            throw new PaymentQueryException(
                $"Unknown payment method '{value}'.");
        }

        return method;
    }

    private static PaymentStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Enum.TryParse<PaymentStatus>(
                value,
                ignoreCase: true,
                out var status) ||
            !Enum.IsDefined(status))
        {
            throw new PaymentQueryException(
                $"Unknown payment status '{value}'.");
        }

        return status;
    }

    private static PaymentAllocationTargetType ParseRequiredTargetType(
        string value)
    {
        if (!Enum.TryParse<PaymentAllocationTargetType>(
                value,
                ignoreCase: true,
                out var targetType) ||
            !Enum.IsDefined(targetType))
        {
            throw new PaymentQueryException(
                $"Unknown payment allocation target type '{value}'.");
        }

        return targetType;
    }

    private static void ValidateDateRange(
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value > toDate.Value)
        {
            throw new PaymentQueryException(
                "fromDate cannot be after toDate.");
        }
    }

    private static void EnsureDraft(Payment payment)
    {
        if (payment.Status != PaymentStatus.Draft)
        {
            throw new PaymentConflictException(
                "Only draft payments can be edited.");
        }
    }

    private static string FormatInternalNumber(
        PaymentDirection direction,
        int calendarYear,
        long sequence)
    {
        if (calendarYear < 1 || sequence <= 0)
        {
            throw new InvalidOperationException(
                "Payment sequence is invalid.");
        }

        var prefix = direction == PaymentDirection.Incoming
            ? "RCV"
            : "PAY";

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{prefix}-{calendarYear:D4}-{sequence:D6}");
    }

    private static string GetSettlementStatus(
        decimal total,
        decimal allocated)
    {
        if (allocated <= 0m)
        {
            return "Unpaid";
        }

        return allocated >= total
            ? "Paid"
            : "PartiallyPaid";
    }

    private static (PaymentAllocationTargetType TargetType, Guid TargetId)
        GetTargetKey(PaymentAllocation allocation) =>
        allocation.TargetType switch
        {
            PaymentAllocationTargetType.SalesInvoice =>
                (
                    allocation.TargetType,
                    allocation.SalesInvoiceId ??
                    throw new InvalidOperationException(
                        "Sales invoice allocation is missing target id.")
                ),
            PaymentAllocationTargetType.PurchaseDocument =>
                (
                    allocation.TargetType,
                    allocation.PurchaseDocumentId ??
                    throw new InvalidOperationException(
                        "Purchase document allocation is missing target id.")
                ),
            _ => throw new InvalidOperationException(
                "Unknown payment allocation target type.")
        };

    private static PaymentSummaryResponse MapSummary(Payment payment) =>
        new(
            payment.Id,
            payment.InternalNumber,
            payment.Direction.ToString(),
            payment.Status.ToString(),
            payment.PaymentDate,
            payment.Method.ToString(),
            payment.CounterpartyId,
            payment.CounterpartyName,
            payment.CurrencyId,
            payment.CurrencyCode,
            payment.Amount,
            payment.AllocatedAmount,
            payment.UnallocatedAmount,
            payment.Reference,
            payment.ConfirmedAtUtc,
            payment.CancelledAtUtc);

    private static PaymentAllocationResponse MapAllocation(
        PaymentAllocation allocation) =>
        new(
            allocation.Id,
            allocation.LineNumber,
            allocation.TargetType.ToString(),
            allocation.SalesInvoiceId ??
                allocation.PurchaseDocumentId ??
                Guid.Empty,
            allocation.DocumentNumber,
            allocation.DocumentCurrencyId,
            allocation.DocumentCurrencyCode,
            allocation.DocumentExchangeRate,
            allocation.DocumentAmount,
            allocation.PaymentAmount);

    private sealed record PreparedAllocation(
        PaymentAllocationTargetType TargetType,
        Guid TargetDocumentId,
        Guid? SalesInvoiceId,
        Guid? PurchaseDocumentId,
        string DocumentNumber,
        decimal DocumentTotal,
        Currency DocumentCurrency,
        decimal DocumentExchangeRate,
        decimal DocumentAmount,
        decimal PaymentAmount);
}
