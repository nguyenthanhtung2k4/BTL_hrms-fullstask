-- ============================================================
-- HRMS Attendance Seed Data - 1 Year (July 2025 to June 2026)
-- Server: localhost,1434  DB: HRMS_AttendanceDb
-- ============================================================

USE HRMS_AttendanceDb;
GO

-- ── 1. Leave Types ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM LeaveTypes WHERE Code='NPN')
    INSERT INTO LeaveTypes (Id, Code, Name, IsPaid, IsActive)
    VALUES
        (NEWID(), 'NPN', N'Nghỉ phép năm',   1, 1),
        (NEWID(), 'NO',  N'Nghỉ ốm',         1, 1),
        (NEWID(), 'NKL', N'Nghỉ không lương', 0, 1),
        (NEWID(), 'NTS', N'Nghỉ thai sản',   1, 1);
PRINT 'LeaveTypes OK';
GO

-- ── 2. Seed Date Tables & Helpers ───────────────────
-- Tạo bảng ngày làm việc 1 năm (01/07/2025 -> 30/06/2026)
IF OBJECT_ID('tempdb..#WD') IS NOT NULL DROP TABLE #WD;
CREATE TABLE #WD (D DATE);

DECLARE @d DATE='2025-07-01';
WHILE @d<='2026-06-30' BEGIN
    -- Loại bỏ thứ Bảy (7) và Chủ Nhật (1)
    IF DATEPART(WEEKDAY, @d) NOT IN (1, 7) 
        INSERT INTO #WD VALUES (@d);
    SET @d = DATEADD(DAY, 1, @d);
END

-- Tạo bảng tháng tiện ích
IF OBJECT_ID('tempdb..#Months') IS NOT NULL DROP TABLE #Months;
CREATE TABLE #Months (
    Yr INT,
    Mn INT,
    StartDate DATE,
    EndDate DATE
);
INSERT INTO #Months VALUES 
(2025, 7, '2025-07-01', '2025-07-31'),
(2025, 8, '2025-08-01', '2025-08-31'),
(2025, 9, '2025-09-01', '2025-09-30'),
(2025, 10, '2025-10-01', '2025-10-31'),
(2025, 11, '2025-11-01', '2025-11-30'),
(2025, 12, '2025-12-01', '2025-12-31'),
(2026, 1, '2026-01-01', '2026-01-31'),
(2026, 2, '2026-02-01', '2026-02-28'),
(2026, 3, '2026-03-01', '2026-03-31'),
(2026, 4, '2026-04-01', '2026-04-30'),
(2026, 5, '2026-05-01', '2026-05-31'),
(2026, 6, '2026-06-01', '2026-06-30');

-- Xóa dữ liệu cũ trong tầm ảnh hưởng
DELETE FROM AttendanceRecords WHERE WorkDate >= '2025-07-01' AND WorkDate <= '2026-06-30';
DELETE FROM LeaveRequests WHERE FromDate >= '2025-07-01' AND ToDate <= '2026-06-30';
DELETE FROM WorkSchedules WHERE WorkDate >= '2025-07-01' AND WorkDate <= '2026-06-30';
DELETE FROM LeaveBalances;
PRINT 'Old records cleared';

-- ── 3. Seed WorkSchedules for all Employees ────────
DECLARE @ShiftId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Shifts WHERE Code = 'CA_HC'); -- Ca Hanh Chinh

INSERT INTO WorkSchedules (Id, EmployeeId, ShiftId, WorkDate, Status, CreatedAt)
SELECT NEWID(), ep.EmployeeId, @ShiftId, w.D, 'Planned', GETUTCDATE()
FROM EmployeeProjections ep
CROSS JOIN #WD w
WHERE ep.EmployeeCode <> 'EMP000';
PRINT 'WorkSchedules seeded';

-- ── 4. Seed LeaveRequests ──────────────────────────
DECLARE @LT_NPN UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM LeaveTypes WHERE Code='NPN');
DECLARE @LT_NO  UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM LeaveTypes WHERE Code='NO');
DECLARE @LT_NKL UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM LeaveTypes WHERE Code='NKL');
DECLARE @LT_NTS UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM LeaveTypes WHERE Code='NTS');
DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'EMP000');

-- Thêm các LeaveRequest cụ thể
-- 1. Thai sản cho NV008 (Lê Thị Thanh Tuyền): 180 ngày từ 01/10/2025 -> 29/03/2026
DECLARE @NV008 UNIQUEIDENTIFIER = (SELECT EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV008');
IF @NV008 IS NOT NULL
    INSERT INTO LeaveRequests (Id, EmployeeId, LeaveTypeId, FromDate, ToDate, TotalDays, Reason, Status, ApprovedByEmployeeId, ApprovedAt, CreatedAt)
    VALUES (NEWID(), @NV008, @LT_NTS, '2025-10-01', '2026-03-29', 180, N'Nghỉ thai sản theo chế độ', 'Approved', @AdminId, '2025-09-25T08:00:00', '2025-09-24T16:00:00');

-- 2. Nghỉ phép năm và nghỉ ốm rải rác
DECLARE @NV003 UNIQUEIDENTIFIER = (SELECT EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV003');
IF @NV003 IS NOT NULL
    INSERT INTO LeaveRequests (Id, EmployeeId, LeaveTypeId, FromDate, ToDate, TotalDays, Reason, Status, ApprovedByEmployeeId, ApprovedAt, CreatedAt)
    VALUES 
        (NEWID(), @NV003, @LT_NPN, '2025-08-11', '2025-08-12', 2, N'Nghỉ phép gia đình', 'Approved', @AdminId, '2025-08-08T09:00:00', '2025-08-07T15:00:00'),
        (NEWID(), @NV003, @LT_NPN, '2026-04-15', '2026-04-15', 1, N'Nghỉ giải quyết việc cá nhân', 'Approved', @AdminId, '2026-04-14T08:00:00', '2026-04-13T10:00:00');

DECLARE @NV004 UNIQUEIDENTIFIER = (SELECT EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV004');
IF @NV004 IS NOT NULL
    INSERT INTO LeaveRequests (Id, EmployeeId, LeaveTypeId, FromDate, ToDate, TotalDays, Reason, Status, ApprovedByEmployeeId, ApprovedAt, CreatedAt)
    VALUES 
        (NEWID(), @NV004, @LT_NO,  '2025-11-03', '2025-11-03', 1, N'Bị sốt xuất huyết nhẹ', 'Approved', @AdminId, '2025-11-03T09:00:00', '2025-11-03T08:30:00'),
        (NEWID(), @NV004, @LT_NO,  '2026-05-12', '2026-05-13', 2, N'Khám sức khỏe định kỳ', 'Approved', @AdminId, '2026-05-10T14:00:00', '2026-05-09T10:00:00');

DECLARE @NV006 UNIQUEIDENTIFIER = (SELECT EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV006');
IF @NV006 IS NOT NULL
    INSERT INTO LeaveRequests (Id, EmployeeId, LeaveTypeId, FromDate, ToDate, TotalDays, Reason, Status, ApprovedByEmployeeId, ApprovedAt, CreatedAt)
    VALUES (NEWID(), @NV006, @LT_NPN, '2025-12-24', '2025-12-31', 6, N'Nghỉ lễ Giáng sinh và cuối năm', 'Approved', @AdminId, '2025-12-20T10:00:00', '2025-12-18T14:00:00');

DECLARE @NV011 UNIQUEIDENTIFIER = (SELECT EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV011');
IF @NV011 IS NOT NULL
    INSERT INTO LeaveRequests (Id, EmployeeId, LeaveTypeId, FromDate, ToDate, TotalDays, Reason, Status, ApprovedByEmployeeId, ApprovedAt, CreatedAt)
    VALUES (NEWID(), @NV011, @LT_NKL, '2026-02-16', '2026-02-18', 3, N'Giải quyết công việc gia đình ở nước ngoài', 'Approved', @AdminId, '2026-02-12T10:00:00', '2026-02-10T09:00:00');

PRINT 'LeaveRequests seeded';

-- ── 5. Seed AttendanceRecords Dynamically ──────────
-- Sử dụng thuật toán băm (HASHBYTES) để tạo dữ liệu giả lập ngẫu nhiên nhưng nhất quán (deterministic)
INSERT INTO AttendanceRecords (Id, EmployeeId, ShiftId, WorkDate, CheckInAt, CheckOutAt, WorkedMinutes, Status, CreatedAt)
SELECT 
    NEWID(),
    ep.EmployeeId,
    @ShiftId,
    w.D,
    -- CheckInAt: 94% đúng giờ (08:00 -> 08:28), 3% đi muộn (08:30 -> 09:10), còn lại nghỉ không phép/vắng
    CASE 
        WHEN (ABS(CHECKSUM(HASHBYTES('SHA2_256', CAST(ep.EmployeeCode AS VARCHAR) + CAST(w.D AS VARCHAR)))) % 100) < 94 
            THEN DATEADD(MINUTE, 8*60 + (ABS(CHECKSUM(HASHBYTES('SHA2_256', CAST(ep.EmployeeCode AS VARCHAR) + CAST(w.D AS VARCHAR) + 'IN'))) % 29), CAST(w.D AS DATETIME))
        ELSE 
            DATEADD(MINUTE, 8*60 + 30 + (ABS(CHECKSUM(HASHBYTES('SHA2_256', CAST(ep.EmployeeCode AS VARCHAR) + CAST(w.D AS VARCHAR) + 'LATE'))) % 41), CAST(w.D AS DATETIME))
    END AS CheckInAt,
    -- CheckOutAt: Từ 17:30 -> 17:45
    DATEADD(MINUTE, 17*60 + 30 + (ABS(CHECKSUM(HASHBYTES('SHA2_256', CAST(ep.EmployeeCode AS VARCHAR) + CAST(w.D AS VARCHAR) + 'OUT'))) % 16), CAST(w.D AS DATETIME)) AS CheckOutAt,
    -- WorkedMinutes: Tính toán dựa trên hiệu số CheckOutAt - CheckInAt - 90 phút nghỉ trưa
    0 AS WorkedMinutes, -- Sẽ cập nhật ngay sau
    -- Status: CheckedOut hoặc Late
    CASE 
        WHEN (ABS(CHECKSUM(HASHBYTES('SHA2_256', CAST(ep.EmployeeCode AS VARCHAR) + CAST(w.D AS VARCHAR)))) % 100) < 94 THEN 'CheckedOut'
        ELSE 'Late'
    END AS Status,
    GETUTCDATE()
FROM EmployeeProjections ep
CROSS JOIN #WD w
WHERE ep.EmployeeCode <> 'EMP000'
  -- Bỏ qua ngày có yêu cầu nghỉ phép được duyệt
  AND NOT EXISTS (
      SELECT 1 FROM LeaveRequests lr
      WHERE lr.EmployeeId = ep.EmployeeId
        AND lr.Status = 'Approved'
        AND w.D >= lr.FromDate
        AND w.D <= lr.ToDate
  )
  -- 3% tỉ lệ nghỉ không phép (vắng mặt đột xuất)
  AND (ABS(CHECKSUM(HASHBYTES('SHA2_256', CAST(ep.EmployeeCode AS VARCHAR) + CAST(w.D AS VARCHAR) + 'ABSENT'))) % 100) >= 3;

-- Cập nhật số phút làm việc thực tế (WorkedMinutes = CheckOutAt - CheckInAt - 90 phút nghỉ trưa)
UPDATE AttendanceRecords
SET WorkedMinutes = DATEDIFF(MINUTE, CheckInAt, CheckOutAt) - 90
WHERE WorkDate >= '2025-07-01' AND WorkDate <= '2026-06-30';

PRINT 'AttendanceRecords seeded and updated';

-- Cập nhật liên kết WorkScheduleId trong AttendanceRecords
UPDATE ar
SET ar.WorkScheduleId = ws.Id
FROM AttendanceRecords ar
JOIN WorkSchedules ws ON ar.EmployeeId = ws.EmployeeId AND ar.WorkDate = ws.WorkDate
WHERE ar.WorkDate >= '2025-07-01' AND ar.WorkDate <= '2026-06-30';
PRINT 'AttendanceRecords linked to WorkSchedules';

-- ── 6. Seed Timesheets Dynamically ─────────────────
DELETE FROM Timesheets WHERE Year >= 2025 AND Year <= 2026;

INSERT INTO Timesheets (Id, EmployeeId, Year, Month, TotalWorkedMinutes, PaidLeaveDays, UnpaidLeaveDays, Status, CreatedAt)
SELECT
    NEWID(),
    ep.EmployeeId,
    m.Yr,
    m.Mn,
    ISNULL(SUM(ar.WorkedMinutes), 0) AS TotalWorkedMinutes,
    -- Số ngày nghỉ hưởng lương trong tháng
    ISNULL((
        SELECT COUNT(*)
        FROM WorkSchedules ws
        JOIN LeaveRequests lr ON ws.EmployeeId = lr.EmployeeId
        JOIN LeaveTypes lt ON lr.LeaveTypeId = lt.Id
        WHERE ws.EmployeeId = ep.EmployeeId
          AND ws.WorkDate >= m.StartDate AND ws.WorkDate <= m.EndDate
          AND ws.WorkDate >= lr.FromDate AND ws.WorkDate <= lr.ToDate
          AND lr.Status = 'Approved'
          AND lt.IsPaid = 1
          AND NOT EXISTS (SELECT 1 FROM AttendanceRecords ar2 WHERE ar2.EmployeeId = ep.EmployeeId AND ar2.WorkDate = ws.WorkDate)
    ), 0) AS PaidLeaveDays,
    -- Số ngày nghỉ không hưởng lương trong tháng
    ISNULL((
        SELECT COUNT(*)
        FROM WorkSchedules ws
        JOIN LeaveRequests lr ON ws.EmployeeId = lr.EmployeeId
        JOIN LeaveTypes lt ON lr.LeaveTypeId = lt.Id
        WHERE ws.EmployeeId = ep.EmployeeId
          AND ws.WorkDate >= m.StartDate AND ws.WorkDate <= m.EndDate
          AND ws.WorkDate >= lr.FromDate AND ws.WorkDate <= lr.ToDate
          AND lr.Status = 'Approved'
          AND lt.IsPaid = 0
          AND NOT EXISTS (SELECT 1 FROM AttendanceRecords ar2 WHERE ar2.EmployeeId = ep.EmployeeId AND ar2.WorkDate = ws.WorkDate)
    ), 0) AS UnpaidLeaveDays,
    'Calculated' AS Status,
    GETUTCDATE()
FROM EmployeeProjections ep
CROSS JOIN #Months m
LEFT JOIN AttendanceRecords ar ON ar.EmployeeId = ep.EmployeeId
                             AND YEAR(ar.WorkDate) = m.Yr
                             AND MONTH(ar.WorkDate) = m.Mn
WHERE ep.EmployeeCode <> 'EMP000'
GROUP BY ep.EmployeeId, m.Yr, m.Mn, m.StartDate, m.EndDate;

PRINT 'Timesheets calculated and seeded';

-- ── 7. Seed User Accounts for Employees ──
USE HRMS_HrCoreDb;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT 'Seeding user accounts for all employees...';

DECLARE @PassHash NVARCHAR(500) = N'$2a$11$9vOHuqSLBAb/oVElrzioROCnVwCYEimTFOCAhoWsyt1NVoqRD5imO'; -- User123!

IF OBJECT_ID('tempdb..#TempUserSeed') IS NOT NULL DROP TABLE #TempUserSeed;
CREATE TABLE #TempUserSeed (
    EmpCode NVARCHAR(50),
    Email NVARCHAR(256),
    RoleName NVARCHAR(100)
);

-- Thêm tài khoản cho toàn bộ 150 nhân viên từ Employees
INSERT INTO #TempUserSeed (EmpCode, Email, RoleName)
SELECT 
    ep.EmployeeCode,
    ep.Email,
    CASE 
        -- Nếu là Trưởng phòng/Giám đốc thì cấp quyền Manager
        WHEN ep.PositionId IN (SELECT Id FROM dbo.Positions WHERE Code IN ('GD', 'TP')) THEN 'Manager'
        ELSE 'Employee'
    END
FROM dbo.Employees ep
WHERE ep.EmployeeCode <> 'EMP000';

-- Thêm vai trò HR cho bất kỳ nhân sự thuộc phòng HR có vị trí Trưởng phòng
INSERT INTO #TempUserSeed (EmpCode, Email, RoleName)
SELECT 
    ep.EmployeeCode,
    ep.Email,
    'HR'
FROM dbo.Employees ep
WHERE ep.DepartmentId IN (SELECT Id FROM dbo.Departments WHERE Code = 'HR')
  AND ep.PositionId IN (SELECT Id FROM dbo.Positions WHERE Code = 'TP')
  AND ep.EmployeeCode <> 'EMP000';


DECLARE @EmpCode_Cursor NVARCHAR(50), @Email_Cursor NVARCHAR(256), @RoleName_Cursor NVARCHAR(100);
DECLARE @EmpId_Cursor UNIQUEIDENTIFIER, @RoleId_Cursor UNIQUEIDENTIFIER, @UserId_Cursor UNIQUEIDENTIFIER;

DECLARE user_cursor CURSOR FOR 
SELECT EmpCode, Email, RoleName FROM #TempUserSeed;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @EmpCode_Cursor, @Email_Cursor, @RoleName_Cursor;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @EmpId_Cursor = (SELECT Id FROM dbo.Employees WHERE EmployeeCode = @EmpCode_Cursor);
    SET @RoleId_Cursor = (SELECT Id FROM dbo.Roles WHERE Name = @RoleName_Cursor);
    
    IF @EmpId_Cursor IS NOT NULL AND @RoleId_Cursor IS NOT NULL
    BEGIN
        SET @UserId_Cursor = (SELECT Id FROM dbo.Users WHERE EmployeeId = @EmpId_Cursor);
        IF @UserId_Cursor IS NULL
        BEGIN
            SET @UserId_Cursor = NEWID();
            INSERT INTO dbo.Users (Id, EmployeeId, Email, PasswordHash, IsActive, CreatedAt)
            VALUES (@UserId_Cursor, @EmpId_Cursor, @Email_Cursor, @PassHash, 1, GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId_Cursor AND RoleId = @RoleId_Cursor)
        BEGIN
            INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
            VALUES (@UserId_Cursor, @RoleId_Cursor, GETUTCDATE());
        END
    END
    
    FETCH NEXT FROM user_cursor INTO @EmpCode_Cursor, @Email_Cursor, @RoleName_Cursor;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;
DROP TABLE #TempUserSeed;
PRINT 'User accounts seeded successfully.';

USE HRMS_AttendanceDb;
GO

-- Cleanup Temp Tables
IF OBJECT_ID('tempdb..#WD') IS NOT NULL DROP TABLE #WD;
IF OBJECT_ID('tempdb..#Months') IS NOT NULL DROP TABLE #Months;

-- ── Summary ──────────────────────────────────────────────
SELECT 'AttendanceRecords' AS [Table], COUNT(*) AS [Count] FROM AttendanceRecords WHERE WorkDate>='2025-07-01'
UNION ALL SELECT 'LeaveRequests',COUNT(*) FROM LeaveRequests WHERE FromDate>='2025-07-01'
UNION ALL SELECT 'Timesheets',COUNT(*) FROM Timesheets WHERE Year >= 2025
UNION ALL SELECT 'WorkSchedules',COUNT(*) FROM WorkSchedules WHERE WorkDate>='2025-07-01'
UNION ALL SELECT 'EmployeeProjections',COUNT(*) FROM EmployeeProjections;
GO
