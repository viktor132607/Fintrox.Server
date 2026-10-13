# Organization owner concurrency safety — ORG-001

**Status: COMPLETE, CI VERIFIED — 2026-10-13.**

Operational implementation: Fintrox's existing (legacy) organization membership service. The new Organizations module Domain Entity definitions remain scaffold models; no target module EF migration, new APIs or database schema changes are implied.

## Why this was necessary

Previously, `OrganizationMemberService` checked `CountActiveOwnersAsync` and then modified an `OrganizationMembership` with a separate database write. Two concurrent owners could each observe two owners and each remove their own ownership, leaving a company with **zero active owners**, or could use stale elevated roles while changing memberships.

## Implemented mechanism

- `OrganizationMemberService.AddAsync`, `ChangeRoleAsync`, and `DeactivateAsync` now execute through the existing `ITransactionRunner` / `EfTransactionRunner`.
- At the **start** of each transaction, `IOrganizationMembershipRepository.LockOrganizationForMembershipChangeAsync` acquires a **PostgreSQL `FOR UPDATE` row lock on `core.organizations`** for the affected company. This lock is shared across server instances because it is enforced in the PostgreSQL database.
- After the lock is acquired, the service **rechecks the acting user's organization membership, active organization, role and permission**, and **reloads the target membership** inside the transaction. If removing/demoting an active Owner, it **recounts active owners under the same lock** before persisting.
- The row lock remains held until transaction commit/rollback, preventing two membership mutations in the same organization from passing the last-owner check simultaneously. Changes to different organizations do not share this organization-row lock.
- Additions and promotions also take the same lock, preventing concurrent membership changes from bypassing the serialized ownership policy and reducing stale-manager or duplicate-add races.
- The repository refuses to acquire the lock without an active transaction. Missing organization IDs fail closed with an organization-membership conflict.
- The existing transaction runner controls execution strategy retries and clears EF change tracker state after a failed transaction. The original migrations, financial data, tenant boundaries and API paths remain unchanged.

**Important guarantee:** this protects all membership **application service** entry points using the repository transaction pattern. Direct database writes, ad-hoc SQL or other code paths bypassing these APIs require separate database permissions/governance. It is not a claim of database-level enforcement against a privileged operator.

## PostgreSQL regression tests

`tests/Fintrox.Domain.Tests/Organizations/OrganizationOwnerConcurrencyTests.cs` adds 4 tests against **a disposable real PostgreSQL database**, one fresh database per test; the test connection is set by `FINTROX_TEST_POSTGRES`:

1. Two simultaneous Owner self-deactivations cannot leave no owners; one succeeds and one is rejected as the last-owner conflict.
2. Two simultaneous Owner-to-Administrator demotions cannot remove both owners.
3. Two owners simultaneously adding the same user results in exactly one membership/one success.
4. A second PostgreSQL transaction cannot acquire the organization-row lock until the first commits; invoking the repository without a transaction is rejected.

No mocked SQLite-in-memory concurrency behavior is used in the 4 new tests.

### Verification

GitHub Actions [run 38428892598](https://github.com/viktor132607/Fintrox.Server/actions/runs/38428892598) on source commit `8ec105e1cd1727ef38a0c9a081728da8f30d26d9`:
- .NET 10 Release build: **0 warnings, 0 errors**;
- **2,895 / 2,895 tests passed**, 0 failed, 0 skipped;
- EF model snapshot check passed;
- migrations applied and listed successfully on PostgreSQL.

Earlier CI iterations identified a test fake missing the new repository method and a manually initiated test transaction incompatible with `NpgsqlRetryingExecutionStrategy`; both were corrected. Production service transactions continue to use the existing EF execution strategy.

## Next queue item and boundaries

The next uncompleted foundation control in `docs/ai/TASK_QUEUE.md` is `TEST-001` (financial/tenant/HTTP regression coverage), with `INT-002` and `API-001` also pending.

This is a foundational identity/organization consistency gate for Fintrox as the [enterprise system of financial record](financial-system-of-record.md), not a replacement for accounting posting authorization, segregation of duties or complete tenant-scoped persistence.
