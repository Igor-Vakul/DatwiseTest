# SafetyPortal — Workplace Safety Management System
> Datwise Technical Assignment | Candidate submission

---

## 3.1 Business Need

A **Safety Officer (מנהל בטיחות)** at an industrial facility faces several daily challenges:

| Challenge | Impact |
|-----------|--------|
| Incidents reported on paper or in spreadsheets | Data is lost, unsearchable, no audit trail |
| No centralized tracking of corrective actions | Actions fall through the cracks, overdue items unnoticed |
| No real-time KPIs for management | Safety officer cannot demonstrate trends or prioritize work |
| Role-based access not enforced | Employees access data they should not see |

**SafetyPortal** digitizes this workflow end-to-end: incident submission → assignment → corrective actions → closure — with a live dashboard showing KPIs and trend charts.

---

## 3.2 Solution Chosen

**Incident Reporting & Corrective Action Management Portal**

**Advanced extensions implemented:**
1. 🔐 **Information Security** — JWT-based authentication, role-based authorization (Admin / SafetyManager / Supervisor / Employee), API endpoints protected by policy
2. 📊 **Report Visualization & Dashboard** — KPI cards, doughnut/bar/line charts (Chart.js), recent incidents table

---

## 3.3 Architectural Explanation

```
┌──────────────────────────────────┐      HTTPS / REST+JWT     ┌────────────────────────────────┐
│     SafetyPortal.Web             │ ─────────────────────────► │     SafetyPortal.Api           │
│     ASP.NET Web Forms            │                            │     .NET 10 Minimal API        │
│     .NET Framework 4.8           │ ◄───────────────────────── │     JWT Bearer Auth            │
│     IIS Express (port 44300)     │       JSON responses       │     EF Core 10 + SQL Server    │
└──────────────────────────────────┘                            └────────────────┬───────────────┘
                                                                                 │
                                                                                 ▼
                                                                  ┌──────────────────────────────┐
                                                                  │     SQL Server Express       │
                                                                  │     SafetyPortalDb           │
                                                                  │     6 tables + seeded data   │
                                                                  └──────────────────────────────┘
```

### Projects

| Project | Technology | Purpose |
|---------|-----------|---------|
| `SafetyPortal.Api` | .NET 10, Minimal API, EF Core, JWT | Backend REST API |
| `SafetyPortal.Web` | ASP.NET Web Forms, .NET FW 4.8 | Frontend UI (server-rendered) |

### Database Schema

```
Roles ◄──── Users ─────┐
                        │
Departments ────────────┤──── IncidentReports ──── CorrectiveActions
                        │
IncidentCategories ─────┘
```

### API Endpoints

| Group | Endpoints |
|-------|-----------|
| Auth | `POST /api/auth/login`, `GET /api/auth/me` |
| Incidents | `GET/POST /api/incidents`, `GET/PUT/DELETE /api/incidents/{id}` |
| Corrective Actions | `GET/POST /api/corrective-actions`, `PUT /{id}/status`, `DELETE /{id}` |
| Dashboard | `GET /api/dashboard/stats` |
| Lookup | `GET /api/lookup/departments`, `/categories`, `/users`, `/roles` |
| Users (Admin) | `GET/POST /api/users`, `PUT /api/users/{id}/toggle-active` |

### Security Architecture

- **JWT tokens** signed with HS256, 120-minute expiry
- **Authorization policies**: `AdminOnly`, `SafetyManagerOrAdmin`, `Authenticated`
- **Server-side session** stores JWT in Web Forms (not exposed to browser)
- **Role-based UI**: Admin menu visible only to Admins; delete/manage buttons gated by role

### Web Forms Pages

| Page | Access | Description |
|------|--------|-------------|
| `Login.aspx` | Public | Login with email/password |
| `Dashboard.aspx` | All roles | KPI cards + 3 charts + recent incidents |
| `Incidents/List.aspx` | All roles | Paginated list with search/filter |
| `Incidents/Create.aspx` | All roles | Submit new incident report |
| `Incidents/Details.aspx` | All roles | View details + add corrective actions |
| `Incidents/Edit.aspx` | All roles | Edit existing incident |
| `CorrectiveActions/List.aspx` | All roles | All actions with overdue highlight |
| `Admin/Users.aspx` | Admin only | Create/toggle users |

---

## 3.4 Running Instructions

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- SQL Server Express (LocalDB also works)
- Visual Studio 2022 (recommended) **or** VS Code

### Step 1 — Configure Database

Edit `SafetyPortal.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SafetyPortalDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### Step 2 — Run the API

```bash
cd SafetyPortal.Api
dotnet run
```

The API will:
- Auto-apply EF Core migrations
- Seed the database (roles, departments, categories, 2 users)
- Start at `https://localhost:7182`
- Swagger UI: `https://localhost:7182/swagger`

### Step 3 — Restore NuGet packages for Web

```bash
cd SafetyPortal.Web
nuget restore
```
Or open `Datwise.sln` in Visual Studio → it will restore automatically.

### Step 4 — Run the Web App

Open `Datwise.sln` in Visual Studio 2022 → Set `SafetyPortal.Web` as startup project → Press **F5**.

The app will open at `https://localhost:44300`.

### Step 5 — Login

| Account | Email | Password | Role |
|---------|-------|----------|------|
| System Admin | `admin@datwise.local` | `Admin123!` | Admin |
| Safety Manager | `safety.manager@datwise.local` | `Safety123!` | SafetyManager |

### Optional: Load Sample Data via SQL

```sql
-- Run database/schema.sql against an empty SafetyPortalDb
-- (only needed if you prefer SQL script over EF migrations)
```

---

## Project Structure

```
c:\DatWiseTest\
├── Datwise.sln
├── README.md
├── database/
│   └── schema.sql                    ← DDL + sample data
├── SafetyPortal.Api/                 ← .NET 10 Minimal API
│   ├── Auth/JwtOptions.cs
│   ├── Data/SafetyPortalDbContext.cs
│   ├── Data/DbSeeder.cs
│   ├── Dtos/
│   │   ├── Auth/
│   │   ├── Incidents/
│   │   ├── CorrectiveActions/
│   │   └── Users/
│   ├── Endpoints/
│   │   ├── AuthEndpoints.cs
│   │   ├── IncidentEndpoints.cs
│   │   ├── CorrectiveActionEndpoints.cs
│   │   ├── DashboardEndpoints.cs
│   │   ├── LookupEndpoints.cs
│   │   └── UserManagementEndpoints.cs
│   ├── Entities/
│   ├── Services/JwtTokenService.cs
│   └── Program.cs
└── SafetyPortal.Web/                 ← ASP.NET Web Forms .NET FW 4.8
    ├── App_Code/
    │   ├── ApiClient.cs              ← HTTP client wrapping the API
    │   ├── BasePage.cs               ← Auth guard base class
    │   ├── Models.cs                 ← DTOs mirroring API responses
    │   └── SessionHelper.cs         ← JWT session management
    ├── Content/Site.css
    ├── Incidents/
    ├── CorrectiveActions/
    ├── Admin/
    ├── Site.Master
    ├── Login.aspx
    ├── Dashboard.aspx
    └── Web.config
```

---

## Evaluation Criteria Coverage

| Criterion | Implementation |
|-----------|---------------|
| Business/Tech integration | Safety Officer workflow: incident → CA → dashboard |
| Web Forms + SQL Server | ASP.NET Web Forms 4.8 + SQL Server via EF Core |
| UI/UX | Bootstrap 5, responsive sidebar, color-coded badges, Chart.js charts |
| Management thinking | Role-based access, risk levels (severity/priority), overdue highlighting |
| Creativity | Server-side JWT session bridge between .NET FW and .NET 10 API |
