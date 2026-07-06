using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hrms.HrCore.Domain.Entities;
using Hrms.HrCore.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hrms.HrCore.Controllers;

[ApiController]
[Route("api/v1/hr/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly HrDbContext _dbContext;

    public NotificationsController(HrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetMyNotifications()
    {
        var employeeIdClaim = User.FindFirst("employeeId")?.Value;
        if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var employeeId))
        {
            return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(Enumerable.Empty<NotificationDto>()));
        }

        var list = await _dbContext.Notifications
            .Where(n => n.EmployeeId == employeeId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.EmployeeId, n.Title, n.Content, n.Type, n.IsRead, n.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(list));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(Guid id)
    {
        var employeeIdClaim = User.FindFirst("employeeId")?.Value;
        if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var employeeId))
        {
            return Forbid();
        }

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.EmployeeId == employeeId);

        if (notification == null)
        {
            return NotFound(ApiResponse.Fail("NotificationNotFound", "Notification not found or access denied."));
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Notification marked as read."));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
    {
        var employeeIdClaim = User.FindFirst("employeeId")?.Value;
        if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var employeeId))
        {
            return Forbid();
        }

        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.EmployeeId == employeeId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unreadNotifications)
        {
            n.IsRead = true;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse.Ok("All notifications marked as read."));
    }
}

public record NotificationDto(
    Guid Id,
    Guid? EmployeeId,
    string Title,
    string Content,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);
