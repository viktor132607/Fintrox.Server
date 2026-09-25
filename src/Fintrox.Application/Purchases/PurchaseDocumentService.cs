using System.Globalization;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Counterparties;
using Fintrox.Application.Currencies;
using Fintrox.Application.Tax;
using Fintrox.Contracts.Purchases;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Tax;

namespace Fintrox.Application.Purchases;

public sealed class PurchaseDocumentService(
    IPurchaseDocumentRepository repository,
    ICounterpartyRepository counterpartyRepository,
    ICurrencyRepository currencyRepository,
    IVatCodeRepository vatCodeRepository,
    ITransactionRunner transactionRunner,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IPurchaseDocumentService
{
    public async Task<IReadOnlyList<PurchaseDocumentSummaryResponse>> ListAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        string? type,
        Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDate, toDate);
        var organizationId = currentOrganization.RequireOrganizationId();

        var documents = await repository.ListAsync(
            organizationId,
            fromDate,
            toDate,
            ParseStatus(status),
            ParseType(type),
            counterpartyId,
            cancellationToken);

        return documents.Select(MapSummary).ToArray();
    }

    public async Task<PurchaseDocumentResponse?> GetAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var document = await repository.GetAsync(
            organizationId,
            documentId,
            trackChanges: false,
            cancellationToken);

        return document is null
            ? null
            : await BuildResponseAsync(document, cancellationToken);
    }

    public async Task<PurchaseDocumentResponse> CreateAsync(
        CreatePurchaseDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var document = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var type = ParseRequiredType(request.Type);
                var supplier = await RequireSupplierAsync(
                    organizationId,
                    request.CounterpartyId,
                    ct);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    request.CurrencyId,
                    ct);

                var supplierNumber = NormalizeSupplierNumber(
                    request.SupplierDocumentNumber);

                await EnsureSupplierDocumentNumberUniqueAsync(
                    organizationId,
                    supplier.Id,
                    supplierNumber,
                    excludingDocumentId: null,
                    ct);

                var dueDate = ResolveDueDate(
                    request.DocumentDate,
                    request.DueDate,
                    supplier.PaymentTermDays);

                var now = timeProvider.GetUtcNow();
                var draft = PurchaseDocument.CreateDraft(
                    organizationId,
                    type,
                    supplier.Id,
                    supplierNumber,
                    request.DocumentDate,
                    dueDate,
                    currency.Id,
                    supplier.Name,
                    supplier.LegalName,
                    supplier.RegistrationNumber,
                    supplier.VatNumber,
                    supplier.CountryCode,
                    supplier.AddressLine1,
                    supplier.AddressLine2,
                    supplier.City,
                    supplier.PostalCode,
                    currency.Code,
                    request.Notes,
                    now);

                await repository.AddAsync(draft, ct);

                var lines = new List<PurchaseDocumentLine>();
                var lineNumber = 1;

                foreach (var requestLine in request.Lines ?? [])
                {
                    var line = await CreateLineAsync(
                        organizationId,
                        draft.Id,
                        lineNumber++,
                        request.DocumentDate,
                        currency,
                        requestLine.ItemCode,
                        requestLine.Description,
                        requestLine.Quantity,
                        requestLine.UnitOfMeasure,
                        requestLine.UnitPrice,
                        requestLine.DiscountPercent,
                        requestLine.VatCodeId,
                        requestLine.RecoverableVatPercent,
                        now,
                        ct);

                    lines.Add(line);
                    await repository.AddLineAsync(line, ct);
                }

                SetTotals(draft, lines, now);
                await repository.SaveChangesAsync(ct);

                return draft;
            },
            cancellationToken);

        return await BuildResponseAsync(document, cancellationToken);
    }

    public async Task<PurchaseDocumentResponse?> UpdateAsync(
        Guid documentId,
        UpdatePurchaseDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var document = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var draft = await repository.GetAsync(
                    organizationId,
                    documentId,
                    trackChanges: true,
                    ct);

                if (draft is null)
                {
                    return null;
                }

                EnsureDraft(draft);

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    documentId,
                    trackChanges: false,
                    ct);

                if (draft.CurrencyId != request.CurrencyId &&
                    lines.Count > 0)
                {
                    throw new PurchaseDocumentConflictException(
                        "Currency cannot be changed after purchase lines have been added.");
                }

                var type = ParseRequiredType(request.Type);
                var supplier = await RequireSupplierAsync(
                    organizationId,
                    request.CounterpartyId,
                    ct);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    request.CurrencyId,
                    ct);

                var supplierNumber = NormalizeSupplierNumber(
                    request.SupplierDocumentNumber);

                await EnsureSupplierDocumentNumberUniqueAsync(
                    organizationId,
                    supplier.Id,
                    supplierNumber,
                    documentId,
                    ct);

                if (draft.DocumentDate != request.DocumentDate)
                {
                    foreach (var line in lines)
                    {
                        await RequirePurchaseVatCodeAsync(
                            organizationId,
                            line.VatCodeId,
                            request.DocumentDate,
                            ct);
                    }
                }

                var dueDate = ResolveDueDate(
                    request.DocumentDate,
                    request.DueDate,
                    supplier.PaymentTermDays);

                draft.UpdateDraft(
                    type,
                    supplier.Id,
                    supplierNumber,
                    request.DocumentDate,
                    dueDate,
                    currency.Id,
                    supplier.Name,
                    supplier.LegalName,
                    supplier.RegistrationNumber,
                    supplier.VatNumber,
                    supplier.CountryCode,
                    supplier.AddressLine1,
                    supplier.AddressLine2,
                    supplier.City,
                    supplier.PostalCode,
                    currency.Code,
                    request.Notes,
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return draft;
            },
            cancellationToken);

        return document is null
            ? null
            : await BuildResponseAsync(document, cancellationToken);
    }

    public async Task<PurchaseDocumentLineResponse?> AddLineAsync(
        Guid documentId,
        CreatePurchaseDocumentLineRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var document = await repository.GetAsync(
                    organizationId,
                    documentId,
                    trackChanges: true,
                    ct);

                if (document is null)
                {
                    return null;
                }

                EnsureDraft(document);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    document.CurrencyId,
                    ct);

                var existingLines = await repository.ListLinesAsync(
                    organizationId,
                    documentId,
                    trackChanges: false,
                    ct);

                var line = await CreateLineAsync(
                    organizationId,
                    document.Id,
                    await repository.GetNextLineNumberAsync(
                        organizationId,
                        documentId,
                        ct),
                    document.DocumentDate,
                    currency,
                    request.ItemCode,
                    request.Description,
                    request.Quantity,
                    request.UnitOfMeasure,
                    request.UnitPrice,
                    request.DiscountPercent,
                    request.VatCodeId,
                    request.RecoverableVatPercent,
                    timeProvider.GetUtcNow(),
                    ct);

                await repository.AddLineAsync(line, ct);
                SetTotals(
                    document,
                    [.. existingLines, line],
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return MapLine(line);
            },
            cancellationToken);
    }

    public async Task<PurchaseDocumentLineResponse?> UpdateLineAsync(
        Guid documentId,
        Guid lineId,
        UpdatePurchaseDocumentLineRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var document = await repository.GetAsync(
                    organizationId,
                    documentId,
                    trackChanges: true,
                    ct);

                if (document is null)
                {
                    return null;
                }

                EnsureDraft(document);

                var line = await repository.GetLineAsync(
                    organizationId,
                    documentId,
                    lineId,
                    trackChanges: true,
                    ct);

                if (line is null)
                {
                    return null;
                }

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    document.CurrencyId,
                    ct);

                var vatCode = await RequirePurchaseVatCodeAsync(
                    organizationId,
                    request.VatCodeId,
                    document.DocumentDate,
                    ct);

                var amounts = CalculateAmounts(
                    request.Quantity,
                    request.UnitPrice,
                    request.DiscountPercent,
                    vatCode.RatePercent,
                    request.RecoverableVatPercent,
                    currency.DecimalPlaces);

                var now = timeProvider.GetUtcNow();

                line.Update(
                    request.ItemCode,
                    request.Description,
                    request.Quantity,
                    request.UnitOfMeasure,
                    request.UnitPrice,
                    request.DiscountPercent,
                    vatCode.Id,
                    vatCode.Code,
                    vatCode.RatePercent,
                    request.RecoverableVatPercent,
                    amounts.Net,
                    amounts.Vat,
                    amounts.RecoverableVat,
                    amounts.NonRecoverableVat,
                    amounts.Gross,
                    now);

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    documentId,
                    trackChanges: false,
                    ct);

                SetTotals(
                    document,
                    lines.Select(existing =>
                            existing.Id == line.Id ? line : existing)
                        .ToArray(),
                    now);

                await repository.SaveChangesAsync(ct);
                return MapLine(line);
            },
            cancellationToken);
    }

    public async Task<bool> DeleteLineAsync(
        Guid documentId,
        Guid lineId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var document = await repository.GetAsync(
                    organizationId,
                    documentId,
                    trackChanges: true,
                    ct);

                if (document is null)
                {
                    return false;
                }

                EnsureDraft(document);

                var line = await repository.GetLineAsync(
                    organizationId,
                    documentId,
                    lineId,
                    trackChanges: true,
                    ct);

                if (line is null)
                {
                    return false;
                }

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    documentId,
                    trackChanges: false,
                    ct);

                repository.RemoveLine(line);

                SetTotals(
                    document,
                    lines.Where(existing => existing.Id != line.Id).ToArray(),
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return true;
            },
            cancellationToken);
    }

    public async Task<PurchaseDocumentResponse?> ReceiveAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var document = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var draft = await repository.GetAsync(
                    organizationId,
                    documentId,
                    trackChanges: true,
                    ct);

                if (draft is null)
                {
                    return null;
                }

                EnsureDraft(draft);

                var supplier = await RequireSupplierAsync(
                    organizationId,
                    draft.CounterpartyId,
                    ct);

                var supplierNumber = NormalizeSupplierNumber(
                    draft.SupplierDocumentNumber);

                if (draft.Type == PurchaseDocumentType.Invoice &&
                    string.IsNullOrWhiteSpace(supplierNumber))
                {
                    throw new PurchaseDocumentConflictException(
                        "Supplier document number is required before receiving a purchase invoice.");
                }

                await EnsureSupplierDocumentNumberUniqueAsync(
                    organizationId,
                    supplier.Id,
                    supplierNumber,
                    draft.Id,
                    ct);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    draft.CurrencyId,
                    ct);

                var baseCurrency = await currencyRepository.GetBaseCurrencyAsync(
                    organizationId,
                    trackChanges: false,
                    ct);

                if (baseCurrency is null || !baseCurrency.IsActive)
                {
                    throw new PurchaseDocumentConflictException(
                        "The organization must have an active base currency before a purchase document can be received.");
                }

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    draft.Id,
                    trackChanges: true,
                    ct);

                if (lines.Count == 0)
                {
                    throw new PurchaseDocumentConflictException(
                        "A purchase document must contain at least one line before it can be received.");
                }

                foreach (var line in lines)
                {
                    var vatCode = await RequirePurchaseVatCodeAsync(
                        organizationId,
                        line.VatCodeId,
                        draft.DocumentDate,
                        ct);

                    if (!string.Equals(
                            line.VatCode,
                            vatCode.Code,
                            StringComparison.Ordinal) ||
                        line.VatRatePercent != vatCode.RatePercent)
                    {
                        throw new PurchaseDocumentConflictException(
                            $"VAT configuration for line {line.LineNumber} changed after the line was created. Update the line before receiving.");
                    }
                }

                SetTotals(draft, lines, timeProvider.GetUtcNow());

                if (draft.GrossTotal <= 0m)
                {
                    throw new PurchaseDocumentConflictException(
                        "A purchase document must have a positive gross total before it can be received.");
                }

                var exchangeRate = await ResolveExchangeRateAsync(
                    organizationId,
                    baseCurrency,
                    currency,
                    draft.DocumentDate,
                    ct);

                var sequence = await repository.AllocateInternalSequenceAsync(
                    organizationId,
                    draft.DocumentDate.Year,
                    ct);

                draft.Receive(
                    FormatInternalNumber(
                        draft.DocumentDate.Year,
                        sequence),
                    supplierNumber,
                    baseCurrency.Id,
                    baseCurrency.Code,
                    currency.Code,
                    supplier.Name,
                    supplier.LegalName,
                    supplier.RegistrationNumber,
                    supplier.VatNumber,
                    supplier.CountryCode,
                    supplier.AddressLine1,
                    supplier.AddressLine2,
                    supplier.City,
                    supplier.PostalCode,
                    exchangeRate,
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return draft;
            },
            cancellationToken);

        return document is null
            ? null
            : await BuildResponseAsync(document, cancellationToken);
    }

    public async Task<PurchaseDocumentResponse?> CancelAsync(
        Guid documentId,
        CancelPurchaseDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var document = await transactionRunner.ExecuteAsync(
            async ct =>
            {
                var existing = await repository.GetAsync(
                    organizationId,
                    documentId,
                    trackChanges: true,
                    ct);

                if (existing is null)
                {
                    return null;
                }

                if (existing.Status != PurchaseDocumentStatus.Received)
                {
                    throw new PurchaseDocumentConflictException(
                        "Only a received purchase document can be cancelled.");
                }

                existing.Cancel(
                    request.Reason,
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(ct);
                return existing;
            },
            cancellationToken);

        return document is null
            ? null
            : await BuildResponseAsync(document, cancellationToken);
    }

    private async Task<PurchaseDocumentLine> CreateLineAsync(
        Guid organizationId,
        Guid documentId,
        int lineNumber,
        DateOnly documentDate,
        Currency currency,
        string? itemCode,
        string description,
        decimal quantity,
        string? unitOfMeasure,
        decimal unitPrice,
        decimal discountPercent,
        Guid vatCodeId,
        decimal recoverableVatPercent,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var vatCode = await RequirePurchaseVatCodeAsync(
            organizationId,
            vatCodeId,
            documentDate,
            cancellationToken);

        var amounts = CalculateAmounts(
            quantity,
            unitPrice,
            discountPercent,
            vatCode.RatePercent,
            recoverableVatPercent,
            currency.DecimalPlaces);

        return PurchaseDocumentLine.Create(
            organizationId,
            documentId,
            lineNumber,
            itemCode,
            description,
            quantity,
            unitOfMeasure,
            unitPrice,
            discountPercent,
            vatCode.Id,
            vatCode.Code,
            vatCode.RatePercent,
            recoverableVatPercent,
            amounts.Net,
            amounts.Vat,
            amounts.RecoverableVat,
            amounts.NonRecoverableVat,
            amounts.Gross,
            now);
    }

    private async Task<Counterparty> RequireSupplierAsync(
        Guid organizationId,
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        var supplier = await counterpartyRepository.GetAsync(
            organizationId,
            counterpartyId,
            trackChanges: false,
            cancellationToken);

        if (supplier is null ||
            !supplier.IsActive ||
            !supplier.IsSupplier)
        {
            throw new PurchaseDocumentConflictException(
                "The selected counterparty must be an active supplier in this organization.");
        }

        return supplier;
    }

    private async Task<Currency> RequireActiveCurrencyAsync(
        Guid organizationId,
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        var currency = await currencyRepository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: false,
            cancellationToken);

        if (currency is null || !currency.IsActive)
        {
            throw new PurchaseDocumentConflictException(
                "The selected purchase currency must exist and be active.");
        }

        return currency;
    }

    private async Task<VatCode> RequirePurchaseVatCodeAsync(
        Guid organizationId,
        Guid vatCodeId,
        DateOnly documentDate,
        CancellationToken cancellationToken)
    {
        var vatCode = await vatCodeRepository.GetAsync(
            organizationId,
            vatCodeId,
            trackChanges: false,
            cancellationToken);

        if (vatCode is null ||
            !vatCode.IsActive ||
            !vatCode.AppliesToPurchases ||
            !vatCode.IsValidOn(documentDate))
        {
            throw new PurchaseDocumentConflictException(
                "The selected VAT code must be active, valid on the document date, and applicable to purchases.");
        }

        return vatCode;
    }

    private async Task EnsureSupplierDocumentNumberUniqueAsync(
        Guid organizationId,
        Guid counterpartyId,
        string? supplierDocumentNumber,
        Guid? excludingDocumentId,
        CancellationToken cancellationToken)
    {
        if (supplierDocumentNumber is null)
        {
            return;
        }

        if (await repository.SupplierDocumentNumberExistsAsync(
                organizationId,
                counterpartyId,
                supplierDocumentNumber,
                excludingDocumentId,
                cancellationToken))
        {
            throw new PurchaseDocumentConflictException(
                $"Supplier document '{supplierDocumentNumber}' already exists for this supplier.");
        }
    }

    private async Task<decimal> ResolveExchangeRateAsync(
        Guid organizationId,
        Currency baseCurrency,
        Currency documentCurrency,
        DateOnly documentDate,
        CancellationToken cancellationToken)
    {
        if (baseCurrency.Id == documentCurrency.Id)
        {
            return 1m;
        }

        var direct = await currencyRepository.GetLatestExchangeRateAsync(
            organizationId,
            baseCurrency.Id,
            documentCurrency.Id,
            documentDate,
            cancellationToken);

        if (direct is not null)
        {
            return direct.Rate;
        }

        var inverse = await currencyRepository.GetLatestExchangeRateAsync(
            organizationId,
            documentCurrency.Id,
            baseCurrency.Id,
            documentDate,
            cancellationToken);

        if (inverse is not null)
        {
            return decimal.Round(
                1m / inverse.Rate,
                10,
                MidpointRounding.AwayFromZero);
        }

        throw new PurchaseDocumentConflictException(
            $"No exchange rate is available between {baseCurrency.Code} and {documentCurrency.Code} as of {documentDate:yyyy-MM-dd}.");
    }

    private async Task<PurchaseDocumentResponse> BuildResponseAsync(
        PurchaseDocument document,
        CancellationToken cancellationToken)
    {
        var lines = await repository.ListLinesAsync(
            document.OrganizationId,
            document.Id,
            trackChanges: false,
            cancellationToken);

        return new PurchaseDocumentResponse(
            document.Id,
            document.OrganizationId,
            document.InternalNumber,
            document.SupplierDocumentNumber,
            document.Type.ToString(),
            document.Status.ToString(),
            document.DocumentDate,
            document.DueDate,
            document.CounterpartyId,
            document.SupplierName,
            document.SupplierLegalName,
            document.SupplierRegistrationNumber,
            document.SupplierVatNumber,
            document.SupplierCountryCode,
            document.SupplierAddressLine1,
            document.SupplierAddressLine2,
            document.SupplierCity,
            document.SupplierPostalCode,
            document.CurrencyId,
            document.CurrencyCode,
            document.BaseCurrencyId,
            document.BaseCurrencyCode,
            document.ExchangeRate,
            document.NetTotal,
            document.VatTotal,
            document.RecoverableVatTotal,
            document.NonRecoverableVatTotal,
            document.GrossTotal,
            document.Notes,
            document.ReceivedAtUtc,
            document.CancelledAtUtc,
            document.CancellationReason,
            lines.OrderBy(line => line.LineNumber).Select(MapLine).ToArray(),
            document.CreatedAtUtc,
            document.CreatedByUserId,
            document.UpdatedAtUtc,
            document.UpdatedByUserId);
    }

    private static (decimal Net, decimal Vat, decimal RecoverableVat, decimal NonRecoverableVat, decimal Gross)
        CalculateAmounts(
            decimal quantity,
            decimal unitPrice,
            decimal discountPercent,
            decimal vatRatePercent,
            decimal recoverableVatPercent,
            int currencyDecimalPlaces)
    {
        if (quantity <= 0m || decimal.Round(quantity, 6) != quantity)
        {
            throw new PurchaseDocumentConflictException(
                "Quantity must be positive and support at most six decimal places.");
        }

        if (unitPrice < 0m || decimal.Round(unitPrice, 4) != unitPrice)
        {
            throw new PurchaseDocumentConflictException(
                "Unit price cannot be negative and supports at most four decimal places.");
        }

        if (discountPercent is < 0m or > 100m ||
            recoverableVatPercent is < 0m or > 100m)
        {
            throw new PurchaseDocumentConflictException(
                "Discount and recoverable VAT percentages must be between 0 and 100.");
        }

        var net = decimal.Round(
            quantity * unitPrice * (1m - discountPercent / 100m),
            currencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var vat = decimal.Round(
            net * vatRatePercent / 100m,
            currencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var recoverableVat = decimal.Round(
            vat * recoverableVatPercent / 100m,
            currencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var nonRecoverableVat = vat - recoverableVat;

        return (
            net,
            vat,
            recoverableVat,
            nonRecoverableVat,
            net + vat);
    }

    private static void SetTotals(
        PurchaseDocument document,
        IReadOnlyCollection<PurchaseDocumentLine> lines,
        DateTimeOffset now)
    {
        document.SetTotals(
            lines.Sum(line => line.NetAmount),
            lines.Sum(line => line.VatAmount),
            lines.Sum(line => line.RecoverableVatAmount),
            lines.Sum(line => line.NonRecoverableVatAmount),
            lines.Sum(line => line.GrossAmount),
            now);
    }

    private static DateOnly ResolveDueDate(
        DateOnly documentDate,
        DateOnly? requestedDueDate,
        int paymentTermDays)
    {
        var dueDate = requestedDueDate ??
            documentDate.AddDays(paymentTermDays);

        if (dueDate < documentDate)
        {
            throw new PurchaseDocumentConflictException(
                "Due date cannot be before document date.");
        }

        return dueDate;
    }

    private static string? NormalizeSupplierNumber(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized.ToUpperInvariant();
    }

    private static void EnsureDraft(PurchaseDocument document)
    {
        if (document.Status != PurchaseDocumentStatus.Draft)
        {
            throw new PurchaseDocumentConflictException(
                "Only draft purchase documents can be edited.");
        }
    }

    private static PurchaseDocumentType ParseRequiredType(string value)
    {
        return ParseType(value) ??
            throw new PurchaseDocumentQueryException(
                "Purchase document type is required.");
    }

    private static PurchaseDocumentType? ParseType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Enum.TryParse<PurchaseDocumentType>(
                value,
                ignoreCase: true,
                out var type) ||
            !Enum.IsDefined(type))
        {
            throw new PurchaseDocumentQueryException(
                $"Unknown purchase document type '{value}'.");
        }

        return type;
    }

    private static PurchaseDocumentStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Enum.TryParse<PurchaseDocumentStatus>(
                value,
                ignoreCase: true,
                out var status) ||
            !Enum.IsDefined(status))
        {
            throw new PurchaseDocumentQueryException(
                $"Unknown purchase document status '{value}'.");
        }

        return status;
    }

    private static void ValidateDateRange(
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value > toDate.Value)
        {
            throw new PurchaseDocumentQueryException(
                "fromDate cannot be after toDate.");
        }
    }

    private static string FormatInternalNumber(
        int calendarYear,
        long sequence)
    {
        if (calendarYear < 1 || sequence <= 0)
        {
            throw new InvalidOperationException(
                "Purchase document sequence is invalid.");
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"PUR-{calendarYear:D4}-{sequence:D6}");
    }

    private static PurchaseDocumentSummaryResponse MapSummary(
        PurchaseDocument document) =>
        new(
            document.Id,
            document.InternalNumber,
            document.SupplierDocumentNumber,
            document.Type.ToString(),
            document.Status.ToString(),
            document.DocumentDate,
            document.DueDate,
            document.CounterpartyId,
            document.SupplierName,
            document.CurrencyId,
            document.CurrencyCode,
            document.NetTotal,
            document.VatTotal,
            document.RecoverableVatTotal,
            document.NonRecoverableVatTotal,
            document.GrossTotal,
            document.ReceivedAtUtc,
            document.CancelledAtUtc);

    private static PurchaseDocumentLineResponse MapLine(
        PurchaseDocumentLine line) =>
        new(
            line.Id,
            line.LineNumber,
            line.ItemCode,
            line.Description,
            line.Quantity,
            line.UnitOfMeasure,
            line.UnitPrice,
            line.DiscountPercent,
            line.VatCodeId,
            line.VatCode,
            line.VatRatePercent,
            line.RecoverableVatPercent,
            line.NetAmount,
            line.VatAmount,
            line.RecoverableVatAmount,
            line.NonRecoverableVatAmount,
            line.GrossAmount);
}
