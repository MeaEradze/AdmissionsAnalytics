import { useState } from 'react'
import { useMarketGaps, useMarketOverview } from '@/hooks/useMarket'
import { useFieldCompetition, useFields } from '@/hooks/useFields'
import { useDefaultYear } from '@/hooks/useMeta'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { QueryError } from '@/components/QueryError'
import { YearSelector } from '@/components/YearSelector'
import { FieldSelector } from '@/components/FieldSelector'
import { PageSkeleton } from '@/components/PageSkeleton'
import { StatCard } from '@/components/StatCard'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from '@/components/ui/chart'
import {
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  ReferenceLine,
} from 'recharts'
import { BookOpen, TrendingUp, Package, Target, Users, Building2, Trophy } from 'lucide-react'
import type { ChartConfig } from '@/components/ui/chart'
import type { GapSeverity } from '@/lib/types'

const SEVERITY_LABEL: Record<GapSeverity, string> = {
  High: 'მაღალი',
  Medium: 'საშუალო',
  Low: 'დაბალი',
}

const SEVERITY_CLASS: Record<GapSeverity, string> = {
  High: 'bg-red-100 text-red-800 border-red-200',
  Medium: 'bg-amber-100 text-amber-800 border-amber-200',
  Low: 'bg-emerald-100 text-emerald-800 border-emerald-200',
}

const SEVERITY_COLOR: Record<GapSeverity, string> = {
  High: 'var(--chart-5)',
  Medium: 'var(--chart-3)',
  Low: 'var(--chart-1)',
}

const CHART_COLORS = [
  'var(--chart-1)',
  'var(--chart-2)',
  'var(--chart-3)',
  'var(--chart-4)',
  'var(--chart-5)',
]

const marketChartConfig: ChartConfig = {
  demandSupplyRatio: { label: 'მოთხოვნა/მიწოდება', color: 'var(--chart-2)' },
}

const MARKET_STAT_HELP = {
  programs: 'ბაზარზე არსებული აქტიური საბაკალავრო პროგრამების საერთო რაოდენობა მოცემული წლისთვის.',
  demand: 'ყველა პროგრამის პირველი პრიორიტეტით განცხადებების ჯამი — საერთო მოთხოვნა.',
  supply: 'ყველა პროგრამის გამოცხადებული ადგილების ჯამი — საერთო მიწოდება.',
  fillRate: 'საშუალო შევსების მაჩვენებელი: ჩარიცხულები გაყოფილი გამოცხადებულ ადგილებზე.',
} as const

function MarketCompetitionContent() {
  const [year, setYear] = useDefaultYear()

  const { data: fields } = useFields()
  const [pickedFieldId, setPickedFieldId] = useState<number | undefined>()
  const fieldId = pickedFieldId ?? fields?.[0]?.id

  const {
    data: overview,
    isLoading: loadingOverview,
    isError: overviewError,
    error: overviewErrorObj,
    refetch: refetchOverview,
  } = useMarketOverview(year)
  const {
    data: gaps,
    isLoading: loadingGaps,
    isError: gapsError,
    refetch: refetchGaps,
  } = useMarketGaps(year)
  const {
    data: fieldComp,
    isLoading: loadingField,
    isError: fieldError,
    error: fieldErrorObj,
    refetch: refetchField,
  } = useFieldCompetition(fieldId, year)

  const sortedUniversities = (fieldComp?.universities ?? [])
    .slice()
    .sort((a, b) => b.marketSharePct - a.marketSharePct)

  const pieConfig: ChartConfig = Object.fromEntries(
    sortedUniversities.map((u, i) => [
      `u${u.universityId}`,
      { label: u.universityName, color: CHART_COLORS[i % CHART_COLORS.length] },
    ])
  )

  return (
    <div className="p-6 space-y-5">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold">ბაზარი და კონკურენცია</h1>
        <YearSelector value={year} onChange={setYear} />
      </div>

      <Tabs defaultValue="market">
        <TabsList>
          <TabsTrigger value="market">ბაზრის ანალიზი</TabsTrigger>
          <TabsTrigger value="competition">კონკურენცია</TabsTrigger>
        </TabsList>

        <TabsContent value="market" className="pt-5 space-y-6">
          {(loadingOverview || loadingGaps) && <PageSkeleton />}
          {(overviewError || gapsError) && (
            <QueryError
              error={overviewErrorObj}
              onRetry={() => { void refetchOverview(); void refetchGaps() }}
            />
          )}
          {overview && (
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              <StatCard
                icon={<BookOpen className="h-4 w-4" />}
                label="პროგრამები"
                value={overview.totalPrograms.toString()}
                description={MARKET_STAT_HELP.programs}
              />
              <StatCard
                icon={<TrendingUp className="h-4 w-4" />}
                label="საერთო მოთხოვნა"
                value={overview.totalDemand.toLocaleString()}
                description={MARKET_STAT_HELP.demand}
              />
              <StatCard
                icon={<Package className="h-4 w-4" />}
                label="საერთო მიწოდება"
                value={overview.totalSupply.toLocaleString()}
                description={MARKET_STAT_HELP.supply}
              />
              <StatCard
                icon={<Target className="h-4 w-4" />}
                label="საშუალო ჩარიცხვა"
                value={`${(overview.avgFillRate * 100).toFixed(1)}%`}
                description={MARKET_STAT_HELP.fillRate}
              />
            </div>
          )}
          {gaps && gaps.length > 0 && (
            <>
              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-semibold">
                    მოთხოვნა / მიწოდება — სფეროების მიხედვით
                  </CardTitle>
                  <p className="text-xs text-muted-foreground">
                    1.0x ნიშნავს ბალანსს; {'>'}2.0x — მაღალი დეფიციტი
                  </p>
                </CardHeader>
                <CardContent>
                  <ChartContainer config={marketChartConfig} className="h-80">
                    <BarChart
                      data={gaps.map((g) => ({
                        name: g.fieldName,
                        demandSupplyRatio: g.demandSupplyRatio,
                        severity: g.gapSeverity,
                      }))}
                      layout="vertical"
                      margin={{ left: 8, right: 16 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                      <XAxis type="number" tick={{ fontSize: 11 }} />
                      <YAxis dataKey="name" type="category" width={230} tick={{ fontSize: 11 }} interval={0} />
                      <ChartTooltip content={<ChartTooltipContent />} />
                      <ReferenceLine x={1} stroke="var(--muted-foreground)" strokeDasharray="3 3" />
                      <Bar dataKey="demandSupplyRatio" radius={[0, 3, 3, 0]}>
                        {gaps.map((g) => (
                          <Cell key={g.fieldId} fill={SEVERITY_COLOR[g.gapSeverity]} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ChartContainer>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="pb-0">
                  <CardTitle className="text-sm font-semibold">სფეროების ანალიზი</CardTitle>
                </CardHeader>
                <CardContent className="p-0 mt-2">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="pl-5">სფერო</TableHead>
                        <TableHead className="text-right">მოთხოვნა</TableHead>
                        <TableHead className="text-right">მიწოდება</TableHead>
                        <TableHead className="text-right">თანაფარდობა</TableHead>
                        <TableHead className="text-right">ჩარიცხვა</TableHead>
                        <TableHead>სიმძიმე</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {gaps.map((g) => (
                        <TableRow key={g.fieldId} className="hover:bg-muted/30">
                          <TableCell className="pl-5 font-medium">{g.fieldName}</TableCell>
                          <TableCell className="text-right tabular-nums">
                            {g.aggregateDemand.toLocaleString()}
                          </TableCell>
                          <TableCell className="text-right tabular-nums">
                            {g.totalSupply.toLocaleString()}
                          </TableCell>
                          <TableCell className="text-right tabular-nums font-semibold">
                            {g.demandSupplyRatio.toFixed(2)}x
                          </TableCell>
                          <TableCell className="text-right tabular-nums">
                            {(g.avgFillRate * 100).toFixed(1)}%
                          </TableCell>
                          <TableCell>
                            <Badge variant="outline" className={`text-xs ${SEVERITY_CLASS[g.gapSeverity]}`}>
                              {SEVERITY_LABEL[g.gapSeverity]}
                            </Badge>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </CardContent>
              </Card>
            </>
          )}
          {!loadingOverview && !loadingGaps && !overviewError && !gapsError &&
            (!gaps || gaps.length === 0) && (
            <p className="text-muted-foreground">მონაცემები არ არის</p>
          )}
        </TabsContent>

        <TabsContent value="competition" className="pt-5 space-y-5">
          <div className="flex items-center gap-3 flex-wrap">
            <FieldSelector value={fieldId} onChange={setPickedFieldId} />
          </div>

          {fieldId !== undefined && loadingField && <PageSkeleton />}
          {fieldId !== undefined && fieldError && (
            <QueryError error={fieldErrorObj} onRetry={() => refetchField()} />
          )}

          {fieldComp && (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <StatCard
                  icon={<Users className="h-4 w-4" />}
                  label="საერთო I პრიორიტეტი"
                  value={fieldComp.totalDemand.toLocaleString()}
                  sub={fieldComp.fieldName}
                  description="ამ სფეროს ყველა პროგრამის პირველი პრიორიტეტით განცხადებების ჯამი."
                />
                <StatCard
                  icon={<Building2 className="h-4 w-4" />}
                  label="უნივერსიტეტები"
                  value={fieldComp.universities.length}
                  sub="სფეროში მონაწილე"
                  description="რამდენი უნივერსიტეტი სთავაზობს ამ სფეროს პროგრამებს."
                />
                <StatCard
                  icon={<Trophy className="h-4 w-4" />}
                  label="ლიდერის წილი"
                  value={
                    fieldComp.universities.length > 0
                      ? `${Math.max(...fieldComp.universities.map((u) => u.marketSharePct)).toFixed(1)}%`
                      : '—'
                  }
                  sub="უმსხვილესი მოთამაშე"
                  description="ყველაზე დიდი ბაზრის წილის მქონე უნივერსიტეტის წილი ამ სფეროში."
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5 items-start">
                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm font-semibold">ბაზრის წილი — {year}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    {fieldComp.universities.length === 0 ? (
                      <p className="text-sm text-muted-foreground">მონაცემები არ არის</p>
                    ) : fieldComp.universities.length === 1 ? (
                      <div className="flex h-60 flex-col items-center justify-center gap-2 text-center">
                        <Trophy className="h-8 w-8 text-primary" />
                        <p className="text-sm font-medium">{fieldComp.universities[0].universityName}</p>
                        <p className="text-3xl font-bold text-primary">100%</p>
                        <p className="text-xs text-muted-foreground">ერთადერთი უნივერსიტეტი ამ სფეროში</p>
                      </div>
                    ) : (
                      <ChartContainer config={pieConfig} className="h-96 w-full">
                        <PieChart>
                          <ChartTooltip content={<ChartTooltipContent />} />
                          <Pie
                            data={sortedUniversities}
                            dataKey="marketSharePct"
                            nameKey="universityName"
                            cx="50%"
                            cy="50%"
                            outerRadius={130}
                            paddingAngle={2}
                          >
                            {sortedUniversities.map((_u, i) => (
                              <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                            ))}
                          </Pie>
                        </PieChart>
                      </ChartContainer>
                    )}
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader className="pb-2">
                    <CardTitle className="text-sm font-semibold">უნივერსიტეტები</CardTitle>
                  </CardHeader>
                  <CardContent className="p-0">
                    {fieldComp.universities.length === 0 ? (
                      <p className="text-sm text-muted-foreground px-5 py-3">მონაცემები არ არის</p>
                    ) : (
                      <Table>
                        <TableHeader>
                          <TableRow>
                            <TableHead>უნივერსიტეტი</TableHead>
                            <TableHead className="text-right">I პრიორიტეტი</TableHead>
                            <TableHead className="text-right">წილი</TableHead>
                          </TableRow>
                        </TableHeader>
                        <TableBody>
                          {sortedUniversities.map((u, i) => (
                            <TableRow key={u.universityId}>
                              <TableCell className="whitespace-normal break-words">
                                <div className="flex items-start gap-2">
                                  <span
                                    className="mt-1.5 inline-block h-2.5 w-2.5 rounded-full shrink-0"
                                    style={{ background: CHART_COLORS[i % CHART_COLORS.length] }}
                                  />
                                  <span className="text-sm">{u.universityName}</span>
                                </div>
                              </TableCell>
                              <TableCell className="text-right tabular-nums">
                                {u.firstPriorityCount}
                              </TableCell>
                              <TableCell className="text-right tabular-nums font-semibold">
                                {u.marketSharePct.toFixed(1)}%
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    )}
                  </CardContent>
                </Card>
              </div>
            </>
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}

export function MarketCompetitionPage() {
  return (
    <ErrorBoundary>
      <MarketCompetitionContent />
    </ErrorBoundary>
  )
}
