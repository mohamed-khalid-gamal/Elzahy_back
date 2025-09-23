# API Testing Examples

This document provides examples of how to test the Elzahy Portfolio API endpoints using various tools.

## Using cURL

### 1. Register a New User
```bash
curl -X POST "https://localhost:7000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "name": "Test User",
    "terms": true
  }'
```

### 2. Login
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@gmail.com",
    "password": "Admin12345"
  }'
```

### 3. Get Current User Profile
```bash
curl -X GET "https://localhost:7000/api/auth/me" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 4. Create a Project (Admin only)
```bash
curl -X POST "https://localhost:7000/api/projects" \
  -H "Authorization: Bearer YOUR_ADMIN_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Portfolio Website",
    "description": "A modern portfolio website built with React and ASP.NET Core",
    "status": "Current",
    "technologiesUsed": "React, ASP.NET Core, SQL Server",
    "projectUrl": "https://portfolio.example.com",
    "gitHubUrl": "https://github.com/user/portfolio",
    "isPublished": true,
    "sortOrder": 1
  }'
```

### 5. Get All Projects
```bash
curl -X GET "https://localhost:7000/api/projects?isPublished=true"
```

### 6. Submit Contact Message
```bash
curl -X POST "https://localhost:7000/api/contact" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "emailAddress": "john@example.com",
    "subject": "Project Inquiry",
    "message": "I would like to discuss a potential project."
  }'
```

## Using PowerShell (Windows)

### Login and Store Token
```powershell
$loginResponse = Invoke-RestMethod -Uri "https://localhost:7000/api/auth/login" -Method Post -ContentType "application/json" -Body '{
  "email": "admin@gmail.com",
  "password": "Admin12345"
}'

$token = $loginResponse.data.accessToken
$headers = @{ "Authorization" = "Bearer $token" }
```

### Get Projects
```powershell
$projects = Invoke-RestMethod -Uri "https://localhost:7000/api/projects" -Method Get -Headers $headers
$projects.data
```

### Create Award
```powershell
$awardData = @{
    name = "Best Developer 2024"
    givenBy = "Tech Conference"
    dateReceived = "2024-06-15T00:00:00Z"
    description = "Outstanding contribution to open source"
    isPublished = $true
    sortOrder = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7000/api/awards" -Method Post -Headers $headers -ContentType "application/json" -Body $awardData
```

## Using JavaScript/Fetch API

### Complete Authentication Flow
```javascript
class ElzahyApiClient {
    constructor(baseUrl = 'https://localhost:7000') {
        this.baseUrl = baseUrl;
        this.accessToken = localStorage.getItem('accessToken');
        this.refreshToken = localStorage.getItem('refreshToken');
    }

    async login(email, password, twoFactorCode = null) {
        const response = await fetch(`${this.baseUrl}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email,
                password,
                twoFactorCode
            })
        });

        const result = await response.json();
        
        if (result.ok && !result.data.requiresTwoFactor) {
            this.accessToken = result.data.accessToken;
            this.refreshToken = result.data.refreshToken;
            localStorage.setItem('accessToken', this.accessToken);
            localStorage.setItem('refreshToken', this.refreshToken);
        }
        
        return result;
    }

    async makeAuthenticatedRequest(url, options = {}) {
        const headers = {
            'Authorization': `Bearer ${this.accessToken}`,
            'Content-Type': 'application/json',
            ...options.headers
        };

        let response = await fetch(`${this.baseUrl}${url}`, {
            ...options,
            headers
        });

        // If token expired, try to refresh
        if (response.status === 401 && this.refreshToken) {
            const refreshed = await this.refreshAccessToken();
            if (refreshed) {
                headers['Authorization'] = `Bearer ${this.accessToken}`;
                response = await fetch(`${this.baseUrl}${url}`, {
                    ...options,
                    headers
                });
            }
        }

        return response.json();
    }

    async refreshAccessToken() {
        try {
            const response = await fetch(`${this.baseUrl}/api/auth/refresh-token`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken: this.refreshToken })
            });

            const result = await response.json();
            
            if (result.ok) {
                this.accessToken = result.data.accessToken;
                this.refreshToken = result.data.refreshToken;
                localStorage.setItem('accessToken', this.accessToken);
                localStorage.setItem('refreshToken', this.refreshToken);
                return true;
            }
        } catch (error) {
            console.error('Token refresh failed:', error);
        }
        
        return false;
    }

    async getProjects(status = null, isPublished = null) {
        const params = new URLSearchParams();
        if (status) params.append('status', status);
        if (isPublished !== null) params.append('isPublished', isPublished);
        
        const url = `/api/projects${params.toString() ? '?' + params.toString() : ''}`;
        return this.makeAuthenticatedRequest(url);
    }

    async createProject(projectData) {
        return this.makeAuthenticatedRequest('/api/projects', {
            method: 'POST',
            body: JSON.stringify(projectData)
        });
    }

    async getContactMessages(filters = {}) {
        const params = new URLSearchParams();
        Object.keys(filters).forEach(key => {
            if (filters[key] !== null && filters[key] !== undefined) {
                params.append(key, filters[key]);
            }
        });
        
        const url = `/api/contact${params.toString() ? '?' + params.toString() : ''}`;
        return this.makeAuthenticatedRequest(url);
    }
}

// Usage Example
const api = new ElzahyApiClient();

// Login
api.login('admin@gmail.com', 'Admin12345').then(result => {
    if (result.ok) {
        console.log('Logged in successfully');
        
        // Get projects
        api.getProjects().then(projects => {
            console.log('Projects:', projects.data);
        });
    }
});
```

## Testing with Postman

### Import Collection
You can create a Postman collection with the following structure:

1. **Authentication Folder**
   - Register
   - Login
   - Refresh Token
   - Get Me
   - Setup 2FA
   - Enable 2FA

2. **Projects Folder**
   - Get All Projects
   - Get Project by ID
   - Create Project
   - Update Project
   - Delete Project

3. **Awards Folder**
   - Get All Awards
   - Create Award
   - Update Award
   - Delete Award

4. **Contact Folder**
   - Submit Contact Message
   - Get Contact Messages (Admin)
   - Mark as Read
   - Mark as Replied

### Environment Variables
Set up these Postman environment variables:
- `baseUrl`: https://localhost:7000
- `accessToken`: {{accessToken}} (auto-updated from login response)
- `refreshToken`: {{refreshToken}} (auto-updated from login response)

### Pre-request Script for Authentication
Add this to requests that require authentication:
```javascript
if (!pm.environment.get("accessToken")) {
    throw new Error("Access token not found. Please login first.");
}
```

### Test Scripts
Add this to login request to auto-save tokens:
```javascript
if (pm.response.json().ok) {
    const responseData = pm.response.json().data;
    pm.environment.set("accessToken", responseData.accessToken);
    pm.environment.set("refreshToken", responseData.refreshToken);
}
```

## Load Testing with Artillery

### Install Artillery
```bash
npm install -g artillery
```

### Create artillery-config.yml
```yaml
config:
  target: 'https://localhost:7000'
  phases:
    - duration: 60
      arrivalRate: 10
  defaults:
    headers:
      Authorization: 'Bearer YOUR_ACCESS_TOKEN'

scenarios:
  - name: "Get Projects"
    requests:
      - get:
          url: "/api/projects"
  
  - name: "Submit Contact Message"
    requests:
      - post:
          url: "/api/contact"
          json:
            fullName: "Load Test User"
            emailAddress: "loadtest@example.com"
            subject: "Load Test Message"
            message: "This is a load test message"
```

### Run Load Test
```bash
artillery run artillery-config.yml
```

## Database Testing

### Check Database State
```sql
-- Check users
SELECT Id, Email, Name, Role, TwoFactorEnabled, EmailConfirmed FROM Users;

-- Check projects by status
SELECT Name, Status, IsPublished, CreatedAt FROM Projects ORDER BY SortOrder;

-- Check contact messages
SELECT FullName, Subject, IsRead, IsReplied, CreatedAt FROM ContactMessages ORDER BY CreatedAt DESC;

-- Check refresh tokens
SELECT UserId, ExpiresAt, IsRevoked FROM RefreshTokens WHERE ExpiresAt > GETDATE();
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check if access token is valid and not expired
   - Verify the Authorization header format: `Bearer {token}`

2. **403 Forbidden**
   - Check if user has the required role (Admin for management endpoints)

3. **Email not sending**
   - Verify SMTP configuration in appsettings.json
   - Check email service logs

4. **2FA not working**
   - Ensure QR code is scanned correctly
   - Verify time synchronization between server and authenticator app

5. **Database connection issues**
   - Check connection string in appsettings.json
   - Ensure database server is running
   - Run `dotnet ef database update` if needed

### Debug Mode
Run the application in debug mode to see detailed error messages:
```bash
dotnet run --environment Development
```

### Logs
Check application logs for detailed error information:
```bash
# View console logs or check log files if file logging is configured
```