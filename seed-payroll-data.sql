-- ============================================================
-- HRMS Payroll DB - Full Data Seed for Payroll Calculation
-- Database: HRMS_PayrollReportDb (port 1434)
-- ============================================================

USE HRMS_PayrollReportDb;
GO

-- Employees đã sync ở script trước. Kiểm tra:
-- SELECT COUNT(*) FROM EmployeeProjections; -- should = 9
GO

-- ── 1. Sync EmployeeSalaryProjections (lương cơ bản từ hợp đồng) ─
-- Lấy contract data từ HR DB
DECLARE @NV002 UNIQUEIDENTIFIER='8d349666-f6c1-4531-865c-735866324b3a';
DECLARE @NV003 UNIQUEIDENTIFIER='52874dce-6f35-4f61-8f72-80a4424b1bf8';
DECLARE @NV004 UNIQUEIDENTIFIER='0a287da2-cbf8-4eee-892f-0e4a3bb6a8ac';
DECLARE @NV005 UNIQUEIDENTIFIER='8cf9bd02-ae4b-40a7-a059-d7dd55db1800';
DECLARE @NV006 UNIQUEIDENTIFIER='5f33723c-4749-4a10-9606-b040e61513aa';
DECLARE @NV007 UNIQUEIDENTIFIER='e743270b-8f02-4aaf-8372-4afab035f793';
DECLARE @NV008 UNIQUEIDENTIFIER='2e3c2a3d-663b-4203-a78c-1af39cffd95c';
DECLARE @NV009 UNIQUEIDENTIFIER='23a814d7-4a85-44ce-9fb2-c569e11cba1c';

-- Xóa cũ
DELETE FROM EmployeeSalaryProjections WHERE EmployeeId IN (@NV002,@NV003,@NV004,@NV005,@NV006,@NV007,@NV008,@NV009);

INSERT INTO EmployeeSalaryProjections(Id,EmployeeId,ContractId,BaseSalary,EffectiveFrom,Status,LastSyncedAt)
VALUES
(NEWID(),@NV002,NEWID(),18000000,'2022-01-10','Active',GETUTCDATE()),
(NEWID(),@NV003,NEWID(),22000000,'2022-06-01','Active',GETUTCDATE()),
(NEWID(),@NV004,NEWID(),20000000,'2021-09-15','Active',GETUTCDATE()),
(NEWID(),@NV005,NEWID(),16000000,'2020-03-01','Active',GETUTCDATE()),
(NEWID(),@NV006,NEWID(),15000000,'2023-02-14','Active',GETUTCDATE()),
(NEWID(),@NV007,NEWID(),14000000,'2021-07-20','Active',GETUTCDATE()),
(NEWID(),@NV008,NEWID(),13000000,'2022-11-01','Active',GETUTCDATE()),
(NEWID(),@NV009,NEWID(),25000000,'2020-12-15','Active',GETUTCDATE());
PRINT 'EmployeeSalaryProjections: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- ── 2. Sync AttendanceProjections (từ Attendance DB) ─────
DELETE FROM AttendanceProjections WHERE WorkDate>='2026-06-01' AND WorkDate<='2026-06-30';

-- NV002: 22 ngày, 1 ngày Late (vẫn tính đủ công)
-- Dùng số liệu tổng hợp: insert 1 row per employee per day
-- Lấy dữ liệu thực tế từ script attendance đã seed:
INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV002,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),
    CASE WHEN DATEADD(DAY,n,CAST('2026-06-01' AS DATE))='2026-06-16' THEN 457 ELSE 484 END,
    CASE WHEN DATEADD(DAY,n,CAST('2026-06-01' AS DATE))='2026-06-16' THEN 'Late' ELSE 'CheckedOut' END,
    GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(8),(9),(10),(11),(12),(15),(16),(17),(18),(19),(22),(23),(24),(25),(26),(29)) AS t(n)
WHERE DATEPART(WEEKDAY,DATEADD(DAY,n,CAST('2026-06-01' AS DATE))) NOT IN(1,7);

INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV003,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),490,'CheckedOut',GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(8),(9),(10),(11),(12),(15),(16),(17),(18),(22),(23),(24),(25),(26),(29)) AS t(n); -- skip 19

INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV004,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),480,'CheckedOut',GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(8),(9),(10),(11),(12),(15),(16),(17),(18),(19),(22),(25),(26),(29)) AS t(n); -- skip 23,24

INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV005,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),500,'CheckedOut',GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(8),(9),(10),(11),(12),(15),(16),(17),(18),(19),(22),(23),(24),(25),(26),(29)) AS t(n);

INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV006,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),482,'CheckedOut',GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(8),(9),(10),(11),(12),(15),(16),(17),(18),(19),(22),(23),(24),(25),(26),(29)) AS t(n);

INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV007,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),473,'CheckedOut',GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(12),(15),(16),(17),(18),(19),(22),(23),(24),(25),(26),(29)) AS t(n); -- skip 9,10,11

INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV008,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),486,'CheckedOut',GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(8),(9),(10),(11),(12),(15),(16),(17),(18),(19),(22),(23),(24),(25),(26),(29)) AS t(n);

INSERT INTO AttendanceProjections(AttendanceRecordId,EmployeeId,WorkDate,WorkedMinutes,Status,LastSyncedAt)
SELECT NEWID(),@NV009,DATEADD(DAY,n,CAST('2026-06-01' AS DATE)),525,'CheckedOut',GETUTCDATE()
FROM (VALUES(1),(2),(3),(4),(5),(8),(9),(10),(11),(12),(15),(16),(17),(18),(19),(22),(23),(24),(25),(26),(29)) AS t(n);

DECLARE @attP INT=(SELECT COUNT(*) FROM AttendanceProjections WHERE WorkDate>='2026-06-01');
PRINT 'AttendanceProjections: ' + CAST(@attP AS VARCHAR);

-- ── 3. Sync LeaveProjections ─────────────────────────────
DELETE FROM LeaveProjections WHERE FromDate>='2026-06-01' AND ToDate<='2026-06-30';

INSERT INTO LeaveProjections(LeaveRequestId,EmployeeId,FromDate,ToDate,TotalDays,IsPaid,LastSyncedAt)
VALUES
-- NV003 nghỉ phép năm 1 ngày 19/6 (có lương)
(NEWID(),@NV003,'2026-06-19','2026-06-19',1,1,GETUTCDATE()),
-- NV004 nghỉ ốm 23-24/6 (có lương)
(NEWID(),@NV004,'2026-06-23','2026-06-24',2,1,GETUTCDATE()),
-- NV007 nghỉ không lương 9-11/6 (không lương)
(NEWID(),@NV007,'2026-06-09','2026-06-11',3,0,GETUTCDATE());

PRINT 'LeaveProjections: 3 approved leaves';

-- ── Summary ──────────────────────────────────────────────
SELECT 'EmployeeProjections'     AS [Table], COUNT(*) AS [Count] FROM EmployeeProjections
UNION ALL SELECT 'SalaryProjections',COUNT(*) FROM EmployeeSalaryProjections
UNION ALL SELECT 'AttendanceProjections',COUNT(*) FROM AttendanceProjections WHERE WorkDate>='2026-06-01'
UNION ALL SELECT 'LeaveProjections',COUNT(*) FROM LeaveProjections WHERE FromDate>='2026-06-01'
UNION ALL SELECT 'PayrollPeriods',COUNT(*) FROM PayrollPeriods
UNION ALL SELECT 'Payslips',COUNT(*) FROM Payslips;
GO
