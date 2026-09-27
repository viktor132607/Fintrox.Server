using Fintrox.Application.Accounting;
using Fintrox.Application.Authorization;
using Fintrox.Application.Counterparties;
using Fintrox.Application.Currencies;
using Fintrox.Application.Organizations;
using Fintrox.Application.Payments;
using Fintrox.Application.Purchases;
using Fintrox.Application.Reports;
using Fintrox.Application.Sales;
using Fintrox.Application.Tax;
using Microsoft.Extensions.DependencyInjection;

namespace Fintrox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IFiscalCalendarService, FiscalCalendarService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<ICounterpartyService, CounterpartyService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IVatCodeService, VatCodeService>();
        services.AddScoped<IAccountingReportService, AccountingReportService>();
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.AddScoped<IPurchaseDocumentService, PurchaseDocumentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IOrganizationMemberService, OrganizationMemberService>();
        services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();

        return services;
    }
}
