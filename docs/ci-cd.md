# CI/CD

## Continuous Integration

The `CI/CD` GitHub Actions workflow runs on every pull request targeting `main`, every push to `main`, and manual dispatch.

The CI gate performs:

1. repository checkout;
2. .NET SDK setup from `global.json`;
3. repository-local tool restore;
4. solution restore;
5. Release build with warnings treated as errors;
6. execution of every `tests/**/*Tests.csproj` project when test projects exist;
7. PostgreSQL 18.6 service startup;
8. EF migration snapshot validation;
9. application of all migrations to a clean PostgreSQL database;
10. Docker image build.

A failed build, test, migration validation, or migration application prevents container publication.

## Continuous Delivery

On a successful push to `main`, the workflow publishes:

```text
ghcr.io/viktor132607/fintrox.server:latest
ghcr.io/viktor132607/fintrox.server:sha-<commit>
```

Pull requests build the image but never publish it.

## Deployment

Provider deployment remains separate from artifact delivery. When the production Render service is connected, it should consume the validated GHCR image.

Production database migrations must remain an explicit deployment step; the API does not mutate the production schema automatically during startup.

## Dependency updates

Dependabot checks NuGet packages and GitHub Actions weekly.
