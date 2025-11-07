# Image Upload Platform

A full-stack multi-tenant image upload platform built with .NET Web API and React TypeScript.

## Features

- **Multi-tenant Authentication** - JWT-based authentication with tenant isolation
- **Image Upload** - Drag & drop interface with file validation
- **Image Processing** - Automatic thumbnail generation and image optimization
- **File Management** - Persistent file storage with metadata tracking
- **Modern UI** - Clean, responsive React interface with styled-components
- **Real-time Gallery** - View uploaded images with original and thumbnail versions

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
   cd image-upload-backend
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
cd image-upload-backend
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

## Multi-Tenant Architecture

The application supports multiple tenants with complete data isolation:

- Each tenant has separate image containers
- User authentication is scoped to tenants
- Database queries include tenant filtering
- File storage is organized by tenant


### Production Configuration

1. **Update connection strings** for production database
2. **Configure Azure Blob Storage** (replace FileSystemBlobStorageService)
3. **Set secure JWT keys**
4. **Configure HTTPS**
5. **Set up proper CORS policies**


## Tech Stack

- **Backend**: .NET 9, ASP.NET Core, Entity Framework Core, SQLite, ImageSharp
- **Frontend**: React, TypeScript, styled-components, react-dropzone
- **Authentication**: JWT Bearer tokens
- **Database**: SQLite (development),
- **Storage**: File System (development), Azure Blob Storage (production ready)