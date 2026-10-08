# Decisions

- 2026-10-09: Extend the actual 23-module baseline to 36, using Sales project XML verbatim with module-name substitution. No new packages or business implementation.
- Preserve .slnx; fix the stale .sln consumers rather than recreating a legacy solution.
- Extend existing module catalog, reserved capabilities, host references and Unit/Integration/Contract locations to satisfy repository architecture policy.
- Logical module collaborations are documented only; no cross-module project dependencies introduced.
- RevenueManagement's planned “Contracts” collaborator means ContractManagement (business module), not a separate 37th module or core Contracts assembly.
- New boundaries explicitly distinguish HR/Payroll, Inventory/Warehouse, Sales/Order orchestration, legal/revenue contracts and Projects/Resource timesheets.
- Frontend UI/13 new frontend shells are deferred; document the catalog-version gap. Runtime activation is not implemented by these manifests.
- Stop after architecture and commit, as directly requested for a model switch. Full audit and implementation remain queued.
