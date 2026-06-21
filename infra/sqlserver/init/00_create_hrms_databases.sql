SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

PRINT 'Creating HRMS databases if they do not exist...';

IF DB_ID(N'HRMS_HrCoreDb') IS NULL
BEGIN
    CREATE DATABASE [HRMS_HrCoreDb];
END
GO

IF DB_ID(N'HRMS_AttendanceDb') IS NULL
BEGIN
    CREATE DATABASE [HRMS_AttendanceDb];
END
GO

IF DB_ID(N'HRMS_PayrollReportDb') IS NULL
BEGIN
    CREATE DATABASE [HRMS_PayrollReportDb];
END
GO

/* =========================================================
   HR Core DB
   ========================================================= */

USE [HRMS_HrCoreDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departments
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Departments_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        ParentDepartmentId UNIQUEIDENTIFIER NULL,
        ManagerEmployeeId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Departments PRIMARY KEY (Id),
        CONSTRAINT UQ_Departments_Code UNIQUE (Code),
        CONSTRAINT FK_Departments_ParentDepartment FOREIGN KEY (ParentDepartmentId) REFERENCES dbo.Departments(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.Positions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Positions
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Positions_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Positions_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Positions_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Positions PRIMARY KEY (Id),
        CONSTRAINT UQ_Positions_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Employees_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeCode NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        Phone NVARCHAR(30) NULL,
        Gender NVARCHAR(20) NULL,
        DateOfBirth DATE NULL,
        HireDate DATE NOT NULL,
        DepartmentId UNIQUEIDENTIFIER NOT NULL,
        PositionId UNIQUEIDENTIFIER NOT NULL,
        ManagerEmployeeId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Employees_Status DEFAULT (N'Active'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Employees PRIMARY KEY (Id),
        CONSTRAINT UQ_Employees_EmployeeCode UNIQUE (EmployeeCode),
        CONSTRAINT UQ_Employees_Email UNIQUE (Email),
        CONSTRAINT FK_Employees_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
        CONSTRAINT FK_Employees_Position FOREIGN KEY (PositionId) REFERENCES dbo.Positions(Id),
        CONSTRAINT FK_Employees_Manager FOREIGN KEY (ManagerEmployeeId) REFERENCES dbo.Employees(Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Departments_ManagerEmployee')
BEGIN
    ALTER TABLE dbo.Departments
    ADD CONSTRAINT FK_Departments_ManagerEmployee
    FOREIGN KEY (ManagerEmployeeId) REFERENCES dbo.Employees(Id);
END
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Users_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NULL,
        Email NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        LastLoginAt DATETIME2(0) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT FK_Users_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Roles_Id DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(300) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Roles PRIMARY KEY (Id),
        CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    );
END
GO

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_UserRoles_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.Contracts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Contracts
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Contracts_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        ContractNo NVARCHAR(100) NOT NULL,
        ContractType NVARCHAR(50) NOT NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        BaseSalary DECIMAL(18, 2) NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Contracts_Status DEFAULT (N'Active'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Contracts_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Contracts PRIMARY KEY (Id),
        CONSTRAINT UQ_Contracts_ContractNo UNIQUE (ContractNo),
        CONSTRAINT FK_Contracts_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.EmployeeStatusHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeStatusHistories
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmployeeStatusHistories_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        OldStatus NVARCHAR(30) NULL,
        NewStatus NVARCHAR(30) NOT NULL,
        Reason NVARCHAR(500) NULL,
        ChangedByUserId UNIQUEIDENTIFIER NULL,
        ChangedAt DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeStatusHistories_ChangedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmployeeStatusHistories PRIMARY KEY (Id),
        CONSTRAINT FK_EmployeeStatusHistories_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id),
        CONSTRAINT FK_EmployeeStatusHistories_ChangedByUser FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AuditLogs_Id DEFAULT NEWSEQUENTIALID(),
        ActorUserId UNIQUEIDENTIFIER NULL,
        Action NVARCHAR(100) NOT NULL,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId NVARCHAR(100) NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
        CONSTRAINT FK_AuditLogs_ActorUser FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OutboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_OutboxMessages_Id DEFAULT NEWSEQUENTIALID(),
        EventName NVARCHAR(200) NOT NULL,
        EventVersion INT NOT NULL CONSTRAINT DF_OutboxMessages_EventVersion DEFAULT (1),
        Payload NVARCHAR(MAX) NOT NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        OccurredAt DATETIME2(0) NOT NULL CONSTRAINT DF_OutboxMessages_OccurredAt DEFAULT SYSUTCDATETIME(),
        ProcessedAt DATETIME2(0) NULL,
        RetryCount INT NOT NULL CONSTRAINT DF_OutboxMessages_RetryCount DEFAULT (0),
        ErrorMessage NVARCHAR(1000) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_OutboxMessages_Status DEFAULT (N'Pending'),
        CONSTRAINT PK_OutboxMessages PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Users_EmployeeId_NotNull')
    CREATE UNIQUE INDEX UX_Users_EmployeeId_NotNull ON dbo.Users(EmployeeId) WHERE EmployeeId IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employees_DepartmentId')
    CREATE INDEX IX_Employees_DepartmentId ON dbo.Employees(DepartmentId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employees_PositionId')
    CREATE INDEX IX_Employees_PositionId ON dbo.Employees(PositionId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Contracts_EmployeeId')
    CREATE INDEX IX_Contracts_EmployeeId ON dbo.Contracts(EmployeeId);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Admin')
    INSERT INTO dbo.Roles (Name, Description) VALUES (N'Admin', N'Toan quyen he thong');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'HR')
    INSERT INTO dbo.Roles (Name, Description) VALUES (N'HR', N'Quan ly nhan su');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Manager')
    INSERT INTO dbo.Roles (Name, Description) VALUES (N'Manager', N'Quan ly nhom va duyet nghi');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Employee')
    INSERT INTO dbo.Roles (Name, Description) VALUES (N'Employee', N'Nhan vien');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'PayrollStaff')
    INSERT INTO dbo.Roles (Name, Description) VALUES (N'PayrollStaff', N'Quan ly luong va bao cao');
GO

/* =========================================================
   Attendance DB
   ========================================================= */

USE [HRMS_AttendanceDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.DepartmentProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DepartmentProjections
    (
        DepartmentId UNIQUEIDENTIFIER NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Att_DepartmentProjections_IsActive DEFAULT (1),
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_DepartmentProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Att_DepartmentProjections PRIMARY KEY (DepartmentId),
        CONSTRAINT UQ_Att_DepartmentProjections_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.PositionProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PositionProjections
    (
        PositionId UNIQUEIDENTIFIER NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Att_PositionProjections_IsActive DEFAULT (1),
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_PositionProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Att_PositionProjections PRIMARY KEY (PositionId),
        CONSTRAINT UQ_Att_PositionProjections_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.EmployeeProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeProjections
    (
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        EmployeeCode NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(256) NULL,
        DepartmentId UNIQUEIDENTIFIER NULL,
        PositionId UNIQUEIDENTIFIER NULL,
        ManagerEmployeeId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(30) NOT NULL,
        HireDate DATETIME2(0) NOT NULL CONSTRAINT DF_Att_EmployeeProjections_HireDate DEFAULT SYSUTCDATETIME(),
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_EmployeeProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Att_EmployeeProjections PRIMARY KEY (EmployeeId),
        CONSTRAINT UQ_Att_EmployeeProjections_EmployeeCode UNIQUE (EmployeeCode),
        CONSTRAINT FK_Att_EmployeeProjections_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.DepartmentProjections(DepartmentId),
        CONSTRAINT FK_Att_EmployeeProjections_Position FOREIGN KEY (PositionId) REFERENCES dbo.PositionProjections(PositionId),
        CONSTRAINT FK_Att_EmployeeProjections_Manager FOREIGN KEY (ManagerEmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.Shifts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shifts
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_Shifts_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        StartTime TIME(0) NOT NULL,
        EndTime TIME(0) NOT NULL,
        BreakMinutes INT NOT NULL CONSTRAINT DF_Att_Shifts_BreakMinutes DEFAULT (0),
        IsOvernight BIT NOT NULL CONSTRAINT DF_Att_Shifts_IsOvernight DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_Att_Shifts_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_Shifts_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Att_Shifts PRIMARY KEY (Id),
        CONSTRAINT UQ_Att_Shifts_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.WorkSchedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkSchedules
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_WorkSchedules_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        ShiftId UNIQUEIDENTIFIER NOT NULL,
        WorkDate DATE NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Att_WorkSchedules_Status DEFAULT (N'Planned'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_WorkSchedules_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Att_WorkSchedules PRIMARY KEY (Id),
        CONSTRAINT FK_Att_WorkSchedules_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId),
        CONSTRAINT FK_Att_WorkSchedules_Shift FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.AttendanceRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceRecords
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_AttendanceRecords_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        WorkScheduleId UNIQUEIDENTIFIER NULL,
        ShiftId UNIQUEIDENTIFIER NOT NULL,
        WorkDate DATE NOT NULL,
        CheckInAt DATETIME2(0) NOT NULL,
        CheckOutAt DATETIME2(0) NULL,
        WorkedMinutes INT NOT NULL CONSTRAINT DF_Att_AttendanceRecords_WorkedMinutes DEFAULT (0),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Att_AttendanceRecords_Status DEFAULT (N'CheckedIn'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_AttendanceRecords_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Att_AttendanceRecords PRIMARY KEY (Id),
        CONSTRAINT FK_Att_AttendanceRecords_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId),
        CONSTRAINT FK_Att_AttendanceRecords_WorkSchedule FOREIGN KEY (WorkScheduleId) REFERENCES dbo.WorkSchedules(Id),
        CONSTRAINT FK_Att_AttendanceRecords_Shift FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.LeaveTypes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveTypes
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_LeaveTypes_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        IsPaid BIT NOT NULL CONSTRAINT DF_Att_LeaveTypes_IsPaid DEFAULT (1),
        IsActive BIT NOT NULL CONSTRAINT DF_Att_LeaveTypes_IsActive DEFAULT (1),
        CONSTRAINT PK_Att_LeaveTypes PRIMARY KEY (Id),
        CONSTRAINT UQ_Att_LeaveTypes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.LeaveRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveRequests
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_LeaveRequests_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        LeaveTypeId UNIQUEIDENTIFIER NOT NULL,
        FromDate DATE NOT NULL,
        ToDate DATE NOT NULL,
        TotalDays DECIMAL(5, 2) NOT NULL,
        Reason NVARCHAR(500) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Att_LeaveRequests_Status DEFAULT (N'Pending'),
        ApprovedByEmployeeId UNIQUEIDENTIFIER NULL,
        ApprovedAt DATETIME2(0) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_LeaveRequests_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Att_LeaveRequests PRIMARY KEY (Id),
        CONSTRAINT FK_Att_LeaveRequests_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId),
        CONSTRAINT FK_Att_LeaveRequests_LeaveType FOREIGN KEY (LeaveTypeId) REFERENCES dbo.LeaveTypes(Id),
        CONSTRAINT FK_Att_LeaveRequests_ApprovedByEmployee FOREIGN KEY (ApprovedByEmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.Timesheets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Timesheets
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_Timesheets_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        [Year] INT NOT NULL,
        [Month] INT NOT NULL,
        TotalWorkedMinutes INT NOT NULL CONSTRAINT DF_Att_Timesheets_TotalWorkedMinutes DEFAULT (0),
        PaidLeaveDays DECIMAL(5, 2) NOT NULL CONSTRAINT DF_Att_Timesheets_PaidLeaveDays DEFAULT (0),
        UnpaidLeaveDays DECIMAL(5, 2) NOT NULL CONSTRAINT DF_Att_Timesheets_UnpaidLeaveDays DEFAULT (0),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Att_Timesheets_Status DEFAULT (N'Draft'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_Timesheets_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Att_Timesheets PRIMARY KEY (Id),
        CONSTRAINT FK_Att_Timesheets_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.InboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        EventName NVARCHAR(200) NOT NULL,
        EventVersion INT NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        ReceivedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_InboxMessages_ReceivedAt DEFAULT SYSUTCDATETIME(),
        ProcessedAt DATETIME2(0) NULL,
        RetryCount INT NOT NULL CONSTRAINT DF_Att_InboxMessages_RetryCount DEFAULT (0),
        ErrorMessage NVARCHAR(1000) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Att_InboxMessages_Status DEFAULT (N'Pending'),
        CONSTRAINT PK_Att_InboxMessages PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OutboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_OutboxMessages_Id DEFAULT NEWSEQUENTIALID(),
        EventName NVARCHAR(200) NOT NULL,
        EventVersion INT NOT NULL CONSTRAINT DF_Att_OutboxMessages_EventVersion DEFAULT (1),
        Payload NVARCHAR(MAX) NOT NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        OccurredAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_OutboxMessages_OccurredAt DEFAULT SYSUTCDATETIME(),
        ProcessedAt DATETIME2(0) NULL,
        RetryCount INT NOT NULL CONSTRAINT DF_Att_OutboxMessages_RetryCount DEFAULT (0),
        ErrorMessage NVARCHAR(1000) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Att_OutboxMessages_Status DEFAULT (N'Pending'),
        CONSTRAINT PK_Att_OutboxMessages PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Att_AuditLogs_Id DEFAULT NEWSEQUENTIALID(),
        ActorEmployeeId UNIQUEIDENTIFIER NULL,
        Action NVARCHAR(100) NOT NULL,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId NVARCHAR(100) NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Att_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Att_AuditLogs PRIMARY KEY (Id),
        CONSTRAINT FK_Att_AuditLogs_ActorEmployee FOREIGN KEY (ActorEmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Att_WorkSchedules_EmployeeDateShift')
    CREATE UNIQUE INDEX UX_Att_WorkSchedules_EmployeeDateShift ON dbo.WorkSchedules(EmployeeId, WorkDate, ShiftId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Att_AttendanceRecords_EmployeeWorkDate')
    CREATE INDEX IX_Att_AttendanceRecords_EmployeeWorkDate ON dbo.AttendanceRecords(EmployeeId, WorkDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Att_LeaveRequests_EmployeeStatus')
    CREATE INDEX IX_Att_LeaveRequests_EmployeeStatus ON dbo.LeaveRequests(EmployeeId, Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Att_Timesheets_EmployeeYearMonth')
    CREATE UNIQUE INDEX UX_Att_Timesheets_EmployeeYearMonth ON dbo.Timesheets(EmployeeId, [Year], [Month]);
GO

/* =========================================================
   Payroll & Report DB
   ========================================================= */

USE [HRMS_PayrollReportDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.DepartmentProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DepartmentProjections
    (
        DepartmentId UNIQUEIDENTIFIER NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Pay_DepartmentProjections_IsActive DEFAULT (1),
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_DepartmentProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_DepartmentProjections PRIMARY KEY (DepartmentId),
        CONSTRAINT UQ_Pay_DepartmentProjections_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.PositionProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PositionProjections
    (
        PositionId UNIQUEIDENTIFIER NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Pay_PositionProjections_IsActive DEFAULT (1),
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_PositionProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_PositionProjections PRIMARY KEY (PositionId),
        CONSTRAINT UQ_Pay_PositionProjections_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.EmployeeProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeProjections
    (
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        EmployeeCode NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(256) NULL,
        DepartmentId UNIQUEIDENTIFIER NULL,
        PositionId UNIQUEIDENTIFIER NULL,
        ManagerEmployeeId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(30) NOT NULL,
        HireDate DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_EmployeeProjections_HireDate DEFAULT SYSUTCDATETIME(),
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_EmployeeProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_EmployeeProjections PRIMARY KEY (EmployeeId),
        CONSTRAINT UQ_Pay_EmployeeProjections_EmployeeCode UNIQUE (EmployeeCode),
        CONSTRAINT FK_Pay_EmployeeProjections_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.DepartmentProjections(DepartmentId),
        CONSTRAINT FK_Pay_EmployeeProjections_Position FOREIGN KEY (PositionId) REFERENCES dbo.PositionProjections(PositionId),
        CONSTRAINT FK_Pay_EmployeeProjections_Manager FOREIGN KEY (ManagerEmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.EmployeeSalaryProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeSalaryProjections
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_EmployeeSalaryProjections_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        ContractId UNIQUEIDENTIFIER NOT NULL,
        BaseSalary DECIMAL(18, 2) NOT NULL,
        EffectiveFrom DATE NOT NULL,
        EffectiveTo DATE NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Pay_EmployeeSalaryProjections_Status DEFAULT (N'Active'),
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_EmployeeSalaryProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_EmployeeSalaryProjections PRIMARY KEY (Id),
        CONSTRAINT FK_Pay_EmployeeSalaryProjections_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.AttendanceProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceProjections
    (
        AttendanceRecordId UNIQUEIDENTIFIER NOT NULL,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        WorkDate DATE NOT NULL,
        WorkedMinutes INT NOT NULL,
        Status NVARCHAR(30) NOT NULL,
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_AttendanceProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_AttendanceProjections PRIMARY KEY (AttendanceRecordId),
        CONSTRAINT FK_Pay_AttendanceProjections_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.LeaveProjections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveProjections
    (
        LeaveRequestId UNIQUEIDENTIFIER NOT NULL,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        FromDate DATE NOT NULL,
        ToDate DATE NOT NULL,
        TotalDays DECIMAL(5, 2) NOT NULL,
        IsPaid BIT NOT NULL,
        LastSyncedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_LeaveProjections_LastSyncedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_LeaveProjections PRIMARY KEY (LeaveRequestId),
        CONSTRAINT FK_Pay_LeaveProjections_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.PayrollRules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayrollRules
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_PayrollRules_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        WorkDayHours DECIMAL(5, 2) NOT NULL CONSTRAINT DF_Pay_PayrollRules_WorkDayHours DEFAULT (8),
        PaidLeaveCountsAsWork BIT NOT NULL CONSTRAINT DF_Pay_PayrollRules_PaidLeaveCountsAsWork DEFAULT (1),
        OvertimeRate DECIMAL(8, 2) NOT NULL CONSTRAINT DF_Pay_PayrollRules_OvertimeRate DEFAULT (1.5),
        IsActive BIT NOT NULL CONSTRAINT DF_Pay_PayrollRules_IsActive DEFAULT (1),
        CONSTRAINT PK_Pay_PayrollRules PRIMARY KEY (Id),
        CONSTRAINT UQ_Pay_PayrollRules_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.PayrollPeriods', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayrollPeriods
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_PayrollPeriods_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        FromDate DATE NOT NULL,
        ToDate DATE NOT NULL,
        StandardWorkDays DECIMAL(5, 2) NOT NULL,
        PayrollRuleId UNIQUEIDENTIFIER NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Pay_PayrollPeriods_Status DEFAULT (N'Draft'),
        ClosedAt DATETIME2(0) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_PayrollPeriods_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Pay_PayrollPeriods PRIMARY KEY (Id),
        CONSTRAINT UQ_Pay_PayrollPeriods_Code UNIQUE (Code),
        CONSTRAINT FK_Pay_PayrollPeriods_PayrollRule FOREIGN KEY (PayrollRuleId) REFERENCES dbo.PayrollRules(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.AllowanceTypes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AllowanceTypes
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_AllowanceTypes_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Pay_AllowanceTypes_IsActive DEFAULT (1),
        CONSTRAINT PK_Pay_AllowanceTypes PRIMARY KEY (Id),
        CONSTRAINT UQ_Pay_AllowanceTypes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.DeductionTypes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeductionTypes
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_DeductionTypes_Id DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Pay_DeductionTypes_IsActive DEFAULT (1),
        CONSTRAINT PK_Pay_DeductionTypes PRIMARY KEY (Id),
        CONSTRAINT UQ_Pay_DeductionTypes_Code UNIQUE (Code)
    );
END
GO

IF OBJECT_ID(N'dbo.EmployeeAllowances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeAllowances
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_EmployeeAllowances_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        PayrollPeriodId UNIQUEIDENTIFIER NOT NULL,
        AllowanceTypeId UNIQUEIDENTIFIER NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Note NVARCHAR(500) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_EmployeeAllowances_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_EmployeeAllowances PRIMARY KEY (Id),
        CONSTRAINT FK_Pay_EmployeeAllowances_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId),
        CONSTRAINT FK_Pay_EmployeeAllowances_PayrollPeriod FOREIGN KEY (PayrollPeriodId) REFERENCES dbo.PayrollPeriods(Id),
        CONSTRAINT FK_Pay_EmployeeAllowances_AllowanceType FOREIGN KEY (AllowanceTypeId) REFERENCES dbo.AllowanceTypes(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.EmployeeDeductions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeDeductions
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_EmployeeDeductions_Id DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        PayrollPeriodId UNIQUEIDENTIFIER NOT NULL,
        DeductionTypeId UNIQUEIDENTIFIER NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Note NVARCHAR(500) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_EmployeeDeductions_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_EmployeeDeductions PRIMARY KEY (Id),
        CONSTRAINT FK_Pay_EmployeeDeductions_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId),
        CONSTRAINT FK_Pay_EmployeeDeductions_PayrollPeriod FOREIGN KEY (PayrollPeriodId) REFERENCES dbo.PayrollPeriods(Id),
        CONSTRAINT FK_Pay_EmployeeDeductions_DeductionType FOREIGN KEY (DeductionTypeId) REFERENCES dbo.DeductionTypes(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.Payslips', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payslips
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_Payslips_Id DEFAULT NEWSEQUENTIALID(),
        PayrollPeriodId UNIQUEIDENTIFIER NOT NULL,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        BaseSalary DECIMAL(18, 2) NOT NULL,
        WorkedDays DECIMAL(5, 2) NOT NULL CONSTRAINT DF_Pay_Payslips_WorkedDays DEFAULT (0),
        PaidLeaveDays DECIMAL(5, 2) NOT NULL CONSTRAINT DF_Pay_Payslips_PaidLeaveDays DEFAULT (0),
        GrossSalary DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Pay_Payslips_GrossSalary DEFAULT (0),
        TotalDeduction DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Pay_Payslips_TotalDeduction DEFAULT (0),
        NetSalary DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Pay_Payslips_NetSalary DEFAULT (0),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Pay_Payslips_Status DEFAULT (N'Draft'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_Payslips_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        CONSTRAINT PK_Pay_Payslips PRIMARY KEY (Id),
        CONSTRAINT FK_Pay_Payslips_PayrollPeriod FOREIGN KEY (PayrollPeriodId) REFERENCES dbo.PayrollPeriods(Id),
        CONSTRAINT FK_Pay_Payslips_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF OBJECT_ID(N'dbo.PayslipItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayslipItems
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_PayslipItems_Id DEFAULT NEWSEQUENTIALID(),
        PayslipId UNIQUEIDENTIFIER NOT NULL,
        ItemType NVARCHAR(30) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        SourceType NVARCHAR(50) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_PayslipItems_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_PayslipItems PRIMARY KEY (Id),
        CONSTRAINT FK_Pay_PayslipItems_Payslip FOREIGN KEY (PayslipId) REFERENCES dbo.Payslips(Id)
    );
END
GO

IF OBJECT_ID(N'dbo.InboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL,
        EventName NVARCHAR(200) NOT NULL,
        EventVersion INT NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        ReceivedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_InboxMessages_ReceivedAt DEFAULT SYSUTCDATETIME(),
        ProcessedAt DATETIME2(0) NULL,
        RetryCount INT NOT NULL CONSTRAINT DF_Pay_InboxMessages_RetryCount DEFAULT (0),
        ErrorMessage NVARCHAR(1000) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Pay_InboxMessages_Status DEFAULT (N'Pending'),
        CONSTRAINT PK_Pay_InboxMessages PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OutboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_OutboxMessages_Id DEFAULT NEWSEQUENTIALID(),
        EventName NVARCHAR(200) NOT NULL,
        EventVersion INT NOT NULL CONSTRAINT DF_Pay_OutboxMessages_EventVersion DEFAULT (1),
        Payload NVARCHAR(MAX) NOT NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        OccurredAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_OutboxMessages_OccurredAt DEFAULT SYSUTCDATETIME(),
        ProcessedAt DATETIME2(0) NULL,
        RetryCount INT NOT NULL CONSTRAINT DF_Pay_OutboxMessages_RetryCount DEFAULT (0),
        ErrorMessage NVARCHAR(1000) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Pay_OutboxMessages_Status DEFAULT (N'Pending'),
        CONSTRAINT PK_Pay_OutboxMessages PRIMARY KEY (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Pay_AuditLogs_Id DEFAULT NEWSEQUENTIALID(),
        ActorEmployeeId UNIQUEIDENTIFIER NULL,
        Action NVARCHAR(100) NOT NULL,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId NVARCHAR(100) NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pay_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Pay_AuditLogs PRIMARY KEY (Id),
        CONSTRAINT FK_Pay_AuditLogs_ActorEmployee FOREIGN KEY (ActorEmployeeId) REFERENCES dbo.EmployeeProjections(EmployeeId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pay_AttendanceProjections_EmployeeWorkDate')
    CREATE INDEX IX_Pay_AttendanceProjections_EmployeeWorkDate ON dbo.AttendanceProjections(EmployeeId, WorkDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pay_LeaveProjections_EmployeeDate')
    CREATE INDEX IX_Pay_LeaveProjections_EmployeeDate ON dbo.LeaveProjections(EmployeeId, FromDate, ToDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pay_EmployeeSalaryProjections_EmployeeEffective')
    CREATE INDEX IX_Pay_EmployeeSalaryProjections_EmployeeEffective ON dbo.EmployeeSalaryProjections(EmployeeId, EffectiveFrom, EffectiveTo);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Pay_Payslips_PeriodEmployee')
    CREATE UNIQUE INDEX UX_Pay_Payslips_PeriodEmployee ON dbo.Payslips(PayrollPeriodId, EmployeeId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pay_EmployeeAllowances_PeriodEmployee')
    CREATE INDEX IX_Pay_EmployeeAllowances_PeriodEmployee ON dbo.EmployeeAllowances(PayrollPeriodId, EmployeeId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pay_EmployeeDeductions_PeriodEmployee')
    CREATE INDEX IX_Pay_EmployeeDeductions_PeriodEmployee ON dbo.EmployeeDeductions(PayrollPeriodId, EmployeeId);
GO

PRINT 'HRMS database initialization completed.';
GO
