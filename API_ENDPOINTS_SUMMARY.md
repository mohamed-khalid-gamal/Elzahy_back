# API Endpoints Summary - Multiple Images & Award Images

## Project Endpoints

### Core Project Operations
- `GET /api/projects` - Get all projects with their images
- `GET /api/projects/{id}` - Get specific project with all images
- `POST /api/projects` - Create project with multiple images (Form Data)
- `PUT /api/projects/{id}` - Update project and manage images (Form Data)
- `DELETE /api/projects/{id}` - Delete project and all associated images

### Project Image Management
- `GET /api/projects/images/{imageId}` - Download specific project image
- `POST /api/projects/{id}/images` - Add single image to existing project
- `DELETE /api/projects/images/{imageId}` - Remove specific image
- `PUT /api/projects/{projectId}/images/{imageId}/set-main` - Set image as main

## Award Endpoints

### Core Award Operations
- `GET /api/awards` - Get all awards
- `GET /api/awards/{id}` - Get specific award
- `POST /api/awards` - Create award with image (Form Data)
- `PUT /api/awards/{id}` - Update award and manage image (Form Data)
- `DELETE /api/awards/{id}` - Delete award

### Award Image Management
- `GET /api/awards/{id}/image` - Download award image

## Form Data Examples

### Create Project with Images
```
POST /api/projects
Content-Type: multipart/form-data

Name: "My Amazing Project"
Description: "Detailed description"
Images: [file1.jpg, file2.png, file3.gif]
MainImageIndex: 0
Status: "Current"
IsPublished: true
SortOrder: 1
```

### Update Project Images
```
PUT /api/projects/{id}
Content-Type: multipart/form-data

NewImages: [newfile1.jpg, newfile2.png]
RemoveImageIds: ["image-id-1", "image-id-2"]
MainImageId: "new-main-image-id"
```

### Create Award with Image
```
POST /api/awards
Content-Type: multipart/form-data

Name: "Excellence Award"
GivenBy: "Tech Conference 2024"
DateReceived: "2024-01-15"
Description: "Outstanding contribution to technology"
Image: certificate.png
IsPublished: true
```

## Response Formats

### Project Response (with images)
```json
{
  "ok": true,
  "data": {
    "id": "project-id",
    "name": "Project Name",
    "description": "Description",
    "images": [
      {
        "id": "image-id-1",
        "imageData": "base64-encoded-image",
        "contentType": "image/jpeg",
        "fileName": "image1.jpg",
        "description": "Main project view",
        "isMainImage": true,
        "sortOrder": 0
      }
    ],
    "mainImage": {
      "id": "image-id-1",
      "imageData": "base64-encoded-image",
      "isMainImage": true
    }
  }
}
```

### Award Response (with image)
```json
{
  "ok": true,
  "data": {
    "id": "award-id",
    "name": "Award Name",
    "imageData": "base64-encoded-image",
    "imageContentType": "image/png",
    "imageFileName": "certificate.png"
  }
}
```

## Key Features

### Project Images
- ? Multiple images per project (max 10)
- ? Main image designation
- ? Image descriptions
- ? Sort ordering
- ? Individual image management
- ? Batch operations during create/update

### Award Images
- ? Single image per award
- ? Image upload and replacement
- ? Image removal capability
- ? Secure binary storage

### Security
- ?? Admin role required for modifications
- ?? File validation (size, format)
- ?? Binary storage (no direct URL access)
- ?? Input sanitization