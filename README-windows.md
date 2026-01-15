# Windows Containers Sketch

(Alan's notes; this seems overkill, but I'm leaving this here, below notes are some notes from Cursos, the new GPT-5.2 Codex, and I'll see if there's anything useful in the notes below.)
Disclaimer; totally untested, unchecked stuff below. Compared to README.md which i've gone through a dozen times and that looks really solid. This would be a random place to start if I actually needed to use a windows image. 

---

This project uses Linux containers by default. If your environment requires
Windows containers, the Dockerfile needs a Windows base image and different
install steps for PowerShell and Playwright.

This is a sketch (not fully validated in this repo) to get you started.

## High-level differences

- Base image must be a Windows variant, e.g.
  `mcr.microsoft.com/dotnet/aspnet:8.0-windowsservercore-ltsc2022`.
- Playwright browser install uses the Windows path and does not use
  `--with-deps`.
- The image is significantly larger than the Linux slim image.
- The Windows base image version must match your host (or be compatible).

## Sketch Dockerfile (Windows containers)

```Dockerfile
# escape=`
FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS build
WORKDIR /src

COPY src/PdfApi/PdfApi.csproj /src/PdfApi/
RUN dotnet restore /src/PdfApi/PdfApi.csproj

COPY src/PdfApi/ /src/PdfApi/
WORKDIR /src/PdfApi
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-windowsservercore-ltsc2022 AS runtime
WORKDIR /app

ENV PLAYWRIGHT_BROWSERS_PATH=C:\ms-playwright
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish/ /app/
COPY src/PdfApi/templates C:\templates
RUN mkdir C:\data

# PowerShell 7 (pwsh) install via MSI (example)
SHELL ["powershell", "-NoProfile", "-Command"]
ARG POWERSHELL_VERSION=7.4.1
RUN $ProgressPreference = 'SilentlyContinue' ; `
    Invoke-WebRequest -UseBasicParsing `
      "https://github.com/PowerShell/PowerShell/releases/download/v$env:POWERSHELL_VERSION/PowerShell-$env:POWERSHELL_VERSION-win-x64.msi" `
      -OutFile C:\pwsh.msi ; `
    Start-Process msiexec.exe -Wait -ArgumentList '/i', 'C:\pwsh.msi', '/qn', '/norestart' ; `
    Remove-Item C:\pwsh.msi

# Install Playwright browsers (Windows)
RUN C:\Program` Files\PowerShell\7\pwsh.exe -File C:\app\playwright.ps1 install chromium

EXPOSE 8080
ENTRYPOINT ["dotnet", "PdfApi.dll"]
```

## Notes

- The `windowsservercore-ltsc2022` tag is a common default. Use the tag that
  matches your host OS build.
- If you already have `pwsh` in your base image, you can skip the MSI step.
- Playwright in Windows containers is more sensitive to image base and host
  configuration; test early in CI.
