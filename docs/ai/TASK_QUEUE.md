# Task queue

| Task ID | Title | Module | Priority | Dependencies | Status | Acceptance Criteria |
|---|---|---|---|---|---|---|
| ARCH-001 | Scaffold 13 additional ERP modules | Platform | P0 | Baseline inspection | COMPLETE | 36 modules; 186 unique projects; original 121 preserved; static architecture passes; GitHub commit |
| VERIFY-001 | Restore, build and test with .NET SDK | Platform | P0 | ARCH-001; SDK 10.0.401 | BLOCKED | All three .slnx commands pass; actual results and SHA recorded |
| AUDIT-001 | Audit existing Server and Client implementation | All | P0 | VERIFY-001 or documented blocker | PENDING | Per-feature real status; identify first incomplete foundation; preserve existing behavior |
| FOUNDATION-001 | Stabilize foundation | Identity/Organizations/Platform | P0 | AUDIT-001 | PENDING | Scope from audit; security/tenancy and regression checks pass |
| ACCOUNTING-001 | Complete accounting foundation | Accounting/Tax/Currencies/Counterparties | P1 | FOUNDATION-001 | PENDING | Audited gaps implemented with financial invariants and meaningful tests |
| ERP-001 | Implement operational and new modules incrementally | Business modules | P2 | Financial foundation; per-module collaborators | PENDING | Contracts/domain/persistence/API/auth/tests complete per use case |
| FE-001 | Expand frontend module shells and align manifests | Client | P2 | Architecture; frontend scope | PENDING | 36 TypeScript module shells; manifests aligned; architecture/typecheck/build pass |
| ENTERPRISE-001 | Enterprise capabilities and hardening | All | P3 | Core ERP | PENDING | See MASTER_ROADMAP phases 9–10 |

No business implementation was authorized in this turn beyond scaffolding. Await the user's model switch.
