using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.PayrollReport.Controllers;

[ApiController]
[Route("api/v1/payroll/payslips")]
[Authorize]
public class PayslipsController : ControllerBase
{
    private readonly IPayslipService _payslipService;

    public PayslipsController(IPayslipService payslipService)
    {
        _payslipService = payslipService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayslipDto>>>> GetPayslips(
        [FromQuery] Guid? periodId,
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId)
    {
        // Enforce role permission: regular employee can only query their own payslips
        if (!User.IsInRole("Admin") && !User.IsInRole("PayrollStaff") && !User.IsInRole("HR") && !User.IsInRole("Manager"))
        {
            var currentEmployeeId = GetCurrentEmployeeId();
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

        var result = await _payslipService.GetPayslipsAsync(periodId, employeeId, departmentId);
        return Ok(ApiResponse<IEnumerable<PayslipDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayslipDto>>>> GetMyPayslips()
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            // Return empty list instead of failing with 400 Bad Request to handle non-employee accounts (like admin) gracefully
            return Ok(ApiResponse<IEnumerable<PayslipDto>>.Ok(Array.Empty<PayslipDto>(), "User is not associated with an Employee account."));
        }

        var result = await _payslipService.GetPayslipsAsync(null, employeeId, null);
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

        // Enforce role permission: regular employee can only view their own payslip
        if (!User.IsInRole("Admin") && !User.IsInRole("PayrollStaff") && !User.IsInRole("HR") && !User.IsInRole("Manager"))
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (result.Value!.EmployeeId != currentEmployeeId)
            {
                return Forbid();
            }
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
