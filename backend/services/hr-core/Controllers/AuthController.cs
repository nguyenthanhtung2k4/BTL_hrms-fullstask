using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hrms.HrCore.Controllers;

[ApiController]
[Route("api/v1/hr/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly HrDbContext _dbContext;

    public AuthController(IAuthService authService, HrDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<AuthResponse>.Ok(result.Value!, result.Message));
    }

    /// <summary>
    /// Lấy thông tin user đang đăng nhập (từ JWT token)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetMe()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(ApiResponse<UserInfoDto>.Fail("InvalidToken", "Token không hợp lệ."));
        }

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(ApiResponse<UserInfoDto>.Fail("UserNotFound", "Không tìm thấy người dùng."));
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        var userInfo = new UserInfoDto(
            Id: user.Id,
            Email: user.Email,
            FullName: user.Employee?.FullName ?? "System Administrator",
            EmployeeId: user.EmployeeId,
            Roles: roles
        );

        return Ok(ApiResponse<UserInfoDto>.Ok(userInfo, "Lấy thông tin người dùng thành công."));
    }
}
