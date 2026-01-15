## Attempt 1
- Docker build failed before executing Dockerfile steps because the SDK base image metadata fetch from `mcr.microsoft.com` returned EOF, indicating a registry/network resolution issue rather than a Dockerfile problem.
## Attempt 2
- Using generic `mcr.microsoft.com/dotnet/*:8.0` tags gets past image pulls, but Debian 12 repo setup did not expose a `powershell` package, so installation failed with "Unable to locate package powershell".
## Attempt 3
- Switching to Ubuntu jammy still fails to locate the `powershell` package on arm64, so the Microsoft apt repo appears to not provide PowerShell for this architecture.
## Attempt 4
- Even with a tarball-based PowerShell install plan, the build can still fail early if Docker cannot fetch base image metadata (EOF from `mcr.microsoft.com`), indicating intermittent registry/network issues.
## Attempt 5
- Docker build still fails at fetching `mcr.microsoft.com/dotnet/sdk:8.0` metadata (EOF), so the issue persists before any Dockerfile steps run.
## Attempt 6
- Switching to `8.0-jammy` tags still fails on metadata fetch (EOF) from `mcr.microsoft.com`, so the registry access issue is independent of the specific tag.
## Attempt 7
- Using `8.0-bookworm-slim` tags succeeded; base image metadata was reachable and the build completed with cached layers.