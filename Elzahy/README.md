# El-Zahy Real Estate API

A comprehensive REST API for managing real estate projects with advanced multilingual support, media management, and sophisticated filtering capabilities.

## ??? Features

### Real Estate Focused
- **Property Management**: Comprehensive project details including location, property type, pricing, and area
- **Media Gallery**: Multiple images and videos per project with main media designation
- **Pricing Management**: Flexible price ranges with currency support
- **Unit Tracking**: Total units and project area management
- **Featured Projects**: Highlight important or premium projects

### Multilingual Support
- **RTL/LTR Support**: Automatic text direction detection for Arabic and English
- **Translation Management**: Complete translation system for project details
- **Language Filtering**: Filter content by available translations
- **Direction-Aware UI**: Frontend can adapt layout based on text direction

### Advanced Search & Filtering
- **Comprehensive Filters**: Status, property type, location, price range, and more
- **Full-Text Search**: Search across project names, descriptions, and translations
- **Pagination**: Efficient pagination with metadata
- **Sorting Options**: Multiple sorting criteria including custom sort order

### Performance Optimized
- **Lightweight Endpoints**: Summary endpoints for listing pages
- **Database Indexing**: Optimized queries with strategic indexes
- **Selective Loading**: Load only necessary data based on endpoint
- **Response Optimization**: 60% smaller payloads for listing operations

## ?? Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server or MySQL
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/mohamed-khalid-gamal/Elzahy_back.git
   cd Elzahy_back
   ```

2. **Configure the database**
   ```bash
   # SQL Server (recommended)
   set ConnectionStrings__DefaultConnection="Server=localhost;Database=ElzahyDB;Trusted_Connection=true;"
   
   # Or MySQL
   set MYSQL_CONNECTION_STRING="Server=localhost;Database=ElzahyDB;Uid=root;Pwd=password;"
   ```

3. **Set JWT configuration**
   ```bash
   set DOTNET_JWT_KEY="your-super-secret-jwt-key-here"
   ```

4. **Run migrations**
   ```bash
   cd Elzahy
   dotnet ef database update
   ```

5. **Start the API**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7000` with Swagger UI at the root URL.

## ?? Documentation

- **[API Documentation](Documentation/API_Documentation.md)** - Complete endpoint reference
- **[Developer Guide](Documentation/Developer_Guide.md)** - Frontend integration examples
- **[Changelog](Documentation/CHANGELOG.md)** - Version history and updates

## ?? API Endpoints Overview

### Public Endpoints
```
GET  /api/projects              - Advanced project filtering
GET  /api/projects/summary      - Lightweight project listing
GET  /api/projects/featured     - Featured projects
GET  /api/projects/{id}         - Project details
GET  /api/projects/search       - Full-text search
GET  /api/projects/images/{id}  - Project images
GET  /api/projects/videos/{id}  - Project videos
```

### Admin Endpoints (Authentication Required)
```
POST   /api/projects                    - Create project
PUT    /api/projects/{id}               - Update project
DELETE /api/projects/{id}               - Delete project
POST   /api/projects/{id}/images        - Add images
POST   /api/projects/{id}/videos        - Add videos
POST   /api/projects/{id}/translations  - Manage translations
GET    /api/projects/analytics/stats    - Project statistics
```

## ?? Real Estate Data Model

### Project Properties
```typescript
{
  // Basic Information
  id: string;
  name: string;
  description: string;
  status: 'Current' | 'Future' | 'Past';
  
  // Real Estate Specific
  location: string;              // "New Cairo, Egypt"
  propertyType: string;          // "Residential", "Commercial", "Mixed-use"
  totalUnits: number;            // 150 units
  projectArea: number;           // 25000.50 sqm
  priceStart: number;            // 2500000.00 EGP
  priceEnd: number;              // 8500000.00 EGP
  priceCurrency: string;         // "EGP"
  
  // Media & Features
  images: ProjectImage[];
  videos: ProjectVideo[];
  isFeatured: boolean;
  
  // Multilingual
  translations: ProjectTranslation[];
}
```

### Translation Support
```typescript
{
  language: 'ar' | 'en';         // Language code
  direction: 'RTL' | 'LTR';      // Text direction
  title: string;                 // Translated title
  description: string;           // Translated description
}
```

## ?? Example Usage

### Get Featured Projects
```javascript
const response = await fetch('/api/projects/featured?count=6&language=ar');
const { data: projects } = await response.json();

projects.forEach(project => {
  const translation = project.translations.find(t => t.language === 'ar');
  console.log(`${translation.title} - ${project.priceRange}`);
  console.log(`Direction: ${translation.direction}`); // RTL for Arabic
});
```

### Search Projects
```javascript
const searchResults = await fetch('/api/projects/search?searchTerm=residential&location=Cairo&language=ar');
const { data } = await searchResults.json();

console.log(`Found ${data.totalCount} projects in ${data.totalPages} pages`);
```

### Create Project (Admin)
```javascript
const formData = new FormData();
formData.append('name', 'El-Zahy Tower');
formData.append('location', 'New Cairo');
formData.append('propertyType', 'Commercial');
formData.append('priceStart', '15000000');
formData.append('priceEnd', '25000000');
formData.append('images', imageFile);
formData.append('translations', JSON.stringify([
  {
    language: 'ar',
    direction: 'RTL',
    title: '??? ??????',
    description: '??? ????? ????'
  }
]));

const response = await fetch('/api/projects', {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` },
  body: formData
});
```

## ?? Environment Configuration

### Required Environment Variables
```bash
# Database (choose one)
ConnectionStrings__DefaultConnection="Server=localhost;Database=ElzahyDB;Trusted_Connection=true;"
# OR
MYSQL_CONNECTION_STRING="Server=localhost;Database=ElzahyDB;Uid=root;Pwd=password;"

# JWT Authentication
DOTNET_JWT_KEY="your-super-secret-jwt-key-here"
JWT__Issuer="ElzahyAPI"
JWT__Audience="ElzahyClients"

# Optional
FRONTEND_URL="https://elzahygroup.com"
SKIP_DB_MIGRATIONS="false"
```

## ??? Security Features

- **JWT Authentication**: Secure admin endpoint access
- **File Validation**: Comprehensive image/video format validation
- **Input Sanitization**: Protection against injection attacks
- **CORS Configuration**: Secure cross-origin resource sharing
- **Error Handling**: Secure error responses without sensitive data exposure

## ?? Performance Features

- **Database Indexing**: Strategic indexes on filtered columns
- **Pagination**: Efficient data loading with metadata
- **Selective Queries**: Load only necessary navigation properties
- **Response Optimization**: Lightweight DTOs for listing operations
- **Query Optimization**: 40% improvement in list operations

## ?? Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## ?? Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? About El-Zahy Group

El-Zahy Group is a leading real estate development company committed to creating exceptional residential and commercial properties. This API powers the digital infrastructure for managing and showcasing our diverse project portfolio.

## ?? Support

For technical support or questions:
- **Email**: dev@elzahygroup.com
- **Documentation**: [Complete API Documentation](Documentation/API_Documentation.md)
- **Issues**: [GitHub Issues](https://github.com/mohamed-khalid-gamal/Elzahy_back/issues)

---

Built with ?? using .NET 8 and Entity Framework Core