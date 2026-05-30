FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer cache optimisation)
COPY WEX.CorporatePayments.sln .
COPY src/WEX.Domain/WEX.Domain.csproj src/WEX.Domain/
COPY src/WEX.Application/WEX.Application.csproj src/WEX.Application/
COPY src/WEX.Infrastructure/WEX.Infrastructure.csproj src/WEX.Infrastructure/
COPY src/WEX.API/WEX.API.csproj src/WEX.API/

RUN dotnet restore src/WEX.API/WEX.API.csproj

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
