#!/usr/bin/env bash
# WEX Corporate Payments — Quick Start Script (Mac/Linux)
# Tries Docker Compose first; falls back to local instructions if Docker is unavailable.

set -e

echo ""
echo "================================================"
echo "  WEX Corporate Payments - Quick Start"
echo "================================================"
echo ""

# Check if Docker is available and running
if command -v docker &>/dev/null && docker info &>/dev/null 2>&1; then
    echo "✅ Docker detected — starting with Docker Compose..."
    echo ""
    echo "  Services starting:"
    echo "    PostgreSQL  → localhost:5432"
    echo "    API         → http://localhost:5000"
    echo "    Swagger     → http://localhost:5000/swagger"
    echo "    Angular UI  → http://localhost:4200"
    echo ""
    echo "Press Ctrl+C to stop all services."
    echo ""

    docker-compose up --build
else
    echo "⚠️  Docker not available — manual setup required."
    echo ""
    echo "Prerequisites:"
    echo "  • .NET 8 SDK     https://dotnet.microsoft.com/download"
    echo "  • Node.js 20+    https://nodejs.org"
    echo "  • PostgreSQL 16  https://www.postgresql.org/download"
    echo ""
    echo "Steps:"
    echo "  1. Create a local PostgreSQL database:"
    echo "       createdb -U postgres wex_corporate_local"
    echo "       createuser -U postgres wex_user"
    echo "       psql -U postgres -c \"ALTER USER wex_user WITH PASSWORD 'wex_local_password';\""
    echo "       psql -U postgres -c \"GRANT ALL PRIVILEGES ON DATABASE wex_corporate_local TO wex_user;\""
    echo ""
    echo "  2. Start the API (in a new terminal):"
    echo "       cd src/WEX.API"
    echo "       dotnet run --launch-profile Local"
    echo "       → API: http://localhost:5000  Swagger: http://localhost:5000/swagger"
    echo ""
    echo "  3. Start the UI (in another terminal):"
    echo "       cd src/WEX.UI"
    echo "       npm install"
    echo "       npx ng serve --configuration local"
    echo "       → UI: http://localhost:4200"
    echo ""
fi
