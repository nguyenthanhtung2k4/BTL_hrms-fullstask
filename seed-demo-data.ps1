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
    [pscustomobject]@{ name="Ban Giam Doc"; code="BGD"; description="Ban lanh dao cong ty" },
    [pscustomobject]@{ name="Phong Nhan Su"; code="HR"; description="Quan ly nhan su" },
    [pscustomobject]@{ name="Phong Ky Thuat"; code="IT"; description="Phat trien he thong" },
    [pscustomobject]@{ name="Phong Ke Toan"; code="ACC"; description="Quan ly tai chinh" },
    [pscustomobject]@{ name="Phong Kinh Doanh"; code="SALES"; description="Ban hang" }
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
    [pscustomobject]@{ name="Giam Doc"; code="GD"; level=1; description="Nguoi dung dau cong ty" },
    [pscustomobject]@{ name="Truong Phong"; code="TP"; level=2; description="Truong cac phong ban" },
    [pscustomobject]@{ name="Nhan Vien"; code="NV"; level=3; description="Nhan vien thuc thi" },
    [pscustomobject]@{ name="Ky Su Phan Mem"; code="SW"; level=3; description="Lap trinh vien" },
    [pscustomobject]@{ name="Ke Toan Vien"; code="KTV"; level=3; description="Nhan vien ke toan" },
    [pscustomobject]@{ name="Chuyen Vien NS"; code="CVNS"; level=3; description="Tuyen dung nhan su" }
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

# 4. Employees
STEP "4. Employees"
$emps = @(
    [pscustomobject]@{ code="NV002"; name="Nguyen Thi Lan Anh"; email="lan.anh@hrms.com"; phone="0901234567"; gender="Female"; dob="1990-05-15"; hire="2022-01-10"; dept="HR"; pos="TP" },
    [pscustomobject]@{ code="NV003"; name="Tran Minh Hoang";    email="minh.hoang@hrms.com"; phone="0902345678"; gender="Male"; dob="1995-08-20"; hire="2022-06-01"; dept="IT"; pos="SW" },
    [pscustomobject]@{ code="NV004"; name="Le Thi Thanh Tuyen"; email="thanh.tuyen@hrms.com"; phone="0903456789"; gender="Female"; dob="1993-03-12"; hire="2021-09-15"; dept="IT"; pos="SW" },
    [pscustomobject]@{ code="NV005"; name="Pham Van Duc";       email="van.duc@hrms.com"; phone="0904567890"; gender="Male"; dob="1988-11-30"; hire="2020-03-01"; dept="ACC"; pos="KTV" },
    [pscustomobject]@{ code="NV006"; name="Hoang Thi Mai";      email="thi.mai@hrms.com"; phone="0905678901"; gender="Female"; dob="1992-07-25"; hire="2023-02-14"; dept="ACC"; pos="KTV" },
    [pscustomobject]@{ code="NV007"; name="Nguyen Quoc Bao";    email="quoc.bao@hrms.com"; phone="0906789012"; gender="Male"; dob="1991-02-08"; hire="2021-07-20"; dept="SALES"; pos="NV" },
    [pscustomobject]@{ code="NV008"; name="Vu Thi Huong";       email="thi.huong@hrms.com"; phone="0907890123"; gender="Female"; dob="1994-09-14"; hire="2022-11-01"; dept="SALES"; pos="NV" },
    [pscustomobject]@{ code="NV009"; name="Dang Huu Nghia";     email="huu.nghia@hrms.com"; phone="0908901234"; gender="Male"; dob="1989-04-03"; hire="2020-12-15"; dept="IT"; pos="TP" }
)
$empIds = @{}
foreach ($e in $emps) {
    try {
        $b = [ordered]@{
            employeeCode = $e.code; fullName = $e.name; email = $e.email; phone = $e.phone
            gender = $e.gender; dateOfBirth = $e.dob; hireDate = $e.hire; status = "Active"
            departmentId = $deptIds[$e.dept]; positionId = $posIds[$e.pos]
        } | ConvertTo-Json
        $r = Invoke-RestMethod -Method POST -Uri "$BASE/hr/employees" -Headers $ah -Body $b
        $empIds[$e.code] = $r.data.id
        OK "Employee: $($e.name)"
    } catch { ERR "Employee $($e.name): $_" }
}

# 5. Contracts
STEP "5. Contracts"
$cts = @(
    [pscustomobject]@{ emp="NV002"; sal=18000000; start="2022-01-10" },
    [pscustomobject]@{ emp="NV003"; sal=22000000; start="2022-06-01" },
    [pscustomobject]@{ emp="NV004"; sal=20000000; start="2021-09-15" },
    [pscustomobject]@{ emp="NV005"; sal=16000000; start="2020-03-01" },
    [pscustomobject]@{ emp="NV006"; sal=15000000; start="2023-02-14" },
    [pscustomobject]@{ emp="NV007"; sal=14000000; start="2021-07-20" },
    [pscustomobject]@{ emp="NV008"; sal=13000000; start="2022-11-01" },
    [pscustomobject]@{ emp="NV009"; sal=25000000; start="2020-12-15" }
)
$idx = 1
foreach ($ct in $cts) {
    if ($empIds.ContainsKey($ct.emp)) {
        try {
            $no = "HD-2024-{0:D3}" -f $idx
            $b = [ordered]@{ contractNumber=$no; employeeId=$empIds[$ct.emp]; contractType="Chinh thuc"; startDate=$ct.start; baseSalary=$ct.sal } | ConvertTo-Json
            Invoke-RestMethod -Method POST -Uri "$BASE/hr/contracts" -Headers $ah -Body $b | Out-Null
            OK "Contract $no for $($ct.emp) - $($ct.sal) VND"
            $idx++
        } catch { ERR "Contract for $($ct.emp): $_" }
    }
}

# 6. Shifts
STEP "6. Work Shifts"
$shifts = @(
    [pscustomobject]@{ code="CA_HC"; name="Ca Hanh Chinh"; start="08:30"; end="17:30"; brk=90 },
    [pscustomobject]@{ code="CA_SANG"; name="Ca Sang"; start="08:00"; end="17:00"; brk=60 },
    [pscustomobject]@{ code="CA_CHIEU"; name="Ca Chieu"; start="13:00"; end="22:00"; brk=60 }
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
            $b = [ordered]@{ employeeId=$empIds[$ec]; shiftId=$defaultShift; startDate="2026-06-01"; endDate="2026-12-31" } | ConvertTo-Json
            Invoke-RestMethod -Method POST -Uri "$BASE/attendance/work-schedules" -Headers $ah -Body $b | Out-Null
            OK "Schedule for $ec"
        } catch { ERR "Schedule for $ec failed" }
    }
}

# 8. Payroll Rule
STEP "8. Payroll Rule"
$ruleId = $null
try {
    $b = [ordered]@{ code="QT_CHUAN"; name="Quy tac luong chuan 2024"; workDayHours=8; paidLeaveCountsAsWork=$true; overtimeRate=1.5; isActive=$true } | ConvertTo-Json
    $r = Invoke-RestMethod -Method POST -Uri "$BASE/payroll/payroll-rules" -Headers $ah -Body $b
    $ruleId = $r.data.id
    OK "Rule: Quy tac luong chuan [ID: $ruleId]"
} catch { ERR "Payroll Rule: $_" }

# 9. Payroll Period
STEP "9. Payroll Period"
if ($ruleId) {
    try {
        $b = [ordered]@{ name="Luong thang 6/2026"; fromDate="2026-06-01"; toDate="2026-06-30"; payrollRuleId=$ruleId } | ConvertTo-Json
        $r = Invoke-RestMethod -Method POST -Uri "$BASE/payroll/payroll-periods" -Headers $ah -Body $b
        $periodId = $r.data.id
        OK "Period: Luong thang 6/2026 [ID: $periodId]"
    } catch { ERR "Period: $_" }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  SEED COMPLETE!" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  5 departments | 6 positions | 8 employees"
Write-Host "  8 contracts | 3 shifts | work schedules"
Write-Host "  1 payroll rule | 1 payroll period"
Write-Host ""
Write-Host "  Visit: http://localhost:5173" -ForegroundColor Cyan
Write-Host ""
