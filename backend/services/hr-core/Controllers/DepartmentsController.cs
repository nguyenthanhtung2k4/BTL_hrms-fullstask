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
[Route("api/v1/hr/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetAll()
    {
        var result = await _departmentService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById(Guid id)
    {
        var result = await _departmentService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<DepartmentDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<DepartmentDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create([FromBody] CreateDepartmentDto dto)
    {
        var result = await _departmentService.CreateAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<DepartmentDto>.Fail(result.Errors, result.Message));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<DepartmentDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var result = await _departmentService.UpdateAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<DepartmentDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<DepartmentDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _departmentService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpGet("my")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetMyDepartments()
    {
        var employeeIdClaim = User.FindFirst("employeeId")?.Value;
        if (!Guid.TryParse(employeeIdClaim, out var managerId))
            return Forbid();

        var result = await _departmentService.GetMyDepartmentsAsync(managerId);
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.Ok(result.Value!, result.Message));
    }
}
