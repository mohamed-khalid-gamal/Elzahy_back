# Elzahy Portfolio Management System

A comprehensive portfolio management system with advanced authentication, 2FA, and admin dashboard built with ASP.NET Core 8.

## Features

### ?? Authentication & Security
- **User Registration & Login** with email validation
- **Two-Factor Authentication (2FA)** with QR code setup
- **JWT-based Authentication** with refresh tokens
- **Password Reset** via email
- **Email Confirmation** for new accounts
- **Role-based Authorization** (User/Admin)

### ?? Portfolio Management
- **Project Management** with three status types:
  - Current Projects
  - Future Projects
  - Past Projects
- **Award Management** with certificates and images
- **Contact Message System** with filtering and sorting

### ?? Email System
- Email confirmation for new registrations
- Password reset emails
- 2FA verification codes
- Welcome emails

## API Endpoints

### Authentication Endpoints

#### Register New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "name": "John Doe",
  "terms": true
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "twoFactorCode": "123456" // Optional, required if 2FA is enabled
}
```

#### Setup 2FA
```http
POST /api/auth/2fa/setup
Authorization: Bearer {access_token}
```

#### Enable 2FA
```http
POST /api/auth/2fa/enable
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "code": "123456"
}
```

#### Forgot Password
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### Reset Password
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "reset-token-from-email",
  "newPassword": "NewPassword123!"
}
```

### Project Management Endpoints

#### Get All Projects
```http
GET /api/projects?status=Current&isPublished=true
```

#### Get Projects by Status
```http
GET /api/projects/status/Current
GET /api/projects/status/Future
GET /api/projects/status/Past
```

#### Create Project (Admin Only)
```http
POST /api/projects
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "name": "E-commerce Platform",
  "description": "A modern e-commerce solution with advanced features",
  "photoUrl": "https://example.com/project-image.jpg",
  "status": "Current",
  "technologiesUsed": "ASP.NET Core, React, SQL Server",
  "projectUrl": "https://ecommerce.example.com",
  "gitHubUrl": "https://github.com/user/ecommerce",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": null,
  "client": "ABC Company",
  "budget": 50000.00,
  "isPublished": true,
  "sortOrder": 1
}
```

#### Update Project (Admin Only)
```http
PUT /api/projects/{id}
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "name": "Updated Project Name",
  "status": "Past",
  "endDate": "2024-12-31T00:00:00Z"
}
```

### Award Management Endpoints

#### Get All Awards
```http
GET /api/awards?isPublished=true
```

#### Create Award (Admin Only)
```http
POST /api/awards
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "name": "Best Developer Award 2024",
  "givenBy": "Tech Conference 2024",
  "dateReceived": "2024-06-15T00:00:00Z",
  "description": "Awarded for outstanding contribution to open source",
  "certificateUrl": "https://example.com/certificate.pdf",
  "imageUrl": "https://example.com/award-image.jpg",
  "isPublished": true,
  "sortOrder": 1
}
```

### Contact Management Endpoints

#### Submit Contact Message
```http
POST /api/contact
Content-Type: application/json

{
  "fullName": "John Doe",
  "emailAddress": "john@example.com",
  "subject": "Project Inquiry",
  "message": "I'm interested in working with you on a project.",
  "phoneNumber": "+1234567890",
  "company": "ABC Corp"
}
```

#### Get Contact Messages (Admin Only)
```http
GET /api/contact?fromDate=2024-01-01&toDate=2024-12-31&isRead=false&sortBy=CreatedAt&sortDescending=true&page=1&pageSize=10
Authorization: Bearer {admin_access_token}
```

#### Mark Message as Read (Admin Only)
```http
POST /api/contact/{id}/mark-read
Authorization: Bearer {admin_access_token}
```

#### Mark Message as Replied (Admin Only)
```http
POST /api/contact/{id}/mark-replied
Authorization: Bearer {admin_access_token}
```

## Configuration

### Database Connection
Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ElzahyPortfolio;Trusted_Connection=True;"
  }
}
```

### Email Configuration
Configure SMTP settings in `appsettings.json`:

```json
{
  "Email": {
    "From": "noreply@elzahy.com",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  }
}
```

### JWT Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "your-very-long-secret-key-here",
    "Issuer": "Elzahy",
    "Audience": "Elzahy-Users",
    "ExpirationInHours": "24"
  }
}
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- An email service (Gmail, SendGrid, etc.) for email functionality

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-repo/elzahy-portfolio.git
   cd elzahy-portfolio
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update configuration**
   - Update `appsettings.json` with your database connection string
   - Configure email settings
   - Update JWT secret key

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - API: `https://localhost:7000`
   - Swagger UI: `https://localhost:7000/swagger`

### Default Admin Account
- **Email**: admin@gmail.com
- **Password**: Admin12345

## Database Schema

### Users Table
- `Id` (Guid, Primary Key)
- `Email` (String, Unique)
- `Name` (String)
- `PasswordHash` (String)
- `Role` (String: "User" | "Admin")
- `TwoFactorEnabled` (Boolean)
- `TwoFactorSecret` (String, Nullable)
- `EmailConfirmed` (Boolean)
- `CreatedAt`, `UpdatedAt` (DateTime)

### Projects Table
- `Id` (Guid, Primary Key)
- `Name` (String)
- `Description` (Text)
- `Status` (Enum: Current, Future, Past)
- `PhotoUrl`, `ProjectUrl`, `GitHubUrl` (String, Nullable)
- `TechnologiesUsed` (String, Nullable)
- `StartDate`, `EndDate` (DateTime, Nullable)
- `Client` (String, Nullable)
- `Budget` (Decimal, Nullable)
- `IsPublished` (Boolean)
- `SortOrder` (Integer)
- `CreatedByUserId` (Guid, Foreign Key)

### Awards Table
- `Id` (Guid, Primary Key)
- `Name` (String)
- `GivenBy` (String)
- `DateReceived` (DateTime)
- `Description` (Text, Nullable)
- `CertificateUrl`, `ImageUrl` (String, Nullable)
- `IsPublished` (Boolean)
- `SortOrder` (Integer)
- `CreatedByUserId` (Guid, Foreign Key)

### ContactMessages Table
- `Id` (Guid, Primary Key)
- `FullName` (String)
- `EmailAddress` (String)
- `Subject` (String)
- `Message` (Text)
- `PhoneNumber`, `Company` (String, Nullable)
- `IsRead`, `IsReplied` (Boolean)
- `ReadAt`, `RepliedAt` (DateTime, Nullable)
- `AdminNotes` (Text, Nullable)
- `CreatedAt` (DateTime)

## Security Features

### Two-Factor Authentication
- QR code generation for authenticator apps
- Email-based backup codes
- TOTP (Time-based One-Time Password) support

### Password Security
- BCrypt hashing for passwords
- Password reset with time-limited tokens
- Email confirmation for new accounts

### API Security
- JWT-based authentication
- Role-based authorization
- CORS configuration for frontend integration
- Request validation and sanitization

## Admin Features

### Project Management
- Create, update, delete projects
- Manage project visibility (published/unpublished)
- Sort projects by custom order
- Filter projects by status and publication state

### Award Management
- Add awards with certificates and images
- Manage award visibility
- Sort awards chronologically or custom order

### Contact Message Management
- View all contact messages with filtering:
  - Date range filtering
  - Read/unread status
  - Replied/not replied status
- Sort by date, name, or subject
- Pagination support
- Mark messages as read/replied
- Add admin notes to messages

## API Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "ok": true,
  "data": {
    // Response data here
  },
  "error": null
}
```

### Error Response
```json
{
  "ok": false,
  "data": null,
  "error": {
    "message": "Error description",
    "internalCode": 4001
  }
}
```

### Paginated Response
```json
{
  "ok": true,
  "data": {
    "data": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

## Error Codes

| Code | Description |
|------|-------------|
| 4001 | Invalid credentials or authentication failed |
| 4002 | Terms and conditions not accepted |
| 4003 | User already exists |
| 4004 | Resource not found |
| 4005 | 2FA setup required |
| 5000 | Internal server error |

## Frontend Integration

### CORS Configuration
The API is configured to accept requests from:
- `http://localhost:4200` (Angular development)
- `https://angular-example-app.netlify.app` (Production)

### Authentication Flow
1. Register/Login to get access and refresh tokens
2. Include access token in Authorization header: `Bearer {token}`
3. Refresh token when access token expires
4. Handle 2FA flow if enabled

### Example Frontend Usage (JavaScript)

```javascript
// Login
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password123'
  })
});

const loginData = await loginResponse.json();
if (loginData.ok) {
  localStorage.setItem('accessToken', loginData.data.accessToken);
  localStorage.setItem('refreshToken', loginData.data.refreshToken);
}

// Authenticated request
const projectsResponse = await fetch('/api/projects', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
  }
});
```

## Deployment

### Production Considerations

1. **Environment Variables**: Use environment variables for sensitive configuration
2. **HTTPS**: Ensure all communication is over HTTPS
3. **Database**: Use a production-grade database server
4. **Email Service**: Configure with a reliable email service (SendGrid, AWS SES, etc.)
5. **Logging**: Implement comprehensive logging for monitoring
6. **Rate Limiting**: Add rate limiting for API endpoints
7. **File Storage**: For production, consider cloud storage for images and certificates

### Docker Deployment (Optional)

Create a `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Elzahy.csproj", "."]
RUN dotnet restore "./Elzahy.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Elzahy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Elzahy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Elzahy.dll"]
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please contact:
- Email: support@elzahy.com
- GitHub Issues: [Project Issues](https://github.com/your-repo/elzahy-portfolio/issues)

---

**Built with ?? using ASP.NET Core 8**