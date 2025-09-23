# Elzahy Portfolio API - Quick Reference

## ?? Quick Start

### Base URL
```
Development: https://localhost:5001/api
Production: [Your Domain]/api
```

### Authentication Header
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## ?? API Endpoints Summary

### ?? Authentication (`/api/auth`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/register` | ? | Register new user |
| POST | `/login` | ? | User login |
| POST | `/2fa/verify` | ? | Verify 2FA code |
| POST | `/2fa/verify-recovery` | ? | Use recovery code |
| POST | `/refresh-token` | ? | Refresh access token |
| POST | `/logout` | ? | Logout user |
| GET | `/me` | ? | Get current user |
| PUT | `/me` | ? | Update profile |
| POST | `/2fa/setup` | ? | Setup 2FA |
| POST | `/2fa/enable` | ? | Enable 2FA |
| POST | `/2fa/disable` | ? | Disable 2FA |
| POST | `/2fa/recovery-codes` | ? | Generate recovery codes |
| POST | `/forgot-password` | ? | Forgot password |
| POST | `/reset-password` | ? | Reset password |
| GET | `/confirm-email` | ? | Confirm email |

### ?? Projects (`/api/projects`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | ? | Get all projects |
| GET | `/{id}` | ? | Get project by ID |
| GET | `/status/{status}` | ? | Get projects by status |
| POST | `/` | ?? Admin | Create project |
| PUT | `/{id}` | ?? Admin | Update project |
| DELETE | `/{id}` | ?? Admin | Delete project |

### ?? Awards (`/api/awards`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | ? | Get all awards |
| GET | `/{id}` | ? | Get award by ID |
| POST | `/` | ?? Admin | Create award |
| PUT | `/{id}` | ?? Admin | Update award |
| DELETE | `/{id}` | ?? Admin | Delete award |

### ?? Contact (`/api/contact`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | ? | Submit contact message |
| GET | `/{id}` | ?? Admin | Get contact message |
| GET | `/` | ?? Admin | Get all messages (paginated) |
| PUT | `/{id}` | ?? Admin | Update message |
| POST | `/{id}/mark-read` | ?? Admin | Mark as read |
| POST | `/{id}/mark-replied` | ?? Admin | Mark as replied |
| DELETE | `/{id}` | ?? Admin | Delete message |

### ?? Health Check
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/health` | ? | API health status |

## ??? Common Request/Response Patterns

### Standard API Response
```json
{
  "ok": true|false,
  "data": { /* response data */ },
  "error": {
    "message": "Error description",
    "internalCode": 1001
  }
}
```

### Authentication Flow
```mermaid
graph TD
    A[Login Request] --> B{2FA Enabled?}
    B -->|No| C[Return Tokens]
    B -->|Yes| D[Return Temp Token]
    D --> E[2FA Verification]
    E --> C
    C --> F[Access Protected Routes]
```

### Login Request
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

### Login Response (No 2FA)
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "id": "guid",
      "email": "user@example.com",
      "name": "John Doe",
      "role": "User",
      "twoFactorEnabled": false
    },
    "expiresIn": 3600
  }
}
```

### Login Response (2FA Required)
```json
{
  "ok": true,
  "data": {
    "requiresTwoFactor": true,
    "tempToken": "temp_token_for_verification"
  }
}
```

## ?? Sample Frontend Code

### JavaScript/Fetch
```javascript
// Login
const login = async (email, password) => {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  return response.json();
};

// Get Projects
const getProjects = async () => {
  const token = localStorage.getItem('accessToken');
  const response = await fetch('/api/projects', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  return response.json();
};
```

### TypeScript/Axios
```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
});

// Request interceptor
api.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Usage
const { data } = await api.get<ApiResponse<Project[]>>('/projects');
if (data.ok) {
  console.log(data.data); // Project[]
}
```

### React Hook
```tsx
const useProjects = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    const fetchProjects = async () => {
      try {
        const response = await api.get('/projects');
        if (response.data.ok) {
          setProjects(response.data.data);
        }
      } catch (error) {
        console.error('Failed to fetch projects:', error);
      } finally {
        setLoading(false);
      }
    };
    
    fetchProjects();
  }, []);
  
  return { projects, loading };
};
```

## ?? Environment Variables

### Required
```bash
# JWT Secret (minimum 32 characters)
DOTNET_JWT_KEY="your-super-secret-jwt-key-here"

# Database Connection
MYSQL_CONNECTION_STRING="Server=localhost;Database=Elzahy;User=root;Password=pass;"

# Email Configuration
EMAIL__Host="smtp.gmail.com"
EMAIL__Port="587"
EMAIL__Username="your-email@gmail.com"
EMAIL__Password="your-app-password"
```

### Optional
```bash
# CORS Configuration
FRONTEND_URL="http://localhost:3000"

# Swagger (Development)
EnableSwagger="true"

# Data Protection Keys Path
DOTNET_DATAPROTECTION_KEYS="./keys"
```

## ? Quick Commands

### Development
```bash
# Start API
dotnet run --project Elzahy

# Apply migrations
dotnet ef database update --project Elzahy

# Create migration
dotnet ef migrations add MigrationName --project Elzahy
```

### Production
```bash
# Build
dotnet build --configuration Release

# Run
dotnet Elzahy.dll

# With environment
ASPNETCORE_ENVIRONMENT=Production dotnet Elzahy.dll
```

## ?? Security Notes

- ? JWT tokens expire in 1 hour (configurable)
- ? Refresh tokens for automatic renewal
- ? 2FA with TOTP (Google Authenticator compatible)
- ? Recovery codes for account recovery
- ? CORS configured for specified origins
- ? HTTPS enforced in production
- ? Input validation on all endpoints
- ? SQL injection protection via EF Core

## ?? Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET, PUT |
| 201 | Created | Successful POST |
| 400 | Bad Request | Invalid request data |
| 401 | Unauthorized | No/invalid auth token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 500 | Server Error | Internal server error |

## ?? Common Issues & Solutions

### Issue: 401 Unauthorized
**Solution:** Check if:
- Access token is included in Authorization header
- Token format is `Bearer <token>`
- Token hasn't expired (refresh if needed)

### Issue: 403 Forbidden
**Solution:** Verify:
- User has required role (Admin for management endpoints)
- Account is confirmed and active

### Issue: CORS Error
**Solution:** Ensure:
- Frontend URL is configured in CORS settings
- Using correct headers in requests
- Not sending credentials in preflight requests

### Issue: 2FA Not Working
**Solution:** Check:
- Time sync on authenticator app
- Secret key is correctly encoded
- Using 6-digit TOTP codes

## ?? Supported Authenticator Apps

- Google Authenticator
- Microsoft Authenticator  
- Authy
- 1Password
- LastPass Authenticator
- Any TOTP-compatible app

## ?? Email Templates

The API sends emails for:
- Account confirmation
- Password reset
- 2FA setup
- Security notifications

Configure SMTP settings in environment variables for production use.

## ?? Useful Links

- [Swagger UI (Development)]: http://localhost:5001
- [Health Check]: http://localhost:5001/health
- [JWT.io]: https://jwt.io (for token debugging)
- [TOTP Test]: https://totp.danhersam.com/ (for 2FA testing)

---

*This API is production-ready with comprehensive security features, proper error handling, and extensive documentation.*