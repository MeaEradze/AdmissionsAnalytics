using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Admissions.Tests.Integration;

public class AuditFixTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;

    public AuditFixTests(TestAppFactory factory) => _client = factory.CreateClient();

    private async Task<JsonElement> GetJson(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Programs_Search_MatchesProgramName()
    {
        var json = await GetJson("/api/programs?year=2025&search=კომპიუტერული");
        Assert.Equal(1, json.GetProperty("total").GetInt32());
        Assert.Equal("კომპიუტერული მეცნიერებები",
            json.GetProperty("data")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task Programs_Search_MatchesUniversityName()
    {

        var json = await GetJson("/api/programs?year=2025&search=ტექნიკური უნივერსიტეტი");
        Assert.Equal(1, json.GetProperty("total").GetInt32());
        Assert.Equal("საქართველოს ტექნიკური უნივერსიტეტი",
            json.GetProperty("data")[0].GetProperty("universityName").GetString());
    }

    [Fact]
    public async Task Programs_Search_NoMatch_EmptyPage()
    {
        var json = await GetJson("/api/programs?year=2025&search=არარსებული");
        Assert.Equal(0, json.GetProperty("total").GetInt32());
        Assert.Equal(0, json.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Programs_List_PageSizeClampedTo100()
    {
        var json = await GetJson("/api/programs?year=2025&pageSize=5000");
        Assert.Equal(100, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task Health_List_SummaryCoversWholeFilteredSet()
    {

        var json = await GetJson("/api/programs/health?year=2025&page=1&pageSize=1");
        Assert.Equal(1, json.GetProperty("data").GetArrayLength());
        Assert.Equal(3, json.GetProperty("total").GetInt32());

        var summary = json.GetProperty("summary");
        Assert.Equal(3, summary.GetProperty("total").GetInt32());
        int categorySum =
            summary.GetProperty("growingCount").GetInt32() +
            summary.GetProperty("stableCount").GetInt32() +
            summary.GetProperty("riskyCount").GetInt32();
        Assert.Equal(3, categorySum);
        Assert.True(summary.GetProperty("averageScore").GetDouble() > 0);
    }

    [Fact]
    public async Task Health_Single_FallbackYearReported()
    {

        var json = await GetJson("/api/programs/1/health?year=2026");
        Assert.Equal(2025, json.GetProperty("year").GetInt32());
        Assert.True(json.GetProperty("isFallback").GetBoolean());

        var exact = await GetJson("/api/programs/1/health?year=2025");
        Assert.False(exact.GetProperty("isFallback").GetBoolean());
    }

    [Fact]
    public async Task Benchmark_FallbackYearReported()
    {
        var json = await GetJson("/api/programs/1/benchmark?year=2026");
        Assert.Equal(2025, json.GetProperty("year").GetInt32());
        Assert.True(json.GetProperty("isFallback").GetBoolean());
    }

    [Fact]
    public async Task Compare_MoreThanFiveIds_Returns400()
    {
        var response = await _client.GetAsync("/api/programs/compare?ids=1,2,3,4,5,6&year=2025");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Compare_ReportsActualYearPerItem()
    {
        var json = await GetJson("/api/programs/compare?ids=1,3&year=2025");
        foreach (var item in json.EnumerateArray())
        {
            Assert.Equal(2025, item.GetProperty("year").GetInt32());
            Assert.False(item.GetProperty("isFallback").GetBoolean());
        }
    }

    [Fact]
    public async Task Import_WrongExtension_Returns400ProblemDetails()
    {
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("plain text, not a workbook"));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(file, "file", "data.txt");

        var response = await _client.PostAsync("/api/import/enrollments?year=2025", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        Assert.Contains("xlsx", json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Import_CorruptXlsxContent_Returns400ProblemDetails()
    {

        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("garbage bytes"));
        file.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(file, "file", "data.xlsx");

        var response = await _client.PostAsync("/api/import/priorities?year=2025", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        Assert.Contains("ფაილის ფორმატი არასწორია", json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Import_NonPdfToHandbook_Returns400ProblemDetails()
    {
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("xlsx pretending to be pdf"));
        file.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(file, "file", "data.xlsx");

        var response = await _client.PostAsync("/api/import/handbook?year=2025", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        Assert.Contains("PDF", json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Universities_DuplicateCode_Returns409ProblemDetails()
    {

        var response = await _client.PostAsJsonAsync("/api/universities",
            new { name = "დუბლიკატი უნივერსიტეტი", code = "001" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        Assert.Contains("უკვე არსებობს", json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Fields_DuplicateCode_Returns409ProblemDetails()
    {

        var response = await _client.PostAsJsonAsync("/api/fields",
            new { name = "დუბლიკატი დარგი", code = "HUM" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Meta_Years_DistinctDescending()
    {
        var json = await GetJson("/api/meta/years");
        var years = json.EnumerateArray().Select(y => y.GetInt32()).ToList();
        Assert.Equal([2025, 2024, 2023], years);
    }
}
