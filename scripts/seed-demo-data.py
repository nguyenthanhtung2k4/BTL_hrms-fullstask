import urllib.request
import json
import sys
import time

BASE = "http://localhost:5000/api/v1"

def request(method, path, data=None, token=None):
    url = f"{BASE}{path}"
    headers = {"Content-Type": "application/json; charset=utf-8"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    
    req_data = json.dumps(data).encode("utf-8") if data is not None else None
    req = urllib.request.Request(url, data=req_data, headers=headers, method=method)
    
    try:
        with urllib.request.urlopen(req) as response:
            res_body = response.read().decode("utf-8")
            return json.loads(res_body)
    except urllib.error.HTTPError as e:
        print(f"  ERR {path}: {e.code} {e.reason}")
        try:
            print(e.read().decode("utf-8"))
        except:
            pass
        raise e
    except Exception as e:
        print(f"  ERR {path}: {e}")
        raise e

def main():
    print("--- 1. Login ---")
    token = None
    for attempt in range(5):
        try:
            r = request("POST", "/hr/auth/login", {"email": "admin@hrms.com", "password": "admin123"})
            token = r["data"]["accessToken"]
            print(f"  OK Token: {token[:20]}...")
            break
        except Exception as e:
            print(f"  Login attempt {attempt+1} failed, retrying in 3 seconds...")
            time.sleep(3)
    
    if not token:
        print("Login failed after 5 attempts. Make sure the backend services and gateway are running.")
        sys.exit(1)

    print("\n--- 2. Departments ---")
    depts = [
        {"name": "Ban Giam Doc", "code": "BGD", "description": "Ban lanh dao cong ty"},
        {"name": "Phong Nhan Su", "code": "HR", "description": "Quan ly nhan su"},
        {"name": "Phong Ky Thuat", "code": "IT", "description": "Phat trien he thong"},
        {"name": "Phong Ke Toan", "code": "ACC", "description": "Quan ly tai chinh"},
        {"name": "Phong Kinh Doanh", "code": "SALES", "description": "Ban hang"}
    ]
    dept_ids = {}
    for d in depts:
        try:
            r = request("POST", "/hr/departments", d, token)
            dept_ids[d["code"]] = r["data"]["id"]
            print(f"  OK Dept: {d['name']}")
        except Exception as e:
            pass

    print("\n--- 3. Positions ---")
    positions = [
        {"name": "Giam Doc", "code": "GD", "level": 1, "description": "Nguoi dung dau cong ty"},
        {"name": "Truong Phong", "code": "TP", "level": 2, "description": "Truong cac phong ban"},
        {"name": "Nhan Vien", "code": "NV", "level": 3, "description": "Nhan vien thuc thi"},
        {"name": "Ky Su Phan Mem", "code": "SW", "level": 3, "description": "Lap trinh vien"},
        {"name": "Ke Toan Vien", "code": "KTV", "level": 3, "description": "Nhan vien ke toan"},
        {"name": "Chuyen Vien NS", "code": "CVNS", "level": 3, "description": "Tuyen dung nhan su"}
    ]
    pos_ids = {}
    for p in positions:
        try:
            r = request("POST", "/hr/positions", p, token)
            pos_ids[p["code"]] = r["data"]["id"]
            print(f"  OK Position: {p['name']}")
        except Exception as e:
            pass

    print("\n--- 4. Employees ---")
    emps = [
        {"code": "NV002", "name": "Nguyen Thi Lan Anh", "email": "lan.anh@hrms.com", "phone": "0901234567", "gender": "Female", "dob": "1990-05-15", "hire": "2022-01-10", "dept": "HR", "pos": "TP"},
        {"code": "NV003", "name": "Tran Minh Hoang",    "email": "minh.hoang@hrms.com", "phone": "0902345678", "gender": "Male", "dob": "1995-08-20", "hire": "2022-06-01", "dept": "IT", "pos": "SW"},
        {"code": "NV004", "name": "Le Thi Thanh Tuyen", "email": "thanh.tuyen@hrms.com", "phone": "0903456789", "gender": "Female", "dob": "1993-03-12", "hire": "2021-09-15", "dept": "IT", "pos": "SW"},
        {"code": "NV005", "name": "Pham Van Duc",       "email": "van.duc@hrms.com", "phone": "0904567890", "gender": "Male", "dob": "1988-11-30", "hire": "2020-03-01", "dept": "ACC", "pos": "KTV"},
        {"code": "NV006", "name": "Hoang Thi Mai",      "email": "thi.mai@hrms.com", "phone": "0905678901", "gender": "Female", "dob": "1992-07-25", "hire": "2023-02-14", "dept": "ACC", "pos": "KTV"},
        {"code": "NV007", "name": "Nguyen Quoc Bao",    "email": "quoc.bao@hrms.com", "phone": "0906789012", "gender": "Male", "dob": "1991-02-08", "hire": "2021-07-20", "dept": "SALES", "pos": "NV"},
        {"code": "NV008", "name": "Vu Thi Huong",       "email": "thi.huong@hrms.com", "phone": "0907890123", "gender": "Female", "dob": "1994-09-14", "hire": "2022-11-01", "dept": "SALES", "pos": "NV"},
        {"code": "NV009", "name": "Dang Huu Nghia",     "email": "huu.nghia@hrms.com", "phone": "0908901234", "gender": "Male", "dob": "1989-04-03", "hire": "2020-12-15", "dept": "IT", "pos": "TP"}
    ]
    emp_ids = {}
    for e in emps:
        try:
            body = {
                "employeeCode": e["code"],
                "fullName": e["name"],
                "email": e["email"],
                "phone": e["phone"],
                "gender": e["gender"],
                "dateOfBirth": e["dob"],
                "hireDate": e["hire"],
                "status": "Active",
                "departmentId": dept_ids.get(e["dept"]),
                "positionId": pos_ids.get(e["pos"])
            }
            r = request("POST", "/hr/employees", body, token)
            emp_ids[e["code"]] = r["data"]["id"]
            print(f"  OK Employee: {e['name']}")
        except Exception as e:
            pass

    print("\n--- 5. Contracts ---")
    cts = [
        {"emp": "NV002", "sal": 18000000, "start": "2022-01-10"},
        {"emp": "NV003", "sal": 22000000, "start": "2022-06-01"},
        {"emp": "NV004", "sal": 20000000, "start": "2021-09-15"},
        {"emp": "NV005", "sal": 16000000, "start": "2020-03-01"},
        {"emp": "NV006", "sal": 15000000, "start": "2023-02-14"},
        {"emp": "NV007", "sal": 14000000, "start": "2021-07-20"},
        {"emp": "NV008", "sal": 13000000, "start": "2022-11-01"},
        {"emp": "NV009", "sal": 25000000, "start": "2020-12-15"}
    ]
    idx = 1
    for ct in cts:
        emp_code = ct["emp"]
        if emp_code in emp_ids:
            try:
                contract_no = f"HD-2024-{idx:03d}"
                body = {
                    "contractNumber": contract_no,
                    "employeeId": emp_ids[emp_code],
                    "contractType": "Chinh thuc",
                    "startDate": ct["start"],
                    "baseSalary": ct["sal"]
                }
                request("POST", "/hr/contracts", body, token)
                print(f"  OK Contract {contract_no} for {emp_code} - {ct['sal']} VND")
                idx += 1
            except Exception as e:
                pass

    print("\n--- 6. Work Shifts ---")
    shifts = [
        {"code": "CA_HC", "name": "Ca Hanh Chinh", "start": "08:30", "end": "17:30", "brk": 90},
        {"code": "CA_SANG", "name": "Ca Sang", "start": "08:00", "end": "17:00", "brk": 60},
        {"code": "CA_CHIEU", "name": "Ca Chieu", "start": "13:00", "end": "22:00", "brk": 60}
    ]
    shift_ids = {}
    for sh in shifts:
        try:
            body = {
                "code": sh["code"],
                "name": sh["name"],
                "startTime": sh["start"],
                "endTime": sh["end"],
                "breakMinutes": sh["brk"]
            }
            r = request("POST", "/attendance/shifts", body, token)
            shift_ids[sh["code"]] = r["data"]["id"]
            print(f"  OK Shift: {sh['name']}")
        except Exception as e:
            pass

    print("\n--- 7. Work Schedules ---")
    default_shift = shift_ids.get("CA_HC")
    if default_shift:
        for ec in emp_ids.keys():
            try:
                body = {
                    "employeeId": emp_ids[ec],
                    "shiftId": default_shift,
                    "startDate": "2026-06-01",
                    "endDate": "2026-12-31"
                }
                request("POST", "/attendance/work-schedules", body, token)
                print(f"  OK Schedule for {ec}")
            except Exception as e:
                pass

    print("\n--- 8. Payroll Rule ---")
    rule_id = None
    try:
        body = {
            "code": "QT_CHUAN",
            "name": "Quy tac luong chuan 2024",
            "workDayHours": 8,
            "paidLeaveCountsAsWork": True,
            "overtimeRate": 1.5,
            "isActive": True
        }
        r = request("POST", "/payroll/payroll-rules", body, token)
        rule_id = r["data"]["id"]
        print(f"  OK Rule: Quy tac luong chuan [ID: {rule_id}]")
    except Exception as e:
        pass

    print("\n--- 9. Payroll Period ---")
    if rule_id:
        try:
            body = {
                "name": "Luong thang 6/2026",
                "fromDate": "2026-06-01",
                "toDate": "2026-06-30",
                "payrollRuleId": rule_id
            }
            r = request("POST", "/payroll/payroll-periods", body, token)
            print(f"  OK Period: Luong thang 6/2026 [ID: {r['data']['id']}]")
        except Exception as e:
            pass

    print("\n============================================")
    print("  SEED COMPLETE!")
    print("============================================")

if __name__ == "__main__":
    main()
