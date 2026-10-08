# Repository map

- Fintrox.Server.slnx: 186 project registrations under Core/Modules/Tests.
- Directory.Build.props / global.json: net10.0/C#14 defaults and SDK selection.
- src/Fintrox.Api: composition root, legacy endpoints and module assembly references.
- src/Fintrox.{Domain,Application,Contracts,Infrastructure}: existing horizontal implementation; preserve during migration.
- src/Modules/<Module>: five-layer scaffold per architecture/modules.json.
- architecture/modules.json: 36-module ownership/features/schema catalog.
- architecture/experience-integration.json: reserved capabilities and future profiles/integrations; no runtime activation.
- tests/Fintrox.Domain.Tests: existing test project.
- tests/Modules/<Module>/{Unit,Integration,Contract}: reserved test locations.
- scripts/check_architecture.py: solution/project graph, module boundaries, catalog/manifest checks.
- .github/workflows/ci-cd.yml: architecture/build/tests/migrations/container checks.
- docs/architecture.md: boundary policy and legacy migration constraints.
- docs/ai: session memory, full MASTER_PROMPT.md, phase index and validation.
- Client repo apps/web: Next.js/TypeScript app; 23 existing module shells.
- Client repo docs/ai: frontend status and pointer to authoritative Server memory.
