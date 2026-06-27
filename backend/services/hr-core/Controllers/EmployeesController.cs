using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.HrCore.Controllers;

[ApiController]
[Route("api/v1/hr/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

[HttpGet]
[Authorize(Roles = "Admin,HR,Manager,PayrollStaff,Employee")]
public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetAll()
{
    var result = await _employeeService.GetAllAsync(User);
    return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Ok(result.Value!, result.Message));
}

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(Guid id)
    {
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Manager") || User.IsInRole("PayrollStaff");
        if (!isPrivileged)
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var employeeId) || employeeId != id)
            {
                return Forbid();
            }
        }

        var result = await _employeeService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<EmployeeDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<EmployeeDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create([FromBody] CreateEmployeeDto dto)
    {
        var result = await _employeeService.CreateAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<EmployeeDto>.Fail(result.Errors, result.Message));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<EmployeeDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(Guid id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await _employeeService.UpdateAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<EmployeeDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<EmployeeDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse>> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        var result = await _employeeService.ChangeStatusAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _employeeService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }
}
