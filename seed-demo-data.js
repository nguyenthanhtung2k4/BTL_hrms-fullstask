const BASE = "http://localhost:5000/api/v1";

function OK(m) { console.log(`\x1b[32m  OK ${m}\x1b[0m`); }
function ERR(m, err) { console.error(`\x1b[31m  ERR ${m}: ${err}\x1b[0m`); }
function STEP(m) { console.log(`\n\x1b[36m--- ${m} ---\x1b[0m`); }

async function main() {
    let TOKEN = "";
    let headers = { "Content-Type": "application/json" };

    // 1. Login
    STEP("1. Login");
    try {
        const response = await fetch(`${BASE}/hr/auth/login`, {
            method: "POST",
            headers: headers,
            body: JSON.stringify({ email: "admin@hrms.com", password: "admin123" })
        });
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const r = await response.json();
        TOKEN = r.data.accessToken;
        headers["Authorization"] = `Bearer ${TOKEN}`;
        OK(`Token: ${TOKEN.substring(0, 20)}...`);
    } catch (e) {
        ERR("Login failed", e.message);
        process.exit(1);
    }

    // 2. Departments
    STEP("2. Departments");
    const depts = [
        { name: "Ban Giam Doc", code: "BGD", description: "Ban lanh dao cong ty" },
        { name: "Phong Nhan Su", code: "HR", description: "Quan ly nhan su" },
        { name: "Phong Ky Thuat", code: "IT", description: "Phat trien he thong" },
        { name: "Phong Ke Toan", code: "ACC", description: "Quan ly tai chinh" },
        { name: "Phong Kinh Doanh", code: "SALES", description: "Ban hang" }
    ];
    const deptIds = {};
    for (const d of depts) {
        try {
            const response = await fetch(`${BASE}/hr/departments`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify(d)
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const r = await response.json();
            deptIds[d.code] = r.data.id;
            OK(`Dept: ${d.name}`);
        } catch (e) {
            ERR(`Dept ${d.name} failed`, e.message);
        }
    }

    // 3. Positions
    STEP("3. Positions");
    const positions = [
        { name: "Giam Doc", code: "GD", level: 1, description: "Nguoi dung dau cong ty" },
        { name: "Truong Phong", code: "TP", level: 2, description: "Truong cac phong ban" },
        { name: "Nhan Vien", code: "NV", level: 3, description: "Nhan vien thuc thi" },
        { name: "Ky Su Phan Mem", code: "SW", level: 3, description: "Lap trinh vien" },
        { name: "Ke Toan Vien", code: "KTV", level: 3, description: "Nhan vien ke toan" },
        { name: "Chuyen Vien NS", code: "CVNS", level: 3, description: "Tuyen dung nhan su" }
    ];
    const posIds = {};
    for (const p of positions) {
        try {
            const response = await fetch(`${BASE}/hr/positions`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify(p)
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const r = await response.json();
            posIds[p.code] = r.data.id;
            OK(`Position: ${p.name}`);
        } catch (e) {
            ERR(`Position ${p.name} failed`, e.message);
        }
    }

    // 4. Employees
    STEP("4. Employees");
    const emps = [
        { code: "NV002", name: "Nguyen Thi Lan Anh", email: "lan.anh@hrms.com", phone: "0901234567", gender: "Female", dob: "1990-05-15", hire: "2022-01-10", dept: "HR", pos: "TP" },
        { code: "NV003", name: "Tran Minh Hoang",    email: "minh.hoang@hrms.com", phone: "0902345678", gender: "Male", dob: "1995-08-20", hire: "2022-06-01", dept: "IT", pos: "SW" },
        { code: "NV004", name: "Le Thi Thanh Tuyen", email: "thanh.tuyen@hrms.com", phone: "0903456789", gender: "Female", dob: "1993-03-12", hire: "2021-09-15", dept: "IT", pos: "SW" },
        { code: "NV005", name: "Pham Van Duc",       email: "van.duc@hrms.com", phone: "0904567890", gender: "Male", dob: "1988-11-30", hire: "2020-03-01", dept: "ACC", pos: "KTV" },
        { code: "NV006", name: "Hoang Thi Mai",      email: "thi.mai@hrms.com", phone: "0905678901", gender: "Female", dob: "1992-07-25", hire: "2023-02-14", dept: "ACC", pos: "KTV" },
        { code: "NV007", name: "Nguyen Quoc Bao",    email: "quoc.bao@hrms.com", phone: "0906789012", gender: "Male", dob: "1991-02-08", hire: "2021-07-20", dept: "SALES", pos: "NV" },
        { code: "NV008", name: "Vu Thi Huong",       email: "thi.huong@hrms.com", phone: "0907890123", gender: "Female", dob: "1994-09-14", hire: "2022-11-01", dept: "SALES", pos: "NV" },
        { code: "NV009", name: "Dang Huu Nghia",     email: "huu.nghia@hrms.com", phone: "0908901234", gender: "Male", dob: "1989-04-03", hire: "2020-12-15", dept: "IT", pos: "TP" }
    ];
    const empIds = {};
    for (const e of emps) {
        try {
            const body = {
                employeeCode: e.code,
                fullName: e.name,
                email: e.email,
                phone: e.phone,
                gender: e.gender,
                dateOfBirth: e.dob,
                hireDate: e.hire,
                status: "Active",
                departmentId: deptIds[e.dept],
                positionId: posIds[e.pos]
            };
            const response = await fetch(`${BASE}/hr/employees`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify(body)
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const r = await response.json();
            empIds[e.code] = r.data.id;
            OK(`Employee: ${e.name}`);
        } catch (e) {
            ERR(`Employee ${e.name} failed`, e.message);
        }
    }

    // 5. Contracts
    STEP("5. Contracts");
    const cts = [
        { emp: "NV002", sal: 18000000, start: "2022-01-10" },
        { emp: "NV003", sal: 22000000, start: "2022-06-01" },
        { emp: "NV004", sal: 20000000, start: "2021-09-15" },
        { emp: "NV005", sal: 16000000, start: "2020-03-01" },
        { emp: "NV006", sal: 15000000, start: "2023-02-14" },
        { emp: "NV007", sal: 14000000, start: "2021-07-20" },
        { emp: "NV008", sal: 13000000, start: "2022-11-01" },
        { emp: "NV009", sal: 25000000, start: "2020-12-15" }
    ];
    let idx = 1;
    for (const ct of cts) {
        if (empIds[ct.emp]) {
            try {
                const no = `HD-2024-${String(idx).padStart(3, '0')}`;
                const body = {
                    contractNumber: no,
                    employeeId: empIds[ct.emp],
                    contractType: "Chinh thuc",
                    startDate: ct.start,
                    baseSalary: ct.sal
                };
                const response = await fetch(`${BASE}/hr/contracts`, {
                    method: "POST",
                    headers: headers,
                    body: JSON.stringify(body)
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                OK(`Contract ${no} for ${ct.emp} - ${ct.sal} VND`);
                idx++;
            } catch (e) {
                ERR(`Contract for ${ct.emp} failed`, e.message);
            }
        }
    }

    // 6. Shifts
    STEP("6. Work Shifts");
    const shifts = [
        { code: "CA_HC", name: "Ca Hanh Chinh", start: "08:30:00", end: "17:30:00", brk: 90 },
        { code: "CA_SANG", name: "Ca Sang", start: "08:00:00", end: "17:00:00", brk: 60 },
        { code: "CA_CHIEU", name: "Ca Chieu", start: "13:00:00", end: "22:00:00", brk: 60 }
    ];
    const shiftIds = {};
    for (const sh of shifts) {
        try {
            const body = {
                code: sh.code,
                name: sh.name,
                startTime: sh.start,
                endTime: sh.end,
                breakMinutes: sh.brk
            };
            const response = await fetch(`${BASE}/attendance/shifts`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify(body)
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const r = await response.json();
            shiftIds[sh.code] = r.data.id;
            OK(`Shift: ${sh.name}`);
        } catch (e) {
            ERR(`Shift ${sh.name} failed`, e.message);
        }
    }

    // 7. Work Schedules
    STEP("7. Work Schedules");
    const defaultShift = shiftIds["CA_HC"];
    if (defaultShift) {
        for (const ec of Object.keys(empIds)) {
            try {
                const body = {
                    employeeId: empIds[ec],
                    shiftId: defaultShift,
                    startDate: "2026-06-01",
                    endDate: "2026-12-31"
                };
                const response = await fetch(`${BASE}/attendance/work-schedules`, {
                    method: "POST",
                    headers: headers,
                    body: JSON.stringify(body)
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                OK(`Schedule for ${ec}`);
            } catch (e) {
                ERR(`Schedule for ${ec} failed`, e.message);
            }
        }
    }

    // 8. Payroll Rule
    STEP("8. Payroll Rule");
    let ruleId = null;
    try {
        const body = {
            code: "QT_CHUAN",
            name: "Quy tac luong chuan 2024",
            workDayHours: 8,
            paidLeaveCountsAsWork: true,
            overtimeRate: 1.5,
            isActive: true
        };
        const response = await fetch(`${BASE}/payroll/payroll-rules`, {
            method: "POST",
            headers: headers,
            body: JSON.stringify(body)
        });
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const r = await response.json();
        ruleId = r.data.id;
        OK(`Rule: Quy tac luong chuan [ID: ${ruleId}]`);
    } catch (e) {
        ERR("Payroll Rule failed", e.message);
    }

    // 9. Payroll Period
    STEP("9. Payroll Period");
    if (ruleId) {
        try {
            const body = {
                name: "Luong thang 6/2026",
                fromDate: "2026-06-01",
                toDate: "2026-06-30",
                payrollRuleId: ruleId
            };
            const response = await fetch(`${BASE}/payroll/payroll-periods`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify(body)
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const r = await response.json();
            const periodId = r.data.id;
            OK(`Period: Luong thang 6/2026 [ID: ${periodId}]`);
        } catch (e) {
            ERR("Period failed", e.message);
        }
    }

    console.log("\n\x1b[35m============================================\x1b[0m");
    console.log("\x1b[35m  SEED COMPLETE!\x1b[0m");
    console.log("\x1b[35m============================================\x1b[0m");
}

main().catch(err => {
    console.error("Unhandled exception:", err);
    process.exit(1);
});
