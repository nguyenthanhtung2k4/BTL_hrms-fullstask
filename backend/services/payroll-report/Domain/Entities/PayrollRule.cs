using System;
using Hrms.Shared.Domain;

namespace Hrms.PayrollReport.Domain.Entities;

public class PayrollRule : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal WorkDayHours { get; set; } = 8;
    public bool PaidLeaveCountsAsWork { get; set; } = true;

    // ĐÃ FIX: Tách hệ số OT thành 3 loại ngày khác nhau để khớp với Frontend và yêu cầu QA
    public decimal OtMultiplierWeekday { get; set; } = 1.5m; // Ngày thường (150%)
    public decimal OtMultiplierWeekend { get; set; } = 2.0m; // Ngày nghỉ (200%)
    public decimal OtMultiplierHoliday { get; set; } = 3.0m; // Ngày lễ (300%)

    public bool IsActive { get; set; } = true;

    // Các cấu hình Ngày công chuẩn và Phạt đi muộn đã có sẵn
    public decimal StandardWorkingDays { get; set; }
    public decimal LatePenaltyRule { get; set; }
    public decimal OvertimeRate { get; set; }
}