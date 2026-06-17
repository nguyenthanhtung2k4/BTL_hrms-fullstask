# 👥 Module 2: HR Core — Nhân sự

> **Service:** HR Core (`/api/v1/hr/`)  
> **Files:** `src/modules/hr/`  
> **Roles được phép:** Admin, HR (CRUD) | Manager (Xem nhân viên phòng mình)

---

## Checklist thực hiện

### Phòng ban (Departments)
- [ ] `department.service.ts` — CRUD API calls
- [ ] `DepartmentListView.vue` — Danh sách + search + filter
- [ ] `DepartmentFormModal.vue` — Modal tạo/sửa
- [ ] `useDepartments.ts` — Composable logic

### Chức vụ (Positions)
- [ ] `position.service.ts`
- [ ] `PositionListView.vue`
- [ ] `PositionFormModal.vue`
- [ ] `usePositions.ts`

### Nhân viên (Employees)
- [ ] `employee.service.ts`
- [ ] `EmployeeListView.vue` — Danh sách + filter theo phòng ban/trạng thái
- [ ] `EmployeeDetailView.vue` — Chi tiết + tab hợp đồng
- [ ] `EmployeeFormModal.vue` — Tạo/sửa hồ sơ
- [ ] `EmployeeStatusModal.vue` — Đổi trạng thái (Active/Inactive/Resigned)
- [ ] `useEmployees.ts`

### Hợp đồng (Contracts)
- [ ] `contract.service.ts`
- [ ] `ContractListView.vue`
- [ ] `ContractFormModal.vue`
- [ ] `useContracts.ts`

---

## 1. Departments — Phòng ban

### Màn hình `DepartmentListView.vue`

**Layout:**
```
[PageHeader: "Phòng ban"] [Button "+ Thêm phòng ban" — chỉ Admin/HR]
[SearchInput]  [Filter: IsActive]
[Table]
  Cột: STT | Mã | Tên phòng ban | Trạng thái | Ngày tạo | Hành động
  Hành động: [Sửa] [Xóa] — chỉ Admin/HR
[Pagination]
```

**States cần handle:**
- Loading → skeleton table
- Empty → "Chưa có phòng ban nào"
- Error → toast lỗi

**Button visibility theo role:**
```
Admin/HR:   Hiện nút "+ Thêm", [Sửa], [Xóa]
Manager:    Ẩn tất cả nút action, chỉ xem
Employee:   Không vào được trang này (route guard)
```

### Modal `DepartmentFormModal.vue`
```
Fields:
  Code*      → Input (required, unique)
  Tên*       → Input (required)
  Mô tả      → Textarea
  Kích hoạt  → Toggle (default: true)

Buttons: [Hủy] [Lưu — loading khi submit]
Validation: Code và Tên không được rỗng
```

---

## 2. Positions — Chức vụ

### Màn hình `PositionListView.vue`
Tương tự Departments:
```
Cột: STT | Mã | Tên chức vụ | Trạng thái | Ngày tạo | Hành động
```

### Modal `PositionFormModal.vue`
```
Fields:
  Code*      → Input
  Tên*       → Input
  Mô tả      → Textarea
  Kích hoạt  → Toggle
```

---

## 3. Employees — Nhân viên

### Màn hình `EmployeeListView.vue`

**Layout:**
```
[PageHeader: "Nhân viên"] [Button "+ Thêm nhân viên" — Admin/HR]
[SearchInput — tìm theo tên, mã NV, email]
[Filter: Phòng ban | Chức vụ | Trạng thái]
[Table]
  Cột: Mã NV | Họ tên | Phòng ban | Chức vụ | Ngày vào | Trạng thái | Hành động
  Trạng thái badge:
    Active   → green
    Inactive → gray
    Resigned → red
  Hành động: [Xem] [Sửa — Admin/HR] [Đổi trạng thái — Admin/HR]
[Pagination]
```

**Filter Panel:**
```
- Dropdown Phòng ban (load từ API)
- Dropdown Chức vụ (load từ API)
- Dropdown Trạng thái: Tất cả / Active / Inactive / Resigned
- Button [Reset filter]
```

### Màn hình `EmployeeDetailView.vue`

**Layout:**
```
[Back button] [Tên nhân viên] [Badge trạng thái]

[Tabs]
  Tab 1: Thông tin cơ bản
    - Mã NV, Họ tên, Email, SĐT, Giới tính, Ngày sinh
    - Phòng ban, Chức vụ, Quản lý trực tiếp
    - Ngày vào làm
    - [Button Sửa — Admin/HR]
    - [Button Đổi trạng thái — Admin/HR]

  Tab 2: Hợp đồng
    - Table hợp đồng của nhân viên này
    - [Button Thêm hợp đồng — Admin/HR]
```

### Modal `EmployeeFormModal.vue`
```
Fields:
  Mã nhân viên*  → Input (readonly khi sửa)
  Họ tên*        → Input
  Email*         → Input email
  SĐT            → Input
  Giới tính      → Select: Nam/Nữ/Khác
  Ngày sinh      → DatePicker
  Ngày vào làm*  → DatePicker
  Phòng ban*     → Select (load từ API)
  Chức vụ*       → Select (load từ API)
  Quản lý        → Select (load nhân viên từ API)
```

### Modal `EmployeeStatusModal.vue`
```
Tiêu đề: "Đổi trạng thái: [Tên nhân viên]"
Trạng thái hiện tại: badge
Trạng thái mới*: Select (Active/Inactive/OnLeave/Resigned)
Lý do*: Textarea
[Hủy] [Xác nhận]
```

---

## 4. Contracts — Hợp đồng

### Màn hình `ContractListView.vue`
```
[PageHeader: "Hợp đồng"] [Button "+ Thêm hợp đồng" — Admin/HR]
[Filter: Nhân viên | Loại HĐ | Trạng thái]
[Table]
  Cột: Số HĐ | Nhân viên | Loại | Lương cơ bản | Từ ngày | Đến ngày | Trạng thái | Hành động
  Trạng thái: Active/Expired/Terminated
  Hành động: [Sửa] [Xóa]
```

### Modal `ContractFormModal.vue`
```
Fields:
  Số hợp đồng*   → Input (unique)
  Nhân viên*     → Select (load từ API)
  Loại HĐ*       → Select: Chính thức / Thử việc / Part-time
  Ngày bắt đầu*  → DatePicker
  Ngày kết thúc  → DatePicker (nullable)
  Lương cơ bản*  → InputNumber (VNĐ)
  Trạng thái     → Select: Active/Expired/Terminated
```

---

## 5. API Services

### `src/services/department.service.ts`
```typescript
export const departmentService = {
  getAll: () => apiClient.get('/api/v1/hr/departments'),
  getById: (id: string) => apiClient.get(`/api/v1/hr/departments/${id}`),
  create: (data) => apiClient.post('/api/v1/hr/departments', data),
  update: (id, data) => apiClient.put(`/api/v1/hr/departments/${id}`, data),
  delete: (id) => apiClient.delete(`/api/v1/hr/departments/${id}`),
}
```

*(Tương tự cho position, employee, contract services)*

---

## 6. TypeScript Types

```typescript
// src/types/hr.types.ts

export interface Department {
  id: string
  code: string
  name: string
  description?: string
  isActive: boolean
  createdAt: string
}

export interface Position {
  id: string
  code: string
  name: string
  description?: string
  isActive: boolean
}

export interface Employee {
  id: string
  employeeCode: string
  fullName: string
  email: string
  phone?: string
  gender?: string
  dateOfBirth?: string
  hireDate: string
  departmentId: string
  departmentName: string
  positionId: string
  positionName: string
  managerEmployeeId?: string
  managerName?: string
  status: 'Active' | 'Inactive' | 'OnLeave' | 'Resigned'
  createdAt: string
}

export interface Contract {
  id: string
  contractNumber: string
  employeeId: string
  employeeName: string
  contractType: 'Chính thức' | 'Thử việc' | 'Part-time'
  startDate: string
  endDate?: string
  baseSalary: number
  status: 'Active' | 'Expired' | 'Terminated'
}
```
