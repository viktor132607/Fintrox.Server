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

## 2026-10-13 — INT-001 outbound webhook security boundary

Accept **only publicly routable HTTP(S) webhook targets**. Validate at subscription registration/update and again at delivery. New outbound connections resolve each hostname and pin a validated public IP directly to the socket, disallowing DNS rebinding and mixed public/private answers; never follow redirects or use environment proxies/cookies. Existing legacy subscriptions must not bypass validation. Blocked destinations fail permanently, not as retryable network faults. Verified in CI 38399255676 (2,891 tests, 0 failed/skipped). See `docs/webhook-security.md`. INT-002 (durable multi-worker delivery) remains separate.

## 2026-10-13 — ORG-001 transactional owner protection

All member-creation, role-update and deactivation operations on an organization must take a PostgreSQL `FOR UPDATE` lock on `core.organizations` and revalidate acting user permissions, target membership and owner count **within the same transaction**; the lock must remain until commit/rollback. Use existing `EfTransactionRunner` execution strategy. Single-row locking, not a process-local mutex, protects simultaneous API requests across instances. Implemented in legacy Organizations runtime; 4 real PostgreSQL tests, CI 38428892598 passed 2,895 tests and EF/PostgreSQL checks. See `docs/organization-owner-concurrency.md`.

## 2026-10-13 — TEST-001 full-stack financial invariants

Financial test acceptance uses migrated disposable PostgreSQL and **actual ASP.NET Core HTTP endpoints**, with tenant-aware real JWT and direct DB verification. Cover posting/reversal, debit=credit, closed periods, cross-tenant access, authentication and immutability triggers before calling finance capabilities production-ready. Seven initial scenarios are verified in CI 38437997153 (2,902 passed, 0 failures/skips, EF/PostgreSQL passed). The tests found positional DTO DataAnnotations incorrectly targeted to properties in 44 request records: move validation to `[param:]`, preserving JSON fields. See `docs/financial-http-postgres-regression.md`. Existing legacy Accounting remains runtime authoritative; migration is pending.
