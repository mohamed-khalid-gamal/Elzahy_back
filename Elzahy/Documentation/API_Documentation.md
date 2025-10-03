# El-Zahy Real Estate API Documentation

## Overview

This is a comprehensive REST API for the El-Zahy Group real estate project management system. The API provides full CRUD operations for managing real estate projects with multilingual support, media management, and advanced filtering capabilities.

**?? Key Features:**
- **File System Storage**: Images and videos are stored on the file system, not in the database
- **Direct Media Serving**: Media files are served directly with proper caching headers
- **URL-Based Media Access**: All media is accessible via direct URLs
- **Performance Optimized**: Much faster than database binary storage

## Base URL
```
https://your-domain.com/api
```

## Authentication

All admin endpoints require JWT Bearer token authentication.

### Headers
```
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

For file uploads:
```
Authorization: Bearer <your-jwt-token>
Content-Type: multipart/form-data
```

## Media File Storage

### Storage System
- **Location**: Files are stored in `wwwroot/uploads/` directory
- **Structure**: `uploads/{type}/{filename}` (e.g., `uploads/projects/guid.jpg`)
- **Access**: Direct URL access at `/uploads/{type}/{filename}`
- **Caching**: 1-year cache headers for optimal performance

### Media URLs in Responses
Instead of base64 data, the API now returns direct URLs:

```json
{
  "images": [
    {
      "id": "guid",
      "imageUrl": "/api/projects/images/guid",
      "fileName": "project-photo.jpg",
      "fileSize": 2048576,
      "contentType": "image/jpeg"
    }
  ]
}
```

## Real Estate Project API Endpoints

### 1. Get Projects with Advanced Filtering

**Endpoint:** `GET /api/projects`

**Description:** Retrieve projects with advanced filtering, pagination, and search capabilities.

**Parameters:**
- `status` (optional): Filter by project status (`Current`, `Future`, `Past`)
- `isPublished` (optional): Filter by publication status (boolean)
- `isFeatured` (optional): Filter by featured status (boolean)
- `propertyType` (optional): Filter by property type (e.g., "Residential", "Commercial")
- `location` (optional): Filter by location
- `priceMin` (optional): Minimum price filter (decimal)
- `priceMax` (optional): Maximum price filter (decimal)
- `searchTerm` (optional): Search in name, description, location
- `language` (optional): Filter by translation language ("ar", "en")
- `startDateFrom` (optional): Filter projects starting from this date
- `startDateTo` (optional): Filter projects starting until this date
- `sortBy` (optional): Sort field (`SortOrder`, `CreatedAt`, `Name`, `StartDate`, `PriceStart`, `Location`)
- `sortDescending` (optional): Sort direction (boolean, default: false)
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 12)

**Example Request:**
```http
GET /api/projects?status=Current&propertyType=Residential&location=Cairo&page=1&pageSize=12&language=ar
```

**Response:**
```json
{
  "ok": true,
  "data": {
    "data": [
      {
        "id": "guid",
        "createdAt": "2024-01-15T10:00:00.000Z",
        "updatedAt": "2024-01-15T10:00:00.000Z",
        "name": "El-Zahy Residences",
        "description": "Luxury residential compound",
        "images": [
          {
            "id": "image-guid",
            "imageUrl": "/api/projects/images/image-guid",
            "contentType": "image/jpeg",
            "fileName": "main-view.jpg",
            "fileSize": 2048576,
            "isMainImage": true,
            "sortOrder": 0
          }
        ],
        "mainImage": {
          "id": "image-guid",
          "imageUrl": "/api/projects/images/image-guid",
          "contentType": "image/jpeg",
          "fileName": "main-view.jpg",
          "fileSize": 2048576,
          "isMainImage": true
        },
        "videos": [
          {
            "id": "video-guid",
            "videoUrl": "/api/projects/videos/video-guid",
            "contentType": "video/mp4",
            "fileName": "property-tour.mp4",
            "fileSize": 52428800,
            "isMainVideo": true
          }
        ],
        "status": "Current",
        "companyUrl": "https://elzahygroup.com/project1",
        "googleMapsUrl": "https://maps.google.com/...",
        "location": "New Cairo, Egypt",
        "propertyType": "Residential",
        "totalUnits": 150,
        "projectArea": 25000.50,
        "priceStart": 2500000.00,
        "priceEnd": 8500000.00,
        "priceCurrency": "EGP",
        "priceRange": "2,500,000 - 8,500,000 EGP",
        "isPublished": true,
        "isFeatured": true,
        "sortOrder": 1,
        "translations": [
          {
            "language": "ar",
            "direction": "RTL",
            "title": "???? ?????? ??????",
            "description": "???? ???? ????..."
          },
          {
            "language": "en",
            "direction": "LTR",
            "title": "El-Zahy Residences",
            "description": "Luxury residential compound..."
          }
        ]
      }
    ],
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 12,
    "totalPages": 4,
    "hasPrevious": false,
    "hasNext": true,
    "nextPage": 2,
    "prevPage": null
  }
}
```

### 2. Get Projects Summary (Lightweight)

**Endpoint:** `GET /api/projects/summary`

**Description:** Get a lightweight version of projects for listing pages (includes only essential fields and main image).

**Parameters:** Same as above endpoint

**Example Request:**
```http
GET /api/projects/summary?isFeatured=true&language=ar&pageSize=6
```

**Response:** Same structure as above but with reduced payload size.

### 3. Get Featured Projects

**Endpoint:** `GET /api/projects/featured`

**Description:** Get featured projects for homepage display.

**Parameters:**
- `count` (optional): Number of projects to return (default: 6)
- `language` (optional): Translation language filter

**Example Request:**
```http
GET /api/projects/featured?count=8&language=ar
```

### 4-8. Other Project Endpoints

All other project retrieval endpoints work the same way, returning direct URLs instead of base64 data.

## Admin Endpoints (Require Authentication)

### 9. Create Project

**Endpoint:** `POST /api/projects`

**Description:** Create a new project with images, videos, and translations.

**Content-Type:** `multipart/form-data`

**Form Fields:**
```
name: string (required, max 200 chars)
description: string (required)
status: enum (Current|Future|Past)
companyUrl: string (optional, max 500 chars)
googleMapsUrl: string (optional, max 500 chars)
location: string (optional, max 200 chars)
propertyType: string (optional, max 100 chars)
totalUnits: integer (optional)
projectArea: decimal (optional)
priceStart: decimal (optional)
priceEnd: decimal (optional)
priceCurrency: string (optional, max 10 chars, default: "EGP")
isPublished: boolean (default: true)
isFeatured: boolean (default: false)
sortOrder: integer (default: 0)
images: file[] (optional - multiple image files)
mainImageIndex: integer (optional - index of main image in images array)
videos: file[] (optional - multiple video files)
mainVideoIndex: integer (optional - index of main video in videos array)
translations: JSON array (optional)
```

**File Upload Limits:**
- **Images**: Max 10MB per file, formats: JPEG, PNG, GIF, WebP
- **Videos**: Max 100MB per file, formats: MP4, WebM, OGG, MOV, AVI, WMV

**Example Response:**
```json
{
  "ok": true,
  "data": {
    "id": "new-project-guid",
    "name": "El-Zahy Tower",
    "images": [
      {
        "id": "image-guid",
        "imageUrl": "/api/projects/images/image-guid",
        "fileName": "uploaded-image.jpg",
        "fileSize": 1024000,
        "contentType": "image/jpeg",
        "isMainImage": true
      }
    ]
  }
}
```

## Media Management

### 13. Get Project Image

**Endpoint:** `GET /api/projects/images/{imageId}`

**Description:** Serves the actual image file with proper headers.

**Response:** 
- **Content-Type**: Original image MIME type
- **Cache-Control**: `public, max-age=31536000` (1 year)
- **Content-Length**: File size
- **Accept-Ranges**: `bytes` (supports range requests)

**Usage:**
```html
<!-- Direct usage in HTML -->
<img src="/api/projects/images/550e8400-e29b-41d4-a716-446655440000" alt="Project Image">

<!-- Or via full URL -->
<img src="https://your-domain.com/api/projects/images/550e8400-e29b-41d4-a716-446655440000">
```

### 17. Get Project Video

**Endpoint:** `GET /api/projects/videos/{videoId}`

**Description:** Serves the actual video file with proper headers.

**Response:**
- **Content-Type**: Original video MIME type
- **Cache-Control**: `public, max-age=31536000` (1 year)
- **Accept-Ranges**: `bytes` (supports video streaming)

**Usage:**
```html
<!-- Direct usage in HTML5 video -->
<video controls>
  <source src="/api/projects/videos/550e8400-e29b-41d4-a716-446655440000" type="video/mp4">
</video>
```

### File Upload Endpoints

#### 14. Add Project Image

**Endpoint:** `POST /api/projects/{id}/images`

**Content-Type:** `multipart/form-data`

**Form Fields:**
```
image: file (required, max 10MB)
description: string (optional)
isMainImage: boolean (optional, default: false)
```

**Response:**
```json
{
  "ok": true,
  "data": {
    "id": "new-image-guid",
    "imageUrl": "/api/projects/images/new-image-guid",
    "fileName": "uploaded-file.jpg",
    "fileSize": 2048576,
    "contentType": "image/jpeg",
    "isMainImage": false,
    "sortOrder": 3
  }
}
```

## Performance Benefits

### File System vs Database Storage

| Aspect | Database Storage | File System Storage |
|--------|------------------|-------------------|
| **Response Size** | Large (base64 encoded) | Small (URLs only) |
| **Load Time** | Slow (database query) | Fast (direct file serve) |
| **Caching** | Limited | Full browser/CDN caching |
| **Memory Usage** | High (loads all data) | Low (streams files) |
| **Scalability** | Database bottleneck | Web server optimized |
| **CDN Support** | No | Yes (direct file URLs) |

### Caching Strategy

1. **Browser Caching**: 1-year cache headers on all media files
2. **CDN Ready**: Direct file URLs work with any CDN
3. **Range Requests**: Support for video streaming and image progressive loading
4. **Conditional Requests**: ETag and Last-Modified headers for efficient caching

## Migration from Database Storage

### For Existing Data

If you have existing projects with database-stored media, you'll need to migrate them:

```bash
# Run the migration to update schema
dotnet ef database update

# Note: Existing binary data will be lost in this migration
# You should export existing images/videos before running the migration
```

### Data Migration Script (if needed)

If you have existing data to preserve, create a data migration script before applying the schema migration:

```csharp
// Example migration script (run before schema migration)
var projects = context.Projects
    .Include(p => p.Images)
    .Include(p => p.Videos)
    .ToList();

foreach (var project in projects)
{
    foreach (var image in project.Images)
    {
        if (image.ImageData != null && image.ImageData.Length > 0)
        {
            // Save to file system
            var fileName = $"{image.Id}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine("wwwroot", "uploads", "projects", fileName);
            await File.WriteAllBytesAsync(filePath, image.ImageData);
            
            // Update image record
            image.FilePath = $"uploads/projects/{fileName}";
            image.FileSize = image.ImageData.Length;
        }
    }
}

await context.SaveChangesAsync();
```

## Frontend Integration Updates

### Updated JavaScript Usage

```javascript
// Before (base64 data)
const imageData = `data:${image.contentType};base64,${image.imageData}`;

// After (direct URLs)
const imageUrl = image.imageUrl; // "/api/projects/images/guid"
// Or with full domain
const fullImageUrl = `${API_BASE_URL}${image.imageUrl}`;

// Usage
<img src={imageUrl} alt="Project" />
```

### React Component Example

```jsx
function ProjectImage({ image }) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  return (
    <div className="project-image">
      {loading && <div className="loading-placeholder">Loading...</div>}
      <img
        src={image.imageUrl}
        alt={image.description || 'Project image'}
        onLoad={() => setLoading(false)}
        onError={() => {
          setLoading(false);
          setError(true);
        }}
        style={{ display: loading ? 'none' : 'block' }}
      />
      {error && <div className="error-placeholder">Failed to load image</div>}
    </div>
  );
}
```

### Video Streaming Example

```jsx
function ProjectVideo({ video }) {
  return (
    <video
      controls
      preload="metadata"
      poster={video.thumbnailUrl} // If you add thumbnails later
    >
      <source src={video.videoUrl} type={video.contentType} />
      Your browser does not support the video tag.
    </video>
  );
}
```

## Error Handling

### File Not Found (404)

```json
{
  "ok": false,
  "error": {
    "message": "Image not found",
    "internalCode": 4004
  }
}
```

### File Upload Errors

```json
{
  "ok": false,
  "error": {
    "message": "Invalid image format. Allowed: JPEG, PNG, GIF, WebP",
    "internalCode": 4002
  }
}
```

## Production Deployment

### File Storage Considerations

1. **Backup Strategy**: Include `wwwroot/uploads/` in your backup strategy
2. **File Permissions**: Ensure web server has read/write access to uploads directory
3. **Disk Space**: Monitor disk usage for uploaded files
4. **CDN Integration**: Point CDN to serve files directly from uploads directory

### Recommended Setup

```nginx
# Nginx configuration for serving static files
location /uploads/ {
    alias /var/www/elzahy/wwwroot/uploads/;
    expires 1y;
    add_header Cache-Control "public, immutable";
    add_header Access-Control-Allow-Origin "*";
}

# API routes
location /api/ {
    proxy_pass http://localhost:5000;
    # ... other proxy settings
}
```

This file-based storage system provides significantly better performance, scalability, and user experience compared to database binary storage.