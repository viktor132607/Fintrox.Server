# Validation history — 2026-10-09

## Current verified state
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
