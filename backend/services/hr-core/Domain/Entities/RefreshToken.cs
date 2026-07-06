using System;
using Hrms.Shared.Domain;

namespace Hrms.HrCore.Domain.Entities;

/// <summary>
/// Lưu trữ refresh token để duy trì phiên đăng nhập mà không cần đăng nhập lại.
/// Mỗi user có thể có nhiều refresh token (multi-device).
/// </summary>
public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string? DeviceInfo { get; set; }  // Optional: browser/device info

    // Navigation
    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
