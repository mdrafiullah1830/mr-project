#!/bin/bash

# MR Shop Order Tracking API - Startup Script
# This script starts the ASP.NET Core API server

echo "🚀 Starting MR Shop Order Tracking API..."
echo ""

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found!"
    echo "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Display .NET version
echo "✓ .NET SDK found: $(dotnet --version)"
echo ""

# Navigate to project directory
cd "$(dirname "$0")"

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Failed to restore dependencies"
    exit 1
fi

echo "✓ Dependencies restored"
echo ""

# Build project
echo "🔨 Building project..."
dotnet build --configuration Release --no-restore --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "✓ Build successful"
echo ""

# Start the API
echo "🌐 Starting API server..."
echo "📍 Swagger UI: http://localhost:5010"
echo "📡 API Base: http://localhost:5010/api/orders"
echo ""
echo "Press Ctrl+C to stop the server"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

dotnet run --no-build --configuration Release
