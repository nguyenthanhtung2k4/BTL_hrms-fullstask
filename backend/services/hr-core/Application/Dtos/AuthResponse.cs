namespace Hrms.HrCore.Application.Dtos;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiry,
    UserInfoDto User
);

public record RefreshTokenRequest(string RefreshToken);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
