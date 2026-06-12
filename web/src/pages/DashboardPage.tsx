import { useDashboardSummary } from '@/hooks/useDashboard'
import { useDefaultYear } from '@/hooks/useMeta'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { QueryError } from '@/components/QueryError'
import { YearSelector } from '@/components/YearSelector'
import { PageSkeleton } from '@/components/PageSkeleton'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { ChartContainer, ChartTooltip, ChartTooltipContent } from '@/components/ui/chart'
import { StatCard } from '@/components/StatCard'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts'
import {
  BookOpen,
  Building2,
  Layers,
  Target,
  TrendingUp,
  TrendingDown,
  Minus,
} from 'lucide-react'
import type { ChartConfig } from '@/components/ui/chart'

const chartConfig: ChartConfig = {
  demand: { label: 'მოთხოვნა', color: 'var(--chart-1)' },
}

function TrendIcon({ score }: { score: number }) {
  if (score >= 70) return <TrendingUp className="h-3.5 w-3.5 text-emerald-500 shrink-0" />
  if (score < 45) return <TrendingDown className="h-3.5 w-3.5 text-rose-500 shrink-0" />
  return <Minus className="h-3.5 w-3.5 text-amber-500 shrink-0" />
}

function scoreColor(score: number) {
  if (score >= 70) return 'text-emerald-600'
  if (score < 45) return 'text-rose-600'
  return 'text-amber-600'
}

function ScoreValue({ score }: { score: number }) {
  return (
    <div className="shrink-0 text-right">
      <span className={`text-base font-bold tabular-nums ${scoreColor(score)}`}>
        {score.toFixed(0)}
      </span>
      <span className="ml-0.5 text-[10px] text-muted-foreground">/100</span>
    </div>
  )
}

function buildStatCards(
  data: { totalPrograms: number; totalUniversities: number; totalFields: number; avgFillRate: number; totalDemand: number },
) {
  return [
    {
      icon: <BookOpen className="h-4 w-4" />,
      label: 'პროგრამები',
      value: data.totalPrograms,
    },
    {
      icon: <Building2 className="h-4 w-4" />,
      label: 'უნივერსიტეტები',
      value: data.totalUniversities,
    },
    {
      icon: <Layers className="h-4 w-4" />,
      label: 'სფეროები',
      value: data.totalFields,
    },
    {
      icon: <Target className="h-4 w-4" />,
      label: 'საშუალო ჩარიცხვა',
      value: `${(data.avgFillRate * 100).toFixed(1)}%`,
    },
    {
      icon: <TrendingUp className="h-4 w-4" />,
      label: 'საერთო მოთხოვნა',
      value: data.totalDemand.toLocaleString(),
    },
  ]
}

function DashboardContent() {
  const [year, setYear] = useDefaultYear()
  const { data, isLoading, isError, error, refetch } = useDashboardSummary(year)

  if (isLoading) return <PageSkeleton />

  if (isError) {
    return (
      <div className="p-6 space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold">მიმოხილვა</h1>
          <YearSelector value={year} onChange={setYear} />
        </div>
        <QueryError error={error} onRetry={() => refetch()} />
      </div>
    )
  }

  if (!data) {
    return (
      <div className="p-6">
        <p className="text-muted-foreground">მონაცემები არ არის</p>
      </div>
    )
  }

  const statCards = buildStatCards(data)

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">მიმოხილვა</h1>
        <YearSelector value={year} onChange={setYear} />
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-5 gap-4">
        {statCards.map((card) => (
          <StatCard key={card.label} {...card} layout="horizontal" />
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center gap-2">
              <TrendingUp className="h-4 w-4 text-emerald-500" />
              <CardTitle className="text-base">მზარდი პროგრამები</CardTitle>
            </div>
            <p className="text-xs text-muted-foreground">ჯანმრთელობის ყველაზე მაღალი ქულა</p>
          </CardHeader>
          <CardContent className="space-y-2.5">
            {data.topGrowingPrograms.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                მონაცემები არ არის — „მზარდი" კატეგორია წინა წლის მონაცემებს საჭიროებს
              </p>
            ) : (
              data.topGrowingPrograms.map((p) => (
                <div key={p.programId} className="flex items-center gap-3">
                  <TrendIcon score={p.healthScore} />
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium leading-snug line-clamp-2 break-words">{p.programName}</p>
                    <p className="text-xs text-muted-foreground truncate">{p.universityName}</p>
                  </div>
                  <ScoreValue score={p.healthScore} />
                </div>
              ))
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center gap-2">
              <TrendingDown className="h-4 w-4 text-rose-500" />
              <CardTitle className="text-base">რისკიანი პროგრამები</CardTitle>
            </div>
            <p className="text-xs text-muted-foreground">ყველაზე დაბალი ჯანმრთელობის ქულა</p>
          </CardHeader>
          <CardContent className="space-y-2.5">
            {data.topRiskyPrograms.length === 0 ? (
              <p className="text-sm text-muted-foreground">მონაცემები არ არის</p>
            ) : (
              data.topRiskyPrograms.map((p) => (
                <div key={p.programId} className="flex items-center gap-3">
                  <TrendIcon score={p.healthScore} />
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium leading-snug line-clamp-2 break-words">{p.programName}</p>
                    <p className="text-xs text-muted-foreground truncate">{p.universityName}</p>
                  </div>
                  <ScoreValue score={p.healthScore} />
                </div>
              ))
            )}
          </CardContent>
        </Card>

        <Card className="md:col-span-2">
          <CardHeader className="pb-3">
            <div className="flex items-center gap-2">
              <Layers className="h-4 w-4 text-amber-500" />
              <CardTitle className="text-base">სფეროების მოთხოვნა</CardTitle>
            </div>
            <p className="text-xs text-muted-foreground">პირველი პრიორიტეტი სფეროს მიხედვით</p>
          </CardHeader>
          <CardContent>
            {data.topFields.length === 0 ? (
              <p className="text-sm text-muted-foreground">მონაცემები არ არის</p>
            ) : (
              <ChartContainer config={chartConfig} className="h-56 w-full">
                <BarChart
                  data={data.topFields.map((f) => ({ name: f.fieldName, demand: f.demand }))}
                  layout="vertical"
                  margin={{ left: 4, right: 16, top: 4, bottom: 4 }}
                >
                  <CartesianGrid strokeDasharray="3 3" horizontal={false} />
                  <XAxis type="number" />
                  <YAxis
                    dataKey="name"
                    type="category"
                    width={230}
                    tick={{ fontSize: 11 }}
                    interval={0}
                  />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="demand" fill="var(--color-demand)" radius={[0, 3, 3, 0]} />
                </BarChart>
              </ChartContainer>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

export function DashboardPage() {
  return (
    <ErrorBoundary>
      <DashboardContent />
    </ErrorBoundary>
  )
}
