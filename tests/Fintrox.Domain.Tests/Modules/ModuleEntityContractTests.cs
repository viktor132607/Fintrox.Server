using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Modules;

[TestClass]
public sealed class ModuleEntityContractTests
{
    public static IEnumerable<object[]> EntityTypes()
    {
        var assemblies = new[]
        {
            typeof(Fintrox.Modules.Accounting.Domain.Entities.AccountGroup).Assembly,
            typeof(Fintrox.Modules.Audit.Domain.Entities.AuditRetentionPolicy).Assembly,
            typeof(Fintrox.Modules.Banking.Domain.Entities.BankAccount).Assembly,
            typeof(Fintrox.Modules.Budgeting.Domain.Entities.Budget).Assembly,
            typeof(Fintrox.Modules.CRM.Domain.Entities.Lead).Assembly,
            typeof(Fintrox.Modules.Capabilities.Domain.Entities.CapabilityDefinition).Assembly,
            typeof(Fintrox.Modules.Commerce.Domain.Entities.CatalogListing).Assembly,
            typeof(Fintrox.Modules.Consolidation.Domain.Entities.CompanyGroup).Assembly,
            typeof(Fintrox.Modules.ContractManagement.Domain.Entities.BusinessContract).Assembly,
            typeof(Fintrox.Modules.Counterparties.Domain.Entities.CounterpartyContact).Assembly,
            typeof(Fintrox.Modules.Currencies.Domain.Entities.ExchangeRateProvider).Assembly,
            typeof(Fintrox.Modules.Documents.Domain.Entities.StoredDocument).Assembly,
            typeof(Fintrox.Modules.Experience.Domain.Entities.ExperienceProfile).Assembly,
            typeof(Fintrox.Modules.FixedAssets.Domain.Entities.AssetCategory).Assembly,
            typeof(Fintrox.Modules.HumanResources.Domain.Entities.Employee).Assembly,
            typeof(Fintrox.Modules.Identity.Domain.Entities.OrganizationInvitation).Assembly,
            typeof(Fintrox.Modules.Integrations.Domain.Entities.AppRegistration).Assembly,
            typeof(Fintrox.Modules.Inventory.Domain.Entities.InventoryItem).Assembly,
            typeof(Fintrox.Modules.Manufacturing.Domain.Entities.BillOfMaterials).Assembly,
            typeof(Fintrox.Modules.OrderManagement.Domain.Entities.FulfillmentOrder).Assembly,
            typeof(Fintrox.Modules.Organizations.Domain.Entities.Branch).Assembly,
            typeof(Fintrox.Modules.Payments.Domain.Entities.PaymentRefund).Assembly,
            typeof(Fintrox.Modules.Payroll.Domain.Entities.EmployeeCompensation).Assembly,
            typeof(Fintrox.Modules.Projects.Domain.Entities.Project).Assembly,
            typeof(Fintrox.Modules.Purchases.Domain.Entities.PurchaseRequisition).Assembly,
            typeof(Fintrox.Modules.QualityManagement.Domain.Entities.InspectionPlan).Assembly,
            typeof(Fintrox.Modules.Reporting.Domain.Entities.ReportDefinition).Assembly,
            typeof(Fintrox.Modules.ResourceManagement.Domain.Entities.Resource).Assembly,
            typeof(Fintrox.Modules.RevenueManagement.Domain.Entities.RevenueRecognitionRule).Assembly,
            typeof(Fintrox.Modules.Sales.Domain.Entities.SalesQuotation).Assembly,
            typeof(Fintrox.Modules.ServiceManagement.Domain.Entities.SupportTicket).Assembly,
            typeof(Fintrox.Modules.SubscriptionBilling.Domain.Entities.SubscriptionPlan).Assembly,
            typeof(Fintrox.Modules.SupplyChain.Domain.Entities.DemandForecast).Assembly,
            typeof(Fintrox.Modules.Tax.Domain.Entities.TaxJurisdiction).Assembly,
            typeof(Fintrox.Modules.WarehouseManagement.Domain.Entities.BinLocation).Assembly,
            typeof(Fintrox.Modules.Workflows.Domain.Entities.WorkflowDefinition).Assembly
        };
        return assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSealed && type.Namespace!.EndsWith(".Entities", StringComparison.Ordinal))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type => new object[] { type });
    }

    [TestMethod]
    [DynamicData(nameof(EntityTypes))]
    public void ValidEntityPreservesIdentityAndScope(Type entityType)
    {
        var constructor = entityType.GetConstructors().Single();
        var arguments = ValidArguments(constructor);
        var entity = constructor.Invoke(arguments);
        Assert.AreEqual(arguments[0], entityType.GetProperty("Id")!.GetValue(entity));
        Assert.AreEqual(arguments[1], entityType.GetProperty("OrganizationId")!.GetValue(entity));
        Assert.AreEqual(arguments[2], entityType.GetProperty("CreatedAtUtc")!.GetValue(entity));
        Assert.IsFalse(entityType.GetProperties().Any(property => property.SetMethod?.IsPublic == true),
            "Entities must not allow callers to bypass constructor invariants or change tenant ownership.");
    }

    [TestMethod]
    [DynamicData(nameof(EntityTypes))]
    public void EmptyIdentityTenantAndForeignReferencesAreRejected(Type entityType)
    {
        var constructor = entityType.GetConstructors().Single();
        var parameters = constructor.GetParameters();
        for (var index = 0; index < parameters.Length; index++)
        {
            if (parameters[index].ParameterType != typeof(Guid)) continue;
            var arguments = ValidArguments(constructor);
            arguments[index] = Guid.Empty;
            var exception = Assert.ThrowsExactly<TargetInvocationException>(() => constructor.Invoke(arguments));
            Assert.IsInstanceOfType<ArgumentException>(exception.InnerException);
        }
    }

    [TestMethod]
    [DynamicData(nameof(EntityTypes))]
    public void MissingDatesAndTimestampsAreRejected(Type entityType)
    {
        var constructor = entityType.GetConstructors().Single();
        var parameters = constructor.GetParameters();
        for (var index = 0; index < parameters.Length; index++)
        {
            var type = parameters[index].ParameterType;
            if (type != typeof(DateOnly) && type != typeof(DateTimeOffset)) continue;
            var arguments = ValidArguments(constructor);
            arguments[index] = Activator.CreateInstance(type)!;
            var exception = Assert.ThrowsExactly<TargetInvocationException>(() => constructor.Invoke(arguments));
            Assert.IsInstanceOfType<ArgumentException>(exception.InnerException);
        }
    }

    private static object[] ValidArguments(ConstructorInfo constructor) => constructor.GetParameters().Select(parameter =>
    {
        var type = parameter.ParameterType;
        var name = parameter.Name!;
        if (type == typeof(Guid)) return (object)Guid.NewGuid();
        if (type == typeof(DateTimeOffset)) return new DateTimeOffset(2026, 1, name == "createdAtUtc" ? 1 : 2, 0, 0, 0, TimeSpan.Zero);
        if (type == typeof(DateOnly)) return new DateOnly(2026, 1, 2);
        if (type == typeof(decimal)) return 1m;
        if (type == typeof(int)) return 1;
        if (type == typeof(long)) return 1L;
        if (type == typeof(bool)) return true;
        if (type.IsEnum) return Enum.GetValues(type).GetValue(0)!;
        if (type == typeof(string))
        {
            if (name.EndsWith("Currency", StringComparison.OrdinalIgnoreCase)) return "EUR";
            if (name == "countryCode") return "BG";
            if (name is "sha256" or "tokenHash" || name.EndsWith("Sha256", StringComparison.OrdinalIgnoreCase)) return new string('a', 64);
            if (name.EndsWith("Json", StringComparison.Ordinal)) return "{}";
            if (name == "email") return "test@example.test";
            return "Sample";
        }
        throw new InvalidOperationException($"No sample for {type}.");
    }).ToArray();
}
