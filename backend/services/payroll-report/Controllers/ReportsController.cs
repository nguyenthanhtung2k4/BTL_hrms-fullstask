using System;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.PayrollReport.Controllers;

[ApiController]
[Route("api/v1/payroll/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<PayrollSummaryReportDto>>> GetSummaryReport([FromQuery] Guid periodId)
    {
        var result = await _reportService.GetSummaryReportAsync(periodId);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PayrollSummaryReportDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollSummaryReportDto>.Ok(result.Value!, result.Message));
    }
}
