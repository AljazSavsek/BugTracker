using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using BugTracker.Models;

namespace BugTracker.Helpers
{
    public static class ExcelHelper
    {
        public static string Export(List<BugItem> bugs)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"BugTracker_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Napake");

            // Glava
            string[] headers = { "ID","Naslov","Status","Prioriteta","Kategorija","Dodeljen","Ustvaritel","Datum" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Podatki
            for (int i = 0; i < bugs.Count; i++)
            {
                var b = bugs[i]; int row = i + 2;
                ws.Cell(row, 1).Value = b.IdNapake;
                ws.Cell(row, 2).Value = b.Naslov;
                ws.Cell(row, 3).Value = b.Status;
                ws.Cell(row, 4).Value = b.Prioriteta;
                ws.Cell(row, 5).Value = b.Kategorija;
                ws.Cell(row, 6).Value = b.Dodeljen;
                ws.Cell(row, 7).Value = b.Ustvaritelj;
                ws.Cell(row, 8).Value = b.DatumUstvarjeno.ToString("dd.MM.yyyy");

                ws.Cell(row, 3).Style.Font.FontColor = b.Status switch
                {
                    "Odprt"  => XLColor.FromHtml("#E74C3C"),
                    "V delu" => XLColor.FromHtml("#F39C12"),
                    "Rešen"  => XLColor.FromHtml("#27AE60"),
                    _        => XLColor.Gray
                };
                ws.Cell(row, 4).Style.Font.FontColor = b.Prioriteta switch
                {
                    "Kritična" => XLColor.FromHtml("#E74C3C"),
                    "Visoka"   => XLColor.FromHtml("#E67E22"),
                    "Srednja"  => XLColor.FromHtml("#F39C12"),
                    _          => XLColor.FromHtml("#27AE60")
                };
                if (i % 2 == 1)
                    ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8F9FA");
            }

            ws.Columns().AdjustToContents();
            ws.Column(2).Width = Math.Min(ws.Column(2).Width, 50);
            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, bugs.Count + 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, bugs.Count + 1, 8).Style.Border.InsideBorder  = XLBorderStyleValues.Hair;

            wb.SaveAs(path);
            return path;
        }
    }
}
