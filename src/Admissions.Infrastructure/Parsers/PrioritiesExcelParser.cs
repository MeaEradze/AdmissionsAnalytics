using Admissions.Application.Common;
using Admissions.Application.Imports;
using ClosedXML.Excel;

namespace Admissions.Infrastructure.Parsers;

public class PrioritiesExcelParser : IPrioritiesFileParser
{
    public PriorityParseResult Parse(Stream file)
    {
        var errors = new List<string>();
        var rows = new List<PriorityRow>();
        int rowsRead = 0;

        using var workbook = OpenWorkbook(file);
        var sheet = workbook.Worksheets.First();
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int r = 2; r <= lastRow; r++)
        {
            string uniCode = CellText(sheet, r, 1);
            string uniName = CellText(sheet, r, 2);
            string progCode = CellText(sheet, r, 3);
            string progName = CellText(sheet, r, 4);

            if (uniCode.Length == 0 && progCode.Length == 0)
            {
                continue;
            }

            rowsRead++;

            if (progCode.Length < 7 || !progCode.All(char.IsDigit))
            {
                errors.Add($"სტრიქონი {r}: არასწორი პროგრამის კოდი „{progCode}“.");
                continue;
            }

            uniCode = uniCode.PadLeft(3, '0');
            progCode = progCode[..7];

            var counts = new int[10];
            for (int i = 0; i < 10; i++)
            {
                counts[i] = CellInt(sheet, r, 5 + i);
            }

            int total = CellInt(sheet, r, 15);

            rows.Add(new PriorityRow(uniCode, uniName, progCode, progName, counts, total));
        }

        return new PriorityParseResult(rowsRead, rows, errors);
    }

    private static XLWorkbook OpenWorkbook(Stream file)
    {
        try
        {
            return new XLWorkbook(file);
        }
        catch (Exception ex)
        {
            throw new InvalidFileFormatException(
                $"ფაილის ფორმატი არასწორია — საჭიროა .xlsx Excel ფაილი. ({ex.Message})");
        }
    }

    private static int CellInt(IXLWorksheet sheet, int row, int col)
    {
        var cell = sheet.Cell(row, col);
        if (cell.IsEmpty())
        {
            return 0;
        }

        if (cell.TryGetValue(out double numeric))
        {
            return (int)numeric;
        }

        return int.TryParse(cell.GetString().Trim(), out int parsed) ? parsed : 0;
    }

    private static string CellText(IXLWorksheet sheet, int row, int col) =>
        sheet.Cell(row, col).GetString().Trim();
}
