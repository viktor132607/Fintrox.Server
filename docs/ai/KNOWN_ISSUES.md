# Known issues — 2026-10-09

| ID | Problem | Impact | Evidence / reproduction | Proposed resolution | Status |
|---|---|---|---|---|---|
| AUTH-001 | Issued tokens outlive actor/scope deactivation | Revoked actors retain temporary access | Baseline bearer handler checks only signature/lifetime | Current-state validator + 13 regression tests, validated at 79ee090 | RESOLVED |
| AUTH-002 | Refresh read/update lacks atomic consumption | Concurrent refresh can create two replacements | AuthenticationService.RefreshAsync + RefreshTokenConfiguration | 422cc38: conditional transaction and 5 PostgreSQL tests; CI 51/51 passed | RESOLVED |
| INT-001 | Webhook URL accepts arbitrary HTTP(S) destinations | Requests may target internal resources | Domain/Integrations/WebhookSubscription.NormalizeUrl; worker default HTTP client | DNS/connect/redirect-safe destination policy | PENDING |
| ORG-001 | Last-owner count and update not serialized | Concurrent demotions can remove every owner | OrganizationMemberService.EnsureNotLastOwnerAsync | Serialize and test concurrent changes | PENDING |
| ORG-002 | General organization permissions do not check organization activity | Inactive tenant may still accept business requests | OrganizationAccessService checks active membership only | Define lifecycle policy preserving reactivation; add use-case tests | PENDING |
| INT-002 | Webhook processing lacks lease recovery | Concurrent duplicate sends / crash-stuck attempts | WebhookDeliveryWorker.ProcessDeliveryAsync | Atomic claim and expired lease recovery | PENDING |
| FE-001 | Client remains 23 shells | 13 new module UI boundaries absent | Compare both catalogs | Expand/synchronize in frontend milestone | PENDING |
| ARCH-LEGACY | Shared legacy DbContext/business layers | Target runtime modular isolation incomplete | docs/architecture.md | Characterize and migrate one capability at a time | PENDING |

Previous missing-SDK blocker resolved: SDK 10.0.401 installed locally; baseline build/test/migration CI also confirmed. Do not treat 33 baseline tests as comprehensive financial/security coverage. See AUDIT.md.
