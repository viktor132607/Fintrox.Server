# CI/CD

## Continuous Integration

The `CI/CD` GitHub Actions workflow runs on every pull request targeting `main`, every push to `main`, and manual dispatch.

The CI gate performs:

1. repository checkout;
2. .NET SDK setup from `global.json`;
3. solution restore;
4. Release build with warnings treated as errors;
5. execution of every `tests/**/*Tests.csproj` project when test projects exist;
6. Docker image build.

A failed build or test prevents the container job from running.

## Continuous Delivery

On a successful push to `main`, the same workflow publishes the application image to GitHub Container Registry:

```text
ghcr.io/viktor132607/fintrox.server:latest
ghcr.io/viktor132607/fintrox.server:sha-<commit>
```

Pull requests build the image but never publish it.

This creates a deployable, immutable application artifact before a hosting provider is connected.

## Deployment

Provider deployment is intentionally separate from artifact delivery. When the production Render service is created, it should deploy the GHCR image produced by this workflow instead of rebuilding an unrelated source revision.

No long-lived registry password is stored: GHCR publishing uses the repository-scoped `GITHUB_TOKEN`.

## Dependency updates

Dependabot checks NuGet packages and GitHub Actions weekly.
