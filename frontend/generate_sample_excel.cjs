const fs = require('fs');
const XLSX = require('xlsx');

try {
  const today = new Date();
  const formatDate = (date) => {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  };

  const tomorrow = new Date(today);
  tomorrow.setDate(today.getDate() + 1);

  const data = [
    {
      'Mã NV': 'NV001',
      'Mã Ca': 'HC',
      'Ngày Làm Việc': formatDate(today)
    },
    {
      'Mã NV': 'NV001',
      'Mã Ca': 'HC',
      'Ngày Làm Việc': formatDate(tomorrow)
    },
    {
      'Mã NV': 'NV002',
      'Mã Ca': 'HC',
      'Ngày Làm Việc': formatDate(today)
    }
  ];

  const ws = XLSX.utils.json_to_sheet(data);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, 'ScheduleTemplate');

  const outputPath = '../Mau_Nhap_Lich_Lam_Viec.xlsx';
  XLSX.writeFile(wb, outputPath);
  console.log(`Excel file successfully created at: ${outputPath}`);
} catch (err) {
  console.error('Error generating Excel file:', err);
}
