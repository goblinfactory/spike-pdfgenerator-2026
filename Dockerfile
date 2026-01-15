FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

COPY src/PdfApi/PdfApi.csproj /src/PdfApi/
RUN dotnet restore /src/PdfApi/PdfApi.csproj

COPY src/PdfApi/ /src/PdfApi/
WORKDIR /src/PdfApi
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish/ /app/
COPY src/PdfApi/templates /templates
RUN mkdir -p /data

RUN test -f /app/playwright.ps1

ARG POWERSHELL_VERSION=7.4.1
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget ca-certificates tar \
    && arch="$(dpkg --print-architecture)" \
    && if [ "$arch" = "amd64" ]; then ps_arch="x64"; \
       elif [ "$arch" = "arm64" ]; then ps_arch="arm64"; \
       else echo "unsupported architecture: $arch"; exit 1; fi \
    && mkdir -p /opt/microsoft/powershell/7 \
    && wget -q "https://github.com/PowerShell/PowerShell/releases/download/v${POWERSHELL_VERSION}/powershell-${POWERSHELL_VERSION}-linux-${ps_arch}.tar.gz" -O /tmp/powershell.tar.gz \
    && tar -xzf /tmp/powershell.tar.gz -C /opt/microsoft/powershell/7 \
    && ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh \
    && rm /tmp/powershell.tar.gz \
    && rm -rf /var/lib/apt/lists/*

RUN pwsh -File /app/playwright.ps1 install --with-deps chromium

EXPOSE 8080

ENTRYPOINT ["dotnet", "PdfApi.dll"]
