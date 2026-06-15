namespace Hrms.HrCore.Application.Dtos;

public record AuthResponse(
    string AccessToken,
    UserInfoDto User
);
