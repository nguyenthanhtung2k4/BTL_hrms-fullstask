-- ============================================================
-- Clean All HRMS Databases before seeding
-- Prevents duplicate keys and guarantees a fresh/latest dataset
-- Temporarily drops FKs to bypass SQL Server optimizer bugs.
-- ============================================================

PRINT 'Cleaning HRMS_HrCoreDb...';
USE [HRMS_HrCoreDb];
GO

-- 1. Drop constraints to avoid circular dependencies and query plan errors
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Employee')
    ALTER TABLE dbo.Users DROP CONSTRAINT FK_Users_Employee;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Contracts_Employee')
    ALTER TABLE dbo.Contracts DROP CONSTRAINT FK_Contracts_Employee;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeStatusHistories_Employee')
    ALTER TABLE dbo.EmployeeStatusHistories DROP CONSTRAINT FK_EmployeeStatusHistories_Employee;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Manager')
    ALTER TABLE dbo.Employees DROP CONSTRAINT FK_Employees_Manager;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Department')
    ALTER TABLE dbo.Employees DROP CONSTRAINT FK_Employees_Department;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Position')
    ALTER TABLE dbo.Employees DROP CONSTRAINT FK_Employees_Position;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Departments_ManagerEmployee')
    ALTER TABLE dbo.Departments DROP CONSTRAINT FK_Departments_ManagerEmployee;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Departments_ParentDepartment')
    ALTER TABLE dbo.Departments DROP CONSTRAINT FK_Departments_ParentDepartment;
GO

-- 2. Safely delete data
IF OBJECT_ID('dbo.EmployeeStatusHistories', 'U') IS NOT NULL DELETE FROM dbo.EmployeeStatusHistories;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DELETE FROM dbo.AuditLogs;
IF OBJECT_ID('dbo.OutboxMessages', 'U') IS NOT NULL DELETE FROM dbo.OutboxMessages;
IF OBJECT_ID('dbo.InboxMessages', 'U') IS NOT NULL DELETE FROM dbo.InboxMessages;
IF OBJECT_ID('dbo.Contracts', 'U') IS NOT NULL DELETE FROM dbo.Contracts;
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DELETE FROM dbo.Employees WHERE EmployeeCode <> 'EMP000';
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DELETE FROM dbo.Departments WHERE Code <> 'DEPT001';
IF OBJECT_ID('dbo.Positions', 'U') IS NOT NULL DELETE FROM dbo.Positions WHERE Code <> 'POS001';
GO

-- 3. Recreate constraints
ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id);
ALTER TABLE dbo.Contracts ADD CONSTRAINT FK_Contracts_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id);
ALTER TABLE dbo.EmployeeStatusHistories ADD CONSTRAINT FK_EmployeeStatusHistories_Employee FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id);
ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Manager FOREIGN KEY (ManagerEmployeeId) REFERENCES dbo.Employees(Id);
ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id);
ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Position FOREIGN KEY (PositionId) REFERENCES dbo.Positions(Id);
ALTER TABLE dbo.Departments ADD CONSTRAINT FK_Departments_ManagerEmployee FOREIGN KEY (ManagerEmployeeId) REFERENCES dbo.Employees(Id);
ALTER TABLE dbo.Departments ADD CONSTRAINT FK_Departments_ParentDepartment FOREIGN KEY (ParentDepartmentId) REFERENCES dbo.Departments(Id);
GO

PRINT 'Cleaning HRMS_AttendanceDb...';
USE [HRMS_AttendanceDb];
GO
IF OBJECT_ID('dbo.Timesheets', 'U') IS NOT NULL DELETE FROM dbo.Timesheets;
IF OBJECT_ID('dbo.LeaveRequests', 'U') IS NOT NULL DELETE FROM dbo.LeaveRequests;
IF OBJECT_ID('dbo.AttendanceRecords', 'U') IS NOT NULL DELETE FROM dbo.AttendanceRecords;
IF OBJECT_ID('dbo.WorkSchedules', 'U') IS NOT NULL DELETE FROM dbo.WorkSchedules;
IF OBJECT_ID('dbo.InboxMessages', 'U') IS NOT NULL DELETE FROM dbo.InboxMessages;
IF OBJECT_ID('dbo.OutboxMessages', 'U') IS NOT NULL DELETE FROM dbo.OutboxMessages;
IF OBJECT_ID('dbo.EmployeeProjections', 'U') IS NOT NULL DELETE FROM dbo.EmployeeProjections WHERE EmployeeCode <> 'EMP000';
IF OBJECT_ID('dbo.LeaveTypes', 'U') IS NOT NULL DELETE FROM dbo.LeaveTypes;
IF OBJECT_ID('dbo.Shifts', 'U') IS NOT NULL DELETE FROM dbo.Shifts;
IF OBJECT_ID('dbo.PositionProjections', 'U') IS NOT NULL DELETE FROM dbo.PositionProjections WHERE Code <> 'POS001';
IF OBJECT_ID('dbo.DepartmentProjections', 'U') IS NOT NULL DELETE FROM dbo.DepartmentProjections WHERE Code <> 'DEPT001';
GO

PRINT 'Cleaning HRMS_PayrollReportDb...';
USE [HRMS_PayrollReportDb];
GO
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DELETE FROM dbo.AuditLogs;
IF OBJECT_ID('dbo.InboxMessages', 'U') IS NOT NULL DELETE FROM dbo.InboxMessages;
IF OBJECT_ID('dbo.OutboxMessages', 'U') IS NOT NULL DELETE FROM dbo.OutboxMessages;
IF OBJECT_ID('dbo.PayslipItems', 'U') IS NOT NULL DELETE FROM dbo.PayslipItems;
IF OBJECT_ID('dbo.Payslips', 'U') IS NOT NULL DELETE FROM dbo.Payslips;
IF OBJECT_ID('dbo.EmployeeAllowances', 'U') IS NOT NULL DELETE FROM dbo.EmployeeAllowances;
IF OBJECT_ID('dbo.EmployeeDeductions', 'U') IS NOT NULL DELETE FROM dbo.EmployeeDeductions;
IF OBJECT_ID('dbo.EmployeeSalaryProjections', 'U') IS NOT NULL DELETE FROM dbo.EmployeeSalaryProjections;
IF OBJECT_ID('dbo.AttendanceProjections', 'U') IS NOT NULL DELETE FROM dbo.AttendanceProjections;
IF OBJECT_ID('dbo.LeaveProjections', 'U') IS NOT NULL DELETE FROM dbo.LeaveProjections;
IF OBJECT_ID('dbo.PayrollPeriods', 'U') IS NOT NULL DELETE FROM dbo.PayrollPeriods;
IF OBJECT_ID('dbo.PayrollRules', 'U') IS NOT NULL DELETE FROM dbo.PayrollRules;
IF OBJECT_ID('dbo.AllowanceTypes', 'U') IS NOT NULL DELETE FROM dbo.AllowanceTypes;
IF OBJECT_ID('dbo.DeductionTypes', 'U') IS NOT NULL DELETE FROM dbo.DeductionTypes;
IF OBJECT_ID('dbo.EmployeeProjections', 'U') IS NOT NULL DELETE FROM dbo.EmployeeProjections WHERE EmployeeCode <> 'EMP000';
IF OBJECT_ID('dbo.PositionProjections', 'U') IS NOT NULL DELETE FROM dbo.PositionProjections WHERE Code <> 'POS001';
IF OBJECT_ID('dbo.DepartmentProjections', 'U') IS NOT NULL DELETE FROM dbo.DepartmentProjections WHERE Code <> 'DEPT001';
GO

PRINT 'Database cleanup completed successfully.';
GO
