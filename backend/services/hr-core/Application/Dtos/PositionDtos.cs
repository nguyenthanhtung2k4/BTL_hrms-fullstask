using System;

namespace Hrms.HrCore.Application.Dtos;

public record PositionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreatePositionDto(
    string Code,
    string Name,
    string? Description
);

public record UpdatePositionDto(
    string Name,
    string? Description,
    bool IsActive
);
