using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.PayrollReport.Infrastructure.Persistence;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hrms.PayrollReport.Controllers;

[ApiController]
[Route("api/v1/payroll/reports")]
[Authorize(Roles = "Admin,HR,PayrollStaff,Manager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly PayrollReportDbContext _dbContext;

    public ReportsController(IReportService reportService, PayrollReportDbContext dbContext)
    {
        _reportService = reportService;
        _dbContext = dbContext;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<PayrollSummaryReportDto>>> GetSummaryReport(
        [FromQuery] Guid periodId,
        [FromQuery] Guid? departmentId = null)
    {
        // Enforce Manager role department isolation
        if (User.IsInRole("Manager") && !User.IsInRole("Admin") && !User.IsInRole("HR") && !User.IsInRole("PayrollStaff"))
        {
            var employeeIdString = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdString) || !Guid.TryParse(employeeIdString, out var employeeId))
            {
                return Forbid();
            }

            var manager = await _dbContext.EmployeeProjections.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (manager == null || !manager.DepartmentId.HasValue)
            {
                return Forbid();
            }

            departmentId = manager.DepartmentId.Value;
        }

        var result = await _reportService.GetSummaryReportAsync(periodId, departmentId);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PayrollSummaryReportDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollSummaryReportDto>.Ok(result.Value!, result.Message));
    }
}
