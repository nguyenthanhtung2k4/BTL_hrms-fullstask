$BASE = "http://localhost:5005/api/v1"
$hdrs = @{ "Content-Type" = "application/json; charset=utf-8" }

function OK($m) { Write-Host "  OK $m" -ForegroundColor Green }
function ERR($m) { Write-Host "  ERR $m" -ForegroundColor Red }
function STEP($m) { Write-Host "`n--- $m ---" -ForegroundColor Cyan }

# 1. Login
STEP "1. Login"
try {
    $r = Invoke-RestMethod -Method POST -Uri "$BASE/hr/auth/login" -Headers $hdrs -Body '{"email":"admin@hrms.com","password":"admin123"}'
    $TOKEN = $r.data.accessToken
    $ah = @{ "Content-Type" = "application/json; charset=utf-8"; "Authorization" = "Bearer $TOKEN" }
    OK "Token: $($TOKEN.Substring(0,20))..."
} catch { ERR "Login failed: $_"; exit }

# 2. Departments
STEP "2. Departments"
$depts = @(
    [pscustomobject]@{ name="Ban Giám Đốc"; code="BGD"; description="Ban lãnh đạo công ty" },
    [pscustomobject]@{ name="Phòng Nhân Sự"; code="HR"; description="Quản lý nhân sự" },
    [pscustomobject]@{ name="Phòng Kỹ Thuật"; code="IT"; description="Phát triển hệ thống và AI" },
    [pscustomobject]@{ name="Phòng Kế Toán"; code="ACC"; description="Quản lý tài chính" },
    [pscustomobject]@{ name="Phòng Kinh Doanh"; code="SALES"; description="Kinh doanh và Marketing" }
)
$deptIds = @{}
foreach ($d in $depts) {
    try {
        $b = [ordered]@{ name=$d.name; code=$d.code; description=$d.description } | ConvertTo-Json
        $r = Invoke-RestMethod -Method POST -Uri "$BASE/hr/departments" -Headers $ah -Body $b
        $deptIds[$d.code] = $r.data.id
        OK "Dept: $($d.name)"
    } catch { ERR "Dept $($d.name): $_" }
}

# 3. Positions
STEP "3. Positions"
$positions = @(
    [pscustomobject]@{ name="Giám Đốc"; code="GD"; level=1; description="Người đứng đầu công ty" },
    [pscustomobject]@{ name="Trưởng Phòng"; code="TP"; level=2; description="Trưởng các phòng ban" },
    [pscustomobject]@{ name="Nhân Viên"; code="NV"; level=3; description="Nhân viên thực thi" },
    [pscustomobject]@{ name="Kỹ Sư Phần Mềm"; code="SW"; level=3; description="Lập trình viên" },
    [pscustomobject]@{ name="Kế Toán Viên"; code="KTV"; level=3; description="Nhân viên kế toán" },
    [pscustomobject]@{ name="Chuyên Viên NS"; code="CVNS"; level=3; description="Tuyển dụng và quản lý nhân sự" }
)
$posIds = @{}
foreach ($p in $positions) {
    try {
        $b = [ordered]@{ name=$p.name; code=$p.code; level=$p.level; description=$p.description } | ConvertTo-Json
        $r = Invoke-RestMethod -Method POST -Uri "$BASE/hr/positions" -Headers $ah -Body $b
        $posIds[$p.code] = $r.data.id
        OK "Position: $($p.name)"
    } catch { ERR "Position $($p.name): $_" }
}

# 4. Employees Generator (150 Employees)
STEP "4. Employees"
$viLast = @("Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý")
$viMale = @("Thế Dân", "Minh Hoàng", "Văn Đức", "Quốc Bảo", "Hữu Nghĩa", "Văn Minh", "Minh Trí", "Thanh Tùng", "Tuấn Anh", "Đức Nam", "Quang Huy", "Khánh Duy", "Xuân Trường")
$viFemale = @("Thị Lan Anh", "Thị Thanh Tuyền", "Thị Mai", "Thị Hương", "Thị Thủy", "Bích Phương", "Ngọc Trinh", "Thanh Hà", "Hồng Vân", "Thùy Linh", "Hải Yến")

$enMale = @("David Miller", "Jean-Luc Picard", "Rajesh Kumar", "John Smith", "Park Ji-sung", "Michael Johnson", "Alexandre Dupont", "Hans Schmidt", "Yuki Tanaka", "Daniel Evans")
$enFemale = @("Sarah Jenkins", "Elena Rostova", "Emily Watson", "Sophia Martinez", "Chloe Dubois", "Maria Rossi", "Anna Johansson", "Li Wei", "Priya Patel")

$emps = @()
# Seed Tier 1 & 2 Heads
$emps += [pscustomobject]@{ code="NV001"; name="Nguyễn Thế Dân"; email="dan.the@hrms.com"; phone="0901112223"; gender="Male"; dob="1980-01-01"; hire="2020-01-01"; dept="BGD"; pos="GD"; manager=$null; sal=150000000 }
$emps += [pscustomobject]@{ code="NV002"; name="David Miller"; email="david.miller@hrms.com"; phone="0901234567"; gender="Male"; dob="1985-04-12"; hire="2020-01-01"; dept="BGD"; pos="GD"; manager="NV001"; sal=120000000 }
$emps += [pscustomobject]@{ code="NV003"; name="Nguyễn Thị Lan Anh"; email="lan.anh@hrms.com"; phone="0902345678"; gender="Female"; dob="1990-05-15"; hire="2022-01-10"; dept="HR"; pos="TP"; manager="NV001"; sal=25000000 }
$emps += [pscustomobject]@{ code="NV004"; name="Phạm Văn Đức"; email="van.duc@hrms.com"; phone="0903456789"; gender="Male"; dob="1988-11-30"; hire="2020-03-01"; dept="ACC"; pos="TP"; manager="NV001"; sal=30000000 }
$emps += [pscustomobject]@{ code="NV005"; name="Nguyễn Quốc Bảo"; email="quoc.bao@hrms.com"; phone="0904567890"; gender="Male"; dob="1991-02-08"; hire="2021-07-20"; dept="SALES"; pos="TP"; manager="NV001"; sal=28000000 }
$emps += [pscustomobject]@{ code="NV006"; name="Đặng Hữu Nghĩa"; email="huu.nghia@hrms.com"; phone="0905678901"; gender="Male"; dob="1989-04-03"; hire="2020-12-15"; dept="IT"; pos="TP"; manager="NV002"; sal=60000000 }

for ($i = 7; $i -le 150; $i++) {
    $code = "NV{0:D3}" -f $i
    $gender = if ($i % 2 -eq 0) { "Female" } else { "Male" }
    $isForeign = ($i % 4 -eq 0)
    
    $name = ""
    if ($isForeign) {
        if ($gender -eq "Male") {
            $name = $enMale[$i % $enMale.Count]
        } else {
            $name = $enFemale[$i % $enFemale.Count]
        }
    } else {
        $last = $viLast[$i % $viLast.Count]
        $first = if ($gender -eq "Male") { $viMale[$i % $viMale.Count] } else { $viFemale[$i % $viFemale.Count] }
        $name = "$last $first"
    }
    
    $mod = $i % 10
    $dept = "IT"
    $pos = "SW"
    $manager = "NV006"
    $sal = 20000000 + ($i % 7) * 5000000
    
    if ($mod -eq 6) {
        $dept = "HR"
        $pos = "CVNS"
        $manager = "NV003"
        $sal = 15000000 + ($i % 5) * 2000000
    } elseif ($mod -eq 7) {
        $dept = "ACC"
        $pos = "KTV"
        $manager = "NV004"
        $sal = 15000000 + ($i % 5) * 2000000
    } elseif ($mod -eq 8 -or $mod -eq 9) {
        $dept = "SALES"
        $pos = "NV"
        $manager = "NV005"
        $sal = 12000000 + ($i % 6) * 2000000
    }
    
    # Strip diacritics for realistic emails
    $normalized = $name.Normalize([System.Text.NormalizationForm]::FormD)
    $sb = [System.Text.StringBuilder]::new()
    foreach ($c in $normalized.ToCharArray()) {
        if ([System.Globalization.CharUnicodeInfo]::GetUnicodeCategory($c) -ne [System.Globalization.UnicodeCategory]::NonSpacingMark) {
            [void]$sb.Append($c)
        }
    }
    $emailPrefix = $sb.ToString().ToLower().Replace(" ", "").Replace("đ", "d")
    $email = "$emailPrefix$i@hrms.com"
    $phone = "090" + (1000000 + $i * 123)
    
    $dobYear = 1980 + ($i % 20)
    $dobMonth = 1 + ($i % 12)
    $dobDay = 1 + ($i % 28)
    $dob = "{0:D4}-{1:D2}-{2:D2}" -f $dobYear, $dobMonth, $dobDay
    
    $hireYear = 2021 + ($i % 4)
    $hireMonth = 1 + ($i % 12)
    $hireDay = 1 + ($i % 28)
    $hire = "{0:D4}-{1:D2}-{2:D2}" -f $hireYear, $hireMonth, $hireDay
    
    $emps += [pscustomobject]@{ code=$code; name=$name; email=$email; phone=$phone; gender=$gender; dob=$dob; hire=$hire; dept=$dept; pos=$pos; manager=$manager; sal=$sal }
}

$empIds = @{}
foreach ($e in $emps) {
    try {
        $mId = $null
        if ($e.manager -and $empIds.ContainsKey($e.manager)) {
            $mId = $empIds[$e.manager]
        }
        $b = [ordered]@{
            employeeCode = $e.code; fullName = $e.name; email = $e.email; phone = $e.phone
            gender = $e.gender; dateOfBirth = $e.dob; hireDate = $e.hire; status = "Active"
            departmentId = $deptIds[$e.dept]; positionId = $posIds[$e.pos]; managerEmployeeId = $mId
        } | ConvertTo-Json
        $r = Invoke-RestMethod -Method POST -Uri "$BASE/hr/employees" -Headers $ah -Body $b
        $empIds[$e.code] = $r.data.id
        OK "Employee: $($e.name) ($($e.code))"
    } catch { ERR "Employee $($e.name): $_" }
}

# 5. Contracts
STEP "5. Contracts"
$idx = 1
foreach ($e in $emps) {
    if ($empIds.ContainsKey($e.code)) {
        try {
            $no = "HD-2024-{0:D3}" -f $idx
            $b = [ordered]@{ contractNumber=$no; employeeId=$empIds[$e.code]; contractType="Chính thức"; startDate=$e.hire; baseSalary=$e.sal } | ConvertTo-Json
            Invoke-RestMethod -Method POST -Uri "$BASE/hr/contracts" -Headers $ah -Body $b | Out-Null
            OK "Contract $no for $($e.code) - $($e.sal) VND"
            $idx++
        } catch { ERR "Contract for $($e.code): $_" }
    }
}

# 6. Shifts
STEP "6. Work Shifts"
$shifts = @(
    [pscustomobject]@{ code="CA_HC"; name="Ca Hành Chính"; start="08:30"; end="17:30"; brk=90 },
    [pscustomobject]@{ code="CA_SANG"; name="Ca Sáng"; start="08:00"; end="17:00"; brk=60 },
    [pscustomobject]@{ code="CA_CHIEU"; name="Ca Chiều"; start="13:00"; end="22:00"; brk=60 }
)
$shiftIds = @{}
foreach ($sh in $shifts) {
    try {
        $b = [ordered]@{ code=$sh.code; name=$sh.name; startTime=$sh.start; endTime=$sh.end; breakMinutes=$sh.brk } | ConvertTo-Json
        $r = Invoke-RestMethod -Method POST -Uri "$BASE/attendance/shifts" -Headers $ah -Body $b
        $shiftIds[$sh.code] = $r.data.id
        OK "Shift: $($sh.name)"
    } catch { ERR "Shift $($sh.name): $_" }
}

# 7. Work Schedules
STEP "7. Work Schedules"
$defaultShift = $shiftIds["CA_HC"]
if ($defaultShift) {
    foreach ($ec in $empIds.Keys) {
        try {
            $b = [ordered]@{ employeeId=$empIds[$ec]; shiftId=$defaultShift; startDate="2025-07-01"; endDate="2026-12-31" } | ConvertTo-Json
            Invoke-RestMethod -Method POST -Uri "$BASE/attendance/work-schedules" -Headers $ah -Body $b | Out-Null
            OK "Schedule for $ec"
        } catch { ERR "Schedule for $ec failed" }
    }
}

# 8. Payroll Rule
STEP "8. Payroll Rule"
$ruleId = $null
try {
    $b = [ordered]@{ code="QT_CHUAN"; name="Quy tắc lương chuẩn 2025-2026"; workDayHours=8; paidLeaveCountsAsWork=$true; overtimeRate=1.5; isActive=$true } | ConvertTo-Json
    $r = Invoke-RestMethod -Method POST -Uri "$BASE/payroll/payroll-rules" -Headers $ah -Body $b
    $ruleId = $r.data.id
    OK "Rule: Quy tắc lương chuẩn [ID: $ruleId]"
} catch { ERR "Payroll Rule: $_" }

# 9. Payroll Period
STEP "9. Payroll Period"
if ($ruleId) {
    try {
        $b = [ordered]@{ name="Lương tháng 6/2026"; fromDate="2026-06-01"; toDate="2026-06-30"; payrollRuleId=$ruleId } | ConvertTo-Json
        $r = Invoke-RestMethod -Method POST -Uri "$BASE/payroll/payroll-periods" -Headers $ah -Body $b
        $periodId = $r.data.id
        OK "Period: Lương tháng 6/2026 [ID: $periodId]"
    } catch { ERR "Period: $_" }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  SEED COMPLETE!" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  5 departments | 6 positions | $($emps.Count) employees"
Write-Host "  $($emps.Count) contracts | 3 shifts | work schedules"
Write-Host "  1 payroll rule | 1 payroll period"
Write-Host ""
Write-Host "  Visit: http://localhost:5173" -ForegroundColor Cyan
Write-Host ""
