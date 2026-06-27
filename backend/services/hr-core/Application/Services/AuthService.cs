using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.HrCore.Domain.Entities;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Hrms.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Hrms.HrCore.Application.Services;

public class AuthService : IAuthService
{
    private readonly HrDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(HrDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    // ─── Login ────────────────────────────────────────────────────────────────
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResponse>.Failure("Email and password are required.");

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Account has been deactivated.");

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        await _dbContext.SaveChangesAsync();

        var userInfo = BuildUserInfoDto(user, roles);
        return Result<AuthResponse>.Success(new AuthResponse(
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            userInfo));
    }

    // ─── Refresh Token ────────────────────────────────────────────────────────
    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Employee)
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
            return Result<AuthResponse>.Failure("Invalid refresh token.");

        if (!storedToken.IsActive)
        {
            // Token đã bị thu hồi hoặc hết hạn → thu hồi toàn bộ family (bảo vệ re-use)
            if (storedToken.IsRevoked)
                await RevokeAllUserTokensAsync(storedToken.UserId);
            return Result<AuthResponse>.Failure("Refresh token is expired or revoked.");
        }

        var user = storedToken.User;
        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Account has been deactivated.");

        // Thu hồi token cũ
        storedToken.IsRevoked = true;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = GenerateJwtToken(user, roles);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

        await _dbContext.SaveChangesAsync();

        var userInfo = BuildUserInfoDto(user, roles);
        return Result<AuthResponse>.Success(new AuthResponse(
            newAccessToken,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt,
            userInfo));
    }

    // ─── Revoke Token ─────────────────────────────────────────────────────────
    public async Task<Result> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
            return Result.Failure("Token not found.");

        if (!storedToken.IsActive)
            return Result.Failure("Token is already revoked or expired.");

        storedToken.IsRevoked = true;
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }

    // ─── Change Password ──────────────────────────────────────────────────────
    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return Result.Failure("Mật khẩu xác nhận không khớp.");

        var (isValid, message) = PasswordHasher.ValidatePassword(request.NewPassword);
        if (!isValid)
            return Result.Failure(message);

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            return Result.Failure("Không tìm thấy người dùng.");

        if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return Result.Failure("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);

        // Revoke all refresh tokens on password change (security)
        await RevokeAllUserTokensAsync(userId);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private string GenerateJwtToken(User user, List<string> roles)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? "hrms-jwt-super-secret-key-1234567890123456";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "Hrms.HrCore";
        var audience = _configuration["JwtSettings:Audience"] ?? "Hrms.App";
        var expiryMinutes = double.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out var exp) ? exp : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));


            var departmentId = user.Employee?.DepartmentId; // if some function need this
            if (departmentId.HasValue)
                claims.Add(new Claim("departmentId", departmentId.Value.ToString()));
        }

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshDays = int.TryParse(_configuration["JwtSettings:RefreshTokenDays"], out var days) ? days : 30;

        // Xoá các token cũ đã hết hạn (housekeeping)
        var expired = _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && (rt.IsRevoked || rt.ExpiresAt < DateTime.UtcNow));
        _dbContext.RefreshTokens.RemoveRange(expired);

        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(tokenBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IsRevoked = false,
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        return refreshToken;
    }

    private async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var t in activeTokens)
            t.IsRevoked = true;
    }

    private static UserInfoDto BuildUserInfoDto(User user, List<string> roles) =>
        new(
            Id: user.Id,
            Email: user.Email,
            FullName: user.Employee?.FullName ?? "System Administrator",
            EmployeeId: user.EmployeeId,
            Roles: roles
        );
}
