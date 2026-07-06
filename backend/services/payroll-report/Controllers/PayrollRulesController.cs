using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.PayrollReport.Application.Dtos;
using Hrms.PayrollReport.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.PayrollReport.Controllers;

[ApiController]
[Route("api/v1/payroll/payroll-rules")]
[Authorize(Roles = "Admin,PayrollStaff")]
public class PayrollRulesController : ControllerBase
{
    private readonly IPayrollRuleService _ruleService;

    public PayrollRulesController(IPayrollRuleService ruleService)
    {
        _ruleService = ruleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PayrollRuleDto>>>> GetAll()
    {
        var result = await _ruleService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PayrollRuleDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PayrollRuleDto>>> GetById(Guid id)
    {
        var result = await _ruleService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<PayrollRuleDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollRuleDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PayrollRuleDto>>> Create([FromBody] CreatePayrollRuleDto request)
    {
        var result = await _ruleService.CreateAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PayrollRuleDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollRuleDto>.Ok(result.Value!, result.Message));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PayrollRuleDto>>> Update(Guid id, [FromBody] UpdatePayrollRuleDto request)
    {
        var result = await _ruleService.UpdateAsync(id, request);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<PayrollRuleDto>.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse<PayrollRuleDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _ruleService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }
        return Ok(ApiResponse.Ok(result.Message));
    }
}
