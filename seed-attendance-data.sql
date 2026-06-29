-- ============================================================
-- HRMS Attendance Seed Data - May, June, July 2026
-- Server: localhost,1434  DB: HRMS_AttendanceDb
-- ============================================================

USE HRMS_AttendanceDb;
GO

-- ── 1. Leave Types ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM LeaveTypes WHERE Code='NPN')
    INSERT INTO LeaveTypes (Id, Code, Name, IsPaid, IsActive)
    VALUES
        (NEWID(),'NPN','Nghi phep nam',  1, 1),
        (NEWID(),'NO', 'Nghi om',        1, 1),
        (NEWID(),'NKL','Nghi khong luong',0,1),
        (NEWID(),'NTS','Nghi thai san',  1, 1);
PRINT 'LeaveTypes OK';
GO

-- ── 2. Seed WorkSchedules & AttendanceRecords ─────────────
DECLARE @ShiftId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Shifts WHERE Code = 'CA_HC'); -- Ca Hanh Chinh
DECLARE @NV002 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV002');
DECLARE @NV003 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV003');
DECLARE @NV004 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV004');
DECLARE @NV005 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV005');
DECLARE @NV006 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV006');
DECLARE @NV007 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV007');
DECLARE @NV008 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV008');
DECLARE @NV009 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV009');

-- Tạo bảng ngày làm việc T5, T6, T7/2026
IF OBJECT_ID('tempdb..#WD') IS NOT NULL DROP TABLE #WD;
CREATE TABLE #WD (D DATE);
DECLARE @d DATE='2026-05-01';
WHILE @d<='2026-07-31' BEGIN
    IF DATEPART(WEEKDAY,@d) NOT IN(1,7) INSERT INTO #WD VALUES(@d);
    SET @d=DATEADD(DAY,1,@d);
END

-- Xóa data cũ
DELETE FROM AttendanceRecords WHERE WorkDate>='2026-05-01' AND WorkDate<='2026-07-31';
DELETE FROM WorkSchedules WHERE WorkDate>='2026-05-01' AND WorkDate<='2026-07-31';

-- Gieo dữ liệu WorkSchedules cho tất cả nhân viên
IF OBJECT_ID('tempdb..#WS_Emps') IS NOT NULL DROP TABLE #WS_Emps;
CREATE TABLE #WS_Emps (EmpId UNIQUEIDENTIFIER);
INSERT INTO #WS_Emps VALUES (@NV002),(@NV003),(@NV004),(@NV005),(@NV006),(@NV007),(@NV008),(@NV009);

INSERT INTO WorkSchedules (Id, EmployeeId, ShiftId, WorkDate, Status, CreatedAt)
SELECT NEWID(), e.EmpId, @ShiftId, w.D, 'Planned', GETUTCDATE()
FROM #WS_Emps e
CROSS JOIN #WD w;

DROP TABLE #WS_Emps;
PRINT 'WorkSchedules inserted';

-- NV002: Đi đầy đủ, đi muộn ngày 15/5, 16/6, 17/7
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV002,@ShiftId,D,
    DATEADD(MINUTE, CASE WHEN D IN ('2026-05-15', '2026-06-16', '2026-07-17') THEN 8*60+55 ELSE 8*60+28 END, CAST(D AS DATETIME)),
    DATEADD(MINUTE, 17*60+32, CAST(D AS DATETIME)),
    CASE WHEN D IN ('2026-05-15', '2026-06-16', '2026-07-17') THEN 457 ELSE 484 END,
    CASE WHEN D IN ('2026-05-15', '2026-06-16', '2026-07-17') THEN 'Late' ELSE 'CheckedOut' END,
    GETUTCDATE()
FROM #WD;

-- NV003: Nghỉ phép ngày 18/5, 19/6, 20/7
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV003,@ShiftId,D,
    DATEADD(MINUTE,8*60+25,CAST(D AS DATETIME)),DATEADD(MINUTE,17*60+35,CAST(D AS DATETIME)),490,'CheckedOut',GETUTCDATE()
FROM #WD WHERE D NOT IN ('2026-05-18', '2026-06-19', '2026-07-20');

-- NV004: Nghỉ ốm 20-21/5, 23-24/6, 22-23/7
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV004,@ShiftId,D,
    DATEADD(MINUTE,8*60+30,CAST(D AS DATETIME)),DATEADD(MINUTE,17*60+30,CAST(D AS DATETIME)),480,'CheckedOut',GETUTCDATE()
FROM #WD WHERE D NOT IN ('2026-05-20','2026-05-21','2026-06-23','2026-06-24','2026-07-22','2026-07-23');

-- NV005: 100% đầy đủ
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV005,@ShiftId,D,
    DATEADD(MINUTE,8*60+20,CAST(D AS DATETIME)),DATEADD(MINUTE,17*60+40,CAST(D AS DATETIME)),500,'CheckedOut',GETUTCDATE()
FROM #WD;

-- NV006: 100% đầy đủ
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV006,@ShiftId,D,
    DATEADD(MINUTE,8*60+29,CAST(D AS DATETIME)),DATEADD(MINUTE,17*60+31,CAST(D AS DATETIME)),482,'CheckedOut',GETUTCDATE()
FROM #WD;

-- NV007: Nghỉ không lương 11-13/5, 9-11/6, 14-16/7
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV007,@ShiftId,D,
    DATEADD(MINUTE,8*60+35,CAST(D AS DATETIME)),DATEADD(MINUTE,17*60+28,CAST(D AS DATETIME)),473,'CheckedOut',GETUTCDATE()
FROM #WD WHERE D NOT IN ('2026-05-11','2026-05-12','2026-05-13','2026-06-09','2026-06-10','2026-06-11','2026-07-14','2026-07-15','2026-07-16');

-- NV008: 100% đầy đủ
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV008,@ShiftId,D,
    DATEADD(MINUTE,8*60+27,CAST(D AS DATETIME)),DATEADD(MINUTE,17*60+33,CAST(D AS DATETIME)),486,'CheckedOut',GETUTCDATE()
FROM #WD;

-- NV009: 100% đầy đủ với OT
INSERT INTO AttendanceRecords(Id,EmployeeId,ShiftId,WorkDate,CheckInAt,CheckOutAt,WorkedMinutes,Status,CreatedAt)
SELECT NEWID(),@NV009,@ShiftId,D,
    DATEADD(MINUTE,8*60+15,CAST(D AS DATETIME)),DATEADD(MINUTE,18*60+00,CAST(D AS DATETIME)),525,'CheckedOut',GETUTCDATE()
FROM #WD;

DECLARE @attCount INT = (SELECT COUNT(*) FROM AttendanceRecords WHERE WorkDate>='2026-05-01');
PRINT 'AttendanceRecords inserted: ' + CAST(@attCount AS VARCHAR);

-- ── 3. Insert LeaveRequests ───────────────────────────────
DECLARE @LT_NPN UNIQUEIDENTIFIER=(SELECT TOP 1 Id FROM LeaveTypes WHERE Code='NPN');
DECLARE @LT_NO  UNIQUEIDENTIFIER=(SELECT TOP 1 Id FROM LeaveTypes WHERE Code='NO');
DECLARE @LT_NKL UNIQUEIDENTIFIER=(SELECT TOP 1 Id FROM LeaveTypes WHERE Code='NKL');
DECLARE @AdminId UNIQUEIDENTIFIER=(SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'EMP000');

DELETE FROM LeaveRequests WHERE FromDate>='2026-05-01' AND ToDate<='2026-07-31';

INSERT INTO LeaveRequests(Id,EmployeeId,LeaveTypeId,FromDate,ToDate,TotalDays,Reason,Status,ApprovedByEmployeeId,ApprovedAt,CreatedAt)
VALUES
-- May 2026
(NEWID(),@NV003,@LT_NPN,'2026-05-18','2026-05-18',1,'Nghi phep ca nhan','Approved',@AdminId,'2026-05-15T08:00:00','2026-05-14T16:00:00'),
(NEWID(),@NV004,@LT_NO, '2026-05-20','2026-05-21',2,'Bi om sot','Approved',@AdminId,'2026-05-20T09:00:00','2026-05-20T08:30:00'),
(NEWID(),@NV007,@LT_NKL,'2026-05-11','2026-05-13',3,'Viec gia dinh gap','Approved',@AdminId,'2026-05-10T10:00:00','2026-05-09T15:00:00'),
-- June 2026
(NEWID(),@NV003,@LT_NPN,'2026-06-19','2026-06-19',1,'Nghi phep ca nhan','Approved',@AdminId,'2026-06-15T08:00:00','2026-06-14T16:00:00'),
(NEWID(),@NV004,@LT_NO, '2026-06-23','2026-06-24',2,'Bi cam sot','Approved',@AdminId,'2026-06-23T09:00:00','2026-06-23T08:30:00'),
(NEWID(),@NV007,@LT_NKL,'2026-06-09','2026-06-11',3,'Viec gia dinh gap','Approved',@AdminId,'2026-06-08T10:00:00','2026-06-07T15:00:00'),
(NEWID(),@NV005,@LT_NPN,'2026-06-30','2026-06-30',1,'Nghi phep cuoi thang','Pending',NULL,NULL,'2026-06-27T09:00:00'),
(NEWID(),@NV002,@LT_NPN,'2026-06-16','2026-06-17',2,'Xin nghi phep tuan','Rejected',@AdminId,'2026-06-14T11:00:00','2026-06-13T17:00:00'),
-- July 2026
(NEWID(),@NV003,@LT_NPN,'2026-07-20','2026-07-20',1,'Nghi phep ca nhan','Approved',@AdminId,'2026-07-15T08:00:00','2026-07-14T16:00:00'),
(NEWID(),@NV004,@LT_NO, '2026-07-22','2026-07-23',2,'Bi om sot','Approved',@AdminId,'2026-07-22T09:00:00','2026-07-22T08:30:00'),
(NEWID(),@NV007,@LT_NKL,'2026-07-14','2026-07-16',3,'Viec gia dinh gap','Approved',@AdminId,'2026-07-13T10:00:00','2026-07-12T15:00:00');

PRINT 'LeaveRequests inserted';

-- ── 4. Insert Timesheets ─────────────────────────────────
DELETE FROM Timesheets WHERE Year=2026 AND Month IN (5, 6, 7);

-- May 2026 (21 work days)
INSERT INTO Timesheets(Id,EmployeeId,Year,Month,TotalWorkedMinutes,PaidLeaveDays,UnpaidLeaveDays,Status,CreatedAt)
VALUES
(NEWID(),@NV002,2026,5,20*484+457, 0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV003,2026,5,20*490,     1.0, 0,'Calculated',GETUTCDATE()),
(NEWID(),@NV004,2026,5,19*480,     2.0, 0,'Calculated',GETUTCDATE()),
(NEWID(),@NV005,2026,5,21*500,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV006,2026,5,21*482,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV007,2026,5,18*473,     0,   3,'Calculated',GETUTCDATE()),
(NEWID(),@NV008,2026,5,21*486,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV009,2026,5,21*525,     0,   0,'Calculated',GETUTCDATE());

-- June 2026 (22 work days)
INSERT INTO Timesheets(Id,EmployeeId,Year,Month,TotalWorkedMinutes,PaidLeaveDays,UnpaidLeaveDays,Status,CreatedAt)
VALUES
(NEWID(),@NV002,2026,6,21*484+457, 0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV003,2026,6,21*490,     1.0, 0,'Calculated',GETUTCDATE()),
(NEWID(),@NV004,2026,6,20*480,     2.0, 0,'Calculated',GETUTCDATE()),
(NEWID(),@NV005,2026,6,22*500,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV006,2026,6,22*482,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV007,2026,6,19*473,     0,   3,'Calculated',GETUTCDATE()),
(NEWID(),@NV008,2026,6,22*486,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV009,2026,6,22*525,     0,   0,'Calculated',GETUTCDATE());

-- July 2026 (23 work days)
INSERT INTO Timesheets(Id,EmployeeId,Year,Month,TotalWorkedMinutes,PaidLeaveDays,UnpaidLeaveDays,Status,CreatedAt)
VALUES
(NEWID(),@NV002,2026,7,22*484+457, 0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV003,2026,7,22*490,     1.0, 0,'Calculated',GETUTCDATE()),
(NEWID(),@NV004,2026,7,21*480,     2.0, 0,'Calculated',GETUTCDATE()),
(NEWID(),@NV005,2026,7,23*500,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV006,2026,7,23*482,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV007,2026,7,20*473,     0,   3,'Calculated',GETUTCDATE()),
(NEWID(),@NV008,2026,7,23*486,     0,   0,'Calculated',GETUTCDATE()),
(NEWID(),@NV009,2026,7,23*525,     0,   0,'Calculated',GETUTCDATE());

PRINT 'Timesheets inserted';

-- Cập nhật liên kết WorkScheduleId trong AttendanceRecords
UPDATE ar
SET ar.WorkScheduleId = ws.Id
FROM AttendanceRecords ar
JOIN WorkSchedules ws ON ar.EmployeeId = ws.EmployeeId AND ar.WorkDate = ws.WorkDate
WHERE ar.WorkDate >= '2026-05-01' AND ar.WorkDate <= '2026-07-31';
PRINT 'AttendanceRecords linked to WorkSchedules';

DROP TABLE #WD;

-- ── 5. Gieo dữ liệu Inbox/Outbox trong AttendanceDb ─────────────────
DELETE FROM OutboxMessages;
DELETE FROM InboxMessages;
INSERT INTO OutboxMessages (Id, EventName, EventVersion, Payload, CorrelationId, OccurredAt, ProcessedAt, RetryCount, Status)
VALUES (NEWID(), 'TimesheetCalculatedEvent', 1, '{"PeriodId":"PERIOD_2026_06"}', NEWID(), GETUTCDATE(), GETUTCDATE(), 0, 'Processed');
INSERT INTO InboxMessages (Id, EventName, EventVersion, Payload, ReceivedAt, ProcessedAt, RetryCount, Status)
VALUES (NEWID(), 'EmployeeCreatedEvent', 1, '{"EmployeeId":"NV002"}', GETUTCDATE(), GETUTCDATE(), 0, 'Processed');

-- ── 6. Gieo dữ liệu AuditLogs, StatusHistories, Outbox trong HR DB ──
USE HRMS_HrCoreDb;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
DELETE FROM dbo.AuditLogs;
DELETE FROM dbo.EmployeeStatusHistories;
DELETE FROM dbo.OutboxMessages;

DECLARE @AdminId_HR UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Employees WHERE EmployeeCode = 'EMP000');
DECLARE @NV002_HR UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Employees WHERE EmployeeCode = 'NV002');
DECLARE @NV003_HR UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Employees WHERE EmployeeCode = 'NV003');

INSERT INTO dbo.AuditLogs (Id, ActorUserId, Action, EntityName, EntityId, OldValues, NewValues, CreatedAt)
VALUES (NEWID(), (SELECT TOP 1 Id FROM dbo.Users), 'CREATE', 'Employee', 'NV002', NULL, '{"EmployeeCode":"NV002","FullName":"Nguyen Thi Lan Anh"}', GETUTCDATE());
INSERT INTO dbo.EmployeeStatusHistories (Id, EmployeeId, OldStatus, NewStatus, Reason, ChangedByUserId, ChangedAt)
VALUES (NEWID(), @NV002_HR, 'Draft', 'Active', N'Nhân viên mới tuyển dụng', (SELECT TOP 1 Id FROM dbo.Users), GETUTCDATE());
INSERT INTO dbo.OutboxMessages (Id, EventName, EventVersion, Payload, CorrelationId, OccurredAt, ProcessedAt, RetryCount, Status)
VALUES (NEWID(), 'EmployeeCreatedEvent', 1, '{"EmployeeId":"NV002"}', NEWID(), GETUTCDATE(), GETUTCDATE(), 0, 'Processed');

PRINT 'HR Core Logs and Histories: seeded successfully';

-- ── 7. Seed User Accounts for Employees ──
PRINT 'Seeding user accounts for employees...';

DECLARE @PassHash NVARCHAR(500) = N'$2a$11$9vOHuqSLBAb/oVElrzioROCnVwCYEimTFOCAhoWsyt1NVoqRD5imO'; -- User123!

IF OBJECT_ID('tempdb..#TempUserSeed') IS NOT NULL DROP TABLE #TempUserSeed;
CREATE TABLE #TempUserSeed (
    EmpCode NVARCHAR(50),
    Email NVARCHAR(256),
    RoleName NVARCHAR(100)
);

INSERT INTO #TempUserSeed VALUES
('NV002', 'lan.anh@hrms.com', 'HR'),
('NV002', 'lan.anh@hrms.com', 'Manager'),
('NV003', 'minh.hoang@hrms.com', 'Employee'),
('NV004', 'thanh.tuyen@hrms.com', 'Employee'),
('NV005', 'van.duc@hrms.com', 'Employee'),
('NV006', 'thi.mai@hrms.com', 'Employee'),
('NV007', 'quoc.bao@hrms.com', 'Employee'),
('NV008', 'thi.huong@hrms.com', 'Employee'),
('NV009', 'huu.nghia@hrms.com', 'Manager');

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

-- ── Summary ──────────────────────────────────────────────
SELECT 'AttendanceRecords' AS [Table], COUNT(*) AS [Count] FROM AttendanceRecords WHERE WorkDate>='2026-05-01'
UNION ALL SELECT 'LeaveRequests',COUNT(*) FROM LeaveRequests WHERE FromDate>='2026-05-01'
UNION ALL SELECT 'Timesheets',COUNT(*) FROM Timesheets WHERE Year=2026 AND Month IN (5,6,7)
UNION ALL SELECT 'WorkSchedules',COUNT(*) FROM WorkSchedules WHERE WorkDate>='2026-05-01'
UNION ALL SELECT 'EmployeeProjections',COUNT(*) FROM EmployeeProjections;
GO
