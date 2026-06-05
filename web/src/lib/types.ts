export interface PagedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export type HealthCategory = 'Growing' | 'Stable' | 'Risky'
export type TrendDirection = 'Growing' | 'Stable' | 'Declining'
export type GapSeverity = 'High' | 'Medium' | 'Low'

export interface University {
  id: number
  name: string
  shortName?: string
  code?: string
}

export interface Field {
  id: number
  name: string
  code?: string
}

export interface ProgramYear {
  year: number
  announcedPlaces: number
  enrolledCount: number
  firstPriorityCount: number
  totalPriorityCount?: number
  annualFee: number
  grantFullCount?: number
  grantPartialCount?: number
}

export interface ProgramListItem {
  id: number
  name: string
  code?: string
  universityId: number
  universityName: string
  fieldId: number
  fieldName: string
  year: number
  announcedPlaces: number
  enrolledCount: number
  firstPriorityCount: number
  annualFee: number
  compositeScore?: number | null
  category?: HealthCategory | null
}

export interface ProgramDetail {
  id: number
  name: string
  code?: string
  degreeLevel?: string
  university: University
  field: Field
  yearStats: ProgramYear[]
}

export interface UniversityShare {
  universityId: number
  universityName: string
  firstPriorityCount: number
  marketSharePct: number
}

export interface FieldCompetition {
  fieldId: number
  fieldName: string
  year: number
  totalDemand: number
  universities: UniversityShare[]
}

export interface YearlyShare {
  year: number
  firstPriorityCount: number
  fieldTotalDemand: number
  marketSharePct: number
}

export interface ProgramCompetitionTrend {
  programId: number
  programName: string
  fieldId: number
  fieldName: string
  years: YearlyShare[]
}

export interface ProgramHealth {
  programId: number
  programName: string
  universityName: string
  fieldName: string

  year: number

  isFallback?: boolean
  demandScore: number
  fillRateScore: number
  priorityQualityScore: number
  priceScore: number
  compositeScore: number
  category: HealthCategory
  fillRate: number
  firstPriorityCount: number
  enrolledCount: number
  announcedPlaces: number
  annualFee: number
}

export interface HealthListSummary {
  total: number
  growingCount: number
  stableCount: number
  riskyCount: number
  averageScore: number
}

export interface HealthListResponse extends PagedResponse<ProgramHealth> {
  summary?: HealthListSummary
}

export interface HistoricalPoint {
  year: number
  enrolledCount: number
}

export interface ProgramForecast {
  programId: number
  programName: string
  methodLabel: string
  pointEstimate: number
  lowerBound: number
  upperBound: number
  projectedYear: number
  historicalData: HistoricalPoint[]
}

export interface TrendPoint {
  year: number
  demandDelta?: number
  fillDelta?: number
  feeDelta?: number
}

export interface TrendYearPoint {
  year: number
  announcedPlaces: number
  enrolledCount: number
  firstPriorityCount: number
  annualFee: number
}

export interface TrendResult {
  entityId: number
  entityName: string
  demandCagr: number | null
  demandTrendLabel: TrendDirection
  yoYDeltas: TrendPoint[]

  yearSeries: TrendYearPoint[]
}

export type ProgramTrend = TrendResult
export type FieldTrend = TrendResult
export type UniversityTrend = TrendResult

export interface ProgramBenchmark {
  programId: number
  programName: string

  year: number

  isFallback?: boolean
  demandRatioVsMedian: number
  fillRateRankInField: number
  feeRankInField: number
  healthDeltaVsFieldAvg: number
  demandPercentile: number
  fillRatePercentile: number
  feePercentile: number
  healthPercentile: number
}

export interface FieldGap {
  fieldId: number
  fieldName: string
  aggregateDemand: number
  totalSupply: number
  demandSupplyRatio: number
  avgFillRate: number
  gapSeverity: GapSeverity
  programCount: number
}

export interface TopRiskyFieldRef {
  fieldId: number
  fieldName: string
  gapSeverity: string
}

export interface MarketOverview {
  year: number
  totalPrograms: number
  totalUniversities: number
  totalFields: number
  totalSupply: number
  totalEnrolled: number
  totalDemand: number
  avgFillRate: number
  avgHealthScore: number
  topFields: FieldGap[]
  topRiskyFieldByGap: TopRiskyFieldRef | null
}

export interface PriorityPoint {
  priority: number
  count: number
}

export interface PriorityDistribution {
  programId: number
  programName: string
  year: number
  firstPriorityCount: number
  totalPriorityCount?: number
  weightedDemandScore: number
  interestBreadth: number
  distribution?: PriorityPoint[]
  isGranular: boolean
}

export interface ConversionYear {
  year: number
  conversionRate: number
  delta?: number | null
}

export interface ProgramConversion {
  programId: number
  programName: string
  historicalAvgConversion: number
  yoYDeltas: ConversionYear[]
}

export interface FeeSensitivity {
  programId: number
  programName: string
  indicative: boolean
  insufficientData: boolean
  pearsonCorrelation: number | null
  slopeSign: 'positive' | 'negative' | 'flat'
}

export interface PortfolioItem {
  programId: number
  programName: string
  fieldId: number
  fieldName: string
  compositeScore: number
  category: HealthCategory
  marketShareInField: number
}

export interface ProgramComparisonItem {
  programId: number
  programName: string

  year?: number

  isFallback?: boolean
  demandScore: number
  fillRateScore: number
  priorityQualityScore: number
  priceScore: number
  compositeScore: number
  category: HealthCategory
  historicalAvgConversion: number
  forecastPointEstimate: number
}

export interface TopProgram {
  programId: number
  programName: string
  universityName: string
  healthScore: number
}

export interface TopField {
  fieldId: number
  fieldName: string
  demand: number
  fillRate: number
}

export interface DashboardSummary {
  year: number
  totalPrograms: number
  totalUniversities: number
  totalFields: number
  avgFillRate: number
  totalDemand: number
  topGrowingPrograms: TopProgram[]
  topRiskyPrograms: TopProgram[]
  topFields: TopField[]
}

export interface ImportResult {
  rowsRead: number
  rowsImported: number
  errors: string[]
  year: number
}

export interface CreateFieldRequest {
  name: string
  code?: string
}

export interface UpdateFieldRequest {
  name: string
  code?: string
}

export interface CreateUniversityRequest {
  name: string
  shortName?: string
  code?: string
}

export interface UpdateProgramYearRequest {
  announcedPlaces: number
  enrolledCount: number
  firstPriorityCount: number
  totalPriorityCount?: number
  annualFee: number
  grantFullCount?: number
  grantPartialCount?: number
}

export interface ProgramListParams {
  universityId?: number
  fieldId?: number
  year?: number
  minFee?: number
  maxFee?: number

  search?: string
  healthCategory?: HealthCategory
  page?: number
  pageSize?: number
}

export interface HealthListParams {
  category?: HealthCategory
  fieldId?: number
  universityId?: number
  year?: number
  minFee?: number
  maxFee?: number
  page?: number
  pageSize?: number
}
