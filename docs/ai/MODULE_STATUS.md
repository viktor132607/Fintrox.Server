# Module status — 2026-10-11

ENTITIES-001: 180 new entity classes across all 36 module Domain projects. Domain foundation implemented; validation results in VALIDATION.md. Existing 32 legacy entity/persistence classes remain in place, without duplication or migration of their runtime behavior.

The new types have immutable IDs/organization ownership, constructor validation and typed business fields. They are not yet mapped in EF or exposed through APIs. No complete ERP workflow, database tenant isolation, authorization or cross-reference existence checks are claimed by these model classes.

Every module keeps five projects. Domain=PARTIAL; Application/Contracts/Infrastructure/Presentation=SCAFFOLD. Frontend remains 23 shells (13 new module shells missing). Entity constructors do not activate a module.

| Module | New entity classes | Count | Domain | Persistence / API |
|---|---|---:|---|---|
| Accounting | `AccountGroup`, `AccountingDimension`, `AccountingDimensionValue`, `AccountingBook`, `RecurringJournal` | 5 | PARTIAL | PENDING |
| Audit | `AuditRetentionPolicy`, `AuditExportRequest` | 2 | PARTIAL | PENDING |
| Banking | `BankAccount`, `BankStatement`, `BankStatementLine`, `BankReconciliationMatch` | 4 | PARTIAL | PENDING |
| Budgeting | `Budget`, `BudgetVersion`, `BudgetLine`, `Forecast` | 4 | PARTIAL | PENDING |
| CRM | `Lead`, `Opportunity`, `CrmActivity`, `SalesPipeline`, `PipelineStage`, `MarketingCampaign` | 6 | PARTIAL | PENDING |
| Capabilities | `CapabilityDefinition`, `CapabilityDependency`, `OrganizationModuleInstallation`, `OrganizationCapabilityActivation`, `UserCapabilityEntitlement` | 5 | PARTIAL | PENDING |
| Commerce | `CatalogListing`, `Storefront`, `PriceList`, `PriceListEntry`, `Promotion`, `CommerceOrder`, `CommerceOrderLine` | 7 | PARTIAL | PENDING |
| Consolidation | `CompanyGroup`, `GroupCompany`, `ConsolidationPeriod`, `EliminationEntry` | 4 | PARTIAL | PENDING |
| ContractManagement | `BusinessContract`, `ContractVersion`, `ContractObligation`, `ContractMilestone`, `ContractRenewal` | 5 | PARTIAL | PENDING |
| Counterparties | `CounterpartyContact`, `CounterpartyAddress`, `CounterpartyBankAccount`, `PaymentTerm`, `CounterpartyGroup` | 5 | PARTIAL | PENDING |
| Currencies | `ExchangeRateProvider`, `CurrencyRoundingPolicy`, `CurrencyRevaluation` | 3 | PARTIAL | PENDING |
| Documents | `StoredDocument`, `DocumentVersion`, `DocumentLink`, `DocumentTemplate` | 4 | PARTIAL | PENDING |
| Experience | `ExperienceProfile`, `UserExperiencePreference`, `DashboardWidget`, `SavedView`, `NavigationPreference` | 5 | PARTIAL | PENDING |
| FixedAssets | `AssetCategory`, `FixedAsset`, `DepreciationScheduleLine`, `AssetDisposal` | 4 | PARTIAL | PENDING |
| HumanResources | `Employee`, `JobOpening`, `JobApplication`, `OnboardingTask`, `LeaveRequest`, `PerformanceReview` | 6 | PARTIAL | PENDING |
| Identity | `OrganizationInvitation`, `RoleDefinition`, `PermissionDefinition`, `RolePermissionGrant`, `ServiceAccount` | 5 | PARTIAL | PENDING |
| Integrations | `AppRegistration`, `AppVersion`, `AppInstallation`, `ExternalReferenceMapping` | 4 | PARTIAL | PENDING |
| Inventory | `InventoryItem`, `UnitOfMeasure`, `ItemCategory`, `Warehouse`, `StockMovement`, `StockReservation`, `InventoryLot`, `InventorySerial` | 8 | PARTIAL | PENDING |
| Manufacturing | `BillOfMaterials`, `BillOfMaterialsLine`, `ProductionRouting`, `RoutingOperation`, `WorkCenter`, `WorkOrder`, `ProductionOutput` | 7 | PARTIAL | PENDING |
| OrderManagement | `FulfillmentOrder`, `FulfillmentLine`, `Backorder`, `ReturnAuthorization`, `ReturnLine` | 5 | PARTIAL | PENDING |
| Organizations | `Branch`, `Department`, `BusinessUnit`, `CompanyAccountingPolicy`, `TaxRegistration` | 5 | PARTIAL | PENDING |
| Payments | `PaymentRefund`, `PaymentAdvance` | 2 | PARTIAL | PENDING |
| Payroll | `EmployeeCompensation`, `PayrollPeriod`, `PayRun`, `Payslip`, `PayrollComponent` | 5 | PARTIAL | PENDING |
| Projects | `Project`, `ProjectTask`, `ProjectExpense`, `ProjectMilestone`, `CostCenter` | 5 | PARTIAL | PENDING |
| Purchases | `PurchaseRequisition`, `PurchaseRequisitionLine`, `PurchaseOrder`, `PurchaseOrderLine`, `GoodsReceipt`, `GoodsReceiptLine` | 6 | PARTIAL | PENDING |
| QualityManagement | `InspectionPlan`, `InspectionCriterion`, `QualityInspection`, `InspectionResult`, `Nonconformity`, `CorrectiveAction`, `QualityHold` | 7 | PARTIAL | PENDING |
| Reporting | `ReportDefinition`, `SavedSearch`, `ReportSchedule`, `KpiDefinition` | 4 | PARTIAL | PENDING |
| ResourceManagement | `Resource`, `ResourceSkill`, `ResourceAllocation`, `ResourceAvailability`, `Timesheet`, `TimesheetEntry` | 6 | PARTIAL | PENDING |
| RevenueManagement | `RevenueRecognitionRule`, `RevenueContract`, `PerformanceObligation`, `RevenueScheduleLine`, `RevenueAdjustment` | 5 | PARTIAL | PENDING |
| Sales | `SalesQuotation`, `SalesQuotationLine`, `SalesOrder`, `SalesOrderLine`, `SalesCreditNote` | 5 | PARTIAL | PENDING |
| ServiceManagement | `SupportTicket`, `ServiceLevelAgreement`, `FieldServiceOrder`, `MaintenancePlan` | 4 | PARTIAL | PENDING |
| SubscriptionBilling | `SubscriptionPlan`, `Subscription`, `BillingCycle`, `UsageRecord`, `SubscriptionRenewal` | 5 | PARTIAL | PENDING |
| SupplyChain | `DemandForecast`, `SupplyPlan`, `ProcurementPlan`, `ReplenishmentPolicy`, `SupplierCommitment` | 5 | PARTIAL | PENDING |
| Tax | `TaxJurisdiction`, `TaxCategory`, `TaxRate`, `VatReturnPeriod`, `VatRegisterEntry` | 5 | PARTIAL | PENDING |
| WarehouseManagement | `BinLocation`, `ReceivingTask`, `PutawayTask`, `PickingTask`, `Shipment`, `ShipmentPackage`, `CycleCount`, `CycleCountLine` | 8 | PARTIAL | PENDING |
| Workflows | `WorkflowDefinition`, `ApprovalStep`, `WorkflowInstance`, `ApprovalDecision`, `ApprovalDelegation` | 5 | PARTIAL | PENDING |

Machine-readable inventory: `architecture/domain-entities.json`; completeness gate: `scripts/check_domain_entities.py`.
Tests: ModuleEntityContractTests exercises each entity's identity, scope immutability, empty references and missing dates; ModuleBusinessInvariantTests covers 18 business boundary scenarios. Actual run results are in VALIDATION.md.

Remaining for each module: aggregate relationships and lifecycle use cases, authorization, cross-reference/tenant checks, versioned contracts, EF mappings/precision/indexes/concurrency, migrations, repositories, endpoints, integration tests and UI. Master prompt feature coverage is broader than the first entity foundation; do not mark entire module COMPLETE.
