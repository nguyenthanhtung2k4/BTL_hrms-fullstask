-- ============================================================
-- HRMS Payroll DB - 1 Year Dynamic Seed (July 2025 to June 2026)
-- Database: HRMS_PayrollReportDb (port 1434)
-- ============================================================

USE HRMS_PayrollReportDb;
GO

-- ── 1. Clear Old Data ──────────────────────────────────────
DELETE FROM dbo.PayslipItems;
DELETE FROM dbo.Payslips;
DELETE FROM dbo.EmployeeAllowances;
DELETE FROM dbo.EmployeeDeductions;
DELETE FROM dbo.EmployeeSalaryProjections;
DELETE FROM dbo.AttendanceProjections;
DELETE FROM dbo.LeaveProjections;
DELETE FROM dbo.PayrollPeriods;
DELETE FROM dbo.PayrollRules;
DELETE FROM dbo.AllowanceTypes;
DELETE FROM dbo.DeductionTypes;
PRINT 'Old payroll records cleared';
GO

-- ── 2. Sync Projections from Attendance DB ──────────────────
INSERT INTO AttendanceProjections (AttendanceRecordId, EmployeeId, WorkDate, WorkedMinutes, Status, LastSyncedAt)
SELECT Id, EmployeeId, WorkDate, WorkedMinutes, Status, GETUTCDATE()
FROM HRMS_AttendanceDb.dbo.AttendanceRecords
WHERE WorkDate >= '2025-07-01' AND WorkDate <= '2026-06-30';

INSERT INTO LeaveProjections (LeaveRequestId, EmployeeId, FromDate, ToDate, TotalDays, IsPaid, LastSyncedAt)
SELECT lr.Id, lr.EmployeeId, lr.FromDate, lr.ToDate, lr.TotalDays, lt.IsPaid, GETUTCDATE()
FROM HRMS_AttendanceDb.dbo.LeaveRequests lr
JOIN HRMS_AttendanceDb.dbo.LeaveTypes lt ON lr.LeaveTypeId = lt.Id
WHERE lr.Status = 'Approved' AND lr.FromDate >= '2025-07-01' AND lr.ToDate <= '2026-06-30';

PRINT 'Projections synced from Attendance DB';

-- ── 3. Seed Payroll Rule & Periods ──────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.PayrollRules WHERE Code = 'RULE_STANDARD')
    INSERT INTO dbo.PayrollRules (Id, Code, Name, WorkDayHours, PaidLeaveCountsAsWork, OvertimeRate, IsActive)
    VALUES (NEWID(), 'RULE_STANDARD', N'Quy tắc lương chuẩn', 8.0, 1, 1.5, 1);

DECLARE @RuleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.PayrollRules WHERE Code = 'RULE_STANDARD');

-- Bảng 12 chu kỳ tính lương
IF OBJECT_ID('tempdb..#PeriodDef') IS NOT NULL DROP TABLE #PeriodDef;
CREATE TABLE #PeriodDef (
    Code VARCHAR(50),
    Name NVARCHAR(100),
    FromD DATE,
    ToD DATE,
    WDays INT
);

INSERT INTO #PeriodDef VALUES 
('PERIOD_2025_07', N'Lương T7/2025', '2025-07-01', '2025-07-31', 23),
('PERIOD_2025_08', N'Lương T8/2025', '2025-08-01', '2025-08-31', 21),
('PERIOD_2025_09', N'Lương T9/2025', '2025-09-01', '2025-09-30', 22),
('PERIOD_2025_10', N'Lương T10/2025', '2025-10-01', '2025-10-31', 23),
('PERIOD_2025_11', N'Lương T11/2025', '2025-11-01', '2025-11-30', 20),
('PERIOD_2025_12', N'Lương T12/2025', '2025-12-01', '2025-12-31', 23),
('PERIOD_2026_01', N'Lương T1/2026', '2026-01-01', '2026-01-31', 22),
('PERIOD_2026_02', N'Lương T2/2026', '2026-02-01', '2026-02-28', 20),
('PERIOD_2026_03', N'Lương T3/2026', '2026-03-01', '2026-03-31', 22),
('PERIOD_2026_04', N'Lương T4/2026', '2026-04-01', '2026-04-30', 22),
('PERIOD_2026_05', N'Lương T5/2026', '2026-05-01', '2026-05-31', 21),
('PERIOD_2026_06', N'Lương T6/2026', '2026-06-01', '2026-06-30', 22);

INSERT INTO dbo.PayrollPeriods (Id, Code, Name, FromDate, ToDate, StandardWorkDays, PayrollRuleId, Status, CreatedAt)
SELECT NEWID(), Code, Name, FromD, ToD, WDays, @RuleId, 'Draft', GETUTCDATE()
FROM #PeriodDef;

PRINT '12 Payroll periods seeded';

-- ── 4. Allowance & Deduction Types ─────────────────────────
INSERT INTO dbo.AllowanceTypes (Id, Code, Name, IsActive) VALUES 
(NEWID(), 'ALLOW_EAT', N'Phụ cấp ăn trưa', 1),
(NEWID(), 'ALLOW_FUEL', N'Phụ cấp đi lại', 1),
(NEWID(), 'ALLOW_TEL', N'Phụ cấp điện thoại', 1);

INSERT INTO dbo.DeductionTypes (Id, Code, Name, IsActive) VALUES 
(NEWID(), 'DED_INS', N'Khấu trừ bảo hiểm xã hội', 1),
(NEWID(), 'DED_TAX', N'Thuế thu nhập cá nhân', 1),
(NEWID(), 'DED_PENALTY', N'Khấu trừ đi muộn/vi phạm', 1);

DECLARE @AllowEatId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_EAT');
DECLARE @AllowFuelId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_FUEL');
DECLARE @AllowTelId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.AllowanceTypes WHERE Code = 'ALLOW_TEL');

DECLARE @DedInsId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_INS');
DECLARE @DedTaxId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_TAX');
DECLARE @DedPenaltyId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.DeductionTypes WHERE Code = 'DED_PENALTY');

-- ── 5. Sync EmployeeSalaryProjections ──────────────────────
-- Đồng bộ trực tiếp từ hợp đồng để đảm bảo có bảng lương làm gốc
INSERT INTO dbo.EmployeeSalaryProjections (Id, EmployeeId, ContractId, BaseSalary, EffectiveFrom, EffectiveTo, Status, LastSyncedAt)
SELECT NEWID(), EmployeeId, Id, BaseSalary, StartDate, EndDate, Status, GETUTCDATE()
FROM HRMS_HrCoreDb.dbo.Contracts;

PRINT 'Salary Projections synchronized';

-- ── 6. Seed EmployeeAllowances Dynamically ──────────────────
-- Mọi nhân viên đều có phụ cấp ăn trưa: 680,000 VND
INSERT INTO dbo.EmployeeAllowances (Id, EmployeeId, PayrollPeriodId, AllowanceTypeId, Amount, Note)
SELECT NEWID(), ep.EmployeeId, p.Id, @AllowEatId, 680000, N'Ăn trưa ' + p.Name
FROM EmployeeProjections ep
CROSS JOIN PayrollPeriods p
WHERE ep.EmployeeCode <> 'EMP000';

-- Phụ cấp đi lại & điện thoại: Trưởng phòng/Giám đốc (Lương >= 25M) nhận 500k, Nhân viên khác nhận 200k/300k
INSERT INTO dbo.EmployeeAllowances (Id, EmployeeId, PayrollPeriodId, AllowanceTypeId, Amount, Note)
SELECT 
    NEWID(), 
    ep.EmployeeId, 
    p.Id, 
    @AllowFuelId, 
    CASE WHEN esp.BaseSalary >= 25000000 THEN 500000 ELSE 300000 END,
    N'Đi lại ' + p.Name
FROM EmployeeProjections ep
JOIN EmployeeSalaryProjections esp ON ep.EmployeeId = esp.EmployeeId
CROSS JOIN PayrollPeriods p
WHERE ep.EmployeeCode <> 'EMP000';

INSERT INTO dbo.EmployeeAllowances (Id, EmployeeId, PayrollPeriodId, AllowanceTypeId, Amount, Note)
SELECT 
    NEWID(), 
    ep.EmployeeId, 
    p.Id, 
    @AllowTelId, 
    CASE WHEN esp.BaseSalary >= 25000000 THEN 500000 ELSE 200000 END,
    N'Điện thoại ' + p.Name
FROM EmployeeProjections ep
JOIN EmployeeSalaryProjections esp ON ep.EmployeeId = esp.EmployeeId
CROSS JOIN PayrollPeriods p
WHERE ep.EmployeeCode <> 'EMP000';

PRINT 'Employee allowances seeded dynamically';

-- ── 7. Seed EmployeeDeductions Dynamically ──────────────────
-- Khấu trừ Bảo hiểm xã hội: 10.5% lương cơ bản
INSERT INTO dbo.EmployeeDeductions (Id, EmployeeId, PayrollPeriodId, DeductionTypeId, Amount, Note)
SELECT 
    NEWID(), 
    ep.EmployeeId, 
    p.Id, 
    @DedInsId, 
    CAST(esp.BaseSalary * 0.105 AS INT),
    N'BHXH bắt buộc 10.5% ' + p.Name
FROM EmployeeProjections ep
JOIN EmployeeSalaryProjections esp ON ep.EmployeeId = esp.EmployeeId
CROSS JOIN PayrollPeriods p
WHERE ep.EmployeeCode <> 'EMP000';

-- Khấu trừ đi muộn: Mỗi ngày đi muộn phạt 100,000 VND
INSERT INTO dbo.EmployeeDeductions (Id, EmployeeId, PayrollPeriodId, DeductionTypeId, Amount, Note)
SELECT 
    NEWID(), 
    ep.EmployeeId, 
    p.Id, 
    @DedPenaltyId, 
    lateCount.TotalLate * 100000,
    N'Phạt đi muộn ' + CAST(lateCount.TotalLate AS VARCHAR) + N' lần ' + p.Name
FROM EmployeeProjections ep
CROSS JOIN PayrollPeriods p
JOIN (
    SELECT ep.EmployeeId, p.Id AS PeriodId, COUNT(*) AS TotalLate
    FROM EmployeeProjections ep
    CROSS JOIN PayrollPeriods p
    JOIN AttendanceProjections ap ON ap.EmployeeId = ep.EmployeeId AND ap.WorkDate >= p.FromDate AND ap.WorkDate <= p.ToDate
    WHERE ap.Status = 'Late'
    GROUP BY ep.EmployeeId, p.Id
) lateCount ON ep.EmployeeId = lateCount.EmployeeId AND p.Id = lateCount.PeriodId
WHERE ep.EmployeeCode <> 'EMP000' AND lateCount.TotalLate > 0;

-- Thuế TNCN: Tạm tính 10% phần thu nhập tính thuế vượt trên 11 triệu (sau khi trừ BHXH)
INSERT INTO dbo.EmployeeDeductions (Id, EmployeeId, PayrollPeriodId, DeductionTypeId, Amount, Note)
SELECT 
    NEWID(), 
    ep.EmployeeId, 
    p.Id, 
    @DedTaxId, 
    CASE 
        WHEN (esp.BaseSalary - (esp.BaseSalary * 0.105) - 11000000) > 0 
            THEN CAST((esp.BaseSalary - (esp.BaseSalary * 0.105) - 11000000) * 0.10 AS INT)
        ELSE 0 
    END,
    N'Thuế TNCN tạm tính ' + p.Name
FROM EmployeeProjections ep
JOIN EmployeeSalaryProjections esp ON ep.EmployeeId = esp.EmployeeId
CROSS JOIN PayrollPeriods p
WHERE ep.EmployeeCode <> 'EMP000';

PRINT 'Employee deductions seeded dynamically';

-- ── 8. Seed Payslips & PayslipItems Dynamically ──────────────
-- Dùng bảng tạm để tính toán lương trước khi insert
IF OBJECT_ID('tempdb..#CalcSlips') IS NOT NULL DROP TABLE #CalcSlips;

CREATE TABLE #CalcSlips (
    SlipId UNIQUEIDENTIFIER,
    PeriodId UNIQUEIDENTIFIER,
    PeriodName NVARCHAR(100),
    EmployeeId UNIQUEIDENTIFIER,
    BaseSalary DECIMAL(18,2),
    StandardWDays DECIMAL(18,2),
    WorkedDays DECIMAL(18,2),
    PaidLeaveDays DECIMAL(18,2),
    Allowances DECIMAL(18,2),
    Deductions DECIMAL(18,2),
    Gross DECIMAL(18,2),
    TotalDed DECIMAL(18,2),
    Net DECIMAL(18,2)
);

INSERT INTO #CalcSlips
SELECT 
    NEWID() AS SlipId,
    p.Id AS PeriodId,
    p.Name AS PeriodName,
    ep.EmployeeId,
    esp.BaseSalary,
    p.StandardWorkDays AS StandardWDays,
    -- Tính số ngày đi làm thực tế
    ISNULL(wd.WdCount, 0) AS WorkedDays,
    -- Tính số ngày nghỉ hưởng lương
    ISNULL(pl.PlCount, 0) AS PaidLeaveDays,
    -- Tổng phụ cấp
    ISNULL(al.TotalAllow, 0) AS Allowances,
    -- Tổng khấu trừ
    ISNULL(de.TotalDed, 0) AS Deductions,
    0, 0, 0 -- Sẽ update sau
FROM EmployeeProjections ep
JOIN EmployeeSalaryProjections esp ON ep.EmployeeId = esp.EmployeeId
CROSS JOIN PayrollPeriods p
-- Đi làm thực tế
LEFT JOIN (
    SELECT ap.EmployeeId, p.Id AS PeriodId, COUNT(DISTINCT ap.WorkDate) AS WdCount
    FROM AttendanceProjections ap
    CROSS JOIN PayrollPeriods p
    WHERE ap.WorkDate >= p.FromDate AND ap.WorkDate <= p.ToDate
    GROUP BY ap.EmployeeId, p.Id
) wd ON ep.EmployeeId = wd.EmployeeId AND p.Id = wd.PeriodId
-- Nghỉ hưởng lương
LEFT JOIN (
    SELECT lp.EmployeeId, p.Id AS PeriodId, SUM(lp.TotalDays) AS PlCount
    FROM LeaveProjections lp
    CROSS JOIN PayrollPeriods p
    WHERE lp.FromDate >= p.FromDate AND lp.ToDate <= p.ToDate AND lp.IsPaid = 1
    GROUP BY lp.EmployeeId, p.Id
) pl ON ep.EmployeeId = pl.EmployeeId AND p.Id = pl.PeriodId
-- Tổng phụ cấp
LEFT JOIN (
    SELECT ea.EmployeeId, ea.PayrollPeriodId, SUM(ea.Amount) AS TotalAllow
    FROM EmployeeAllowances ea
    GROUP BY ea.EmployeeId, ea.PayrollPeriodId
) al ON ep.EmployeeId = al.EmployeeId AND p.Id = al.PayrollPeriodId
-- Tổng khấu trừ
LEFT JOIN (
    SELECT ed.EmployeeId, ed.PayrollPeriodId, SUM(ed.Amount) AS TotalDed
    FROM EmployeeDeductions ed
    GROUP BY ed.EmployeeId, ed.PayrollPeriodId
) de ON ep.EmployeeId = de.EmployeeId AND p.Id = de.PayrollPeriodId
WHERE ep.EmployeeCode <> 'EMP000';

-- Cập nhật lương gộp, tổng khấu trừ và thực lĩnh
UPDATE #CalcSlips
SET 
    -- Lương gộp = (Lương cơ bản * (Ngày đi làm + Ngày nghỉ hưởng lương) / Ngày tiêu chuẩn) + Phụ cấp
    -- Lương gộp tối đa không vượt quá Lương cơ bản + Phụ cấp
    Gross = CAST(
        (BaseSalary * 
         CASE 
             WHEN (WorkedDays + PaidLeaveDays) > StandardWDays THEN StandardWDays 
             ELSE (WorkedDays + PaidLeaveDays) 
         END / StandardWDays
        ) + Allowances AS DECIMAL(18,2)
    ),
    TotalDed = Deductions;

UPDATE #CalcSlips
SET Net = Gross - TotalDed;

-- Insert dữ liệu vào bảng Payslips
INSERT INTO dbo.Payslips (Id, PayrollPeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, GrossSalary, TotalDeduction, NetSalary, Status, CreatedAt)
SELECT SlipId, PeriodId, EmployeeId, BaseSalary, WorkedDays, PaidLeaveDays, Gross, TotalDed, Net, 'Approved', GETUTCDATE()
FROM #CalcSlips;

PRINT 'Payslips seeded successfully';

-- Insert dữ liệu vào bảng PayslipItems cho từng Payslip
-- 1. Lương cơ bản làm việc thực tế
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT 
    NEWID(),
    SlipId,
    'Salary',
    'BASE_SALARY',
    N'Lương làm việc thực tế (' + CAST(CAST(WorkedDays + PaidLeaveDays AS INT) AS VARCHAR) + N'/' + CAST(CAST(StandardWDays AS INT) AS VARCHAR) + N' ngày)',
    CAST(BaseSalary * CASE WHEN (WorkedDays + PaidLeaveDays) > StandardWDays THEN StandardWDays ELSE (WorkedDays + PaidLeaveDays) END / StandardWDays AS INT),
    'AttendanceCalculation',
    GETUTCDATE()
FROM #CalcSlips;

-- 2. Phụ cấp ăn trưa
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), cs.SlipId, 'Allowance', 'ALLOW_EAT', N'Phụ cấp ăn trưa', ea.Amount, 'AllowanceSetting', GETUTCDATE()
FROM #CalcSlips cs
JOIN EmployeeAllowances ea ON cs.EmployeeId = ea.EmployeeId AND cs.PeriodId = ea.PayrollPeriodId
WHERE ea.AllowanceTypeId = @AllowEatId;

-- 3. Phụ cấp đi lại
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), cs.SlipId, 'Allowance', 'ALLOW_FUEL', N'Phụ cấp đi lại', ea.Amount, 'AllowanceSetting', GETUTCDATE()
FROM #CalcSlips cs
JOIN EmployeeAllowances ea ON cs.EmployeeId = ea.EmployeeId AND cs.PeriodId = ea.PayrollPeriodId
WHERE ea.AllowanceTypeId = @AllowFuelId;

-- 4. Phụ cấp điện thoại
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), cs.SlipId, 'Allowance', 'ALLOW_TEL', N'Phụ cấp điện thoại', ea.Amount, 'AllowanceSetting', GETUTCDATE()
FROM #CalcSlips cs
JOIN EmployeeAllowances ea ON cs.EmployeeId = ea.EmployeeId AND cs.PeriodId = ea.PayrollPeriodId
WHERE ea.AllowanceTypeId = @AllowTelId;

-- 5. Khấu trừ bảo hiểm xã hội
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), cs.SlipId, 'Deduction', 'DED_INS', N'Khấu trừ bảo hiểm xã hội (10.5%)', ed.Amount, 'DeductionSetting', GETUTCDATE()
FROM #CalcSlips cs
JOIN EmployeeDeductions ed ON cs.EmployeeId = ed.EmployeeId AND cs.PeriodId = ed.PayrollPeriodId
WHERE ed.DeductionTypeId = @DedInsId;

-- 6. Khấu trừ đi muộn
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), cs.SlipId, 'Deduction', 'DED_PENALTY', N'Khấu trừ đi muộn/vi phạm kỷ luật', ed.Amount, 'AttendancePenalty', GETUTCDATE()
FROM #CalcSlips cs
JOIN EmployeeDeductions ed ON cs.EmployeeId = ed.EmployeeId AND cs.PeriodId = ed.PayrollPeriodId
WHERE ed.DeductionTypeId = @DedPenaltyId;

-- 7. Khấu trừ Thuế TNCN
INSERT INTO dbo.PayslipItems (Id, PayslipId, ItemType, Code, Name, Amount, SourceType, CreatedAt)
SELECT NEWID(), cs.SlipId, 'Deduction', 'DED_TAX', N'Thuế thu nhập cá nhân tạm tính', ed.Amount, 'TaxSetting', GETUTCDATE()
FROM #CalcSlips cs
JOIN EmployeeDeductions ed ON cs.EmployeeId = ed.EmployeeId AND cs.PeriodId = ed.PayrollPeriodId
WHERE ed.DeductionTypeId = @DedTaxId;

PRINT 'PayslipItems seeded successfully';

-- ── 9. Clean Temp Tables ────────────────────────────────────
IF OBJECT_ID('tempdb..#PeriodDef') IS NOT NULL DROP TABLE #PeriodDef;
IF OBJECT_ID('tempdb..#CalcSlips') IS NOT NULL DROP TABLE #CalcSlips;
GO

-- ── Summary ──────────────────────────────────────────────
SELECT 'EmployeeProjections'     AS [Table], COUNT(*) AS [Count] FROM EmployeeProjections
UNION ALL SELECT 'SalaryProjections',COUNT(*) FROM EmployeeSalaryProjections
UNION ALL SELECT 'AttendanceProjections',COUNT(*) FROM AttendanceProjections WHERE WorkDate>='2025-07-01'
UNION ALL SELECT 'LeaveProjections',COUNT(*) FROM LeaveProjections WHERE FromDate>='2025-07-01'
UNION ALL SELECT 'PayrollPeriods',COUNT(*) FROM PayrollPeriods
UNION ALL SELECT 'Payslips',COUNT(*) FROM Payslips
UNION ALL SELECT 'PayslipItems',COUNT(*) FROM PayslipItems
UNION ALL SELECT 'AllowanceTypes',COUNT(*) FROM AllowanceTypes
UNION ALL SELECT 'DeductionTypes',COUNT(*) FROM DeductionTypes
UNION ALL SELECT 'EmployeeAllowances',COUNT(*) FROM EmployeeAllowances
UNION ALL SELECT 'EmployeeDeductions',COUNT(*) FROM EmployeeDeductions;
GO
