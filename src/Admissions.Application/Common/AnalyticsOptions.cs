namespace Admissions.Application.Common;

public class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    public double DemandWeight { get; set; } = 0.35;
    public double FillRateWeight { get; set; } = 0.25;
    public double PriorityQualityWeight { get; set; } = 0.25;
    public double PriceWeight { get; set; } = 0.15;

    public double GrowingThreshold { get; set; } = 70;
    public double RiskyThreshold { get; set; } = 45;
    public double LowFillRateThreshold { get; set; } = 0.5;

    public double TrendGrowingCutoff { get; set; } = 0.03;
    public double TrendDecliningCutoff { get; set; } = -0.03;

    public double GapHighDemandSupplyRatio { get; set; } = 2.0;
    public double GapHighMinAvgFillRate { get; set; } = 0.9;
    public double GapMediumDemandSupplyRatio { get; set; } = 1.3;

    public double SlopeSignPositiveCutoff { get; set; } = 0.05;
    public double SlopeSignNegativeCutoff { get; set; } = -0.05;

    public double ForecastLowerFactor { get; set; } = 0.9;
    public double ForecastUpperFactor { get; set; } = 1.1;

    public double NeutralScore { get; set; } = 50;

    public string NormalizationMode { get; set; } = "Percentile";

    public bool TreatMissingDataAsNeutral { get; set; } = true;
}
