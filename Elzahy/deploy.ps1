# Elzahy Portfolio API Deployment Script for Windows
Write-Host "Starting deployment of Elzahy Portfolio API..." -ForegroundColor Green

# Set environment variables for production
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:DOTNET_ENVIRONMENT = "Production"

# Database connection
$env:MYSQL_CONNECTION_STRING = "Server=db28000.public.databaseasp.net; Database=db28000; User Id=db28000; Password=gZ=6-8HcAd7%; Encrypt=False; MultipleActiveResultSets=True;"

# JWT Configuration (Use secure values in production)
$env:DOTNET_JWT_KEY = "MyVerySecretKeyForJWTTokenGeneration2024!@#$%^&*()_+"
$env:JWT__ISSUER = "Elzahy"
$env:JWT__AUDIENCE = "Elzahy-Users"

# App URLs (Update these with your actual production URLs)
$env:APP__BASEURL = "https://your-production-domain.com"
$env:FRONTEND_URL = "https://your-frontend-domain.com"

# Data Protection
$env:DOTNET_DATAPROTECTION_KEYS = "./keys"

# Email Configuration (Use secure values in production)
$env:EMAIL__FROM = "net16654@gmail.com"
$env:EMAIL__SMTPHOST = "smtp.gmail.com"
$env:EMAIL__SMTPPORT = "587"
$env:EMAIL__SMTPUSER = "net16654@gmail.com"
$env:EMAIL__SMTPPASSWORD = "kitl fdix kbwv ktjc"

# Build and publish the application
Write-Host "Building application..." -ForegroundColor Yellow
dotnet publish ./Elzahy/Elzahy.csproj -c Release -o ./publish

# Create keys directory
New-Item -ItemType Directory -Force -Path "./publish/keys"

Write-Host "Deployment preparation complete!" -ForegroundColor Green
Write-Host "Published files are in the ./publish directory" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Yellow
Write-Host "cd ./publish"
Write-Host "dotnet Elzahy.dll"
Write-Host ""
Write-Host "Important: Update the following environment variables with your production values:" -ForegroundColor Red
Write-Host "- APP__BASEURL: Your production API URL"
Write-Host "- FRONTEND_URL: Your production frontend URL"
Write-Host "- DOTNET_JWT_KEY: A secure JWT secret key"
Write-Host "- Email settings: Use your production email credentials"