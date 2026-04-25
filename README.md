# AUCA Pulse — Smart Campus Platform

> **Feel the pulse of AUCA.** A real-time campus coordination platform where lecturers, staff, and students all see the same live state of campus.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages%20%2B%20REST-2563EB)](https://docs.microsoft.com/en-us/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15%2B-336791)](https://www.postgresql.org/)
[![Status](https://img.shields.io/badge/status-fully%20functional-10B981)](#highlights)

---

## Overview

**AUCA Pulse** is a campus operations platform built for the Adventist University of Central Africa. One web app coordinates four distinct journeys — **student**, **lecturer**, **staff**, **admin** — around three live signals: where lecturers are, which rooms are free, and what just changed.

It replaces the WhatsApp-and-walking-to-offices status quo with a single, role-aware experience powered by a Round Robin timetable generator, instant class cancellation with auto room release, and an actionable notification system.

---

## Highlights

- **Round Robin timetable generator** — fairly distributes course slots and rooms across Mon–Fri + Sun (Saturday excluded for Sabbath).
- **One-click cancel today's class** — frees the room instantly, alerts admins, leaves next week's occurrence on the timetable.
- **Real-time room occupancy** — driven by the schedule itself, not by manual booking.
- **Click-to-navigate notification bell** — pulses on new items, deep-links to the right page.
- **Role-based experience** — same codebase, four tailored sidebars, route guard enforcing access.
- **Friendly exception layer** — translates raw PostgreSQL errors into plain English for users.
- **Modern, animated UI** — Poppins typography, blue accent theme, staggered card animations, frosted-glass top bar.

---

## Roles & journeys

| Role | What they get |
|---|---|
| **STUDENT** | Browse lecturers with live status, view available rooms, book appointments with staff, see today's schedule on the home dashboard. |
| **LECTURER** | Personal dashboard, weekly schedule with **Cancel today / Reinstate** action, live status broadcast, appointment inbox, room reservations. |
| **STAFF** | Office hours management, status broadcast (In Meeting, Available, Away, Unavailable), appointment approvals, room directory. |
| **ADMIN** | User verification queue, course / group / assignment management, **Round Robin timetable generator**, **Cancelled Classes** dashboard, office assignment to staff, reports, notifications hub. |

---

## Tech stack

| Layer | Technology |
|---|---|
| Language | C# 12 (.NET 8.0, nullable reference types) |
| Framework | ASP.NET Core 8.0 — Razor Pages + Web API |
| Data | PostgreSQL 15+ via **Entity Framework Core 8.0.4** (Npgsql) |
| Auth | JWT bearer (API) + session-based role (Razor Pages) |
| Frontend | Razor Pages + Bootstrap 5 + Bootstrap Icons + Poppins (Google Fonts) + vanilla JS + Chart.js |
| Email | MailKit 4 (Gmail SMTP for OTP & password reset) |
| Hashing | BCrypt.Net-Next 4 |
| Background work | `IHostedService` for the room auto-release loop |

---

## Architecture at a glance

```
┌────────────────────────────────────────────────────────────────────┐
│ Razor Pages (Pages/) · Bootstrap 5 · Poppins · Chart.js            │  Presentation
│  Admin / Lecturer / Staff layout    Student layout                 │
├────────────────────────────────────────────────────────────────────┤
│ Controllers (Controllers/)  ←→  Business Services (Services/)      │  API + Logic
│   /api/...                       Round Robin, cancellation flows   │
│  RoleGuardMiddleware  ·  FriendlyExceptionMiddleware               │
├────────────────────────────────────────────────────────────────────┤
│ ApplicationDbContext (EF Core)   ·   Migrations                    │  Data
│ PostgreSQL — auca_pulse_db                                         │
├────────────────────────────────────────────────────────────────────┤
│ Cross-cutting: JWT  ·  EmailService  ·  NotificationService        │
│  RoomAutoReleaseService (BackgroundService)                        │
└────────────────────────────────────────────────────────────────────┘
```

---

## Standout features (in detail)

### Round Robin room assignment
[`Services/RoundRobinRoomService.cs`](Services/RoundRobinRoomService.cs)

Singleton service maintaining a circular pointer over `Status == AVAILABLE` rooms. Every ad-hoc reservation increments the pointer modulo the available count, so requests rotate evenly instead of always picking room #1. The increment is wrapped in `lock(_pointerLock)` for thread safety; a fresh scoped `DbContext` is resolved through `IServiceScopeFactory` per call so the singleton never holds a request-scoped dependency.

### Same-day class cancellation with instant room release
[`Services/LectureScheduleService.CancelForTodayAsync`](Services/LectureScheduleService.cs)

A lecturer hits **Suspend / End now** on `/Schedule`. The service stamps `LectureSchedule.CancelledOn = today (UTC)` and, if that lecturer was holding the room, frees it (`Room.Status = AVAILABLE`, `CurrentLecturerId = null`). All admins receive a notification — *"Class cancelled today: CS201"* — with a deep link to the lecturer's profile. Reinstatement (`ReinstateForTodayAsync`) clears `CancelledOn`. The recurring weekly pattern survives untouched.

### Real-time room occupancy
[`Services/RoomService.GetRoomNumbersWithActiveClassAsync`](Services/RoomService.cs)

A room is shown as **OCCUPIED** if a `LectureSchedule` for today's day-of-week intersects the current time, falls inside the active semester window, and **isn't cancelled today**. The room detail page also shows an **In Session Now** card with the course code, lecturer, group, and time when an active class is running.

### Notification bell with deep linking
[`Pages/Shared/_Layout.cshtml`](Pages/Shared/_Layout.cshtml) + [`Services/NotificationService.cs`](Services/NotificationService.cs)

Polls `/api/notification/user/{id}/unread-count` every 30s, badge pulses when new. Clicking the bell opens a Bootstrap modal listing notifications with relative timestamps. Each notification can carry a `Link`; clicking the row marks it read and navigates to that page.

### Role-aware route guard
[`Middleware/RoleGuardMiddleware.cs`](Middleware/RoleGuardMiddleware.cs)

Runs after `UseSession()` and before Razor Pages. A rules table maps path prefixes to allowed roles; `/api/*` is delegated to `[Authorize]` attributes; signed-out users hit `/Login`; unauthorized roles get `/AccessDenied`.

### Friendly exception middleware
[`Middleware/FriendlyExceptionMiddleware.cs`](Middleware/FriendlyExceptionMiddleware.cs)

Catches unhandled exceptions, logs the full stack server-side, and returns plain-English text to the user. PostgreSQL uniqueness violations become *"This record already exists"*; FK violations become *"Still in use"*; everything else becomes *"Something went wrong on our end"*. JSON for API callers, redirect to `/Error` for page requests.

### Background room auto-release
[`Services/RoomAutoReleaseService.cs`](Services/RoomAutoReleaseService.cs)

`IHostedService` ticking every minute that releases rooms whose `OccupiedUntil` has passed — independent of HTTP traffic, so a forgotten reservation never blocks the room overnight.

---

## Domain model

16 EF Core entities under [`Models/`](Models/). Grouped by concern:

| Group | Entities |
|---|---|
| **People** | `User`, `Role`, `Location` (Province → District → Sector → Cell → Village), `VerificationRequest`, `PasswordResetRequest` |
| **Academic** | `Course`, `CourseAssignment`, `Group`, `Semester`, `LectureSchedule` |
| **Spaces** | `Room`, `Office` |
| **Interactions** | `Appointment`, `LecturerStatus`, `Notification`, `ChatMessage` |

Key relationships: `User` → `Role`; `LectureSchedule` → `User` (lecturer) + `Semester`; `Room` → `User` (current lecturer); `Appointment` → `User` (student) + `User` (staff); `Notification` → `User`.

---

## API surface

All routes are under `/api/[controller]`:

| Controller | Notable endpoints |
|---|---|
| `AuthController` | `POST /register`, `POST /login`, `POST /verify-otp`, `POST /forgot-password`, `POST /reset-password` |
| `UserController` | CRUD users, approvals |
| `RoomController` | CRUD, `GET /available`, `POST /{id}/occupy`, `POST /{id}/release`, `POST /auto-assign` |
| `LectureScheduleController` | CRUD, `GET /lecturer/{id}`, `GET /room/{number}`, `POST /{id}/cancel-today`, `POST /{id}/reinstate-today`, `GET /cancelled?date=` |
| `LecturerStatusController` | Update / read live status |
| `LecturerLocationController` | Aggregate live lecturer state |
| `OfficeController` | CRUD offices, availability updates |
| `AppointmentController` | CRUD, approve / reject / complete |
| `NotificationController` | `GET /user/{id}`, `GET /user/{id}/unread-count`, `PUT /{id}/mark-read`, `PUT /user/{id}/mark-all-read` |
| `CourseController` · `GroupController` · `CourseAssignmentController` · `SemesterController` | Catalog management |
| `LocationController` | Hierarchical locations |
| `TimetableController` | Round Robin timetable generation |
| `VerificationRequestController` · `PasswordResetRequestController` | Approval workflows |
| `ChatController` | Direct messaging |

---

## Project structure

```
AUCA-Pulse-Habiyaremye/
├── Controllers/           REST controllers (Auth, User, Room, ...
│                          ...LectureSchedule, Appointment, Notification, etc.)
├── Models/                EF Core entities (16)
├── DTOs/Request,Response/ Wire shapes
├── Services/              Business logic (services + interfaces)
│   ├── RoundRobinRoomService.cs       Singleton, thread-safe pointer
│   ├── LectureScheduleService.cs      Cancel/reinstate today, notify admins
│   ├── RoomService.cs                 Active class detection
│   ├── RoomAutoReleaseService.cs      BackgroundService, 1-min tick
│   ├── TimetableGeneratorService.cs   Round Robin slot+room assignment
│   ├── NotificationService.cs         Bell, polling, deep links
│   └── ...
├── Middleware/
│   ├── RoleGuardMiddleware.cs         Page-level role enforcement
│   └── FriendlyExceptionMiddleware.cs Plain-English errors
├── Data/
│   └── ApplicationDbContext.cs        EF Core configuration + relationships
├── Migrations/                         EF Core migrations
├── Pages/                              Razor Pages
│   ├── Dashboard/                      Admin & Lecturer & Staff dashboard
│   ├── StaffDashboard.cshtml           (alternate entry kept for legacy)
│   ├── Home.cshtml                     Student home (with hero + tiles)
│   ├── Schedule.cshtml                 Weekly schedule + Cancel today modal
│   ├── Rooms.cshtml + RoomDetails/     Browse + In-Session-Now card
│   ├── Lecturers.cshtml + LecturerDetail.cshtml + LecturerProfile/
│   ├── Appointments.cshtml             Lecturer/staff inbox
│   ├── Student/MyAppointments.cshtml   Student outbox
│   ├── UpdateStatus.cshtml             Lecturer & staff status broadcast
│   ├── MyOffice.cshtml                 Staff office management
│   ├── OfficeManagement.cshtml         Admin: assign offices to staff
│   ├── CourseAssignment, Courses, Groups, Semesters
│   ├── TimetableGenerator.cshtml       Admin: Round Robin generator
│   ├── CancelledClasses.cshtml         Admin: today's cancellations
│   ├── VerificationRequests, UserManagement, Reports
│   ├── Notifications.cshtml            Full list view
│   └── Shared/_Layout.cshtml + _StudentLayout.cshtml
├── wwwroot/
│   ├── css/site.css                    Design tokens, animations,
│   │                                   stagger, shine, modal slide-up
│   └── js/                             Toast, table-paginate, site.js
├── scripts/
│   └── build_presentation.py           Class-presentation generator
├── Program.cs                          DI, middleware order, JWT config
├── appsettings.json                    Connection string, JWT, SMTP
└── AUCAPulse.csproj
```

---

## Getting started

### Prerequisites
- **.NET 8.0 SDK**
- **PostgreSQL 15+** running locally
- A Gmail account with an app password (for OTP / password-reset emails) — or any SMTP server
- (Optional) **Visual Studio 2022** or **VS Code** with the C# extension

### 1. Clone

```bash
git clone https://github.com/Samillah47/AUCA-Pulse.git
cd AUCA-Pulse
git checkout final/team-merge
```

### 2. Configure `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=auca_pulse_db;Username=postgres;Password=YOUR_PASSWORD"
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
    "SenderPassword": "your-gmail-app-password",
    "SenderName": "AUCA Pulse"
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:5204/api"
  }
}
```

> **Tip:** keep secrets out of git. `appsettings.Development.json` is the recommended override file for local dev.

### 3. Restore + migrate + run

```bash
dotnet restore
dotnet tool install -g Microsoft.Web.LibraryManager.Cli   # one-time, for libman
libman restore                                            # bootstrap, jquery
dotnet ef database update
dotnet run
```

The app starts at **http://localhost:5204**.

### 4. First login

Migrations seed an admin account on a fresh database. Use the credentials configured in [`Helpers/AdminUserInitializer.cs`](Helpers/AdminUserInitializer.cs) (or whichever admin you set up). From there: create roles → semesters → courses → groups → assignments → generate timetable.

---

## How a typical day flows

1. **Admin** creates the semester, courses, groups, and assigns lecturers to course-groups.
2. **Admin** clicks **Generate Timetable** — Round Robin distributes slots Mon–Fri + Sun, picks rooms in rotation.
3. **Students** open `/Home` and see today's classes with rooms; the **Lecturers** page shows live status.
4. **Lecturer** opens `/Schedule`; their classes appear under "Today". They can press **Suspend / End now** to cancel today only.
5. The **Room** the lecturer was holding flips back to AVAILABLE; students see it in `/Rooms` immediately.
6. **Admins** see a notification with a link to that lecturer; the **Cancelled Classes** dashboard lists every cancellation for the day.
7. **Staff** approve student appointments from `/StaffDashboard` (or `/Appointments`); both parties get notifications.

---

## Security model

- Passwords hashed with **BCrypt** (no plaintext, ever).
- **JWT bearer** secures the REST API (`/api/*`).
- **Session-based role** powers Razor Pages — required by the `RoleGuardMiddleware` rules table.
- **OTP email verification** at signup — accounts stay PENDING until OTP is confirmed.
- **Admin approval** is then required before lecturers and staff can sign in.
- Errors don't leak: end users see human messages, full stack traces stay in server logs.

---

## Class presentation

A pre-built PowerPoint walkthrough lives at the project root:

- [`AUCA-Pulse-Presentation.pptx`](AUCA-Pulse-Presentation.pptx) — 20 slides covering problem, solution, tech stack, UML class / use-case / sequence diagrams, Round Robin pseudo-code, cancellation flow, role views, security, demo script, and Q&A.
- Regenerate any time:
  ```bash
  python scripts/build_presentation.py
  ```

---

## Team — Group B

Adventist University of Central Africa · Bachelor of Information Technology · 2026

- **Habiyaremye Adolphe** — backend lead (.NET, EF Core, Round Robin, route guard, cancellation flow)
- **Kwizera** — staff & office module, integrations
- **Samillah Mutoni** — student portal, dashboards
- **Joseph Manizabayo** — lecturer portal, password-reset flow

The original Java Spring Boot version of AUCA Pulse was authored by **Joseph Manizabayo** ([acpulse-backend](https://github.com/josephmanizabayo/acpulse-backend)). This repository is the .NET migration plus a substantial set of new features (Round Robin generator, same-day cancellation, cancelled-classes dashboard, notification deep-linking, modernized UI).

---

## Contributing

We work on per-feature branches. The current integration branch is `final/team-merge`.

```bash
git checkout -b feature/your-thing
# ... edits, commits ...
git push -u origin feature/your-thing
# open a PR against final/team-merge
```

Commits should describe **why**, not just **what**. Builds and tests must pass before merging.

---

## License

For academic use under the AUCA WebTech course. Reuse outside coursework requires attribution to the original author (Joseph Manizabayo) and the .NET migration team.

---

<div align="center">

**© 2026 AUCA Pulse · Group B**

*Built with .NET, PostgreSQL, Bootstrap 5, and a lot of merge conflicts.*

</div>
