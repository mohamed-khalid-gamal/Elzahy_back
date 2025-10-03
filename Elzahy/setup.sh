#!/bin/bash

echo "==========================================="
echo "   Elzahy Portfolio API - Quick Setup     "
echo "==========================================="

# Check if .NET 8 is installed
if ! command -v dotnet &> /dev/null; then
    echo "? .NET 8 SDK is not installed. Please install it first."
    exit 1
fi

echo "? .NET SDK found: $(dotnet --version)"

# Restore packages
echo "?? Restoring NuGet packages..."
dotnet restore ./Elzahy/Elzahy.csproj

# Build the application
echo "?? Building application..."
dotnet build ./Elzahy/Elzahy.csproj -c Release

# Publish for production
echo "?? Publishing for production deployment..."
dotnet publish ./Elzahy/Elzahy.csproj -c Release -o ./publish

# Create required directories
echo "?? Creating required directories..."
mkdir -p ./publish/keys

# Make scripts executable
chmod +x deploy.sh
chmod +x start-production.sh

echo ""
echo "? Setup complete!"
echo ""
echo "?? To deploy and run:"
echo "   1. For production: ./deploy.sh"
echo "   2. Or manually: cd publish && ../start-production.sh"
echo "   3. Or with Docker: docker-compose up -d"
echo ""
echo "?? Health check: http://localhost:8080/health"
echo "?? API docs (dev): http://localhost:8080/"
echo ""
echo "??  Remember to update these values for production:"
echo "   - APP__BASEURL in environment variables"
echo "   - FRONTEND_URL in environment variables"
echo "   - JWT secret key for security"