# Decisions

- 2026-10-09: Extend the actual 23-module baseline to 36, using Sales project XML verbatim with module-name substitution. No new packages or business implementation.
- Preserve .slnx; fix the stale .sln consumers rather than recreating a legacy solution.
- Extend existing module catalog, reserved capabilities, host references and Unit/Integration/Contract locations to satisfy repository architecture policy.
- Logical module collaborations are documented only; no cross-module project dependencies introduced.
- RevenueManagement's planned “Contracts” collaborator means ContractManagement (business module), not a separate 37th module or core Contracts assembly.
- New boundaries explicitly distinguish HR/Payroll, Inventory/Warehouse, Sales/Order orchestration, legal/revenue contracts and Projects/Resource timesheets.
- Frontend UI/13 new frontend shells are deferred; document the catalog-version gap. Runtime activation is not implemented by these manifests.
- Stop after architecture and commit, as directly requested for a model switch. Full audit and implementation remain queued.

- 2026-10-09 continuation: user authorized continuing queued work. VERIFY-001 can be completed using observed CI, not merely local tools.
- AUTH-001 repairs the active legacy bearer boundary in place. This is an explicit exception to the no-new-legacy-feature rule: moving authentication into scaffold modules during a security fix would expand regression risk. No module boundary references are changed.
- Bearer validation reads current state per request (no revocation cache). It rejects a whole integration token when any issued scope has been removed; added permissions require a newly issued token. Existing membership authorization and JWT signature/lifetime validation remain mandatory.
- No migrations/packages/new projects needed. The existing test project gains an Application reference for foundation regression coverage; project count stays 186.

- AUTH-002 keeps the existing schema and endpoints. PostgreSQL conditional updates serialize consumption; replacement insertion shares an explicit transaction inside the provider execution strategy. Failed attempts detach only their own replacement entity before retry. An ambiguous commit may require login again; replay never mints a second replacement.
- Revoking an already rotated token preserves its replacement link and does not revoke descendants. Broader session-family revocation is a separate policy change.
- Five relational tests reuse the existing test project with an Infrastructure reference (186 projects retained). They create isolated random databases and run against CI PostgreSQL; no in-memory concurrency substitute.

- 2026-10-11: explicit user request prioritizes adding entities across all modules; the former scaffold-only phase is complete. Add domain models now and keep per-module progress in MODULE_STATUS.md.
- Existing 32 legacy entity/persistence classes and their runtime consumers are preserved. New models fill missing concepts (e.g. quotations/orders rather than a duplicate invoice/posting engine). Future migration of existing types requires parity tests.
- No new project/shared kernel or cross-module dependencies. Each Domain has local OrganizationEntity/DomainRules; module entities are immutable constructor-validated models. Foreign IDs require application-level ownership/existence checks before persistence; constructor validation alone is not authorization.
- New types are not registered in the legacy DbContext. EF mapping, indexes, financial precision, database concurrency and application/API integration remain explicit ENTITIES-002 work. No payroll/tax/revenue legal calculation rules are invented.

## 2026-10-13 — FIN-SOR-001: all enterprise finance within Fintrox

User directive: all finance-related business processes and the complete money flow for the enterprise belong in one Fintrox ERP platform. Accounting owns the single GL/posting engine; source financial facts belong to specialized Fintrox modules and flow via versioned, tenant-authorized, idempotent contracts. External products are producers/consumers, not competing accounting systems. No new shared DbContext or cross-module direct database writes. Specification and acceptance gates: `docs/financial-system-of-record.md`. Status: architectural decision and ordered delivery plan; not fully implemented.
