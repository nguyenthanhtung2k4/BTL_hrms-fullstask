using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.HrCore.Controllers;

[ApiController]
[Route("api/v1/hr/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
}
