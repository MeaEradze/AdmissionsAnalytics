using System.Globalization;
using System.Text.RegularExpressions;
using Admissions.Application.Common;
using Admissions.Application.Imports;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Admissions.Infrastructure.Parsers;

public partial class HandbookPdfParser : IHandbookFileParser
{
    [GeneratedRegex(@"^(\d{7})\s+(.+)$")]
    private static partial Regex ProgramHeaderRegex();

    [GeneratedRegex(@"(?:არ\s+)?ფინანსდება\s+.*?(\d+(?:[.,]\d+)?)\s+(\d+)\s*$")]
    private static partial Regex FundingLineRegex();

    [GeneratedRegex(@"^(\d{3})\s+(.+?)(?:\s+\d{2}[./]\d{2}[./]\d{4}.*)?$")]
    private static partial Regex UniversityFooterRegex();

    public HandbookParseResult Parse(Stream file)
    {
        var errors = new List<string>();
        var programs = new List<HandbookProgram>();

        var seenCodes = new HashSet<string>();
        int rowsRead = 0;

        using var document = OpenDocument(file);
        foreach (var page in document.GetPages())
        {
            List<string> lines;
            try
            {
                lines = ExtractLines(page);
            }
            catch (Exception ex)
            {
                errors.Add($"გვერდი {page.Number}: წაკითხვის შეცდომა — {ex.Message}");
                continue;
            }

            if (lines.Count == 0)
            {
                continue;
            }

            var header = ProgramHeaderRegex().Match(lines[0]);
            if (!header.Success)
            {
                continue;
            }

            string code = header.Groups[1].Value;
            string name = header.Groups[2].Value.Trim();

            if (!seenCodes.Add(code))
            {
                continue;
            }

            rowsRead++;

            string? fundingLine = lines.FirstOrDefault(l => FundingLineRegex().IsMatch(l));
            if (fundingLine is null)
            {
                errors.Add($"გვერდი {page.Number} ({code}): საფასურის/ადგილების სტრიქონი ვერ მოიძებნა.");
                continue;
            }

            var match = FundingLineRegex().Match(fundingLine);
            string feeText = match.Groups[1].Value.Replace(',', '.');
            if (!decimal.TryParse(feeText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal fee) ||
                !int.TryParse(match.Groups[2].Value, out int places))
            {
                errors.Add($"გვერდი {page.Number} ({code}): საფასური/ადგილები ვერ წავიკითხეთ.");
                continue;
            }

            string uniCode = code[..3];
            string? uniName = FindUniversityName(lines, uniCode);

            programs.Add(new HandbookProgram(uniCode, uniName, code, name, fee, places));
        }

        return new HandbookParseResult(rowsRead, programs, errors);
    }

    private static PdfDocument OpenDocument(Stream file)
    {
        try
        {
            return PdfDocument.Open(file);
        }
        catch (Exception ex)
        {
            throw new InvalidFileFormatException(
                $"ფაილის ფორმატი არასწორია — საჭიროა PDF ფაილი. ({ex.Message})");
        }
    }

    private static string? FindUniversityName(List<string> lines, string uniCode)
    {
        for (int i = lines.Count - 1; i >= Math.Max(0, lines.Count - 6); i--)
        {
            var m = UniversityFooterRegex().Match(lines[i]);
            if (m.Success && m.Groups[1].Value == uniCode)
            {
                string name = m.Groups[2].Value.Trim();
                if (name.Length > 3)
                {
                    return name;
                }
            }
        }

        return null;
    }

    private static List<string> ExtractLines(Page page)
    {
        const double tolerance = 2.5;

        var words = page.GetWords()
            .OrderByDescending(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        var lines = new List<string>();
        var current = new List<Word>();
        double currentBottom = double.NaN;

        foreach (var word in words)
        {
            if (current.Count > 0 && Math.Abs(word.BoundingBox.Bottom - currentBottom) > tolerance)
            {
                lines.Add(string.Join(' ', current.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)).Trim());
                current.Clear();
            }

            if (current.Count == 0)
            {
                currentBottom = word.BoundingBox.Bottom;
            }

            current.Add(word);
        }

        if (current.Count > 0)
        {
            lines.Add(string.Join(' ', current.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)).Trim());
        }

        return lines.Where(l => l.Length > 0).ToList();
    }
}
