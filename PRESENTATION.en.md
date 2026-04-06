# SafetyPortal — Technical Presentation

---

## 1. Project Overview

SafetyPortal is an Incident Management System for industrial safety.  
It enables reporting incidents, assigning corrective actions, tracking statuses, and generating analytics.

**Three projects in one solution:**

```
DatwiseTest/
├── SafetyPortal.Api/        .NET 10 — REST API (Minimal APIs)
├── SafetyPortal.Web/        .NET Framework 4.8 — ASP.NET Web Forms (frontend)
└── SafetyPortal.Shared/     .NET Standard 2.0 — shared models and constants
```

> `SafetyPortal.Shared` targets `netstandard2.0` so it can be referenced by both .NET FW 4.8 and .NET 10 simultaneously.

---

## 2. Architecture

```
┌─────────────────────────────────────────────────────┐
│                    User's Browser                    │
└─────────────────────┬───────────────────────────────┘
                      │ HTTP
┌─────────────────────▼───────────────────────────────┐
│          SafetyPortal.Web (.NET FW 4.8)              │
│   ASP.NET Web Forms + Bootstrap 5 + Chart.js         │
│                                                       │
│  Infrastructure/   Services/      Handlers/           │
│  BasePage.cs       ApiBase.cs     ExportExcel.ashx    │
│  SessionHelper.cs  8 services     DownloadAttach.ashx │
└─────────────────────┬───────────────────────────────┘
                      │ HTTP + JWT Bearer
┌─────────────────────▼───────────────────────────────┐
│          SafetyPortal.Api (.NET 10)                  │
│   Minimal APIs + EF Core + Hangfire + SendGrid       │
│                                                       │
│  Endpoints/        Jobs/           Services/          │
│  10 endpoint files  3 background jobs  JWT, Email, Excel│
└─────────────────────┬───────────────────────────────┘
                      │ EF Core
┌─────────────────────▼───────────────────────────────┐
│              SQL Server Database                      │
│  8 tables: Users, Roles, IncidentReports,            │
│  CorrectiveActions, Departments, Categories,          │
│  IncidentAttachments, AuditLogs                      │
└─────────────────────────────────────────────────────┘
```

---

## 3. Technology Stack

| Layer | Technology | Why |
|-------|-----------|-----|
| API framework | .NET 10 Minimal APIs | Minimal boilerplate, high performance |
| ORM | Entity Framework Core | Code-first migrations, LINQ queries |
| Database | SQL Server | Reliability, transactions, ACID compliance |
| Authentication | JWT Bearer (HS256) | Stateless, works for both SPA and Web Forms |
| Background jobs | Hangfire + SQL Server | Persistent queues, retry, built-in dashboard |
| Email | SendGrid | Reliable delivery, templates |
| Frontend | ASP.NET Web Forms | Rapid UI development, server-side rendering |
| UI framework | Bootstrap 5 + Bootstrap Icons | Ready-made components, RTL support |
| Charts | Chart.js | Interactive dashboard charts |
| Excel export | ClosedXML / EPPlus | Generate .xlsx without Office installed |

---

## 4. File Structure — SafetyPortal.Api

```
SafetyPortal.Api/
├── Program.cs                          # Entry point: DI, middleware, migrations, seed
├── AppConstants.cs                     # Global constants (enum RoleName, IncidentStatus…)
│
├── Endpoints/                          # All API routes
│   ├── AuthEndpoints.cs                # POST /api/auth/login, GET /api/auth/me
│   ├── IncidentEndpoints.cs            # Incident CRUD + archiving + export
│   ├── CorrectiveActionEndpoints.cs    # Corrective action CRUD + export
│   ├── AttachmentEndpoints.cs          # File upload/download
│   ├── DashboardEndpoints.cs           # GET /api/dashboard/stats
│   ├── UserManagementEndpoints.cs      # User CRUD (Admin only)
│   ├── AdminCategoryEndpoints.cs       # Category management
│   ├── AdminDepartmentEndpoints.cs     # Department management
│   ├── LookupEndpoints.cs              # Read-only dropdowns
│   └── HangfireLoginEndpoints.cs       # Hangfire Dashboard auth
│
├── Entities/                           # EF Core entities (DB tables)
│   ├── User.cs                         # User: login, password hash, lockout
│   ├── Role.cs                         # Role (Admin, SafetyManager, Supervisor, Employee)
│   ├── IncidentReport.cs               # Incident: status, severity, archive flag
│   ├── CorrectiveAction.cs             # Corrective action: due date, priority
│   ├── IncidentCategory.cs             # Incident category
│   ├── Department.cs                   # Department with color and location
│   ├── IncidentAttachment.cs           # File: original name, GUID name, type
│   └── AuditLog.cs                     # Audit: event, IP, email, details
│
├── Jobs/                               # Hangfire background jobs
│   ├── IncidentNotificationJob.cs      # Notify on incident create/update
│   ├── CorrectiveActionReminderJob.cs  # Daily reminder 3 days before due date
│   └── IncidentEscalationJob.cs        # Escalate if incident open for 3+ days
│
├── Services/
│   ├── JwtTokenService.cs              # Create JWT tokens with claims
│   ├── IEmailService.cs                # Email interface + context records per scenario
│   ├── SendGridEmailService.cs         # SendGrid implementation
│   ├── FileSignatureValidator.cs       # Magic byte file validation
│   └── ExcelOrCsvCreator.cs            # Generate .xlsx files
│
├── Auth/
│   ├── JwtOptions.cs                   # JWT settings (Issuer, Key, ExpirationMinutes)
│   └── HangfireAdminAuthorizationFilter.cs  # Cookie auth for Hangfire
│
├── Data/
│   ├── SafetyPortalDbContext.cs        # DbContext: 8 DbSets + relationship config
│   ├── DbSeeder.cs                     # Seed data: roles, users, departments
│   └── SafetyPortalDbContextFactory.cs # Design-time factory for migrations
│
├── Dtos/                               # Request/response DTOs
│   ├── Auth/                           # LoginRequestDto, LoginResponseDto
│   ├── Incidents/                      # CreateIncidentDto, UpdateIncidentDto, IncidentFilterQuery
│   ├── CorrectiveActions/              # CreateCorrectiveActionDto, UpdateActionStatusDto
│   ├── Users/                          # CreateUserDto, UpdateUserDto, UserSummaryDto
│   ├── Categories/                     # CreateCategoryDto, UpdateCategoryDto
│   └── Departments/                    # CreateDepartmentDto, UpdateDepartmentDto
│
└── Migrations/                         # EF Core migrations (6 total)
```

---

## 5. File Structure — SafetyPortal.Web

```
SafetyPortal.Web/
├── Site.Master                         # Master page: sidebar, topbar, language switcher
├── Site.Master.cs                      # Code-behind: role, language, dir (rtl/ltr)
├── Login.aspx / Login.aspx.cs          # Login form
├── Dashboard.aspx / .cs                # Dashboard with Chart.js graphs
├── Global.asax.cs                      # Static HttpClient shared across all services
│
├── Infrastructure/                     # Base classes (moved from App_Code)
│   ├── BasePage.cs                     # Base page: auth check, Translate(), role props
│   ├── SessionHelper.cs                # Session: token, role, userId storage
│   └── LanguageHelper.cs              # Language switching (en/he), CultureInfo
│
├── Services/                           # API client wrappers
│   ├── ApiBase.cs                      # HTTP + JWT + JSON (Get<T>, Post, Put, Delete, Param)
│   ├── AuthService.cs                  # Login
│   ├── IncidentService.cs              # GetIncidents, GetIncident, Create, Update, Archive
│   ├── CorrectiveActionService.cs      # GetActions, Create, UpdateStatus, Delete
│   ├── DashboardService.cs             # GetDashboardStats
│   ├── AttachmentService.cs            # Upload, ProxyDownload, ProxyExport
│   ├── LookupService.cs                # GetDepartments, GetCategories, GetUsers, GetRoles
│   ├── UserService.cs                  # GetAllUsers, CreateUser, UpdateUser, Toggle
│   └── AdminService.cs                 # Departments & Categories CRUD
│
├── Incidents/
│   ├── List.aspx / .cs                 # Incident list: filters, pagination, archive tab
│   ├── Details.aspx / .cs              # Details: corrective actions, file attachments
│   ├── Create.aspx / .cs               # Create incident + file upload
│   └── Edit.aspx / .cs                 # Edit incident
│
├── CorrectiveActions/
│   └── List.aspx / .cs                 # Corrective actions list + filter
│
├── Admin/
│   ├── Users.aspx / .cs                # User management
│   ├── Departments.aspx / .cs          # Department management
│   └── Categories.aspx / .cs           # Category management
│
├── Handlers/
│   ├── DownloadAttachment.ashx.cs      # File download proxy (injects JWT)
│   └── ExportExcel.ashx.cs             # Excel export proxy (injects JWT)
│
├── App_GlobalResources/
│   ├── Strings.resx                    # English localization
│   └── Strings.he.resx                 # Hebrew localization
│
└── Content/
    ├── Site.css                        # Styles: sidebar, kpi-cards, status badges
    └── lib/                            # Bootstrap 5, Bootstrap Icons, Chart.js
```

---

## 6. File Structure — SafetyPortal.Shared

```
SafetyPortal.Shared/
├── Enums/
│   └── SharedEnums.cs                  # All domain enums
│       ├── RoleName       { Admin=1, SafetyManager=2, Supervisor=3, Employee=4 }
│       ├── IncidentStatus { Open=1, InProgress=2, Closed=3 }
│       ├── SeverityLevel  { Low=1, Medium=2, High=3, Critical=4 }
│       ├── ActionStatus   { Pending=1, InProgress=2, Completed=3 }
│       └── TextDirection  { Ltr=1, Rtl=2 }
│
├── Constants/
│   └── AppConstants.cs                 # Pagination (20/page), validation, date formats
│
└── Models/
    ├── AuthModels.cs                   # LoginRequest, LoginResponse
    ├── IncidentModels.cs               # IncidentSummary, IncidentDetail, PagedResult<T>
    ├── CorrectiveActionModels.cs       # CorrectiveActionSummary, CreateCorrectiveActionRequest
    ├── DashboardModels.cs              # DashboardStats, ChartItem, MonthItem
    ├── LookupModels.cs                 # DepartmentItem, CategoryItem, UserLookupItem
    └── UserModels.cs                   # UserSummary, CreateUserRequest, UpdateUserRequest
```

---

## 7. Roles and Permissions

```
┌──────────────────┬──────────────────────────────────────────────────────────┐
│ Role             │ Capabilities                                              │
├──────────────────┼──────────────────────────────────────────────────────────┤
│ Admin            │ Everything: user management, deletion, archiving          │
│ SafetyManager    │ Manage incidents, archive, export, send emails            │
│ Supervisor       │ Create and update corrective actions                      │
│ Employee         │ Create incidents, view ONLY their own                     │
└──────────────────┴──────────────────────────────────────────────────────────┘
```

**Employee visibility filter** (`IncidentEndpoints.cs`):
```csharp
if (isEmployee)
    query = query.Where(x =>
        x.ReportedByUserId == currentUserId ||      // reported by them
        x.AssignedToUserId == currentUserId ||      // assigned to them
        x.CorrectiveActions.Any(ca =>               // has a corrective action
            ca.AssignedToUserId == currentUserId)); // assigned to them
```

---

## 8. Authentication & Security

### JWT Token

**Created in** `JwtTokenService.cs`:
```
Claims: sub, email, ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Role
Algorithm: HMAC-SHA256
Lifetime: configured via appsettings.json → Jwt:ExpirationMinutes
```

### Brute-force protection (`AuthEndpoints.cs`)
- 5 failed attempts → account locked for 15 minutes
- IP address recorded in `AuditLog` on every event
- Rate limiting: 10 requests/minute on `/api/auth/login`

### File uploads (AttachmentEndpoints.cs + FileSignatureValidator.cs)
- MIME-type validation against whitelist
- **Magic byte validation** (first bytes of file) — prevents renaming `.exe` to `.jpg`
- Size limits: images 5 MB, documents 20 MB
- Files stored under GUID names; original name preserved for download

### HTTP Security Headers
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'self'; frame-ancestors 'none'
Strict-Transport-Security: max-age=31536000
```

---

## 9. Background Jobs (Hangfire)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Scenario 1 — Fire-and-Forget (IncidentNotificationJob.cs)              │
│ Trigger: incident created or updated                                    │
│ Action: email to reporter and assignee                                  │
│ Code: jobs.Enqueue<IncidentNotificationJob>(j => j.SendCreatedAsync(id))│
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Scenario 2 — Recurring (CorrectiveActionReminderJob.cs)                │
│ Schedule: every day at 08:00 (cron: "0 8 * * *")                       │
│ Action: email assignees whose due date is in exactly 3 days            │
│ Code: RecurringJob.AddOrUpdate(j => j.SendRemindersAsync(), "0 8 * * *")│
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Scenario 3 — Delayed (IncidentEscalationJob.cs)                        │
│ Trigger: scheduled at incident creation, runs 3 days later             │
│ Condition: only if status is still Open                                 │
│ Action: notify all Admins and SafetyManagers                           │
│ Code: jobs.Schedule<IncidentEscalationJob>(..., TimeSpan.FromDays(3))  │
└─────────────────────────────────────────────────────────────────────────┘
```

**Hangfire Dashboard:** `/hangfire` — Admin only, cookie-based authentication

---

## 10. API Endpoints — Summary

| Method | Route | Role | Description |
|--------|-------|------|-------------|
| POST | /api/auth/login | — | Login, rate-limited |
| GET | /api/auth/me | Any | Current user info |
| GET | /api/incidents | Any | List with filters and pagination |
| GET | /api/incidents/export | Any | Export to Excel |
| GET | /api/incidents/{id} | Any | Incident details |
| POST | /api/incidents | Any | Create incident |
| PUT | /api/incidents/{id} | Any | Update incident |
| PUT | /api/incidents/{id}/archive | SafetyManager+ | Archive (Closed only) |
| DELETE | /api/incidents/{id} | SafetyManager+ | Delete |
| GET | /api/corrective-actions | Any | List actions |
| GET | /api/corrective-actions/export | Any | Export to Excel |
| POST | /api/corrective-actions | Supervisor+ | Create action |
| PUT | /api/corrective-actions/{id}/status | Supervisor+ | Update status |
| DELETE | /api/corrective-actions/{id} | SafetyManager+ | Delete |
| POST | /api/incidents/{id}/attachments | Any | Upload file |
| GET | /api/incidents/{id}/attachments | Any | List files |
| GET | …/attachments/{id}/download | Any | Download file |
| DELETE | …/attachments/{id} | SafetyManager+ | Delete file |
| GET | /api/dashboard/stats | Any | KPIs and chart data |
| GET | /api/users | Admin | List users |
| POST | /api/users | Admin | Create user |
| PUT | /api/users/{id} | Admin | Update user |
| PUT | /api/users/{id}/toggle-active | Admin | Activate/deactivate |
| POST | /api/users/{id}/send-email | Admin | Send direct email |
| GET/POST/PUT/DELETE | /api/admin/categories/* | Admin | Category management |
| GET/POST/PUT/DELETE | /api/admin/departments/* | Admin | Department management |
| GET | /api/lookup/* | Any | Read-only dropdowns |

---

## 11. Database Schema

```
Users                    Roles
├─ Id                    ├─ Id
├─ FullName              └─ Name
├─ Email
├─ PasswordHash          IncidentCategories
├─ IsActive              ├─ Id
├─ FailedLoginAttempts   ├─ Name
├─ LockedUntil           ├─ Description
└─ RoleId ──────────────►└─ IsActive

Departments              IncidentReports
├─ Id                    ├─ Id
├─ Name                  ├─ ReportNumber (INC-YYYY-NNNN)
├─ LocationName          ├─ Title / Description
├─ Color (#hex)          ├─ CategoryId
└─ IsActive              ├─ DepartmentId
                         ├─ ReportedByUserId
CorrectiveActions        ├─ AssignedToUserId
├─ Id                    ├─ IncidentDate / ReportedAt
├─ ReportId ────────────►├─ LocationDetails
├─ ActionTitle           ├─ SeverityLevel (Low/Medium/High/Critical)
├─ ActionDescription     ├─ Status (Open/InProgress/Closed)
├─ AssignedToUserId      └─ IsArchived
├─ DueDate
├─ Status                IncidentAttachments
└─ PriorityLevel         ├─ Id
                         ├─ IncidentReportId
AuditLogs                ├─ OriginalFileName
├─ Id                    ├─ StoredFileName (GUID)
├─ OccurredAt            ├─ ContentType
├─ EventType             ├─ FileSizeBytes
├─ UserEmail             ├─ FileCategory (image/document)
├─ UserId                └─ UploadedByUserId
├─ IpAddress
└─ Details
```

---

## 12. Key Architectural Decisions

| Decision | Why |
|----------|-----|
| `netstandard2.0` for Shared | Compatible with both .NET FW 4.8 (Web) and .NET 10 (Api) |
| Static `HttpClient` in Web | Single instance prevents socket exhaustion |
| JWT added per-request (not DefaultRequestHeaders) | Thread-safe in Web Forms multi-threaded environment |
| Enum values match DB IDs | `RoleName.Admin = 1` matches `Roles.Id = 1` in DB |
| `Infrastructure/` instead of `App_Code/` | `App_Code` compiles twice in WAP → duplicate type conflict |
| `Translate()` instead of `T()` | Readability, follows C# naming conventions |
| Hangfire on SQL Server | Persistent queues — jobs survive application restarts |
| Magic byte validation | Content-Type header is easy to spoof; file bytes are not |

---

## 13. Localization (EN / HE)

- Resource files: `App_GlobalResources/Strings.resx` (EN) and `Strings.he.resx` (HE)
- Switching: `?lang=he` / `?lang=en` query parameter
- RTL support: Bootstrap 5 RTL CSS for Hebrew, `dir="rtl"` on `<html>`
- `TextDirection` enum: `Ltr=1`, `Rtl=2`
- `LanguageHelper.ApplyCulture()` sets `Thread.CurrentCulture` and `CurrentUICulture`

---

## 14. Automatic Behaviors

| Event | What happens automatically |
|-------|---------------------------|
| Incident created | Report number assigned (`INC-YYYY-NNNN`) + email notification + escalation job scheduled |
| Incident updated | Email notification sent to reporter and assignee |
| Incident closed | All corrective actions → status `Completed` |
| Every day at 08:00 | Reminder emails to assignees with due date in 3 days |
| 5 failed logins | Account locked for 15 minutes |
| Any login attempt | Entry written to AuditLog with IP address |

---

## 15. Default Seed Data

**Users:**
| Email | Password | Role |
|-------|----------|------|
| admin@datwise.local | Admin123! | Admin |
| safety.manager@datwise.local | Safety123! | SafetyManager |

**Departments:** Production (Plant A), Warehouse (Plant A), Maintenance (Plant B), Logistics (Plant B), Quality Assurance (HQ)

**Categories:** Near Miss, Hazard, Unsafe Condition, Unsafe Act
