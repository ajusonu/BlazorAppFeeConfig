using OfficeOpenXml;
using System;

namespace FeesAutomationWebsite.Common
{
    public static class ExcelWorksheetExtension
    {
        /// <summary>
        /// Get Cell Value 
        /// </summary>
        /// <param name="excelWorksheet"></param>
        /// <param name="rowId"></param>
        /// <param name="columnId"></param>
        /// <returns></returns>
        public static string GetCellValue(this ExcelWorksheet excelWorksheet, int rowId, int columnId)
        {
            try
            {
                return excelWorksheet.Cells[rowId, columnId].Text.TidyCellValue();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error {ex.ToString()} Getting Column value at (row no: {rowId} col no: {columnId})");
            }
        }

        /// <summary>
        /// Get Cell value converted to double
        /// </summary>
        /// <param name="excelWorksheet"></param>
        /// <param name="rowId"></param>
        /// <param name="columnId"></param>
        /// <returns></returns>
        public static decimal GetCellDollarValue(this ExcelWorksheet excelWorksheet, int rowId, int columnId)
        {
            string cellValue = "";
            try
            {
                cellValue = excelWorksheet.GetCellValue(rowId, columnId);
                return decimal.Parse(cellValue);
            }
            catch
            {
                throw new Exception($"Error Getting Dollar value {cellValue} for header ({excelWorksheet.GetCellValue(1, columnId)})");
            }
        }

        /// <summary>
        /// Get Cell value converted to Boolean
        /// </summary>
        /// <param name="excelWorksheet"></param>
        /// <param name="rowId"></param>
        /// <param name="columnId"></param>
        /// <returns></returns>
        public static bool GetCellBooleanValue(this ExcelWorksheet excelWorksheet, int rowId, int columnId)
        {
            string cellValue = "";
            try
            {
                cellValue = excelWorksheet.GetCellValue(rowId, columnId);
                return bool.Parse(cellValue);
            }
            catch
            {
                throw new Exception($"Error Getting Boolean value {cellValue} for header ({excelWorksheet.GetCellValue(1, columnId)})");
            }
        }

        /// <summary>
        /// Get Cell value converted to long
        /// </summary>
        /// <param name="excelWorksheet"></param>
        /// <param name="rowId"></param>
        /// <param name="columnId"></param>
        /// <returns></returns>
        public static long GetCellLongValue(this ExcelWorksheet excelWorksheet, int rowId, int columnId)
        {
            string cellValue = "";
            try
            {
                cellValue = excelWorksheet.GetCellValue(rowId, columnId);
                return long.Parse(cellValue);
            }
            catch (Exception)
            {
                throw new Exception($"Error Getting Dollar value {cellValue} for header ({excelWorksheet.GetCellValue(1, columnId)})");
            }
        }

    }
}