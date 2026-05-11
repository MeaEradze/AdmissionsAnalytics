using Admissions.Application.Common;
using Admissions.Application.Imports;
using ClosedXML.Excel;

namespace Admissions.Infrastructure.Parsers;

public class EnrollmentExcelParser : IEnrollmentFileParser
{
    public EnrollmentParseResult Parse(Stream file)
    {
        var errors = new List<string>();
        var groups = new Dictionary<(string Uni, string Prog), MutableAggregate>();
        int rowsRead = 0;

        using var workbook = OpenWorkbook(file);
        var sheet = workbook.Worksheets.First();
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int r = 2; r <= lastRow; r++)
        {
            string uniCode = CellText(sheet, r, 1);
            string uniName = CellText(sheet, r, 2);
            string rawCode = CellText(sheet, r, 3);
            string progName = CellText(sheet, r, 4);

            if (uniCode.Length == 0 && rawCode.Length == 0)
            {
                continue;
            }

            rowsRead++;

            if (rawCode.Length < 7 || !rawCode.All(char.IsDigit))
            {
                errors.Add($"სტრიქონი {r}: არასწორი პროგრამის კოდი „{rawCode}“.");
                continue;
            }

            if (uniCode.Length == 0)
            {
                errors.Add($"სტრიქონი {r}: უსდ კოდი ცარიელია.");
                continue;
            }

            string baseCode = rawCode[..7];
            uniCode = uniCode.PadLeft(3, '0');

            int? grantPct = ReadGrantPercent(sheet, r);

            var key = (uniCode, baseCode);
            if (!groups.TryGetValue(key, out var agg))
            {
                agg = new MutableAggregate(uniCode, uniName, baseCode, progName);
                groups[key] = agg;
            }

            agg.Enrolled++;
            if (grantPct == 100)
            {
                agg.GrantFull++;
            }
            else if (grantPct is 50 or 70)
            {
                agg.GrantPartial++;
            }
        }

        var aggregates = groups.Values
            .Select(a => new EnrollmentAggregate(
                a.UniversityCode, a.UniversityName, a.ProgramCode, a.ProgramName,
                a.Enrolled, a.GrantFull, a.GrantPartial))
            .ToList();

        return new EnrollmentParseResult(rowsRead, aggregates, errors);
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

    private static int? ReadGrantPercent(IXLWorksheet sheet, int row)
    {
        var cell = sheet.Cell(row, 18);
        if (cell.IsEmpty())
        {
            return null;
        }

        if (cell.TryGetValue(out double numeric))
        {
            return (int)numeric;
        }

        string text = cell.GetString().Trim();
        return int.TryParse(text, out int parsed) ? parsed : null;
    }

    private static string CellText(IXLWorksheet sheet, int row, int col) =>
        sheet.Cell(row, col).GetString().Trim();

    private sealed class MutableAggregate
    {
        public MutableAggregate(string uniCode, string uniName, string progCode, string progName)
        {
            UniversityCode = uniCode;
            UniversityName = uniName;
            ProgramCode = progCode;
            ProgramName = progName;
        }

        public string UniversityCode { get; }
        public string UniversityName { get; }
        public string ProgramCode { get; }
        public string ProgramName { get; }
        public int Enrolled { get; set; }
        public int GrantFull { get; set; }
        public int GrantPartial { get; set; }
    }
}
