#!/bin/bash

# Elzahy Portfolio API Production Startup Script
echo "Starting Elzahy Portfolio API in Production mode..."

# Set production environment
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ENVIRONMENT=Production

# Ensure keys directory exists
mkdir -p ./keys

# Set database connection (already configured in appsettings)
export MYSQL_CONNECTION_STRING="Server=db28000.public.databaseasp.net; Database=db28000; User Id=db28000; Password=gZ=6-8HcAd7%; Encrypt=False; MultipleActiveResultSets=True;"

# Start the application
echo "Starting API server..."
dotnet Elzahy.dll

echo "API server stopped."