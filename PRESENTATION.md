# SafetyPortal — Техническая презентация

---

## 1. Обзор проекта

SafetyPortal — система управления производственными инцидентами (Incident Management System).  
Позволяет регистрировать инциденты, назначать корректирующие действия, отслеживать статусы и получать аналитику.

**Три проекта в одном решении:**

```
DatwiseTest/
├── SafetyPortal.Api/        .NET 10 — REST API (Minimal APIs)
├── SafetyPortal.Web/        .NET Framework 4.8 — ASP.NET Web Forms (фронтенд)
└── SafetyPortal.Shared/     .NET Standard 2.0 — общие модели и константы
```

> `SafetyPortal.Shared` совместим с обоими фреймворками одновременно — именно поэтому выбран `netstandard2.0`

---

## 2. Архитектура

```
┌─────────────────────────────────────────────────────┐
│               Браузер пользователя                   │
└─────────────────────┬───────────────────────────────┘
                      │ HTTP
┌─────────────────────▼───────────────────────────────┐
│          SafetyPortal.Web (.NET FW 4.8)              │
│   ASP.NET Web Forms + Bootstrap 5 + Chart.js         │
│                                                       │
│  Infrastructure/   Services/      Handlers/           │
│  BasePage.cs       ApiBase.cs     ExportExcel.ashx    │
│  SessionHelper.cs  8 сервисов     DownloadAttach.ashx │
└─────────────────────┬───────────────────────────────┘
                      │ HTTP + JWT Bearer
┌─────────────────────▼───────────────────────────────┐
│          SafetyPortal.Api (.NET 10)                  │
│   Minimal APIs + EF Core + Hangfire + SendGrid       │
│                                                       │
│  Endpoints/        Jobs/           Services/          │
│  10 endpoint-файлов 3 фоновых задачи  JWT, Email, Excel│
└─────────────────────┬───────────────────────────────┘
                      │ EF Core
┌─────────────────────▼───────────────────────────────┐
│              SQL Server Database                      │
│  8 таблиц: Users, Roles, IncidentReports,            │
│  CorrectiveActions, Departments, Categories,          │
│  IncidentAttachments, AuditLogs                      │
└─────────────────────────────────────────────────────┘
```

---

## 3. Технологический стек

| Слой | Технология | Зачем |
|------|-----------|-------|
| API фреймворк | .NET 10 Minimal APIs | Минимальный boilerplate, высокая производительность |
| ORM | Entity Framework Core | Code-first миграции, LINQ-запросы |
| База данных | SQL Server | Надёжность, транзакции, ACID |
| Аутентификация | JWT Bearer (HS256) | Stateless, подходит для SPA и WF |
| Фоновые задачи | Hangfire + SQL Server | Персистентные очереди, retry, dashboard |
| Email | SendGrid | Надёжная доставка, шаблоны |
| Фронтенд | ASP.NET Web Forms | Быстрая разработка UI, серверный рендеринг |
| UI-фреймворк | Bootstrap 5 + Bootstrap Icons | Готовые компоненты, RTL-поддержка |
| Графики | Chart.js | Интерактивные диаграммы на dashboard |
| Экспорт | ClosedXML / EPPlus | Excel (.xlsx) без Office |

---

## 4. Структура файлов — SafetyPortal.Api

```
SafetyPortal.Api/
├── Program.cs                          # Точка входа: DI, middleware, миграции, seed
├── AppConstants.cs                     # Глобальные константы (enum RoleName, IncidentStatus…)
│
├── Endpoints/                          # Все API маршруты
│   ├── AuthEndpoints.cs                # POST /api/auth/login, GET /api/auth/me
│   ├── IncidentEndpoints.cs            # CRUD инцидентов + архивирование + экспорт
│   ├── CorrectiveActionEndpoints.cs    # CRUD корректирующих действий + экспорт
│   ├── AttachmentEndpoints.cs          # Загрузка/скачивание файлов
│   ├── DashboardEndpoints.cs           # GET /api/dashboard/stats
│   ├── UserManagementEndpoints.cs      # CRUD пользователей (Admin only)
│   ├── AdminCategoryEndpoints.cs       # Управление категориями
│   ├── AdminDepartmentEndpoints.cs     # Управление отделами
│   ├── LookupEndpoints.cs              # Справочники для выпадающих списков
│   └── HangfireLoginEndpoints.cs       # Авторизация в Hangfire Dashboard
│
├── Entities/                           # EF Core сущности (таблицы БД)
│   ├── User.cs                         # Пользователь: логин, хэш пароля, блокировка
│   ├── Role.cs                         # Роль (Admin, SafetyManager, Supervisor, Employee)
│   ├── IncidentReport.cs               # Инцидент: статус, серьёзность, архив
│   ├── CorrectiveAction.cs             # Корректирующее действие: срок, приоритет
│   ├── IncidentCategory.cs             # Категория инцидента
│   ├── Department.cs                   # Отдел с цветом и локацией
│   ├── IncidentAttachment.cs           # Файл: оригинальное имя, GUID-имя, тип
│   └── AuditLog.cs                     # Аудит: событие, IP, email, детали
│
├── Jobs/                               # Фоновые задачи Hangfire
│   ├── IncidentNotificationJob.cs      # Уведомление при создании/обновлении инцидента
│   ├── CorrectiveActionReminderJob.cs  # Ежедневное напоминание за 3 дня до срока
│   └── IncidentEscalationJob.cs        # Эскалация если инцидент открыт 3+ дней
│
├── Services/
│   ├── JwtTokenService.cs              # Создание JWT токенов с claims
│   ├── IEmailService.cs                # Интерфейс email + контексты для каждого сценария
│   ├── SendGridEmailService.cs         # Реализация через SendGrid
│   ├── FileSignatureValidator.cs       # Проверка magic bytes файлов
│   └── ExcelOrCsvCreator.cs            # Генерация .xlsx файлов
│
├── Auth/
│   ├── JwtOptions.cs                   # Настройки JWT (Issuer, Key, ExpirationMinutes)
│   └── HangfireAdminAuthorizationFilter.cs  # Cookie-auth для Hangfire
│
├── Data/
│   ├── SafetyPortalDbContext.cs        # DbContext: 8 DbSet + конфигурации связей
│   ├── DbSeeder.cs                     # Начальные данные: роли, пользователи, отделы
│   └── SafetyPortalDbContextFactory.cs # Design-time фабрика для миграций
│
├── Dtos/                               # DTO для запросов/ответов
│   ├── Auth/                           # LoginRequestDto, LoginResponseDto
│   ├── Incidents/                      # CreateIncidentDto, UpdateIncidentDto, IncidentFilterQuery
│   ├── CorrectiveActions/              # CreateCorrectiveActionDto, UpdateActionStatusDto
│   ├── Users/                          # CreateUserDto, UpdateUserDto, UserSummaryDto
│   ├── Categories/                     # CreateCategoryDto, UpdateCategoryDto
│   └── Departments/                    # CreateDepartmentDto, UpdateDepartmentDto
│
└── Migrations/                         # EF Core миграции (6 штук)
```

---

## 5. Структура файлов — SafetyPortal.Web

```
SafetyPortal.Web/
├── Site.Master                         # Мастер-страница: sidebar, topbar, язык
├── Site.Master.cs                      # Code-behind: роль, язык, dir (rtl/ltr)
├── Login.aspx / Login.aspx.cs          # Форма входа
├── Dashboard.aspx / .cs                # Дашборд с графиками Chart.js
├── Global.asax.cs                      # Статический HttpClient для всех сервисов
│
├── Infrastructure/                     # Базовые классы (перенесены из App_Code)
│   ├── BasePage.cs                     # Базовая страница: аутентификация, Translate(), роли
│   ├── SessionHelper.cs                # Работа с сессией: токен, роль, userId
│   └── LanguageHelper.cs              # Переключение языка (en/he), CultureInfo
│
├── Services/                           # Клиенты к API
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
│   ├── List.aspx / .cs                 # Список инцидентов: фильтры, пагинация, архив
│   ├── Details.aspx / .cs              # Детали: корректирующие действия, файлы
│   ├── Create.aspx / .cs               # Создание инцидента + загрузка файлов
│   └── Edit.aspx / .cs                 # Редактирование инцидента
│
├── CorrectiveActions/
│   └── List.aspx / .cs                 # Список корректирующих действий + фильтр
│
├── Admin/
│   ├── Users.aspx / .cs                # Управление пользователями
│   ├── Departments.aspx / .cs          # Управление отделами
│   └── Categories.aspx / .cs           # Управление категориями
│
├── Handlers/
│   ├── DownloadAttachment.ashx.cs      # Прокси для скачивания файлов
│   └── ExportExcel.ashx.cs             # Прокси для экспорта Excel
│
├── App_GlobalResources/
│   ├── Strings.resx                    # Локализация EN
│   └── Strings.he.resx                 # Локализация HE (иврит)
│
└── Content/
    ├── Site.css                        # Стили: sidebar, kpi-cards, badges
    └── lib/                            # Bootstrap 5, Bootstrap Icons, Chart.js
```

---

## 6. Структура файлов — SafetyPortal.Shared

```
SafetyPortal.Shared/
├── Enums/
│   └── SharedEnums.cs                  # Все enum'ы домена
│       ├── RoleName       { Admin=1, SafetyManager=2, Supervisor=3, Employee=4 }
│       ├── IncidentStatus { Open=1, InProgress=2, Closed=3 }
│       ├── SeverityLevel  { Low=1, Medium=2, High=3, Critical=4 }
│       ├── ActionStatus   { Pending=1, InProgress=2, Completed=3 }
│       └── TextDirection  { Ltr=1, Rtl=2 }
│
├── Constants/
│   └── AppConstants.cs                 # Пагинация (20/стр), валидация, форматы дат
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

## 7. Роли и права доступа

```
┌──────────────┬────────────────────────────────────────────────────────────┐
│ Роль         │ Возможности                                                │
├──────────────┼────────────────────────────────────────────────────────────┤
│ Admin        │ Всё: управление пользователями, удаление, архивирование   │
│ SafetyManager│ Управление инцидентами, архивирование, экспорт, рассылки  │
│ Supervisor   │ Создание/обновление корректирующих действий               │
│ Employee     │ Создание инцидентов, просмотр ТОЛЬКО своих               │
└──────────────┴────────────────────────────────────────────────────────────┘
```

**Фильтрация для Employee** (`IncidentEndpoints.cs`):
```csharp
if (isEmployee)
    query = query.Where(x =>
        x.ReportedByUserId == currentUserId ||      // сам создал
        x.AssignedToUserId == currentUserId ||      // назначен ответственным
        x.CorrectiveActions.Any(ca =>               // есть корректирующее
            ca.AssignedToUserId == currentUserId)); // действие на него
```

---

## 8. Аутентификация и безопасность

### JWT Токен

**Создаётся в** `JwtTokenService.cs`:
```
Claims: sub, email, ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Role
Алгоритм: HMAC-SHA256
Срок жизни: настраивается в appsettings.json → Jwt:ExpirationMinutes
```

### Защита от брутфорса (`AuthEndpoints.cs`)
- 5 неудачных попыток → блокировка на 15 минут
- IP адрес записывается в `AuditLog` при каждом событии
- Rate limiting: 10 запросов/минуту на endpoint `/api/auth/login`

### Файлы (AttachmentEndpoints.cs + FileSignatureValidator.cs)
- Проверка MIME-type по белому списку
- Проверка **magic bytes** (первые байты файла) — нельзя переименовать `.exe` в `.jpg`
- Лимиты: изображения 5 MB, документы 20 MB
- Хранение под GUID-именами

### HTTP заголовки безопасности
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'self'; frame-ancestors 'none'
Strict-Transport-Security: max-age=31536000
```

---

## 9. Фоновые задачи Hangfire

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Сценарий 1 — Fire-and-Forget (IncidentNotificationJob.cs)              │
│ Триггер: создание или обновление инцидента                              │
│ Действие: email репортёру и ответственному                             │
│ Код: jobs.Enqueue<IncidentNotificationJob>(j => j.SendCreatedAsync(id)) │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Сценарий 2 — Recurring (CorrectiveActionReminderJob.cs)                │
│ Расписание: каждый день в 08:00 (cron: "0 8 * * *")                   │
│ Действие: email исполнителям у кого срок через 3 дня                  │
│ Код: RecurringJob.AddOrUpdate(j => j.SendRemindersAsync(), "0 8 * * *") │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Сценарий 3 — Delayed (IncidentEscalationJob.cs)                        │
│ Триггер: при создании инцидента, выполняется через 3 дня               │
│ Условие: только если статус всё ещё Open                               │
│ Действие: уведомить всех Admin и SafetyManager                         │
│ Код: jobs.Schedule<IncidentEscalationJob>(..., TimeSpan.FromDays(3))   │
└─────────────────────────────────────────────────────────────────────────┘
```

**Hangfire Dashboard:** `/hangfire` — только для Admin, cookie-аутентификация

---

## 10. API Endpoints — сводная таблица

| Метод | Маршрут | Роль | Описание |
|-------|---------|------|----------|
| POST | /api/auth/login | — | Вход, rate-limited |
| GET | /api/auth/me | Любой | Текущий пользователь |
| GET | /api/incidents | Любой | Список с фильтрами и пагинацией |
| GET | /api/incidents/export | Любой | Экспорт в Excel |
| GET | /api/incidents/{id} | Любой | Детали инцидента |
| POST | /api/incidents | Любой | Создать инцидент |
| PUT | /api/incidents/{id} | Любой | Обновить инцидент |
| PUT | /api/incidents/{id}/archive | SafetyManager+ | Архивировать (только Closed) |
| DELETE | /api/incidents/{id} | SafetyManager+ | Удалить |
| GET | /api/corrective-actions | Любой | Список действий |
| GET | /api/corrective-actions/export | Любой | Экспорт в Excel |
| POST | /api/corrective-actions | Supervisor+ | Создать действие |
| PUT | /api/corrective-actions/{id}/status | Supervisor+ | Обновить статус |
| DELETE | /api/corrective-actions/{id} | SafetyManager+ | Удалить |
| POST | /api/incidents/{id}/attachments | Любой | Загрузить файл |
| GET | /api/incidents/{id}/attachments | Любой | Список файлов |
| GET | …/attachments/{id}/download | Любой | Скачать файл |
| DELETE | …/attachments/{id} | SafetyManager+ | Удалить файл |
| GET | /api/dashboard/stats | Любой | Статистика и KPI |
| GET | /api/users | Admin | Список пользователей |
| POST | /api/users | Admin | Создать пользователя |
| PUT | /api/users/{id} | Admin | Обновить пользователя |
| PUT | /api/users/{id}/toggle-active | Admin | Активировать/деактивировать |
| POST | /api/users/{id}/send-email | Admin | Отправить email |
| GET/POST/PUT/DELETE | /api/admin/categories/* | Admin | Управление категориями |
| GET/POST/PUT/DELETE | /api/admin/departments/* | Admin | Управление отделами |
| GET | /api/lookup/* | Любой | Справочники |

---

## 11. База данных — схема таблиц

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

## 12. Ключевые архитектурные решения

| Решение | Почему |
|---------|--------|
| `netstandard2.0` для Shared | Совместим с .NET FW 4.8 (Web) и .NET 10 (Api) одновременно |
| Статический `HttpClient` в Web | Один экземпляр на всё приложение — избегает исчерпания сокетов |
| JWT per-request (не DefaultRequestHeaders) | Thread-safe в многопоточной среде Web Forms |
| Enum с явными ID = ID в БД | `RoleName.Admin = 1` совпадает с `Roles.Id = 1` в БД |
| `Infrastructure/` вместо `App_Code/` | `App_Code` компилируется дважды в WAP → конфликт типов |
| `Translate()` вместо `T()` | Читаемость кода, соответствие C# naming conventions |
| Hangfire на SQL Server | Персистентные очереди — задачи не теряются при перезапуске |
| Magic byte валидация | MIME-type в заголовке легко подделать, bytes файла — нет |

---

## 13. Локализация (EN / HE)

- Ресурсные файлы: `App_GlobalResources/Strings.resx` (EN) и `Strings.he.resx` (HE)
- Переключение: `?lang=he` / `?lang=en` в URL
- RTL поддержка: Bootstrap 5 RTL CSS для иврита, `dir="rtl"` на `<html>`
- `TextDirection` enum: `Ltr=1`, `Rtl=2`
- `LanguageHelper.ApplyCulture()` устанавливает `Thread.CurrentCulture` и `CurrentUICulture`

---

## 14. Автоматическое поведение

| Событие | Что происходит автоматически |
|---------|------------------------------|
| Создание инцидента | Присваивается номер `INC-YYYY-NNNN` + email-уведомление + планирование эскалации |
| Обновление инцидента | Email-уведомление об изменениях |
| Закрытие инцидента | Все корректирующие действия → статус `Completed` |
| Каждый день 08:00 | Напоминания исполнителям у кого срок через 3 дня |
| 5 неудачных входов | Аккаунт блокируется на 15 минут |
| Любой вход | Запись в AuditLog с IP адресом |
