using ClosedXML.Excel;

// Output directory: arg[0] or ./mock-data relative to current dir.
string outDir = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "mock-data");
Directory.CreateDirectory(outDir);

int[] years = { 2023, 2024, 2025 };

// ---- Mock catalog: 5 programs across 3 universities ----
// Program codes are 7 digits (parser uses the first 7 as the base code).
var programs = new List<Prog>
{
    new("001", "სსიპ - ივანე ჯავახიშვილის სახელობის თბილისის სახელმწიფო უნივერსიტეტი",
        "0010101", "ქართული ფილოლოგია",
        Enrolled: new() { [2023] = 45, [2024] = 42, [2025] = 38 },   // declining
        FirstPriority: new() { [2023] = 120, [2024] = 110, [2025] = 95 }),

    new("001", "სსიპ - ივანე ჯავახიშვილის სახელობის თბილისის სახელმწიფო უნივერსიტეტი",
        "0010102", "ისტორია",
        Enrolled: new() { [2023] = 30, [2024] = 33, [2025] = 35 },   // rising
        FirstPriority: new() { [2023] = 70, [2024] = 78, [2025] = 85 }),

    new("002", "სსიპ - ილიას სახელმწიფო უნივერსიტეტი",
        "0020201", "კომპიუტერული მეცნიერება",
        Enrolled: new() { [2023] = 55, [2024] = 62, [2025] = 70 },   // strong growth
        FirstPriority: new() { [2023] = 160, [2024] = 185, [2025] = 210 }),

    new("002", "სსიპ - ილიას სახელმწიფო უნივერსიტეტი",
        "0020202", "ბიზნესის ადმინისტრირება",
        Enrolled: new() { [2023] = 48, [2024] = 50, [2025] = 52 },   // steady
        FirstPriority: new() { [2023] = 140, [2024] = 150, [2025] = 158 }),

    new("003", "შპს - თბილისის სახელმწიფო სამედიცინო უნივერსიტეტი",
        "0030301", "მედიცინა",
        Enrolled: new() { [2023] = 60, [2024] = 58, [2025] = 61 },   // stable, high demand
        FirstPriority: new() { [2023] = 200, [2024] = 205, [2025] = 220 }),
};

foreach (int year in years)
{
    WriteEnrollments(Path.Combine(outDir, $"enrollments-{year}.xlsx"), programs, year);
    WritePriorities(Path.Combine(outDir, $"priorities-{year}.xlsx"), programs, year);
}

Console.WriteLine($"Done. Files written to: {outDir}");

// ---------- Enrollments: ONE ROW PER ENROLLED STUDENT ----------
// Columns: 1=uniCode 2=uniName 3=progCode 4=progName ... 18=grant%
//   grant%: 100 = full grant, 70/50 = partial, blank = none.
static void WriteEnrollments(string path, List<Prog> programs, int year)
{
    using var wb = new XLWorkbook();
    var ws = wb.Worksheets.Add("enrollments");

    // Header row (row 1 is skipped by the parser).
    ws.Cell(1, 1).Value = "უსდ კოდი";
    ws.Cell(1, 2).Value = "უსდ დასახელება";
    ws.Cell(1, 3).Value = "პროგრამის კოდი";
    ws.Cell(1, 4).Value = "პროგრამის დასახელება";
    ws.Cell(1, 18).Value = "გრანტი %";

    int r = 2;
    foreach (var p in programs)
    {
        int enrolled = p.Enrolled[year];
        int full = (int)Math.Round(enrolled * 0.20);     // 20% full grant (100%)
        int partial = (int)Math.Round(enrolled * 0.30);  // 30% partial grant (70/50)

        for (int i = 0; i < enrolled; i++)
        {
            ws.Cell(r, 1).Value = p.UniCode;
            ws.Cell(r, 2).Value = p.UniName;
            ws.Cell(r, 3).Value = p.ProgCode;
            ws.Cell(r, 4).Value = p.ProgName;

            if (i < full)
                ws.Cell(r, 18).Value = 100;
            else if (i < full + partial)
                ws.Cell(r, 18).Value = (i % 2 == 0) ? 70 : 50;
            // else: leave grant cell blank => no grant
            r++;
        }
    }

    wb.SaveAs(path);
    Console.WriteLine($"  {Path.GetFileName(path)}: {r - 2} student rows");
}

// ---------- Priorities: ONE ROW PER PROGRAM ----------
// Columns: 1=uniCode 2=uniName 3=progCode 4=progName 5..14=priority1..10 counts 15=total
static void WritePriorities(string path, List<Prog> programs, int year)
{
    using var wb = new XLWorkbook();
    var ws = wb.Worksheets.Add("priorities");

    ws.Cell(1, 1).Value = "უსდ კოდი";
    ws.Cell(1, 2).Value = "უსდ დასახელება";
    ws.Cell(1, 3).Value = "პროგრამის კოდი";
    ws.Cell(1, 4).Value = "პროგრამის დასახელება";
    for (int pr = 1; pr <= 10; pr++)
        ws.Cell(1, 4 + pr).Value = $"პრიორიტეტი {pr}";
    ws.Cell(1, 15).Value = "სულ";

    int r = 2;
    foreach (var p in programs)
    {
        int p1 = p.FirstPriority[year];

        // Descending funnel: each lower priority gets ~55% of the previous.
        int total = 0;
        for (int i = 0; i < 10; i++)
        {
            int count = (int)Math.Round(p1 * Math.Pow(0.55, i));
            ws.Cell(r, 5 + i).Value = count;
            total += count;
        }

        ws.Cell(r, 1).Value = p.UniCode;
        ws.Cell(r, 2).Value = p.UniName;
        ws.Cell(r, 3).Value = p.ProgCode;
        ws.Cell(r, 4).Value = p.ProgName;
        ws.Cell(r, 15).Value = total;
        r++;
    }

    wb.SaveAs(path);
    Console.WriteLine($"  {Path.GetFileName(path)}: {r - 2} program rows");
}

record Prog(
    string UniCode,
    string UniName,
    string ProgCode,
    string ProgName,
    Dictionary<int, int> Enrolled,
    Dictionary<int, int> FirstPriority);
