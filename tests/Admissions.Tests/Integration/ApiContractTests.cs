using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Admissions.Tests.Integration;

public class ApiContractTests : IClassFixture<TestAppFactory>
{
    private readonly HttpClient _client;

    public ApiContractTests(TestAppFactory factory) => _client = factory.CreateClient();

    private async Task<JsonElement> GetJson(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
    }

    private static void AssertHasKeys(JsonElement obj, params string[] keys)
    {
        foreach (string key in keys)
        {
            Assert.True(obj.TryGetProperty(key, out _), $"missing key '{key}'");
        }
    }

    [Fact]
    public async Task Universities_List_CamelCaseShape()
    {
        var json = await GetJson("/api/universities");
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(2, json.GetArrayLength());
        AssertHasKeys(json[0], "id", "name", "shortName", "code");
    }

    [Fact]
    public async Task Universities_Create_ReturnsCreatedEntity()
    {
        var response = await _client.PostAsJsonAsync("/api/universities",
            new { name = "ილიას სახელმწიფო უნივერსიტეტი", shortName = "ილიასუ", code = "011" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        Assert.True(json.GetProperty("id").GetInt32() > 0);
        Assert.Equal("011", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Fields_CreateAndUpdate_RoundTrip()
    {
        var create = await _client.PostAsJsonAsync("/api/fields", new { name = "სამართალი", code = "LAW" });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await create.Content.ReadAsStringAsync());
        int id = created.GetProperty("id").GetInt32();

        var update = await _client.PutAsJsonAsync($"/api/fields/{id}", new { name = "სამართალმცოდნეობა" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = JsonSerializer.Deserialize<JsonElement>(await update.Content.ReadAsStringAsync());
        Assert.Equal("სამართალმცოდნეობა", updated.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Fields_Create_EmptyName_Returns400ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync("/api/fields", new { name = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Programs_List_PaginationEnvelopeAndItemShape()
    {
        var json = await GetJson("/api/programs?year=2025&page=1&pageSize=2");
        AssertHasKeys(json, "data", "total", "page", "pageSize");
        Assert.Equal(3, json.GetProperty("total").GetInt32());
        Assert.Equal(2, json.GetProperty("data").GetArrayLength());

        var item = json.GetProperty("data")[0];
        AssertHasKeys(item,
            "id", "name", "code", "universityId", "universityName", "fieldId", "fieldName",
            "year", "announcedPlaces", "enrolledCount", "firstPriorityCount", "annualFee",
            "compositeScore", "category");
    }

    [Fact]
    public async Task Programs_List_FiltersByUniversityAndCategory()
    {
        var byUni = await GetJson("/api/programs?year=2025&universityId=1");
        Assert.Equal(2, byUni.GetProperty("total").GetInt32());

        var byCat = await GetJson("/api/programs?year=2025&healthCategory=Growing");
        foreach (var item in byCat.GetProperty("data").EnumerateArray())
        {
            Assert.Equal("Growing", item.GetProperty("category").GetString());
        }
    }

    [Fact]
    public async Task Programs_Detail_IncludesOrderedYearStats()
    {
        var json = await GetJson("/api/programs/1");
        AssertHasKeys(json, "id", "name", "code", "degreeLevel", "university", "field", "yearStats");
        var years = json.GetProperty("yearStats").EnumerateArray().Select(y => y.GetProperty("year").GetInt32()).ToList();
        Assert.Equal([2023, 2024, 2025], years);
        AssertHasKeys(json.GetProperty("yearStats")[0],
            "year", "announcedPlaces", "enrolledCount", "firstPriorityCount",
            "totalPriorityCount", "annualFee", "grantFullCount", "grantPartialCount");
    }

    [Fact]
    public async Task Programs_Detail_Unknown_Returns404()
    {
        var response = await _client.GetAsync("/api/programs/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Programs_UpdateYearStats_Returns204AndPersists()
    {
        var response = await _client.PutAsJsonAsync("/api/programs/2/year-stats/2025", new
        {
            announcedPlaces = 85,
            enrolledCount = 60,
            firstPriorityCount = 50,
            totalPriorityCount = 160,
            annualFee = 2400,
            grantFullCount = 10,
            grantPartialCount = 8,
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var detail = await GetJson("/api/programs/2");
        var stat2025 = detail.GetProperty("yearStats").EnumerateArray()
            .Single(y => y.GetProperty("year").GetInt32() == 2025);
        Assert.Equal(85, stat2025.GetProperty("announcedPlaces").GetInt32());
        Assert.Equal(2400, stat2025.GetProperty("annualFee").GetDecimal());
    }

    [Fact]
    public async Task Health_Single_ExactEnumStringsAndShape()
    {
        var json = await GetJson("/api/programs/1/health?year=2025");
        AssertHasKeys(json,
            "programId", "programName", "universityName", "fieldName", "year",
            "demandScore", "fillRateScore", "priorityQualityScore", "priceScore",
            "compositeScore", "category", "fillRate", "firstPriorityCount",
            "enrolledCount", "announcedPlaces", "annualFee");
        Assert.Contains(json.GetProperty("category").GetString(), new[] { "Growing", "Stable", "Risky" });
        Assert.Equal(2025, json.GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task Health_List_UsesCategoryParamName()
    {
        var json = await GetJson("/api/programs/health?year=2025&category=Stable&page=1&pageSize=20");
        AssertHasKeys(json, "data", "total", "page", "pageSize");
        foreach (var item in json.GetProperty("data").EnumerateArray())
        {
            Assert.Equal("Stable", item.GetProperty("category").GetString());
        }
    }

    [Fact]
    public async Task Competition_Field_SharesSumToHundred()
    {
        var json = await GetJson("/api/fields/1/competition?year=2025");
        AssertHasKeys(json, "fieldId", "fieldName", "year", "totalDemand", "universities");
        Assert.Equal(178 + 43, json.GetProperty("totalDemand").GetInt32());

        double shareSum = json.GetProperty("universities").EnumerateArray()
            .Sum(u => u.GetProperty("marketSharePct").GetDouble());
        Assert.Equal(100, shareSum, 1);
    }

    [Fact]
    public async Task Competition_Program_YearlySharesShape()
    {
        var json = await GetJson("/api/programs/1/competition?fromYear=2023&toYear=2025");
        AssertHasKeys(json, "programId", "programName", "fieldId", "fieldName", "years");
        var year2025 = json.GetProperty("years").EnumerateArray()
            .Single(y => y.GetProperty("year").GetInt32() == 2025);

        Assert.Equal(80.54, year2025.GetProperty("marketSharePct").GetDouble(), 2);
    }

    [Fact]
    public async Task Forecast_MockProgram1_HandComputed124()
    {
        var json = await GetJson("/api/programs/1/forecast");
        Assert.Equal("წრფივი ტრენდის პროექცია", json.GetProperty("methodLabel").GetString());
        Assert.Equal(124, json.GetProperty("pointEstimate").GetInt32());
        Assert.Equal(112, json.GetProperty("lowerBound").GetInt32());
        Assert.Equal(136, json.GetProperty("upperBound").GetInt32());
        Assert.Equal(2026, json.GetProperty("projectedYear").GetInt32());
    }

    [Fact]
    public async Task Trend_JsonKeyIsExactlyYoYDeltas()
    {
        var response = await _client.GetAsync("/api/programs/1/trend");
        string raw = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"yoYDeltas\"", raw);
        Assert.Contains("\"demandTrendLabel\":\"Growing\"", raw);

        var json = JsonSerializer.Deserialize<JsonElement>(raw);
        Assert.Equal(Math.Pow(178.0 / 145, 0.5) - 1, json.GetProperty("demandCagr").GetDouble(), 6);
    }

    [Fact]
    public async Task Trend_Field_AggregatesMembers()
    {
        var json = await GetJson("/api/fields/1/trend");
        Assert.Equal(1, json.GetProperty("entityId").GetInt32());
        Assert.Equal(2, json.GetProperty("yoYDeltas").GetArrayLength());
    }

    [Fact]
    public async Task Conversion_MockProgram1_HandComputedAverage()
    {
        var json = await GetJson("/api/programs/1/conversion");
        Assert.Equal(0.6615, json.GetProperty("historicalAvgConversion").GetDouble(), 4);
        var first = json.GetProperty("yoYDeltas")[0];
        Assert.Equal(JsonValueKind.Null, first.GetProperty("delta").ValueKind);
    }

    [Fact]
    public async Task FeeSensitivity_SlopeSignLowercase()
    {
        var json = await GetJson("/api/programs/1/fee-sensitivity");
        Assert.True(json.GetProperty("indicative").GetBoolean());
        Assert.Contains(json.GetProperty("slopeSign").GetString(), new[] { "positive", "negative", "flat" });
    }

    [Fact]
    public async Task Benchmark_RanksWithinField()
    {
        var json = await GetJson("/api/programs/1/benchmark?year=2025");
        AssertHasKeys(json,
            "programId", "programName", "year", "demandRatioVsMedian", "fillRateRankInField",
            "feeRankInField", "healthDeltaVsFieldAvg", "demandPercentile", "fillRatePercentile",
            "feePercentile", "healthPercentile");

        Assert.Equal(1.0, json.GetProperty("demandRatioVsMedian").GetDouble(), 3);
        Assert.Equal(100, json.GetProperty("demandPercentile").GetDouble(), 1);
    }

    [Fact]
    public async Task PriorityDistribution_GranularFromBreakdowns()
    {
        var json = await GetJson("/api/programs/1/priority-distribution?year=2025");
        Assert.True(json.GetProperty("isGranular").GetBoolean());
        Assert.Equal(10, json.GetProperty("distribution").GetArrayLength());
        Assert.Equal(178, json.GetProperty("distribution")[0].GetProperty("count").GetInt32());
        Assert.Equal(178, json.GetProperty("firstPriorityCount").GetInt32());
    }

    [Fact]
    public async Task PriorityDistribution_NonGranular_OmitsDistribution()
    {

        var json = await GetJson("/api/programs/2/priority-distribution?year=2025");
        Assert.False(json.GetProperty("isGranular").GetBoolean());
        Assert.False(json.TryGetProperty("distribution", out _));
    }

    [Fact]
    public async Task MarketGaps_SeverityStringsAndSorting()
    {
        var json = await GetJson("/api/market/gaps?year=2025");
        Assert.True(json.GetArrayLength() >= 2);
        var severities = json.EnumerateArray()
            .Select(g => g.GetProperty("gapSeverity").GetString()!)
            .ToList();
        Assert.All(severities, s => Assert.Contains(s, new[] { "High", "Medium", "Low" }));

        var order = new Dictionary<string, int> { ["High"] = 0, ["Medium"] = 1, ["Low"] = 2 };
        var ranks = severities.Select(s => order[s]).ToList();
        Assert.Equal(ranks.OrderBy(r => r), ranks);
    }

    [Fact]
    public async Task MarketOverview_TopRiskyFieldNullable()
    {
        var json = await GetJson("/api/market/overview?year=2025");
        AssertHasKeys(json,
            "year", "totalPrograms", "totalUniversities", "totalFields", "totalSupply",
            "totalEnrolled", "totalDemand", "avgFillRate", "avgHealthScore", "topFields",
            "topRiskyFieldByGap");
        Assert.Equal(3, json.GetProperty("totalPrograms").GetInt32());
        Assert.NotEqual(JsonValueKind.Undefined, json.GetProperty("topRiskyFieldByGap").ValueKind);
    }

    [Fact]
    public async Task Portfolio_MarketShareWithinField()
    {
        var json = await GetJson("/api/universities/1/portfolio?year=2025");
        Assert.Equal(2, json.GetArrayLength());
        var philology = json.EnumerateArray().Single(p => p.GetProperty("programId").GetInt32() == 1);
        Assert.Equal(80.54, philology.GetProperty("marketShareInField").GetDouble(), 2);
    }

    [Fact]
    public async Task Compare_ParsesCsvIdsAndSkipsUnknown()
    {
        var json = await GetJson("/api/programs/compare?ids=1,3,999&year=2025");
        Assert.Equal(2, json.GetArrayLength());
        AssertHasKeys(json[0],
            "programId", "programName", "demandScore", "fillRateScore", "priorityQualityScore",
            "priceScore", "compositeScore", "category", "historicalAvgConversion",
            "forecastPointEstimate");
    }

    [Fact]
    public async Task Dashboard_SummaryShape()
    {
        var json = await GetJson("/api/dashboard/summary?year=2025");
        AssertHasKeys(json,
            "year", "totalPrograms", "totalUniversities", "totalFields", "avgFillRate",
            "totalDemand", "topGrowingPrograms", "topRiskyPrograms", "topFields");
        Assert.Equal(178 + 43 + 345, json.GetProperty("totalDemand").GetInt32());
    }

    [Fact]
    public async Task UniversityTrend_AggregatesPrograms()
    {
        var json = await GetJson("/api/universities/1/trend");
        Assert.Equal(2, json.GetProperty("yoYDeltas").GetArrayLength());
        AssertHasKeys(json, "entityId", "entityName", "demandCagr", "demandTrendLabel", "yoYDeltas");
    }
}
