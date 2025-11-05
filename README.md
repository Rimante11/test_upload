# Image Upload Platform

A full-stack multi-tenant image upload platform built with .NET Web API and React TypeScript.

## Features

- 🔐 **Multi-tenant Authentication** - JWT-based authentication with tenant isolation
- 📤 **Image Upload** - Drag & drop interface with file validation
- 🖼️ **Image Processing** - Automatic thumbnail generation and image optimization
- 🗂️ **File Management** - Persistent file storage with metadata tracking
- 🎨 **Modern UI** - Clean, responsive React interface with styled-components
- 🔄 **Real-time Gallery** - View uploaded images with original and thumbnail versions

## Architecture

### Backend (.NET 9 Web API)
- **Framework**: ASP.NET Core 9.0
- **Database**: SQLite with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Image Processing**: ImageSharp library
- **Storage**: File system (development) / Azure Blob Storage (production ready)

### Frontend (React TypeScript)
- **Framework**: React 18 with TypeScript
- **Styling**: styled-components
- **File Upload**: react-dropzone
- **HTTP Client**: axios
- **Routing**: react-router-dom

## Project Structure

```
imageUpload/
├── ImageUploadApi/           # Backend (.NET Web API)
│   ├── Controllers/          # API endpoints
│   ├── Data/                 # Database context and migrations
│   ├── Models/              # Entity models and DTOs
│   ├── Services/            # Business logic services
│   └── uploads/             # File storage directory
├── image-upload-frontend/    # Frontend (React TypeScript)
│   ├── src/
│   │   ├── components/      # React components
│   │   ├── contexts/        # React contexts (Auth)
│   │   └── api/             # API client
│   └── public/              # Static assets
└── README.md
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/imageUpload.git
   cd imageUpload
   ```

2. **Setup Backend**
   ```bash
   cd ImageUploadApi
   dotnet restore
   dotnet build
   ```

3. **Setup Frontend**
   ```bash
   cd ../image-upload-frontend
   npm install
   ```

### Running the Application

You need to run both the backend and frontend simultaneously:

#### Terminal 1 - Backend API
```bash
cd ImageUploadApi
dotnet run
# API will be available at http://localhost:5119
```

#### Terminal 2 - React Frontend
```bash
cd image-upload-frontend
npm start
# Frontend will be available at http://localhost:3000
```

### Default Login Credentials

- **Tenant**: `acme`
- **Email**: `john.doe@acme.com`
- **Password**: `password123`

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `GET /api/auth/validate` - Token validation

### Images
- `POST /api/images/upload` - Upload image
- `GET /api/images` - List user images
- `GET /api/images/{id}` - Get image details
- `DELETE /api/images/{id}` - Delete image
- `GET /api/images/blob/{container}/{filename}` - Serve image files

### Health
- `GET /health` - API health check

## Configuration

### Backend Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ImageUpload.db"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "ImageUploadApi"
  },
  "FileStorage": {
    "BasePath": "uploads"
  }
}
```

### Environment Variables
- `JWT_KEY`: JWT signing key (override default)
- `JWT_ISSUER`: JWT issuer (override default)

## Multi-Tenant Architecture

The application supports multiple tenants with complete data isolation:

- Each tenant has separate image containers
- User authentication is scoped to tenants
- Database queries include tenant filtering
- File storage is organized by tenant

## Development

### Adding New Features

1. **Backend**: Add controllers in `Controllers/`, services in `Services/`, models in `Models/`
2. **Frontend**: Add components in `src/components/`, update API client in `src/api/`

### Database Migrations

```bash
cd ImageUploadApi
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Deployment

### Production Configuration

1. **Update connection strings** for production database
2. **Configure Azure Blob Storage** (replace FileSystemBlobStorageService)
3. **Set secure JWT keys**
4. **Configure HTTPS**
5. **Set up proper CORS policies**

### Docker Support (Coming Soon)

Docker configuration will be added for containerized deployment.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Tech Stack

- **Backend**: .NET 9, ASP.NET Core, Entity Framework Core, SQLite, ImageSharp
- **Frontend**: React, TypeScript, styled-components, react-dropzone
- **Authentication**: JWT Bearer tokens
- **Database**: SQLite (development), SQL Server (production ready)
- **Storage**: File System (development), Azure Blob Storage (production ready)