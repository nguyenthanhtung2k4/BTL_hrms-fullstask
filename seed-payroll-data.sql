-- ============================================================
-- HRMS Payroll DB - May, June, July 2026
-- Database: HRMS_PayrollReportDb (port 1434)
-- ============================================================

USE HRMS_PayrollReportDb;
GO

-- Employee IDs
DECLARE @NV002 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV002');
DECLARE @NV003 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV003');
DECLARE @NV004 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV004');
DECLARE @NV005 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV005');
DECLARE @NV006 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV006');
DECLARE @NV007 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV007');
DECLARE @NV008 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV008');
DECLARE @NV009 UNIQUEIDENTIFIER = (SELECT TOP 1 EmployeeId FROM EmployeeProjections WHERE EmployeeCode = 'NV009');

-- ── 1. Sync AttendanceProjections (direct from Attendance DB) ──
DELETE FROM AttendanceProjections WHERE WorkDate>='2026-05-01' AND WorkDate<='2026-07-31';

INSERT INTO AttendanceProjections (AttendanceRecordId, EmployeeId, WorkDate, WorkedMinutes, Status, LastSyncedAt)
SELECT Id, EmployeeId, WorkDate, WorkedMinutes, Status, GETUTCDATE()
FROM HRMS_AttendanceDb.dbo.AttendanceRecords
WHERE WorkDate>='2026-05-01' AND WorkDate<='2026-07-31';

DECLARE @attP INT=(SELECT COUNT(*) FROM AttendanceProjections WHERE WorkDate>='2026-05-01');
PRINT 'AttendanceProjections synced: ' + CAST(@attP AS VARCHAR);

-- ── 2. Sync LeaveProjections (direct from Attendance DB) ──
DELETE FROM LeaveProjections WHERE FromDate>='2026-05-01' AND ToDate<='2026-07-31';

INSERT INTO LeaveProjections (LeaveRequestId, EmployeeId, FromDate, ToDate, TotalDays, IsPaid, LastSyncedAt)
SELECT lr.Id, lr.EmployeeId, lr.FromDate, lr.ToDate, lr.TotalDays, lt.IsPaid, GETUTCDATE()
FROM HRMS_AttendanceDb.dbo.LeaveRequests lr
JOIN HRMS_AttendanceDb.dbo.LeaveTypes lt ON lr.LeaveTypeId = lt.Id
WHERE lr.Status = 'Approved' AND lr.FromDate>='2026-05-01' AND lr.ToDate<='2026-07-31';

PRINT 'LeaveProjections synced';

-- ── 3. Seed Payroll Rule & Periods ──────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.PayrollRules WHERE Code = 'RULE_STANDARD')
    INSERT INTO dbo.PayrollRules (Id, Code, Name, WorkDayHours, PaidLeaveCountsAsWork, OvertimeRate, IsActive)
    VALUES (NEWID(), 'RULE_STANDARD', N'Quy tắc lương chuẩn', 8.0, 1, 1.5, 1);

DECLARE @RuleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.PayrollRules WHERE Code = 'RULE_STANDARD');

-- May 2026
DECLARE @PeriodId_05 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.PayrollPeriods WHERE Code = 'PERIOD_2026_05');
IF @PeriodId_05 IS NULL
BEGIN
    SET @PeriodId_05 = NEWID();
    INSERT INTO dbo.PayrollPeriods(Id, Code, Name, FromDate, ToDate, StandardWorkDays, PayrollRuleId, Status, CreatedAt)
    VALUES (@PeriodId_05, 'PERIOD_2026_05', N'Lương T5/2026', '2026-05-01', '2026-05-31', 21, @RuleId, 'Draft', GETUTCDATE());
END

-- June 2026
DECLARE @PeriodId_06 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.PayrollPeriods WHERE Code = 'PERIOD_2026_06');
IF @PeriodId_06 IS NULL
BEGIN
    SET @PeriodId_06 = NEWID();
    INSERT INTO dbo.PayrollPeriods(Id, Code, Name, FromDate, ToDate, StandardWorkDays, PayrollRuleId, Status, CreatedAt)
    VALUES (@PeriodId_06, 'PERIOD_2026_06', N'Lương T6/2026', '2026-06-01', '2026-06-30', 22, @RuleId, 'Draft', GETUTCDATE());
END

-- July 2026
DECLARE @PeriodId_07 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.PayrollPeriods WHERE Code = 'PERIOD_2026_07');
IF @PeriodId_07 IS NULL
BEGIN
    SET @PeriodId_07 = NEWID();
    INSERT INTO dbo.PayrollPeriods(Id, Code, Name, FromDate, ToDate, StandardWorkDays, PayrollRuleId, Status, CreatedAt)
    VALUES (@PeriodId_07, 'PERIOD_2026_07', N'Lương T7/2026', '2026-07-01', '2026-07-31', 23, @RuleId, 'Draft', GETUTCDATE());
END

-- ── 4. Allowance & Deduction Types ─────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_EAT')
    INSERT INTO dbo.AllowanceTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'ALLOW_EAT', N'Phụ cấp ăn trưa', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_FUEL')
    INSERT INTO dbo.AllowanceTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'ALLOW_FUEL', N'Phụ cấp đi lại', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_TEL')
    INSERT INTO dbo.AllowanceTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'ALLOW_TEL', N'Phụ cấp điện thoại', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.DeductionTypes WHERE Code = 'DED_INS')
    INSERT INTO dbo.DeductionTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.DeductionTypes WHERE Code = 'DED_TAX')
    INSERT INTO dbo.DeductionTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'DED_TAX', N'Thuế thu nhập cá nhân', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.DeductionTypes WHERE Code = 'DED_PENALTY')
    INSERT INTO dbo.DeductionTypes (Id, Code, Name, IsActive) VALUES (NEWID(), 'DED_PENALTY', N'Khấu trừ đi muộn/vi phạm', 1);

DECLARE @AllowEatId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_EAT');
DECLARE @AllowFuelId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_FUEL');
DECLARE @AllowTelId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_TEL');

DECLARE @DedInsId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_INS');
DECLARE @DedTaxId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_TAX');
DECLARE @DedPenaltyId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_PENALTY');

-- ── 5. Seed Allowances & Deductions for June 2026 ──────────
DELETE FROM dbo.EmployeeAllowances WHERE PayrollPeriodId IN (@PeriodId_05, @PeriodId_06, @PeriodId_07);
DELETE FROM dbo.EmployeeDeductions WHERE PayrollPeriodId IN (@PeriodId_05, @PeriodId_06, @PeriodId_07);

INSERT INTO dbo.EmployeeAllowances(Id, EmployeeId, PayrollPeriodId, AllowanceTypeId, Amount, Note) VALUES
(NEWID(), @NV002, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV002, @PeriodId_06, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV003, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV003, @PeriodId_06, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV003, @PeriodId_06, @AllowTelId, 200000, N'Điện thoại T6/2026'),
(NEWID(), @NV004, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV004, @PeriodId_06, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV005, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV005, @PeriodId_06, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV006, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV006, @PeriodId_06, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV007, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV007, @PeriodId_06, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV008, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV008, @PeriodId_06, @AllowFuelId, 300000, N'Đi lại T6/2026'),
(NEWID(), @NV009, @PeriodId_06, @AllowEatId, 680000, N'Ăn trưa T6/2026'),
(NEWID(), @NV009, @PeriodId_06, @AllowFuelId, 500000, N'Đi lại T6/2026'),
(NEWID(), @NV009, @PeriodId_06, @AllowTelId, 500000, N'Điện thoại T6/2026');

INSERT INTO dbo.EmployeeDeductions(Id, EmployeeId, PayrollPeriodId, DeductionTypeId, Amount, Note) VALUES
(NEWID(), @NV002, @PeriodId_06, @DedInsId, 1800000, N'BHXH T6/2026'),
(NEWID(), @NV002, @PeriodId_06, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV002, @PeriodId_06, @DedPenaltyId, 100000, N'Đi muộn ngày 16/6'),
(NEWID(), @NV003, @PeriodId_06, @DedInsId, 2200000, N'BHXH T6/2026'),
(NEWID(), @NV003, @PeriodId_06, @DedTaxId, 200000, N'Thuế TNCN T6/2026'),
(NEWID(), @NV004, @PeriodId_06, @DedInsId, 2000000, N'BHXH T6/2026'),
(NEWID(), @NV004, @PeriodId_06, @DedTaxId, 100000, N'Thuế TNCN T6/2026'),
(NEWID(), @NV005, @PeriodId_06, @DedInsId, 1600000, N'BHXH T6/2026'),
(NEWID(), @NV005, @PeriodId_06, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV006, @PeriodId_06, @DedInsId, 1500000, N'BHXH T6/2026'),
(NEWID(), @NV006, @PeriodId_06, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV007, @PeriodId_06, @DedInsId, 1400000, N'BHXH T6/2026'),
(NEWID(), @NV007, @PeriodId_06, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV008, @PeriodId_06, @DedInsId, 1300000, N'BHXH T6/2026'),
(NEWID(), @NV008, @PeriodId_06, @DedTaxId, 0, N'Thuế TNCN T6/2026'),
(NEWID(), @NV009, @PeriodId_06, @DedInsId, 2500000, N'BHXH T6/2026'),
(NEWID(), @NV009, @PeriodId_06, @DedTaxId, 800000, N'Thuế TNCN T6/2026');

-- Duplicate for May 2026
INSERT INTO dbo.EmployeeAllowances (Id, EmployeeId, PayrollPeriodId, AllowanceTypeId, Amount, Note)
SELECT NEWID(), EmployeeId, @PeriodId_05, AllowanceTypeId, Amount, REPLACE(Note, 'T6/2026', 'T5/2026')
FROM dbo.EmployeeAllowances WHERE PayrollPeriodId = @PeriodId_06;

INSERT INTO dbo.EmployeeDeductions (Id, EmployeeId, PayrollPeriodId, DeductionTypeId, Amount, Note)
SELECT NEWID(), EmployeeId, @PeriodId_05, DeductionTypeId, Amount, REPLACE(REPLACE(Note, 'T6/2026', 'T5/2026'), '16/6', '15/5')
FROM dbo.EmployeeDeductions WHERE PayrollPeriodId = @PeriodId_06;

-- Duplicate for July 2026
INSERT INTO dbo.EmployeeAllowances (Id, EmployeeId, PayrollPeriodId, AllowanceTypeId, Amount, Note)
SELECT NEWID(), EmployeeId, @PeriodId_07, AllowanceTypeId, Amount, REPLACE(Note, 'T6/2026', 'T7/2026')
FROM dbo.EmployeeAllowances WHERE PayrollPeriodId = @PeriodId_06;

INSERT INTO dbo.EmployeeDeductions (Id, EmployeeId, PayrollPeriodId, DeductionTypeId, Amount, Note)
SELECT NEWID(), EmployeeId, @PeriodId_07, DeductionTypeId, Amount, REPLACE(REPLACE(Note, 'T6/2026', 'T7/2026'), '16/6', '17/7')
FROM dbo.EmployeeDeductions WHERE PayrollPeriodId = @PeriodId_06;

PRINT 'Allowances & Deductions populated for all months';

-- ── 6. Seed Payslips & PayslipItems for June 2026 ──────────
DELETE FROM dbo.PayslipItems WHERE PayslipId IN (SELECT Id FROM dbo.Payslips WHERE PayrollPeriodId IN (@PeriodId_05, @PeriodId_06, @PeriodId_07));
DELETE FROM dbo.Payslips WHERE PayrollPeriodId IN (@PeriodId_05, @PeriodId_06, @PeriodId_07);

DECLARE @PayslipId UNIQUEIDENTIFIER;

-- --- NV002 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId_06, @NV002, 18000000, 22.0, 0.0, 18980000, 1900000, 17080000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 18000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1800000, 'DeductionSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_PENALTY', N'Đi muộn ngày 16/6', 100000, 'AttendancePenalty', GETUTCDATE());

-- --- NV003 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId_06, @NV003, 22000000, 21.0, 1.0, 23180000, 2400000, 20780000, 'Approved', GETUTCDATE());
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
VALUES (@PayslipId, @PeriodId_06, @NV004, 20000000, 20.0, 2.0, 20980000, 2100000, 18880000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 20000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 2000000, 'DeductionSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_TAX', N'Thuế thu nhập cá nhân', 100000, 'TaxSetting', GETUTCDATE());

-- --- NV005 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId_06, @NV005, 16000000, 22.0, 0.0, 16980000, 1600000, 15380000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 16000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1600000, 'DeductionSetting', GETUTCDATE());

-- --- NV006 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId_06, @NV006, 15000000, 22.0, 0.0, 15980000, 1500000, 14480000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 15000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1500000, 'DeductionSetting', GETUTCDATE());

-- --- NV007 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId_06, @NV007, 14000000, 19.0, 0.0, 13070909, 1400000, 11670909, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương làm việc thực tế (19/22 ngày)', 12090909, 'AttendanceCalculation', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1400000, 'DeductionSetting', GETUTCDATE());

-- --- NV008 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId_06, @NV008, 13000000, 22.0, 0.0, 13980000, 1300000, 12680000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 13000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 300000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1300000, 'DeductionSetting', GETUTCDATE());

-- --- NV009 ---
SET @PayslipId = NEWID();
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
VALUES (@PayslipId, @PeriodId_06, @NV009, 25000000, 22.0, 0.0, 26680000, 3300000, 23380000, 'Approved', GETUTCDATE());
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt) VALUES
(NEWID(), @PayslipId, 'Salary', 'BASE_SALARY', N'Lương cơ bản', 25000000, 'Contract', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', 680000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', 500000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Allowance', 'ALLOW_TEL', N'Phụ cấp điện thoại', 500000, 'AllowanceSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 2500000, 'DeductionSetting', GETUTCDATE()),
(NEWID(), @PayslipId, 'Deduction', 'DED_TAX', N'Thuế thu nhập cá nhân', 800000, 'TaxSetting', GETUTCDATE());

PRINT 'Payslips seeded for June 2026';

-- ── 7. Duplicate Payslips & PayslipItems for May 2026 (21 standard days) ──
IF OBJECT_ID('tempdb..#PayslipMap') IS NOT NULL DROP TABLE #PayslipMap;
CREATE TABLE #PayslipMap (OldId UNIQUEIDENTIFIER, NewId UNIQUEIDENTIFIER);

INSERT INTO #PayslipMap (OldId, NewId)
SELECT Id, NEWID() FROM dbo.Payslips WHERE PayrollPeriodId = @PeriodId_06;

-- Insert May Payslips
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
SELECT m.NewId, @PeriodId_05, p.EmployeeId, p.BaseSalary, 
       CASE WHEN p.WorkedDays = 22.0 THEN 21.0 ELSE p.WorkedDays - 1.0 END,
       p.PaidLeaveDays, p.GrossSalary, p.TotalDeduction, p.NetSalary, p.Status, GETUTCDATE()
FROM dbo.Payslips p
JOIN #PayslipMap m ON p.Id = m.OldId;

-- Insert May PayslipItems
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), m.NewId, pi.ItemType, pi.Code, REPLACE(pi.Name, '16/6', '15/5'), pi.Amount, pi.SourceType, GETUTCDATE()
FROM dbo.PayslipItems pi
JOIN #PayslipMap m ON pi.PayslipId = m.OldId;

-- ── 8. Duplicate Payslips & PayslipItems for July 2026 (23 standard days) ──
TRUNCATE TABLE #PayslipMap;

INSERT INTO #PayslipMap (OldId, NewId)
SELECT Id, NEWID() FROM dbo.Payslips WHERE PayrollPeriodId = @PeriodId_06;

-- Insert July Payslips
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
SELECT m.NewId, @PeriodId_07, p.EmployeeId, p.BaseSalary, 
       CASE WHEN p.WorkedDays = 22.0 THEN 23.0 ELSE p.WorkedDays + 1.0 END,
       p.PaidLeaveDays, p.GrossSalary, p.TotalDeduction, p.NetSalary, p.Status, GETUTCDATE()
FROM dbo.Payslips p
JOIN #PayslipMap m ON p.Id = m.OldId;

-- Insert July PayslipItems
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), m.NewId, pi.ItemType, pi.Code, REPLACE(pi.Name, '16/6', '17/7'), pi.Amount, pi.SourceType, GETUTCDATE()
FROM dbo.PayslipItems pi
JOIN #PayslipMap m ON pi.PayslipId = m.OldId;

DROP TABLE #PayslipMap;

PRINT 'Payslips populated for all months';

-- ── 9. Seed Extra Tables in Payroll DB ─────────────────────────
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
UNION ALL SELECT 'AttendanceProjections',COUNT(*) FROM AttendanceProjections WHERE WorkDate>='2026-05-01'
UNION ALL SELECT 'LeaveProjections',COUNT(*) FROM LeaveProjections WHERE FromDate>='2026-05-01'
UNION ALL SELECT 'PayrollPeriods',COUNT(*) FROM PayrollPeriods
UNION ALL SELECT 'Payslips',COUNT(*) FROM Payslips
UNION ALL SELECT 'PayslipItems',COUNT(*) FROM PayslipItems
UNION ALL SELECT 'AllowanceTypes',COUNT(*) FROM AllowanceTypes
UNION ALL SELECT 'DeductionTypes',COUNT(*) FROM DeductionTypes
UNION ALL SELECT 'EmployeeAllowances',COUNT(*) FROM EmployeeAllowances
UNION ALL SELECT 'EmployeeDeductions',COUNT(*) FROM EmployeeDeductions;
GO
