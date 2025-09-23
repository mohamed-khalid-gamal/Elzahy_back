# Multi-Image Support and Award Image Implementation

## Overview
This document outlines the comprehensive changes made to support multiple images for projects and single image support for awards in the Elzahy system.

## Summary of Changes

### 1. New Models

#### ProjectImage Model (`Elzahy/Models/ProjectImage.cs`)
- **Purpose**: Represents individual images associated with projects
- **Key Features**:
  - Stores image as binary data (`ImageData`)
  - Tracks content type and filename
  - Support for main image designation (`IsMainImage`)
  - Sort ordering capability
  - Optional description field
  - Relationship to Project and User (creator)

```csharp
public class ProjectImage
{
    public Guid Id { get; set; }
    public byte[] ImageData { get; set; }  // Binary image data
    public string ContentType { get; set; } // MIME type
    public string FileName { get; set; }
    public string? Description { get; set; }
    public bool IsMainImage { get; set; }   // Designates primary image
    public int SortOrder { get; set; }      // Display ordering
    public Guid ProjectId { get; set; }     // Foreign key to Project
    public virtual Project Project { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public virtual User? CreatedBy { get; set; }
}
```

### 2. Updated Models

#### Project Model Changes (`Elzahy/Models/Project.cs`)
- **Removed**: Single image properties (`ImageData`, `ImageContentType`, `ImageFileName`)
- **Added**: Navigation property for multiple images
- **New Feature**: `ICollection<ProjectImage> Images` for one-to-many relationship

#### Award Model Changes (`Elzahy/Models/Award.cs`)
- **Removed**: `ImageUrl` property (string-based URL)
- **Added**: Image data properties for binary storage:
  - `ImageData` (byte[])
  - `ImageContentType` (string)
  - `ImageFileName` (string)

### 3. Database Context Updates (`Elzahy/Data/AppDbContext.cs`)
- **Added**: `DbSet<ProjectImage> ProjectImages`
- **Updated**: Project configuration (removed old image properties)
- **Updated**: Award configuration (replaced ImageUrl with image data properties)
- **Added**: ProjectImage entity configuration with proper relationships and constraints

### 4. New DTOs

#### Request DTOs (`Elzahy/DTOs/RequestDtos.cs`)

**CreateProjectFormRequestDto**:
- `List<IFormFile>? Images` - Multiple image upload support
- `int? MainImageIndex` - Specify which image should be the main one

**UpdateProjectFormRequestDto**:
- `List<IFormFile>? NewImages` - Add new images
- `List<Guid>? RemoveImageIds` - Remove specific images by ID
- `Guid? MainImageId` - Change the main image

**CreateAwardFormRequestDto**:
- `IFormFile? Image` - Single image upload for awards

**UpdateAwardFormRequestDto**:
- `IFormFile? Image` - Update award image
- `bool RemoveImage` - Flag to remove existing image

#### Response DTOs (`Elzahy/DTOs/ResponseDtos.cs`)

**ProjectDto**:
- `List<ProjectImageDto> Images` - All project images
- `ProjectImageDto? MainImage` - Quick access to main image

**ProjectImageDto**:
- Complete image information including Base64 encoded data
- Metadata (description, sort order, main image flag)

**AwardDto**:
- `string? ImageData` - Base64 encoded image data
- Image metadata properties

**AwardImageDto**:
- Binary image data transfer object for file serving

### 5. Enhanced Services

#### ProjectService Updates (`Elzahy/Services/ProjectService.cs`)

**New Methods**:
- `GetProjectImageAsync(Guid imageId)` - Retrieve specific image
- `DeleteProjectImageAsync(Guid imageId)` - Remove specific image
- `AddProjectImageAsync(...)` - Add single image to existing project
- `SetMainImageAsync(Guid projectId, Guid imageId)` - Change main image designation

**Enhanced Features**:
- Maximum 10 images per project limit
- Automatic main image management
- Comprehensive image validation (size, format)
- Support for image descriptions and ordering

#### AwardService Updates (`Elzahy/Services/AwardService.cs`)

**New Methods**:
- `GetAwardImageAsync(Guid id)` - Retrieve award image for download

**Enhanced Features**:
- Image upload validation (5MB limit, format checking)
- Support for image removal during updates
- Binary image storage and retrieval

### 6. Controller Updates

#### ProjectsController (`Elzahy/Controllers/ProjectsController.cs`)

**New Endpoints**:
- `GET /api/projects/images/{imageId}` - Download specific project image
- `POST /api/projects/{id}/images` - Add image to existing project
- `DELETE /api/projects/images/{imageId}` - Remove specific image
- `PUT /api/projects/{projectId}/images/{imageId}/set-main` - Set main image

**Updated Endpoints**:
- `POST /api/projects` - Now supports multiple image upload
- `PUT /api/projects/{id}` - Enhanced with image management capabilities

#### AwardsController (`Elzahy/Controllers/AwardsController.cs`)

**New Endpoints**:
- `GET /api/awards/{id}/image` - Download award image

**Updated Endpoints**:
- `POST /api/awards` - Now supports image upload via form data
- `PUT /api/awards/{id}` - Enhanced with image management

## Technical Specifications

### Image Constraints
- **Maximum file size**: 5MB per image
- **Supported formats**: JPEG, JPG, PNG, GIF, WebP
- **Storage method**: Binary data in database
- **Maximum images per project**: 10

### Database Changes
- New `ProjectImages` table with foreign key relationships
- Updated `Projects` table (removed old image columns)
- Updated `Awards` table (replaced ImageUrl with binary data columns)
- Proper cascade delete configurations

### API Changes

#### Project Image Management Flow
1. **Create Project**: Upload multiple images during creation, optionally specify main image
2. **Update Project**: Add/remove images, change main image designation
3. **Individual Image Operations**: Add, delete, or modify individual images post-creation
4. **Image Retrieval**: Download images by their unique ID

#### Award Image Management Flow
1. **Create Award**: Upload single image during creation
2. **Update Award**: Replace or remove existing image
3. **Image Retrieval**: Download award image by award ID

## Migration Notes

### Database Migration
- Migration file: `MultipleImagesAndAwardImages`
- **Data Loss Warning**: Existing project images will be lost during migration
- **Award Migration**: ImageUrl data will be lost, replace with uploaded images

### Breaking Changes
1. **Project API**: Endpoints now expect form data instead of JSON for image operations
2. **Award API**: Similar form data requirement for image uploads
3. **Response Structure**: Project DTOs now include image arrays instead of single image data

## Usage Examples

### Creating Project with Multiple Images
```http
POST /api/projects
Content-Type: multipart/form-data

{
  "Name": "My Project",
  "Description": "Project description",
  "Images": [file1.jpg, file2.png, file3.gif],
  "MainImageIndex": 0,
  "Status": "Current",
  "IsPublished": true
}
```

### Adding Image to Existing Project
```http
POST /api/projects/{projectId}/images
Content-Type: multipart/form-data

{
  "image": file.jpg,
  "description": "Additional project screenshot",
  "isMainImage": false
}
```

### Creating Award with Image
```http
POST /api/awards
Content-Type: multipart/form-data

{
  "Name": "Achievement Award",
  "GivenBy": "Organization",
  "DateReceived": "2024-01-01",
  "Image": certificate.png
}
```

## Benefits

1. **Enhanced Visual Experience**: Projects can showcase multiple angles/aspects
2. **Better Organization**: Main image designation for consistent display
3. **Flexible Management**: Individual image operations without affecting others
4. **Consistent Storage**: Both projects and awards use secure binary storage
5. **Scalable Design**: Easy to extend for additional features (thumbnails, compression, etc.)

## Future Enhancements

1. **Image Compression**: Automatic resizing and compression
2. **Thumbnail Generation**: Create multiple sizes for responsive design
3. **Cloud Storage Integration**: Move from database to cloud storage
4. **Image Optimization**: WebP conversion and progressive loading
5. **Batch Operations**: Upload/delete multiple images simultaneously

## Security Considerations

1. **File Validation**: Comprehensive format and size checking
2. **Binary Storage**: Images stored as binary data, not accessible via direct URLs
3. **Authentication**: All modification operations require admin role
4. **Input Sanitization**: File names and descriptions are validated

This implementation provides a robust foundation for image management while maintaining security and performance considerations.