# Validation history — 2026-10-13

## ENTITIES-EXPANSION (2026-10-13)
- First expanded-head CI (38323676459) identified reserved C# keyword used as constructor parameter in Capabilities/CapabilityConstraint.Operator; fixed with escaped @operator in the next commit. Other generated identifiers were scanned against reserved C# keywords. New CI/build result pending.
- All 36 modules expanded by 756 additional Domain entity classes: 936 total inventoried module model types; the 32 legacy core persistence types remain unchanged.
- Recursive GitHub tree verified 936 distinct entity model source paths, all matched by the machine inventory; tree result was not truncated.
- Verified 36 sequential per-module progress records and 36 module-status rows.
- Expanded the inventory checker to verify field-property names/types and fixed dynamic reflection test samples for nationality and supply-country codes.
- No local .NET SDK: build, tests and GitHub CI are not yet confirmed for this expansion. Previous 604 local passing tests below predate the new models.
- Domain definitions only. EF mappings, migrations, authorization, application services, APIs and UI remain pending.


## ENTITIES-001 current validation
- Static architecture and full 180-entity/36-module inventory checks PASSED.
- Local Release build of the affected test/dependency graph with warnings-as-errors PASSED. 604 tests passed; 5 PostgreSQL tests explicitly skipped locally (no server). CI full solution and database verification pending.
- New regression coverage: 540 entity identity/scope/date cases plus 18 business-boundary tests. No module persistence or API integration claimed.

## Previous verified state
Code commit: 422cc38dd2a20200ca3a5e1ee91a8c6c78d876dd (AUTH-002).
CI run 38012762635, job 114096261806: restore, Release build with warnings as errors (0 warnings/errors), 51/51 tests (0 skipped), EF snapshot and PostgreSQL migration application/listing PASSED.
Five PostgreSQL tests cover simultaneous refresh, revoke winning during refresh read, revocation after rotation, failed replacement-save rollback/same-context retry, and session ownership/revocation.
Local affected-project Release build and 46 unit tests passed; five relational tests explicitly skipped locally (PostgreSQL unavailable), then all five passed in CI.
Architecture check passes. No new migration/project or production verification.
Previous AUTH-001 code 79ee090 also passed full build, 46/46 tests and migrations (CI 37900969688).
Historical scaffold-only NOT RUN entries below are superseded by CI evidence.

## Initial scaffold checks (historical)

- PASS: 36 module directories, 180 module projects, 5 core projects, 1 test project.
- PASS: 186 unique .slnx paths; every project exists; original 121 retained.
- PASS: every original solution folder/registration preserved unchanged.
- PASS: all original project XML unchanged except 26 additive API host references.
- PASS: 13 modules each contain five projects, requested Application features, README and tracked placeholder directories.
- PASS: dependency graph, missing targets, cycles, layer boundaries, catalog/capability coverage and host composition checked by scripts/check_architecture.py.
- PASS: negative verification rejects duplicate solution registrations and forbidden/cyclic project references.
- PASS: no new .cs business files, services, entities, DTOs, endpoints or migrations.
- NOT RUN: dotnet restore Fintrox.Server.slnx — dotnet executable not installed.
- NOT RUN: dotnet build Fintrox.Server.slnx --no-restore — same SDK blocker.
- NOT RUN: dotnet test Fintrox.Server.slnx --no-restore — same SDK blocker.
- NOT RUN: Visual Studio loading, database migration execution, deployment, production verification.

Commands for an SDK-capable environment:
```bash
python3 scripts/check_architecture.py
dotnet restore Fintrox.Server.slnx
dotnet build Fintrox.Server.slnx --no-restore
dotnet test Fintrox.Server.slnx --no-restore
```
SDK requirement: global.json requests 10.0.401 with latestFeature roll-forward.
Post-publication verification and actual commit SHA are recorded in SESSION_HANDOFF.md.

## Published commit verification
Architecture commit: c3f0ebfb1f2f4dc547cbaa1286bee61ae21de395 (main updated with expected-head check).
All 362 written files verified against GitHub Git blob hashes; all untouched baseline files retain their original hashes.
186 .csproj files confirmed in the published tree. No runtime verification performed.
