using System;
using System.Collections.Generic;

namespace Hrms.HrCore.Application.Dtos;

public record UserInfoDto(
    Guid Id,
    string Email,
    string FullName,
    Guid? EmployeeId,
    List<string> Roles
);
