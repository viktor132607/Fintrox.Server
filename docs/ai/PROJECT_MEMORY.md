# Fintrox project memory

## Vision and stack
Modular ERP/accounting platform informed by SAP, Oracle NetSuite, Dynamics 365, Odoo, Zero and Xero.
Server: .NET 10 / C# 14, ASP.NET Core Web API, EF Core, PostgreSQL.
Client: Next.js + strict TypeScript; never replace with JavaScript.
Repositories: viktor132607/Fintrox.Server and viktor132607/Fintrox.Client; branch main.

## Architecture
Modular monolith with Clean Architecture/DDD boundaries, future independent extraction.
36 modules, five layers each: Domain, Application, Contracts, Infrastructure, Presentation.
186 projects: 180 module projects + 5 legacy/core projects + 1 test project.
Fintrox.Server.slnx is authoritative; preserve it. Catalog: architecture/modules.json.
Business implementation still lives in legacy horizontal projects; module assemblies are scaffolds.
Do not mistake README descriptions, reserved capabilities or empty test folders for implemented features.

## Rules
- Preserve existing behavior, migrations and project paths; migrate one capability with parity tests.
- Domain/Contracts have no references; Application uses own Domain/Contracts; Infrastructure uses own Application/Domain/Contracts; Presentation uses own Application/Contracts.
- Cross-module contracts require explicit review; no cross-module entity/DbContext references.
- Reference host Infrastructure/Presentation assemblies without registering nonexistent services.
- Money is decimal; posted journals balance, posting is atomic/idempotent, corrections use reversals.
- Enforce organization membership and tenant isolation on the server; UI modes never grant permissions.
- Simple/accountant/expert layouts are customizable; activation/preferences are still planned behavior.
- Do not generate images, secrets, speculative infrastructure or unrelated refactors.
- Commit authorized changes to main; never claim unexecuted tests/builds/deployments.

## Continuity
Read SESSION_HANDOFF.md, CURRENT_STATE.md and TASK_QUEUE.md first.
Read MASTER_PROMPT.md only for relevant detailed requirements; MASTER_ROADMAP.md indexes phases.
Reuse verified findings and inspect changed files only until the explicitly scheduled full audit.
Update concise project memory after each milestone; continue from the next verified foundation task; the user authorized continuation after the model switch.
