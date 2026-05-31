#!/usr/bin/env pwsh
# WEX Corporate Payments — Quick Start Script (Windows/PowerShell)
# Tries Docker Compose first; falls back to local instructions if Docker is unavailable.

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  WEX Corporate Payments - Quick Start" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is available and running
$dockerAvailable = $false
try {
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -eq 0) {
        $dockerAvailable = $true
    }
} catch {
    $dockerAvailable = $false
}

if ($dockerAvailable) {
    Write-Host "✅ Docker detected — starting with Docker Compose..." -ForegroundColor Green
    Write-Host ""
    Write-Host "  Services starting:" -ForegroundColor Yellow
    Write-Host "    PostgreSQL  → localhost:5432"
    Write-Host "    API         → http://localhost:5000"
    Write-Host "    Swagger     → http://localhost:5000/swagger"
    Write-Host "    Angular UI  → http://localhost:4200"
    Write-Host ""
    Write-Host "Press Ctrl+C to stop all services." -ForegroundColor DarkGray
    Write-Host ""

    docker-compose up --build
} else {
    Write-Host "⚠️  Docker not available — manual setup required." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Prerequisites:" -ForegroundColor Cyan
    Write-Host "  • .NET 8 SDK     https://dotnet.microsoft.com/download"
    Write-Host "  • Node.js 20+    https://nodejs.org"
    Write-Host "  • PostgreSQL 16  https://www.postgresql.org/download"
    Write-Host ""
    Write-Host "Steps:" -ForegroundColor Cyan
    Write-Host "  1. Create a local PostgreSQL database:"
    Write-Host "       createdb -U postgres wex_corporate_local"
    Write-Host "       createuser -U postgres wex_user"
    Write-Host "       psql -U postgres -c `"ALTER USER wex_user WITH PASSWORD 'wex_local_password';`""
    Write-Host "       psql -U postgres -c `"GRANT ALL PRIVILEGES ON DATABASE wex_corporate_local TO wex_user;`""
    Write-Host ""
    Write-Host "  2. Start the API (in a new terminal):"
    Write-Host "       cd src\WEX.API"
    Write-Host "       dotnet run --launch-profile Local"
    Write-Host "       → API: http://localhost:5000  Swagger: http://localhost:5000/swagger"
    Write-Host ""
    Write-Host "  3. Start the UI (in another terminal):"
    Write-Host "       cd src\WEX.UI"
    Write-Host "       npm install"
    Write-Host "       npx ng serve --configuration local"
    Write-Host "       → UI: http://localhost:4200"
    Write-Host ""
}
