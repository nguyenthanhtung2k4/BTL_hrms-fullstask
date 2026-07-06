using System;
using System.Collections.Generic;

namespace Hrms.HrCore.Application.Dtos;

public record CreateUserDto(
    Guid EmployeeId,
    string Email,
    string Password,
    List<string> Roles
);

public record UpdateUserRolesDto(
    List<string> Roles
);

public record ResetPasswordDto(
    string NewPassword
);

public record ChangeUserStatusDto(
    bool IsActive
);

public record UserDto(
    Guid Id,
    Guid? EmployeeId,
    string Email,
    bool IsActive,
    List<string> Roles,
    DateTime? LastLoginAt
);
