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
        var result = await _payslipService.GetPayslipsAsync(periodId, employeeId, departmentId);
        return Ok(ApiResponse<IEnumerable<PayslipDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<PayslipDto>>> GetMyPayslip([FromQuery] Guid periodId)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == Guid.Empty)
        {
            return BadRequest(ApiResponse<PayslipDto>.Fail("InvalidUser", "User must be associated with an active Employee account."));
        }

        var result = await _payslipService.GetMyPayslipAsync(employeeId, periodId);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<PayslipDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<PayslipDto>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PayslipDto>>> GetById(Guid id)
    {
        var result = await _payslipService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<PayslipDto>.Fail(result.Errors, result.Message));
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
