# Current state — 2026-10-09

- Architecture: 36 modules, 186 projects; all target module assemblies remain scaffolds.
- VERIFY-001 COMPLETE: Server baseline 04e50ceb CI run 37860035722 passed restore, Release build (0 warnings/errors), 33 tests, EF snapshot validation and PostgreSQL migrations. Client run 37859970261 passed check/build.
- AUDIT-001 COMPLETE: see AUDIT.md for all 36 module statuses, actual legacy features, frontend gaps and prioritized risks.
- AUTH-001 implemented: bearer OnTokenValidated checks current user activity or integration-client/organization activity and scope validity. Regression tests added; execution pending at this checkpoint.
- Existing financial logic, migrations, module scaffolds and frontend source unchanged.
- Local .NET SDK 10.0.401 is now available under /tmp/fintrox-dotnet; NuGet restore is still pending. Do not confuse local SDK availability with successful package restore.
- Client still has 23 TypeScript module shells. No operational business screens exist.
- Next P0 items: refresh-token atomic rotation and webhook SSRF policy (see TASK_QUEUE.md).
