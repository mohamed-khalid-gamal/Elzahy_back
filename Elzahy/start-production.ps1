# Elzahy Portfolio API Production Startup Script for Windows
Write-Host "Starting Elzahy Portfolio API in Production mode..." -ForegroundColor Green

# Set production environment
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:DOTNET_ENVIRONMENT = "Production"

# Ensure keys directory exists
New-Item -ItemType Directory -Force -Path "./keys" | Out-Null

# Set database connection (already configured in appsettings)
$env:MYSQL_CONNECTION_STRING = "Server=db28000.public.databaseasp.net; Database=db28000; User Id=db28000; Password=gZ=6-8HcAd7%; Encrypt=False; MultipleActiveResultSets=True;"

# Start the application
Write-Host "Starting API server..." -ForegroundColor Yellow
dotnet Elzahy.dll

Write-Host "API server stopped." -ForegroundColor Red