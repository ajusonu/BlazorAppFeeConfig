using OfficeOpenXml;
using System;
using System.Collections.Generic;

namespace FeesAutomationWebsite.Common
{
    public class ColumnMapper
    {
        /// <summary>
        /// internal map for the column names
        /// </summary>
        private Dictionary<string, int> columnMap;

        /// <summary>
        /// Maps all the column headers in the spreadsheet
        /// </summary>
        /// <param name="sheet"></param>
        public void Initialize(ExcelWorksheet sheet)
        {
            // clear any existing map
            columnMap = new Dictionary<string, int>();

            // find all the column headers and map them for later use
            for (int i = 1; i <= sheet.Dimension.Columns; i++)
            {
                string header = sheet.GetCellValue(1, i).Replace(" ", "");
                if (!string.IsNullOrEmpty(header))
                {
                    columnMap.Add(header.Trim().ToLower(), i);
                }
            }
        }

        /// <summary>
        /// Gets the column Id from the header
        /// </summary>
        /// <param name="columnHeader"></param>
        /// <returns></returns>
        public int GetColumnId(string columnHeader)
        {
            columnHeader = columnHeader.Replace("\"", "").Replace(" ", "");
            if (columnMap.ContainsKey(columnHeader.ToLower()))
                return columnMap[columnHeader.ToLower()];

            throw new Exception($"Missing Column ({columnHeader})");
        }
    }
}