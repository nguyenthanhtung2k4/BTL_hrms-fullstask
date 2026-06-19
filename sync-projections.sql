-- ============================================================
-- Sync Projections between HRMS Databases
-- Syncs Departments, Positions and updates EmployeeProjections
-- ============================================================

-- 1. Sync Departments to HRMS_AttendanceDb
MERGE HRMS_AttendanceDb.dbo.DepartmentProjections AS target
USING HRMS_HrCoreDb.dbo.Departments AS source
ON (target.DepartmentId = source.Id)
WHEN MATCHED THEN
    UPDATE SET target.Code = source.Code, target.Name = source.Name, target.IsActive = source.IsActive, target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (DepartmentId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 2. Sync Departments to HRMS_PayrollReportDb
MERGE HRMS_PayrollReportDb.dbo.DepartmentProjections AS target
USING HRMS_HrCoreDb.dbo.Departments AS source
ON (target.DepartmentId = source.Id)
WHEN MATCHED THEN
    UPDATE SET target.Code = source.Code, target.Name = source.Name, target.IsActive = source.IsActive, target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (DepartmentId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 3. Sync Positions to HRMS_AttendanceDb
MERGE HRMS_AttendanceDb.dbo.PositionProjections AS target
USING HRMS_HrCoreDb.dbo.Positions AS source
ON (target.PositionId = source.Id)
WHEN MATCHED THEN
    UPDATE SET target.Code = source.Code, target.Name = source.Name, target.IsActive = source.IsActive, target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (PositionId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 4. Sync Positions to HRMS_PayrollReportDb
MERGE HRMS_PayrollReportDb.dbo.PositionProjections AS target
USING HRMS_HrCoreDb.dbo.Positions AS source
ON (target.PositionId = source.Id)
WHEN MATCHED THEN
    UPDATE SET target.Code = source.Code, target.Name = source.Name, target.IsActive = source.IsActive, target.LastSyncedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (PositionId, Code, Name, IsActive, LastSyncedAt)
    VALUES (source.Id, source.Code, source.Name, source.IsActive, GETUTCDATE());

-- 5. Update EmployeeProjections in HRMS_AttendanceDb
UPDATE dest
SET dest.DepartmentId = src.DepartmentId,
    dest.PositionId = src.PositionId
FROM HRMS_AttendanceDb.dbo.EmployeeProjections dest
INNER JOIN HRMS_HrCoreDb.dbo.Employees src ON dest.EmployeeId = src.Id;

-- 6. Update EmployeeProjections in HRMS_PayrollReportDb
UPDATE dest
SET dest.DepartmentId = src.DepartmentId,
    dest.PositionId = src.PositionId
FROM HRMS_PayrollReportDb.dbo.EmployeeProjections dest
INNER JOIN HRMS_HrCoreDb.dbo.Employees src ON dest.EmployeeId = src.Id;

PRINT 'Projections synchronized successfully.';
GO
