# AUCA Pulse - .NET Implementation

> **Feel the Pulse of AUCA** — A smarter, connected, and dynamic campus experience.

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-blue.svg)](https://www.postgresql.org/)

## Overview

**AUCA Pulse** is a comprehensive campus digital management platform designed to revolutionize campus operations at the Adventist University of Central Africa (AUCA). This is the **.NET C# implementation** using ASP.NET Core with REST API backend and Razor Pages frontend, migrated from the original Spring Boot Java application.

The system provides real-time insights into lecturer availability, room utilization, personnel status, and campus-wide notifications, fostering a more efficient, transparent, and connected academic environment.

## Current Status

✅ **FULLY FUNCTIONAL** - Complete backend API and frontend UI with authentication, role-based access, and all core features implemented.

## Key Features

### 🔐 Authentication & Authorization
- Multi-role user system supporting **STUDENT**, **LECTURER**, **STAFF**, and **ADMIN** roles
- Secure JWT-based authentication
- Two-Factor Authentication (2FA) with OTP via email
- Administrative verification workflow for new user approvals
- Password reset with admin approval workflow
- BCrypt password hashing

### 🏫 Room & Lecturer Management
- Real-time room occupancy tracking and management
- Lecturer availability status monitoring
- Dynamic room booking with extension and release capabilities
- Lecture schedule management
- Conflict resolution for room scheduling

### 📍 Hierarchical Location System
- Five-tier geographic hierarchy: Province → District → Sector → Cell → Village
- Recursive relationship modeling for flexible location queries
- Campus-wide location mapping and search functionality

### 📬 Notification System
- Automated notifications for status changes and approvals
- Read/unread tracking with persistence
- Email notifications for important events

### 🛠️ Administrative Tools
- Comprehensive verification request management
- Office status and assignment controls
- User management and role assignment interfaces
- Password reset request approval system
- System-wide configuration and monitoring

## Technology Stack

| Layer | Technology |
|-------|-----------|
| **Language** | C# 12 |
| **Framework** | ASP.NET Core 8.0 (REST API + Razor Pages) |
| **Database** | PostgreSQL 12+ |
| **ORM** | Entity Framework Core 8.0 |
| **Authentication** | JWT Bearer Tokens |
| **Password Hashing** | BCrypt.Net |
| **Email** | MailKit |
| **Build Tool** | .NET CLI / Visual Studio |

## Project Structure

```
FinalProject_GroupB/
├── Controllers/             # REST API Controllers (12 endpoints)
│   ├── AuthController.cs
│   ├── UserController.cs
│   ├── RoomController.cs
│   ├── LecturerStatusController.cs
│   ├── LectureScheduleController.cs
│   ├── OfficeController.cs
│   ├── LocationController.cs
│   ├── NotificationController.cs
│   ├── VerificationRequestController.cs
│   ├── PasswordResetRequestController.cs
│   ├── SemesterController.cs
│   └── TestController.cs
├── Models/                  # Entity models (11 entities)
│   ├── User.cs
│   ├── Role.cs
│   ├── Room.cs
│   ├── LecturerStatus.cs
│   ├── LectureSchedule.cs
│   ├── Office.cs
│   ├── Location.cs
│   ├── Notification.cs
│   ├── VerificationRequest.cs
│   ├── PasswordResetRequest.cs
│   └── Semester.cs
├── Data/                    # DbContext and database configuration
│   └── ApplicationDbContext.cs
├── Services/                # Business logic layer (22 services)
│   ├── AuthService.cs
│   ├── EmailService.cs
│   ├── UserService.cs
│   ├── RoomService.cs
│   ├── LecturerStatusService.cs
│   ├── LectureScheduleService.cs
│   ├── OfficeService.cs
│   ├── LocationService.cs
│   ├── NotificationService.cs
│   ├── VerificationRequestService.cs
│   ├── PasswordResetRequestService.cs
│   ├── SemesterService.cs
│   └── I[Service]Service.cs (interfaces)
├── Pages/                   # Razor Pages UI (Frontend)
│   ├── Dashboard/          # Role-based dashboard
│   ├── Login.cshtml        # Authentication pages
│   ├── VerifyOtp.cshtml
│   ├── Signup.cshtml
│   ├── Logout.cshtml
│   ├── Rooms.cshtml        # Room management
│   ├── RoomDetails/
│   ├── Lecturers.cshtml    # Lecturer directory
│   ├── LecturerProfile/
│   ├── Schedule.cshtml     # Lecture schedules
│   ├── Profile.cshtml      # User profile
│   ├── Notifications.cshtml
│   ├── UserManagement.cshtml      # Admin pages
│   ├── VerificationRequests.cshtml
│   ├── PasswordResets.cshtml
│   └── Shared/             # Layout and partials
├── DTOs/                    # Data Transfer Objects
│   ├── Request/            # API request DTOs
│   └── Response/           # API response DTOs
├── Helpers/                 # Utility classes
│   ├── JwtHelper.cs
│   ├── OtpHelper.cs
│   └── AdminUserInitializer.cs
├── Migrations/              # EF Core migrations
│   └── 20260404145133_InitialCreate.cs
├── wwwroot/                 # Static files (CSS, JS, Bootstrap)
├── appsettings.json         # Configuration
├── Program.cs               # Application entry point
├── AUCAPulse.csproj        # Project file
└── FinalProject_GroupB.sln # Solution file
```

## Getting Started

### Prerequisites

- **.NET 8.0 SDK** or higher
- **PostgreSQL** 12+ installed and running
- **Visual Studio 2022** or **VS Code** (optional)
- **Git** for version control

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Samillah47/AUCA-Pulse.git
   cd "AUCA-Pulse/FinalProject_GroupB"
   git checkout Habiyaremye_Adolphe_26751
   ```

2. **Configure database connection**
   
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=auca_pulse_db;Username=postgres;Password=your_password"
     },
     "JwtSettings": {
       "SecretKey": "your-secret-key-minimum-32-characters-long",
       "Issuer": "AUCAPulse",
       "Audience": "AUCAPulseUsers",
       "ExpirationMinutes": 10080
     },
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@gmail.com",
       "SenderPassword": "your-app-password",
       "SenderName": "AUCA Pulse"
     },
     "ApiSettings": {
       "BaseUrl": "http://localhost:5204/api"
     }
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```
   Or open `FinalProject_GroupB.sln` in Visual Studio and press F5.

The application will start at `http://localhost:5204`

### Initial Data

The application automatically seeds essential data on first migration:

| Entity | Sample Data |
|--------|-------------|
| **Roles** | STUDENT, LECTURER, STAFF, ADMIN |
| **Admin Account** | Email: `habiyaadolphe19@gmail.com` <br/> Password: `Mugisha1234!@` |
| **Locations** | Rwanda provinces, districts, sectors, cells, villages |
| **Rooms** | 10 lecture halls and labs |
| **Offices** | 5 administrative offices |
| **Users** | 15 sample users (students, lecturers, staff) |
| **Schedules** | Sample lecture schedules |
| **Semesters** | Current and upcoming semesters |

## Development Phases

### ✅ Phase 1: Project Setup & Core Infrastructure (COMPLETED)
- ASP.NET Core project created with REST API + Razor Pages
- NuGet packages installed (EF Core, PostgreSQL, JWT, BCrypt, MailKit)
- Git repository initialized and connected to GitHub
- .gitignore configured

### ✅ Phase 2: Database Models & Context (COMPLETED)
- 11 entity models created (User, Role, Location, Room, Office, etc.)
- ApplicationDbContext configured with relationships
- Database constraints and indexes
- Initial migration created and applied
- Seed data for all entities

### ✅ Phase 3: Authentication & Authorization (COMPLETED)
- JWT authentication with Bearer tokens
- OTP-based two-factor authentication via email
- BCrypt password hashing
- Role-based authorization (ADMIN, LECTURER, STAFF, STUDENT)
- Session management with 7-day persistent cookies

### ✅ Phase 4: Backend API Development (COMPLETED)
- 12 REST API controllers with full CRUD operations
- 22 service classes with business logic
- Email service with MailKit
- Notification system
- Verification request workflow
- Password reset workflow
- Room booking and management
- Lecturer status and schedule management

### ✅ Phase 5: Razor Pages Frontend (COMPLETED)
- Authentication pages (Login, Signup, OTP Verification, Logout)
- Role-based dashboard with real-time statistics
- Room management and details pages
- Lecturer directory and profile pages
- Lecture schedule viewer
- User profile management
- Notifications page
- Admin pages (User Management, Verification Requests, Password Resets)
- Responsive UI with Bootstrap 5
- API integration with HttpClient

### ✅ Phase 6: Bug Fixes & Optimization (COMPLETED)
- Fixed authentication flow and session persistence
- Corrected API response parsing across all pages
- Fixed admin endpoint authorization
- Added public lecturer endpoint for non-admin users
- Removed mock data and integrated real API calls
- Configured HTTP (disabled HTTPS for development)
- Extended session timeout to 7 days
- Project structure cleanup

### 📋 Phase 7: Testing & Deployment (FUTURE)
- Unit tests for services
- Integration tests for API endpoints
- UI testing
- Production deployment configuration
- HTTPS configuration for production

## Team Members

This project is developed by **Group B**:

- **Habiyaremye Adolphe** (26751) - Lead Developer - Branch: `Habiyaremye_Adolphe_26751`

## Contributing

Each team member works on their own branch and creates pull requests for code review before merging to main.

### Branch Naming Convention
```
FirstName_LastName_StudentID
```

### Commit Message Convention
```
Phase X.Y - Feature: Brief description of changes
```

### Git Workflow
- Each phase is committed separately with descriptive messages
- All commits are pushed to the feature branch
- 16+ commits documenting the complete development journey

## Original Project

This is a .NET C# migration of the original Java Spring Boot project by **Joseph Manizabayo**.

**Original Repository:** [ACPulse Backend](https://github.com/josephmanizabayo/acpulse-backend)

## License

This project is licensed under the AUCA WebTech class License.

**Attribution Required:** Any reuse, modification, or extension of this codebase must credit the original author (Joseph Manizabayo) and the migration team.

---

<div align="center">

**© 2026 AUCA Group B — All Rights Reserved**

*Migrated to .NET by Habiyaremye Adolphe and Team*

</div>
