using System;

namespace Hrms.HrCore.Application.Dtos;

public record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string? Phone,
    string? Gender,
    DateTime? DateOfBirth,
    DateTime HireDate,
    Guid DepartmentId,
    string DepartmentName,
    Guid PositionId,
    string PositionName,
    Guid? ManagerEmployeeId,
    string? ManagerName,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateEmployeeDto(
    string EmployeeCode,
    string FullName,
    string Email,
    string? Phone,
    string? Gender,
    DateTime? DateOfBirth,
    DateTime HireDate,
    Guid DepartmentId,
    Guid PositionId,
    Guid? ManagerEmployeeId
);

public record UpdateEmployeeDto(
    string FullName,
    string? Phone,
    string? Gender,
    DateTime? DateOfBirth,
    DateTime HireDate,
    Guid DepartmentId,
    Guid PositionId,
    Guid? ManagerEmployeeId,
    string Status
);

public record ChangeStatusDto(
    string NewStatus,
    string? Reason,
    Guid ChangedByUserId
);
