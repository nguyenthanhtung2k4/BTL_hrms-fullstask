using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.PayrollReport.Controllers;

[ApiController]
[Route("api/v1/payroll/deductions")]
[Authorize]
public class DeductionsController : ControllerBase
{
    private readonly IDeductionService _deductionService;

    public DeductionsController(IDeductionService deductionService)
    {
        _deductionService = deductionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDeductionDto>>>> GetDeductions(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? periodId)
    {
        // Check permissions: Admin, HR, Manager, PayrollStaff can query any employee.
        // Employee can only query their own employeeId.
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Manager") || User.IsInRole("PayrollStaff");
        if (!isPrivileged)
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var claimGuid))
            {
                return Forbid();
            }
            employeeId = claimGuid; // Force to their own ID
        }

        var result = await _deductionService.GetDeductionsAsync(employeeId, periodId);
        return Ok(ApiResponse<IEnumerable<EmployeeDeductionDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DeductionTypeDto>>>> GetDeductionTypes()
    {
        var result = await _deductionService.GetDeductionTypesAsync();
        return Ok(ApiResponse<IEnumerable<DeductionTypeDto>>.Ok(result.Value!, result.Message));
    }

    [HttpPost("types")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<DeductionTypeDto>>> CreateDeductionType([FromBody] CreateTypeRequest request)
    {
        var result = await _deductionService.CreateDeductionTypeAsync(request.Name);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<DeductionTypeDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<DeductionTypeDto>.Ok(result.Value!, result.Message));
    }

    public record CreateTypeRequest(string Name);

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeDeductionDto>>> GetById(Guid id)
    {
        var result = await _deductionService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<EmployeeDeductionDto>.Fail(result.Errors, result.Message));
        }

        // Employee can only read their own deduction record
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Manager") || User.IsInRole("PayrollStaff");
        if (!isPrivileged)
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var claimGuid) || result.Value!.EmployeeId != claimGuid)
            {
                return Forbid();
            }
        }

        return Ok(ApiResponse<EmployeeDeductionDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<EmployeeDeductionDto>>> Create([FromBody] CreateEmployeeDeductionDto request)
    {
        var result = await _deductionService.CreateAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<EmployeeDeductionDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<EmployeeDeductionDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<EmployeeDeductionDto>>> Update(Guid id, [FromBody] UpdateEmployeeDeductionDto request)
    {
        var result = await _deductionService.UpdateAsync(id, request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<EmployeeDeductionDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<EmployeeDeductionDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _deductionService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }
}
