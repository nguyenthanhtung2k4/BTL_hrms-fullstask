using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}
