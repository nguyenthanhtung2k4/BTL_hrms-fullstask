using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hrms.HrCore.Application.Dtos;
using Hrms.HrCore.Application.Interfaces;
using Hrms.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hrms.HrCore.Controllers;

[ApiController]
[Route("api/v1/hr/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ContractDto>>>> GetAll()
    {
        var result = await _contractService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ContractDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> GetById(Guid id)
    {
        var result = await _contractService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound(ApiResponse<ContractDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<ContractDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ContractDto>>> Create([FromBody] CreateContractDto dto)
    {
        var result = await _contractService.CreateAsync(dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<ContractDto>.Fail(result.Errors, result.Message));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<ContractDto>.Ok(result.Value, result.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> Update(Guid id, [FromBody] UpdateContractDto dto)
    {
        var result = await _contractService.UpdateAsync(id, dto);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse<ContractDto>.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse<ContractDto>.Ok(result.Value!, result.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _contractService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }
}
