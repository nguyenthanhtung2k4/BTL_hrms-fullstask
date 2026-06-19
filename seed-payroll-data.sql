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
DECLARE @NV002 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV002');
DECLARE @NV003 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV003');
DECLARE @NV004 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV004');
DECLARE @NV005 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV005');
DECLARE @NV006 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV006');
DECLARE @NV007 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV007');
DECLARE @NV008 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV008');
DECLARE @NV009 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV009');

PRINT 'EmployeeSalaryProjections (Done in sync-projections.sql)';

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

-- ── 4. Seed Payroll Period, Allowances, Deductions, Payslips ──────────
IF NOT EXISTS (SELECT 1 FROM dbo.PayrollRules)
    INSERT INTO dbo.PayrollRules (Id, Code, Name, WorkDayHours, PaidLeaveCountsAsWork, OvertimeRate, IsActive)
    VALUES (NEWID(), 'RULE_STANDARD', N'Quy tắc lương chuẩn', 8.0, 1, 1.5, 1);

DECLARE @RuleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.PayrollRules WHERE Code = 'RULE_STANDARD' OR Code = 'STANDARD');
IF @RuleId IS NULL
    SET @RuleId = (SELECT TOP 1 Id FROM dbo.PayrollRules);

DECLARE @PeriodId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.PayrollPeriods WHERE Code = 'PERIOD_2026_06');
IF @PeriodId IS NULL
BEGIN
    SET @PeriodId = NEWID();
    INSERT INTO dbo.PayrollPeriods(Id, Code, Name, FromDate, ToDate, StandardWorkDays, PayrollRuleId, Status, CreatedAt)
    VALUES (@PeriodId, 'PERIOD_2026_06', N'Lương T6/2026', '2026-06-01', '2026-06-30', 22, @RuleId, 'Draft', GETUTCDATE());
END

-- Allowance Types
IF NOT EXISTS (SELECT 1 FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_EAT')
    INSERT INTO dbo.AllowanceTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'ALLOW_EAT', N'Phụ cấp ăn trưa', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_FUEL')
    INSERT INTO dbo.AllowanceTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'ALLOW_FUEL', N'Phụ cấp đi lại', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_TEL')
    INSERT INTO dbo.AllowanceTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'ALLOW_TEL', N'Phụ cấp điện thoại', 1);

-- Deduction Types
IF NOT EXISTS (SELECT 1 FROM dbo.DeductionTypes WHERE Code = 'DED_INS')
    INSERT INTO dbo.DeductionTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.DeductionTypes WHERE Code = 'DED_TAX')
    INSERT INTO dbo.DeductionTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'DED_TAX', N'Thuế thu nhập cá nhân', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.DeductionTypes WHERE Code = 'DED_PENALTY')
    INSERT INTO dbo.DeductionTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'DED_PENALTY', N'Khấu trừ đi muộn/vi phạm', 1);

-- Clean old EmployeeAllowances, EmployeeDeductions, Payslips, PayslipItems
DELETE FROM dbo.EmployeeAllowances WHERE PayrollPeriodId = @PeriodId;
DELETE FROM dbo.EmployeeDeductions WHERE PayrollPeriodId = @PeriodId;
DELETE FROM dbo.PayslipItems WHERE PayslipId IN (SELECT Id FROM dbo.Payslips WHERE PayrollPeriodId = @PeriodId);
DELETE FROM dbo.Payslips WHERE PayrollPeriodId = @PeriodId;

DECLARE @AllowEatId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_EAT');
DECLARE @AllowFuelId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_FUEL');
DECLARE @AllowTelId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_TEL');

DECLARE @DedInsId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_INS');
DECLARE @DedTaxId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_TAX');
DECLARE @DedPenaltyId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_PENALTY');

-- Insert Employee Allowances
INSERT INTO dbo.EmployeeAllowances(Id, EmployeeId, PayrollPeriodId, AllowanceTypeId, Amount, Note) VALUES
(NEWID(), @NV002, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV002, @PeriodId, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV003, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV003, @PeriodId, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV003, @PeriodId, @AllowTelId, 200000, N'Điện thoại T6/2026'),
(NEWID(), @NV004, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV004, @PeriodId, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV005, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV005, @PeriodId, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV006, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV006, @PeriodId, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV007, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV007, @PeriodId, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV008, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV008, @PeriodId, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV009, @PeriodId, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV009, @PeriodId, @AllowFuelId, 500000, N'Đi lại T6/2026'),
(NEWID(), @NV009, @PeriodId, @AllowTelId, 500000, N'Điện thoại T6/2026');

-- Insert Employee Deductions
INSERT INTO dbo.EmployeeDeductions(Id, EmployeeId, PayrollPeriodId, DeductionTypeId, Amount, Note) VALUES
(NEWID(), @NV002, @PeriodId, @DedInsId, 1800000, N'BHXH T6/2026'),
(NEWID(), @NV002, @PeriodId, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV002, @PeriodId, @DedPenaltyId, 100000, N'Đi muộn ngày 16/6'),
(NEWID(), @NV003, @PeriodId, @DedInsId, 2200000, N'BHXH T6/2026'),
(NEWID(), @NV003, @PeriodId, @DedTaxId, 200000, N'Thuế TNCN T6/2026'),
(NEWID(), @NV004, @PeriodId, @DedInsId, 2000000, N'BHXH T6/2026'),
(NEWID(), @NV004, @PeriodId, @DedTaxId, 100000, N'Thuế TNCN T6/2026'),
(NEWID(), @NV005, @PeriodId, @DedInsId, 1600000, N'BHXH T6/2026'),
(NEWID(), @NV005, @PeriodId, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV006, @PeriodId, @DedInsId, 1500000, N'BHXH T6/2026'),
(NEWID(), @NV006, @PeriodId, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV007, @PeriodId, @DedInsId, 1400000, N'BHXH T6/2026'),
(NEWID(), @NV007, @PeriodId, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV008, @PeriodId, @DedInsId, 1300000, N'BHXH T6/2026'),
(NEWID(), @NV008, @PeriodId, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV009, @PeriodId, @DedInsId, 2500000, N'BHXH T6/2026'),
(NEWID(), @NV009, @PeriodId, @DedTaxId, 800000, N'Thuế TNCN T6/2026');

-- Seed Payslips & PayslipItems
DECLARE @PayslipId UNIQUEIDENTIFIER;

-- --- NV002 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV002, 18000000, 22.0, 0.0, 18980000, 1900000, 17080000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 18000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1800000, 'DeductionSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_PENALTY', N'Đi muộn ngày 16/6', 100000, 'AttendancePenalty', GETUTCDATE());

-- --- NV003 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV003, 22000000, 21.0, 1.0, 23180000, 2400000, 20780000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 22000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_TEL', N'Phụ cấp điện thoại', 200000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 2200000, 'DeductionSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_TAX', N'Thuế thu nhập cá nhân', 200000, 'TaxSetting', GETUTCDATE());

-- --- NV004 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV004, 20000000, 20.0, 2.0, 20980000, 2100000, 18880000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 20000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 2000000, 'DeductionSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_TAX', N'Thuế thu nhập cá nhân', 100000, 'TaxSetting', GETUTCDATE());

-- --- NV005 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV005, 16000000, 22.0, 0.0, 16980000, 1600000, 15380000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 16000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1600000, 'DeductionSetting', GETUTCDATE());

-- --- NV006 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV006, 15000000, 22.0, 0.0, 15980000, 1500000, 14480000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 15000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1500000, 'DeductionSetting', GETUTCDATE());

-- --- NV007 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV007, 14000000, 19.0, 0.0, 13070909, 1400000, 11670909, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương làm việc thực tế (19/22 ngày)', 12090909, 'AttendanceCalculation', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1400000, 'DeductionSetting', GETUTCDATE());

-- --- NV008 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV008, 13000000, 22.0, 0.0, 13980000, 1300000, 12680000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 13000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1300000, 'DeductionSetting', GETUTCDATE());

-- --- NV009 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId, @NV009, 25000000, 22.0, 0.0, 26680000, 3300000, 23380000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 25000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 500000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_TEL', N'Phụ cấp điện thoại', 500000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 2500000, 'DeductionSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_TAX', N'Thuế thu nhập cá nhân', 800000, 'TaxSetting', GETUTCDATE());

PRINT 'Payslips & PayslipItems: seeded successfully';

-- ── 5. Seed Extra Tables in Payroll DB ─────────────────────────
DELETE FROM OutboxMessages;
DELETE FROM InboxMessages;
DELETE FROM AuditLogs;

INSERT INTO OutboxMessages (Id, EventName, EventVersion, Payload, CorrelationId, OccurredAt, ProcessedAt, RetryCount, Status)
VALUES (NEWID(), 'PayrollCalculatedEvent', 1, '{"PeriodId":"PERIOD_2026_06"}', NEWID(), GETUTCDATE(), GETUTCDATE(), 0, 'Processed');
INSERT INTO InboxMessages (Id, EventName, EventVersion, Payload, ReceivedAt, ProcessedAt, RetryCount, Status)
VALUES (NEWID(), 'EmployeeSalaryCreatedEvent', 1, '{"EmployeeId":"NV002","BaseSalary":18000000}', GETUTCDATE(), GETUTCDATE(), 0, 'Processed');
INSERT INTO AuditLogs (Id, ActorEmployeeId, Action, EntityName, EntityId, OldValues, NewValues, CreatedAt)
VALUES (NEWID(), @NV009, 'CALCULATE', 'Payroll', 'PERIOD_2026_06', NULL, '{"Status":"Draft","TotalPayslips":8}', GETUTCDATE());

PRINT 'Inbox, Outbox & AuditLogs in Payroll DB: seeded successfully';

-- ── Summary ──────────────────────────────────────────────
SELECT 'EmployeeProjections'     AS [Table], COUNT(*) AS [Count] FROM EmployeeProjections
UNION ALL SELECT 'SalaryProjections',COUNT(*) FROM EmployeeSalaryProjections
UNION ALL SELECT 'AttendanceProjections',COUNT(*) FROM AttendanceProjections WHERE WorkDate>='2026-06-01'
UNION ALL SELECT 'LeaveProjections',COUNT(*) FROM LeaveProjections WHERE FromDate>='2026-06-01'
UNION ALL SELECT 'PayrollPeriods',COUNT(*) FROM PayrollPeriods
UNION ALL SELECT 'Payslips',COUNT(*) FROM Payslips
UNION ALL SELECT 'PayslipItems',COUNT(*) FROM PayslipItems
UNION ALL SELECT 'AllowanceTypes',COUNT(*) FROM AllowanceTypes
UNION ALL SELECT 'DeductionTypes',COUNT(*) FROM DeductionTypes
UNION ALL SELECT 'EmployeeAllowances',COUNT(*) FROM EmployeeAllowances
UNION ALL SELECT 'EmployeeDeductions',COUNT(*) FROM EmployeeDeductions;
GO
