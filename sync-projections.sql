-- ============================================================
-- Sync Projections between HRMS Databases
-- Pulls the latest master data from HR Core database (HRMS_HrCoreDb)
-- to Attendance (HRMS_AttendanceDb) and Payroll (HRMS_PayrollReportDb)
-- ============================================================

-- 1. Sync DepartmentProjections to HRMS_AttendanceDb
MERGE HRMS_AttendanceDb.dbo.DepartmentProjections AS target
USING HRMS_HrCoreDb.dbo.Departments AS source
ON (target.DepartmentId = source.Id)
WHEN MATCHED THEN
    UPDATE SET 
        target.Code = source.Code, 
        target.Name = source.Name, 
        target.IsActive = source.IsActive, 
        target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (DepartmentId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 2. Sync DepartmentProjections to HRMS_PayrollReportDb
MERGE HRMS_PayrollReportDb.dbo.DepartmentProjections AS target
USING HRMS_HrCoreDb.dbo.Departments AS source
ON (target.DepartmentId = source.Id)
WHEN MATCHED THEN
    UPDATE SET 
        target.Code = source.Code, 
        target.Name = source.Name, 
        target.IsActive = source.IsActive, 
        target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (DepartmentId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 3. Sync PositionProjections to HRMS_AttendanceDb
MERGE HRMS_AttendanceDb.dbo.PositionProjections AS target
USING HRMS_HrCoreDb.dbo.Positions AS source
ON (target.PositionId = source.Id)
WHEN MATCHED THEN
    UPDATE SET 
        target.Code = source.Code, 
        target.Name = source.Name, 
        target.IsActive = source.IsActive, 
        target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (PositionId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 4. Sync PositionProjections to HRMS_PayrollReportDb
MERGE HRMS_PayrollReportDb.dbo.PositionProjections AS target
USING HRMS_HrCoreDb.dbo.Positions AS source
ON (target.PositionId = source.Id)
WHEN MATCHED THEN
    UPDATE SET 
        target.Code = source.Code, 
        target.Name = source.Name, 
        target.IsActive = source.IsActive, 
        target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (PositionId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 5. Sync EmployeeProjections to HRMS_AttendanceDb
MERGE HRMS_AttendanceDb.dbo.EmployeeProjections AS target
USING HRMS_HrCoreDb.dbo.Employees AS source
ON (target.EmployeeId = source.Id)
WHEN MATCHED THEN
    UPDATE SET 
        target.EmployeeCode = source.EmployeeCode,
        target.FullName = source.FullName,
        target.Email = source.Email,
        target.DepartmentId = source.DepartmentId,
        target.PositionId = source.PositionId,
        target.ManagerEmployeeId = source.ManagerEmployeeId,
        target.Status = source.Status,
        target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (EmployeeId, EmployeeCode, FullName, Email, DepartmentId, PositionId, ManagerEmployeeId, Status, LastSyncedAt)
    VALUES (source.Id, source.EmployeeCode, source.FullName, source.Email, source.DepartmentId, source.PositionId, source.ManagerEmployeeId, source.Status, GETUTCDATE());

-- 6. Sync EmployeeProjections to HRMS_PayrollReportDb
MERGE HRMS_PayrollReportDb.dbo.EmployeeProjections AS target
USING HRMS_HrCoreDb.dbo.Employees AS source
ON (target.EmployeeId = source.Id)
WHEN MATCHED THEN
    UPDATE SET 
        target.EmployeeCode = source.EmployeeCode,
        target.FullName = source.FullName,
        target.Email = source.Email,
        target.DepartmentId = source.DepartmentId,
        target.PositionId = source.PositionId,
        target.ManagerEmployeeId = source.ManagerEmployeeId,
        target.Status = source.Status,
        target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (EmployeeId, EmployeeCode, FullName, Email, DepartmentId, PositionId, ManagerEmployeeId, Status, LastSyncedAt)
    VALUES (source.Id, source.EmployeeCode, source.FullName, source.Email, source.DepartmentId, source.PositionId, source.ManagerEmployeeId, source.Status, GETUTCDATE());

-- 7. Sync EmployeeSalaryProjections to HRMS_PayrollReportDb (Contracts -> SalaryProjections)
MERGE HRMS_PayrollReportDb.dbo.EmployeeSalaryProjections AS target
USING HRMS_HrCoreDb.dbo.Contracts AS source
ON (target.ContractId = source.Id)
WHEN MATCHED THEN
    UPDATE SET 
        target.EmployeeId = source.EmployeeId,
        target.BaseSalary = source.BaseSalary,
        target.EffectiveFrom = source.StartDate,
        target.EffectiveTo = source.EndDate,
        target.Status = source.Status,
        target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (Id, EmployeeId, ContractId, BaseSalary, EffectiveFrom, EffectiveTo, Status, LastSyncedAt)
    VALUES (NEWID(), source.EmployeeId, source.Id, source.BaseSalary, source.StartDate, source.EndDate, source.Status, GETUTCDATE());

PRINT 'All projections synchronized with the latest data from HR Core successfully.';
GO
