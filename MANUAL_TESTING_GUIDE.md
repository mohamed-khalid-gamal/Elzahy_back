# Manual Testing Guide

This guide provides step-by-step instructions for manually testing the 2FA authentication system.

## Prerequisites

1. Start the application:
   ```bash
   cd Elzahy
   dotnet run
   ```

2. Application will be available at: `https://localhost:7000`
3. Swagger UI available at: `https://localhost:7000`

## Test Scenario 1: Basic Registration and Login (No 2FA)

### Step 1: Register a New User

**Request:**
```http
POST https://localhost:7000/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123!",
  "name": "Test User",
  "terms": true
}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "user": {
      "id": "user-guid",
      "email": "test@example.com",
      "name": "Test User",
      "twoFactorEnabled": false,
      "emailConfirmed": false
    },
    "expiresIn": 3600
  }
}
```

### Step 2: Login Without 2FA

**Request:**
```http
POST https://localhost:7000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123!"
}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "requiresTwoFactor": false,
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "user": {
      "id": "user-guid",
      "email": "test@example.com",
      "name": "Test User",
      "twoFactorEnabled": false
    },
    "expiresIn": 3600
  }
}
```

### Step 3: Access Protected Endpoint

**Request:**
```http
GET https://localhost:7000/api/auth/me
Authorization: Bearer {accessToken}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "id": "user-guid",
    "email": "test@example.com",
    "name": "Test User",
    "twoFactorEnabled": false,
    "emailConfirmed": false
  }
}
```

## Test Scenario 2: Complete 2FA Setup and Login Flow

### Step 1: Setup 2FA

**Request:**
```http
POST https://localhost:7000/api/auth/2fa/setup
Authorization: Bearer {accessToken}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "secretKey": "ABCDEFGHIJKLMNOP1234567890123456",
    "qrCodeImage": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
    "manualEntryKey": "ABCD EFGH IJKL MNOP 1234 5678 9012 3456"
  }
}
```

**Instructions:**
1. Scan the QR code with Google Authenticator, Authy, or another TOTP app
2. OR manually enter the `manualEntryKey` in your authenticator app
3. Wait for the app to generate a 6-digit code

### Step 2: Enable 2FA

**Request:**
```http
POST https://localhost:7000/api/auth/2fa/enable
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "code": "123456"
}
```
*Replace `123456` with the actual TOTP code from your authenticator app*

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "success": true,
    "recoveryCodes": [
      "123-456",
      "789-012",
      "345-678",
      "901-234",
      "567-890",
      "123-789",
      "456-012",
      "789-345",
      "012-678",
      "345-901"
    ],
    "message": "Two-factor authentication has been enabled successfully."
  }
}
```

**Important:** Save the recovery codes in a secure location. Each can only be used once.

### Step 3: Login with 2FA (First Step)

**Request:**
```http
POST https://localhost:7000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123!"
}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "requiresTwoFactor": true,
    "tempToken": "CfDJ8Nq6QqJ5K2L3M4N5O6P7Q8R9S0T1U2V3W4X5Y6Z7A8B9C0D1E2F3G4H5I6J7",
    "accessToken": "",
    "refreshToken": "",
    "expiresIn": 300
  }
}
```

**Note:** The `tempToken` is valid for 5 minutes only.

### Step 4: Verify 2FA with TOTP Code

**Request:**
```http
POST https://localhost:7000/api/auth/2fa/verify
Content-Type: application/json

{
  "tempToken": "CfDJ8Nq6QqJ5K2L3M4N5O6P7Q8R9S0T1U2V3W4X5Y6Z7A8B9C0D1E2F3G4H5I6J7",
  "code": "654321"
}
```
*Replace with the actual temp token and TOTP code*

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "user": {
      "id": "user-guid",
      "email": "test@example.com",
      "name": "Test User",
      "twoFactorEnabled": true
    },
    "expiresIn": 3600
  }
}
```

## Test Scenario 3: Recovery Code Authentication

### Step 1: Login (Get Temp Token)

Follow Step 3 from Scenario 2 to get a temp token.

### Step 2: Use Recovery Code Instead of TOTP

**Request:**
```http
POST https://localhost:7000/api/auth/2fa/verify-recovery
Content-Type: application/json

{
  "tempToken": "CfDJ8Nq6QqJ5K2L3M4N5O6P7Q8R9S0T1U2V3W4X5Y6Z7A8B9C0D1E2F3G4H5I6J7",
  "recoveryCode": "123-456"
}
```
*Use one of the recovery codes from Step 2 of Scenario 2*

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "user": {
      "id": "user-guid",
      "email": "test@example.com",
      "name": "Test User",
      "twoFactorEnabled": true
    },
    "expiresIn": 3600
  }
}
```

### Step 3: Try to Reuse the Same Recovery Code

Repeat Step 2 with the same recovery code.

**Expected Response:**
```json
{
  "ok": false,
  "error": {
    "message": "Invalid recovery code",
    "internalCode": 4001
  }
}
```

## Test Scenario 4: Security Testing

### Test 1: Account Lockout

Attempt to login with wrong password 5 times:

**Request (repeat 5 times):**
```http
POST https://localhost:7000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "WrongPassword"
}
```

**Expected Response (after 5th attempt):**
```json
{
  "ok": false,
  "error": {
    "message": "Account is locked. Try again in 15 minutes.",
    "internalCode": 4029
  }
}
```

### Test 2: Expired Temp Token

1. Get a temp token by logging in
2. Wait 6 minutes (temp tokens expire after 5 minutes)
3. Try to verify 2FA

**Expected Response:**
```json
{
  "ok": false,
  "error": {
    "message": "Invalid or expired temporary token",
    "internalCode": 4001
  }
}
```

### Test 3: Invalid TOTP Code

**Request:**
```http
POST https://localhost:7000/api/auth/2fa/verify
Content-Type: application/json

{
  "tempToken": "valid-temp-token",
  "code": "000000"
}
```

**Expected Response:**
```json
{
  "ok": false,
  "error": {
    "message": "Invalid 2FA code",
    "internalCode": 4001
  }
}
```

## Test Scenario 5: Token Management

### Test 1: Refresh Token

**Request:**
```http
POST https://localhost:7000/api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "expiresIn": 3600
  }
}
```

### Test 2: Logout

**Request:**
```http
POST https://localhost:7000/api/auth/logout
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

**Expected Response:**
```json
{
  "message": "Logged out successfully"
}
```

## Test Scenario 6: 2FA Management

### Test 1: Generate New Recovery Codes

**Request:**
```http
POST https://localhost:7000/api/auth/2fa/recovery-codes
Authorization: Bearer {accessToken}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": {
    "recoveryCodes": [
      "111-222",
      "333-444",
      "555-666",
      "777-888",
      "999-000",
      "111-333",
      "444-555",
      "666-777",
      "888-999",
      "000-111"
    ],
    "count": 10,
    "generatedAt": "2024-01-15T10:30:00.000Z"
  }
}
```

### Test 2: Disable 2FA

**Request:**
```http
POST https://localhost:7000/api/auth/2fa/disable
Authorization: Bearer {accessToken}
```

**Expected Response:**
```json
{
  "ok": true,
  "data": true
}
```

**Verification:** Login again - should not require 2FA anymore.

## Common Error Responses

### 400 Bad Request
```json
{
  "ok": false,
  "error": {
    "message": "Validation error message",
    "internalCode": 4000
  }
}
```

### 401 Unauthorized
```json
{
  "ok": false,
  "error": {
    "message": "Invalid credentials",
    "internalCode": 4001
  }
}
```

### 404 Not Found
```json
{
  "ok": false,
  "error": {
    "message": "User not found",
    "internalCode": 4004
  }
}
```

### 500 Internal Server Error
```json
{
  "ok": false,
  "error": {
    "message": "Internal server error",
    "internalCode": 5000
  }
}
```

## Expected Test Results Summary

? **Pass Criteria:**
- User registration and login work without 2FA
- 2FA setup generates QR code and secret
- TOTP codes from authenticator apps are accepted
- Recovery codes work and become single-use
- Account lockout triggers after 5 failed attempts
- Temp tokens expire after 5 minutes
- Invalid codes are rejected
- Tokens can be refreshed
- 2FA can be disabled

? **Fail Indicators:**
- 500 errors on any endpoint
- QR codes not scannable by authenticator apps
- Valid TOTP codes rejected
- Recovery codes can be reused
- No account lockout after failed attempts
- Temp tokens don't expire
- Unauthorized access to protected endpoints

## Tools for Testing

1. **Postman**: Import `Elzahy.postman_collection.json`
2. **Curl**: Use command line for automation
3. **Swagger UI**: Interactive testing at `https://localhost:7000`
4. **Authenticator Apps**: Google Authenticator, Authy, Microsoft Authenticator
5. **Browser Developer Tools**: Inspect requests/responses