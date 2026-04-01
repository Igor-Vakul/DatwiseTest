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
2. 📊 **Report Visualization & Dashboard** — KPI cards, doughnut/bar/line charts (Chart.js), recent incidents table; donut chart supports Category / Severity / Status grouping with count + percentage in legend
3. ⏱️ **Background Jobs (Hangfire)** — Fire-and-forget email notifications, daily recurring reminders, delayed escalation scheduling
4. 📎 **File Attachments** — Upload/download/delete incident attachments with magic-byte validation and size limits
5. 📥 **Excel Export** — Filtered export of incidents and corrective actions using EPPlus
6. 🏢 **Admin: Departments & Categories** — Full CRUD with Bootstrap modals, department color picker (colors reflected in dashboard bar chart), IsActive toggle, delete restriction on open incidents
7. 📦 **Incident Archive** — Soft-delete via `IsArchived` flag; only Closed incidents can be archived; archived incidents excluded from dashboard stats; Active/Archived tabs on list page

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
                                                                  │     8 tables + seeded data   │
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
(Color, IsActive)       │    (IsArchived)    │
                        │                   └──── IncidentAttachments
IncidentCategories ─────┘
(IsActive)

AuditLogs  (standalone — login events, failed attempts, lockouts)
```

### API Endpoints

| Group | Endpoints |
|-------|-----------|
| Auth | `POST /api/auth/login`, `GET /api/auth/me` |
| Incidents | `GET/POST /api/incidents`, `GET/PUT/DELETE /api/incidents/{id}`, `PUT /{id}/archive` |
| Corrective Actions | `GET/POST /api/corrective-actions`, `PUT /{id}/status`, `DELETE /{id}` |
| Dashboard | `GET /api/dashboard/stats` |
| Lookup | `GET /api/lookup/departments`, `/categories` (active only), `/users`, `/roles` |
| Users (Admin) | `GET/POST /api/users`, `PUT /api/users/{id}/toggle-active` |
| Admin Departments | `GET/POST /api/admin/departments`, `PUT /{id}`, `PUT /{id}/toggle-active`, `DELETE /{id}` |
| Admin Categories | `GET/POST /api/admin/categories`, `PUT /{id}`, `PUT /{id}/toggle-active`, `DELETE /{id}` |
| Attachments | `POST/GET /api/incidents/{id}/attachments`, `GET /{attId}/download`, `DELETE /{attId}` |
| Export | `GET /api/incidents/export`, `GET /api/corrective-actions/export` |
| Hangfire | `GET /hangfire` (Admin dashboard), `GET/POST /jobs/login`, `GET /jobs/logout` |

### Security Architecture

- **JWT tokens** signed with HS256, 120-minute expiry
- **Authorization policies**: `AdminOnly`, `SafetyManagerOrAdmin`, `Authenticated`
- **Server-side session** stores JWT in Web Forms (not exposed to browser)
- **Role-based UI**: Admin menu visible only to Admins; delete/manage buttons gated by role

#### Rate Limiting

| Policy | Limit | Scope |
|--------|-------|-------|
| `login` | 10 requests / minute / IP | `POST /api/auth/login` only |
| `api` | 300 requests / minute / IP | All other API endpoints |

Requests exceeding the limit receive `429 Too Many Requests`.

#### Account Lockout

After **5 consecutive failed** login attempts the account is locked for **15 minutes**.
The lockout state (`FailedLoginAttempts`, `LockedUntil`) is stored in the `Users` table.
A successful login resets the counter.
Locked accounts receive `423 Locked` with a message indicating remaining time.

#### Security HTTP Headers

Applied to every API response and every Web Forms page:

| Header | Value |
|--------|-------|
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` |
| `Content-Security-Policy` | `default-src 'none'; frame-ancestors 'none'` (API) |
| `X-Powered-By` | removed |

#### Open Redirect Prevention

`Login.aspx` validates the `returnUrl` parameter: only paths starting with `/` that do **not** begin with `//` or `/\` are accepted. All other values redirect to `~/Dashboard.aspx`.

#### Password Policy

Enforced at both the Web Forms layer (`BasePage.ValidatePassword`) and the API layer (`[RegularExpression]` DataAnnotation on DTOs):

- Minimum **8 characters**
- At least one **uppercase** letter
- At least one **lowercase** letter
- At least one **digit**
- At least one **special character** (`!@#$%^&*` etc.)

#### Audit Log

All authentication events are persisted to the `AuditLogs` table with timestamp, event type, user email, user ID, and source IP:

| EventType | Trigger |
|-----------|---------|
| `LoginFailed` | Wrong password or unknown email |
| `AccountLocked` | Account locked after 5 failed attempts |
| `LoginBlocked` | Login attempt while account is already locked |
| `LoginSuccess` | Successful authentication |

#### XSS / HTML Injection Protection

Protection is applied at two independent layers so that neither layer alone is a single point of failure.

| Layer | Where | What it does |
|-------|-------|-------------|
| **Input sanitization** | Web Forms code-behind, on every form submit | `StripHtml()` (compiled regex `<[^>]*>`) removes all HTML tags from every free-text field before the value is sent to the API |
| **Output encoding** | Every `.aspx` template that renders a string | `System.Web.HttpUtility.HtmlEncode()` wraps every `<%= ... %>` expression that outputs user-controlled or database-sourced text |
| **JSON injection guard** | `Dashboard.aspx.cs` | `SafeJson()` replaces `</` → `<\/` in JSON strings inlined into `<script>` blocks, preventing `</script>` breakout |
| **API input validation** | All Minimal API DTOs | `[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]` DataAnnotations + `builder.Services.AddValidation()` enforce length limits and reject malformed input at the API boundary |

**Fields sanitized on input (`StripHtml`):**

| Page | Fields |
|------|--------|
| `Login.aspx` | Email |
| `Incidents/Create.aspx` | Title, Description, Location Details |
| `Incidents/Edit.aspx` | Title, Description, Location Details |
| `Incidents/Details.aspx` | Corrective Action Title, Corrective Action Description |
| `Admin/Users.aspx` | Full Name (create & edit), Email, Email Subject, Email Body |

### Web Forms Pages

| Page | Access | Description |
|------|--------|-------------|
| `Login.aspx` | Public | Login with email/password |
| `Dashboard.aspx` | All roles | KPI cards + 3 charts (donut by Category/Severity/Status, bar by department with colors, trend line) + recent incidents |
| `Incidents/List.aspx` | All roles | Paginated list with search/filter, Active/Archived tabs |
| `Incidents/Create.aspx` | All roles | Submit new incident report |
| `Incidents/Details.aspx` | All roles | View details, corrective actions, upload/download/delete attachments |
| `Incidents/Edit.aspx` | All roles | Edit existing incident |
| `CorrectiveActions/List.aspx` | All roles | All actions with overdue highlight |
| `Admin/Users.aspx` | Admin only | Create/toggle-active users |
| `Admin/Departments.aspx` | Admin only | CRUD departments with color picker and IsActive toggle |
| `Admin/Categories.aspx` | Admin only | CRUD incident categories with IsActive toggle |

---

### Background Jobs (Hangfire)

Hangfire is wired into `SafetyPortal.Api` and uses the same SQL Server database for job storage.

| Scenario | Type | Description |
|----------|------|-------------|
| Incident created | Fire-and-forget | Email notification sent to the assigned user immediately |
| Incident updated | Fire-and-forget | Email notification sent to the assigned user immediately |
| Corrective action due | Recurring (daily 08:00) | Reminder email sent 3 days before the due date |
| Incident escalation | Delayed (3 days) | If incident is still Open after 3 days, all Admins/SafetyManagers are notified |

#### Hangfire Dashboard Access

The Hangfire dashboard is available at `/hangfire` and is protected — **Admin role only**.

1. Navigate to `https://localhost:7182/jobs/login`
2. Log in with an Admin account (e.g. `admin@datwise.local` / `Admin123!`)
3. You will be redirected to `https://localhost:7182/hangfire`

The dashboard shows queued, processing, succeeded and failed jobs in real time.

> **Note:** The login at `/jobs/login` uses a separate cookie (`HangfireAuth`) that is independent from the main Web Forms session. This is required because the Hangfire dashboard is served by the API project, not the Web project.

#### Email Configuration (SendGrid)

Email sending requires a SendGrid API key. Configure it in `SafetyPortal.Api/appsettings.json`:

```json
"SendGrid": {
  "ApiKey": "YOUR_SENDGRID_API_KEY",
  "FromEmail": "noreply@yourdomain.com",
  "FromName": "SafetyPortal"
}
```

If the API key is not set, job execution will fail silently (jobs are still queued and visible in the dashboard).

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

## Changelog

### v1.3.0
- Admin CRUD pages for Departments and Categories (Bootstrap modals, IsActive toggle, delete restricted to open-incident-free records)
- Department color picker — colors reflected in dashboard "Incidents by Department" bar chart (per-bar colors)
- Inactive categories hidden from incident creation/edit dropdowns
- `Roles` static class replaced with `enum` in `AppConstants`
- Hebrew/English localization for all new admin pages

### v1.2.0
- Incident archive feature — `IsArchived` flag, Active/Archived tabs on list page, archive endpoint restricted to Closed status, archived incidents excluded from dashboard stats
- Dashboard donut chart: dropdown to switch grouping between Category / Severity / Status client-side; legend shows count + percentage
- `[AsParameters]` record (`IncidentFilterQuery`) replaces 8 individual query params on incident GET endpoints

### v1.1.0
- Password policy enforcement (min 8 chars, upper, lower, digit, special)
- Account lockout after 5 failed login attempts (15 min, stored in DB)
- Audit log for all auth events (`AuditLogs` table)
- Security HTTP headers on all responses
- Open redirect prevention on `Login.aspx`
- File attachment support (upload / download / delete, magic-byte validation)
- Excel export for incidents and corrective actions

### v1.0.0
- Initial release: incident reporting, corrective actions, dashboard, role-based access, JWT auth, Hangfire background jobs

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
│   │   ├── Incidents/          ← includes IncidentFilterQuery ([AsParameters])
│   │   ├── CorrectiveActions/
│   │   ├── Departments/
│   │   ├── Categories/
│   │   └── Users/
│   ├── Endpoints/
│   │   ├── AuthEndpoints.cs
│   │   ├── IncidentEndpoints.cs
│   │   ├── CorrectiveActionEndpoints.cs
│   │   ├── DashboardEndpoints.cs
│   │   ├── LookupEndpoints.cs
│   │   ├── AdminDepartmentEndpoints.cs
│   │   ├── AdminCategoryEndpoints.cs
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
    │   ├── Users.aspx
    │   ├── Departments.aspx    ← CRUD + color picker + IsActive toggle
    │   └── Categories.aspx     ← CRUD + IsActive toggle
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

---

## Role Hierarchy

```
Admin
  └── SafetyManager
        └── Supervisor
              └── Employee
```

| Role | Capabilities |
|------|-------------|
| **Admin** | Full access: user management, delete incidents/attachments/actions, Hangfire dashboard |
| **SafetyManager** | All except user management: create/edit/delete incidents, manage corrective actions, delete attachments |
| **Supervisor** | Create/edit incidents, add corrective actions, mark actions complete, upload attachments |
| **Employee** | View and create incidents only — read-only access to corrective actions |

### Authorization Policies

| Policy | Roles |
|--------|-------|
| `AdminOnly` | Admin |
| `SafetyManagerOrAdmin` | Admin, SafetyManager |
| `SupervisorOrAbove` | Admin, SafetyManager, Supervisor |
| `Authenticated` | All logged-in users |

### Permissions Matrix

| Action | Employee | Supervisor | SafetyManager | Admin |
|--------|:--------:|:----------:|:-------------:|:-----:|
| View incidents | ✅ | ✅ | ✅ | ✅ |
| Create incidents | ✅ | ✅ | ✅ | ✅ |
| Edit incidents | ✅ | ✅ | ✅ | ✅ |
| Upload attachments | ✅ | ✅ | ✅ | ✅ |
| Download attachments | ✅ | ✅ | ✅ | ✅ |
| Add corrective actions | ❌ | ✅ | ✅ | ✅ |
| Mark action complete | ❌ | ✅ | ✅ | ✅ |
| Delete incidents | ❌ | ❌ | ✅ | ✅ |
| Delete attachments | ❌ | ❌ | ✅ | ✅ |
| Delete corrective actions | ❌ | ❌ | ✅ | ✅ |
| Archive/unarchive incidents | ❌ | ❌ | ✅ | ✅ |
| User management | ❌ | ❌ | ❌ | ✅ |
| Manage departments | ❌ | ❌ | ❌ | ✅ |
| Manage categories | ❌ | ❌ | ❌ | ✅ |
| Hangfire dashboard | ❌ | ❌ | ❌ | ✅ |
