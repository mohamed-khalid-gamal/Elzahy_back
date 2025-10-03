# Frontend API Guide - Projects Endpoints

This guide provides comprehensive documentation for frontend developers on how to use the Projects API endpoints.

## Table of Contents
1. [Authentication](#authentication)
2. [Base Configuration](#base-configuration)
3. [Response Format](#response-format)
4. [Public Endpoints](#public-endpoints)
5. [Admin Endpoints](#admin-endpoints)
6. [Error Handling](#error-handling)
7. [Code Examples](#code-examples)
8. [Angular 20 Integration](#angular-20-integration)

## Authentication

### Authorization Header
For admin endpoints, include the JWT token in the Authorization header:
```javascript
headers: {
  'Authorization': `Bearer ${accessToken}`,
  'Content-Type': 'application/json'
}
```

### Admin Role Required
All endpoints marked with `[Admin]` require the user to have the "Admin" role.

## Base Configuration

```javascript
const API_BASE_URL = 'https://your-api-domain.com/api';
const PROJECTS_ENDPOINT = `${API_BASE_URL}/Projects`;
```

## Response Format

All endpoints return responses in this format:
```typescript
interface ApiResponse<T> {
  ok: boolean;
  data?: T;
  error?: {
    message: string;
    internalCode?: number;
    details?: string;
    traceId?: string;
  };
}

interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
  nextPage?: number;
  prevPage?: number;
}
```

## Public Endpoints

### 1. Get Projects (with filtering and pagination)
**GET** `/api/Projects`

#### Query Parameters:
```typescript
interface ProjectFilterDto {
  status?: 'Current' | 'Future' | 'Past';
  isPublished?: boolean;
  isFeatured?: boolean;
  propertyType?: string;
  location?: string;
  priceMin?: number;
  priceMax?: number;
  searchTerm?: string;
  language?: string;
  startDateFrom?: string; // ISO date
  startDateTo?: string;   // ISO date
  sortBy?: 'SortOrder' | 'CreatedAt' | 'Name' | 'StartDate' | 'PriceStart';
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}
```

#### Example Usage:
```javascript
// Get current projects with pagination
const getProjects = async (filters = {}) => {
  const params = new URLSearchParams();
  
  Object.keys(filters).forEach(key => {
    if (filters[key] !== undefined && filters[key] !== null) {
      params.append(key, filters[key]);
    }
  });
  
  const response = await fetch(`${PROJECTS_ENDPOINT}?${params}`);
  return await response.json();
};

// Usage examples
const currentProjects = await getProjects({ 
  status: 'Current', 
  page: 1, 
  pageSize: 12 
});

const searchResults = await getProjects({ 
  searchTerm: 'luxury apartment',
  location: 'New Cairo',
  priceMin: 1000000,
  priceMax: 5000000
});
```

### 2. Get Projects Summary (lightweight)
**GET** `/api/Projects/summary`

Returns lightweight project data optimized for listings.

```javascript
const getProjectsSummary = async (filters = {}) => {
  const params = new URLSearchParams(filters);
  const response = await fetch(`${PROJECTS_ENDPOINT}/summary?${params}`);
  return await response.json();
};
```

### 3. Get Featured Projects
**GET** `/api/Projects/featured`

#### Query Parameters:
- `count` (optional): Number of projects to return (default: 6)
- `language` (optional): Language filter

```javascript
const getFeaturedProjects = async (count = 6, language = null) => {
  const params = new URLSearchParams();
  params.append('count', count);
  if (language) params.append('language', language);
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/featured?${params}`);
  return await response.json();
};
```

### 4. Get Projects by Status
**GET** `/api/Projects/by-status/{status}`

```javascript
const getProjectsByStatus = async (status, options = {}) => {
  const { page = 1, pageSize = 12, language = null } = options;
  const params = new URLSearchParams({ page, pageSize });
  if (language) params.append('language', language);
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/by-status/${status}?${params}`);
  return await response.json();
};

// Usage
const currentProjects = await getProjectsByStatus('Current');
const futureProjects = await getProjectsByStatus('Future', { page: 2, pageSize: 8 });
```

### 5. Search Projects
**GET** `/api/Projects/search`

```javascript
const searchProjects = async (searchTerm, options = {}) => {
  const { page = 1, pageSize = 12, language = null, status = null } = options;
  const params = new URLSearchParams({ 
    searchTerm, 
    page, 
    pageSize 
  });
  
  if (language) params.append('language', language);
  if (status) params.append('status', status);
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/search?${params}`);
  return await response.json();
};
```

### 6. Get Single Project
**GET** `/api/Projects/{id}`

```javascript
const getProject = async (projectId, language = null) => {
  const params = language ? `?language=${language}` : '';
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}${params}`);
  return await response.json();
};
```

### 7. Get Projects by Property Type
**GET** `/api/Projects/by-property-type/{propertyType}`

```javascript
const getProjectsByPropertyType = async (propertyType, options = {}) => {
  const { page = 1, pageSize = 12, language = null } = options;
  const params = new URLSearchParams({ page, pageSize });
  if (language) params.append('language', language);
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/by-property-type/${propertyType}?${params}`);
  return await response.json();
};
```

### 8. Get Projects by Location
**GET** `/api/Projects/by-location/{location}`

```javascript
const getProjectsByLocation = async (location, options = {}) => {
  const { page = 1, pageSize = 12, language = null } = options;
  const params = new URLSearchParams({ page, pageSize });
  if (language) params.append('language', language);
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/by-location/${location}?${params}`);
  return await response.json();
};
```

### 9. Get Project Image
**GET** `/api/Projects/images/{imageId}`

Returns the actual image file (not JSON).

```javascript
const getProjectImageUrl = (imageId) => {
  return `${PROJECTS_ENDPOINT}/images/${imageId}`;
};

// Usage in HTML
// <img src={getProjectImageUrl(imageId)} alt="Project Image" />
```

### 10. Get Project Video
**GET** `/api/Projects/videos/{videoId}`

Returns the actual video file (not JSON).

```javascript
const getProjectVideoUrl = (videoId) => {
  return `${PROJECTS_ENDPOINT}/videos/${videoId}`;
};

// Usage in HTML
// <video src={getProjectVideoUrl(videoId)} controls></video>
```

## Admin Endpoints

### 1. Create Project
**POST** `/api/Projects` [Admin]

#### Request Body (multipart/form-data):
```typescript
interface CreateProjectFormRequestDto {
  name: string;
  description: string;
  images?: File[];
  mainImageIndex?: number;
  videos?: File[];
  mainVideoIndex?: number;
  status: 'Current' | 'Future' | 'Past';
  companyUrl?: string;
  googleMapsUrl?: string;
  location?: string;
  propertyType?: string;
  totalUnits?: number;
  projectArea?: number;
  priceStart?: number;
  priceEnd?: number;
  priceCurrency?: string;
  isPublished?: boolean;
  isFeatured?: boolean;
  sortOrder?: number;
  translations?: ProjectTranslationUpsertDto[];
}

interface ProjectTranslationUpsertDto {
  language: string;
  direction: 'RTL' | 'LTR';
  title: string;
  description: string;
}
```

#### Example Usage:
```javascript
const createProject = async (projectData, accessToken) => {
  const formData = new FormData();
  
  // Add text fields
  formData.append('name', projectData.name);
  formData.append('description', projectData.description);
  formData.append('status', projectData.status);
  
  // Add optional fields
  if (projectData.location) formData.append('location', projectData.location);
  if (projectData.propertyType) formData.append('propertyType', projectData.propertyType);
  if (projectData.priceStart) formData.append('priceStart', projectData.priceStart);
  if (projectData.priceEnd) formData.append('priceEnd', projectData.priceEnd);
  if (projectData.isFeatured) formData.append('isFeatured', projectData.isFeatured);
  
  // Add images
  if (projectData.images && projectData.images.length > 0) {
    projectData.images.forEach(image => {
      formData.append('images', image);
    });
    if (projectData.mainImageIndex !== undefined) {
      formData.append('mainImageIndex', projectData.mainImageIndex);
    }
  }
  
  // Add videos
  if (projectData.videos && projectData.videos.length > 0) {
    projectData.videos.forEach(video => {
      formData.append('videos', video);
    });
    if (projectData.mainVideoIndex !== undefined) {
      formData.append('mainVideoIndex', projectData.mainVideoIndex);
    }
  }
  
  // Add translations
  if (projectData.translations) {
    formData.append('translations', JSON.stringify(projectData.translations));
  }
  
  const response = await fetch(PROJECTS_ENDPOINT, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    },
    body: formData
  });
  
  return await response.json();
};
```

### 2. Update Project
**PUT** `/api/Projects/{id}` [Admin]

#### Request Body (multipart/form-data):
```typescript
interface UpdateProjectFormRequestDto {
  name?: string;
  description?: string;
  newImages?: File[];
  removeImageIds?: string[];
  mainImageId?: string;
  newVideos?: File[];
  removeVideoIds?: string[];
  mainVideoId?: string;
  status?: 'Current' | 'Future' | 'Past';
  // ... other optional fields
  translations?: ProjectTranslationUpsertDto[];
}
```

```javascript
const updateProject = async (projectId, updateData, accessToken) => {
  const formData = new FormData();
  
  // Add fields to update
  Object.keys(updateData).forEach(key => {
    if (updateData[key] !== undefined && updateData[key] !== null) {
      if (key === 'newImages' || key === 'newVideos') {
        updateData[key].forEach(file => {
          formData.append(key, file);
        });
      } else if (key === 'removeImageIds' || key === 'removeVideoIds') {
        updateData[key].forEach(id => {
          formData.append(key, id);
        });
      } else if (key === 'translations') {
        formData.append(key, JSON.stringify(updateData[key]));
      } else {
        formData.append(key, updateData[key]);
      }
    }
  });
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    },
    body: formData
  });
  
  return await response.json();
};
```

### 3. Delete Project
**DELETE** `/api/Projects/{id}` [Admin]

```javascript
const deleteProject = async (projectId, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

### 4. Toggle Featured Status
**PUT** `/api/Projects/{id}/toggle-featured` [Admin]

```javascript
const toggleFeatured = async (projectId, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}/toggle-featured`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

### 5. Add Project Image
**POST** `/api/Projects/{id}/images` [Admin]

```javascript
const addProjectImage = async (projectId, imageFile, description = null, isMainImage = false, accessToken) => {
  const formData = new FormData();
  formData.append('image', imageFile);
  if (description) formData.append('description', description);
  formData.append('isMainImage', isMainImage);
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}/images`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    },
    body: formData
  });
  
  return await response.json();
};
```

### 6. Delete Project Image
**DELETE** `/api/Projects/images/{imageId}` [Admin]

```javascript
const deleteProjectImage = async (imageId, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/images/${imageId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

### 7. Set Main Image
**PUT** `/api/Projects/{projectId}/images/{imageId}/set-main` [Admin]

```javascript
const setMainImage = async (projectId, imageId, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}/images/${imageId}/set-main`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

### 8. Add Project Video
**POST** `/api/Projects/{id}/videos` [Admin]

```javascript
const addProjectVideo = async (projectId, videoFile, description = null, isMainVideo = false, accessToken) => {
  const formData = new FormData();
  formData.append('video', videoFile);
  if (description) formData.append('description', description);
  formData.append('isMainVideo', isMainVideo);
  
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}/videos`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    },
    body: formData
  });
  
  return await response.json();
};
```

### 9. Delete Project Video
**DELETE** `/api/Projects/videos/{videoId}` [Admin]

```javascript
const deleteProjectVideo = async (videoId, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/videos/${videoId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

### 10. Set Main Video
**PUT** `/api/Projects/{projectId}/videos/{videoId}/set-main` [Admin]

```javascript
const setMainVideo = async (projectId, videoId, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}/videos/${videoId}/set-main`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

### 11. Add/Update Project Translation
**POST** `/api/Projects/{id}/translations` [Admin]

```javascript
const upsertProjectTranslation = async (projectId, translation, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}/translations`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(translation)
  });
  
  return await response.json();
};

// Usage
const translation = {
  language: 'ar',
  direction: 'RTL',
  title: '????? ???? ????',
  description: '??? ??????? ?????? ???????...'
};

await upsertProjectTranslation(projectId, translation, accessToken);
```

### 12. Delete Project Translation
**DELETE** `/api/Projects/{id}/translations/{language}` [Admin]

```javascript
const deleteProjectTranslation = async (projectId, language, accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/${projectId}/translations/${language}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

### 13. Get Project Statistics
**GET** `/api/Projects/analytics/stats` [Admin]

```javascript
const getProjectStatistics = async (accessToken) => {
  const response = await fetch(`${PROJECTS_ENDPOINT}/analytics/stats`, {
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  
  return await response.json();
};
```

## Error Handling

### Standard Error Response
```typescript
interface ErrorResponse {
  ok: false;
  error: {
    message: string;
    internalCode?: number;
    details?: string;
    traceId?: string;
  };
}
```

### Error Handling Example
```javascript
const handleApiCall = async (apiFunction) => {
  try {
    const response = await apiFunction();
    
    if (!response.ok) {
      // Handle API error
      console.error('API Error:', response.error?.message);
      throw new Error(response.error?.message || 'Unknown error occurred');
    }
    
    return response.data;
  } catch (error) {
    // Handle network or other errors
    console.error('Network Error:', error);
    throw error;
  }
};

// Usage
try {
  const projects = await handleApiCall(() => getProjects({ status: 'Current' }));
  console.log('Projects loaded:', projects);
} catch (error) {
  // Show error to user
  alert(`Failed to load projects: ${error.message}`);
}
```

## Code Examples

### React Hook for Projects
```javascript
import { useState, useEffect } from 'react';

const useProjects = (filters = {}) => {
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState(null);

  const fetchProjects = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await getProjects(filters);
      if (response.ok) {
        setProjects(response.data.data);
        setPagination({
          totalCount: response.data.totalCount,
          pageNumber: response.data.pageNumber,
          pageSize: response.data.pageSize,
          totalPages: response.data.totalPages,
          hasNext: response.data.hasNext,
          hasPrevious: response.data.hasPrevious
        });
      } else {
        setError(response.error?.message || 'Failed to fetch projects');
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProjects();
  }, [JSON.stringify(filters)]);

  return { 
    projects, 
    loading, 
    error, 
    pagination, 
    refetch: fetchProjects 
  };
};

// Usage in component
const ProjectsList = () => {
  const { projects, loading, error, pagination } = useProjects({
    status: 'Current',
    page: 1,
    pageSize: 12
  });

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      {projects.map(project => (
        <div key={project.id}>
          <h3>{project.name}</h3>
          <p>{project.description}</p>
          {project.mainImage && (
            <img 
              src={getProjectImageUrl(project.mainImage.id)} 
              alt={project.name}
            />
          )}
        </div>
      ))}
      
      {pagination && (
        <div>
          Page {pagination.pageNumber} of {pagination.totalPages}
          ({pagination.totalCount} total projects)
        </div>
      )}
    </div>
  );
};
```

### Vue.js Composition API Example
```javascript
import { ref, onMounted, watch } from 'vue';

export const useProjects = (filters = ref({})) => {
  const projects = ref([]);
  const loading = ref(false);
  const error = ref(null);
  const pagination = ref(null);

  const fetchProjects = async () => {
    loading.value = true;
    error.value = null;
    
    try {
      const response = await getProjects(filters.value);
      if (response.ok) {
        projects.value = response.data.data;
        pagination.value = response.data;
      } else {
        error.value = response.error?.message || 'Failed to fetch projects';
      }
    } catch (err) {
      error.value = err.message;
    } finally {
      loading.value = false;
    }
  };

  watch(filters, fetchProjects, { deep: true });
  onMounted(fetchProjects);

  return {
    projects,
    loading,
    error,
    pagination,
    refetch: fetchProjects
  };
};
```

## Angular 20 Integration

### TypeScript Interfaces
```typescript
// src/app/interfaces/project.interface.ts
export interface ProjectDto {
  id: string;
  createdAt: string;
  updatedAt: string;
  name: string;
  description: string;
  images: ProjectImageDto[];
  mainImage?: ProjectImageDto;
  videos: ProjectVideoDto[];
  mainVideo?: ProjectVideoDto;
  status: 'Current' | 'Future' | 'Past';
  companyUrl?: string;
  googleMapsUrl?: string;
  location?: string;
  propertyType?: string;
  totalUnits?: number;
  projectArea?: number;
  priceStart?: number;
  priceEnd?: number;
  priceCurrency?: string;
  priceRange?: string;
  isPublished: boolean;
  isFeatured: boolean;
  sortOrder: number;
  createdByName?: string;
  translations: ProjectTranslationDto[];
}

export interface ProjectImageDto {
  id: string;
  imageUrl: string;
  contentType: string;
  fileName: string;
  fileSize: number;
  description?: string;
  isMainImage: boolean;
  sortOrder: number;
}

export interface ProjectVideoDto {
  id: string;
  videoUrl: string;
  contentType: string;
  fileName: string;
  fileSize: number;
  description?: string;
  isMainVideo: boolean;
  sortOrder: number;
}

export interface ProjectTranslationDto {
  language: string;
  direction: string;
  title: string;
  description: string;
}

export interface ProjectFilterDto {
  status?: 'Current' | 'Future' | 'Past';
  isPublished?: boolean;
  isFeatured?: boolean;
  propertyType?: string;
  location?: string;
  priceMin?: number;
  priceMax?: number;
  searchTerm?: string;
  language?: string;
  startDateFrom?: string;
  startDateTo?: string;
  sortBy?: 'SortOrder' | 'CreatedAt' | 'Name' | 'StartDate' | 'PriceStart';
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}
```

### Angular 20 Service with Signals and HTTP Client
```typescript
// src/app/services/projects.service.ts
import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  ProjectDto, 
  ProjectFilterDto, 
  ApiResponse, 
  PagedResponse,
  ProjectSummaryDto 
} from '../interfaces/project.interface';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Projects`;

  // Signals for reactive state management
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);
  private readonly _projects = signal<ProjectDto[]>([]);
  private readonly _pagination = signal<any>(null);
  private readonly _featuredProjects = signal<ProjectSummaryDto[]>([]);

  // Public readonly signals
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly projects = this._projects.asReadonly();
  readonly pagination = this._pagination.asReadonly();
  readonly featuredProjects = this._featuredProjects.asReadonly();

  // Computed values
  readonly hasProjects = computed(() => this._projects().length > 0);
  readonly totalCount = computed(() => this._pagination()?.totalCount || 0);

  /**
   * Get projects with filtering and pagination
   */
  getProjects(filters: ProjectFilterDto = {}): Observable<ApiResponse<PagedResponse<ProjectDto>>> {
    this._loading.set(true);
    this._error.set(null);
    
    let params = new HttpParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.append(key, value.toString());
      }
    });

    return this.http.get<ApiResponse<PagedResponse<ProjectDto>>>(this.baseUrl, { params })
      .pipe(
        tap(response => {
          if (response.ok && response.data) {
            this._projects.set(response.data.data);
            this._pagination.set({
              totalCount: response.data.totalCount,
              pageNumber: response.data.pageNumber,
              pageSize: response.data.pageSize,
              totalPages: response.data.totalPages,
              hasNext: response.data.hasNext,
              hasPrevious: response.data.hasPrevious
            });
          } else {
            this._error.set(response.error?.message || 'Failed to fetch projects');
          }
          this._loading.set(false);
        }),
        catchError(error => {
          this._error.set(error.message || 'Network error occurred');
          this._loading.set(false);
          return throwError(() => error);
        })
      );
  }

  /**
   * Get projects summary (lightweight)
   */
  getProjectsSummary(filters: ProjectFilterDto = {}): Observable<ApiResponse<PagedResponse<ProjectSummaryDto>>> {
    let params = new HttpParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.append(key, value.toString());
      }
    });

    return this.http.get<ApiResponse<PagedResponse<ProjectSummaryDto>>>(
      `${this.baseUrl}/summary`, 
      { params }
    );
  }

  /**
   * Get featured projects
   */
  getFeaturedProjects(count: number = 6, language?: string): Observable<ApiResponse<ProjectSummaryDto[]>> {
    let params = new HttpParams().set('count', count.toString());
    
    if (language) {
      params = params.set('language', language);
    }

    return this.http.get<ApiResponse<ProjectSummaryDto[]>>(`${this.baseUrl}/featured`, { params })
      .pipe(
        tap(response => {
          if (response.ok && response.data) {
            this._featuredProjects.set(response.data);
          }
        })
      );
  }

  /**
   * Get projects by status
   */
  getProjectsByStatus(
    status: 'Current' | 'Future' | 'Past', 
    page: number = 1, 
    pageSize: number = 12, 
    language?: string
  ): Observable<ApiResponse<PagedResponse<ProjectSummaryDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (language) {
      params = params.set('language', language);
    }

    return this.http.get<ApiResponse<PagedResponse<ProjectSummaryDto>>>(
      `${this.baseUrl}/by-status/${status}`, 
      { params }
    );
  }

  /**
   * Search projects
   */
  searchProjects(
    searchTerm: string, 
    page: number = 1, 
    pageSize: number = 12, 
    language?: string, 
    status?: string
  ): Observable<ApiResponse<PagedResponse<ProjectSummaryDto>>> {
    let params = new HttpParams()
      .set('searchTerm', searchTerm)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (language) params = params.set('language', language);
    if (status) params = params.set('status', status);

    return this.http.get<ApiResponse<PagedResponse<ProjectSummaryDto>>>(
      `${this.baseUrl}/search`, 
      { params }
    );
  }

  /**
   * Get single project by ID
   */
  getProject(id: string, language?: string): Observable<ApiResponse<ProjectDto>> {
    let params = new HttpParams();
    if (language) {
      params = params.set('language', language);
    }
    
    return this.http.get<ApiResponse<ProjectDto>>(`${this.baseUrl}/${id}`, { params });
  }

  /**
   * Get projects by property type
   */
  getProjectsByPropertyType(
    propertyType: string, 
    page: number = 1, 
    pageSize: number = 12, 
    language?: string
  ): Observable<ApiResponse<PagedResponse<ProjectSummaryDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (language) params = params.set('language', language);

    return this.http.get<ApiResponse<PagedResponse<ProjectSummaryDto>>>(
      `${this.baseUrl}/by-property-type/${propertyType}`, 
      { params }
    );
  }

  /**
   * Get projects by location
   */
  getProjectsByLocation(
    location: string, 
    page: number = 1, 
    pageSize: number = 12, 
    language?: string
  ): Observable<ApiResponse<PagedResponse<ProjectSummaryDto>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (language) params = params.set('language', language);

    return this.http.get<ApiResponse<PagedResponse<ProjectSummaryDto>>>(
      `${this.baseUrl}/by-location/${location}`, 
      { params }
    );
  }

  /**
   * Create new project (Admin only)
   */
  createProject(projectData: FormData): Observable<ApiResponse<ProjectDto>> {
    return this.http.post<ApiResponse<ProjectDto>>(this.baseUrl, projectData);
  }

  /**
   * Update project (Admin only)
   */
  updateProject(id: string, projectData: FormData): Observable<ApiResponse<ProjectDto>> {
    return this.http.put<ApiResponse<ProjectDto>>(`${this.baseUrl}/${id}`, projectData);
  }

  /**
   * Delete project (Admin only)
   */
  deleteProject(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  /**
   * Toggle project featured status (Admin only)
   */
  toggleFeatured(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/${id}/toggle-featured`, {});
  }

  /**
   * Add project image (Admin only)
   */
  addProjectImage(
    projectId: string, 
    image: File, 
    description?: string, 
    isMainImage: boolean = false
  ): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('image', image);
    if (description) formData.append('description', description);
    formData.append('isMainImage', isMainImage.toString());

    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${projectId}/images`, formData);
  }

  /**
   * Delete project image (Admin only)
   */
  deleteProjectImage(imageId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/images/${imageId}`);
  }

  /**
   * Set main image (Admin only)
   */
  setMainImage(projectId: string, imageId: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.baseUrl}/${projectId}/images/${imageId}/set-main`, 
      {}
    );
  }

  /**
   * Add project video (Admin only)
   */
  addProjectVideo(
    projectId: string, 
    video: File, 
    description?: string, 
    isMainVideo: boolean = false
  ): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('video', video);
    if (description) formData.append('description', description);
    formData.append('isMainVideo', isMainVideo.toString());

    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${projectId}/videos`, formData);
  }

  /**
   * Delete project video (Admin only)
   */
  deleteProjectVideo(videoId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/videos/${videoId}`);
  }

  /**
   * Set main video (Admin only)
   */
  setMainVideo(projectId: string, videoId: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.baseUrl}/${projectId}/videos/${videoId}/set-main`, 
      {}
    );
  }

  /**
   * Add/Update project translation (Admin only)
   */
  upsertProjectTranslation(
    projectId: string, 
    translation: any
  ): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
      `${this.baseUrl}/${projectId}/translations`, 
      translation
    );
  }

  /**
   * Delete project translation (Admin only)
   */
  deleteProjectTranslation(projectId: string, language: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(
      `${this.baseUrl}/${projectId}/translations/${language}`
    );
  }

  /**
   * Get project statistics (Admin only)
   */
  getProjectStatistics(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/analytics/stats`);
  }

  /**
   * Utility methods for image and video URLs
   */
  getProjectImageUrl(imageId: string): string {
    return `${this.baseUrl}/images/${imageId}`;
  }

  getProjectVideoUrl(videoId: string): string {
    return `${this.baseUrl}/videos/${videoId}`;
  }

  /**
   * Reset state
   */
  resetState(): void {
    this._projects.set([]);
    this._pagination.set(null);
    this._loading.set(false);
    this._error.set(null);
  }
}
```

### Angular 20 Standalone Component Example
```typescript
// src/app/components/projects-list/projects-list.component.ts
import { Component, OnInit, inject, signal, effect, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectsService } from '../../services/projects.service';
import { ProjectDto, ProjectFilterDto } from '../../interfaces/project.interface';

@Component({
  selector: 'app-projects-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="projects-container">
      <!-- Filters -->
      <div class="filters-section">
        <div class="filter-group">
          <label for="status">Status:</label>
          <select 
            id="status" 
            [(ngModel)]="currentFilters.status" 
            (change)="onFiltersChange()">
            <option value="">All Statuses</option>
            <option value="Current">Current</option>
            <option value="Future">Future</option>
            <option value="Past">Past</option>
          </select>
        </div>

        <div class="filter-group">
          <label for="location">Location:</label>
          <input 
            id="location"
            type="text" 
            [(ngModel)]="currentFilters.location" 
            (input)="onFiltersChange()"
            placeholder="Enter location">
        </div>

        <div class="filter-group">
          <label for="propertyType">Property Type:</label>
          <input 
            id="propertyType"
            type="text" 
            [(ngModel)]="currentFilters.propertyType" 
            (input)="onFiltersChange()"
            placeholder="Enter property type">
        </div>

        <div class="filter-group">
          <label for="searchTerm">Search:</label>
          <input 
            id="searchTerm"
            type="text" 
            [(ngModel)]="currentFilters.searchTerm" 
            (input)="onFiltersChange()"
            placeholder="Search projects...">
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="projectsService.loading()" class="loading">
        Loading projects...
      </div>

      <!-- Error State -->
      <div *ngIf="projectsService.error()" class="error">
        {{ projectsService.error() }}
      </div>

      <!-- Projects Grid -->
      <div *ngIf="!projectsService.loading() && !projectsService.error()" class="projects-grid">
        <div 
          *ngFor="let project of projectsService.projects()" 
          class="project-card"
          [class.featured]="project.isFeatured">
          
          <!-- Main Image -->
          <div class="project-image" *ngIf="project.mainImage">
            <img 
              [src]="projectsService.getProjectImageUrl(project.mainImage.id)" 
              [alt]="project.name"
              loading="lazy">
            <div *ngIf="project.isFeatured" class="featured-badge">Featured</div>
            <div class="status-badge" [class]="project.status.toLowerCase()">
              {{ project.status }}
            </div>
          </div>

          <!-- Project Info -->
          <div class="project-info">
            <h3 class="project-title">{{ project.name }}</h3>
            <p class="project-description">{{ project.description | slice:0:150 }}...</p>
            
            <div class="project-details">
              <div *ngIf="project.location" class="detail-item">
                <span class="label">Location:</span>
                <span class="value">{{ project.location }}</span>
              </div>
              
              <div *ngIf="project.propertyType" class="detail-item">
                <span class="label">Type:</span>
                <span class="value">{{ project.propertyType }}</span>
              </div>
              
              <div *ngIf="project.priceRange" class="detail-item">
                <span class="label">Price:</span>
                <span class="value">{{ project.priceRange }}</span>
              </div>
              
              <div *ngIf="project.totalUnits" class="detail-item">
                <span class="label">Units:</span>
                <span class="value">{{ project.totalUnits }}</span>
              </div>
            </div>

            <!-- Translations -->
            <div *ngIf="project.translations.length > 0" class="translations">
              <div 
                *ngFor="let translation of project.translations" 
                class="translation"
                [class.rtl]="translation.direction === 'RTL'">
                <strong>{{ translation.language.toUpperCase() }}:</strong>
                {{ translation.title }}
              </div>
            </div>

            <div class="project-actions">
              <button 
                type="button"
                class="btn btn-primary"
                (click)="viewProject(project.id)">
                View Details
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div *ngIf="projectsService.pagination()" class="pagination">
        <button 
          type="button"
          class="btn btn-secondary"
          [disabled]="!projectsService.pagination()?.hasPrevious"
          (click)="goToPage(projectsService.pagination()?.pageNumber - 1)">
          Previous
        </button>
        
        <span class="page-info">
          Page {{ projectsService.pagination()?.pageNumber }} of {{ projectsService.pagination()?.totalPages }}
          ({{ projectsService.totalCount() }} total projects)
        </span>
        
        <button 
          type="button"
          class="btn btn-secondary"
          [disabled]="!projectsService.pagination()?.hasNext"
          (click)="goToPage(projectsService.pagination()?.pageNumber + 1)">
          Next
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./projects-list.component.scss']
})
export class ProjectsListComponent implements OnInit {
  protected readonly projectsService = inject(ProjectsService);
  
  // Input properties for external filters
  filters = input<ProjectFilterDto>({});
  
  // Local filter state
  currentFilters: ProjectFilterDto = {
    page: 1,
    pageSize: 12,
    sortBy: 'SortOrder',
    sortDescending: false,
    isPublished: true
  };

  private debounceTimer: any;

  constructor() {
    // React to input changes
    effect(() => {
      const inputFilters = this.filters();
      this.currentFilters = { ...this.currentFilters, ...inputFilters };
      this.loadProjects();
    });
  }

  ngOnInit(): void {
    this.loadProjects();
  }

  private loadProjects(): void {
    this.projectsService.getProjects(this.currentFilters).subscribe({
      next: (response) => {
        // Data is automatically handled by the service signals
      },
      error: (error) => {
        console.error('Failed to load projects:', error);
      }
    });
  }

  onFiltersChange(): void {
    // Reset to first page when filters change
    this.currentFilters.page = 1;
    
    // Debounce the API call to avoid too many requests
    clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => {
      this.loadProjects();
    }, 300);
  }

  goToPage(page: number): void {
    if (page >= 1) {
      this.currentFilters.page = page;
      this.loadProjects();
    }
  }

  viewProject(projectId: string): void {
    // Navigate to project details or emit event
    console.log('View project:', projectId);
    // this.router.navigate(['/projects', projectId]);
  }
}
```

### Angular 20 Standalone Featured Projects Component
```typescript
// src/app/components/featured-projects/featured-projects.component.ts
import { Component, OnInit, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectsService } from '../../services/projects.service';
import { ProjectSummaryDto } from '../../interfaces/project.interface';

@Component({
  selector: 'app-featured-projects',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="featured-projects">
      <h2>Featured Projects</h2>
      
      <div class="featured-grid">
        <div 
          *ngFor="let project of projectsService.featuredProjects()" 
          class="featured-card">
          
          <div class="project-image" *ngIf="project.mainImage">
            <img 
              [src]="projectsService.getProjectImageUrl(project.mainImage.id)" 
              [alt]="project.name"
              loading="lazy">
            <div class="featured-overlay">
              <span class="featured-label">Featured</span>
            </div>
          </div>
          
          <div class="project-content">
            <h3>{{ project.name }}</h3>
            <div class="project-meta">
              <span *ngIf="project.location" class="location">
                ?? {{ project.location }}
              </span>
              <span *ngIf="project.propertyType" class="type">
                ?? {{ project.propertyType }}
              </span>
              <span *ngIf="project.priceRange" class="price">
                ?? {{ project.priceRange }}
              </span>
            </div>
            
            <!-- Translations for different languages -->
            <div *ngIf="project.translations.length > 0" class="translations">
              <div 
                *ngFor="let translation of project.translations" 
                class="translation-item"
                [class.rtl]="translation.direction === 'RTL'">
                <small>{{ translation.language.toUpperCase() }}:</small>
                <span>{{ translation.title }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styleUrls: ['./featured-projects.component.scss']
})
export class FeaturedProjectsComponent implements OnInit {
  protected readonly projectsService = inject(ProjectsService);
  
  // Input properties
  count = input<number>(6);
  language = input<string | undefined>(undefined);

  ngOnInit(): void {
    this.loadFeaturedProjects();
  }

  private loadFeaturedProjects(): void {
    this.projectsService.getFeaturedProjects(this.count(), this.language()).subscribe({
      next: (response) => {
        // Data is automatically handled by the service signals
      },
      error: (error) => {
        console.error('Failed to load featured projects:', error);
      }
    });
  }
}
```

### Angular 20 Search Component with Reactive Forms
```typescript
// src/app/components/project-search/project-search.component.ts
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { ProjectsService } from '../../services/projects.service';
import { ProjectSummaryDto } from '../../interfaces/project.interface';

@Component({
  selector: 'app-project-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="search-container">
      <form [formGroup]="searchForm" class="search-form">
        <div class="search-row">
          <div class="form-group">
            <label for="searchTerm">Search Term</label>
            <input 
              id="searchTerm"
              type="text" 
              formControlName="searchTerm"
              placeholder="Search projects..."
              class="form-control">
          </div>
          
          <div class="form-group">
            <label for="location">Location</label>
            <input 
              id="location"
              type="text" 
              formControlName="location"
              placeholder="Enter location"
              class="form-control">
          </div>
          
          <div class="form-group">
            <label for="propertyType">Property Type</label>
            <input 
              id="propertyType"
              type="text" 
              formControlName="propertyType"
              placeholder="Property type"
              class="form-control">
          </div>
        </div>
        
        <div class="search-row">
          <div class="form-group">
            <label for="priceMin">Min Price</label>
            <input 
              id="priceMin"
              type="number" 
              formControlName="priceMin"
              placeholder="Min price"
              class="form-control">
          </div>
          
          <div class="form-group">
            <label for="priceMax">Max Price</label>
            <input 
              id="priceMax"
              type="number" 
              formControlName="priceMax"
              placeholder="Max price"
              class="form-control">
          </div>
          
          <div class="form-group">
            <label for="status">Status</label>
            <select id="status" formControlName="status" class="form-control">
              <option value="">All Statuses</option>
              <option value="Current">Current</option>
              <option value="Future">Future</option>
              <option value="Past">Past</option>
            </select>
          </div>
        </div>
        
        <div class="search-actions">
          <button type="button" (click)="clearSearch()" class="btn btn-secondary">
            Clear
          </button>
          <button type="button" (click)="search()" class="btn btn-primary">
            Search
          </button>
        </div>
      </form>
      
      <!-- Search Results -->
      <div class="search-results">
        <div *ngIf="searching()" class="loading">Searching...</div>
        
        <div *ngIf="searchError()" class="error">
          {{ searchError() }}
        </div>
        
        <div *ngIf="searchResults().length > 0" class="results-grid">
          <div 
            *ngFor="let project of searchResults()" 
            class="result-card">
            
            <div class="project-image" *ngIf="project.mainImage">
              <img 
                [src]="projectsService.getProjectImageUrl(project.mainImage.id)" 
                [alt]="project.name"
                loading="lazy">
            </div>
            
            <div class="project-info">
              <h3>{{ project.name }}</h3>
              <div class="meta-info">
                <span *ngIf="project.location" class="location">
                  ?? {{ project.location }}
                </span>
                <span *ngIf="project.propertyType" class="type">
                  ?? {{ project.propertyType }}
                </span>
                <span *ngIf="project.priceRange" class="price">
                  ?? {{ project.priceRange }}
                </span>
              </div>
            </div>
          </div>
        </div>
        
        <div *ngIf="!searching() && searchResults().length === 0 && hasSearched()" class="no-results">
          No projects found matching your criteria.
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./project-search.component.scss']
})
export class ProjectSearchComponent {
  private readonly fb = inject(FormBuilder);
  protected readonly projectsService = inject(ProjectsService);
  
  // Signals for component state
  searchResults = signal<ProjectSummaryDto[]>([]);
  searching = signal<boolean>(false);
  searchError = signal<string | null>(null);
  hasSearched = signal<boolean>(false);
  
  searchForm: FormGroup = this.fb.group({
    searchTerm: [''],
    location: [''],
    propertyType: [''],
    priceMin: [null],
    priceMax: [null],
    status: ['']
  });

  constructor() {
    // Auto-search on form changes with debounce
    this.searchForm.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(formValue => {
        if (this.hasAnyValue(formValue)) {
          return this.performSearch(formValue);
        }
        return [];
      })
    ).subscribe();
  }

  private hasAnyValue(formValue: any): boolean {
    return Object.values(formValue).some(value => 
      value !== null && value !== undefined && value !== ''
    );
  }

  private performSearch(formValue: any) {
    this.searching.set(true);
    this.searchError.set(null);
    this.hasSearched.set(true);
    
    const filters = {
      ...formValue,
      page: 1,
      pageSize: 20,
      isPublished: true
    };
    
    return this.projectsService.getProjectsSummary(filters);
  }

  search(): void {
    const formValue = this.searchForm.value;
    
    this.performSearch(formValue).subscribe({
      next: (response) => {
        if (response.ok && response.data) {
          this.searchResults.set(response.data.data);
        } else {
          this.searchError.set(response.error?.message || 'Search failed');
        }
        this.searching.set(false);
      },
      error: (error) => {
        this.searchError.set(error.message || 'Search failed');
        this.searching.set(false);
      }
    });
  }

  clearSearch(): void {
    this.searchForm.reset();
    this.searchResults.set([]);
    this.searchError.set(null);
    this.hasSearched.set(false);
  }
}
```

### Environment Configuration
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://your-api-domain.com/api'
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api-domain.com/api'
};
```

### HTTP Interceptor for Authentication
```typescript
// src/app/interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();
  
  if (token) {
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authReq);
  }
  
  return next(req);
};
```

### App Configuration with Providers
```typescript
// src/app/app.config.ts
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    // Add other providers as needed
  ]
};
```

## Common Use Cases

### 1. Property Listings Page
```javascript
// Fetch current projects with pagination
const loadPropertyListings = async (page = 1) => {
  const response = await getProjectsSummary({
    status: 'Current',
    isPublished: true,
    page: page,
    pageSize: 12,
    sortBy: 'SortOrder'
  });
  
  return response;
};
```

### 2. Featured Properties Section
```javascript
// Get featured properties for homepage
const loadFeaturedProperties = async () => {
  const response = await getFeaturedProjects(6);
  return response.ok ? response.data : [];
};
```

### 3. Property Search with Filters
```javascript
const searchProperties = async (searchFilters) => {
  const filters = {
    searchTerm: searchFilters.searchTerm,
    location: searchFilters.location,
    propertyType: searchFilters.propertyType,
    priceMin: searchFilters.priceMin,
    priceMax: searchFilters.priceMax,
    status: 'Current',
    isPublished: true,
    page: searchFilters.page || 1,
    pageSize: 12
  };
  
  return await getProjects(filters);
};
```

### 4. Property Detail Page
```javascript
const loadPropertyDetails = async (projectId, language = 'en') => {
  const response = await getProject(projectId, language);
  return response.ok ? response.data : null;
};
```

This guide covers all the available endpoints with practical examples for common frontend frameworks, with a special focus on Angular 20 modern features including signals, standalone components, and reactive programming. Remember to handle authentication properly for admin endpoints and implement proper error handling throughout your application.