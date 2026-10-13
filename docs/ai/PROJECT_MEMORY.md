# Fintrox project memory

## Vision and stack
Modular ERP/accounting platform informed by SAP, Oracle NetSuite, Dynamics 365, Odoo, Zero and Xero.
Server: .NET 10 / C# 14, ASP.NET Core Web API, EF Core, PostgreSQL.
Client: Next.js + strict TypeScript; never replace with JavaScript.
Repositories: viktor132607/Fintrox.Server and viktor132607/Fintrox.Client; branch main.

## Architecture
Modular monolith with Clean Architecture/DDD boundaries, future independent extraction.
36 modules, five layers each: Domain, Application, Contracts, Infrastructure, Presentation.
186 projects: 180 module projects + 5 legacy/core projects + 1 test project.
Fintrox.Server.slnx is authoritative; preserve it. Catalog: architecture/modules.json.
Existing runtime business implementation remains in legacy horizontal projects. ENTITIES-001 adds 180 domain entity types across all 36 module Domain assemblies; other module layers remain scaffolds. These entities have not yet been mapped into EF or connected to endpoints.
Do not mistake README descriptions, reserved capabilities or empty test folders for implemented features.

## Rules
- Preserve existing behavior, migrations and project paths; migrate one capability with parity tests.
- Domain/Contracts have no references; Application uses own Domain/Contracts; Infrastructure uses own Application/Domain/Contracts; Presentation uses own Application/Contracts.
- Cross-module contracts require explicit review; no cross-module entity/DbContext references.
- Reference host Infrastructure/Presentation assemblies without registering nonexistent services.
- Money is decimal; posted journals balance, posting is atomic/idempotent, corrections use reversals.
- Enforce organization membership and tenant isolation on the server; UI modes never grant permissions.
- Simple/accountant/expert layouts are customizable; activation/preferences are still planned behavior.
- Do not generate images, secrets, speculative infrastructure or unrelated refactors.
- Commit authorized changes to main; never claim unexecuted tests/builds/deployments.

## Continuity
Read SESSION_HANDOFF.md, CURRENT_STATE.md and TASK_QUEUE.md first.
Read MASTER_PROMPT.md only for relevant detailed requirements; MASTER_ROADMAP.md indexes phases.
Reuse verified findings and inspect changed files only until the explicitly scheduled full audit.
Update concise project memory after each milestone; continue from the next verified foundation task; the user authorized continuation after the model switch.

User direction 2026-10-11: prioritize adding module entities and record per-module progress in MODULE_STATUS.md instead of repeating inventories in chat. Security backlog remains tracked; do not silently mark it complete.

## 2026-10-13 — Mandatory financial source of truth and first foundation gate
- User requires *all* enterprise financial management and money flows (AP, AR, GL, billing, bank/cash, taxes, payroll, assets, cost accounting, forecasts, consolidation and reports) to reside in Fintrox. Connected user projects may hold operational data but must submit/receive finance through Fintrox integrations. **Accounting is single GL owner**, modules own source documents; no second financial ledger outside Fintrox. See `docs/financial-system-of-record.md` and MASTER_PROMPT.md.
- First stabilization milestone ORG-002: the operational legacy OrganizationAccessService checks both active membership and organization activity; inactive tenant is denied all business permissions; only active owner can use new `organizations.reactivate` for reactivation. 7 regression tests and .NET CI run 38329276594 passed (2,884 tests, 0 failed/skipped), EF snapshot and PostgreSQL migrations. Existing modular Domain 936 entity inventory unchanged.
- Next: implement operational Identity/Organizations migration and Accounting source-to-posting capabilities one verified slice at a time; preserve previous legacy financial behavior until parity tests pass.

## 2026-10-13 — INT-001 webhook outbound SSRF protection verified
- In live legacy integrations code: `WebhookTargetPolicy` in Domain, `WebhookOutboundTransport` in Infrastructure, named client handler wired in API, worker validates persisted unsafe URLs and permanently fails blocked targets. IP checks permit global unicast, reject private/local/special-use/mapped/6to4 destinations; DNS must return entirely public results, and sockets connect directly to checked IP. Automatic redirect, proxies and cookies disabled. Seven new deterministic DNS/network-policy tests. CI 38399255676 2,891 passed; 0 warnings/errors; EF/Postgres passed. No new DB migration, target Integrations module still not fully operational. Docs: `docs/webhook-security.md`. Next ORG-001.

## 2026-10-13 — ORG-001 concurrent last-owner protection verified
- Existing legacy membership operations `OrganizationMemberService.AddAsync/ChangeRoleAsync/DeactivateAsync` use `ITransactionRunner` + `IOrganizationMembershipRepository.LockOrganizationForMembershipChangeAsync`: transaction locks `core.organizations` via `FOR UPDATE` (DB-level, cross-instance), authorization/target/owner-count are rechecked under lock; SaveChanges commits or rolls back together. 4 new real PostgreSQL concurrency tests; CI 38428892598 passed 2,895 tests, zero warnings/errors, EF snapshot/migrations. No new schema or frontend changes. `docs/organization-owner-concurrency.md`. Next queue: TEST-001; INT-002/API-001 pending.
