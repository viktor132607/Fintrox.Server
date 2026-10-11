using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fintrox.Modules.CRM.Domain.Entities;
using Fintrox.Modules.Manufacturing.Domain.Entities;
using Fintrox.Modules.SupplyChain.Domain.Entities;
using Fintrox.Modules.WarehouseManagement.Domain.Entities;
using Fintrox.Modules.OrderManagement.Domain.Entities;
using Fintrox.Modules.RevenueManagement.Domain.Entities;
using Fintrox.Modules.SubscriptionBilling.Domain.Entities;
using Fintrox.Modules.HumanResources.Domain.Entities;
using Fintrox.Modules.QualityManagement.Domain.Entities;
using Fintrox.Modules.ServiceManagement.Domain.Entities;
using Fintrox.Modules.Commerce.Domain.Entities;
using Fintrox.Modules.ResourceManagement.Domain.Entities;
using Fintrox.Modules.ContractManagement.Domain.Entities;
using Fintrox.Modules.ContractManagement.Domain;
using Fintrox.Modules.FixedAssets.Domain.Entities;
using Fintrox.Modules.Payroll.Domain.Entities;
using Fintrox.Modules.Capabilities.Domain.Entities;
using Fintrox.Modules.Experience.Domain.Entities;
using Fintrox.Modules.Identity.Domain.Entities;
using Fintrox.Modules.Workflows.Domain.Entities;

namespace Fintrox.Domain.Tests.Modules;

[TestClass]
public sealed class ModuleBusinessInvariantTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 11, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Start = new(2026, 10, 11);
    private static Guid Id => Guid.NewGuid();

    [TestMethod]
    public void OpportunityRejectsNegativeValueAndNormalizesCurrency()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Opportunity(Id, Id, Now, Id, "Deal", -1m, "EUR", Start));
        var opportunity = new Opportunity(Id, Id, Now, Id, " Deal ", 10m, " eur ", Start);
        Assert.AreEqual("EUR", opportunity.Currency);
        Assert.AreEqual("Deal", opportunity.Name);
        Assert.ThrowsExactly<ArgumentException>(() => new Opportunity(Id, Id, Now, Id, "Deal", 10m, "E1R", Start));
    }

    [TestMethod]
    public void ManufacturingRequiresMaterialQuantitiesAndNonemptyOutput()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BillOfMaterialsLine(Id, Id, Now, Id, Id, 0m));
        Assert.ThrowsExactly<ArgumentException>(() => new ProductionOutput(Id, Id, Now, Id, Id, 0m, 0m));
        var scrapOnly = new ProductionOutput(Id, Id, Now, Id, Id, 0m, 2m);
        Assert.AreEqual(2m, scrapOnly.ScrapQuantity);
    }

    [TestMethod]
    public void ReplenishmentTargetCannotBeBelowTrigger()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ReplenishmentPolicy(Id, Id, Now, Id, Id, 10m, 5m, 2));
    }

    [TestMethod]
    public void WarehouseAllowsZeroCountButRejectsZeroPick()
    {
        var count = new CycleCountLine(Id, Id, Now, Id, Id, 0m);
        Assert.AreEqual(0m, count.CountedQuantity);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PickingTask(Id, Id, Now, Id, Id, Id, 0m));
    }

    [TestMethod]
    public void FulfillmentAndReturnsRequirePositiveQuantities()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new FulfillmentLine(Id, Id, Now, Id, Id, Id, -1m));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ReturnLine(Id, Id, Now, Id, Id, 0m));
    }

    [TestMethod]
    public void RevenueAdjustmentsAllowBothSignsButNotZero()
    {
        Assert.AreEqual(-10m, new RevenueAdjustment(Id, Id, Now, Id, -10m, "EUR", "Correction").Amount);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RevenueAdjustment(Id, Id, Now, Id, 0m, "EUR", "Correction"));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PerformanceObligation(Id, Id, Now, Id, "Service", -1m, "EUR"));
    }

    [TestMethod]
    public void BillingRejectsReversedPeriodsAndAllowsPrepayment()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new Subscription(Id, Id, Now, Id, Id, Start, Start.AddDays(-1), 1m));
        Assert.AreEqual(Start, new BillingCycle(Id, Id, Now, Id, Start, Start.AddMonths(1), Start).DueOn);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SubscriptionPlan(Id, Id, Now, "basic", "Basic", 0, 1m, "EUR"));
    }

    [TestMethod]
    public void LeaveRequiresOrderedDates()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new LeaveRequest(Id, Id, Now, Id, Start, Start.AddDays(-1), "Annual"));
    }

    [TestMethod]
    public void QualityBoundsAreOrderedButCanBeNegative()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new InspectionCriterion(Id, Id, Now, Id, "Temperature", 5m, -5m));
        Assert.AreEqual(-5m, new InspectionCriterion(Id, Id, Now, Id, "Temperature", -5m, 5m).Minimum);
    }

    [TestMethod]
    public void SlaResolutionCannotPrecedeResponse()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceLevelAgreement(Id, Id, Now, "Urgent", 60, 30));
    }

    [TestMethod]
    public void PromotionCannotExceedFullPriceAndPricesCannotBeNegative()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Promotion(Id, Id, Now, "sale", Start, Start, 101m));
        Assert.AreEqual(100m, new Promotion(Id, Id, Now, "free", Start, Start, 100m).DiscountPercent);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PriceListEntry(Id, Id, Now, Id, Id, -1m, "EUR"));
    }

    [TestMethod]
    public void ResourceAllocationRejectsReversedTimeAndExcessCapacity()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ResourceAllocation(Id, Id, Now, Id, Id, Now.AddHours(1), Now, 50m));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ResourceAllocation(Id, Id, Now, Id, Id, Now, Now.AddHours(1), 101m));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new TimesheetEntry(Id, Id, Now, Id, Id, Start, 25m));
    }

    [TestMethod]
    public void ContractRejectsUnknownKindAndReversedTerm()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BusinessContract(Id, Id, Now, Id, "C1", (ContractKind)999, Start, Start));
        Assert.ThrowsExactly<ArgumentException>(() => new BusinessContract(Id, Id, Now, Id, "C1", ContractKind.Customer, Start, Start.AddDays(-1)));
    }

    [TestMethod]
    public void AssetResidualCannotExceedCost()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new FixedAsset(Id, Id, Now, Id, "A1", "Machine", Start, 100m, 101m, "EUR"));
    }

    [TestMethod]
    public void PayslipHasConsistentNetPay()
    {
        Assert.AreEqual(800m, new Payslip(Id, Id, Now, Id, Id, 1000m, 200m, "EUR").NetPay);
        Assert.ThrowsExactly<ArgumentException>(() => new Payslip(Id, Id, Now, Id, Id, 1000m, 1001m, "EUR"));
    }

    [TestMethod]
    public void SelfDependenciesAndDelegationAreRejected()
    {
        var same = Id;
        Assert.ThrowsExactly<ArgumentException>(() => new CapabilityDependency(Id, Id, Now, same, same));
        Assert.ThrowsExactly<ArgumentException>(() => new ApprovalDelegation(Id, Id, Now, same, same, Start, Start));
    }

    [TestMethod]
    public void SavedViewRequiresJsonObject()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new SavedView(Id, Id, Now, Id, "invoices", "Mine", "not-json"));
        Assert.ThrowsExactly<ArgumentException>(() => new SavedView(Id, Id, Now, Id, "invoices", "Mine", "[]"));
        Assert.AreEqual("{}", new SavedView(Id, Id, Now, Id, "invoices", "Mine", "{}").DefinitionJson);
    }

    [TestMethod]
    public void InvitationsRequireHashedTokenAndFutureExpiry()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new OrganizationInvitation(Id, Id, Now, "a@example.test", Id, Now.AddDays(1), "raw-token"));
        Assert.ThrowsExactly<ArgumentException>(() => new OrganizationInvitation(Id, Id, Now, "a@example.test", Id, Now, new string('a', 64)));
    }
}
