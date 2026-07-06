using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.Contracts.Events;
using Hrms.Shared.Domain;
using Hrms.Attendance.Application.Dtos;
using Hrms.Attendance.Application.Interfaces;
using Hrms.Attendance.Domain.Entities;
using Hrms.Attendance.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hrms.Attendance.Application.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly AttendanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public LeaveRequestService(AttendanceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<IEnumerable<LeaveTypeDto>>> GetLeaveTypesAsync()
    {
        var leaveTypes = await _dbContext.LeaveTypes.Where(t => t.IsActive).ToListAsync();
        var dtos = leaveTypes.Select(t => new LeaveTypeDto(t.Id, t.Code, t.Name, t.IsPaid, t.IsActive));
        return Result<IEnumerable<LeaveTypeDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetPersonalRequestsAsync(Guid employeeId)
    {
        return await GetRequestsAsync(null, null, null, null, employeeId);
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetRequestsAsync(string? status, Guid? departmentId, DateOnly? fromDate, DateOnly? toDate)
    {
        return await GetRequestsAsync(status, departmentId, fromDate, toDate, null);
    }

    private async Task<Result<IEnumerable<LeaveRequestDto>>> GetRequestsAsync(string? status, Guid? departmentId, DateOnly? fromDate, DateOnly? toDate, Guid? employeeId)
    {
        var query = _dbContext.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Include(l => l.ApprovedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(l => l.Status == status);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(l => l.Employee.DepartmentId == departmentId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.FromDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.ToDate <= toDate.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        }

        var requests = await query.ToListAsync();
        var dtos = requests.Select(MapToDto);
        return Result<IEnumerable<LeaveRequestDto>>.Success(dtos);
    }

    private async Task<LeaveBalance> GetOrCreateLeaveBalanceAsync(Guid employeeId, Guid leaveTypeId, int year)
    {
        var balance = await _dbContext.LeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year);

        var leaveType = await _dbContext.LeaveTypes.FindAsync(leaveTypeId);
        decimal entitledDays = 12; // Default 12 days for annual leave
        if (leaveType != null)
        {
            if (leaveType.Code == "NTS") entitledDays = 180; // 180 days for maternity leave
            else if (leaveType.Code == "NO") entitledDays = 30; // 30 days for sick leave
            else if (leaveType.Code == "NKL") entitledDays = 365; // 365 days for unpaid leave
        }

        if (balance == null)
        {
            balance = new LeaveBalance
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                Year = year,
                EntitledDays = entitledDays,
                UsedDays = 0,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.LeaveBalances.Add(balance);
            await _dbContext.SaveChangesAsync();
        }
        else if (balance.EntitledDays == 12 && entitledDays != 12)
        {
            balance.EntitledDays = entitledDays;
            await _dbContext.SaveChangesAsync();
        }

        return balance;
    }

    public async Task<Result<LeaveRequestDto>> CreateAsync(Guid employeeId, CreateLeaveRequestDto dto)
    {
        var employee = await _dbContext.EmployeeProjections.FindAsync(employeeId);
        if (employee == null) return Result<LeaveRequestDto>.Failure("Employee not found.");

        if (employee.Status != "Active")
        {
            return Result<LeaveRequestDto>.Failure("Cannot request leave. Employee status is not Active.");
        }

        var leaveType = await _dbContext.LeaveTypes.FindAsync(dto.LeaveTypeId);
        if (leaveType == null || !leaveType.IsActive)
        {
            return Result<LeaveRequestDto>.Failure("Selected leave type is invalid or inactive.");
        }

        if (dto.ToDate < dto.FromDate)
        {
            return Result<LeaveRequestDto>.Failure("To date must be on or after from date.");
        }

        // Calculate days
        decimal totalDays = (dto.ToDate.DayNumber - dto.FromDate.DayNumber) + 1;

        // Check leave balance
        var year = dto.FromDate.Year;
        var balance = await GetOrCreateLeaveBalanceAsync(employeeId, dto.LeaveTypeId, year);
        if (balance.RemainingDays < totalDays)
        {
            return Result<LeaveRequestDto>.Failure($"Không đủ ngày phép khả dụng cho năm {year}. Số ngày yêu cầu: {totalDays}, Số ngày còn lại: {balance.RemainingDays}.");
        }

        var request = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = dto.LeaveTypeId,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            TotalDays = totalDays,
            Reason = dto.Reason,
            Status = "Pending"
        };

        _dbContext.LeaveRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(request.Id);
    }

    public async Task<Result<LeaveRequestDto>> GetByIdAsync(Guid id)
    {
        var request = await _dbContext.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Include(l => l.ApprovedBy)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (request == null) return Result<LeaveRequestDto>.Failure("Leave request not found.");
        return Result<LeaveRequestDto>.Success(MapToDto(request));
    }

    public async Task<Result<LeaveRequestDto>> ApproveAsync(Guid id, Guid approvedByEmployeeId)
    {
        var request = await _dbContext.LeaveRequests
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (request == null) return Result<LeaveRequestDto>.Failure("Leave request not found.");

        if (request.Status != "Pending")
        {
            return Result<LeaveRequestDto>.Failure($"Cannot approve leave request with status '{request.Status}'.");
        }

        var approver = await _dbContext.EmployeeProjections.FindAsync(approvedByEmployeeId);
        if (approver == null) return Result<LeaveRequestDto>.Failure("Approver employee not found.");

        // Deduct from leave balance
        var year = request.FromDate.Year;
        var balance = await GetOrCreateLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId, year);
        if (balance.RemainingDays < request.TotalDays)
        {
            return Result<LeaveRequestDto>.Failure($"Không đủ ngày phép khả dụng để duyệt. Số ngày yêu cầu: {request.TotalDays}, Số ngày còn lại: {balance.RemainingDays}.");
        }

        balance.UsedDays += request.TotalDays;

        request.Status = "Approved";
        request.ApprovedByEmployeeId = approvedByEmployeeId;
        request.ApprovedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Publish event
        var integrationEvent = new IntegrationEvent<LeaveApprovedPayload>(
            EventId: Guid.NewGuid(),
            EventName: EventNames.LeaveApproved,
            Version: 1,
            OccurredAt: DateTimeOffset.UtcNow,
            SourceService: "attendance",
            CorrelationId: null,
            Payload: new LeaveApprovedPayload(
                request.Id,
                request.EmployeeId,
                request.FromDate,
                request.ToDate,
                request.TotalDays,
                request.LeaveType.Code,
                request.LeaveType.IsPaid,
                approvedByEmployeeId
            )
        );
        await _publishEndpoint.Publish(integrationEvent);

        return await GetByIdAsync(id);
    }

    public async Task<Result<LeaveRequestDto>> RejectAsync(Guid id, Guid approvedByEmployeeId)
    {
        var request = await _dbContext.LeaveRequests.FindAsync(id);
        if (request == null) return Result<LeaveRequestDto>.Failure("Leave request not found.");

        if (request.Status != "Pending")
        {
            return Result<LeaveRequestDto>.Failure($"Cannot reject leave request with status '{request.Status}'.");
        }

        var approver = await _dbContext.EmployeeProjections.FindAsync(approvedByEmployeeId);
        if (approver == null) return Result<LeaveRequestDto>.Failure("Approver employee not found.");

        request.Status = "Rejected";
        request.ApprovedByEmployeeId = approvedByEmployeeId;
        request.ApprovedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<Result<LeaveRequestDto>> CancelAsync(Guid id, Guid employeeId)
    {
        var request = await _dbContext.LeaveRequests.FindAsync(id);
        if (request == null) return Result<LeaveRequestDto>.Failure("Leave request not found.");

        if (request.EmployeeId != employeeId)
        {
            return Result<LeaveRequestDto>.Failure("Cannot cancel leave request belonging to another employee.");
        }

        if (request.Status != "Pending")
        {
            return Result<LeaveRequestDto>.Failure($"Cannot cancel leave request that is already '{request.Status}'.");
        }

        request.Status = "Cancelled";
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private static LeaveRequestDto MapToDto(LeaveRequest l)
    {
        return new LeaveRequestDto(
            l.Id,
            l.EmployeeId,
            l.Employee.FullName,
            l.LeaveTypeId,
            l.LeaveType.Name,
            l.LeaveType.IsPaid,
            l.FromDate,
            l.ToDate,
            l.TotalDays,
            l.Reason,
            l.Status,
            l.ApprovedByEmployeeId,
            l.ApprovedBy?.FullName,
            l.ApprovedAt,
            l.CreatedAt,
            l.UpdatedAt
        );
    }
}
