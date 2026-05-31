FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet config first — ensures only nuget.org is used (no Windows-specific fallback paths)
COPY NuGet.Config .

# Copy project files first (layer cache optimisation — only re-restores when csproj changes)
COPY src/WEX.Domain/WEX.Domain.csproj src/WEX.Domain/
COPY src/WEX.Application/WEX.Application.csproj src/WEX.Application/
COPY src/WEX.Infrastructure/WEX.Infrastructure.csproj src/WEX.Infrastructure/
COPY src/WEX.API/WEX.API.csproj src/WEX.API/

RUN dotnet restore src/WEX.API/WEX.API.csproj --configfile NuGet.Config

# Copy all source
COPY src/ src/

RUN dotnet publish src/WEX.API/WEX.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WEX.API.dll"]
