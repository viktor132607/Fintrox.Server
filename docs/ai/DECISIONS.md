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
