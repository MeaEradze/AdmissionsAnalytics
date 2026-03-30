using System.Text.Json.Serialization;
using Admissions.Domain.Enums;

namespace Admissions.Application.Dtos;

public class UniversityShareDto
{
    public int UniversityId { get; set; }
    public string UniversityName { get; set; } = "";
    public int FirstPriorityCount { get; set; }
    public double MarketSharePct { get; set; }
}

public class FieldCompetitionDto
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = "";
    public int Year { get; set; }
    public int TotalDemand { get; set; }
    public List<UniversityShareDto> Universities { get; set; } = [];
}

public class YearlyShareDto
{
    public int Year { get; set; }
    public int FirstPriorityCount { get; set; }
    public int FieldTotalDemand { get; set; }
    public double MarketSharePct { get; set; }
}

public class ProgramCompetitionTrendDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public int FieldId { get; set; }
    public string FieldName { get; set; } = "";
    public List<YearlyShareDto> Years { get; set; } = [];
}

public class ProgramHealthDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public string UniversityName { get; set; } = "";
    public string FieldName { get; set; } = "";

    public int Year { get; set; }

    public bool IsFallback { get; set; }

    public double DemandScore { get; set; }
    public double FillRateScore { get; set; }
    public double PriorityQualityScore { get; set; }
    public double PriceScore { get; set; }
    public double CompositeScore { get; set; }
    public HealthCategory Category { get; set; }
    public double FillRate { get; set; }
    public int FirstPriorityCount { get; set; }
    public int EnrolledCount { get; set; }
    public int AnnouncedPlaces { get; set; }
    public decimal AnnualFee { get; set; }
}

public class HealthListSummaryDto
{
    public int Total { get; set; }
    public int GrowingCount { get; set; }
    public int StableCount { get; set; }
    public int RiskyCount { get; set; }
    public double AverageScore { get; set; }
}

public class HealthListResponse : PagedResponse<ProgramHealthDto>
{
    public HealthListSummaryDto Summary { get; set; } = new();
}

public class HistoricalPointDto
{
    public int Year { get; set; }
    public int EnrolledCount { get; set; }
}

public class ProgramForecastDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public string MethodLabel { get; set; } = "წრფივი ტრენდის პროექცია";
    public int PointEstimate { get; set; }
    public int LowerBound { get; set; }
    public int UpperBound { get; set; }
    public int ProjectedYear { get; set; }
    public List<HistoricalPointDto> HistoricalData { get; set; } = [];
}

public class TrendPointDto
{
    public int Year { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? DemandDelta { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FillDelta { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FeeDelta { get; set; }
}

public class TrendYearPointDto
{
    public int Year { get; set; }
    public int AnnouncedPlaces { get; set; }
    public int EnrolledCount { get; set; }
    public int FirstPriorityCount { get; set; }
    public double AnnualFee { get; set; }
}

public class TrendResultDto
{
    public int EntityId { get; set; }
    public string EntityName { get; set; } = "";

    public double? DemandCagr { get; set; }
    public TrendDirection DemandTrendLabel { get; set; }
    public List<TrendPointDto> YoYDeltas { get; set; } = [];

    public List<TrendYearPointDto> YearSeries { get; set; } = [];
}

public class ProgramBenchmarkDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";

    public int Year { get; set; }

    public bool IsFallback { get; set; }
    public double DemandRatioVsMedian { get; set; }
    public int FillRateRankInField { get; set; }
    public int FeeRankInField { get; set; }
    public double HealthDeltaVsFieldAvg { get; set; }
    public double DemandPercentile { get; set; }
    public double FillRatePercentile { get; set; }
    public double FeePercentile { get; set; }
    public double HealthPercentile { get; set; }
}

public class FieldGapDto
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = "";
    public int AggregateDemand { get; set; }
    public int TotalSupply { get; set; }
    public double DemandSupplyRatio { get; set; }
    public double AvgFillRate { get; set; }
    public GapSeverity GapSeverity { get; set; }
    public int ProgramCount { get; set; }
}

public class TopRiskyFieldRefDto
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = "";
    public string GapSeverity { get; set; } = "";
}

public class MarketOverviewDto
{
    public int Year { get; set; }
    public int TotalPrograms { get; set; }
    public int TotalUniversities { get; set; }
    public int TotalFields { get; set; }
    public int TotalSupply { get; set; }
    public int TotalEnrolled { get; set; }
    public int TotalDemand { get; set; }
    public double AvgFillRate { get; set; }
    public double AvgHealthScore { get; set; }
    public List<FieldGapDto> TopFields { get; set; } = [];

    public TopRiskyFieldRefDto? TopRiskyFieldByGap { get; set; }
}

public class PriorityPointDto
{
    public int Priority { get; set; }
    public int Count { get; set; }
}

public class PriorityDistributionDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public int Year { get; set; }
    public int FirstPriorityCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPriorityCount { get; set; }

    public double WeightedDemandScore { get; set; }
    public int InterestBreadth { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PriorityPointDto>? Distribution { get; set; }

    public bool IsGranular { get; set; }
}

public class ConversionYearDto
{
    public int Year { get; set; }
    public double ConversionRate { get; set; }

    public double? Delta { get; set; }
}

public class ProgramConversionDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public double HistoricalAvgConversion { get; set; }
    public List<ConversionYearDto> YoYDeltas { get; set; } = [];
}

public class FeeSensitivityDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public bool Indicative { get; set; } = true;
    public bool InsufficientData { get; set; }

    public double? PearsonCorrelation { get; set; }

    public string SlopeSign { get; set; } = "flat";
}

public class PortfolioItemDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public int FieldId { get; set; }
    public string FieldName { get; set; } = "";
    public double CompositeScore { get; set; }
    public HealthCategory Category { get; set; }
    public double MarketShareInField { get; set; }
}

public class ProgramComparisonItemDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";

    public int Year { get; set; }

    public bool IsFallback { get; set; }

    public double DemandScore { get; set; }
    public double FillRateScore { get; set; }
    public double PriorityQualityScore { get; set; }
    public double PriceScore { get; set; }
    public double CompositeScore { get; set; }
    public HealthCategory Category { get; set; }
    public double HistoricalAvgConversion { get; set; }
    public int ForecastPointEstimate { get; set; }
}

public class TopProgramDto
{
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = "";
    public string UniversityName { get; set; } = "";
    public double HealthScore { get; set; }
}

public class TopFieldDto
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = "";
    public int Demand { get; set; }
    public double FillRate { get; set; }
}

public class DashboardSummaryDto
{
    public int Year { get; set; }
    public int TotalPrograms { get; set; }
    public int TotalUniversities { get; set; }
    public int TotalFields { get; set; }
    public double AvgFillRate { get; set; }
    public int TotalDemand { get; set; }
    public List<TopProgramDto> TopGrowingPrograms { get; set; } = [];
    public List<TopProgramDto> TopRiskyPrograms { get; set; } = [];
    public List<TopFieldDto> TopFields { get; set; } = [];
}

public class ImportResultDto
{
    public int RowsRead { get; set; }
    public int RowsImported { get; set; }
    public List<string> Errors { get; set; } = [];
    public int Year { get; set; }
}
