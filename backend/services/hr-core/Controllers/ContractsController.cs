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
    [Authorize(Roles = "Admin,HR,Manager,PayrollStaff")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ContractDto>>>> GetAll()
    {
        var result = await _contractService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ContractDto>>.Ok(result.Value!, result.Message));
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ContractDto>>>> GetByEmployeeId(Guid employeeId)
    {
        // Check permissions: Admin, HR, Manager, PayrollStaff can view any contract.
        // Employee can only view their own contracts.
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Manager") || User.IsInRole("PayrollStaff");
        if (!isPrivileged)
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var claimEmpId) || claimEmpId != employeeId)
            {
                return Forbid();
            }
        }

        var result = await _contractService.GetByEmployeeIdAsync(employeeId);
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

        // Check permissions: Admin, HR, Manager, PayrollStaff can view any contract.
        // Employee can only view their own contract.
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Manager") || User.IsInRole("PayrollStaff");
        if (!isPrivileged)
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var employeeId) || employeeId != result.Value!.EmployeeId)
            {
                return Forbid();
            }
        }

        return Ok(ApiResponse<ContractDto>.Ok(result.Value!, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
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
    [Authorize(Roles = "Admin,HR")]
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
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var result = await _contractService.DeleteAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Errors, result.Message));
        }

        return Ok(ApiResponse.Ok(result.Message));
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,HR")]
    [Consumes("multipart/form-data")] // Quan trọng: định nghĩa content-type
    public async Task<ActionResult<ApiResponse<UploadResultDto>>> UploadFile([FromForm] IFormFile file)
    {
        // 1. Validate file
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<UploadResultDto>.Fail("File không được để trống"));

        // Kiểm tra kích thước (VD: tối đa 10MB)
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(ApiResponse<UploadResultDto>.Fail("File vượt quá 10MB"));

        // Kiểm tra loại file (VD: chỉ cho phép PDF, JPG, PNG, DOC, DOCX)
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(ApiResponse<UploadResultDto>.Fail("Định dạng file không được hỗ trợ"));

        try
        {
            // 2. Tạo tên file unique (tránh trùng)
            var fileName = $"{Guid.NewGuid():N}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var folderPath = Path.Combine("wwwroot", "uploads", "contracts");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);

            // 3. Lưu file vào thư mục
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Trả về URL tương đối (không cần base URL vì đã có gateway)
            var relativeUrl = $"/uploads/contracts/{fileName}";
            return Ok(ApiResponse<UploadResultDto>.Ok(
                new UploadResultDto { Url = relativeUrl, FileName = file.FileName },
                "Upload file thành công"
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UploadResultDto>.Fail($"Lỗi khi lưu file: {ex.Message}"));
        }
    }
}
