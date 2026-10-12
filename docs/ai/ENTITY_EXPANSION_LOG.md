# Entity expansion — 2026-10-13

Module-by-module Domain entity expansion, preserving existing 180 entities and the 32 legacy runtime types. Each module has its own atomic commit with inventory, code and documentation.

| Sequence | Module | Added | Module total | Verification | State |
|---:|---|---:|---:|---|---|
| 1 | Accounting | 30 | 35 | Static source/inventory validation; .NET CI pending | Domain models added; persistence/API pending |
| 2 | Audit | 15 | 17 | Structural/inventory checks; .NET CI pending | Domain models added; persistence/API pending |
| 3 | Banking | 22 | 26 | Structural/inventory checks; .NET CI pending | Domain models added; persistence/API pending |
| 4 | Budgeting | 16 | 20 | Inventory and structural checks; .NET CI pending | Domain expanded, EF/API pending |
| 5 | CRM | 22 | 28 | Inventory and structural checks; .NET CI pending | Domain expanded, EF/API pending |
| 6 | Capabilities | 15 | 20 | Inventory and structural checks; .NET CI pending | Domain expanded, EF/API pending |
| 7 | Commerce | 22 | 29 | Static structure / inventory; .NET CI pending | Domain expanded; EF/API pending |
| 8 | Consolidation | 18 | 22 | Static structure / inventory; .NET CI pending | Domain expanded; EF/API pending |
| 9 | ContractManagement | 18 | 23 | Static structure / inventory; .NET CI pending | Domain expanded; EF/API pending |
| 10 | Counterparties | 18 | 23 | Structural inventory; no .NET CI yet | Domain expanded; EF/API pending |
| 11 | Currencies | 14 | 17 | Structural inventory; no .NET CI yet | Domain expanded; EF/API pending |
| 12 | Documents | 20 | 24 | Structural inventory; no .NET CI yet | Domain expanded; EF/API pending |
| 13 | Experience | 18 | 23 | Static inventory; .NET CI pending | Domain expanded; EF/API pending |
| 14 | FixedAssets | 20 | 24 | Static inventory; .NET CI pending | Domain expanded; EF/API pending |
| 15 | HumanResources | 22 | 28 | Static inventory; .NET CI pending | Domain expanded; EF/API pending |
| 16 | Identity | 22 | 27 | Structure/inventory checked; .NET CI pending | Domain expanded; EF/API pending |
| 17 | Integrations | 24 | 28 | Structure/inventory checked; .NET CI pending | Domain expanded; EF/API pending |
| 18 | Inventory | 24 | 32 | Structure/inventory checked; .NET CI pending | Domain expanded; EF/API pending |
| 19 | Manufacturing | 24 | 31 | Structural inventory; CI not rerun | Domain expanded; EF/API pending |
| 20 | OrderManagement | 20 | 25 | Structural inventory; CI not rerun | Domain expanded; EF/API pending |
| 21 | Organizations | 18 | 23 | Structural inventory; CI not rerun | Domain expanded; EF/API pending |
| 22 | Payments | 21 | 23 | Inventory checked; CI pending | Domain expanded; EF/API pending |
| 23 | Payroll | 22 | 27 | Inventory checked; CI pending | Domain expanded; EF/API pending |
| 24 | Projects | 20 | 25 | Inventory checked; CI pending | Domain expanded; EF/API pending |
| 25 | Purchases | 23 | 29 | Inventory check; .NET CI pending | Domain expanded; EF/API pending |
| 26 | QualityManagement | 21 | 28 | Inventory check; .NET CI pending | Domain expanded; EF/API pending |
| 27 | Reporting | 23 | 27 | Inventory check; .NET CI pending | Domain expanded; EF/API pending |
| 28 | ResourceManagement | 20 | 26 | Static inventory only; CI pending | Domain expanded; EF/API pending |
| 29 | RevenueManagement | 20 | 25 | Static inventory only; CI pending | Domain expanded; EF/API pending |
| 30 | Sales | 22 | 27 | Static inventory only; CI pending | Domain expanded; EF/API pending |
| 31 | ServiceManagement | 21 | 25 | Source structure/inventory; .NET CI pending | Domain expanded; EF/API pending |
| 32 | SubscriptionBilling | 23 | 28 | Source structure/inventory; .NET CI pending | Domain expanded; EF/API pending |
| 33 | SupplyChain | 21 | 26 | Source structure/inventory; .NET CI pending | Domain expanded; EF/API pending |
| 34 | Tax | 26 | 31 | Source and inventory only; full .NET CI pending | Domain expanded; EF/API pending |
| 35 | WarehouseManagement | 26 | 34 | Source and inventory only; full .NET CI pending | Domain expanded; EF/API pending |
| 36 | Workflows | 25 | 30 | Source and inventory only; full .NET CI pending | Domain expanded; EF/API pending |

## Completion and verification status

- 36/36 module updates committed separately: 756 added C# Domain entity types; 936 module-owned Entity classes in total (initial 180 included).
- GitHub recursive main tree has 936 matching unique model source files; architecture/domain-entities.json contains those 936 paths and all 36 module names.
- ENTITY_EXPANSION_LOG.md contains exactly 36 ordered module entries, with a corresponding row in MODULE_STATUS.md for each.
- .NET build/test/CI verification pending; no EF tables, module use cases, endpoints or frontend screens were added.

CI feedback: after the 36 module commits, five generated entity names ending Attribute/Exception were renamed to satisfy CA1711. Model counts and module ownership are unchanged. See VALIDATION.md.
