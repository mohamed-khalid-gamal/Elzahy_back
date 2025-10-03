# Elzahy Project Gallery Integration Guide for Angular Frontend

## Overview

This document provides comprehensive instructions for integrating with the Elzahy backend API to display project images and videos in an Angular frontend application after backend deployment.

## Backend Architecture Summary

The Elzahy backend provides a robust file storage system with the following key components:

- **Projects Controller**: `/api/projects` - Main endpoint for project operations
- **File Storage**: Uses filesystem storage in `wwwroot/uploads/` directory
- **Image Management**: Multiple images per project with main image support
- **Video Management**: Multiple videos per project with main video support
- **Multi-language Support**: Project translations for internationalization

## API Endpoints for Gallery Integration

### 1. Get Project with Full Gallery Details

```http
GET /api/projects/{projectId}?language={language}
```

**Purpose**: Retrieve complete project information including all images and videos.

**Parameters**:
- `projectId` (required): GUID of the project
- `language` (optional): Language code for translations (e.g., "en", "ar")

**Response Structure**:
```json
{
  "ok": true,
  "data": {
    "id": "guid",
    "name": "Project Name",
    "description": "Project Description",
    "images": [
      {
        "id": "image-guid",
        "imageUrl": "/api/projects/images/{imageId}",
        "contentType": "image/jpeg",
        "fileName": "image.jpg",
        "fileSize": 1024000,
        "description": "Image description",
        "isMainImage": true,
        "sortOrder": 0
      }
    ],
    "mainImage": {
      "id": "main-image-guid",
      "imageUrl": "/api/projects/images/{mainImageId}",
      "contentType": "image/jpeg",
      "fileName": "main-image.jpg",
      "isMainImage": true
    },
    "videos": [
      {
        "id": "video-guid",
        "videoUrl": "/api/projects/videos/{videoId}",
        "contentType": "video/mp4",
        "fileName": "video.mp4",
        "fileSize": 50000000,
        "description": "Video description",
        "isMainVideo": true,
        "sortOrder": 0
      }
    ],
    "mainVideo": {
      "id": "main-video-guid",
      "videoUrl": "/api/projects/videos/{mainVideoId}",
      "contentType": "video/mp4",
      "fileName": "main-video.mp4",
      "isMainVideo": true
    },
    "status": "Completed",
    "location": "Cairo, Egypt",
    "propertyType": "Residential",
    "priceRange": "2,000,000 - 5,000,000 EGP",
    "translations": [
      {
        "language": "ar",
        "direction": "RTL",
        "title": "??? ???????",
        "description": "??? ???????"
      }
    ]
  }
}
```

### 2. Get Project Image File

```http
GET /api/projects/images/{imageId}
```

**Purpose**: Retrieve the actual image file for display.

**Response**: Binary image data with appropriate content-type headers.

**Features**:
- Supports range requests for efficient loading
- Automatic content-type detection
- Caching headers (1 year cache)

### 3. Get Project Video File

```http
GET /api/projects/videos/{videoId}
```

**Purpose**: Retrieve the actual video file for playback.

**Response**: Binary video data with appropriate content-type headers.

**Features**:
- Supports range requests for streaming
- Automatic content-type detection
- Caching headers (1 year cache)

### 4. Get Projects Summary (for Listing Pages)

```http
GET /api/projects/summary?page={page}&pageSize={pageSize}&language={language}
```

**Purpose**: Get lightweight project data for listing pages.

**Parameters**:
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 12)
- `language` (optional): Language code for translations

**Response**: Paginated list with main images only for performance.

### 5. Get Featured Projects

```http
GET /api/projects/featured?count={count}&language={language}
```

**Purpose**: Get featured projects for homepage display.

**Parameters**:
- `count` (optional): Number of projects to return (default: 6)
- `language` (optional): Language code for translations

## Angular Service Implementation

### 1. Create Project Service

```typescript
// services/project.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface ProjectImage {
  id: string;
  imageUrl: string;
  contentType: string;
  fileName: string;
  fileSize: number;
  description?: string;
  isMainImage: boolean;
  sortOrder: number;
}

export interface ProjectVideo {
  id: string;
  videoUrl: string;
  contentType: string;
  fileName: string;
  fileSize: number;
  description?: string;
  isMainVideo: boolean;
  sortOrder: number;
}

export interface Project {
  id: string;
  name: string;
  description: string;
  images: ProjectImage[];
  mainImage?: ProjectImage;
  videos: ProjectVideo[];
  mainVideo?: ProjectVideo;
  status: string;
  location?: string;
  propertyType?: string;
  priceRange?: string;
  translations: ProjectTranslation[];
}

export interface ProjectTranslation {
  language: string;
  direction: string;
  title: string;
  description: string;
}

export interface ApiResponse<T> {
  ok: boolean;
  data?: T;
  error?: {
    message: string;
    internalCode?: number;
  };
}

export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private apiUrl = `${environment.apiUrl}/api/projects`;

  constructor(private http: HttpClient) {}

  getProject(projectId: string, language?: string): Observable<ApiResponse<Project>> {
    let params = new HttpParams();
    if (language) {
      params = params.set('language', language);
    }
    
    return this.http.get<ApiResponse<Project>>(`${this.apiUrl}/${projectId}`, { params });
  }

  getProjects(page = 1, pageSize = 12, language?: string): Observable<ApiResponse<PagedResponse<Project>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (language) {
      params = params.set('language', language);
    }
    
    return this.http.get<ApiResponse<PagedResponse<Project>>>(this.apiUrl, { params });
  }

  getFeaturedProjects(count = 6, language?: string): Observable<ApiResponse<Project[]>> {
    let params = new HttpParams()
      .set('count', count.toString());
    
    if (language) {
      params = params.set('language', language);
    }
    
    return this.http.get<ApiResponse<Project[]>>(`${this.apiUrl}/featured`, { params });
  }

  // Helper method to get full image URL
  getImageUrl(imageId: string): string {
    return `${environment.apiUrl}/api/projects/images/${imageId}`;
  }

  // Helper method to get full video URL
  getVideoUrl(videoId: string): string {
    return `${environment.apiUrl}/api/projects/videos/${videoId}`;
  }
}
```

### 2. Environment Configuration

```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://your-backend-domain.com' // Replace with your deployed backend URL
};

// environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://your-production-backend-domain.com' // Replace with your production backend URL
};
```

## Angular Component Implementation

### 1. Project Gallery Component

```typescript
// components/project-gallery/project-gallery.component.ts
import { Component, OnInit, Input } from '@angular/core';
import { Project, ProjectService } from '../../services/project.service';

@Component({
  selector: 'app-project-gallery',
  templateUrl: './project-gallery.component.html',
  styleUrls: ['./project-gallery.component.scss']
})
export class ProjectGalleryComponent implements OnInit {
  @Input() projectId?: string;
  @Input() language?: string;
  
  project?: Project;
  selectedMediaIndex = 0;
  isLoading = false;
  error?: string;
  
  // Combined media array for unified gallery navigation
  get allMedia(): Array<{type: 'image' | 'video', item: any, index: number}> {
    if (!this.project) return [];
    
    const media: Array<{type: 'image' | 'video', item: any, index: number}> = [];
    
    // Add images
    this.project.images.forEach((image, index) => {
      media.push({ type: 'image', item: image, index });
    });
    
    // Add videos
    this.project.videos.forEach((video, index) => {
      media.push({ type: 'video', item: video, index: index + this.project!.images.length });
    });
    
    return media.sort((a, b) => a.item.sortOrder - b.item.sortOrder);
  }

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    if (this.projectId) {
      this.loadProject();
    }
  }

  loadProject(): void {
    if (!this.projectId) return;
    
    this.isLoading = true;
    this.error = undefined;
    
    this.projectService.getProject(this.projectId, this.language)
      .subscribe({
        next: (response) => {
          if (response.ok && response.data) {
            this.project = response.data;
          } else {
            this.error = response.error?.message || 'Failed to load project';
          }
          this.isLoading = false;
        },
        error: (err) => {
          this.error = 'Network error occurred';
          this.isLoading = false;
          console.error('Error loading project:', err);
        }
      });
  }

  selectMedia(index: number): void {
    this.selectedMediaIndex = index;
  }

  nextMedia(): void {
    const maxIndex = this.allMedia.length - 1;
    this.selectedMediaIndex = this.selectedMediaIndex >= maxIndex ? 0 : this.selectedMediaIndex + 1;
  }

  previousMedia(): void {
    const maxIndex = this.allMedia.length - 1;
    this.selectedMediaIndex = this.selectedMediaIndex <= 0 ? maxIndex : this.selectedMediaIndex - 1;
  }

  getImageUrl(imageId: string): string {
    return this.projectService.getImageUrl(imageId);
  }

  getVideoUrl(videoId: string): string {
    return this.projectService.getVideoUrl(videoId);
  }

  // Get translated content based on current language
  getTranslatedContent(): { title: string; description: string } {
    if (!this.project) return { title: '', description: '' };
    
    if (this.language && this.project.translations.length > 0) {
      const translation = this.project.translations.find(t => t.language === this.language);
      if (translation) {
        return {
          title: translation.title,
          description: translation.description
        };
      }
    }
    
    // Fallback to default content
    return {
      title: this.project.name,
      description: this.project.description
    };
  }
}
```

### 2. Project Gallery Template

```html
<!-- components/project-gallery/project-gallery.component.html -->
<div class="project-gallery" *ngIf="project">
  <!-- Loading State -->
  <div class="loading-container" *ngIf="isLoading">
    <div class="spinner"></div>
    <p>Loading gallery...</p>
  </div>

  <!-- Error State -->
  <div class="error-container" *ngIf="error">
    <p class="error-message">{{ error }}</p>
    <button (click)="loadProject()" class="retry-btn">Retry</button>
  </div>

  <!-- Gallery Content -->
  <div class="gallery-content" *ngIf="!isLoading && !error">
    <!-- Project Info -->
    <div class="project-info">
      <h1>{{ getTranslatedContent().title }}</h1>
      <p class="description">{{ getTranslatedContent().description }}</p>
      
      <div class="project-meta">
        <span class="status">{{ project.status }}</span>
        <span class="location" *ngIf="project.location">{{ project.location }}</span>
        <span class="property-type" *ngIf="project.propertyType">{{ project.propertyType }}</span>
        <span class="price-range" *ngIf="project.priceRange">{{ project.priceRange }}</span>
      </div>
    </div>

    <!-- Main Media Display -->
    <div class="main-media-container" *ngIf="allMedia.length > 0">
      <div class="media-viewer">
        <!-- Image Display -->
        <img 
          *ngIf="allMedia[selectedMediaIndex]?.type === 'image'"
          [src]="getImageUrl(allMedia[selectedMediaIndex].item.id)"
          [alt]="allMedia[selectedMediaIndex].item.description || 'Project Image'"
          class="main-media-image"
          (error)="onMediaError($event)"
        >
        
        <!-- Video Display -->
        <video 
          *ngIf="allMedia[selectedMediaIndex]?.type === 'video'"
          [src]="getVideoUrl(allMedia[selectedMediaIndex].item.id)"
          class="main-media-video"
          controls
          preload="metadata"
          (error)="onMediaError($event)"
        >
          Your browser does not support video playback.
        </video>
      </div>

      <!-- Navigation Controls -->
      <div class="media-controls" *ngIf="allMedia.length > 1">
        <button 
          class="nav-btn prev-btn" 
          (click)="previousMedia()"
          [attr.aria-label]="'Previous media'"
        >
          ‹
        </button>
        
        <div class="media-counter">
          {{ selectedMediaIndex + 1 }} / {{ allMedia.length }}
        </div>
        
        <button 
          class="nav-btn next-btn" 
          (click)="nextMedia()"
          [attr.aria-label]="'Next media'"
        >
          ›
        </button>
      </div>
    </div>

    <!-- Thumbnail Gallery -->
    <div class="thumbnail-gallery" *ngIf="allMedia.length > 1">
      <div 
        class="thumbnail-container"
        *ngFor="let media of allMedia; let i = index"
        [class.active]="i === selectedMediaIndex"
        (click)="selectMedia(i)"
      >
        <!-- Image Thumbnail -->
        <img 
          *ngIf="media.type === 'image'"
          [src]="getImageUrl(media.item.id)"
          [alt]="media.item.description || 'Project Image'"
          class="thumbnail-image"
          loading="lazy"
        >
        
        <!-- Video Thumbnail -->
        <div *ngIf="media.type === 'video'" class="video-thumbnail">
          <video 
            [src]="getVideoUrl(media.item.id)"
            class="thumbnail-video"
            preload="metadata"
            muted
          ></video>
          <div class="play-icon">?</div>
        </div>
        
        <!-- Media Type Indicator -->
        <div class="media-type-badge">
          {{ media.type === 'image' ? '??' : '??' }}
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div class="empty-gallery" *ngIf="allMedia.length === 0">
      <p>No images or videos available for this project.</p>
    </div>
  </div>
</div>
```

### 3. Project Gallery Styles

```scss
/* components/project-gallery/project-gallery.component.scss */
.project-gallery {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;

  .loading-container, .error-container {
    text-align: center;
    padding: 60px 20px;
    
    .spinner {
      width: 40px;
      height: 40px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid #3498db;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 20px;
    }
    
    .error-message {
      color: #e74c3c;
      margin-bottom: 20px;
    }
    
    .retry-btn {
      background: #3498db;
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 5px;
      cursor: pointer;
      
      &:hover {
        background: #2980b9;
      }
    }
  }

  .project-info {
    margin-bottom: 30px;
    
    h1 {
      font-size: 2.5em;
      margin-bottom: 15px;
      color: #2c3e50;
    }
    
    .description {
      font-size: 1.1em;
      line-height: 1.6;
      color: #34495e;
      margin-bottom: 20px;
    }
    
    .project-meta {
      display: flex;
      flex-wrap: wrap;
      gap: 15px;
      
      span {
        background: #ecf0f1;
        padding: 5px 12px;
        border-radius: 15px;
        font-size: 0.9em;
        color: #2c3e50;
        
        &.status {
          background: #3498db;
          color: white;
        }
        
        &.price-range {
          background: #27ae60;
          color: white;
          font-weight: bold;
        }
      }
    }
  }

  .main-media-container {
    margin-bottom: 30px;
    
    .media-viewer {
      position: relative;
      background: #000;
      border-radius: 10px;
      overflow: hidden;
      margin-bottom: 15px;
      
      .main-media-image, .main-media-video {
        width: 100%;
        height: auto;
        max-height: 70vh;
        object-fit: contain;
        display: block;
      }
    }
    
    .media-controls {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 20px;
      
      .nav-btn {
        background: #3498db;
        color: white;
        border: none;
        width: 50px;
        height: 50px;
        border-radius: 50%;
        font-size: 24px;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.3s ease;
        
        &:hover {
          background: #2980b9;
          transform: scale(1.1);
        }
        
        &:active {
          transform: scale(0.95);
        }
      }
      
      .media-counter {
        background: rgba(0, 0, 0, 0.7);
        color: white;
        padding: 8px 16px;
        border-radius: 20px;
        font-weight: bold;
      }
    }
  }

  .thumbnail-gallery {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
    gap: 15px;
    
    .thumbnail-container {
      position: relative;
      cursor: pointer;
      border-radius: 8px;
      overflow: hidden;
      transition: all 0.3s ease;
      border: 3px solid transparent;
      
      &:hover {
        transform: translateY(-5px);
        box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
      }
      
      &.active {
        border-color: #3498db;
        transform: translateY(-3px);
      }
      
      .thumbnail-image, .thumbnail-video {
        width: 100%;
        height: 100px;
        object-fit: cover;
        display: block;
      }
      
      .video-thumbnail {
        position: relative;
        background: #000;
        
        .play-icon {
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          color: white;
          font-size: 24px;
          background: rgba(0, 0, 0, 0.7);
          border-radius: 50%;
          width: 40px;
          height: 40px;
          display: flex;
          align-items: center;
          justify-content: center;
        }
      }
      
      .media-type-badge {
        position: absolute;
        top: 5px;
        right: 5px;
        background: rgba(0, 0, 0, 0.7);
        color: white;
        padding: 2px 6px;
        border-radius: 10px;
        font-size: 12px;
      }
    }
  }

  .empty-gallery {
    text-align: center;
    padding: 60px 20px;
    color: #7f8c8d;
    font-style: italic;
  }
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

// Responsive Design
@media (max-width: 768px) {
  .project-gallery {
    padding: 15px;
    
    .project-info h1 {
      font-size: 2em;
    }
    
    .project-meta {
      justify-content: center;
    }
    
    .media-controls .nav-btn {
      width: 40px;
      height: 40px;
      font-size: 18px;
    }
    
    .thumbnail-gallery {
      grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
      gap: 10px;
      
      .thumbnail-container {
        .thumbnail-image, .thumbnail-video {
          height: 80px;
        }
      }
    }
  }
}
```

## Usage Examples

### 1. Display Project Gallery in a Route Component

```typescript
// pages/project-detail/project-detail.component.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-project-detail',
  template: `
    <div class="project-detail-page">
      <app-project-gallery 
        [projectId]="projectId" 
        [language]="currentLanguage">
      </app-project-gallery>
    </div>
  `
})
export class ProjectDetailComponent implements OnInit {
  projectId?: string;
  currentLanguage = 'en'; // Get from your language service

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.projectId = params['id'];
    });
  }
}
```

### 2. Display Featured Projects on Homepage

```typescript
// components/featured-projects/featured-projects.component.ts
import { Component, OnInit } from '@angular/core';
import { Project, ProjectService } from '../../services/project.service';

@Component({
  selector: 'app-featured-projects',
  template: `
    <section class="featured-projects">
      <h2>Featured Projects</h2>
      <div class="projects-grid">
        <div 
          class="project-card" 
          *ngFor="let project of featuredProjects"
          [routerLink]="['/projects', project.id]"
        >
          <div class="project-image">
            <img 
              [src]="getProjectImageUrl(project)"
              [alt]="project.name"
              loading="lazy"
            >
            <div class="project-overlay">
              <h3>{{ project.name }}</h3>
              <p>{{ project.location }}</p>
              <span class="price">{{ project.priceRange }}</span>
            </div>
          </div>
        </div>
      </div>
    </section>
  `
})
export class FeaturedProjectsComponent implements OnInit {
  featuredProjects: Project[] = [];

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    this.projectService.getFeaturedProjects(6, 'en')
      .subscribe(response => {
        if (response.ok && response.data) {
          this.featuredProjects = response.data;
        }
      });
  }

  getProjectImageUrl(project: Project): string {
    if (project.mainImage) {
      return this.projectService.getImageUrl(project.mainImage.id);
    }
    if (project.images.length > 0) {
      return this.projectService.getImageUrl(project.images[0].id);
    }
    return '/assets/images/placeholder.jpg';
  }
}
```

## Error Handling and Best Practices

### 1. Image Loading Error Handler

```typescript
// Add to your component
onMediaError(event: any): void {
  console.error('Media loading error:', event);
  // Set a placeholder image
  event.target.src = '/assets/images/error-placeholder.jpg';
}
```

### 2. Loading States and Caching

```typescript
// Enhanced service with caching
@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private projectCache = new Map<string, Project>();

  getProject(projectId: string, language?: string): Observable<ApiResponse<Project>> {
    const cacheKey = `${projectId}-${language || 'default'}`;
    
    // Check cache first
    if (this.projectCache.has(cacheKey)) {
      return of({
        ok: true,
        data: this.projectCache.get(cacheKey)
      });
    }

    // Fetch from API
    return this.http.get<ApiResponse<Project>>(`${this.apiUrl}/${projectId}`, { params })
      .pipe(
        tap(response => {
          if (response.ok && response.data) {
            this.projectCache.set(cacheKey, response.data);
          }
        })
      );
  }
}
```

### 3. Lazy Loading and Performance Optimization

```html
<!-- Use loading="lazy" for images -->
<img 
  [src]="imageUrl"
  [alt]="altText"
  loading="lazy"
  (load)="onImageLoad()"
  (error)="onImageError($event)"
>

<!-- Preload critical images -->
<link rel="preload" [href]="mainImageUrl" as="image">
```

## Security Considerations

1. **Content Security Policy**: Ensure your CSP allows loading images/videos from your backend domain
2. **CORS Configuration**: The backend is configured to accept requests from your frontend domain
3. **Authentication**: Some endpoints require authentication (image/video management)
4. **File Validation**: The backend validates file types and sizes

## Deployment Checklist

1. ? Update environment.ts with correct backend URL
2. ? Configure CORS in backend for your frontend domain
3. ? Ensure upload directories exist and have proper permissions
4. ? Set up CDN for static file serving (optional but recommended)
5. ? Configure caching headers for better performance
6. ? Test image/video loading across different devices and browsers
7. ? Implement proper error handling and fallback images
8. ? Add loading states for better user experience

## Additional Features

### Multi-language Support

The API supports project translations. To implement language switching:

```typescript
switchLanguage(language: string): void {
  this.currentLanguage = language;
  this.loadProject(); // Reload with new language
}
```

### Advanced Filtering

```typescript
// Filter projects by property type
getProjectsByPropertyType(propertyType: string): Observable<ApiResponse<PagedResponse<Project>>> {
  return this.http.get<ApiResponse<PagedResponse<Project>>>(
    `${this.apiUrl}/by-property-type/${propertyType}`
  );
}

// Search projects
searchProjects(searchTerm: string): Observable<ApiResponse<PagedResponse<Project>>> {
  const params = new HttpParams().set('searchTerm', searchTerm);
  return this.http.get<ApiResponse<PagedResponse<Project>>>(
    `${this.apiUrl}/search`, 
    { params }
  );
}
```

This comprehensive guide provides everything needed to successfully integrate the Elzahy backend gallery system with an Angular frontend application.