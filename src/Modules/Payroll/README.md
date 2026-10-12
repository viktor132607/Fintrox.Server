# Payroll

Status: PARTIAL — Domain entity foundation implemented; persistence, application use cases and API pending.

Owns: Employees, PayRuns, Contributions, Localizations.
Reserved PostgreSQL schema: `payroll` (not created).
Existing implementation/reference: Not implemented; optional future module.

## Boundaries

- Domain: pure model, value objects, invariants and internal domain events.
- Application: feature-oriented use cases; ports in Abstractions. Commands/queries belong under their feature when implemented.
- Contracts: versioned DTOs and integration events; no entities, EF, ASP.NET or implementation dependencies.
- Infrastructure: module-owned context, configurations, migrations, messaging and external adapters.
- Presentation: thin HTTP endpoints and transport mapping; no persistence access.
- Composition: the host explicitly wires Infrastructure and Presentation when implementation is authorized.

Foreign data is referenced by identifiers/snapshots and accessed through approved Contracts.
No cross-module DbContext, repositories, entity navigations, SQL joins, foreign keys or shared transactions in the target architecture.
See [architecture](../../../docs/architecture.md) for the current legacy exceptions and migration gates.

## Implemented entity foundation

`EmployeeCompensation`, `PayrollPeriod`, `PayRun`, `Payslip`, `PayrollComponent`.

Models live in this module Domain/Entities directory; foreign objects are referenced by IDs. These are domain types, not registered database tables or endpoints. Existing legacy workflows remain in core projects. See [module progress](../../../docs/ai/MODULE_STATUS.md) for inventory and remaining work.

## Domain expansion — 2026-10-13

Added 22 records (27 module total): `PayrollEmployeeProfile`, `PayrollEmploymentAssignment`, `PayrollPayGroup`, `PayrollEarningType`, `PayrollDeductionType`, `PayrollBenefitType`, `PayrollPayItem`, `PayrollAdjustment`, `PayrollOvertimeRecord`, `PayrollLeavePayment`, `PayrollTaxRule`, `PayrollTaxWithholding`, `PayrollContributionRule`, `PayrollContributionLine`, `PayrollPaymentInstruction`, `PayrollDisbursement`, `PayrollAccrual`, `PayrollPostingBatch`, `PayrollYearEndStatement`, `PayrollLocalizationConfig`, `PayrollCorrection`, `PayrollApproval`.

Pure model definitions; no runtime payroll/payment/project transactions were introduced. EF mappings, security, services, endpoints and tests remain separate tasks.
