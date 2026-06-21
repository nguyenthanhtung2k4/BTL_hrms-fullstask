import * as XLSX from 'xlsx'

/**
 * Exports an array of objects to an Excel (.xlsx) file.
 * @param data Array of objects to be exported.
 * @param fileName Name of the output file (without extension).
 * @param sheetName Name of the worksheet inside the Excel file.
 */
export function exportToExcel(data: any[], fileName: string, sheetName: string = 'Data') {
  if (!data || data.length === 0) {
    throw new Error('Không có dữ liệu để xuất.')
  }

  // Create a worksheet from the JSON data
  const worksheet = XLSX.utils.json_to_sheet(data)
  
  // Create a new workbook
  const workbook = XLSX.utils.book_new()
  
  // Append the worksheet to the workbook
  XLSX.utils.book_append_sheet(workbook, worksheet, sheetName)
  
  // Generate buffer and trigger browser download
  XLSX.writeFile(workbook, `${fileName}.xlsx`)
}

/**
 * Reads an uploaded Excel file and converts it to an array of objects.
 * @param file The File object from an input element.
 * @returns A promise resolving to an array of objects representing rows.
 */
export function parseExcelFile(file: File): Promise<any[]> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    
    reader.onload = (e) => {
      try {
        const data = e.target?.result
        if (!data) {
          reject(new Error('Không thể đọc nội dung file.'))
          return
        }

        const workbook = XLSX.read(data, { type: 'binary' })
        const firstSheetName = workbook.SheetNames[0]
        const worksheet = workbook.Sheets[firstSheetName]
        
        // Convert to array of objects
        const jsonData = XLSX.utils.sheet_to_json(worksheet)
        resolve(jsonData)
      } catch (err) {
        reject(err)
      }
    }
    
    reader.onerror = (err) => reject(err)
    reader.readAsBinaryString(file)
  })
}
