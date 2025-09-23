# Elzahy Portfolio API Documentation

## Table of Contents
1. [Overview](#overview)
2. [Base URL & Environment](#base-url--environment)
3. [Authentication](#authentication)
4. [Error Handling](#error-handling)
5. [API Endpoints](#api-endpoints)
   - [Authentication & User Management](#authentication--user-management)
   - [Projects Management](#projects-management)
   - [Awards Management](#awards-management)
   - [Contact Messages](#contact-messages)
   - [Health Check](#health-check)
6. [Data Models](#data-models)
7. [Frontend Integration Guide](#frontend-integration-guide)
8. [Security Features](#security-features)
9. [Environment Configuration](#environment-configuration)

## Overview

The Elzahy Portfolio API is a production-ready .NET 8 Web API designed for a personal portfolio website. It includes comprehensive authentication with 2FA support, project management, awards tracking, and contact form handling.

### Key Features
- **JWT-based Authentication** with refresh tokens
- **Two-Factor Authentication (2FA)** with TOTP and email verification
- **Role-based Authorization** (Admin/User roles)
- **Project Portfolio Management**
- **Awards & Certifications Management**
- **Contact Form Processing**
- **Email Services** for notifications and 2FA
- **Comprehensive Error Handling**
- **Swagger Documentation** (enabled in development)
- **CORS Support** for frontend integration

## Base URL & Environment

### Development
```
Base URL: https://localhost:5001/api
Swagger UI: https://localhost:5001/
Health Check: https://localhost:5001/health
```

### Production
```
Base URL: [Your Production Domain]/api
Health Check: [Your Production Domain]/health
```

## Authentication

The API uses JWT (JSON Web Tokens) with refresh tokens for authentication. All authenticated endpoints require the `Authorization` header with a Bearer token.

### Authentication Flow
1. Register or Login to get access and refresh tokens
2. Use access token in Authorization header: `Bearer {token}`
3. When access token expires, use refresh token to get new tokens
4. For 2FA-enabled users, complete the two-factor verification

### Headers
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

## Error Handling

All API responses follow a consistent format using the `ApiResponse<T>` wrapper:

### Success Response
```json
{
  "ok": true,
  "data": { /* response data */ },
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
    "internalCode": 1001
  }
}
```

### HTTP Status Codes
- `200 OK` - Successful GET, PUT requests
- `201 Created` - Successful POST requests
- `400 Bad Request` - Invalid request data or validation errors
- `401 Unauthorized` - Authentication required or invalid
- `403 Forbidden` - Insufficient permissions (Admin role required)
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## API Endpoints

### Authentication & User Management

#### Register User
**POST** `/api/auth/register`

Registers a new user account and sends email confirmation.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "name": "John Doe",
  "terms": true
}
```

**Response:**
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "createdAt": "2024-01-15T10:30:00.000Z",
      "updatedAt": "2024-01-15T10:30:00.000Z",
      "email": "user@example.com",
      "name": "John Doe",
      "language": "en",
      "role": "User",
      "twoFactorEnabled": false,
      "emailConfirmed": false
    },
    "requiresTwoFactor": false,
    "tempToken": null,
    "expiresIn": 3600
  }
}
```

#### Login
**POST** `/api/auth/login`

Authenticates a user and returns tokens. If 2FA is enabled, may require additional verification.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "twoFactorCode": "123456"  // Optional, required if 2FA is enabled
}
```

**Response (2FA Not Required):**
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "user": { /* UserDto object */ },
    "requiresTwoFactor": false,
    "tempToken": null,
    "expiresIn": 3600
  }
}
```

**Response (2FA Required):**
```json
{
  "ok": true,
  "data": {
    "accessToken": "",
    "refreshToken": "",
    "user": null,
    "requiresTwoFactor": true,
    "tempToken": "temp_token_for_2fa_verification",
    "expiresIn": 0
  }
}
```

#### Verify Two-Factor Authentication
**POST** `/api/auth/2fa/verify`

Completes 2FA verification during login using TOTP code.

**Request Body:**
```json
{
  "tempToken": "temp_token_from_login",
  "code": "123456"
}
```

**Response:**
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "user": { /* UserDto object */ },
    "requiresTwoFactor": false,
    "tempToken": null,
    "expiresIn": 3600
  }
}
```

#### Verify Recovery Code
**POST** `/api/auth/2fa/verify-recovery`

Uses a recovery code for 2FA verification when TOTP is unavailable.

**Request Body:**
```json
{
  "tempToken": "temp_token_from_login",
  "recoveryCode": "abcd-efgh-ijkl"
}
```

#### Refresh Token
**POST** `/api/auth/refresh-token`

Refreshes the access token using the refresh token.

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response:**
```json
{
  "ok": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 3600
  }
}
```

#### Logout
**POST** `/api/auth/logout`
**Authentication Required**

Invalidates the refresh token on the server.

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response:**
```json
{
  "message": "Logged out successfully"
}
```

#### Get Current User
**GET** `/api/auth/me`
**Authentication Required**

Returns current user information.

**Response:**
```json
{
  "ok": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T10:30:00.000Z",
    "email": "user@example.com",
    "name": "John Doe",
    "language": "en",
    "role": "User",
    "twoFactorEnabled": false,
    "emailConfirmed": true
  }
}
```

#### Update Profile
**PUT** `/api/auth/me`
**Authentication Required**

Updates user profile information.

**Request Body:**
```json
{
  "name": "Updated Name",
  "language": "en"
}
```

#### Setup Two-Factor Authentication
**POST** `/api/auth/2fa/setup`
**Authentication Required**

Generates 2FA setup information including QR code for authenticator apps.

**Response:**
```json
{
  "ok": true,
  "data": {
    "secretKey": "JBSWY3DPEHPK3PXP",
    "qrCodeImage": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
    "manualEntryKey": "JBSW Y3DP EHPK 3PXP"
  }
}
```

#### Enable Two-Factor Authentication
**POST** `/api/auth/2fa/enable`
**Authentication Required**

Enables 2FA after verifying the setup code from authenticator app.

**Request Body:**
```json
{
  "code": "123456"
}
```

**Response:**
```json
{
  "ok": true,
  "data": {
    "success": true,
    "recoveryCodes": [
      "abcd-efgh-ijkl",
      "mnop-qrst-uvwx",
      "yzab-cdef-ghij"
    ],
    "message": "Two-factor authentication enabled successfully"
  }
}
```

#### Disable Two-Factor Authentication
**POST** `/api/auth/2fa/disable`
**Authentication Required**

Disables 2FA for the user.

**Response:**
```json
{
  "ok": true,
  "data": true
}
```

#### Generate New Recovery Codes
**POST** `/api/auth/2fa/recovery-codes`
**Authentication Required**

Generates new recovery codes for 2FA (invalidates previous codes).

**Response:**
```json
{
  "ok": true,
  "data": {
    "recoveryCodes": [
      "abcd-efgh-ijkl",
      "mnop-qrst-uvwx",
      "yzab-cdef-ghij"
    ],
    "count": 3,
    "generatedAt": "2024-01-15T10:30:00.000Z"
  }
}
```

#### Forgot Password
**POST** `/api/auth/forgot-password`

Initiates password reset process by sending reset email.

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "ok": true,
  "data": true
}
```

#### Reset Password
**POST** `/api/auth/reset-password`

Resets password using the token from email.

**Request Body:**
```json
{
  "token": "reset_token_from_email",
  "newPassword": "NewSecurePassword123!"
}
```

**Response:**
```json
{
  "ok": true,
  "data": true
}
```

#### Confirm Email
**GET** `/api/auth/confirm-email?token=confirmation_token`

Confirms user email address using token from registration email.

**Response:**
```json
{
  "ok": true,
  "data": true
}
```

### Projects Management

#### Get All Projects
**GET** `/api/projects`

Retrieves all projects with optional filtering.

**Query Parameters:**
- `status` (optional): `Current`, `Future`, or `Past`
- `isPublished` (optional): `true` or `false`

**Response:**
```json
{
  "ok": true,
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "createdAt": "2024-01-15T10:30:00.000Z",
      "updatedAt": "2024-01-15T10:30:00.000Z",
      "name": "E-Commerce Platform",
      "description": "A modern e-commerce solution built with .NET and Angular",
      "photoUrl": "https://example.com/photo.jpg",
      "status": "Current",
      "technologiesUsed": ".NET 8, Angular 17, SQL Server",
      "projectUrl": "https://project.example.com",
      "gitHubUrl": "https://github.com/user/project",
      "startDate": "2024-01-15T00:00:00Z",
      "endDate": null,
      "client": "ABC Company",
      "budget": 50000.00,
      "isPublished": true,
      "sortOrder": 1,
      "createdByName": "Admin User"
    }
  ]
}
```

#### Get Project by ID
**GET** `/api/projects/{id}`

Retrieves a specific project by ID.

**Response:**
```json
{
  "ok": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T10:30:00.000Z",
    "name": "Portfolio Website",
    "description": "Personal portfolio showcasing projects and skills",
    "photoUrl": "https://example.com/portfolio.jpg",
    "status": "Current",
    "technologiesUsed": ".NET 8, Angular 17",
    "projectUrl": "https://portfolio.example.com",
    "gitHubUrl": "https://github.com/user/portfolio",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": null,
    "client": null,
    "budget": null,
    "isPublished": true,
    "sortOrder": 1,
    "createdByName": "Admin User"
  }
}
```

#### Get Projects by Status
**GET** `/api/projects/status/{status}`

Retrieves projects filtered by status (`Current`, `Future`, or `Past`).

#### Create Project
**POST** `/api/projects`
**Authentication Required (Admin Role)**

Creates a new project.

**Request Body:**
```json
{
  "name": "New Project",
  "description": "Project description",
  "photoUrl": "https://example.com/photo.jpg",
  "status": "Current",
  "technologiesUsed": ".NET 8, React",
  "projectUrl": "https://project.example.com",
  "gitHubUrl": "https://github.com/user/project",
  "startDate": "2024-01-15T00:00:00Z",
  "endDate": null,
  "client": "Client Name",
  "budget": 25000.00,
  "isPublished": true,
  "sortOrder": 1
}
```

**Response:** Returns 201 Created with the created project data.

#### Update Project
**PUT** `/api/projects/{id}`
**Authentication Required (Admin Role)**

Updates an existing project. All fields are optional.

**Request Body:**
```json
{
  "name": "Updated Project Name",
  "description": "Updated description",
  "status": "Past",
  "isPublished": false
}
```

#### Delete Project
**DELETE** `/api/projects/{id}`
**Authentication Required (Admin Role)**

Deletes a project.

**Response:**
```json
{
  "ok": true,
  "data": true
}
```

### Awards Management

#### Get All Awards
**GET** `/api/awards`

Retrieves all awards with optional filtering.

**Query Parameters:**
- `isPublished` (optional): `true` or `false`

**Response:**
```json
{
  "ok": true,
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "createdAt": "2024-03-15T10:30:00.000Z",
      "updatedAt": "2024-03-15T10:30:00.000Z",
      "name": "Best Developer Award",
      "givenBy": "Tech Conference 2024",
      "dateReceived": "2024-03-15T00:00:00Z",
      "description": "Awarded for outstanding contribution to web development",
      "certificateUrl": "https://example.com/certificate.pdf",
      "imageUrl": "https://example.com/award.jpg",
      "isPublished": true,
      "sortOrder": 1,
      "createdByName": "Admin User"
    }
  ]
}
```

#### Get Award by ID
**GET** `/api/awards/{id}`

Retrieves a specific award by ID.

#### Create Award
**POST** `/api/awards`
**Authentication Required (Admin Role)**

Creates a new award.

**Request Body:**
```json
{
  "name": "Certificate Name",
  "givenBy": "Issuing Organization",
  "dateReceived": "2024-03-15T00:00:00Z",
  "description": "Award description",
  "certificateUrl": "https://example.com/certificate.pdf",
  "imageUrl": "https://example.com/award.jpg",
  "isPublished": true,
  "sortOrder": 1
}
```

#### Update Award
**PUT** `/api/awards/{id}`
**Authentication Required (Admin Role)**

Updates an existing award. All fields are optional.

#### Delete Award
**DELETE** `/api/awards/{id}`
**Authentication Required (Admin Role)**

Deletes an award.

### Contact Messages

#### Submit Contact Message
**POST** `/api/contact`

Submits a new contact message from visitors.

**Request Body:**
```json
{
  "fullName": "John Doe",
  "emailAddress": "john@example.com",
  "subject": "Project Inquiry",
  "message": "I'm interested in discussing a project...",
  "phoneNumber": "+1234567890",
  "company": "ABC Corp"
}
```

**Response:** Returns 201 Created with the contact message data.

#### Get Contact Message
**GET** `/api/contact/{id}`
**Authentication Required (Admin Role)**

Retrieves a specific contact message.

#### Get All Contact Messages
**GET** `/api/contact`
**Authentication Required (Admin Role)**

Retrieves contact messages with filtering and pagination.

**Query Parameters (ContactMessageFilterDto):**
- `fromDate` (optional): Filter from date (ISO format)
- `toDate` (optional): Filter to date (ISO format)
- `isRead` (optional): Filter by read status
- `isReplied` (optional): Filter by reply status
- `sortBy` (optional): Sort field (`CreatedAt`, `Subject`, `FullName`) - default: `CreatedAt`
- `sortDescending` (optional): Sort direction - default: `true`
- `page` (optional): Page number - default: `1`
- `pageSize` (optional): Page size - default: `10`

**Response:**
```json
{
  "ok": true,
  "data": {
    "data": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "createdAt": "2024-01-15T10:30:00.000Z",
        "fullName": "John Doe",
        "emailAddress": "john@example.com",
        "subject": "Project Inquiry",
        "message": "I'm interested in discussing a project...",
        "isRead": false,
        "isReplied": false,
        "readAt": null,
        "repliedAt": null,
        "phoneNumber": "+1234567890",
        "company": "ABC Corp",
        "adminNotes": null
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

#### Update Contact Message
**PUT** `/api/contact/{id}`
**Authentication Required (Admin Role)**

Updates contact message status and admin notes.

**Request Body:**
```json
{
  "isRead": true,
  "isReplied": true,
  "adminNotes": "Follow up scheduled for next week"
}
```

#### Mark as Read
**POST** `/api/contact/{id}/mark-read`
**Authentication Required (Admin Role)**

Marks a contact message as read and sets `readAt` timestamp.

#### Mark as Replied
**POST** `/api/contact/{id}/mark-replied`
**Authentication Required (Admin Role)**

Marks a contact message as replied and sets `repliedAt` timestamp.

#### Delete Contact Message
**DELETE** `/api/contact/{id}`
**Authentication Required (Admin Role)**

Deletes a contact message.

### Health Check

#### Health Check
**GET** `/health`

Returns API health status.

**Response:**
```json
{
  "status": "Healthy",
  "environment": "Development",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

## Data Models

### Core Response Types

#### ApiResponse<T>
```typescript
interface ApiResponse<T> {
  ok: boolean;
  data?: T;
  error?: ErrorDetails;
}

interface ErrorDetails {
  message: string;
  internalCode?: number;
}
```

#### PagedResponse<T>
```typescript
interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
```

### Authentication Models

#### UserDto
```typescript
interface UserDto {
  id: string;
  createdAt: string;
  updatedAt: string;
  email: string;
  name: string;
  language: string;
  role: string; // "Admin" | "User"
  twoFactorEnabled: boolean;
  emailConfirmed: boolean;
}
```

#### AuthResponseDto
```typescript
interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  user: UserDto | null;
  requiresTwoFactor: boolean;
  tempToken: string | null;
  expiresIn: number;
}
```

#### TokenRefreshResponseDto
```typescript
interface TokenRefreshResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}
```

#### Setup2FAResponseDto
```typescript
interface Setup2FAResponseDto {
  secretKey: string;
  qrCodeImage: string; // Base64 encoded PNG
  manualEntryKey: string; // Formatted for manual entry
}
```

#### Enable2FAResponseDto
```typescript
interface Enable2FAResponseDto {
  success: boolean;
  recoveryCodes: string[];
  message: string;
}
```

#### RecoveryCodesResponseDto
```typescript
interface RecoveryCodesResponseDto {
  recoveryCodes: string[];
  count: number;
  generatedAt: string;
}
```

### Project Models

#### ProjectStatus Enum
```typescript
enum ProjectStatus {
  Current = "Current",
  Future = "Future", 
  Past = "Past"
}
```

#### ProjectDto
```typescript
interface ProjectDto {
  id: string;
  createdAt: string;
  updatedAt: string;
  name: string;
  description: string;
  photoUrl?: string;
  status: ProjectStatus;
  technologiesUsed?: string;
  projectUrl?: string;
  gitHubUrl?: string;
  startDate?: string;
  endDate?: string;
  client?: string;
  budget?: number;
  isPublished: boolean;
  sortOrder: number;
  createdByName?: string;
}
```

### Award Models

#### AwardDto
```typescript
interface AwardDto {
  id: string;
  createdAt: string;
  updatedAt: string;
  name: string;
  givenBy: string;
  dateReceived: string;
  description?: string;
  certificateUrl?: string;
  imageUrl?: string;
  isPublished: boolean;
  sortOrder: number;
  createdByName?: string;
}
```

### Contact Models

#### ContactMessageDto
```typescript
interface ContactMessageDto {
  id: string;
  createdAt: string;
  fullName: string;
  emailAddress: string;
  subject: string;
  message: string;
  isRead: boolean;
  isReplied: boolean;
  readAt?: string;
  repliedAt?: string;
  phoneNumber?: string;
  company?: string;
  adminNotes?: string;
}
```

#### ContactMessageFilterDto
```typescript
interface ContactMessageFilterDto {
  fromDate?: string;
  toDate?: string;
  isRead?: boolean;
  isReplied?: boolean;
  sortBy?: "CreatedAt" | "Subject" | "FullName";
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}
```

### Request DTOs

#### Register/Login
```typescript
interface RegisterRequestDto {
  email: string;
  password: string; // Min 6 characters
  name: string; // Max 100 characters
  terms: boolean; // Must be true
}

interface LoginRequestDto {
  email: string;
  password: string;
  twoFactorCode?: string; // 6 digits when 2FA required
}
```

#### Project Management
```typescript
interface CreateProjectRequestDto {
  name: string; // Required, max 200 chars
  description: string; // Required
  photoUrl?: string;
  status: ProjectStatus; // Required
  technologiesUsed?: string;
  projectUrl?: string;
  gitHubUrl?: string;
  startDate?: string;
  endDate?: string;
  client?: string;
  budget?: number;
  isPublished?: boolean; // Default: true
  sortOrder?: number; // Default: 0
}

interface UpdateProjectRequestDto {
  name?: string;
  description?: string;
  photoUrl?: string;
  status?: ProjectStatus;
  technologiesUsed?: string;
  projectUrl?: string;
  gitHubUrl?: string;
  startDate?: string;
  endDate?: string;
  client?: string;
  budget?: number;
  isPublished?: boolean;
  sortOrder?: number;
}
```

#### Award Management
```typescript
interface CreateAwardRequestDto {
  name: string; // Required, max 200 chars
  givenBy: string; // Required, max 200 chars
  dateReceived: string; // Required, ISO date
  description?: string;
  certificateUrl?: string;
  imageUrl?: string;
  isPublished?: boolean; // Default: true
  sortOrder?: number; // Default: 0
}

interface UpdateAwardRequestDto {
  name?: string;
  givenBy?: string;
  dateReceived?: string;
  description?: string;
  certificateUrl?: string;
  imageUrl?: string;
  isPublished?: boolean;
  sortOrder?: number;
}
```

#### Contact Management
```typescript
interface CreateContactMessageRequestDto {
  fullName: string; // Required, max 100 chars
  emailAddress: string; // Required, valid email, max 255 chars
  subject: string; // Required, max 200 chars
  message: string; // Required
  phoneNumber?: string;
  company?: string;
}

interface UpdateContactMessageRequestDto {
  isRead?: boolean;
  isReplied?: boolean;
  adminNotes?: string;
}
```

## Frontend Integration Guide

### Setting Up HTTP Client

#### Angular/TypeScript Example
```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'https://localhost:5001/api';
  
  constructor(private http: HttpClient) {}
  
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('accessToken');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` })
    });
  }
  
  // Authentication
  login(credentials: LoginRequestDto): Observable<ApiResponse<AuthResponseDto>> {
    return this.http.post<ApiResponse<AuthResponseDto>>(
      `${this.baseUrl}/auth/login`, 
      credentials,
      { headers: this.getHeaders() }
    );
  }
  
  refreshToken(refreshToken: string): Observable<ApiResponse<TokenRefreshResponseDto>> {
    return this.http.post<ApiResponse<TokenRefreshResponseDto>>(
      `${this.baseUrl}/auth/refresh-token`,
      { refreshToken },
      { headers: this.getHeaders() }
    );
  }
  
  // Projects
  getProjects(status?: ProjectStatus, isPublished?: boolean): Observable<ApiResponse<ProjectDto[]>> {
    let params: any = {};
    if (status) params.status = status;
    if (isPublished !== undefined) params.isPublished = isPublished;
    
    return this.http.get<ApiResponse<ProjectDto[]>>(
      `${this.baseUrl}/projects`,
      { headers: this.getHeaders(), params }
    );
  }
  
  createProject(project: CreateProjectRequestDto): Observable<ApiResponse<ProjectDto>> {
    return this.http.post<ApiResponse<ProjectDto>>(
      `${this.baseUrl}/projects`,
      project,
      { headers: this.getHeaders() }
    );
  }
  
  // Contact
  submitContact(message: CreateContactMessageRequestDto): Observable<ApiResponse<ContactMessageDto>> {
    return this.http.post<ApiResponse<ContactMessageDto>>(
      `${this.baseUrl}/contact`,
      message,
      { headers: this.getHeaders() }
    );
  }
  
  getContactMessages(filter: ContactMessageFilterDto): Observable<ApiResponse<PagedResponse<ContactMessageDto>>> {
    return this.http.get<ApiResponse<PagedResponse<ContactMessageDto>>>(
      `${this.baseUrl}/contact`,
      { headers: this.getHeaders(), params: filter as any }
    );
  }
}
```

### Authentication Flow Implementation

#### 1. Complete Login Process with 2FA
```typescript
async login(email: string, password: string, twoFactorCode?: string) {
  try {
    const response = await this.apiService.login({ email, password, twoFactorCode }).toPromise();
    
    if (response.ok) {
      if (response.data.requiresTwoFactor) {
        // Show 2FA form and store temp token
        this.showTwoFactorForm(response.data.tempToken);
        return { requiresTwoFactor: true };
      } else {
        // Login successful - store tokens
        this.storeTokens(response.data);
        this.navigateToDashboard();
        return { success: true };
      }
    } else {
      this.showError(response.error.message);
      return { error: response.error.message };
    }
  } catch (error) {
    this.showError('Login failed');
    return { error: 'Login failed' };
  }
}

// Complete 2FA verification
async verifyTwoFactor(tempToken: string, code: string) {
  try {
    const response = await this.apiService.verifyTwoFactor({ tempToken, code }).toPromise();
    
    if (response.ok) {
      this.storeTokens(response.data);
      this.navigateToDashboard();
      return { success: true };
    } else {
      this.showError(response.error.message);
      return { error: response.error.message };
    }
  } catch (error) {
    this.showError('Verification failed');
    return { error: 'Verification failed' };
  }
}
```

#### 2. Token Management Service
```typescript
class TokenService {
  storeTokens(authData: AuthResponseDto) {
    localStorage.setItem('accessToken', authData.accessToken);
    localStorage.setItem('refreshToken', authData.refreshToken);
    localStorage.setItem('user', JSON.stringify(authData.user));
  }
  
  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }
  
  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }
  
  getCurrentUser(): UserDto | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }
  
  async refreshToken(): Promise<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) return false;
    
    try {
      const response = await this.apiService.refreshToken(refreshToken).toPromise();
      if (response.ok) {
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        return true;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
    }
    
    this.clearTokens();
    return false;
  }
  
  clearTokens() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }
  
  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }
  
  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Admin';
  }
}
```

#### 3. HTTP Interceptor for Automatic Token Refresh
```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private tokenService: TokenService,
    private router: Router
  ) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !req.url.includes('/auth/login') && !req.url.includes('/auth/refresh-token')) {
          return this.handle401Error(req, next);
        }
        return throwError(error);
      })
    );
  }
  
  private handle401Error(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return from(this.tokenService.refreshToken()).pipe(
      switchMap((success: boolean) => {
        if (success) {
          const token = this.tokenService.getAccessToken();
          const authReq = req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
          });
          return next.handle(authReq);
        } else {
          this.tokenService.clearTokens();
          this.router.navigate(['/login']);
          return throwError('Token refresh failed');
        }
      })
    );
  }
}
```

### Error Handling Best Practices

#### Global Error Handler
```typescript
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private notificationService: NotificationService) {}
  
  handleError(error: any): void {
    console.error('Global error:', error);
    
    if (error instanceof HttpErrorResponse) {
      this.handleHttpError(error);
    } else {
      this.notificationService.showError('An unexpected error occurred');
    }
  }
  
  private handleHttpError(error: HttpErrorResponse) {
    const errorMessage = this.extractErrorMessage(error);
    
    switch (error.status) {
      case 400:
        this.notificationService.showError(errorMessage);
        break;
      case 401:
        this.notificationService.showError('Authentication required');
        break;
      case 403:
        this.notificationService.showError('Access denied - Admin role required');
        break;
      case 404:
        this.notificationService.showError('Resource not found');
        break;
      case 500:
        this.notificationService.showError('Server error. Please try again later.');
        break;
      default:
        this.notificationService.showError('Network error. Please check your connection.');
    }
  }
  
  private extractErrorMessage(error: HttpErrorResponse): string {
    // Extract message from ApiResponse error format
    if (error.error?.error?.message) {
      return error.error.error.message;
    }
    // Fallback to standard error message
    return error.message || 'Request failed';
  }
}
```

## Security Features

### 1. JWT Token Security
- **Access tokens** expire in 1 hour (configurable via JwtSettings:ExpirationMinutes)
- **Refresh tokens** for secure token renewal (expire in 7 days by default)
- **Token blacklisting** on logout (refresh tokens invalidated on server)
- **Secure token storage** using HTTP-only cookies recommended for production

### 2. Two-Factor Authentication (2FA)
- **TOTP-based** (Time-based One-Time Password) using industry standards
- **QR code generation** for easy setup with authenticator apps
- **Recovery codes** for account recovery when TOTP device is unavailable
- **App compatibility** with Google Authenticator, Authy, Microsoft Authenticator, etc.
- **Temp tokens** for secure 2FA verification flow

### 3. Password Security
- **Minimum requirements**: 6+ characters (configurable)
- **Secure hashing** with bcrypt and salt
- **Password reset** via secure email tokens with expiration
- **Account lockout** protection available through configuration

### 4. API Security
- **CORS configuration** restricts cross-origin requests to allowed domains
- **Security headers** (XSS protection, content type options, frame options)
- **HTTPS enforcement** in production environments
- **Bearer token authentication** with proper validation
- **Role-based authorization** (Admin/User roles)

### 5. Data Protection
- **SQL injection prevention** via Entity Framework parameterized queries
- **Input validation** on all endpoints using Data Annotations
- **XSS protection** through proper header configuration
- **Data encryption** for sensitive information in database
- **Environment variable protection** for secrets

## Environment Configuration

### Required Environment Variables

```bash
# Database Connection
MYSQL_CONNECTION_STRING="Server=localhost;Database=Elzahy;User=root;Password=yourpassword;"
# OR for SQL Server
ConnectionStrings__DefaultConnection="Server=(localdb)\\MSSQLLocalDB;Database=Elzahy;Trusted_Connection=True;"

# JWT Settings
DOTNET_JWT_KEY="your-super-secret-jwt-key-minimum-32-characters-long"
# OR
JWT__Key="your-super-secret-jwt-key-minimum-32-characters-long"

# Email Settings (SMTP)
EMAIL__Host="smtp.gmail.com"
EMAIL__Port="587"
EMAIL__Username="your-email@gmail.com"
EMAIL__Password="your-app-password"
EMAIL__FromEmail="noreply@yourdomain.com"
EMAIL__FromName="Elzahy Portfolio"

# Application Settings
FRONTEND_URL="http://localhost:4200"
# OR
App__FrontendUrl="http://localhost:4200"

# Data Protection Keys
DOTNET_DATAPROTECTION_KEYS="./keys"
# OR
DataProtection__KeysPath="./keys"

# Production Settings
ASPNETCORE_ENVIRONMENT="Production"
EnableSwagger="false"
```

### appsettings.json Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Elzahy": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Elzahy;Trusted_Connection=True;",
    "MySqlConnection": "Server=localhost;Database=Elzahy;User=root;Password=yourpassword;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-minimum-32-characters-long",
    "Issuer": "ElzahyPortfolioAPI",
    "Audience": "ElzahyPortfolioUsers",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Elzahy Portfolio"
  },
  "App": {
    "FrontendUrl": "http://localhost:4200",
    "DefaultAdminEmail": "admin@elzahy.com",
    "DefaultAdminPassword": "Admin123!",
    "DefaultAdminName": "Administrator"
  },
  "DataProtection": {
    "KeysPath": "./keys"
  },
  "EnableSwagger": false
}
```

### Production Deployment Checklist

1. **Environment Variables**
   - [ ] Set secure JWT secret key (minimum 32 characters)
   - [ ] Configure production database connection string
   - [ ] Set up email SMTP settings with app passwords
   - [ ] Configure correct frontend URL for CORS

2. **Security Configuration**
   - [ ] Enable HTTPS and HTTP-to-HTTPS redirection
   - [ ] Configure secure CORS origins
   - [ ] Disable Swagger UI in production (`EnableSwagger: false`)
   - [ ] Set up security headers middleware
   - [ ] Configure Data Protection keys persistence

3. **Database Setup**
   - [ ] Run Entity Framework migrations: `dotnet ef database update`
   - [ ] Verify default admin user seeding works
   - [ ] Set up automated database backups
   - [ ] Test database connection and performance

4. **Monitoring & Logging**
   - [ ] Configure structured logging (Serilog recommended)
   - [ ] Set up health check monitoring
   - [ ] Configure error tracking (Application Insights, etc.)
   - [ ] Monitor API performance and response times
   - [ ] Set up alerts for failed authentication attempts

5. **Testing**
   - [ ] Test all authentication flows including 2FA
   - [ ] Verify role-based authorization works correctly
   - [ ] Test token refresh functionality
   - [ ] Validate email sending for password reset and 2FA
   - [ ] Test CORS configuration with actual frontend

This comprehensive documentation accurately reflects your actual API implementation and provides all the necessary information for frontend developers to integrate successfully with your Elzahy Portfolio API.