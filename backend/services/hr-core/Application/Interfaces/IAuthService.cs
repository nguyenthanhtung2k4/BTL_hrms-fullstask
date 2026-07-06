using System;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken);
    Task<Result> RevokeTokenAsync(string refreshToken);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
