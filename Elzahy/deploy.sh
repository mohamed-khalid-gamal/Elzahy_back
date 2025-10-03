#!/bin/bash

# Elzahy Portfolio API Deployment Script
echo "Starting deployment of Elzahy Portfolio API..."

# Set environment variables for production
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ENVIRONMENT=Production

# Database connection
export MYSQL_CONNECTION_STRING="Server=db28000.public.databaseasp.net; Database=db28000; User Id=db28000; Password=gZ=6-8HcAd7%; Encrypt=False; MultipleActiveResultSets=True;"

# JWT Configuration (Use secure values in production)
export DOTNET_JWT_KEY="MyVerySecretKeyForJWTTokenGeneration2024!@#$%^&*()_+"
export JWT__ISSUER="Elzahy"
export JWT__AUDIENCE="Elzahy-Users"

# App URLs (Update these with your actual production URLs)
export APP__BASEURL="https://your-production-domain.com"
export FRONTEND_URL="https://your-frontend-domain.com"

# Data Protection
export DOTNET_DATAPROTECTION_KEYS="/app/keys"

# Email Configuration (Use secure values in production)
export EMAIL__FROM="net16654@gmail.com"
export EMAIL__SMTPHOST="smtp.gmail.com"
export EMAIL__SMTPPORT="587"
export EMAIL__SMTPUSER="net16654@gmail.com"
export EMAIL__SMTPPASSWORD="kitl fdix kbwv ktjc"

# Build and publish the application
echo "Building application..."
dotnet publish ./Elzahy/Elzahy.csproj -c Release -o ./publish

# Create keys directory
mkdir -p ./publish/keys

echo "Deployment preparation complete!"
echo "Published files are in the ./publish directory"
echo ""
echo "To run the application:"
echo "cd ./publish"
echo "dotnet Elzahy.dll"
echo ""
echo "Important: Update the following environment variables with your production values:"
echo "- APP__BASEURL: Your production API URL"
echo "- FRONTEND_URL: Your production frontend URL"
echo "- DOTNET_JWT_KEY: A secure JWT secret key"
echo "- Email settings: Use your production email credentials"