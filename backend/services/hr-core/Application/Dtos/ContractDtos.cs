// DTO trả về (GET)
public record ContractDto(
    Guid Id,
    string ContractNumber,
    Guid EmployeeId,
    string EmployeeName,
    string ContractType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BaseSalary,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? AttachmentUrl // 👈 Thêm dòng này
);

// DTO tạo mới (POST)
public record CreateContractDto(
    string ContractNumber,
    Guid EmployeeId,
    string ContractType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BaseSalary,
    string? AttachmentUrl // 👈 Thêm dòng này
);

// DTO cập nhật (PUT)
public record UpdateContractDto(
    string ContractType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal BaseSalary,
    string Status,
    string? AttachmentUrl
);