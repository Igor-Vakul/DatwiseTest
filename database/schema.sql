-- =============================================================
--  SafetyPortal Database – Schema + Sample Data
--  SQL Server 2019+ / SQL Server Express
--  Run against an empty database named: SafetyPortalDb
-- =============================================================

USE SafetyPortalDb;
GO

-- ── Roles ──────────────────────────────────────────────────────────────────
CREATE TABLE Roles (
    Id   INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
GO

-- ── Users ──────────────────────────────────────────────────────────────────
CREATE TABLE Users (
    Id           INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    IsActive     BIT           NOT NULL DEFAULT 1,
    RoleId       INT           NOT NULL,
    CONSTRAINT UQ_Users_Email  UNIQUE (Email),
    CONSTRAINT FK_Users_Roles  FOREIGN KEY (RoleId) REFERENCES Roles(Id)
        ON DELETE NO ACTION ON UPDATE NO ACTION
);
GO

-- ── Departments ────────────────────────────────────────────────────────────
CREATE TABLE Departments (
    Id           INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100) NOT NULL,
    LocationName NVARCHAR(100) NULL,
    IsActive     BIT           NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Departments_Name UNIQUE (Name)
);
GO

-- ── IncidentCategories ────────────────────────────────────────────────────
CREATE TABLE IncidentCategories (
    Id          INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    CONSTRAINT UQ_IncidentCategories_Name UNIQUE (Name)
);
GO

-- ── IncidentReports ───────────────────────────────────────────────────────
CREATE TABLE IncidentReports (
    Id               INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    ReportNumber     NVARCHAR(20)  NOT NULL,
    Title            NVARCHAR(200) NOT NULL,
    Description      NVARCHAR(MAX) NOT NULL,
    CategoryId       INT           NOT NULL,
    DepartmentId     INT           NOT NULL,
    ReportedByUserId INT           NOT NULL,
    AssignedToUserId INT           NULL,
    IncidentDate     DATETIME2     NOT NULL,
    ReportedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    LocationDetails  NVARCHAR(200) NULL,
    SeverityLevel    NVARCHAR(20)  NOT NULL,  -- Low | Medium | High | Critical
    Status           NVARCHAR(30)  NOT NULL,  -- Open | InProgress | Closed
    CONSTRAINT UQ_IncidentReports_Number UNIQUE (ReportNumber),
    CONSTRAINT FK_IR_Category   FOREIGN KEY (CategoryId)       REFERENCES IncidentCategories(Id),
    CONSTRAINT FK_IR_Dept       FOREIGN KEY (DepartmentId)     REFERENCES Departments(Id),
    CONSTRAINT FK_IR_Reporter   FOREIGN KEY (ReportedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_IR_Assigned   FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id)
);
GO

-- ── IncidentAttachments ──────────────────────────────────────────────────
CREATE TABLE IncidentAttachments (
    Id                INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    IncidentReportId  INT           NOT NULL,
    OriginalFileName  NVARCHAR(260) NOT NULL,
    StoredFileName    NVARCHAR(260) NOT NULL,
    ContentType       NVARCHAR(100) NOT NULL,
    FileSizeBytes     BIGINT        NOT NULL,
    FileCategory      NVARCHAR(20)  NOT NULL,  -- image | document
    UploadedAt        DATETIME2     NOT NULL,
    UploadedByUserId  INT           NOT NULL,
    CONSTRAINT FK_IA_Report    FOREIGN KEY (IncidentReportId)  REFERENCES IncidentReports(Id) ON DELETE CASCADE,
    CONSTRAINT FK_IA_Uploader  FOREIGN KEY (UploadedByUserId)  REFERENCES Users(Id)
);
CREATE INDEX IX_IA_IncidentReportId  ON IncidentAttachments (IncidentReportId);
CREATE INDEX IX_IA_UploadedByUserId  ON IncidentAttachments (UploadedByUserId);
GO

-- ── CorrectiveActions ─────────────────────────────────────────────────────
CREATE TABLE CorrectiveActions (
    Id                INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    ReportId          INT           NOT NULL,
    ActionTitle       NVARCHAR(200) NOT NULL,
    ActionDescription NVARCHAR(500) NULL,
    AssignedToUserId  INT           NOT NULL,
    DueDate           DATE          NOT NULL,
    CompletedAt       DATETIME2     NULL,
    Status            NVARCHAR(30)  NOT NULL,  -- Pending | InProgress | Completed
    PriorityLevel     NVARCHAR(20)  NOT NULL,  -- Low | Medium | High | Critical
    CONSTRAINT FK_CA_Report   FOREIGN KEY (ReportId)         REFERENCES IncidentReports(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CA_Assigned FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id)
);
GO

-- =============================================================
--  SEED DATA
-- =============================================================

-- Roles
SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (Id, Name) VALUES
    (1, 'Admin'),
    (2, 'SafetyManager'),
    (3, 'Supervisor'),
    (4, 'Employee');
SET IDENTITY_INSERT Roles OFF;
GO

-- Departments
SET IDENTITY_INSERT Departments ON;
INSERT INTO Departments (Id, Name, LocationName, IsActive) VALUES
    (1, 'Production',        'Plant A', 1),
    (2, 'Warehouse',         'Plant A', 1),
    (3, 'Maintenance',       'Plant B', 1),
    (4, 'Logistics',         'Plant B', 1),
    (5, 'Quality Assurance', 'HQ',      1);
SET IDENTITY_INSERT Departments OFF;
GO

-- Incident Categories
SET IDENTITY_INSERT IncidentCategories ON;
INSERT INTO IncidentCategories (Id, Name, Description) VALUES
    (1, 'Near Miss',         'Event that could have caused injury or damage but did not'),
    (2, 'Hazard',            'Identified workplace hazard'),
    (3, 'Unsafe Condition',  'Unsafe physical condition in workplace'),
    (4, 'Unsafe Act',        'Unsafe employee action or behavior');
SET IDENTITY_INSERT IncidentCategories OFF;
GO

-- Users  (passwords are ASP.NET Identity PasswordHasher hashes)
-- admin@datwise.local / Admin123!
-- safety.manager@datwise.local / Safety123!
-- These hashes are set by DbSeeder.cs at first run; the rows below use placeholder hashes.
-- Run the application once to let DbSeeder create the real hashed users,
-- OR use the API POST /api/users after first login.
--
-- Placeholder inserts (will be skipped by DbSeeder if rows already exist):
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, FullName, Email, PasswordHash, IsActive, RoleId) VALUES
    (1, 'System Admin',  'admin@datwise.local',           'PLACEHOLDER_RUN_APP', 1, 1),
    (2, 'Dana Levi',     'safety.manager@datwise.local',  'PLACEHOLDER_RUN_APP', 1, 2),
    (3, 'Avi Cohen',     'supervisor@datwise.local',       'PLACEHOLDER_RUN_APP', 1, 3),
    (4, 'Yael Shapiro',  'employee@datwise.local',         'PLACEHOLDER_RUN_APP', 1, 4);
SET IDENTITY_INSERT Users OFF;
GO

-- Sample Incidents
SET IDENTITY_INSERT IncidentReports ON;
INSERT INTO IncidentReports
    (Id, ReportNumber, Title, Description, CategoryId, DepartmentId, ReportedByUserId, AssignedToUserId,
     IncidentDate, ReportedAt, LocationDetails, SeverityLevel, Status)
VALUES
    (1, 'INC-2026-0001', 'Forklift near-miss in aisle 3',
     'Forklift operator failed to yield at intersection. No injuries but near collision with pedestrian.',
     1, 2, 2, 3, '2026-03-10', '2026-03-10T08:30:00Z', 'Warehouse – Aisle 3', 'High', 'Open'),

    (2, 'INC-2026-0002', 'Chemical spill – Lab B',
     'Small solvent spill near workbench. Area secured and cleaned immediately.',
     3, 3, 3, 2, '2026-03-15', '2026-03-15T13:00:00Z', 'Maintenance Lab B', 'Medium', 'Closed'),

    (3, 'INC-2026-0003', 'Unsecured electrical panel',
     'Panel cover found open and unguarded. Immediate lockout/tagout applied.',
     2, 1, 4, 3, '2026-03-20', '2026-03-20T09:15:00Z', 'Production Floor – Zone A', 'Critical', 'InProgress'),

    (4, 'INC-2026-0004', 'Employee bypassed machine guard',
     'Worker removed safety guard to speed up operation. No injury reported.',
     4, 1, 2, 2, '2026-03-22', '2026-03-22T11:45:00Z', 'Production Line 2', 'High', 'Open'),

    (5, 'INC-2026-0005', 'Slippery floor near loading dock',
     'Water leakage created slip hazard. Warning signs placed, maintenance notified.',
     3, 4, 3, NULL, '2026-03-25', '2026-03-25T07:00:00Z', 'Loading Dock – East', 'Low', 'Open');
SET IDENTITY_INSERT IncidentReports OFF;
GO

-- Sample Corrective Actions
INSERT INTO CorrectiveActions (ReportId, ActionTitle, ActionDescription, AssignedToUserId, DueDate, Status, PriorityLevel) VALUES
    (1, 'Install pedestrian crossing signs',   'Add visible floor markings and warning signs at all forklift intersections', 3, '2026-04-05', 'Pending',    'High'),
    (1, 'Conduct forklift safety retraining',  'Mandatory retraining for all forklift operators',                           2, '2026-04-15', 'Pending',    'High'),
    (3, 'Repair and secure electrical panel',  'Replace damaged cover and ensure proper locking mechanism installed',       3, '2026-03-28', 'InProgress', 'Critical'),
    (4, 'Reinstall machine guards',            'Reinstall all safety guards and verify cannot be removed without tools',    3, '2026-03-30', 'Pending',    'Critical'),
    (4, 'Issue safety violation notice',       'Document violation and issue formal notice per company safety policy',      2, '2026-04-01', 'Completed',  'Medium');
GO

PRINT 'SafetyPortal database schema and sample data loaded successfully.';
GO
