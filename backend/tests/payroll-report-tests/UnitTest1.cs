using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hrms.PayrollReport.Infrastructure.Persistence;
using Hrms.PayrollReport.Domain.Entities;
using Hrms.PayrollReport.Application.Services;
using Xunit;

namespace payroll_report_tests;

public class PayrollCalculationTests
{
    private PayrollReportDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PayrollReportDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PayrollReportDbContext(options);
    }

    [Fact]
    public async Task CalculatePeriodPayslipsAsync_ShouldComputeTaxAndInsuranceCorrectly()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PayslipService(context);

        // 1. Create a PayrollRule
        var rule = new PayrollRule
        {
            Id = Guid.NewGuid(),
            Code = "STANDARD_RULE",
            Name = "Quy tắc chuẩn",
            WorkDayHours = 8m,
            PaidLeaveCountsAsWork = true
        };
        context.PayrollRules.Add(rule);

        // 2. Create a PayrollPeriod
        var periodId = Guid.NewGuid();
        var period = new PayrollPeriod
        {
            Id = periodId,
            Code = "PERIOD_2026_06",
            Name = "Kỳ lương Tháng 6/2026",
            PayrollRuleId = rule.Id,
            FromDate = new DateOnly(2026, 6, 1),
            ToDate = new DateOnly(2026, 6, 30),
            StandardWorkDays = 22,
            Status = "Draft"
        };
        context.PayrollPeriods.Add(period);

        // 3. Create Department and Position
        var dept = new DepartmentProjection
        {
            Id = Guid.NewGuid(),
            Code = "TECH",
            Name = "Technology Division",
            IsActive = true
        };
        var pos = new PositionProjection
        {
            Id = Guid.NewGuid(),
            Code = "SE",
            Name = "Software Engineer",
            IsActive = true
        };
        context.DepartmentProjections.Add(dept);
        context.PositionProjections.Add(pos);

        // 4. Create Employee (Hired 5 years ago, so they qualify for Seniority Allowance)
        var employeeId = Guid.NewGuid();
        var employee = new EmployeeProjection
        {
            Id = employeeId,
            EmployeeCode = "EMP001",
            FullName = "Nguyễn Văn A",
            Email = "vana@hrms.com",
            DepartmentId = dept.Id,
            PositionId = pos.Id,
            HireDate = new DateTime(2021, 6, 1), // 5 years before June 2026
            Status = "Active"
        };
        context.EmployeeProjections.Add(employee);

        // 5. Create EmployeeSalaryProjection (Base Salary: 50,000,000 VND)
        var salary = new EmployeeSalaryProjection
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ContractId = Guid.NewGuid(),
            BaseSalary = 50000000m,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Active"
        };
        context.EmployeeSalaryProjections.Add(salary);

        // 6. Create Attendance Projections (22 days * 8 hours = 176 hours = 10560 worked minutes)
        // This will result in exactly 22 worked days.
        var attendance = new AttendanceProjection
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            WorkDate = new DateOnly(2026, 6, 1),
            WorkedMinutes = 10560, // 22 days * 8 hours * 60 minutes
            Status = "Completed"
        };
        context.AttendanceProjections.Add(attendance);

        await context.SaveChangesAsync();

        // Act
        var result = await service.CalculatePeriodPayslipsAsync(periodId);

        // Assert
        Assert.True(result.IsSuccess);

        var payslip = await context.Payslips
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PayrollPeriodId == periodId);

        Assert.NotNull(payslip);
        Assert.Equal(50000000m, payslip.BaseSalary);
        Assert.Equal(22m, payslip.WorkedDays);

        // Seniority allowance should be: 5 years * 200,000 VND = 1,000,000 VND
        var seniorityAllowance = payslip.Items.FirstOrDefault(i => i.Code == "ALLOWANCE_SENIORITY");
        Assert.NotNull(seniorityAllowance);
        Assert.Equal(1000000m, seniorityAllowance.Amount);

        // Total Gross = 50,000,000 (Base) + 1,000,000 (Seniority) = 51,000,000 VND
        Assert.Equal(51000000m, payslip.GrossSalary);

        // Insurances check:
        // baseSalary = 50,000,000 VND.
        // Ceiling for Social and Health is 46,800,000 VND.
        // Ceiling for Unemployment is 99,200,000 VND.
        // BHXH = 46,800,000 * 8% = 3,744,000 VND
        // BHYT = 46,800,000 * 1.5% = 702,000 VND
        // BHTN = 50,000,000 * 1% = 500,000 VND
        // Total compulsory insurance = 3,744,000 + 702,000 + 500,000 = 4,946,000 VND
        var bhxhItem = payslip.Items.FirstOrDefault(i => i.Code == "DEDUCTION_BHXH");
        var bhytItem = payslip.Items.FirstOrDefault(i => i.Code == "DEDUCTION_BHYT");
        var bhtnItem = payslip.Items.FirstOrDefault(i => i.Code == "DEDUCTION_BHTN");

        Assert.NotNull(bhxhItem);
        Assert.Equal(3744000m, bhxhItem.Amount);
        Assert.NotNull(bhytItem);
        Assert.Equal(702000m, bhytItem.Amount);
        Assert.NotNull(bhtnItem);
        Assert.Equal(500000m, bhtnItem.Amount);

        // Tax calculation check:
        // Gross = 51,000,000
        // Compulsory insurances = 4,946,000
        // Personal deduction = 11,000,000
        // Assessable income = 51,000,000 - 4,946,000 - 11,000,000 = 35,054,000 VND
        // PIT tax brackets:
        // Bracket 1: 5,000,000 * 5% = 250,000 VND
        // Bracket 2: 5,000,000 * 10% = 500,000 VND
        // Bracket 3: 8,000,000 * 15% = 1,200,000 VND
        // Bracket 4: 14,000,000 * 20% = 2,800,000 VND
        // Bracket 5: (35,054,000 - 32,000,000) * 25% = 3,054,000 * 25% = 763,500 VND
        // Total PIT = 250,000 + 500,000 + 1,200,000 + 2,800,000 + 763,500 = 5,513,500 VND
        var pitItem = payslip.Items.FirstOrDefault(i => i.Code == "DEDUCTION_PIT");
        Assert.NotNull(pitItem);
        Assert.Equal(5513500m, pitItem.Amount);

        // Total Deduction = Insurances (4,946,000) + PIT (5,513,500) = 10,459,500 VND
        Assert.Equal(10459500m, payslip.TotalDeduction);

        // Net Salary = 51,000,000 - 10,459,500 = 40,540,500 VND
        Assert.Equal(40540500m, payslip.NetSalary);
    }
}
