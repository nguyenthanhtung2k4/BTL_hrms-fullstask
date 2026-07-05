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
[Route("api/v1/hr/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<UserDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<UserDto>.Ok(result.Value!, result.Message));
    }

    [HttpGet("employee/{employeeId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetByEmployeeId(Guid employeeId)
    {
        var result = await _userService.GetByEmployeeIdAsync(employeeId);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<UserDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<UserDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateUserAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(result.Errors, result.Message));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<UserDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateRoles(Guid id, [FromBody] UpdateUserRolesDto dto)
    {
        var result = await _userService.UpdateRolesAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<UserDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id:guid}/password")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
    {
        var result = await _userService.ResetPasswordAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> ChangeStatus(Guid id, [FromBody] ChangeUserStatusDto dto)
    {
        var result = await _userService.ChangeStatusAsync(id, dto.IsActive);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }
}
