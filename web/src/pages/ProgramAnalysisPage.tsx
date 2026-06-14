import { useState } from 'react'
import {
  useProgramBenchmark,
  useProgramForecast,
  usePriorityDistribution,
} from '@/hooks/usePrograms'
import { useDefaultYear } from '@/hooks/useMeta'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { FallbackYearNote } from '@/components/FallbackYearNote'
import { QueryError } from '@/components/QueryError'
import { ProgramSelector } from '@/components/ProgramSelector'
import { YearSelector } from '@/components/YearSelector'
import { PageSkeleton } from '@/components/PageSkeleton'
import { StatCard } from '@/components/StatCard'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Badge } from '@/components/ui/badge'
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from '@/components/ui/chart'
import {
  BarChart,
  Bar,
  Line,
  LineChart,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts'
import {
  Award,
  Banknote,
  BarChart2,
  Activity,
  Star,
  TrendingUp,
  Users,
  Maximize2,
  MoveVertical,
  CalendarClock,
} from 'lucide-react'
import type { ChartConfig } from '@/components/ui/chart'

const BENCHMARK_HELP = {
  fillRank: 'პროგრამის ადგილი თავის სფეროში შევსების მაჩვენებლით — #1 ნიშნავს ყველაზე მაღალ ჩარიცხვას.',
  feeRank: 'პროგრამის ადგილი სფეროში წლიური საფასურით — #1 ნიშნავს ყველაზე იაფ პროგრამას.',
  demandRatio: 'პროგრამის მოთხოვნა სფეროს მედიანასთან მიმართებაში. 1.0x = მედიანა, >1.0x = საშუალოზე მაღალი.',
  healthDelta: 'პროგრამის ჯანმრთელობის ქულის სხვაობა სფეროს საშუალოსთან. დადებითი = საშუალოზე უკეთესი.',
} as const

function PercentileBar({ label, value, color = 'bg-primary' }: { label: string; value: number; color?: string }) {
  const pct = Math.min(100, Math.max(0, value))
  return (
    <div className="space-y-1.5">
      <div className="flex justify-between text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="font-semibold tabular-nums">{pct.toFixed(0)}%</span>
      </div>
      <div className="h-2 rounded-full bg-muted overflow-hidden">
        <div className={`h-full rounded-full transition-all ${color}`} style={{ width: `${pct}%` }} />
      </div>
    </div>
  )
}

const PRIORITY_HELP = {
  firstPriority: 'აბიტურიენტების რაოდენობა, რომლებმაც ეს პროგრამა პირველ ადგილზე დააფიქსირეს განცხადებაში.',
  weighted: 'შეწონილი მოთხოვნა — თითო პრიორიტეტის განცხადებები იყოფა მის რანგზე და იჯამება.',
  breadth: 'რამდენ სხვადასხვა პრიორიტეტულ დონეზე ჩნდება პროგრამა — მოთხოვნის სიგანის მაჩვენებელი.',
} as const

const priorityChartConfig: ChartConfig = {
  count: { label: 'განაცხადი', color: 'var(--chart-2)' },
}

const FORECAST_HELP = {
  point: 'მოსალოდნელი ჩარიცხვების რაოდენობა მომავალ მისაღებზე — გასული წლების ტრენდის გაგრძელება.',
  range: 'პროგნოზის სავარაუდო დიაპაზონი — რეალური შედეგი დიდი ალბათობით ამ ფარგლებში მოხვდება.',
  year: 'წელი, რომლისთვისაც გაკეთდა პროგნოზი, და გამოყენებული სტატისტიკური მეთოდი.',
} as const

const forecastChartConfig: ChartConfig = {
  enrolled: { label: 'ჩარიცხული (ფაქტი)', color: 'var(--chart-1)' },
  forecast: { label: 'პროგნოზი', color: 'var(--chart-4)' },
}

function ProgramAnalysisContent() {
  const [programId, setProgramId] = useState<number | undefined>()
  const [year, setYear] = useDefaultYear()

  const {
    data: benchData,
    isLoading: loadingBench,
    isError: benchError,
    error: benchErrorObj,
    refetch: refetchBench,
  } = useProgramBenchmark(programId, year)
  const {
    data: priorityData,
    isLoading: loadingPriority,
    isError: priorityError,
    error: priorityErrorObj,
    refetch: refetchPriority,
  } = usePriorityDistribution(programId, year)
  const {
    data: forecastData,
    isLoading: loadingForecast,
    isError: forecastError,
    error: forecastErrorObj,
    refetch: refetchForecast,
  } = useProgramForecast(programId)

  return (
    <div className="p-6 space-y-5">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold">პროგრამის ანალიზი</h1>
        <div className="flex gap-3 flex-wrap">
          <ProgramSelector value={programId} onChange={setProgramId} />
          <YearSelector value={year} onChange={setYear} />
        </div>
      </div>

      <Tabs defaultValue="benchmark">
        <TabsList>
          <TabsTrigger value="benchmark">ბენჩმარქი</TabsTrigger>
          <TabsTrigger value="priority">პრიორიტეტები</TabsTrigger>
          <TabsTrigger value="forecast">პროგნოზი</TabsTrigger>
        </TabsList>

        <TabsContent value="benchmark" className="pt-5 space-y-5">
          {programId === undefined && (
            <Card className="border-dashed">
              <CardContent className="py-10 text-center text-sm text-muted-foreground">
                აირჩიეთ პროგრამა ანალიზის სანახავად
              </CardContent>
            </Card>
          )}
          {programId !== undefined && loadingBench && <PageSkeleton />}
          {programId !== undefined && benchError && (
            <QueryError error={benchErrorObj} onRetry={() => refetchBench()} />
          )}
          {benchData && (
            <>
              <FallbackYearNote shownYear={benchData.year} requestedYear={year} />
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <StatCard
                  icon={<Award className="h-4 w-4" />}
                  label="ჩარიცხვის რანგი"
                  value={`#${benchData.fillRateRankInField}`}
                  sub="სფეროში"
                  description={BENCHMARK_HELP.fillRank}
                />
                <StatCard
                  icon={<Banknote className="h-4 w-4" />}
                  label="ფასის რანგი"
                  value={`#${benchData.feeRankInField}`}
                  sub="სფეროში (1 = იაფი)"
                  description={BENCHMARK_HELP.feeRank}
                />
                <StatCard
                  icon={<BarChart2 className="h-4 w-4" />}
                  label="მოთხოვნა / მედიანა"
                  value={`${benchData.demandRatioVsMedian.toFixed(2)}x`}
                  sub="სფეროს მედიანასთან შედარებით"
                  description={BENCHMARK_HELP.demandRatio}
                />
                <StatCard
                  icon={<Activity className="h-4 w-4" />}
                  label="ჯანმრთელობის სხვაობა"
                  value={`${benchData.healthDeltaVsFieldAvg > 0 ? '+' : ''}${benchData.healthDeltaVsFieldAvg.toFixed(1)}`}
                  sub="სფეროს საშუალოსთან შედარებით"
                  description={BENCHMARK_HELP.healthDelta}
                  iconColor={benchData.healthDeltaVsFieldAvg >= 0 ? 'text-emerald-600' : 'text-amber-600'}
                />
              </div>
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-sm font-semibold">პერცენტილები სფეროში</CardTitle>
                  <p className="text-xs text-muted-foreground">
                    <span className={benchData.healthDeltaVsFieldAvg >= 0 ? 'font-semibold text-emerald-600' : 'font-semibold text-amber-600'}>
                      {benchData.healthDeltaVsFieldAvg > 0 ? '+' : ''}{benchData.healthDeltaVsFieldAvg.toFixed(1)}
                    </span>
                    {' '}ჯანმრთელობის ქულა სფეროს საშუალოსთან შედარებით
                  </p>
                </CardHeader>
                <CardContent className="space-y-5">
                  <PercentileBar label="მოთხოვნა" value={benchData.demandPercentile} color="bg-chart-2" />
                  <PercentileBar label="ჩარიცხვა" value={benchData.fillRatePercentile} color="bg-chart-1" />
                  <PercentileBar label="ფასი" value={benchData.feePercentile} color="bg-chart-3" />
                  <PercentileBar label="ჯანმრთელობა" value={benchData.healthPercentile} color="bg-primary" />
                </CardContent>
              </Card>
            </>
          )}
        </TabsContent>

        <TabsContent value="priority" className="pt-5 space-y-5">
          {programId === undefined && (
            <Card className="border-dashed">
              <CardContent className="py-10 text-center text-sm text-muted-foreground">
                აირჩიეთ პროგრამა ანალიზის სანახავად
              </CardContent>
            </Card>
          )}
          {programId !== undefined && loadingPriority && <PageSkeleton />}
          {programId !== undefined && priorityError && (
            <QueryError error={priorityErrorObj} onRetry={() => refetchPriority()} />
          )}
          {priorityData && (
            <>
              <FallbackYearNote shownYear={priorityData.year} requestedYear={year} />
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <StatCard
                  icon={<Star className="h-4 w-4" />}
                  label="I პრიორიტეტი"
                  value={priorityData.firstPriorityCount.toLocaleString()}
                  sub="სტუდენტი — პირველ ადგილზე"
                  description={PRIORITY_HELP.firstPriority}
                />
                <StatCard
                  icon={<Users className="h-4 w-4" />}
                  label="შეწონილი მოთხოვნა"
                  value={priorityData.weightedDemandScore.toFixed(1)}
                  sub="რანგით შეწონილი ჯამი"
                  description={PRIORITY_HELP.weighted}
                />
                <StatCard
                  icon={<Maximize2 className="h-4 w-4" />}
                  label="ინტერესის სიგანე"
                  value={`${priorityData.interestBreadth}`}
                  sub={priorityData.isGranular ? 'დეტალური მონაცემი' : 'შეჯამებული'}
                  description={PRIORITY_HELP.breadth}
                />
              </div>
              {priorityData.isGranular && priorityData.distribution && priorityData.distribution.length > 0 ? (
                <Card>
                  <CardHeader className="pb-2">
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-sm font-semibold">
                        {priorityData.programName} — პრიორიტეტის განაწილება
                      </CardTitle>
                      <Badge variant="secondary" className="text-xs">დეტალური</Badge>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <ChartContainer config={priorityChartConfig} className="h-72">
                      <BarChart
                        data={priorityData.distribution.map((d) => ({
                          priority: `${d.priority}`,
                          count: d.count,
                        }))}
                      >
                        <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                        <XAxis
                          dataKey="priority"
                          tick={{ fontSize: 11 }}
                          label={{ value: 'პრიორიტეტი', position: 'insideBottom', offset: -2, fontSize: 11 }}
                        />
                        <YAxis tick={{ fontSize: 11 }} />
                        <ChartTooltip content={<ChartTooltipContent />} />
                        <Bar dataKey="count" fill="var(--color-count)" radius={[3, 3, 0, 0]} />
                      </BarChart>
                    </ChartContainer>
                  </CardContent>
                </Card>
              ) : (
                <Card className="border-dashed">
                  <CardContent className="py-6">
                    <p className="text-sm text-muted-foreground">
                      დეტალური პრიორიტეტული მონაცემი მიუწვდომელია. I პრიორიტეტი:{' '}
                      <span className="font-semibold">{priorityData.firstPriorityCount}</span>, ჯამური:{' '}
                      <span className="font-semibold">{priorityData.totalPriorityCount ?? 'არ არის'}</span>.
                    </p>
                  </CardContent>
                </Card>
              )}
            </>
          )}
        </TabsContent>

        <TabsContent value="forecast" className="pt-5 space-y-5">
          {programId === undefined && (
            <Card className="border-dashed">
              <CardContent className="py-10 text-center text-sm text-muted-foreground">
                აირჩიეთ პროგრამა პროგნოზის სანახავად
              </CardContent>
            </Card>
          )}
          {programId !== undefined && loadingForecast && <PageSkeleton />}
          {programId !== undefined && forecastError && (
            <QueryError error={forecastErrorObj} onRetry={() => refetchForecast()} />
          )}
          {forecastData && (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <StatCard
                  icon={<TrendingUp className="h-4 w-4" />}
                  label={`პროგნოზი — ${forecastData.projectedYear} წ.`}
                  value={forecastData.pointEstimate.toLocaleString()}
                  sub="მოსალოდნელი ჩარიცხვა"
                  description={FORECAST_HELP.point}
                />
                <StatCard
                  icon={<MoveVertical className="h-4 w-4" />}
                  label="სავარაუდო დიაპაზონი"
                  value={`${forecastData.lowerBound.toLocaleString()}–${forecastData.upperBound.toLocaleString()}`}
                  sub="ქვედა — ზედა ზღვარი"
                  description={FORECAST_HELP.range}
                />
                <StatCard
                  icon={<CalendarClock className="h-4 w-4" />}
                  label="მეთოდი"
                  value={forecastData.methodLabel}
                  sub={`${forecastData.historicalData.length} წლის მონაცემზე დაყრდნობით`}
                  description={FORECAST_HELP.year}
                />
              </div>
              {forecastData.historicalData.length > 0 ? (
                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm font-semibold">
                      {forecastData.programName} — ჩარიცხვების დინამიკა და პროგნოზი
                    </CardTitle>
                    <p className="text-xs text-muted-foreground">
                      წყვეტილი ხაზი — {forecastData.projectedYear} წლის პროგნოზი
                    </p>
                  </CardHeader>
                  <CardContent>
                    <ChartContainer config={forecastChartConfig} className="h-72">
                      <LineChart
                        data={[
                          ...forecastData.historicalData.map((h, i) => ({
                            year: `${h.year}`,
                            enrolled: h.enrolledCount,

                            forecast: i === forecastData.historicalData.length - 1
                              ? h.enrolledCount
                              : undefined,
                          })),
                          {
                            year: `${forecastData.projectedYear}`,
                            enrolled: undefined,
                            forecast: forecastData.pointEstimate,
                          },
                        ]}
                      >
                        <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                        <XAxis dataKey="year" tick={{ fontSize: 12 }} />
                        <YAxis tick={{ fontSize: 12 }} allowDecimals={false} />
                        <ChartTooltip content={<ChartTooltipContent />} />
                        <Line
                          type="monotone"
                          dataKey="enrolled"
                          stroke="var(--color-enrolled)"
                          strokeWidth={2.5}
                          dot={{ r: 4 }}
                        />
                        <Line
                          type="monotone"
                          dataKey="forecast"
                          stroke="var(--color-forecast)"
                          strokeWidth={2.5}
                          strokeDasharray="6 4"
                          dot={{ r: 4 }}
                        />
                      </LineChart>
                    </ChartContainer>
                  </CardContent>
                </Card>
              ) : (
                <Card className="border-dashed">
                  <CardContent className="py-6 text-sm text-muted-foreground">
                    ისტორიული მონაცემები არ არის — პროგნოზი მიუწვდომელია.
                  </CardContent>
                </Card>
              )}
            </>
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}

export function ProgramAnalysisPage() {
  return (
    <ErrorBoundary>
      <ProgramAnalysisContent />
    </ErrorBoundary>
  )
}
