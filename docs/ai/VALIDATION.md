# Validation history — 2026-10-09

## Current verified state
Code commit: 79ee090e2e50ff64d3b3fb11d01e7fda31eb5b50.
CI run 37900969688, job 113723180801: restore, Release build with warnings as errors (0 warnings/errors), 46/46 tests, EF model snapshot and PostgreSQL migration application/listing all PASSED.
Local architecture check, package restore, full Release build (0 warnings/errors) and 46/46 tests PASSED with SDK 10.0.401.
No schema migration added. No manual HTTP/end-to-end or production verification claimed.
The historical scaffold-only NOT RUN entries below are superseded by this CI evidence.

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
