# AUCA Pulse - .NET Implementation

> **Feel the Pulse of AUCA** — A smarter, connected, and dynamic campus experience.

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-blue.svg)](https://www.postgresql.org/)

## Overview

**AUCA Pulse** is a comprehensive campus digital management platform designed to revolutionize campus operations at the Adventist University of Central Africa (AUCA). This is the **.NET C# implementation** using ASP.NET Core Razor Pages, migrated from the original Spring Boot Java application.

The system provides real-time insights into lecturer availability, room utilization, personnel status, and campus-wide notifications, fostering a more efficient, transparent, and connected academic environment.

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
| **Framework** | ASP.NET Core 8.0 (Razor Pages) |
| **Database** | PostgreSQL 12+ |
| **ORM** | Entity Framework Core 8.0 |
| **Authentication** | JWT Bearer Tokens |
| **Password Hashing** | BCrypt.Net |
| **Email** | MailKit |
| **Build Tool** | .NET CLI / Visual Studio |

## Project Structure

```
AUCAPulse/
├── Models/                  # Entity models (User, Role, Room, etc.)
├── Data/                    # DbContext and migrations
├── Services/                # Business logic layer
│   ├── AuthService.cs
│   ├── EmailService.cs
│   ├── AdminService.cs
│   ├── LecturerService.cs
│   └── ...
├── Pages/                   # Razor Pages (UI)
│   ├── Auth/               # Login, Register, OTP
│   ├── Admin/              # Admin dashboard
│   ├── Lecturer/           # Lecturer dashboard
│   ├── Student/            # Student dashboard
│   └── ...
├── DTOs/                    # Data Transfer Objects
├── Helpers/                 # Utility classes
├── wwwroot/                 # Static files (CSS, JS, images)
├── appsettings.json         # Configuration
└── Program.cs               # Application entry point
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
   cd AUCA-Pulse
   git checkout "Habiyaremye Adolphe_26751"
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
       "ExpirationMinutes": 1440
     },
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@gmail.com",
       "SenderPassword": "your-app-password",
       "SenderName": "AUCA Pulse"
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

The application will start at `https://localhost:5001` or `http://localhost:5000`

### Initial Data

The application automatically seeds essential data on startup:

| Entity | Sample Data |
|--------|-------------|
| **Roles** | STUDENT, LECTURER, STAFF, ADMIN |
| **Admin Account** | Email: `admin@auca.ac.rw` <br/> Password: `Admin123!` |
| **Locations** | Rwanda provinces and districts |
| **Rooms** | Sample lecture halls and labs |
| **Offices** | Sample administrative offices |

## Development Phases

### ✅ Phase 1: Project Setup & Core Infrastructure (COMPLETED)
- ASP.NET Core Razor Pages project created
- NuGet packages installed (EF Core, PostgreSQL, JWT, BCrypt, MailKit)
- Git repository initialized
- .gitignore configured
- README created

### 🚧 Phase 2: Database Models & Context (IN PROGRESS)
- Entity models (User, Role, Location, Room, Office, etc.)
- DbContext configuration
- Relationships and constraints
- Initial migration

### 📋 Phase 3: Authentication & Authorization (PLANNED)
- JWT service implementation
- Authentication middleware
- OTP system
- Password hashing

### 📋 Phase 4: Core Services (PLANNED)
- Email service
- Admin service
- Lecturer service
- Room service
- Notification service

### 📋 Phase 5: Razor Pages (UI) (PLANNED)
- Authentication pages
- Dashboards
- Management pages

### 📋 Phase 6: Testing & Deployment (PLANNED)
- Unit tests
- Integration tests
- Deployment configuration

## Team Members

This project is developed by **Group B**:

- **Habiyaremye Adolphe** (26751) - Branch: `Habiyaremye Adolphe_26751`
- [Add other team members here]

## Contributing

Each team member works on their own branch and creates pull requests for code review before merging to main.

### Branch Naming Convention
```
FirstName LastName_StudentID
```

### Commit Message Convention
```
[Phase X] Brief description of changes
```

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
