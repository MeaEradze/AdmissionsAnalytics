import type {
  CreateFieldRequest,
  CreateUniversityRequest,
  DashboardSummary,
  FeeSensitivity,
  Field,
  FieldCompetition,
  FieldGap,
  FieldTrend,
  HealthCategory,
  HealthListParams,
  HealthListResponse,
  ImportResult,
  MarketOverview,
  PagedResponse,
  PortfolioItem,
  PriorityDistribution,
  ProgramBenchmark,
  ProgramCompetitionTrend,
  ProgramConversion,
  ProgramDetail,
  ProgramForecast,
  ProgramHealth,
  ProgramComparisonItem,
  ProgramListItem,
  ProgramListParams,
  ProgramTrend,
  University,
  UniversityTrend,
  UpdateFieldRequest,
  UpdateProgramYearRequest,
} from './types'

const pause = (ms = 180) => new Promise<void>((r) => setTimeout(r, ms))

const UNIVERSITIES: University[] = [
  { id: 1, name: 'თბილისის სახელმწიფო უნივერსიტეტი', shortName: 'თსუ', code: '001' },
  { id: 2, name: 'საქართველოს ტექნიკური უნივერსიტეტი', shortName: 'სტუ', code: '003' },
  { id: 3, name: 'თბილისის სახელმწიფო სამედიცინო უნივერსიტეტი', shortName: 'თსსუ', code: '004' },
  { id: 4, name: 'თავისუფალი უნივერსიტეტი', shortName: 'თავსუ', code: '010' },
  { id: 5, name: 'ილიას სახელმწიფო უნივერსიტეტი', shortName: 'ილიასუ', code: '011' },
]

const FIELDS: Field[] = [
  { id: 1, name: 'ჰუმანიტარული მეცნიერებები', code: 'HUM' },
  { id: 2, name: 'ტექნიკური მეცნიერებები', code: 'TECH' },
  { id: 3, name: 'სამედიცინო მეცნიერებები', code: 'MED' },
  { id: 4, name: 'სამართლებრივი მეცნიერებები', code: 'LAW' },
  { id: 5, name: 'ეკონომიკა და ბიზნესი', code: 'ECON' },
  { id: 6, name: 'სოციალური მეცნიერებები', code: 'SOC' },
]

type YearSnap = {
  year: number
  announced: number
  enrolled: number
  firstPriority: number
  totalPriority: number
  fee: number
  grantFull?: number
  grantPartial?: number
}

type MockProg = {
  id: number
  name: string
  code: string
  universityId: number
  universityName: string
  fieldId: number
  fieldName: string
  degreeLevel?: string
  snaps: YearSnap[]

  demandScore: number
  fillRateScore: number
  priorityQualityScore: number
  priceScore: number
  compositeScore: number
  category: HealthCategory
}

const RAW: MockProg[] = [
  {
    id: 1, name: 'ქართული ფილოლოგია', code: '0010101',
    universityId: 1, universityName: 'თბილისის სახელმწიფო უნივერსიტეტი',
    fieldId: 1, fieldName: 'ჰუმანიტარული მეცნიერებები',
    snaps: [
      { year: 2023, announced: 120, enrolled: 98,  firstPriority: 145, totalPriority: 380, fee: 2250, grantFull: 22, grantPartial: 18 },
      { year: 2024, announced: 120, enrolled: 108, firstPriority: 163, totalPriority: 420, fee: 2250, grantFull: 25, grantPartial: 20 },
      { year: 2025, announced: 130, enrolled: 115, firstPriority: 178, totalPriority: 460, fee: 2500, grantFull: 28, grantPartial: 22 },
    ],
    demandScore: 78, fillRateScore: 88, priorityQualityScore: 72, priceScore: 68,
    compositeScore: 77.5, category: 'Growing',
  },
  {
    id: 2, name: 'ისტორია', code: '0010102',
    universityId: 1, universityName: 'თბილისის სახელმწიფო უნივერსიტეტი',
    fieldId: 1, fieldName: 'ჰუმანიტარული მეცნიერებები',
    snaps: [
      { year: 2023, announced: 80, enrolled: 55, firstPriority: 48, totalPriority: 165, fee: 2250, grantFull: 10, grantPartial: 8 },
      { year: 2024, announced: 80, enrolled: 52, firstPriority: 45, totalPriority: 152, fee: 2250, grantFull: 9,  grantPartial: 7  },
      { year: 2025, announced: 80, enrolled: 52, firstPriority: 43, totalPriority: 148, fee: 2250, grantFull: 9,  grantPartial: 7  },
    ],
    demandScore: 52, fillRateScore: 65, priorityQualityScore: 58, priceScore: 72,
    compositeScore: 59.8, category: 'Stable',
  },
  {
    id: 3, name: 'სამართალი', code: '0010139',
    universityId: 1, universityName: 'თბილისის სახელმწიფო უნივერსიტეტი',
    fieldId: 4, fieldName: 'სამართლებრივი მეცნიერებები',
    snaps: [
      { year: 2023, announced: 150, enrolled: 130, firstPriority: 195, totalPriority: 480, fee: 2250, grantFull: 35, grantPartial: 28 },
      { year: 2024, announced: 160, enrolled: 148, firstPriority: 218, totalPriority: 540, fee: 2500, grantFull: 40, grantPartial: 32 },
      { year: 2025, announced: 160, enrolled: 146, firstPriority: 225, totalPriority: 580, fee: 2500, grantFull: 38, grantPartial: 30 },
    ],
    demandScore: 85, fillRateScore: 91, priorityQualityScore: 78, priceScore: 62,
    compositeScore: 81.0, category: 'Growing',
  },
  {
    id: 4, name: 'მედიცინა', code: '0010146',
    universityId: 1, universityName: 'თბილისის სახელმწიფო უნივერსიტეტი',
    fieldId: 3, fieldName: 'სამედიცინო მეცნიერებები',
    snaps: [
      { year: 2023, announced: 200, enrolled: 190, firstPriority: 420, totalPriority: 890,  fee: 7500, grantFull: 55, grantPartial: 45 },
      { year: 2024, announced: 210, enrolled: 198, firstPriority: 452, totalPriority: 950,  fee: 8000, grantFull: 60, grantPartial: 48 },
      { year: 2025, announced: 220, enrolled: 202, firstPriority: 480, totalPriority: 1020, fee: 8000, grantFull: 62, grantPartial: 50 },
    ],
    demandScore: 95, fillRateScore: 92, priorityQualityScore: 85, priceScore: 35,
    compositeScore: 82.8, category: 'Growing',
  },
  {
    id: 5, name: 'კომპიუტერული მეცნიერებები', code: '0030101',
    universityId: 2, universityName: 'საქართველოს ტექნიკური უნივერსიტეტი',
    fieldId: 2, fieldName: 'ტექნიკური მეცნიერებები',
    snaps: [
      { year: 2023, announced: 100, enrolled: 95,  firstPriority: 280, totalPriority: 650, fee: 2500, grantFull: 28, grantPartial: 22 },
      { year: 2024, announced: 110, enrolled: 105, firstPriority: 310, totalPriority: 720, fee: 2700, grantFull: 32, grantPartial: 25 },
      { year: 2025, announced: 120, enrolled: 115, firstPriority: 345, totalPriority: 810, fee: 2900, grantFull: 36, grantPartial: 28 },
    ],
    demandScore: 92, fillRateScore: 96, priorityQualityScore: 82, priceScore: 58,
    compositeScore: 85.4, category: 'Growing',
  },
  {
    id: 6, name: 'სამოქალაქო ინჟინერია', code: '0030201',
    universityId: 2, universityName: 'საქართველოს ტექნიკური უნივერსიტეტი',
    fieldId: 2, fieldName: 'ტექნიკური მეცნიერებები',
    snaps: [
      { year: 2023, announced: 90, enrolled: 68, firstPriority: 62, totalPriority: 210, fee: 2500, grantFull: 14, grantPartial: 12 },
      { year: 2024, announced: 90, enrolled: 65, firstPriority: 60, totalPriority: 195, fee: 2600, grantFull: 13, grantPartial: 11 },
      { year: 2025, announced: 90, enrolled: 65, firstPriority: 58, totalPriority: 192, fee: 2700, grantFull: 13, grantPartial: 10 },
    ],
    demandScore: 60, fillRateScore: 72, priorityQualityScore: 55, priceScore: 65,
    compositeScore: 62.5, category: 'Stable',
  },
  {
    id: 7, name: 'ბიზნეს ადმინისტრირება', code: '0100101',
    universityId: 4, universityName: 'თავისუფალი უნივერსიტეტი',
    fieldId: 5, fieldName: 'ეკონომიკა და ბიზნესი',
    snaps: [
      { year: 2023, announced: 120, enrolled: 108, firstPriority: 168, totalPriority: 420, fee: 4000, grantFull: 20, grantPartial: 18 },
      { year: 2024, announced: 130, enrolled: 118, firstPriority: 185, totalPriority: 460, fee: 4200, grantFull: 22, grantPartial: 20 },
      { year: 2025, announced: 130, enrolled: 110, firstPriority: 195, totalPriority: 495, fee: 4500, grantFull: 24, grantPartial: 20 },
    ],
    demandScore: 88, fillRateScore: 85, priorityQualityScore: 75, priceScore: 50,
    compositeScore: 78.3, category: 'Growing',
  },
  {
    id: 8, name: 'ფინანსები', code: '0100201',
    universityId: 4, universityName: 'თავისუფალი უნივერსიტეტი',
    fieldId: 5, fieldName: 'ეკონომიკა და ბიზნესი',
    snaps: [
      { year: 2023, announced: 80, enrolled: 58, firstPriority: 62, totalPriority: 198, fee: 4000, grantFull: 10, grantPartial: 9 },
      { year: 2024, announced: 80, enrolled: 58, firstPriority: 60, totalPriority: 185, fee: 4200, grantFull: 10, grantPartial: 8 },
      { year: 2025, announced: 80, enrolled: 56, firstPriority: 58, totalPriority: 180, fee: 4500, grantFull: 10, grantPartial: 8 },
    ],
    demandScore: 65, fillRateScore: 70, priorityQualityScore: 60, priceScore: 55,
    compositeScore: 63.5, category: 'Stable',
  },
  {
    id: 9, name: 'ფსიქოლოგია', code: '0110101',
    universityId: 5, universityName: 'ილიას სახელმწიფო უნივერსიტეტი',
    fieldId: 6, fieldName: 'სოციალური მეცნიერებები',
    snaps: [
      { year: 2023, announced: 60, enrolled: 45, firstPriority: 38, totalPriority: 125, fee: 3500, grantFull: 7, grantPartial: 6 },
      { year: 2024, announced: 60, enrolled: 38, firstPriority: 32, totalPriority: 105, fee: 3800, grantFull: 5, grantPartial: 5 },
      { year: 2025, announced: 60, enrolled: 28, firstPriority: 25, totalPriority: 88,  fee: 4000, grantFull: 3, grantPartial: 4 },
    ],
    demandScore: 28, fillRateScore: 47, priorityQualityScore: 35, priceScore: 40,
    compositeScore: 36.3, category: 'Risky',
  },
  {
    id: 10, name: 'ჟურნალისტიკა', code: '0110201',
    universityId: 5, universityName: 'ილიას სახელმწიფო უნივერსიტეტი',
    fieldId: 6, fieldName: 'სოციალური მეცნიერებები',
    snaps: [
      { year: 2023, announced: 50, enrolled: 36, firstPriority: 30, totalPriority: 95,  fee: 3200, grantFull: 5, grantPartial: 4 },
      { year: 2024, announced: 50, enrolled: 32, firstPriority: 26, totalPriority: 82,  fee: 3500, grantFull: 4, grantPartial: 4 },
      { year: 2025, announced: 50, enrolled: 21, firstPriority: 20, totalPriority: 72,  fee: 3800, grantFull: 3, grantPartial: 3 },
    ],
    demandScore: 32, fillRateScore: 42, priorityQualityScore: 38, priceScore: 45,
    compositeScore: 37.9, category: 'Risky',
  },
  {
    id: 11, name: 'სტომატოლოგია', code: '0040101',
    universityId: 3, universityName: 'თბილისის სახელმწიფო სამედიცინო უნივერსიტეტი',
    fieldId: 3, fieldName: 'სამედიცინო მეცნიერებები',
    snaps: [
      { year: 2023, announced: 80, enrolled: 56, firstPriority: 52, totalPriority: 165, fee: 8500,  grantFull: 6, grantPartial: 8 },
      { year: 2024, announced: 80, enrolled: 55, firstPriority: 50, totalPriority: 158, fee: 9000,  grantFull: 6, grantPartial: 7 },
      { year: 2025, announced: 80, enrolled: 54, firstPriority: 48, totalPriority: 152, fee: 9500,  grantFull: 6, grantPartial: 7 },
    ],
    demandScore: 58, fillRateScore: 68, priorityQualityScore: 52, priceScore: 25,
    compositeScore: 54.1, category: 'Stable',
  },
  {
    id: 12, name: 'ფარმაცია', code: '0040201',
    universityId: 3, universityName: 'თბილისის სახელმწიფო სამედიცინო უნივერსიტეტი',
    fieldId: 3, fieldName: 'სამედიცინო მეცნიერებები',
    snaps: [
      { year: 2023, announced: 60, enrolled: 38, firstPriority: 28, totalPriority: 98, fee: 6500, grantFull: 3, grantPartial: 5 },
      { year: 2024, announced: 60, enrolled: 35, firstPriority: 26, totalPriority: 88, fee: 7000, grantFull: 3, grantPartial: 4 },
      { year: 2025, announced: 60, enrolled: 26, firstPriority: 22, totalPriority: 80, fee: 7500, grantFull: 2, grantPartial: 4 },
    ],
    demandScore: 38, fillRateScore: 44, priorityQualityScore: 42, priceScore: 30,
    compositeScore: 39.3, category: 'Risky',
  },
]

function snapFor(p: MockProg, year: number): YearSnap {
  return p.snaps.find((s) => s.year === year) ?? p.snaps[p.snaps.length - 1]
}

function toProgramListItem(p: MockProg, year = 2025): ProgramListItem {
  const s = snapFor(p, year)
  return {
    id: p.id,
    name: p.name,
    code: p.code,
    universityId: p.universityId,
    universityName: p.universityName,
    fieldId: p.fieldId,
    fieldName: p.fieldName,
    year: s.year,
    announcedPlaces: s.announced,
    enrolledCount: s.enrolled,
    firstPriorityCount: s.firstPriority,
    annualFee: s.fee,
    compositeScore: p.compositeScore,
    category: p.category,
  }
}

function paged<T>(items: T[], page = 1, pageSize = 20): PagedResponse<T> {
  const start = (page - 1) * pageSize
  return { data: items.slice(start, start + pageSize), total: items.length, page, pageSize }
}

function buildHealth(p: MockProg, year: number): ProgramHealth {
  const s = snapFor(p, year)
  return {
    programId: p.id,
    programName: p.name,
    universityName: p.universityName,
    fieldName: p.fieldName,
    year,
    demandScore: p.demandScore,
    fillRateScore: p.fillRateScore,
    priorityQualityScore: p.priorityQualityScore,
    priceScore: p.priceScore,
    compositeScore: p.compositeScore,
    category: p.category,
    fillRate: s.enrolled / s.announced,
    firstPriorityCount: s.firstPriority,
    enrolledCount: s.enrolled,
    announcedPlaces: s.announced,
    annualFee: s.fee,
  }
}

function buildTrend(id: number, name: string, snaps: YearSnap[]): ProgramTrend {
  const yoYDeltas = snaps.slice(1).map((s, i) => {
    const prev = snaps[i]
    const demandDelta = prev.firstPriority > 0
      ? (s.firstPriority - prev.firstPriority) / prev.firstPriority
      : null
    const fillDelta = prev.announced > 0 && s.announced > 0
      ? (s.enrolled / s.announced) - (prev.enrolled / prev.announced)
      : null
    const feeDelta = prev.fee > 0
      ? (s.fee - prev.fee) / prev.fee
      : null
    return { year: s.year, demandDelta: demandDelta ?? undefined, fillDelta: fillDelta ?? undefined, feeDelta: feeDelta ?? undefined }
  })

  const first = snaps[0]
  const last = snaps[snaps.length - 1]
  const years = snaps.length - 1
  const demandCagr = first.firstPriority > 0 && years > 0
    ? Math.pow(last.firstPriority / first.firstPriority, 1 / years) - 1
    : null

  const avgDelta = yoYDeltas.length > 0
    ? yoYDeltas.reduce((acc, d) => acc + (d.demandDelta ?? 0), 0) / yoYDeltas.length
    : 0

  const demandTrendLabel: 'Growing' | 'Stable' | 'Declining' =
    avgDelta > 0.03 ? 'Growing' : avgDelta < -0.03 ? 'Declining' : 'Stable'

  const yearSeries = snaps.map((s) => ({
    year: s.year,
    announcedPlaces: s.announced,
    enrolledCount: s.enrolled,
    firstPriorityCount: s.firstPriority,
    annualFee: s.fee,
  }))

  return { entityId: id, entityName: name, demandCagr, demandTrendLabel, yoYDeltas, yearSeries }
}

function buildForecast(p: MockProg): ProgramForecast {
  const snaps = p.snaps
  const historicalData = snaps.map((s) => ({ year: s.year, enrolledCount: s.enrolled }))

  const xs = snaps.map((s) => s.year)
  const ys = snaps.map((s) => s.enrolled)
  const n = xs.length
  const sumX = xs.reduce((a, b) => a + b, 0)
  const sumY = ys.reduce((a, b) => a + b, 0)
  const sumXY = xs.reduce((acc, x, i) => acc + x * ys[i], 0)
  const sumX2 = xs.reduce((acc, x) => acc + x * x, 0)
  const slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX)
  const intercept = (sumY - slope * sumX) / n
  const projYear = 2026
  const raw = Math.round(slope * projYear + intercept)
  const point = Math.max(0, raw)

  return {
    programId: p.id,
    programName: p.name,
    methodLabel: 'წრფივი ტრენდის პროექცია',
    pointEstimate: point,
    lowerBound: Math.round(point * 0.9),
    upperBound: Math.round(point * 1.1),
    projectedYear: projYear,
    historicalData,
  }
}

function buildConversion(p: MockProg): ProgramConversion {
  const yoYDeltas = p.snaps.map((s, i) => {
    const rate = s.firstPriority > 0 ? s.enrolled / s.firstPriority : 0
    const prev = i > 0 ? p.snaps[i - 1] : null
    const prevRate = prev && prev.firstPriority > 0 ? prev.enrolled / prev.firstPriority : null
    const delta = prevRate !== null && prevRate > 0 ? (rate - prevRate) / prevRate : null
    return { year: s.year, conversionRate: parseFloat(rate.toFixed(4)), delta }
  })
  const avg = yoYDeltas.reduce((acc, d) => acc + d.conversionRate, 0) / yoYDeltas.length
  return {
    programId: p.id,
    programName: p.name,
    historicalAvgConversion: parseFloat(avg.toFixed(4)),
    yoYDeltas,
  }
}

function buildFieldGap(fieldId: number, year: number): FieldGap {
  const field = FIELDS.find((f) => f.id === fieldId)!
  const progs = RAW.filter((p) => p.fieldId === fieldId)
  const snaps = progs.map((p) => snapFor(p, year))

  const aggregateDemand = snaps.reduce((a, s) => a + s.firstPriority, 0)
  const totalSupply = snaps.reduce((a, s) => a + s.announced, 0)
  const demandSupplyRatio = totalSupply > 0 ? aggregateDemand / totalSupply : 0
  const avgFillRate =
    snaps.length > 0 ? snaps.reduce((a, s) => a + s.enrolled / s.announced, 0) / snaps.length : 0

  const gapSeverity: 'High' | 'Medium' | 'Low' =
    demandSupplyRatio > 2 && avgFillRate > 0.9
      ? 'High'
      : demandSupplyRatio > 1.3
      ? 'Medium'
      : 'Low'

  return {
    fieldId,
    fieldName: field.name,
    aggregateDemand,
    totalSupply,
    demandSupplyRatio: parseFloat(demandSupplyRatio.toFixed(3)),
    avgFillRate: parseFloat(avgFillRate.toFixed(4)),
    gapSeverity,
    programCount: progs.length,
  }
}

export const mockApi = {
  universities: {
    list: async (): Promise<University[]> => {
      await pause()
      return UNIVERSITIES
    },
    create: async (body: CreateUniversityRequest): Promise<University> => {
      await pause()
      return { id: 99, name: body.name, shortName: body.shortName, code: body.code }
    },
  },

  fields: {
    list: async (): Promise<Field[]> => {
      await pause()
      return FIELDS
    },
    create: async (body: CreateFieldRequest): Promise<Field> => {
      await pause()
      return { id: 99, name: body.name, code: body.code }
    },
    update: async (id: number, body: UpdateFieldRequest): Promise<Field> => {
      await pause()
      return { id, name: body.name, code: body.code }
    },
  },

  programs: {
    list: async (params: ProgramListParams = {}): Promise<PagedResponse<ProgramListItem>> => {
      await pause()
      const year = params.year ?? 2025
      let items = RAW.map((p) => toProgramListItem(p, year))

      if (params.universityId)
        items = items.filter((p) => p.universityId === params.universityId)
      if (params.fieldId)
        items = items.filter((p) => p.fieldId === params.fieldId)
      if (params.healthCategory)
        items = items.filter((p) => p.category === params.healthCategory)
      if (params.minFee !== undefined)
        items = items.filter((p) => p.annualFee >= params.minFee!)
      if (params.maxFee !== undefined)
        items = items.filter((p) => p.annualFee <= params.maxFee!)
      if (params.search?.trim()) {
        const term = params.search.trim().toLowerCase()
        items = items.filter(
          (p) =>
            p.name.toLowerCase().includes(term) ||
            p.universityName.toLowerCase().includes(term),
        )
      }

      return paged(items, params.page ?? 1, params.pageSize ?? 20)
    },

    get: async (id: number): Promise<ProgramDetail> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')
      const uni = UNIVERSITIES.find((u) => u.id === p.universityId)!
      const field = FIELDS.find((f) => f.id === p.fieldId)!
      return {
        id: p.id,
        name: p.name,
        code: p.code,
        degreeLevel: 'ბაკალავრი',
        university: uni,
        field,
        yearStats: p.snaps.map((s) => ({
          year: s.year,
          announcedPlaces: s.announced,
          enrolledCount: s.enrolled,
          firstPriorityCount: s.firstPriority,
          totalPriorityCount: s.totalPriority,
          annualFee: s.fee,
          grantFullCount: s.grantFull,
          grantPartialCount: s.grantPartial,
        })),
      }
    },

    updateYearStats: async (
      _id: number,
      _year: number,
      _body: UpdateProgramYearRequest,
    ): Promise<void> => {
      await pause()
    },

    assignField: async (_id: number, _fieldId: number): Promise<void> => {
      await pause()
    },
  },

  competition: {
    field: async (fieldId: number, year: number): Promise<FieldCompetition> => {
      await pause()
      const field = FIELDS.find((f) => f.id === fieldId)!
      const progs = RAW.filter((p) => p.fieldId === fieldId)
      const totalDemand = progs.reduce((a, p) => a + snapFor(p, year).firstPriority, 0)

      const uniMap = new Map<number, { id: number; name: string; fp: number }>()
      for (const p of progs) {
        const fp = snapFor(p, year).firstPriority
        const existing = uniMap.get(p.universityId)
        if (existing) existing.fp += fp
        else uniMap.set(p.universityId, { id: p.universityId, name: p.universityName, fp })
      }

      const universities = Array.from(uniMap.values()).map((u) => ({
        universityId: u.id,
        universityName: u.name,
        firstPriorityCount: u.fp,
        marketSharePct: totalDemand > 0 ? parseFloat(((u.fp / totalDemand) * 100).toFixed(2)) : 0,
      }))

      return { fieldId, fieldName: field.name, year, totalDemand, universities }
    },

    program: async (id: number, fromYear: number, toYear: number): Promise<ProgramCompetitionTrend> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')

      const years = p.snaps
        .filter((s) => s.year >= fromYear && s.year <= toYear)
        .map((s) => {
          const fieldSnaps = RAW.filter((r) => r.fieldId === p.fieldId)
            .map((r) => snapFor(r, s.year))
          const fieldTotal = fieldSnaps.reduce((a, fs) => a + fs.firstPriority, 0)
          return {
            year: s.year,
            firstPriorityCount: s.firstPriority,
            fieldTotalDemand: fieldTotal,
            marketSharePct:
              fieldTotal > 0 ? parseFloat(((s.firstPriority / fieldTotal) * 100).toFixed(2)) : 0,
          }
        })

      return { programId: p.id, programName: p.name, fieldId: p.fieldId, fieldName: p.fieldName, years }
    },
  },

  health: {
    program: async (id: number, year: number): Promise<ProgramHealth> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')
      return buildHealth(p, year)
    },

    list: async (params: HealthListParams = {}): Promise<HealthListResponse> => {
      await pause()
      const year = params.year ?? 2025
      let items = RAW.map((p) => buildHealth(p, year))

      if (params.category) items = items.filter((h) => h.category === params.category)
      if (params.fieldId) items = items.filter((h) => {
        const p = RAW.find((r) => r.id === h.programId)
        return p?.fieldId === params.fieldId
      })
      if (params.universityId) items = items.filter((h) => {
        const p = RAW.find((r) => r.id === h.programId)
        return p?.universityId === params.universityId
      })
      if (params.minFee !== undefined) items = items.filter((h) => h.annualFee >= params.minFee!)
      if (params.maxFee !== undefined) items = items.filter((h) => h.annualFee <= params.maxFee!)

      const summary = {
        total: items.length,
        growingCount: items.filter((h) => h.category === 'Growing').length,
        stableCount: items.filter((h) => h.category === 'Stable').length,
        riskyCount: items.filter((h) => h.category === 'Risky').length,
        averageScore:
          items.length > 0
            ? parseFloat((items.reduce((a, h) => a + h.compositeScore, 0) / items.length).toFixed(1))
            : 0,
      }

      return { ...paged(items, params.page ?? 1, params.pageSize ?? 20), summary }
    },
  },

  forecast: {
    program: async (id: number): Promise<ProgramForecast> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')
      return buildForecast(p)
    },
  },

  trend: {
    program: async (id: number): Promise<ProgramTrend> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')
      return buildTrend(p.id, p.name, p.snaps)
    },

    field: async (fieldId: number): Promise<FieldTrend> => {
      await pause()
      const field = FIELDS.find((f) => f.id === fieldId)!
      const progs = RAW.filter((p) => p.fieldId === fieldId)
      const years = [2023, 2024, 2025]
      const aggregated: YearSnap[] = years.map((y) => {
        const ss = progs.map((p) => snapFor(p, y))
        return {
          year: y,
          announced: ss.reduce((a, s) => a + s.announced, 0),
          enrolled: ss.reduce((a, s) => a + s.enrolled, 0),
          firstPriority: ss.reduce((a, s) => a + s.firstPriority, 0),
          totalPriority: ss.reduce((a, s) => a + s.totalPriority, 0),
          fee: ss.length > 0 ? ss.reduce((a, s) => a + s.fee, 0) / ss.length : 0,
        }
      })
      return buildTrend(fieldId, field.name, aggregated)
    },

    university: async (id: number): Promise<UniversityTrend> => {
      await pause()
      const uni = UNIVERSITIES.find((u) => u.id === id)!
      const progs = RAW.filter((p) => p.universityId === id)
      const years = [2023, 2024, 2025]
      const aggregated: YearSnap[] = years.map((y) => {
        const ss = progs.map((p) => snapFor(p, y))
        return {
          year: y,
          announced: ss.reduce((a, s) => a + s.announced, 0),
          enrolled: ss.reduce((a, s) => a + s.enrolled, 0),
          firstPriority: ss.reduce((a, s) => a + s.firstPriority, 0),
          totalPriority: ss.reduce((a, s) => a + s.totalPriority, 0),
          fee: ss.length > 0 ? ss.reduce((a, s) => a + s.fee, 0) / ss.length : 0,
        }
      })
      return buildTrend(id, uni.name, aggregated)
    },
  },

  benchmark: {
    program: async (id: number, year: number): Promise<ProgramBenchmark> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')
      const peers = RAW.filter((r) => r.fieldId === p.fieldId)
      const snap = snapFor(p, year)

      const peerSnaps = peers.map((r) => snapFor(r, year))
      const sortedFill = [...peerSnaps].sort(
        (a, b) => b.enrolled / b.announced - a.enrolled / a.announced,
      )
      const sortedFee = [...peerSnaps].sort((a, b) => a.fee - b.fee)
      const myFill = snap.enrolled / snap.announced

      const medianFP =
        peerSnaps.length > 0
          ? [...peerSnaps].sort((a, b) => a.firstPriority - b.firstPriority)[
              Math.floor(peerSnaps.length / 2)
            ].firstPriority
          : snap.firstPriority

      const peerHealthScores = peers.map((r) => r.compositeScore)
      const avgHealth = peerHealthScores.reduce((a, b) => a + b, 0) / peerHealthScores.length

      const pct = (rank: number, total: number) =>
        parseFloat((((total - rank) / total) * 100).toFixed(1))

      const fillRankInField =
        sortedFill.findIndex((s) => s.enrolled / s.announced <= myFill) + 1
      const feeRankInField = sortedFee.findIndex((s) => s.fee >= snap.fee) + 1

      return {
        programId: p.id,
        programName: p.name,
        year,
        demandRatioVsMedian: medianFP > 0 ? parseFloat((snap.firstPriority / medianFP).toFixed(3)) : 1,
        fillRateRankInField: fillRankInField || 1,
        feeRankInField: feeRankInField || 1,
        healthDeltaVsFieldAvg: parseFloat((p.compositeScore - avgHealth).toFixed(2)),
        demandPercentile: pct(peers.findIndex((r) => r.id === p.id), peers.length),
        fillRatePercentile: pct(fillRankInField - 1, peers.length),
        feePercentile: pct(feeRankInField - 1, peers.length),
        healthPercentile: pct(
          [...peers].sort((a, b) => b.compositeScore - a.compositeScore).findIndex((r) => r.id === p.id),
          peers.length,
        ),
      }
    },
  },

  market: {
    gaps: async (year: number): Promise<FieldGap[]> => {
      await pause()
      return FIELDS.map((f) => buildFieldGap(f.id, year)).sort(
        (a, b) =>
          ['High', 'Medium', 'Low'].indexOf(a.gapSeverity) -
          ['High', 'Medium', 'Low'].indexOf(b.gapSeverity),
      )
    },

    overview: async (year: number): Promise<MarketOverview> => {
      await pause()
      const gaps = FIELDS.map((f) => buildFieldGap(f.id, year))
      const snaps = RAW.map((p) => snapFor(p, year))
      const totalDemand = snaps.reduce((a, s) => a + s.firstPriority, 0)
      const totalSupply = snaps.reduce((a, s) => a + s.announced, 0)
      const avgFillRate =
        snaps.length > 0
          ? snaps.reduce((a, s) => a + s.enrolled / s.announced, 0) / snaps.length
          : 0

      const topFields = [...gaps].sort((a, b) => b.aggregateDemand - a.aggregateDemand).slice(0, 3)
      const totalEnrolled = snaps.reduce((a, s) => a + s.enrolled, 0)
      const riskiest = [...gaps].sort(
        (a, b) =>
          ['High', 'Medium', 'Low'].indexOf(a.gapSeverity) -
          ['High', 'Medium', 'Low'].indexOf(b.gapSeverity),
      )[0]

      return {
        year,
        totalPrograms: RAW.length,
        totalUniversities: UNIVERSITIES.length,
        totalFields: FIELDS.length,
        totalSupply,
        totalEnrolled,
        totalDemand,
        avgFillRate: parseFloat(avgFillRate.toFixed(4)),
        avgHealthScore: 55,
        topFields,
        topRiskyFieldByGap: riskiest
          ? {
              fieldId: riskiest.fieldId,
              fieldName: riskiest.fieldName,
              gapSeverity: riskiest.gapSeverity,
            }
          : null,
      }
    },
  },

  priority: {
    distribution: async (id: number, year: number): Promise<PriorityDistribution> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')
      const snap = snapFor(p, year)

      const total = snap.totalPriority
      const fp = snap.firstPriority
      const dist: { priority: number; count: number }[] = []
      let remaining = total - fp
      for (let i = 2; i <= 10 && remaining > 0; i++) {
        const count = Math.max(1, Math.round(remaining * (1 / (i * 1.5))))
        dist.push({ priority: i, count: Math.min(count, remaining) })
        remaining -= count
      }
      dist.unshift({ priority: 1, count: fp })

      const weightedScore = dist.reduce((a, d) => a + d.count / d.priority, 0)

      return {
        programId: p.id,
        programName: p.name,
        year,
        firstPriorityCount: fp,
        totalPriorityCount: total,
        weightedDemandScore: parseFloat(weightedScore.toFixed(2)),
        interestBreadth: dist.filter((d) => d.count > 0).length,
        distribution: dist,
        isGranular: true,
      }
    },
  },

  conversion: {
    program: async (id: number): Promise<ProgramConversion> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')
      return buildConversion(p)
    },
  },

  feeSensitivity: {
    program: async (id: number): Promise<FeeSensitivity> => {
      await pause()
      const p = RAW.find((r) => r.id === id)
      if (!p) throw new Error('Not found')

      const snaps = p.snaps
      if (snaps.length < 2) {
        return {
          programId: p.id, programName: p.name,
          indicative: true, insufficientData: true,
          pearsonCorrelation: null, slopeSign: 'flat',
        }
      }

      const deltaFees = snaps.slice(1).map((s, i) =>
        snaps[i].fee > 0 ? (s.fee - snaps[i].fee) / snaps[i].fee : 0,
      )
      const deltaDemands = snaps.slice(1).map((s, i) =>
        snaps[i].firstPriority > 0
          ? (s.firstPriority - snaps[i].firstPriority) / snaps[i].firstPriority
          : 0,
      )
      const n = deltaFees.length
      if (n < 2) {
        return {
          programId: p.id, programName: p.name,
          indicative: true, insufficientData: true,
          pearsonCorrelation: null, slopeSign: 'flat',
        }
      }
      const meanF = deltaFees.reduce((a, b) => a + b, 0) / n
      const meanD = deltaDemands.reduce((a, b) => a + b, 0) / n
      const num = deltaFees.reduce((a, f, i) => a + (f - meanF) * (deltaDemands[i] - meanD), 0)
      const denF = Math.sqrt(deltaFees.reduce((a, f) => a + (f - meanF) ** 2, 0))
      const denD = Math.sqrt(deltaDemands.reduce((a, d) => a + (d - meanD) ** 2, 0))
      const r = denF > 0 && denD > 0 ? parseFloat((num / (denF * denD)).toFixed(4)) : null

      return {
        programId: p.id, programName: p.name,
        indicative: true, insufficientData: false,
        pearsonCorrelation: r,
        slopeSign: r === null ? 'flat' : r > 0.05 ? 'positive' : r < -0.05 ? 'negative' : 'flat',
      }
    },
  },

  portfolioItems: {
    list: async (universityId: number, year: number): Promise<PortfolioItem[]> => {
      await pause()
      const progs = RAW.filter((p) => p.universityId === universityId)
      return progs.map((p) => {
        const snap = snapFor(p, year)
        const fieldProgs = RAW.filter((r) => r.fieldId === p.fieldId)
        const fieldTotal = fieldProgs.reduce((a, r) => a + snapFor(r, year).firstPriority, 0)
        return {
          programId: p.id,
          programName: p.name,
          fieldId: p.fieldId,
          fieldName: p.fieldName,
          compositeScore: p.compositeScore,
          category: p.category,
          marketShareInField:
            fieldTotal > 0 ? parseFloat(((snap.firstPriority / fieldTotal) * 100).toFixed(2)) : 0,
        }
      })
    },
  },

  compare: {
    programs: async (ids: number[], _year: number): Promise<ProgramComparisonItem[]> => {
      await pause()
      return ids.flatMap((id) => {
        const p = RAW.find((r) => r.id === id)
        if (!p) return []
        const conv = buildConversion(p)
        const forecast = buildForecast(p)
        return [{
          programId: p.id,
          programName: p.name,
          demandScore: p.demandScore,
          fillRateScore: p.fillRateScore,
          priorityQualityScore: p.priorityQualityScore,
          priceScore: p.priceScore,
          compositeScore: p.compositeScore,
          category: p.category,
          historicalAvgConversion: conv.historicalAvgConversion,
          forecastPointEstimate: forecast.pointEstimate,
        }]
      })
    },
  },

  dashboard: {
    summary: async (year: number): Promise<DashboardSummary> => {
      await pause()
      const snaps = RAW.map((p) => ({ p, s: snapFor(p, year) }))
      const totalDemand = snaps.reduce((a, { s }) => a + s.firstPriority, 0)
      const avgFillRate =
        snaps.reduce((a, { s }) => a + s.enrolled / s.announced, 0) / snaps.length

      const growing = RAW.filter((p) => p.category === 'Growing')
        .sort((a, b) => b.compositeScore - a.compositeScore)
        .slice(0, 5)
        .map((p) => ({
          programId: p.id,
          programName: p.name,
          universityName: p.universityName,
          healthScore: p.compositeScore,
        }))

      const risky = RAW.filter((p) => p.category === 'Risky')
        .sort((a, b) => a.compositeScore - b.compositeScore)
        .slice(0, 5)
        .map((p) => ({
          programId: p.id,
          programName: p.name,
          universityName: p.universityName,
          healthScore: p.compositeScore,
        }))

      const topFields = FIELDS.map((f) => {
        const fps = RAW.filter((p) => p.fieldId === f.id)
        const demand = fps.reduce((a, p) => a + snapFor(p, year).firstPriority, 0)
        const fillRate =
          fps.length > 0
            ? fps.reduce((a, p) => {
                const s = snapFor(p, year)
                return a + s.enrolled / s.announced
              }, 0) / fps.length
            : 0
        return { fieldId: f.id, fieldName: f.name, demand, fillRate }
      })
        .sort((a, b) => b.demand - a.demand)
        .slice(0, 5)

      return {
        year,
        totalPrograms: RAW.length,
        totalUniversities: UNIVERSITIES.length,
        totalFields: FIELDS.length,
        avgFillRate: parseFloat(avgFillRate.toFixed(4)),
        totalDemand,
        topGrowingPrograms: growing,
        topRiskyPrograms: risky,
        topFields,
      }
    },
  },

  import: {
    enrollments: async (_file: File, year: number): Promise<ImportResult> => {
      await pause(600)
      return { rowsRead: 31704, rowsImported: 722, errors: [], year }
    },
    priorities: async (_file: File, year: number): Promise<ImportResult> => {
      await pause(400)
      return { rowsRead: 778, rowsImported: 778, errors: [], year }
    },
    handbook: async (_file: File, year: number): Promise<ImportResult> => {
      await pause(800)
      return { rowsRead: 834, rowsImported: 834, errors: [], year }
    },
  },

  meta: {
    years: async (): Promise<number[]> => {
      await pause()
      return [2025, 2024, 2023]
    },
  },
}
