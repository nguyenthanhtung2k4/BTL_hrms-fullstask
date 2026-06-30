using System;
using System.Linq;
using System.Threading.Tasks;
using Hrms.PayrollReport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hrms.PayrollReport.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(PayrollReportDbContext context)
    {
        // Ensure database tables exist
        await context.Database.EnsureCreatedAsync();

        // Migrate HireDate column if it does not exist
        await context.Database.ExecuteSqlRawAsync(
            "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmployeeProjections') AND name = 'HireDate') " +
            "ALTER TABLE dbo.EmployeeProjections ADD HireDate DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME();"
        );

        // Migrate IsDeleted column if it does not exist
        await context.Database.ExecuteSqlRawAsync(
            "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmployeeProjections') AND name = 'IsDeleted') " +
            "BEGIN " +
            "    ALTER TABLE dbo.EmployeeProjections ADD IsDeleted BIT NOT NULL DEFAULT 0; " +
            "END"
        );

        // 1. Seed Allowance Types
        var allowanceTypes = new[]
        {
            new AllowanceType { Id = Guid.NewGuid(), Code = "ALLOWANCE_TRAVEL", Name = "Phụ cấp đi lại", IsActive = true },
            new AllowanceType { Id = Guid.NewGuid(), Code = "ALLOWANCE_MEAL", Name = "Phụ cấp ăn trưa", IsActive = true },
            new AllowanceType { Id = Guid.NewGuid(), Code = "ALLOWANCE_TELEPHONE", Name = "Phụ cấp điện thoại", IsActive = true }
        };

        foreach (var allowance in allowanceTypes)
        {
            if (!await context.AllowanceTypes.AnyAsync(a => a.Code == allowance.Code))
            {
                await context.AllowanceTypes.AddAsync(allowance);
            }
        }

        // 2. Seed Deduction Types
        var deductionTypes = new[]
        {
            new DeductionType { Id = Guid.NewGuid(), Code = "DEDUCTION_SOCIAL_INSURANCE", Name = "Bảo hiểm xã hội (8%)", IsActive = true },
            new DeductionType { Id = Guid.NewGuid(), Code = "DEDUCTION_HEALTH_INSURANCE", Name = "Bảo hiểm y tế (1.5%)", IsActive = true },
            new DeductionType { Id = Guid.NewGuid(), Code = "DEDUCTION_UNEMPLOYMENT_INSURANCE", Name = "Bảo hiểm thất nghiệp (1%)", IsActive = true },
            new DeductionType { Id = Guid.NewGuid(), Code = "DEDUCTION_PERSONAL_INCOME_TAX", Name = "Thuế thu nhập cá nhân", IsActive = true },
            new DeductionType { Id = Guid.NewGuid(), Code = "DEDUCTION_LATE", Name = "Khấu trừ đi muộn", IsActive = true }
        };

        foreach (var deduction in deductionTypes)
        {
            if (!await context.DeductionTypes.AnyAsync(d => d.Code == deduction.Code))
            {
                await context.DeductionTypes.AddAsync(deduction);
            }
        }

        // 3. Seed Default Payroll Rule
        if (!await context.PayrollRules.AnyAsync())
        {
            await context.PayrollRules.AddAsync(new PayrollRule
            {
                Id = Guid.NewGuid(),
                Code = "RULE_STANDARD",
                Name = "Quy tắc tính lương tiêu chuẩn",
                WorkDayHours = 8,
                PaidLeaveCountsAsWork = true,
                OvertimeRate = 1.5m,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();
    }
}
