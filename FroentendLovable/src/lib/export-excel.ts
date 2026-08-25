import * as XLSX from "xlsx";

/** Download an array of rows as an .xlsx file. */
export function exportToExcel(
  rows: Array<Record<string, string | number>>,
  fileName: string,
  sheetName = "Report",
) {
  const worksheet = XLSX.utils.json_to_sheet(rows);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, sheetName.slice(0, 31));
  XLSX.writeFile(workbook, `${fileName}.xlsx`);
}
