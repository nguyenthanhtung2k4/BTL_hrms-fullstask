using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.PayrollReport.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hrms.PayrollReport.Controllers;

[ApiController]
[Route("api/v1/payroll/payslips")]
[Authorize]
public class PayslipsController : ControllerBase
{
    private readonly IPayslipService _payslipService;
    private readonly PayrollReportDbContext _dbContext;

    public PayslipsController(IPayslipService payslipService, PayrollReportDbContext dbContext)
    {
        _payslipService = payslipService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayslipDto>>>> GetPayslips(
        [FromQuery] Guid? periodId,
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId)
    {
        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("PayrollStaff");
        var isManager = User.IsInRole("Manager");
        var currentEmployeeId = GetCurrentEmployeeId();

        if (!isSuperPrivileged)
        {
            if (isManager)
            {
                var managerEmp = await _dbContext.EmployeeProjections.FindAsync(currentEmployeeId);
                var managerDeptId = managerEmp?.DepartmentId;

                if (employeeId.HasValue)
                {
                    var targetEmp = await _dbContext.EmployeeProjections.FindAsync(employeeId.Value);
                    if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != currentEmployeeId))
                    {
                        return Forbid();
                    }
                }
                else
                {
                    if (departmentId.HasValue && departmentId.Value != managerDeptId)
                    {
                        return Forbid();
                    }
                    departmentId = managerDeptId;
                }
            }
            else // Employee
            {
                if (currentEmployeeId == Guid.Empty)
                {
                    return Forbid();
                }
                if (employeeId.HasValue && employeeId.Value != currentEmployeeId)
                {
                    return Forbid();
                }
                employeeId = currentEmployeeId;
            }
        }

        var result = await _payslipService.GetPayslipsAsync(periodId, employeeId, departmentId);
        return Ok(ApiResponse<IEnumerable<PayslipDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayslipDto>>>> GetMyPayslips()
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return Ok(ApiResponse<IEnumerable<PayslipDto>>.Ok(Array.Empty<PayslipDto>(), "User is not associated with an Employee account."));
        }

        var result = await _payslipService.GetPayslipsAsync(null, employeeId, null, onlyCalculatedOrClosed: true);
        return Ok(ApiResponse<IEnumerable<PayslipDto>>.Ok(result.Value ?? Array.Empty<PayslipDto>(), result.Message));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PayslipDto>>> GetById(Guid id)
    {
        var result = await _payslipService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<PayslipDto>.Fail(result.Errors, result.Message));
        }

        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("PayrollStaff");
        var isManager = User.IsInRole("Manager");

        if (!isSuperPrivileged)
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == Guid.Empty)
            {
                return Forbid();
            }

            if (isManager)
            {
                var managerEmp = await _dbContext.EmployeeProjections.FindAsync(currentEmployeeId);
                var managerDeptId = managerEmp?.DepartmentId;

                var targetEmp = await _dbContext.EmployeeProjections.FindAsync(result.Value!.EmployeeId);
                if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != currentEmployeeId && targetEmp.Id != currentEmployeeId))
                {
                    return Forbid();
                }
            }
            else // Employee
            {
                if (result.Value!.EmployeeId != currentEmployeeId)
                {
                    return Forbid();
                }
            }
        }

        return Ok(ApiResponse<PayslipDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR,Manager,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<PayslipDto>>> Update(Guid id, [FromBody] UpdatePayslipDto request)
    {
        var isSuperPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("PayrollStaff");
        if (!isSuperPrivileged && User.IsInRole("Manager"))
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == Guid.Empty)
            {
                return Forbid();
            }

            var managerEmp = await _dbContext.EmployeeProjections.FindAsync(currentEmployeeId);
            var managerDeptId = managerEmp?.DepartmentId;

            var existingPayslip = await _payslipService.GetByIdAsync(id);
            if (existingPayslip.IsFailure)
            {
                return NotFound(ApiResponse<PayslipDto>.Fail(existingPayslip.Errors, existingPayslip.Message));
            }

            var targetEmp = await _dbContext.EmployeeProjections.FindAsync(existingPayslip.Value!.EmployeeId);
            if (targetEmp == null || (targetEmp.DepartmentId != managerDeptId && targetEmp.ManagerEmployeeId != currentEmployeeId))
            {
                return Forbid();
            }
        }

        var result = await _payslipService.UpdatePayslipAsync(id, request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PayslipDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayslipDto>.Ok(result.Value!, result.Message));
    }

    private Guid GetCurrentEmployeeId()
    {
        var claimValue = User.FindFirst("employeeId")?.Value;
        if (Guid.TryParse(claimValue, out var employeeId))
        {
            return employeeId;
        }
        return Guid.Empty;
    }
}
