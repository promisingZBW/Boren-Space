# 🎧 English Learning Web Platform (ELWeb)

A podcast-style English personal podcast platform built with a .NET 8 microservices architecture, following Domain-Driven Design (DDD) principles.

## 📋 Project Overview

This project is a full-stack personal podcast web application. 
The system adopts a front-end/back-end separated architecture, with a .NET 8 DDD-based backend and a React 18+ frontend.

### Key Features

- 🎙️ **Podcast-style Learning**：Audio playback with synchronized subtitles
- 👤 **User Authentication**：JWT-based authentication, role management, password hashing
- 📁 **File Management**：Supports AWS S3 cloud storage and local storage
- 📝 **Subtitle Support**：SRT subtitle parsing and timeline synchronization
- 🎨 **Modern UI**：Responsive design with Ant Design component library
- 🔐 **Security**：Token blacklist mechanism, soft delete, global exception handling

---

## 🏗️ Technical Architecture

### Backend Tech Stack

- **.NET 8** - Cross-platform
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core** - ORM framework
- **SQL Server** - Relational database
- **JWT** - Authentication & authorization
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **MediatR (Reserved)** - Event-driven architecture

### Frontend Tech Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Ant Design 5** - UI component library
- **Zustand** - State management
- **Vite** - Build tool
- **React Router 7** - Routing
- **Axios** - HTTP client

### Infrastructure

- **AWS S3** - Cloud storage
- **SQL Server** - Database
- **Redis (Reserved)** - Cache / session storage

---

## 📁 Project Structure
```
ELWeb-net8/
├─ 🌐 listening-frontend/ # React frontend application
│ ├─ src/
│ │ ├─ pages/ # Page components
│ │ ├─ components/ # Reusable components
│ │ ├─ services/ # API services
│ │ ├─ store/ # Zustand state management
│ │ └─ utils/ # Utility functions
│ └─ package.json
│
├─ 🏛️ IdentityService/ # Identity & authentication service
│ ├─ IdentityService.Domain/ # Domain layer (User, Role)
│ ├─ IdentityService.Infrastructure/ # Infrastructure layer (EF Core)
│ └─ IdentityService.WebAPI/ # API layer
│
├─ 📁 FileService/ # File management service
│ ├─ FileService.Domain/ # Domain layer (File entities)
│ ├─ FileService.Infrastructure/ # Infrastructure layer (S3 / Local storage)
│ └─ FileService.WebAPI/ # API layer
│
├─ 🎧 Listening/ # Listening & learning service
│ ├─ Listening.Domain/ # Domain layer (Episode, Sentence)
│ ├─ Listening.Infrastructure/ # Infrastructure layer (Repositories)
│ ├─ Listening.Admin.WebAPI/ # Admin API (Upload / Management)
│ └─ Listening.Main.WebAPI/ # User API (Browse / Learn)
│
├─ 🔧 Commons/ # Shared modules
│ ├─ Zbw.Commons/ # Common utilities
│ ├─ Zbw.DomainCommons/ # DDD domain base classes
│ ├─ Zbw.Infrastructure/ # EF Core infrastructure
│ ├─ Zbw.ASPNETCore/ # ASP.NET Core extensions
│ ├─ Zbw.JWT/ # JWT authentication
│ ├─ Zbw.EventBus/ # Event bus (reserved)
│ └─ CommonInitializer/ # Unified initialization
│
└─ ELWeb-net8.sln # Visual Studio solution file
```


---

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Node.js 18+** ([Download](https://nodejs.org/))
- **SQL Server** 
- **Visual Studio 2022** or **VS Code**
- **AWS Account** (Optional, for S3 storage)

### 1. Clone the Repository

git clone <your-repo-url>
cd ELWeb-net8

### 2. Backend Configuration

#### 2.1 Create Database Configuration Files

Create appsettings.Development.json for each service (based on appsettings.Example.json):

##### IdentityService
cp IdentityService.WebAPI/appsettings.Example.json IdentityService.WebAPI/appsettings.Development.json

##### FileService
cp FileService.WebAPI/appsettings.Example.json FileService.WebAPI/appsettings.Development.json

##### Listening.Admin
cp Listening.Admin.WebAPI/appsettings.Example.json Listening.Admin.WebAPI/appsettings.Development.json

##### Listening.Main
cp Listening.Main.WebAPI/appsettings.Example.json Listening.Main.WebAPI/appsettings.Development.json

#### 2.2 Modify Configuration Files

Edit each appsettings.Development.json and configure:
- Database connection strings
- JWT SecretKey
- AWS S3 settings (optional)
- Allowed CORS frontend origins

**Example configuration:**
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ELWeb_Identity;Trusted_Connection=True;"
  },
  "JWT": {
    "SecretKey": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "ELWeb",
    "Audience": "ELWeb",
    "ExpireMinutes": 1440
  },
  "AWS": {
    "Profile": "default",
    "Region": "ap-southeast-2",
    "S3": {
      "BucketName": "your-bucket-name"
    }
  }
}
```

#### 2.3 Run Database Migrations

```
# # Navigate to each WebAPI project and run migrations
dotnet ef database update --project IdentityService.Infrastructure --startup-project IdentityService.WebAPI
dotnet ef database update --project FileService.Infrastructure --startup-project FileService.WebAPI
dotnet ef database update --project Listening.Infrastructure --startup-project Listening.Main.WebAPI
```

#### 2.4 Start Backend Services

**Option 1: Using PowerShell script (Recommended)**
```
# One-click startup for all backend & frontend services
.\start-dev.ps1
```
**Option 2: Using Visual Studio

Open ELWeb-net8.sln in Visual Studio, configure multiple startup projects, and start all four WebAPI projects.

**Option 3: Manual Startup**

In four separate terminal windows, run:

```
dotnet run --project IdentityService.WebAPI
dotnet run --project FileService.WebAPI
dotnet run --project Listening.Admin.WebAPI
dotnet run --project Listening.Main.WebAPI
```

### 3. Frontend Configuration

#### 3.1 Install Dependencies

cd listening-frontend
npm install

#### 3.2 Configure API Base URL

Edit listening-frontend/src/services/api.ts and ensure baseURL points to the correct backend address.

#### 3.3 Start Frontend Dev Server
cd ".\ELWeb-net8\listening-frontend"
npm run dev


## 📸 Project Showcase
#### Home Page
<img width="2557" height="1347" alt="image" src="https://github.com/user-attachments/assets/5b41a5cd-8466-4c6d-b7ab-7a36de1dd456" />

<img width="2556" height="1344" alt="image" src="https://github.com/user-attachments/assets/667699e3-598d-40fc-a98c-c775cdb1edf7" />
### Player Interface
<img width="2559" height="1341" alt="image" src="https://github.com/user-attachments/assets/0c20d699-9dab-45be-839c-8960d05aa86d" />
### Audio Management Page
<img width="2554" height="1338" alt="image" src="https://github.com/user-attachments/assets/0b47682f-b8f7-406c-a3a3-9f41dbdfd967" />

<img width="2559" height="1339" alt="image" src="https://github.com/user-attachments/assets/c325d0df-2f30-43d4-94f9-cb7e1ef83361" />

