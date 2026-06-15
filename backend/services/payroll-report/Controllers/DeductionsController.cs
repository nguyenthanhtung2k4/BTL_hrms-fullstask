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
        var result = await _deductionService.GetDeductionsAsync(employeeId, periodId);
        return Ok(ApiResponse<IEnumerable<EmployeeDeductionDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DeductionTypeDto>>>> GetDeductionTypes()
    {
        var result = await _deductionService.GetDeductionTypesAsync();
        return Ok(ApiResponse<IEnumerable<DeductionTypeDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeDeductionDto>>> GetById(Guid id)
    {
        var result = await _deductionService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<EmployeeDeductionDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<EmployeeDeductionDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
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
