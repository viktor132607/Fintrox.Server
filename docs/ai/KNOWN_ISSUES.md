# Known issues

| Problem | Impact | Affected Module | Reproduction | Proposed Resolution | Status |
|---|---|---|---|---|---|
| No .NET executable in this work environment | No restore/build/test proof | All | dotnet --info → command not found | Run .NET 10.0.401 checks locally or inspect CI | BLOCKED |
| Client still has original 23-module design snapshot | Frontend does not expose new 13 modules | Client | Compare Client catalog with Server 36-module catalog | Expand frontend in its authorized milestone, then synchronize manifests | PENDING |
| Legacy horizontal projects own shared persistence/business logic | Target modular isolation not implemented at runtime | Legacy/core | See docs/architecture.md, Existing coupling | Audit, characterize behavior, migrate one capability at a time | PENDING |

No runtime claims have been made; other business gaps await AUDIT-001.
