using System;

namespace Hrms.HrCore.Application.Dtos;

public record DepartmentDto(
    Guid Id,
    string Code,
    string Name,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    Guid? ManagerEmployeeId,
    string? ManagerName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateDepartmentDto(
    string Code,
    string Name,
    Guid? ParentDepartmentId,
    Guid? ManagerEmployeeId
);

public record UpdateDepartmentDto(
    string Name,
    Guid? ParentDepartmentId,
    Guid? ManagerEmployeeId,
    bool IsActive
);
