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
[Route("api/v1/payroll/payroll-periods")]
[Authorize]
public class PayrollPeriodsController : ControllerBase
{
    private readonly IPayrollPeriodService _periodService;

    public PayrollPeriodsController(IPayrollPeriodService periodService)
    {
        _periodService = periodService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR,Manager,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayrollPeriodDto>>>> GetAll()
    {
        var result = await _periodService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PayrollPeriodDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,HR,Manager,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> GetById(Guid id)
    {
        var result = await _periodService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<PayrollPeriodDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollPeriodDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> Create([FromBody] CreatePayrollPeriodDto request)
    {
        var result = await _periodService.CreateAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PayrollPeriodDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollPeriodDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> Update(Guid id, [FromBody] UpdatePayrollPeriodDto request)
    {
        var result = await _periodService.UpdateAsync(id, request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PayrollPeriodDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollPeriodDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _periodService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpPost("{id}/calculate")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse>> Calculate(Guid id)
    {
        var result = await _periodService.CalculateAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpPost("{id}/close")]
    [Authorize(Roles = "Admin,PayrollStaff")]
    public async Task<ActionResult<ApiResponse>> Close(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid closedBy = Guid.Empty;
        if (Guid.TryParse(userIdString, out var parsedId))
        {
            closedBy = parsedId;
        }

        var result = await _periodService.CloseAsync(id, closedBy);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }
}
