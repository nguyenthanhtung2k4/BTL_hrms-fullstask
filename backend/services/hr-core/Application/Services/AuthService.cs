using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
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

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Email and password are required.");
        }

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure("Account has been deactivated.");
        }

        // Verify password hash
        bool isPasswordValid = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        // Update login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Generate Roles List
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = GenerateJwtToken(user, roles);

        var userInfo = new UserInfoDto(
            Id: user.Id,
            Email: user.Email,
            FullName: user.Employee?.FullName ?? "System Administrator",
            EmployeeId: user.EmployeeId,
            Roles: roles
        );

        return Result<AuthResponse>.Success(new AuthResponse(token, userInfo));
    }

    private string GenerateJwtToken(Domain.Entities.User user, List<string> roles)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? "hrms-jwt-super-secret-key-1234567890123456";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "Hrms.HrCore";
        var audience = _configuration["JwtSettings:Audience"] ?? "Hrms.App";
        var expiryMinutes = double.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out var exp) ? exp : 60;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
