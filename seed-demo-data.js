const BASE = "http://localhost:5005/api/v1";

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
        { name: "Ban Giám Đốc", code: "BGD", description: "Ban lãnh đạo công ty" },
        { name: "Phòng Nhân Sự", code: "HR", description: "Quản lý nhân sự" },
        { name: "Phòng Kỹ Thuật", code: "IT", description: "Phát triển hệ thống và AI" },
        { name: "Phòng Kế Toán", code: "ACC", description: "Quản lý tài chính" },
        { name: "Phòng Kinh Doanh", code: "SALES", description: "Kinh doanh và Marketing" }
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
        { name: "Giám Đốc", code: "GD", level: 1, description: "Người đứng đầu công ty" },
        { name: "Trưởng Phòng", code: "TP", level: 2, description: "Trưởng các phòng ban" },
        { name: "Nhân Viên", code: "NV", level: 3, description: "Nhân viên thực thi" },
        { name: "Kỹ Sư Phần Mềm", code: "SW", level: 3, description: "Lập trình viên" },
        { name: "Kế Toán Viên", code: "KTV", level: 3, description: "Nhân viên kế toán" },
        { name: "Chuyên Viên NS", code: "CVNS", level: 3, description: "Tuyển dụng và quản lý nhân sự" }
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

    // 4. Employees Generator
    STEP("4. Employees");
    const viLast = ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý"];
    const viMale = ["Thế Dân", "Minh Hoàng", "Văn Đức", "Quốc Bảo", "Hữu Nghĩa", "Văn Minh", "Minh Trí", "Thanh Tùng", "Tuấn Anh", "Đức Nam", "Quang Huy", "Khánh Duy", "Xuân Trường"];
    const viFemale = ["Thị Lan Anh", "Thị Thanh Tuyền", "Thị Mai", "Thị Hương", "Thị Thủy", "Bích Phương", "Ngọc Trinh", "Thanh Hà", "Hồng Vân", "Thùy Linh", "Hải Yến"];

    const enMale = ["David Miller", "Jean-Luc Picard", "Rajesh Kumar", "John Smith", "Park Ji-sung", "Michael Johnson", "Alexandre Dupont", "Hans Schmidt", "Yuki Tanaka", "Daniel Evans"];
    const enFemale = ["Sarah Jenkins", "Elena Rostova", "Emily Watson", "Sophia Martinez", "Chloe Dubois", "Maria Rossi", "Anna Johansson", "Li Wei", "Priya Patel"];

    const emps = [
        { code: "NV001", name: "Nguyễn Thế Dân", email: "dan.the@hrms.com", phone: "0901112223", gender: "Male", dob: "1980-01-01", hire: "2020-01-01", dept: "BGD", pos: "GD", manager: null, sal: 150000000 },
        { code: "NV002", name: "David Miller", email: "david.miller@hrms.com", phone: "0901234567", gender: "Male", dob: "1985-04-12", hire: "2020-01-01", dept: "BGD", pos: "GD", manager: "NV001", sal: 120000000 },
        { code: "NV003", name: "Nguyễn Thị Lan Anh", email: "lan.anh@hrms.com", phone: "0902345678", gender: "Female", dob: "1990-05-15", hire: "2022-01-10", dept: "HR", pos: "TP", manager: "NV001", sal: 25000000 },
        { code: "NV004", name: "Phạm Văn Đức", email: "van.duc@hrms.com", phone: "0903456789", gender: "Male", dob: "1988-11-30", hire: "2020-03-01", dept: "ACC", pos: "TP", manager: "NV001", sal: 30000000 },
        { code: "NV005", name: "Nguyễn Quốc Bảo", email: "quoc.bao@hrms.com", phone: "0904567890", gender: "Male", dob: "1991-02-08", hire: "2021-07-20", dept: "SALES", pos: "TP", manager: "NV001", sal: 28000000 },
        { code: "NV006", name: "Đặng Hữu Nghĩa", email: "huu.nghia@hrms.com", phone: "0905678901", gender: "Male", dob: "1989-04-03", hire: "2020-12-15", dept: "IT", pos: "TP", manager: "NV002", sal: 60000000 }
    ];

    for (let i = 7; i <= 150; i++) {
        const code = `NV${String(i).padStart(3, '0')}`;
        const gender = (i % 2 === 0) ? "Female" : "Male";
        const isForeign = (i % 4 === 0);

        let name = "";
        if (isForeign) {
            name = (gender === "Male") ? enMale[i % enMale.length] : enFemale[i % enFemale.length];
        } else {
            const last = viLast[i % viLast.length];
            const first = (gender === "Male") ? viMale[i % viMale.length] : viFemale[i % viFemale.length];
            name = `${last} ${first}`;
        }

        const mod = i % 10;
        let dept = "IT";
        let pos = "SW";
        let manager = "NV006";
        let sal = 20000000 + (i % 7) * 5000000;

        if (mod === 6) {
            dept = "HR";
            pos = "CVNS";
            manager = "NV003";
            sal = 15000000 + (i % 5) * 2000000;
        } else if (mod === 7) {
            dept = "ACC";
            pos = "KTV";
            manager = "NV004";
            sal = 15000000 + (i % 5) * 2000000;
        } else if (mod === 8 || mod === 9) {
            dept = "SALES";
            pos = "NV";
            manager = "NV005";
            sal = 12000000 + (i % 6) * 2000000;
        }

        const emailPrefix = name.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase().replace(/ /g, "").replace(/đ/g, "d");
        const email = `${emailPrefix}${i}@hrms.com`;
        const phone = `090${1000000 + i * 123}`;

        const dobYear = 1980 + (i % 20);
        const dobMonth = 1 + (i % 12);
        const dobDay = 1 + (i % 28);
        const dob = `${dobYear}-${String(dobMonth).padStart(2, '0')}-${String(dobDay).padStart(2, '0')}`;

        const hireYear = 2021 + (i % 4);
        const hireMonth = 1 + (i % 12);
        const hireDay = 1 + (i % 28);
        const hire = `${hireYear}-${String(hireMonth).padStart(2, '0')}-${String(hireDay).padStart(2, '0')}`;

        emps.push({ code, name, email, phone, gender, dob, hire, dept, pos, manager, sal });
    }

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
                positionId: posIds[e.pos],
                managerEmployeeId: e.manager ? empIds[e.manager] : null
            };
            const response = await fetch(`${BASE}/hr/employees`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify(body)
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const r = await response.json();
            empIds[e.code] = r.data.id;
            OK(`Employee: ${e.name} (${e.code})`);
        } catch (e) {
            ERR(`Employee ${e.name} failed`, e.message);
        }
    }

    // 5. Contracts
    STEP("5. Contracts");
    let idx = 1;
    for (const e of emps) {
        if (empIds[e.code]) {
            try {
                const no = `HD-2024-${String(idx).padStart(3, '0')}`;
                
                const hireParts = e.hire.split("-");
                const hireYear = parseInt(hireParts[0]);
                const hireMonth = hireParts[1];
                const hireDay = hireParts[2];
                
                let endDate = null;
                const empNum = parseInt(e.code.replace("NV", ""));
                if (e.code !== "NV001" && e.code !== "NV002" && e.code !== "NV003" && e.code !== "NV004" && e.code !== "NV005" && e.code !== "NV006") {
                    if (empNum % 3 === 0) {
                        // 1-year contract
                        endDate = `${hireYear + 1}-${hireMonth}-${hireDay}`;
                    } else if (empNum % 3 === 1) {
                        // 3-year contract
                        endDate = `${hireYear + 3}-${hireMonth}-${hireDay}`;
                    }
                }

                const body = {
                    contractNumber: no,
                    employeeId: empIds[e.code],
                    contractType: "Chính thức",
                    startDate: e.hire,
                    endDate: endDate,
                    baseSalary: e.sal
                };
                const response = await fetch(`${BASE}/hr/contracts`, {
                    method: "POST",
                    headers: headers,
                    body: JSON.stringify(body)
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                const r = await response.json();
                const contractId = r.data.id;

                if (endDate) {
                    const endD = new Date(endDate);
                    const now = new Date("2026-07-06");
                    if (endD < now) {
                        // Update status to Expired
                        const updateBody = {
                            contractType: "Chính thức",
                            startDate: e.hire,
                            endDate: endDate,
                            baseSalary: e.sal,
                            status: "Expired",
                            attachmentUrl: null
                        };
                        const putResponse = await fetch(`${BASE}/hr/contracts/${contractId}`, {
                            method: "PUT",
                            headers: headers,
                            body: JSON.stringify(updateBody)
                        });
                        if (!putResponse.ok) throw new Error(`PUT error! status: ${putResponse.status}`);
                        OK(`Contract ${no} for ${e.code} marked as EXPIRED (Ended: ${endDate})`);
                    } else {
                        OK(`Contract ${no} for ${e.code} - ${e.sal} VND (Ends: ${endDate})`);
                    }
                } else {
                    OK(`Contract ${no} for ${e.code} - ${e.sal} VND (Indefinite)`);
                }
                idx++;
            } catch (e) {
                ERR(`Contract for ${e.code} failed`, e.message);
            }
        }
    }

    // 6. Shifts
    STEP("6. Work Shifts");
    const shifts = [
        { code: "CA_HC", name: "Ca Hành Chính", start: "08:30:00", end: "17:30:00", brk: 90 },
        { code: "CA_SANG", name: "Ca Sáng", start: "08:00:00", end: "17:00:00", brk: 60 },
        { code: "CA_CHIEU", name: "Ca Chiều", start: "13:00:00", end: "22:00:00", brk: 60 }
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
                    startDate: "2025-07-01",
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
            name: "Quy tắc lương chuẩn 2025-2026",
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
        OK(`Rule: Quy tắc lương chuẩn [ID: ${ruleId}]`);
    } catch (e) {
        ERR("Payroll Rule failed", e.message);
    }

    // 9. Payroll Period
    STEP("9. Payroll Period");
    if (ruleId) {
        try {
            const body = {
                code: "PERIOD_2026_06",
                name: "Lương tháng 6/2026",
                fromDate: "2026-06-01",
                toDate: "2026-06-30",
                standardWorkDays: 22,
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
            OK(`Period: Lương tháng 6/2026 [ID: ${periodId}]`);
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
