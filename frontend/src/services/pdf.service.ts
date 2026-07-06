/**
 * pdf.service.ts — Tái sử dụng cho tất cả chức năng xuất PDF trong HRMS
 * Dùng jsPDF + jspdf-autotable
 */
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'
import type { Payslip } from '../types/payroll.types'

// ─── Helpers ──────────────────────────────────────────────────────────────────
function fmtMoney(n: number): string {
  return n.toLocaleString('vi-VN') + ' VND'
}

function formatPeriod(name?: string): string {
  if (!name) return 'Ky luong'
  return name.replace(/Luong\s+thang/gi, 'Thang').replace(/Ky\s+luong/gi, 'Ky luong')
}

function today(): string {
  return new Date().toLocaleDateString('vi-VN')
}

// ─── PHIẾU LƯƠNG PDF ─────────────────────────────────────────────────────────
export function exportPayslipPdf(payslip: Payslip): void {
  const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' })
  const pageW = doc.internal.pageSize.getWidth()

  // ── Header gradient bar (rectangle)
  doc.setFillColor(16, 185, 129) // emerald-500
  doc.rect(0, 0, pageW, 28, 'F')

  // ── Company / title text
  doc.setTextColor(255, 255, 255)
  doc.setFontSize(16)
  doc.setFont('helvetica', 'bold')
  doc.text('PHIEU LUONG CHI TIET', pageW / 2, 12, { align: 'center' })

  doc.setFontSize(10)
  doc.setFont('helvetica', 'normal')
  doc.text(`Ky luong: ${formatPeriod(payslip.periodName)}`, pageW / 2, 20, { align: 'center' })

  // ── Reset color
  doc.setTextColor(30, 41, 59) // slate-800

  // ── Employee info block
  let y = 36
  doc.setFontSize(9)
  doc.setFont('helvetica', 'bold')
  doc.text('THONG TIN NHAN VIEN', 14, y)
  y += 5

  doc.setFont('helvetica', 'normal')
  doc.setFontSize(9)

  const infoLeft = [
    ['Ho va ten:', payslip.fullName],
    ['Ma nhan vien:', payslip.employeeCode],
    ['Luong co ban (HĐ):', fmtMoney(payslip.baseSalary)],
  ]
  const infoRight = [
    ['Ngay cong lam viec:', `${payslip.workedDays?.toFixed(1) ?? 0} ngay`],
    ['Nghi phep huong luong:', `${payslip.paidLeaveDays?.toFixed(1) ?? 0} ngay`],
    ['Trang thai:', payslip.status === 'Paid' ? 'Da chi tra' : payslip.status === 'Draft' ? 'Ban nhap' : 'Da chot'],
  ]

  infoLeft.forEach(([label, value], i) => {
    doc.setFont('helvetica', 'bold')
    doc.text(label, 14, y + i * 6)
    doc.setFont('helvetica', 'normal')
    doc.text(value, 60, y + i * 6)
  })
  infoRight.forEach(([label, value], i) => {
    doc.setFont('helvetica', 'bold')
    doc.text(label, 110, y + i * 6)
    doc.setFont('helvetica', 'normal')
    doc.text(value, 160, y + i * 6)
  })
  y += infoLeft.length * 6 + 6

  // Divider
  doc.setDrawColor(226, 232, 240)
  doc.line(14, y, pageW - 14, y)
  y += 6

  // ── Earnings table
  const earningItems = (payslip.items ?? []).filter((it) => it.itemType !== 'Deduction')
  const deductionItems = (payslip.items ?? []).filter((it) => it.itemType === 'Deduction')

  doc.setFont('helvetica', 'bold')
  doc.setFontSize(9)
  doc.text('CONG THEM VE', 14, y)
  y += 3

  autoTable(doc, {
    startY: y,
    margin: { left: 14, right: 14 },
    head: [['Khoan thu nhap', 'So tien (VND)']],
    body: [
      ...earningItems.map((it) => [it.name, fmtMoney(it.amount)]),
      [{ content: 'TONG THU NHAP (GROSS)', styles: { fontStyle: 'bold', fillColor: [240, 253, 244] } },
       { content: fmtMoney(payslip.grossSalary), styles: { fontStyle: 'bold', halign: 'right', fillColor: [240, 253, 244], textColor: [21, 128, 61] } }],
    ],
    styles: { fontSize: 9, cellPadding: 3 },
    headStyles: { fillColor: [16, 185, 129], textColor: 255, fontStyle: 'bold' },
    columnStyles: { 1: { halign: 'right' } },
    theme: 'striped',
  })

  y = (doc as any).lastAutoTable.finalY + 6

  // ── Deductions table
  doc.setFont('helvetica', 'bold')
  doc.setFontSize(9)
  doc.text('CAC KHOAN KHAU TRU', 14, y)
  y += 3

  autoTable(doc, {
    startY: y,
    margin: { left: 14, right: 14 },
    head: [['Khoan khau tru', 'So tien (VND)']],
    body: [
      ...(deductionItems.length > 0
        ? deductionItems.map((it) => [it.name, fmtMoney(it.amount)])
        : [['Khong co khoan khau tru nao', '—']]),
      [{ content: 'TONG KHAU TRU', styles: { fontStyle: 'bold', fillColor: [255, 241, 242] } },
       { content: '-' + fmtMoney(payslip.totalDeduction), styles: { fontStyle: 'bold', halign: 'right', fillColor: [255, 241, 242], textColor: [190, 18, 60] } }],
    ],
    styles: { fontSize: 9, cellPadding: 3 },
    headStyles: { fillColor: [244, 63, 94], textColor: 255, fontStyle: 'bold' },
    columnStyles: { 1: { halign: 'right' } },
    theme: 'striped',
  })

  y = (doc as any).lastAutoTable.finalY + 8

  // ── Net Salary box
  doc.setFillColor(16, 185, 129)
  doc.roundedRect(14, y, pageW - 28, 22, 3, 3, 'F')
  doc.setTextColor(255, 255, 255)
  doc.setFontSize(10)
  doc.setFont('helvetica', 'normal')
  doc.text('LUONG THUC LINH (NET)', 22, y + 8)
  doc.setFontSize(16)
  doc.setFont('helvetica', 'bold')
  doc.text(fmtMoney(payslip.netSalary), pageW - 22, y + 13, { align: 'right' })

  // ── Footer
  doc.setTextColor(148, 163, 184)
  doc.setFontSize(8)
  doc.setFont('helvetica', 'normal')
  doc.text(`Xuat ngay: ${today()} | HRMS Microservices System`, pageW / 2, 290, { align: 'center' })

  // ── Save
  const filename = `PhieuLuong_${payslip.employeeCode}_${formatPeriod(payslip.periodName).replace(/\s/g, '_')}.pdf`
  doc.save(filename)
}

// ─── BẢNG LƯƠNG TỔNG HỢP PDF (cho Admin/PayrollStaff) ───────────────────────
export function exportPayslipListPdf(
  payslips: Payslip[],
  periodName: string
): void {
  const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' })
  const pageW = doc.internal.pageSize.getWidth()

  doc.setFillColor(16, 185, 129)
  doc.rect(0, 0, pageW, 22, 'F')
  doc.setTextColor(255, 255, 255)
  doc.setFontSize(14)
  doc.setFont('helvetica', 'bold')
  doc.text(`BANG LUONG TONG HOP — ${formatPeriod(periodName)}`, pageW / 2, 14, { align: 'center' })

  doc.setTextColor(30, 41, 59)

  const body = payslips.map((p, i) => [
    i + 1,
    p.employeeCode,
    p.fullName,
    fmtMoney(p.baseSalary),
    p.workedDays?.toFixed(1) ?? '0',
    fmtMoney(p.grossSalary),
    '-' + fmtMoney(p.totalDeduction),
    fmtMoney(p.netSalary),
    p.status === 'Paid' ? 'Da chi tra' : p.status === 'Draft' ? 'Ban nhap' : 'Da chot',
  ])

  const totalNet = payslips.reduce((sum, p) => sum + (p.netSalary ?? 0), 0)
  const totalGross = payslips.reduce((sum, p) => sum + (p.grossSalary ?? 0), 0)

  autoTable(doc, {
    startY: 28,
    margin: { left: 10, right: 10 },
    head: [['#', 'Ma NV', 'Ho ten', 'Luong co ban', 'Ngay cong', 'Gross', 'Khau tru', 'Net (Thuc linh)', 'TT']],
    body: [
      ...body,
      [
        { content: 'TONG CONG', colSpan: 5, styles: { fontStyle: 'bold', halign: 'right', fillColor: [240, 253, 244] } },
        { content: fmtMoney(totalGross), styles: { fontStyle: 'bold', fillColor: [240, 253, 244], textColor: [21, 128, 61] } },
        { content: '', styles: { fillColor: [240, 253, 244] } },
        { content: fmtMoney(totalNet), styles: { fontStyle: 'bold', fillColor: [240, 253, 244], textColor: [21, 128, 61] } },
        { content: '', styles: { fillColor: [240, 253, 244] } },
      ],
    ],
    styles: { fontSize: 8, cellPadding: 2.5 },
    headStyles: { fillColor: [16, 185, 129], textColor: 255, fontStyle: 'bold', fontSize: 8 },
    columnStyles: {
      0: { halign: 'center', cellWidth: 8 },
      1: { cellWidth: 18 },
      4: { halign: 'center' },
      8: { halign: 'center', cellWidth: 20 },
    },
    theme: 'striped',
  })

  doc.setTextColor(148, 163, 184)
  doc.setFontSize(8)
  doc.text(`Xuat ngay: ${today()} | HRMS Microservices System`, pageW / 2, 200, { align: 'center' })

  doc.save(`BangLuong_${formatPeriod(periodName).replace(/\s/g, '_')}.pdf`)
}
