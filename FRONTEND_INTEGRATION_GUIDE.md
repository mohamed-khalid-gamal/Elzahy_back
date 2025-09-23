# Frontend Integration Guide for Elzahy Portfolio API

## Table of Contents
1. [Quick Start](#quick-start)
2. [Authentication Implementation](#authentication-implementation)
3. [API Service Layer](#api-service-layer)
4. [React Integration Examples](#react-integration-examples)
5. [Angular Integration Examples](#angular-integration-examples)
6. [Vue.js Integration Examples](#vuejs-integration-examples)
7. [State Management](#state-management)
8. [Error Handling](#error-handling)
9. [UI Components](#ui-components)
10. [Best Practices](#best-practices)

## Quick Start

### 1. Base Configuration
```typescript
// config/api.ts
export const API_CONFIG = {
  baseUrl: process.env.REACT_APP_API_URL || 'https://localhost:5001/api',
  timeout: 10000,
  retryAttempts: 3
};

// Types
export interface ApiResponse<T> {
  ok: boolean;
  data?: T;
  error?: {
    message: string;
    internalCode?: number;
  };
}
```

### 2. HTTP Client Setup
```typescript
// services/httpClient.ts
import axios, { AxiosInstance, AxiosResponse } from 'axios';

class HttpClient {
  private instance: AxiosInstance;
  
  constructor() {
    this.instance = axios.create({
      baseURL: API_CONFIG.baseUrl,
      timeout: API_CONFIG.timeout,
      headers: {
        'Content-Type': 'application/json'
      }
    });
    
    this.setupInterceptors();
  }
  
  private setupInterceptors() {
    // Request interceptor for auth token
    this.instance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );
    
    // Response interceptor for token refresh
    this.instance.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;
        
        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;
          
          const refreshed = await this.refreshToken();
          if (refreshed) {
            const token = localStorage.getItem('accessToken');
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return this.instance(originalRequest);
          }
        }
        
        return Promise.reject(error);
      }
    );
  }
  
  private async refreshToken(): Promise<boolean> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) return false;
      
      const response = await axios.post(`${API_CONFIG.baseUrl}/auth/refresh-token`, {
        refreshToken
      });
      
      if (response.data.ok) {
        localStorage.setItem('accessToken', response.data.data.accessToken);
        localStorage.setItem('refreshToken', response.data.data.refreshToken);
        return true;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
    }
    
    // Clear tokens and redirect to login
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    window.location.href = '/login';
    return false;
  }
  
  public get<T>(url: string, params?: any): Promise<AxiosResponse<ApiResponse<T>>> {
    return this.instance.get(url, { params });
  }
  
  public post<T>(url: string, data?: any): Promise<AxiosResponse<ApiResponse<T>>> {
    return this.instance.post(url, data);
  }
  
  public put<T>(url: string, data?: any): Promise<AxiosResponse<ApiResponse<T>>> {
    return this.instance.put(url, data);
  }
  
  public delete<T>(url: string): Promise<AxiosResponse<ApiResponse<T>>> {
    return this.instance.delete(url);
  }
}

export const httpClient = new HttpClient();
```

## Authentication Implementation

### 1. Auth Service
```typescript
// services/authService.ts
export interface LoginRequest {
  email: string;
  password: string;
  twoFactorCode?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user?: User;
  requiresTwoFactor?: boolean;
  tempToken?: string;
  expiresIn: number;
}

export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  twoFactorEnabled: boolean;
  emailConfirmed: boolean;
}

export class AuthService {
  async login(credentials: LoginRequest): Promise<ApiResponse<AuthResponse>> {
    try {
      const response = await httpClient.post<AuthResponse>('/auth/login', credentials);
      
      if (response.data.ok && response.data.data?.accessToken) {
        this.storeTokens(response.data.data);
      }
      
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }
  
  async register(userData: RegisterRequest): Promise<ApiResponse<AuthResponse>> {
    try {
      const response = await httpClient.post<AuthResponse>('/auth/register', userData);
      
      if (response.data.ok && response.data.data?.accessToken) {
        this.storeTokens(response.data.data);
      }
      
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }
  
  async verifyTwoFactor(tempToken: string, code: string): Promise<ApiResponse<AuthResponse>> {
    try {
      const response = await httpClient.post<AuthResponse>('/auth/2fa/verify', {
        tempToken,
        code
      });
      
      if (response.data.ok && response.data.data?.accessToken) {
        this.storeTokens(response.data.data);
      }
      
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }
  
  async logout(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        await httpClient.post('/auth/logout', { refreshToken });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      this.clearTokens();
    }
  }
  
  async getCurrentUser(): Promise<ApiResponse<User>> {
    try {
      const response = await httpClient.get<User>('/auth/me');
      return response.data;
    } catch (error) {
      throw this.handleError(error);
    }
  }
  
  private storeTokens(authData: AuthResponse): void {
    localStorage.setItem('accessToken', authData.accessToken);
    localStorage.setItem('refreshToken', authData.refreshToken);
    if (authData.user) {
      localStorage.setItem('user', JSON.stringify(authData.user));
    }
  }
  
  private clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }
  
  isAuthenticated(): boolean {
    return !!localStorage.getItem('accessToken');
  }
  
  getCurrentUserFromStorage(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
  
  private handleError(error: any): Error {
    if (error.response?.data?.error?.message) {
      return new Error(error.response.data.error.message);
    }
    return new Error('An unexpected error occurred');
  }
}

export const authService = new AuthService();
```

### 2. Auth Context/Provider
```typescript
// contexts/AuthContext.tsx (React)
import React, { createContext, useContext, useReducer, useEffect } from 'react';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

type AuthAction = 
  | { type: 'LOGIN_START' }
  | { type: 'LOGIN_SUCCESS'; payload: User }
  | { type: 'LOGIN_FAILURE'; payload: string }
  | { type: 'LOGOUT' }
  | { type: 'CLEAR_ERROR' };

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: false,
  error: null
};

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'LOGIN_START':
      return { ...state, isLoading: true, error: null };
    case 'LOGIN_SUCCESS':
      return { 
        ...state, 
        user: action.payload, 
        isAuthenticated: true, 
        isLoading: false,
        error: null
      };
    case 'LOGIN_FAILURE':
      return { 
        ...state, 
        error: action.payload, 
        isLoading: false,
        isAuthenticated: false,
        user: null
      };
    case 'LOGOUT':
      return { ...initialState };
    case 'CLEAR_ERROR':
      return { ...state, error: null };
    default:
      return state;
  }
}

const AuthContext = createContext<{
  state: AuthState;
  login: (credentials: LoginRequest) => Promise<boolean>;
  logout: () => Promise<void>;
  verifyTwoFactor: (tempToken: string, code: string) => Promise<boolean>;
  clearError: () => void;
} | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState);
  
  useEffect(() => {
    // Check if user is already logged in
    const user = authService.getCurrentUserFromStorage();
    if (user && authService.isAuthenticated()) {
      dispatch({ type: 'LOGIN_SUCCESS', payload: user });
    }
  }, []);
  
  const login = async (credentials: LoginRequest): Promise<boolean> => {
    dispatch({ type: 'LOGIN_START' });
    
    try {
      const response = await authService.login(credentials);
      
      if (response.ok) {
        if (response.data?.requiresTwoFactor) {
          // Return false to indicate 2FA is required
          dispatch({ type: 'LOGIN_FAILURE', payload: '2FA required' });
          return false;
        } else if (response.data?.user) {
          dispatch({ type: 'LOGIN_SUCCESS', payload: response.data.user });
          return true;
        }
      }
      
      dispatch({ type: 'LOGIN_FAILURE', payload: response.error?.message || 'Login failed' });
      return false;
    } catch (error) {
      dispatch({ type: 'LOGIN_FAILURE', payload: error.message });
      return false;
    }
  };
  
  const verifyTwoFactor = async (tempToken: string, code: string): Promise<boolean> => {
    try {
      const response = await authService.verifyTwoFactor(tempToken, code);
      
      if (response.ok && response.data?.user) {
        dispatch({ type: 'LOGIN_SUCCESS', payload: response.data.user });
        return true;
      }
      
      dispatch({ type: 'LOGIN_FAILURE', payload: response.error?.message || '2FA verification failed' });
      return false;
    } catch (error) {
      dispatch({ type: 'LOGIN_FAILURE', payload: error.message });
      return false;
    }
  };
  
  const logout = async (): Promise<void> => {
    await authService.logout();
    dispatch({ type: 'LOGOUT' });
  };
  
  const clearError = () => {
    dispatch({ type: 'CLEAR_ERROR' });
  };
  
  return (
    <AuthContext.Provider value={{ state, login, logout, verifyTwoFactor, clearError }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
```

## API Service Layer

### 1. Base API Service
```typescript
// services/baseApiService.ts
export abstract class BaseApiService {
  protected async handleRequest<T>(
    request: () => Promise<AxiosResponse<ApiResponse<T>>>
  ): Promise<T> {
    try {
      const response = await request();
      
      if (response.data.ok && response.data.data !== undefined) {
        return response.data.data;
      }
      
      throw new Error(response.data.error?.message || 'Request failed');
    } catch (error) {
      if (error.response?.data?.error?.message) {
        throw new Error(error.response.data.error.message);
      }
      throw error;
    }
  }
  
  protected async handleVoidRequest(
    request: () => Promise<AxiosResponse<ApiResponse<any>>>
  ): Promise<void> {
    try {
      const response = await request();
      
      if (!response.data.ok) {
        throw new Error(response.data.error?.message || 'Request failed');
      }
    } catch (error) {
      if (error.response?.data?.error?.message) {
        throw new Error(error.response.data.error.message);
      }
      throw error;
    }
  }
}
```

### 2. Projects Service
```typescript
// services/projectsService.ts
export interface Project {
  id: string;
  name: string;
  description: string;
  photoUrl?: string;
  status: 'Current' | 'Future' | 'Past';
  technologiesUsed?: string;
  projectUrl?: string;
  gitHubUrl?: string;
  startDate?: string;
  endDate?: string;
  client?: string;
  budget?: number;
  isPublished: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
  createdByName?: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
  photoUrl?: string;
  status: Project['status'];
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

export class ProjectsService extends BaseApiService {
  async getProjects(status?: Project['status'], isPublished?: boolean): Promise<Project[]> {
    const params: any = {};
    if (status) params.status = status;
    if (isPublished !== undefined) params.isPublished = isPublished;
    
    return this.handleRequest(() => httpClient.get<Project[]>('/projects', params));
  }
  
  async getProject(id: string): Promise<Project> {
    return this.handleRequest(() => httpClient.get<Project>(`/projects/${id}`));
  }
  
  async getProjectsByStatus(status: Project['status']): Promise<Project[]> {
    return this.handleRequest(() => httpClient.get<Project[]>(`/projects/status/${status}`));
  }
  
  async createProject(project: CreateProjectRequest): Promise<Project> {
    return this.handleRequest(() => httpClient.post<Project>('/projects', project));
  }
  
  async updateProject(id: string, updates: Partial<CreateProjectRequest>): Promise<Project> {
    return this.handleRequest(() => httpClient.put<Project>(`/projects/${id}`, updates));
  }
  
  async deleteProject(id: string): Promise<void> {
    return this.handleVoidRequest(() => httpClient.delete(`/projects/${id}`));
  }
}

export const projectsService = new ProjectsService();
```

### 3. Contact Service
```typescript
// services/contactService.ts
export interface ContactMessage {
  id: string;
  fullName: string;
  emailAddress: string;
  subject: string;
  message: string;
  phoneNumber?: string;
  company?: string;
  isRead: boolean;
  isReplied: boolean;
  readAt?: string;
  repliedAt?: string;
  adminNotes?: string;
  createdAt: string;
}

export interface CreateContactRequest {
  fullName: string;
  emailAddress: string;
  subject: string;
  message: string;
  phoneNumber?: string;
  company?: string;
}

export interface ContactMessageFilter {
  fromDate?: string;
  toDate?: string;
  isRead?: boolean;
  isReplied?: boolean;
  sortBy?: 'CreatedAt' | 'Subject' | 'FullName';
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export class ContactService extends BaseApiService {
  async submitContact(message: CreateContactRequest): Promise<ContactMessage> {
    return this.handleRequest(() => httpClient.post<ContactMessage>('/contact', message));
  }
  
  // Admin endpoints
  async getContactMessage(id: string): Promise<ContactMessage> {
    return this.handleRequest(() => httpClient.get<ContactMessage>(`/contact/${id}`));
  }
  
  async getContactMessages(filter: ContactMessageFilter = {}): Promise<PagedResponse<ContactMessage>> {
    return this.handleRequest(() => httpClient.get<PagedResponse<ContactMessage>>('/contact', filter));
  }
  
  async markAsRead(id: string): Promise<void> {
    return this.handleVoidRequest(() => httpClient.post(`/contact/${id}/mark-read`));
  }
  
  async markAsReplied(id: string): Promise<void> {
    return this.handleVoidRequest(() => httpClient.post(`/contact/${id}/mark-replied`));
  }
  
  async deleteContactMessage(id: string): Promise<void> {
    return this.handleVoidRequest(() => httpClient.delete(`/contact/${id}`));
  }
}

export const contactService = new ContactService();
```

## React Integration Examples

### 1. Login Component
```tsx
// components/auth/LoginForm.tsx
import React, { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { LoadingButton } from '../ui/LoadingButton';
import { ErrorAlert } from '../ui/ErrorAlert';

export function LoginForm() {
  const { state, login, verifyTwoFactor, clearError } = useAuth();
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    twoFactorCode: ''
  });
  const [tempToken, setTempToken] = useState<string | null>(null);
  const [showTwoFactor, setShowTwoFactor] = useState(false);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    clearError();
    
    if (showTwoFactor) {
      const success = await verifyTwoFactor(tempToken!, formData.twoFactorCode);
      if (success) {
        // Redirect to dashboard
        window.location.href = '/dashboard';
      }
    } else {
      try {
        const response = await authService.login({
          email: formData.email,
          password: formData.password
        });
        
        if (response.ok) {
          if (response.data?.requiresTwoFactor) {
            setTempToken(response.data.tempToken!);
            setShowTwoFactor(true);
          } else {
            window.location.href = '/dashboard';
          }
        }
      } catch (error) {
        // Error is handled by context
      }
    }
  };
  
  return (
    <div className="max-w-md mx-auto">
      <form onSubmit={handleSubmit} className="space-y-4">
        <h2 className="text-2xl font-bold text-center">
          {showTwoFactor ? 'Two-Factor Authentication' : 'Login'}
        </h2>
        
        {state.error && <ErrorAlert message={state.error} onClose={clearError} />}
        
        {!showTwoFactor ? (
          <>
            <div>
              <label className="block text-sm font-medium mb-1">Email</label>
              <input
                type="email"
                required
                value={formData.email}
                onChange={(e) => setFormData({...formData, email: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Password</label>
              <input
                type="password"
                required
                value={formData.password}
                onChange={(e) => setFormData({...formData, password: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </>
        ) : (
          <div>
            <label className="block text-sm font-medium mb-1">
              Enter your 6-digit authentication code
            </label>
            <input
              type="text"
              required
              maxLength={6}
              value={formData.twoFactorCode}
              onChange={(e) => setFormData({...formData, twoFactorCode: e.target.value.replace(/\D/g, '')})}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-center text-lg tracking-widest"
              placeholder="000000"
            />
          </div>
        )}
        
        <LoadingButton
          type="submit"
          isLoading={state.isLoading}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white"
        >
          {showTwoFactor ? 'Verify' : 'Login'}
        </LoadingButton>
        
        {showTwoFactor && (
          <button
            type="button"
            onClick={() => setShowTwoFactor(false)}
            className="w-full text-sm text-gray-600 hover:text-gray-800"
          >
            Back to login
          </button>
        )}
      </form>
    </div>
  );
}
```

### 2. Projects Gallery Component
```tsx
// components/projects/ProjectsGallery.tsx
import React, { useState, useEffect } from 'react';
import { Project, projectsService } from '../../services/projectsService';
import { ProjectCard } from './ProjectCard';
import { ProjectFilter } from './ProjectFilter';
import { LoadingSpinner } from '../ui/LoadingSpinner';
import { ErrorAlert } from '../ui/ErrorAlert';

export function ProjectsGallery() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [filteredProjects, setFilteredProjects] = useState<Project[]>([]);
  const [selectedStatus, setSelectedStatus] = useState<Project['status'] | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    loadProjects();
  }, []);
  
  useEffect(() => {
    filterProjects();
  }, [projects, selectedStatus]);
  
  const loadProjects = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await projectsService.getProjects(undefined, true); // Only published
      setProjects(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load projects');
    } finally {
      setIsLoading(false);
    }
  };
  
  const filterProjects = () => {
    if (selectedStatus) {
      setFilteredProjects(projects.filter(p => p.status === selectedStatus));
    } else {
      setFilteredProjects(projects);
    }
  };
  
  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-20">
        <LoadingSpinner size="large" />
      </div>
    );
  }
  
  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-3xl font-bold mb-4">My Projects</h2>
        <p className="text-gray-600 max-w-2xl mx-auto">
          Here are some of the projects I've worked on. Each project represents 
          a unique challenge and learning experience.
        </p>
      </div>
      
      {error && (
        <ErrorAlert message={error} onClose={() => setError(null)} />
      )}
      
      <ProjectFilter
        selectedStatus={selectedStatus}
        onStatusChange={setSelectedStatus}
        projectCounts={{
          total: projects.length,
          current: projects.filter(p => p.status === 'Current').length,
          future: projects.filter(p => p.status === 'Future').length,
          past: projects.filter(p => p.status === 'Past').length
        }}
      />
      
      {filteredProjects.length === 0 ? (
        <div className="text-center py-20">
          <div className="text-gray-400 mb-4">
            <svg className="w-16 h-16 mx-auto" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clipRule="evenodd" />
            </svg>
          </div>
          <h3 className="text-xl font-medium text-gray-900 mb-2">No projects found</h3>
          <p className="text-gray-500">
            {selectedStatus 
              ? `No ${selectedStatus.toLowerCase()} projects available.`
              : 'No projects available at the moment.'
            }
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredProjects.map((project) => (
            <ProjectCard key={project.id} project={project} />
          ))}
        </div>
      )}
    </div>
  );
}
```

### 3. Contact Form Component
```tsx
// components/contact/ContactForm.tsx
import React, { useState } from 'react';
import { contactService, CreateContactRequest } from '../../services/contactService';
import { LoadingButton } from '../ui/LoadingButton';
import { SuccessAlert } from '../ui/SuccessAlert';
import { ErrorAlert } from '../ui/ErrorAlert';

export function ContactForm() {
  const [formData, setFormData] = useState<CreateContactRequest>({
    fullName: '',
    emailAddress: '',
    subject: '',
    message: '',
    phoneNumber: '',
    company: ''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);
    
    try {
      await contactService.submitContact({
        ...formData,
        phoneNumber: formData.phoneNumber || undefined,
        company: formData.company || undefined
      });
      
      setSuccess(true);
      setFormData({
        fullName: '',
        emailAddress: '',
        subject: '',
        message: '',
        phoneNumber: '',
        company: ''
      });
      
      // Hide success message after 5 seconds
      setTimeout(() => setSuccess(false), 5000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to send message');
    } finally {
      setIsSubmitting(false);
    }
  };
  
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };
  
  return (
    <div className="max-w-2xl mx-auto">
      <div className="text-center mb-8">
        <h2 className="text-3xl font-bold mb-4">Get In Touch</h2>
        <p className="text-gray-600">
          Have a project in mind or want to collaborate? I'd love to hear from you!
        </p>
      </div>
      
      {success && (
        <SuccessAlert 
          message="Thank you for your message! I'll get back to you soon."
          onClose={() => setSuccess(false)}
        />
      )}
      
      {error && (
        <ErrorAlert message={error} onClose={() => setError(null)} />
      )}
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label htmlFor="fullName" className="block text-sm font-medium text-gray-700 mb-1">
              Full Name *
            </label>
            <input
              type="text"
              id="fullName"
              name="fullName"
              required
              maxLength={100}
              value={formData.fullName}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          
          <div>
            <label htmlFor="emailAddress" className="block text-sm font-medium text-gray-700 mb-1">
              Email Address *
            </label>
            <input
              type="email"
              id="emailAddress"
              name="emailAddress"
              required
              maxLength={255}
              value={formData.emailAddress}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label htmlFor="phoneNumber" className="block text-sm font-medium text-gray-700 mb-1">
              Phone Number
            </label>
            <input
              type="tel"
              id="phoneNumber"
              name="phoneNumber"
              value={formData.phoneNumber}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          
          <div>
            <label htmlFor="company" className="block text-sm font-medium text-gray-700 mb-1">
              Company
            </label>
            <input
              type="text"
              id="company"
              name="company"
              value={formData.company}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
        </div>
        
        <div>
          <label htmlFor="subject" className="block text-sm font-medium text-gray-700 mb-1">
            Subject *
          </label>
          <input
            type="text"
            id="subject"
            name="subject"
            required
            maxLength={200}
            value={formData.subject}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
        
        <div>
          <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-1">
            Message *
          </label>
          <textarea
            id="message"
            name="message"
            required
            rows={6}
            value={formData.message}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-vertical"
            placeholder="Tell me about your project or what you'd like to discuss..."
          />
        </div>
        
        <div className="text-center">
          <LoadingButton
            type="submit"
            isLoading={isSubmitting}
            className="bg-blue-600 hover:bg-blue-700 text-white px-8 py-3 rounded-md font-medium"
          >
            Send Message
          </LoadingButton>
        </div>
      </form>
    </div>
  );
}
```

## State Management

### Using React Query/TanStack Query
```typescript
// hooks/useProjects.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsService, Project, CreateProjectRequest } from '../services/projectsService';

export function useProjects(status?: Project['status'], isPublished?: boolean) {
  return useQuery({
    queryKey: ['projects', status, isPublished],
    queryFn: () => projectsService.getProjects(status, isPublished),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useProject(id: string) {
  return useQuery({
    queryKey: ['project', id],
    queryFn: () => projectsService.getProject(id),
    enabled: !!id,
  });
}

export function useCreateProject() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (project: CreateProjectRequest) => projectsService.createProject(project),
    onSuccess: () => {
      // Invalidate all project queries
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
}

export function useUpdateProject() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, updates }: { id: string; updates: Partial<CreateProjectRequest> }) =>
      projectsService.updateProject(id, updates),
    onSuccess: (updatedProject) => {
      // Update specific project in cache
      queryClient.setQueryData(['project', updatedProject.id], updatedProject);
      // Invalidate projects list
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
}
```

### Using with React Query
```tsx
// components/projects/ProjectsPage.tsx
import React from 'react';
import { useProjects } from '../../hooks/useProjects';
import { ProjectsGallery } from './ProjectsGallery';
import { LoadingSpinner } from '../ui/LoadingSpinner';
import { ErrorAlert } from '../ui/ErrorAlert';

export function ProjectsPage() {
  const { data: projects, isLoading, error, refetch } = useProjects(undefined, true);
  
  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-20">
        <LoadingSpinner size="large" />
      </div>
    );
  }
  
  if (error) {
    return (
      <div className="py-20">
        <ErrorAlert 
          message={error instanceof Error ? error.message : 'Failed to load projects'}
          onClose={() => refetch()}
          action="Retry"
        />
      </div>
    );
  }
  
  return <ProjectsGallery projects={projects || []} />;
}
```

## Error Handling

### Global Error Boundary
```tsx
// components/ErrorBoundary.tsx
import React, { Component, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }
  
  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }
  
  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
    
    // You can log error to external service here
    // logErrorToService(error, errorInfo);
  }
  
  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div className="min-h-screen flex items-center justify-center bg-gray-50">
          <div className="text-center">
            <div className="text-red-500 mb-4">
              <svg className="w-16 h-16 mx-auto" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Something went wrong</h2>
            <p className="text-gray-600 mb-4">We're sorry for the inconvenience. Please try refreshing the page.</p>
            <button
              onClick={() => window.location.reload()}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-md"
            >
              Refresh Page
            </button>
          </div>
        </div>
      );
    }
    
    return this.props.children;
  }
}
```

### Custom Error Hook
```typescript
// hooks/useError.ts
import { useState, useCallback } from 'react';
import { toast } from 'react-hot-toast'; // or your preferred notification library

export interface ErrorInfo {
  message: string;
  code?: number;
  timestamp: Date;
}

export function useError() {
  const [errors, setErrors] = useState<ErrorInfo[]>([]);
  
  const addError = useCallback((error: string | Error, code?: number) => {
    const errorInfo: ErrorInfo = {
      message: error instanceof Error ? error.message : error,
      code,
      timestamp: new Date()
    };
    
    setErrors(prev => [...prev, errorInfo]);
    
    // Also show toast notification
    toast.error(errorInfo.message);
  }, []);
  
  const clearErrors = useCallback(() => {
    setErrors([]);
  }, []);
  
  const removeError = useCallback((index: number) => {
    setErrors(prev => prev.filter((_, i) => i !== index));
  }, []);
  
  return {
    errors,
    addError,
    clearErrors,
    removeError,
    hasErrors: errors.length > 0
  };
}
```

## Best Practices

### 1. Environment Configuration
```typescript
// config/environment.ts
interface Environment {
  apiUrl: string;
  environment: 'development' | 'staging' | 'production';
  enableDevTools: boolean;
  logLevel: 'debug' | 'info' | 'warn' | 'error';
}

function getEnvironment(): Environment {
  const env = process.env.NODE_ENV || 'development';
  
  switch (env) {
    case 'production':
      return {
        apiUrl: process.env.REACT_APP_API_URL || 'https://api.elzahy.com/api',
        environment: 'production',
        enableDevTools: false,
        logLevel: 'error'
      };
    case 'staging':
      return {
        apiUrl: process.env.REACT_APP_API_URL || 'https://staging-api.elzahy.com/api',
        environment: 'staging',
        enableDevTools: true,
        logLevel: 'warn'
      };
    default:
      return {
        apiUrl: process.env.REACT_APP_API_URL || 'https://localhost:5001/api',
        environment: 'development',
        enableDevTools: true,
        logLevel: 'debug'
      };
  }
}

export const environment = getEnvironment();
```

### 2. Route Protection
```tsx
// components/auth/ProtectedRoute.tsx
import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { LoadingSpinner } from '../ui/LoadingSpinner';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: 'Admin' | 'User';
}

export function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
  const { state } = useAuth();
  const location = useLocation();
  
  if (state.isLoading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <LoadingSpinner size="large" />
      </div>
    );
  }
  
  if (!state.isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  
  if (requiredRole && state.user?.role !== requiredRole) {
    return <Navigate to="/unauthorized" replace />;
  }
  
  return <>{children}</>;
}
```

### 3. Performance Optimization
```tsx
// hooks/useDebounce.ts
import { useState, useEffect } from 'react';

export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    
    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);
  
  return debouncedValue;
}

// Usage in search component
function SearchProjects() {
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 300);
  
  const { data: projects } = useQuery({
    queryKey: ['projects', 'search', debouncedSearchTerm],
    queryFn: () => projectsService.searchProjects(debouncedSearchTerm),
    enabled: debouncedSearchTerm.length > 2
  });
  
  return (
    <input
      type="text"
      value={searchTerm}
      onChange={(e) => setSearchTerm(e.target.value)}
      placeholder="Search projects..."
    />
  );
}
```

### 4. Accessibility
```tsx
// components/ui/Modal.tsx
import React, { useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
}

export function Modal({ isOpen, onClose, title, children }: ModalProps) {
  const modalRef = useRef<HTMLDivElement>(null);
  const previousActiveElement = useRef<HTMLElement | null>(null);
  
  useEffect(() => {
    if (isOpen) {
      previousActiveElement.current = document.activeElement as HTMLElement;
      modalRef.current?.focus();
      
      // Trap focus within modal
      const handleKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
          onClose();
        }
        
        if (e.key === 'Tab') {
          const focusableElements = modalRef.current?.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
          );
          
          if (focusableElements && focusableElements.length > 0) {
            const firstElement = focusableElements[0] as HTMLElement;
            const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;
            
            if (e.shiftKey && document.activeElement === firstElement) {
              e.preventDefault();
              lastElement.focus();
            } else if (!e.shiftKey && document.activeElement === lastElement) {
              e.preventDefault();
              firstElement.focus();
            }
          }
        }
      };
      
      document.addEventListener('keydown', handleKeyDown);
      document.body.style.overflow = 'hidden';
      
      return () => {
        document.removeEventListener('keydown', handleKeyDown);
        document.body.style.overflow = 'unset';
        previousActiveElement.current?.focus();
      };
    }
  }, [isOpen, onClose]);
  
  if (!isOpen) return null;
  
  return createPortal(
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center"
      role="dialog" 
      aria-modal="true"
      aria-labelledby="modal-title"
    >
      <div className="fixed inset-0 bg-black bg-opacity-50" onClick={onClose} />
      <div
        ref={modalRef}
        tabIndex={-1}
        className="relative bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6"
      >
        <h2 id="modal-title" className="text-xl font-semibold mb-4">
          {title}
        </h2>
        {children}
      </div>
    </div>,
    document.body
  );
}
```

This comprehensive frontend integration guide provides practical examples and best practices for integrating with the Elzahy Portfolio API across different frameworks and scenarios.