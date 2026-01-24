using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelieftManagement.Crawler
{
    public static class ExcelExporter
    {
        public static void ExportLocations(
            IEnumerable<LocationInfo> locations,
            string filePath)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Locations");

            // Header
            ws.Cell(1, 1).Value = "Province";
            ws.Cell(1, 2).Value = "Ward";
            ws.Cell(1, 3).Value = "Area (km²)";
            ws.Cell(1, 4).Value = "Population";

            int row = 2;
            foreach (var loc in locations)
            {
                ws.Cell(row, 1).Value = loc.Province;
                ws.Cell(row, 2).Value = loc.Ward;
                ws.Cell(row, 3).Value = loc.Area;
                ws.Cell(row, 4).Value = loc.Population;
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
    }
}
