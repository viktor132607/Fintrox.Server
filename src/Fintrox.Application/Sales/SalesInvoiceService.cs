using System.Globalization;
using Fintrox.Application.Accounting;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Counterparties;
using Fintrox.Application.Currencies;
using Fintrox.Application.Tax;
using Fintrox.Contracts.Sales;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Sales;
using Fintrox.Domain.Tax;

namespace Fintrox.Application.Sales;

public sealed class SalesInvoiceService(
    ISalesInvoiceRepository repository,
    ICounterpartyRepository counterpartyRepository,
    ICurrencyRepository currencyRepository,
    IVatCodeRepository vatCodeRepository,
    IAutoPostingEngine autoPostingEngine,
    ITransactionRunner transactionRunner,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : ISalesInvoiceService
{
    public async Task<IReadOnlyList<SalesInvoiceSummaryResponse>> ListAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDate, toDate);
        var parsedStatus = ParseStatus(status);
        var organizationId = currentOrganization.RequireOrganizationId();

        var invoices = await repository.ListInvoicesAsync(
            organizationId,
            fromDate,
            toDate,
            parsedStatus,
            counterpartyId,
            cancellationToken);

        return invoices.Select(MapSummary).ToArray();
    }

    public async Task<SalesInvoiceResponse?> GetAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var invoice = await repository.GetInvoiceAsync(
            organizationId,
            invoiceId,
            trackChanges: false,
            cancellationToken);

        return invoice is null
            ? null
            : await BuildResponseAsync(invoice, cancellationToken);
    }

    public async Task<SalesInvoiceResponse> CreateAsync(
        CreateSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var invoice = await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var counterparty = await RequireCustomerAsync(
                    organizationId,
                    request.CounterpartyId,
                    transactionCancellationToken);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    request.CurrencyId,
                    transactionCancellationToken);

                var dueDate = ResolveDueDate(
                    request.InvoiceDate,
                    request.DueDate,
                    counterparty.PaymentTermDays);

                var now = timeProvider.GetUtcNow();
                var draft = SalesInvoice.CreateDraft(
                    organizationId,
                    counterparty.Id,
                    request.InvoiceDate,
                    dueDate,
                    currency.Id,
                    counterparty.Name,
                    counterparty.LegalName,
                    counterparty.RegistrationNumber,
                    counterparty.VatNumber,
                    counterparty.CountryCode,
                    counterparty.AddressLine1,
                    counterparty.AddressLine2,
                    counterparty.City,
                    counterparty.PostalCode,
                    currency.Code,
                    request.Notes,
                    now);

                await repository.AddInvoiceAsync(
                    draft,
                    transactionCancellationToken);

                var lines = new List<SalesInvoiceLine>();
                var lineNumber = 1;

                foreach (var requestLine in request.Lines ?? [])
                {
                    var vatCode = await RequireSalesVatCodeAsync(
                        organizationId,
                        requestLine.VatCodeId,
                        request.InvoiceDate,
                        transactionCancellationToken);

                    var amounts = CalculateAmounts(
                        requestLine.Quantity,
                        requestLine.UnitPrice,
                        requestLine.DiscountPercent,
                        vatCode.RatePercent,
                        currency.DecimalPlaces);

                    var line = SalesInvoiceLine.Create(
                        organizationId,
                        draft.Id,
                        lineNumber++,
                        requestLine.ItemCode,
                        requestLine.Description,
                        requestLine.Quantity,
                        requestLine.UnitOfMeasure,
                        requestLine.UnitPrice,
                        requestLine.DiscountPercent,
                        vatCode.Id,
                        vatCode.Code,
                        vatCode.RatePercent,
                        amounts.Net,
                        amounts.Vat,
                        amounts.Gross,
                        now);

                    lines.Add(line);
                    await repository.AddLineAsync(
                        line,
                        transactionCancellationToken);
                }

                SetTotals(draft, lines, now);

                await repository.SaveChangesAsync(
                    transactionCancellationToken);

                return draft;
            },
            cancellationToken);

        return await BuildResponseAsync(invoice, cancellationToken);
    }

    public async Task<SalesInvoiceResponse?> UpdateAsync(
        Guid invoiceId,
        UpdateSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var invoice = await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var draft = await repository.GetInvoiceAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (draft is null)
                {
                    return null;
                }

                EnsureDraft(draft);

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: false,
                    transactionCancellationToken);

                if (draft.CurrencyId != request.CurrencyId &&
                    lines.Count > 0)
                {
                    throw new SalesInvoiceConflictException(
                        "Currency cannot be changed after invoice lines have been added.");
                }

                var counterparty = await RequireCustomerAsync(
                    organizationId,
                    request.CounterpartyId,
                    transactionCancellationToken);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    request.CurrencyId,
                    transactionCancellationToken);

                if (draft.InvoiceDate != request.InvoiceDate)
                {
                    foreach (var line in lines)
                    {
                        await RequireSalesVatCodeAsync(
                            organizationId,
                            line.VatCodeId,
                            request.InvoiceDate,
                            transactionCancellationToken);
                    }
                }

                var dueDate = ResolveDueDate(
                    request.InvoiceDate,
                    request.DueDate,
                    counterparty.PaymentTermDays);

                draft.UpdateDraft(
                    counterparty.Id,
                    request.InvoiceDate,
                    dueDate,
                    currency.Id,
                    counterparty.Name,
                    counterparty.LegalName,
                    counterparty.RegistrationNumber,
                    counterparty.VatNumber,
                    counterparty.CountryCode,
                    counterparty.AddressLine1,
                    counterparty.AddressLine2,
                    counterparty.City,
                    counterparty.PostalCode,
                    currency.Code,
                    request.Notes,
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(
                    transactionCancellationToken);

                return draft;
            },
            cancellationToken);

        return invoice is null
            ? null
            : await BuildResponseAsync(invoice, cancellationToken);
    }

    public async Task<SalesInvoiceLineResponse?> AddLineAsync(
        Guid invoiceId,
        CreateSalesInvoiceLineRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var invoice = await repository.GetInvoiceAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (invoice is null)
                {
                    return null;
                }

                EnsureDraft(invoice);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    invoice.CurrencyId,
                    transactionCancellationToken);

                var vatCode = await RequireSalesVatCodeAsync(
                    organizationId,
                    request.VatCodeId,
                    invoice.InvoiceDate,
                    transactionCancellationToken);

                var amounts = CalculateAmounts(
                    request.Quantity,
                    request.UnitPrice,
                    request.DiscountPercent,
                    vatCode.RatePercent,
                    currency.DecimalPlaces);

                var existingLines = await repository.ListLinesAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: false,
                    transactionCancellationToken);

                var lineNumber = await repository.GetNextLineNumberAsync(
                    organizationId,
                    invoiceId,
                    transactionCancellationToken);

                var now = timeProvider.GetUtcNow();
                var line = SalesInvoiceLine.Create(
                    organizationId,
                    invoice.Id,
                    lineNumber,
                    request.ItemCode,
                    request.Description,
                    request.Quantity,
                    request.UnitOfMeasure,
                    request.UnitPrice,
                    request.DiscountPercent,
                    vatCode.Id,
                    vatCode.Code,
                    vatCode.RatePercent,
                    amounts.Net,
                    amounts.Vat,
                    amounts.Gross,
                    now);

                await repository.AddLineAsync(
                    line,
                    transactionCancellationToken);

                SetTotals(
                    invoice,
                    [.. existingLines, line],
                    now);

                await repository.SaveChangesAsync(
                    transactionCancellationToken);

                return MapLine(line);
            },
            cancellationToken);
    }

    public async Task<SalesInvoiceLineResponse?> UpdateLineAsync(
        Guid invoiceId,
        Guid lineId,
        UpdateSalesInvoiceLineRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var invoice = await repository.GetInvoiceAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (invoice is null)
                {
                    return null;
                }

                EnsureDraft(invoice);

                var line = await repository.GetLineAsync(
                    organizationId,
                    invoiceId,
                    lineId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (line is null)
                {
                    return null;
                }

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    invoice.CurrencyId,
                    transactionCancellationToken);

                var vatCode = await RequireSalesVatCodeAsync(
                    organizationId,
                    request.VatCodeId,
                    invoice.InvoiceDate,
                    transactionCancellationToken);

                var amounts = CalculateAmounts(
                    request.Quantity,
                    request.UnitPrice,
                    request.DiscountPercent,
                    vatCode.RatePercent,
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
                    amounts.Net,
                    amounts.Vat,
                    amounts.Gross,
                    now);

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: false,
                    transactionCancellationToken);

                var totals = lines
                    .Select(existing => existing.Id == line.Id ? line : existing)
                    .ToArray();

                SetTotals(invoice, totals, now);

                await repository.SaveChangesAsync(
                    transactionCancellationToken);

                return MapLine(line);
            },
            cancellationToken);
    }

    public async Task<bool> DeleteLineAsync(
        Guid invoiceId,
        Guid lineId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var invoice = await repository.GetInvoiceAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (invoice is null)
                {
                    return false;
                }

                EnsureDraft(invoice);

                var line = await repository.GetLineAsync(
                    organizationId,
                    invoiceId,
                    lineId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (line is null)
                {
                    return false;
                }

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: false,
                    transactionCancellationToken);

                repository.RemoveLine(line);

                SetTotals(
                    invoice,
                    lines.Where(existing => existing.Id != line.Id).ToArray(),
                    timeProvider.GetUtcNow());

                await repository.SaveChangesAsync(
                    transactionCancellationToken);

                return true;
            },
            cancellationToken);
    }

    public async Task<SalesInvoiceResponse?> IssueAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var invoice = await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var draft = await repository.GetInvoiceAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (draft is null)
                {
                    return null;
                }

                EnsureDraft(draft);

                var customer = await RequireCustomerAsync(
                    organizationId,
                    draft.CounterpartyId,
                    transactionCancellationToken);

                var currency = await RequireActiveCurrencyAsync(
                    organizationId,
                    draft.CurrencyId,
                    transactionCancellationToken);

                var baseCurrency = await currencyRepository.GetBaseCurrencyAsync(
                    organizationId,
                    trackChanges: false,
                    transactionCancellationToken);

                if (baseCurrency is null || !baseCurrency.IsActive)
                {
                    throw new SalesInvoiceConflictException(
                        "The organization must have an active base currency before an invoice can be issued.");
                }

                var lines = await repository.ListLinesAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (lines.Count == 0)
                {
                    throw new SalesInvoiceConflictException(
                        "A sales invoice must contain at least one line before it can be issued.");
                }

                foreach (var line in lines)
                {
                    var vatCode = await RequireSalesVatCodeAsync(
                        organizationId,
                        line.VatCodeId,
                        draft.InvoiceDate,
                        transactionCancellationToken);

                    if (!string.Equals(
                            line.VatCode,
                            vatCode.Code,
                            StringComparison.Ordinal) ||
                        line.VatRatePercent != vatCode.RatePercent)
                    {
                        throw new SalesInvoiceConflictException(
                            $"VAT configuration for line {line.LineNumber} changed after the line was created. Update the line before issuing.");
                    }
                }

                SetTotals(
                    draft,
                    lines,
                    timeProvider.GetUtcNow());

                if (draft.GrossTotal <= 0m)
                {
                    throw new SalesInvoiceConflictException(
                        "A sales invoice must have a positive gross total before it can be issued.");
                }

                var exchangeRate = await ResolveExchangeRateAsync(
                    organizationId,
                    baseCurrency,
                    currency,
                    draft.InvoiceDate,
                    transactionCancellationToken);

                var sequence = await repository.AllocateInvoiceSequenceAsync(
                    organizationId,
                    draft.InvoiceDate.Year,
                    transactionCancellationToken);

                draft.Issue(
                    FormatInvoiceNumber(
                        draft.InvoiceDate.Year,
                        sequence),
                    baseCurrency.Id,
                    baseCurrency.Code,
                    currency.Code,
                    customer.Name,
                    customer.LegalName,
                    customer.RegistrationNumber,
                    customer.VatNumber,
                    customer.CountryCode,
                    customer.AddressLine1,
                    customer.AddressLine2,
                    customer.City,
                    customer.PostalCode,
                    exchangeRate,
                    timeProvider.GetUtcNow());

                try
                {
                    await autoPostingEngine.PostSalesInvoiceAsync(
                        draft,
                        lines,
                        transactionCancellationToken);
                }
                catch (AutoPostingException exception)
                {
                    throw new SalesInvoiceConflictException(
                        $"Accounting auto-posting failed: {exception.Message}");
                }

                await repository.SaveChangesAsync(
                    transactionCancellationToken);

                return draft;
            },
            cancellationToken);

        return invoice is null
            ? null
            : await BuildResponseAsync(invoice, cancellationToken);
    }

    public async Task<SalesInvoiceResponse?> CancelAsync(
        Guid invoiceId,
        CancelSalesInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        var invoice = await transactionRunner.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var existing = await repository.GetInvoiceAsync(
                    organizationId,
                    invoiceId,
                    trackChanges: true,
                    transactionCancellationToken);

                if (existing is null)
                {
                    return null;
                }

                if (existing.Status != SalesInvoiceStatus.Issued)
                {
                    throw new SalesInvoiceConflictException(
                        "Only an issued sales invoice can be cancelled.");
                }

                existing.Cancel(
                    request.Reason,
                    timeProvider.GetUtcNow());

                try
                {
                    await autoPostingEngine.ReverseSalesInvoiceAsync(
                        existing,
                        request.Reason,
                        transactionCancellationToken);
                }
                catch (AutoPostingException exception)
                {
                    throw new SalesInvoiceConflictException(
                        $"Accounting reversal failed: {exception.Message}");
                }

                await repository.SaveChangesAsync(
                    transactionCancellationToken);

                return existing;
            },
            cancellationToken);

        return invoice is null
            ? null
            : await BuildResponseAsync(invoice, cancellationToken);
    }

    private async Task<SalesInvoiceResponse> BuildResponseAsync(
        SalesInvoice invoice,
        CancellationToken cancellationToken)
    {
        var lines = await repository.ListLinesAsync(
            invoice.OrganizationId,
            invoice.Id,
            trackChanges: false,
            cancellationToken);

        return new SalesInvoiceResponse(
            invoice.Id,
            invoice.OrganizationId,
            invoice.Number,
            invoice.Status.ToString(),
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.CounterpartyId,
            invoice.CustomerName,
            invoice.CustomerLegalName,
            invoice.CustomerRegistrationNumber,
            invoice.CustomerVatNumber,
            invoice.CustomerCountryCode,
            invoice.CustomerAddressLine1,
            invoice.CustomerAddressLine2,
            invoice.CustomerCity,
            invoice.CustomerPostalCode,
            invoice.CurrencyId,
            invoice.CurrencyCode,
            invoice.BaseCurrencyId,
            invoice.BaseCurrencyCode,
            invoice.ExchangeRate,
            invoice.NetTotal,
            invoice.VatTotal,
            invoice.GrossTotal,
            invoice.Notes,
            invoice.IssuedAtUtc,
            invoice.CancelledAtUtc,
            invoice.CancellationReason,
            lines.OrderBy(line => line.LineNumber).Select(MapLine).ToArray(),
            invoice.CreatedAtUtc,
            invoice.CreatedByUserId,
            invoice.UpdatedAtUtc,
            invoice.UpdatedByUserId);
    }

    private async Task<Counterparty> RequireCustomerAsync(
        Guid organizationId,
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        if (counterpartyId == Guid.Empty)
        {
            throw new SalesInvoiceConflictException(
                "A customer is required.");
        }

        var counterparty = await counterpartyRepository.GetAsync(
            organizationId,
            counterpartyId,
            trackChanges: false,
            cancellationToken);

        if (counterparty is null)
        {
            throw new SalesInvoiceConflictException(
                "The selected customer does not exist in this organization.");
        }

        if (!counterparty.IsActive || !counterparty.IsCustomer)
        {
            throw new SalesInvoiceConflictException(
                "The selected counterparty must be an active customer.");
        }

        return counterparty;
    }

    private async Task<Currency> RequireActiveCurrencyAsync(
        Guid organizationId,
        Guid currencyId,
        CancellationToken cancellationToken)
    {
        if (currencyId == Guid.Empty)
        {
            throw new SalesInvoiceConflictException(
                "Invoice currency is required.");
        }

        var currency = await currencyRepository.GetCurrencyAsync(
            organizationId,
            currencyId,
            trackChanges: false,
            cancellationToken);

        if (currency is null || !currency.IsActive)
        {
            throw new SalesInvoiceConflictException(
                "The selected invoice currency must exist and be active.");
        }

        return currency;
    }

    private async Task<VatCode> RequireSalesVatCodeAsync(
        Guid organizationId,
        Guid vatCodeId,
        DateOnly invoiceDate,
        CancellationToken cancellationToken)
    {
        if (vatCodeId == Guid.Empty)
        {
            throw new SalesInvoiceConflictException(
                "A VAT code is required for every invoice line.");
        }

        var vatCode = await vatCodeRepository.GetAsync(
            organizationId,
            vatCodeId,
            trackChanges: false,
            cancellationToken);

        if (vatCode is null ||
            !vatCode.IsActive ||
            !vatCode.AppliesToSales ||
            !vatCode.IsValidOn(invoiceDate))
        {
            throw new SalesInvoiceConflictException(
                "The selected VAT code must be active, valid on the invoice date, and applicable to sales.");
        }

        return vatCode;
    }

    private async Task<decimal> ResolveExchangeRateAsync(
        Guid organizationId,
        Currency baseCurrency,
        Currency invoiceCurrency,
        DateOnly invoiceDate,
        CancellationToken cancellationToken)
    {
        if (baseCurrency.Id == invoiceCurrency.Id)
        {
            return 1m;
        }

        var direct = await currencyRepository.GetLatestExchangeRateAsync(
            organizationId,
            baseCurrency.Id,
            invoiceCurrency.Id,
            invoiceDate,
            cancellationToken);

        if (direct is not null)
        {
            return direct.Rate;
        }

        var inverse = await currencyRepository.GetLatestExchangeRateAsync(
            organizationId,
            invoiceCurrency.Id,
            baseCurrency.Id,
            invoiceDate,
            cancellationToken);

        if (inverse is not null)
        {
            return decimal.Round(
                1m / inverse.Rate,
                10,
                MidpointRounding.AwayFromZero);
        }

        throw new SalesInvoiceConflictException(
            $"No exchange rate is available between {baseCurrency.Code} and {invoiceCurrency.Code} as of {invoiceDate:yyyy-MM-dd}.");
    }

    private static (decimal Net, decimal Vat, decimal Gross) CalculateAmounts(
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal vatRatePercent,
        int currencyDecimalPlaces)
    {
        if (quantity <= 0m ||
            decimal.Round(quantity, 6) != quantity)
        {
            throw new SalesInvoiceConflictException(
                "Quantity must be positive and support at most six decimal places.");
        }

        if (unitPrice < 0m ||
            decimal.Round(unitPrice, 4) != unitPrice)
        {
            throw new SalesInvoiceConflictException(
                "Unit price cannot be negative and supports at most four decimal places.");
        }

        if (discountPercent is < 0m or > 100m ||
            decimal.Round(discountPercent, 4) != discountPercent)
        {
            throw new SalesInvoiceConflictException(
                "Discount percent must be between 0 and 100 and support at most four decimal places.");
        }

        var undiscounted = quantity * unitPrice;
        var discounted = undiscounted *
            (1m - (discountPercent / 100m));

        var net = decimal.Round(
            discounted,
            currencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var vat = decimal.Round(
            net * vatRatePercent / 100m,
            currencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        return (net, vat, net + vat);
    }

    private static void SetTotals(
        SalesInvoice invoice,
        IReadOnlyCollection<SalesInvoiceLine> lines,
        DateTimeOffset now)
    {
        invoice.SetTotals(
            lines.Sum(line => line.NetAmount),
            lines.Sum(line => line.VatAmount),
            lines.Sum(line => line.GrossAmount),
            now);
    }

    private static DateOnly ResolveDueDate(
        DateOnly invoiceDate,
        DateOnly? requestedDueDate,
        int paymentTermDays)
    {
        var dueDate = requestedDueDate ??
            invoiceDate.AddDays(paymentTermDays);

        if (dueDate < invoiceDate)
        {
            throw new SalesInvoiceConflictException(
                "Due date cannot be before invoice date.");
        }

        return dueDate;
    }

    private static SalesInvoiceStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Enum.TryParse<SalesInvoiceStatus>(
                value,
                ignoreCase: true,
                out var status) ||
            !Enum.IsDefined(status))
        {
            throw new SalesInvoiceQueryException(
                $"Unknown sales invoice status '{value}'.");
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
            throw new SalesInvoiceQueryException(
                "fromDate cannot be after toDate.");
        }
    }

    private static void EnsureDraft(SalesInvoice invoice)
    {
        if (invoice.Status != SalesInvoiceStatus.Draft)
        {
            throw new SalesInvoiceConflictException(
                "Only draft sales invoices can be edited.");
        }
    }

    private static string FormatInvoiceNumber(
        int calendarYear,
        long sequence)
    {
        if (calendarYear < 1 || sequence <= 0)
        {
            throw new InvalidOperationException(
                "Sales invoice number sequence is invalid.");
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"INV-{calendarYear:D4}-{sequence:D6}");
    }

    private static SalesInvoiceSummaryResponse MapSummary(
        SalesInvoice invoice) =>
        new(
            invoice.Id,
            invoice.Number,
            invoice.Status.ToString(),
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.CounterpartyId,
            invoice.CustomerName,
            invoice.CurrencyId,
            invoice.CurrencyCode,
            invoice.NetTotal,
            invoice.VatTotal,
            invoice.GrossTotal,
            invoice.IssuedAtUtc,
            invoice.CancelledAtUtc);

    private static SalesInvoiceLineResponse MapLine(
        SalesInvoiceLine line) =>
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
            line.NetAmount,
            line.VatAmount,
            line.GrossAmount);
}
