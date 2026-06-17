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
[Route("api/v1/payroll/allowances")]
[Authorize]
public class AllowancesController : ControllerBase
{
    private readonly IAllowanceService _allowanceService;

    public AllowancesController(IAllowanceService allowanceService)
    {
        _allowanceService = allowanceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeAllowanceDto>>>> GetAllowances(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? periodId)
    {
        var result = await _allowanceService.GetAllowancesAsync(employeeId, periodId);
        return Ok(ApiResponse<IEnumerable<EmployeeAllowanceDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AllowanceTypeDto>>>> GetAllowanceTypes()
    {
        var result = await _allowanceService.GetAllowanceTypesAsync();
        return Ok(ApiResponse<IEnumerable<AllowanceTypeDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeAllowanceDto>>> GetById(Guid id)
    {
        var result = await _allowanceService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<EmployeeAllowanceDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<EmployeeAllowanceDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeAllowanceDto>>> Create([FromBody] CreateEmployeeAllowanceDto request)
    {
        var result = await _allowanceService.CreateAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<EmployeeAllowanceDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<EmployeeAllowanceDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeAllowanceDto>>> Update(Guid id, [FromBody] UpdateEmployeeAllowanceDto request)
    {
        var result = await _allowanceService.UpdateAsync(id, request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<EmployeeAllowanceDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<EmployeeAllowanceDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _allowanceService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }
}
