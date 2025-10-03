# El-Zahy Real Estate API - Developer Guide

## Quick Start Guide

### Prerequisites
- .NET 8 SDK
- SQL Server or MySQL database
- JWT authentication setup

### Installation & Setup

1. **Clone the repository**
```bash
git clone https://github.com/mohamed-khalid-gamal/Elzahy_back.git
cd Elzahy_back
```

2. **Configure Database Connection**

Set environment variables:
```bash
# For SQL Server
set ConnectionStrings__DefaultConnection="Server=localhost;Database=ElzahyDB;Trusted_Connection=true;"

# For MySQL (alternative)
set MYSQL_CONNECTION_STRING="Server=localhost;Database=ElzahyDB;Uid=root;Pwd=password;"
```

3. **Configure JWT Settings**
```bash
set DOTNET_JWT_KEY="your-super-secret-jwt-key-here"
set JWT__Issuer="ElzahyAPI"
set JWT__Audience="ElzahyClients"
```

4. **Run Database Migrations**
```bash
dotnet ef database update
```

5. **Start the Application**
```bash
dotnet run
```

The API will be available at `https://localhost:7000` (or your configured port).

## Frontend Integration Examples

### React/Next.js Implementation

#### 1. API Service Setup
```typescript
// services/api.ts
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7000/api';

interface ApiResponse<T> {
  ok: boolean;
  data?: T;
  error?: {
    message: string;
    internalCode?: number;
    traceId?: string;
  };
}

class ProjectAPI {
  private async request<T>(
    endpoint: string, 
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    });
    
    return await response.json();
  }

  // Get projects with filtering
  async getProjects(filters: ProjectFilter = {}): Promise<ApiResponse<PagedResponse<ProjectSummary>>> {
    const params = new URLSearchParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, value.toString());
      }
    });
    
    return this.request(`/projects/summary?${params}`);
  }

  // Get featured projects
  async getFeaturedProjects(count = 6, language?: string): Promise<ApiResponse<ProjectSummary[]>> {
    const params = new URLSearchParams();
    params.append('count', count.toString());
    if (language) params.append('language', language);
    
    return this.request(`/projects/featured?${params}`);
  }

  // Get single project
  async getProject(id: string, language?: string): Promise<ApiResponse<Project>> {
    const params = language ? `?language=${language}` : '';
    return this.request(`/projects/${id}${params}`);
  }

  // Search projects
  async searchProjects(searchTerm: string, filters: Partial<ProjectFilter> = {}): Promise<ApiResponse<PagedResponse<ProjectSummary>>> {
    const params = new URLSearchParams();
    params.append('searchTerm', searchTerm);
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, value.toString());
      }
    });
    
    return this.request(`/projects/search?${params}`);
  }
}

export const projectAPI = new ProjectAPI();

// TypeScript interfaces
interface ProjectFilter {
  status?: 'Current' | 'Future' | 'Past';
  isPublished?: boolean;
  isFeatured?: boolean;
  propertyType?: string;
  location?: string;
  priceMin?: number;
  priceMax?: number;
  language?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
  nextPage?: number;
  prevPage?: number;
}

interface ProjectTranslation {
  language: string;
  direction: 'RTL' | 'LTR';
  title: string;
  description: string;
}

interface ProjectSummary {
  id: string;
  name: string;
  status: string;
  location?: string;
  propertyType?: string;
  priceRange?: string;
  mainImage?: ProjectImage;
  isFeatured: boolean;
  translations: ProjectTranslation[];
}

interface Project extends ProjectSummary {
  description: string;
  images: ProjectImage[];
  videos: ProjectVideo[];
  companyUrl?: string;
  googleMapsUrl?: string;
  totalUnits?: number;
  projectArea?: number;
  priceStart?: number;
  priceEnd?: number;
  priceCurrency?: string;
  // ... other fields
}
```

#### 2. React Hook for Projects
```typescript
// hooks/useProjects.ts
import { useState, useEffect } from 'react';
import { projectAPI } from '../services/api';

export function useProjects(filters: ProjectFilter = {}) {
  const [projects, setProjects] = useState<ProjectSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState<any>(null);

  const loadProjects = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await projectAPI.getProjects(filters);
      
      if (response.ok && response.data) {
        setProjects(response.data.data);
        setPagination({
          totalCount: response.data.totalCount,
          totalPages: response.data.totalPages,
          hasNext: response.data.hasNext,
          hasPrevious: response.data.hasPrevious,
          currentPage: response.data.pageNumber,
        });
      } else {
        setError(response.error?.message || 'Failed to load projects');
      }
    } catch (err) {
      setError('Network error occurred');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProjects();
  }, [JSON.stringify(filters)]);

  return {
    projects,
    loading,
    error,
    pagination,
    refetch: loadProjects,
  };
}

// Usage in component
function ProjectsPage() {
  const { projects, loading, error, pagination } = useProjects({
    isPublished: true,
    language: 'ar',
    pageSize: 12,
  });

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-6">
        {projects.map(project => (
          <ProjectCard key={project.id} project={project} />
        ))}
      </div>
      
      {pagination && (
        <Pagination 
          currentPage={pagination.currentPage}
          totalPages={pagination.totalPages}
          hasNext={pagination.hasNext}
          hasPrevious={pagination.hasPrevious}
        />
      )}
    </div>
  );
}
```

#### 3. Project Card Component with RTL Support
```tsx
// components/ProjectCard.tsx
import { ProjectSummary } from '../types';

interface ProjectCardProps {
  project: ProjectSummary;
  language?: string;
}

export function ProjectCard({ project, language = 'ar' }: ProjectCardProps) {
  // Get translation for current language
  const translation = project.translations.find(t => t.language === language) 
    || project.translations[0] 
    || { title: project.name, description: '', direction: 'LTR' };

  const isRTL = translation.direction === 'RTL';

  return (
    <div 
      className={`bg-white rounded-lg shadow-lg overflow-hidden ${isRTL ? 'rtl' : 'ltr'}`}
      dir={translation.direction.toLowerCase()}
    >
      {/* Main Image */}
      {project.mainImage && (
        <div className="aspect-video relative">
          <img
            src={`data:${project.mainImage.contentType};base64,${project.mainImage.imageData}`}
            alt={translation.title}
            className="w-full h-full object-cover"
          />
          {project.isFeatured && (
            <div className={`absolute top-2 ${isRTL ? 'right-2' : 'left-2'} bg-yellow-500 text-white px-2 py-1 rounded text-sm`}>
              {isRTL ? '????' : 'Featured'}
            </div>
          )}
        </div>
      )}

      <div className="p-4">
        {/* Title */}
        <h3 className="text-lg font-bold mb-2 text-gray-900">
          {translation.title}
        </h3>

        {/* Location & Property Type */}
        <div className="flex items-center text-sm text-gray-600 mb-2">
          {project.location && (
            <span className={`${isRTL ? 'ml-2' : 'mr-2'}`}>
              ?? {project.location}
            </span>
          )}
          {project.propertyType && (
            <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded text-xs">
              {project.propertyType}
            </span>
          )}
        </div>

        {/* Price Range */}
        {project.priceRange && (
          <div className="text-lg font-semibold text-green-600 mb-2">
            {project.priceRange}
          </div>
        )}

        {/* Status */}
        <div className={`inline-block px-2 py-1 rounded text-xs font-medium ${
          project.status === 'Current' ? 'bg-green-100 text-green-800' :
          project.status === 'Future' ? 'bg-blue-100 text-blue-800' :
          'bg-gray-100 text-gray-800'
        }`}>
          {isRTL ? getStatusArabic(project.status) : project.status}
        </div>
      </div>
    </div>
  );
}

function getStatusArabic(status: string): string {
  switch (status) {
    case 'Current': return '???? ??????';
    case 'Future': return '????';
    case 'Past': return '?????';
    default: return status;
  }
}
```

#### 4. Advanced Search Component
```tsx
// components/ProjectSearch.tsx
import { useState } from 'react';
import { projectAPI } from '../services/api';

interface SearchFilters extends ProjectFilter {
  searchTerm: string;
}

export function ProjectSearch({ onResults, language = 'ar' }: {
  onResults: (projects: ProjectSummary[]) => void;
  language?: string;
}) {
  const [filters, setFilters] = useState<SearchFilters>({
    searchTerm: '',
    language,
    isPublished: true,
  });
  const [loading, setLoading] = useState(false);

  const handleSearch = async () => {
    if (!filters.searchTerm.trim()) return;

    setLoading(true);
    try {
      const response = await projectAPI.searchProjects(filters.searchTerm, filters);
      if (response.ok && response.data) {
        onResults(response.data.data);
      }
    } finally {
      setLoading(false);
    }
  };

  const isRTL = language === 'ar';

  return (
    <div className={`bg-white p-6 rounded-lg shadow ${isRTL ? 'rtl' : 'ltr'}`} dir={isRTL ? 'rtl' : 'ltr'}>
      {/* Search Input */}
      <div className="mb-4">
        <input
          type="text"
          placeholder={isRTL ? '???? ?? ????????...' : 'Search projects...'}
          value={filters.searchTerm}
          onChange={(e) => setFilters({ ...filters, searchTerm: e.target.value })}
          className="w-full p-3 border border-gray-300 rounded-lg"
          dir={isRTL ? 'rtl' : 'ltr'}
        />
      </div>

      {/* Filters */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
        {/* Status Filter */}
        <select
          value={filters.status || ''}
          onChange={(e) => setFilters({ ...filters, status: e.target.value as any })}
          className="p-2 border border-gray-300 rounded"
        >
          <option value="">{isRTL ? '?? ???????' : 'All Statuses'}</option>
          <option value="Current">{isRTL ? '???? ??????' : 'Current'}</option>
          <option value="Future">{isRTL ? '????' : 'Future'}</option>
          <option value="Past">{isRTL ? '?????' : 'Past'}</option>
        </select>

        {/* Property Type Filter */}
        <select
          value={filters.propertyType || ''}
          onChange={(e) => setFilters({ ...filters, propertyType: e.target.value })}
          className="p-2 border border-gray-300 rounded"
        >
          <option value="">{isRTL ? '?? ???????' : 'All Types'}</option>
          <option value="Residential">{isRTL ? '????' : 'Residential'}</option>
          <option value="Commercial">{isRTL ? '?????' : 'Commercial'}</option>
          <option value="Mixed-use">{isRTL ? '?????' : 'Mixed-use'}</option>
        </select>

        {/* Price Range */}
        <input
          type="number"
          placeholder={isRTL ? '???? ?????? ?????' : 'Min Price'}
          value={filters.priceMin || ''}
          onChange={(e) => setFilters({ ...filters, priceMin: Number(e.target.value) })}
          className="p-2 border border-gray-300 rounded"
        />
        
        <input
          type="number"
          placeholder={isRTL ? '???? ?????? ?????' : 'Max Price'}
          value={filters.priceMax || ''}
          onChange={(e) => setFilters({ ...filters, priceMax: Number(e.target.value) })}
          className="p-2 border border-gray-300 rounded"
        />
      </div>

      {/* Search Button */}
      <button
        onClick={handleSearch}
        disabled={loading}
        className="w-full bg-blue-600 text-white p-3 rounded-lg hover:bg-blue-700 disabled:opacity-50"
      >
        {loading ? (isRTL ? '???? ?????...' : 'Searching...') : (isRTL ? '???' : 'Search')}
      </button>
    </div>
  );
}
```

### Angular Implementation

#### 1. Angular Service
```typescript
// services/project.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private apiUrl = `${environment.apiUrl}/projects`;

  constructor(private http: HttpClient) {}

  getProjects(filters: ProjectFilter = {}): Observable<ApiResponse<PagedResponse<ProjectSummary>>> {
    let params = new HttpParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<ApiResponse<PagedResponse<ProjectSummary>>>(`${this.apiUrl}/summary`, { params });
  }

  getFeaturedProjects(count = 6, language?: string): Observable<ApiResponse<ProjectSummary[]>> {
    let params = new HttpParams().set('count', count.toString());
    if (language) {
      params = params.set('language', language);
    }
    
    return this.http.get<ApiResponse<ProjectSummary[]>>(`${this.apiUrl}/featured`, { params });
  }

  getProject(id: string, language?: string): Observable<ApiResponse<Project>> {
    let params = new HttpParams();
    if (language) {
      params = params.set('language', language);
    }
    
    return this.http.get<ApiResponse<Project>>(`${this.apiUrl}/${id}`, { params });
  }
}
```

#### 2. Angular Component
```typescript
// components/projects-list.component.ts
import { Component, OnInit } from '@angular/core';
import { ProjectService } from '../services/project.service';

@Component({
  selector: 'app-projects-list',
  template: `
    <div class="projects-container" [attr.dir]="currentLanguage === 'ar' ? 'rtl' : 'ltr'">
      <div *ngIf="loading" class="loading">Loading...</div>
      
      <div *ngIf="!loading" class="projects-grid">
        <div *ngFor="let project of projects" 
             class="project-card"
             [class.rtl]="isRTL(project)">
          
          <img *ngIf="project.mainImage" 
               [src]="getImageUrl(project.mainImage)"
               [alt]="getProjectTitle(project)"
               class="project-image">
               
          <div class="project-content">
            <h3>{{ getProjectTitle(project) }}</h3>
            <p class="location">{{ project.location }}</p>
            <p class="price">{{ project.priceRange }}</p>
            <span class="status" [class]="'status-' + project.status.toLowerCase()">
              {{ getStatusText(project.status) }}
            </span>
          </div>
        </div>
      </div>
      
      <div *ngIf="pagination" class="pagination">
        <button *ngIf="pagination.hasPrevious" 
                (click)="loadPage(pagination.currentPage - 1)">
          {{ currentLanguage === 'ar' ? '??????' : 'Previous' }}
        </button>
        
        <span>{{ pagination.currentPage }} / {{ pagination.totalPages }}</span>
        
        <button *ngIf="pagination.hasNext" 
                (click)="loadPage(pagination.currentPage + 1)">
          {{ currentLanguage === 'ar' ? '??????' : 'Next' }}
        </button>
      </div>
    </div>
  `
})
export class ProjectsListComponent implements OnInit {
  projects: ProjectSummary[] = [];
  loading = true;
  pagination: any = null;
  currentLanguage = 'ar';

  constructor(private projectService: ProjectService) {}

  ngOnInit() {
    this.loadProjects();
  }

  loadProjects(page = 1) {
    this.loading = true;
    
    const filters: ProjectFilter = {
      isPublished: true,
      language: this.currentLanguage,
      page,
      pageSize: 12
    };

    this.projectService.getProjects(filters).subscribe({
      next: (response) => {
        if (response.ok && response.data) {
          this.projects = response.data.data;
          this.pagination = {
            currentPage: response.data.pageNumber,
            totalPages: response.data.totalPages,
            hasNext: response.data.hasNext,
            hasPrevious: response.data.hasPrevious
          };
        }
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadPage(page: number) {
    this.loadProjects(page);
  }

  getProjectTitle(project: ProjectSummary): string {
    const translation = project.translations.find(t => t.language === this.currentLanguage);
    return translation?.title || project.name;
  }

  getImageUrl(image: ProjectImage): string {
    return `data:${image.contentType};base64,${image.imageData}`;
  }

  isRTL(project: ProjectSummary): boolean {
    const translation = project.translations.find(t => t.language === this.currentLanguage);
    return translation?.direction === 'RTL';
  }

  getStatusText(status: string): string {
    if (this.currentLanguage === 'ar') {
      switch (status) {
        case 'Current': return '???? ??????';
        case 'Future': return '????';
        case 'Past': return '?????';
        default: return status;
      }
    }
    return status;
  }
}
```

## Admin Panel Integration

### 1. File Upload Component (React)
```tsx
// components/admin/ProjectForm.tsx
import { useState } from 'react';

interface ProjectFormData {
  name: string;
  description: string;
  status: 'Current' | 'Future' | 'Past';
  location: string;
  propertyType: string;
  priceStart: number;
  priceEnd: number;
  images: File[];
  translations: ProjectTranslation[];
}

export function ProjectForm({ onSubmit }: { onSubmit: (data: FormData) => void }) {
  const [formData, setFormData] = useState<ProjectFormData>({
    name: '',
    description: '',
    status: 'Current',
    location: '',
    propertyType: 'Residential',
    priceStart: 0,
    priceEnd: 0,
    images: [],
    translations: [
      { language: 'ar', direction: 'RTL', title: '', description: '' },
      { language: 'en', direction: 'LTR', title: '', description: '' }
    ]
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const formDataToSend = new FormData();
    
    // Add basic fields
    formDataToSend.append('name', formData.name);
    formDataToSend.append('description', formData.description);
    formDataToSend.append('status', formData.status);
    formDataToSend.append('location', formData.location);
    formDataToSend.append('propertyType', formData.propertyType);
    formDataToSend.append('priceStart', formData.priceStart.toString());
    formDataToSend.append('priceEnd', formData.priceEnd.toString());
    formDataToSend.append('priceCurrency', 'EGP');
    
    // Add images
    formData.images.forEach(image => {
      formDataToSend.append('images', image);
    });
    
    // Add translations
    formDataToSend.append('translations', JSON.stringify(formData.translations));
    
    onSubmit(formDataToSend);
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setFormData({
        ...formData,
        images: Array.from(e.target.files)
      });
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Basic Information */}
      <div>
        <label className="block text-sm font-medium text-gray-700">Project Name</label>
        <input
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700">Description</label>
        <textarea
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          rows={4}
          required
        />
      </div>

      {/* Location & Property Type */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-gray-700">Location</label>
          <input
            type="text"
            value={formData.location}
            onChange={(e) => setFormData({ ...formData, location: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Property Type</label>
          <select
            value={formData.propertyType}
            onChange={(e) => setFormData({ ...formData, propertyType: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          >
            <option value="Residential">Residential</option>
            <option value="Commercial">Commercial</option>
            <option value="Mixed-use">Mixed-use</option>
          </select>
        </div>
      </div>

      {/* Price Range */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-gray-700">Starting Price (EGP)</label>
          <input
            type="number"
            value={formData.priceStart}
            onChange={(e) => setFormData({ ...formData, priceStart: Number(e.target.value) })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Ending Price (EGP)</label>
          <input
            type="number"
            value={formData.priceEnd}
            onChange={(e) => setFormData({ ...formData, priceEnd: Number(e.target.value) })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          />
        </div>
      </div>

      {/* Images */}
      <div>
        <label className="block text-sm font-medium text-gray-700">Project Images</label>
        <input
          type="file"
          multiple
          accept="image/*"
          onChange={handleImageChange}
          className="mt-1 block w-full"
        />
        {formData.images.length > 0 && (
          <p className="mt-2 text-sm text-gray-500">
            {formData.images.length} image(s) selected
          </p>
        )}
      </div>

      {/* Translations */}
      <div>
        <h3 className="text-lg font-medium text-gray-900 mb-4">Translations</h3>
        {formData.translations.map((translation, index) => (
          <div key={translation.language} className="border p-4 rounded mb-4">
            <h4 className="font-medium mb-2">
              {translation.language === 'ar' ? 'Arabic' : 'English'} Translation
            </h4>
            
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700">Title</label>
                <input
                  type="text"
                  value={translation.title}
                  onChange={(e) => {
                    const newTranslations = [...formData.translations];
                    newTranslations[index].title = e.target.value;
                    setFormData({ ...formData, translations: newTranslations });
                  }}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                  dir={translation.direction.toLowerCase()}
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  value={translation.description}
                  onChange={(e) => {
                    const newTranslations = [...formData.translations];
                    newTranslations[index].description = e.target.value;
                    setFormData({ ...formData, translations: newTranslations });
                  }}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
                  rows={3}
                  dir={translation.direction.toLowerCase()}
                />
              </div>
            </div>
          </div>
        ))}
      </div>

      <div>
        <button
          type="submit"
          className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
        >
          Create Project
        </button>
      </div>
    </form>
  );
}
```

## Best Practices

### 1. Performance Optimization
- Use the `/summary` endpoint for listing pages
- Implement client-side caching for frequently accessed data
- Consider implementing image lazy loading
- Use pagination appropriately (12 items for grids, 10 for lists)

### 2. Error Handling
```typescript
// Comprehensive error handling
async function handleApiCall<T>(apiCall: () => Promise<ApiResponse<T>>): Promise<T | null> {
  try {
    const response = await apiCall();
    
    if (response.ok && response.data) {
      return response.data;
    }
    
    // Handle API errors
    const errorMessage = response.error?.message || 'Unknown error occurred';
    console.error('API Error:', errorMessage, response.error?.traceId);
    
    // Show user-friendly error messages
    showToast(errorMessage, 'error');
    
    return null;
  } catch (error) {
    // Handle network errors
    console.error('Network Error:', error);
    showToast('Network connection failed. Please try again.', 'error');
    return null;
  }
}
```

### 3. Language & Direction Handling
```typescript
// Utility functions for RTL/LTR support
export const getTextDirection = (language: string): 'rtl' | 'ltr' => {
  const rtlLanguages = ['ar', 'he', 'ur', 'fa'];
  return rtlLanguages.includes(language) ? 'rtl' : 'ltr';
};

export const getTranslation = (
  translations: ProjectTranslation[], 
  preferredLanguage: string
): ProjectTranslation | null => {
  return translations.find(t => t.language === preferredLanguage) 
    || translations.find(t => t.language === 'ar') 
    || translations[0] 
    || null;
};

// CSS for RTL support
.rtl {
  direction: rtl;
  text-align: right;
}

.ltr {
  direction: ltr;
  text-align: left;
}
```

### 4. Security Considerations
- Always validate and sanitize user inputs
- Use HTTPS in production
- Implement proper CORS policies
- Store JWT tokens securely (httpOnly cookies recommended)
- Validate file uploads (type, size, content)

### 5. Testing
```typescript
// Unit test example for project service
describe('ProjectService', () => {
  let service: ProjectService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProjectService]
    });
    service = TestBed.inject(ProjectService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should fetch projects with filters', () => {
    const mockResponse: ApiResponse<PagedResponse<ProjectSummary>> = {
      ok: true,
      data: {
        data: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 12,
        totalPages: 1,
        hasNext: false,
        hasPrevious: false
      }
    };

    service.getProjects({ language: 'ar' }).subscribe(response => {
      expect(response.ok).toBeTruthy();
      expect(response.data?.data).toEqual([]);
    });

    const req = httpMock.expectOne(req => req.url.includes('/projects/summary'));
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('language')).toBe('ar');
    req.flush(mockResponse);
  });
});
```

This developer guide provides comprehensive examples for integrating with the El-Zahy Real Estate API across different frontend frameworks with proper error handling, performance optimization, and multilingual support.