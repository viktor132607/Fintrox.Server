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
