FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:5ef85cc12cb25be6ec319a7392d1e9efd53c3bc8abb971c53d8058a473f09053 AS build
WORKDIR /src

COPY WAssisInsurance.sln ./
COPY src/WAssis.Domain.Core/WAssis.Domain.Core.csproj src/WAssis.Domain.Core/
COPY src/WAssis.Domain/WAssis.Domain.csproj src/WAssis.Domain/
COPY src/WAssis.Application/WAssis.Application.csproj src/WAssis.Application/
COPY src/WAssis.Infra.CrossCutting.Bus/WAssis.Infra.CrossCutting.Bus.csproj src/WAssis.Infra.CrossCutting.Bus/
COPY src/WAssis.Infra.CrossCutting.Identity/WAssis.Infra.CrossCutting.Identity.csproj src/WAssis.Infra.CrossCutting.Identity/
COPY src/WAssis.Infra.CrossCutting.IoC/WAssis.Infra.CrossCutting.IoC.csproj src/WAssis.Infra.CrossCutting.IoC/
COPY src/WAssis.Infra.Data/WAssis.Infra.Data.csproj src/WAssis.Infra.Data/
COPY src/WAssis.Services.Api/WAssis.Services.Api.csproj src/WAssis.Services.Api/
COPY src/WAssis.BackgroundTasks/WAssis.BackgroundTasks.csproj src/WAssis.BackgroundTasks/
COPY src/WAssis.UI.Web/WAssis.UI.Web.csproj src/WAssis.UI.Web/
COPY tests/WAssis.Tests/WAssis.Tests.csproj tests/WAssis.Tests/

RUN dotnet restore WAssisInsurance.sln

COPY . .
RUN dotnet publish src/WAssis.Services.Api/WAssis.Services.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:9a464e9a7e8c6144631020975f703c89034fe386417cb740620df69c2c6cfe24 AS runtime
# Security patch newer than the pinned upstream image (CVE-2026-86145 / CVE-2026-89161).
RUN apt-get update \
    && apt-get install --no-install-recommends --only-upgrade -y libpcre2-8-0=10.42-1+deb12u1 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app

ARG BUILD_SHA=local
ENV BUILD_SHA=$BUILD_SHA
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "WAssis.Services.Api.dll"]
