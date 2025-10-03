# Elzahy Portfolio API - Deployment Guide

This document provides instructions for deploying the Elzahy Portfolio API to production.

## Database Configuration

The application is configured to use the following production database:
- **Server**: db28000.public.databaseasp.net
- **Database**: db28000
- **Connection String**: `Server=db28000.public.databaseasp.net; Database=db28000; User Id=db28000; Password=gZ=6-8HcAd7%; Encrypt=False; MultipleActiveResultSets=True;`

## Deployment Options

### Option 1: Standard .NET Deployment

1. **Build and Publish**:
   ```bash
   dotnet publish ./Elzahy/Elzahy.csproj -c Release -o ./publish
   ```

2. **Set Environment Variables** (Update URLs with your actual production values):
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   export MYSQL_CONNECTION_STRING="Server=db28000.public.databaseasp.net; Database=db28000; User Id=db28000; Password=gZ=6-8HcAd7%; Encrypt=False; MultipleActiveResultSets=True;"
   export DOTNET_JWT_KEY="MyVerySecretKeyForJWTTokenGeneration2024!@#$%^&*()_+"
   export APP__BASEURL="https://your-production-domain.com"
   export FRONTEND_URL="https://your-frontend-domain.com"
   ```

3. **Run the Application**:
   ```bash
   cd ./publish
   dotnet Elzahy.dll
   ```

### Option 2: Using Deployment Scripts

#### Linux/macOS:
```bash
chmod +x deploy.sh
./deploy.sh
```

#### Windows:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
./deploy.ps1
```

### Option 3: Docker Deployment

1. **Build and Run with Docker Compose**:
   ```bash
   docker-compose up -d
   ```

2. **Or build manually**:
   ```bash
   docker build -t elzahy-api -f Elzahy/Dockerfile .
   docker run -d -p 8080:8080 -p 8081:8081 \
     -e ASPNETCORE_ENVIRONMENT=Production \
     -e MYSQL_CONNECTION_STRING="Server=db28000.public.databaseasp.net; Database=db28000; User Id=db28000; Password=gZ=6-8HcAd7%; Encrypt=False; MultipleActiveResultSets=True;" \
     elzahy-api
   ```

## Environment Configuration

### Required Environment Variables for Production:

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `MYSQL_CONNECTION_STRING` | Database connection | Already configured |
| `DOTNET_JWT_KEY` | JWT secret key | `MyVerySecretKeyForJWTTokenGeneration2024!@#$%^&*()_+` |
| `APP__BASEURL` | API base URL | `https://your-api-domain.com` |
| `FRONTEND_URL` | Frontend URL for CORS | `https://your-frontend-domain.com` |

### Optional Environment Variables:

| Variable | Description | Default Value |
|----------|-------------|---------------|
| `EMAIL__FROM` | Email sender address | `net16654@gmail.com` |
| `EMAIL__SMTPHOST` | SMTP server | `smtp.gmail.com` |
| `EMAIL__SMTPPORT` | SMTP port | `587` |
| `DOTNET_DATAPROTECTION_KEYS` | Data protection keys path | `./keys` |

## Security Considerations

1. **JWT Secret Key**: Generate a strong, unique secret key for production
2. **Database Credentials**: The current credentials are configured for the specified database
3. **HTTPS**: Ensure your production environment uses HTTPS
4. **CORS**: Update `FRONTEND_URL` to match your actual frontend domain
5. **Email Credentials**: Use secure email credentials for production

## Health Check

The API includes a health check endpoint at `/health` that returns:
```json
{
  "Status": "Healthy",
  "Environment": "Production",
  "Timestamp": "2024-01-01T00:00:00.000Z"
}
```

## API Documentation

- **Development**: Swagger UI is enabled at the root URL (`/`)
- **Production**: Swagger is disabled by default for security

## Database Migrations

On startup, the application will:
- **Development**: Use `EnsureCreated()` for database initialization
- **Production**: Run pending migrations automatically
- Seed a default admin user

## Logging

Production logging levels are configured to reduce verbosity while maintaining important information:
- Default: Warning
- Microsoft.AspNetCore: Warning
- Microsoft.EntityFrameworkCore: Warning

## Troubleshooting

1. **Database Connection Issues**: Verify the connection string and network access
2. **JWT Issues**: Ensure the JWT secret key is properly set
3. **CORS Issues**: Verify the frontend URL is correctly configured
4. **Email Issues**: Check SMTP credentials and network access

## Support

For deployment issues or questions, refer to the application logs or contact the development team.