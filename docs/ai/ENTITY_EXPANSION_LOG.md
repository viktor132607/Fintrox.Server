# Entity expansion — 2026-10-13

Module-by-module Domain entity expansion, preserving existing 180 entities and the 32 legacy runtime types. Each module has its own atomic commit with inventory, code and documentation.

| Sequence | Module | Added | Module total | Verification | State |
|---:|---|---:|---:|---|---|
| 1 | Accounting | 30 | 35 | Static source/inventory validation; .NET CI pending | Domain models added; persistence/API pending |
| 2 | Audit | 15 | 17 | Structural/inventory checks; .NET CI pending | Domain models added; persistence/API pending |
| 3 | Banking | 22 | 26 | Structural/inventory checks; .NET CI pending | Domain models added; persistence/API pending |
| 4 | Budgeting | 16 | 20 | Inventory and structural checks; .NET CI pending | Domain expanded, EF/API pending |
| 5 | CRM | 22 | 28 | Inventory and structural checks; .NET CI pending | Domain expanded, EF/API pending |
