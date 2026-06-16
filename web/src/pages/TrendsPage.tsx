import { useState } from 'react'
import { useProgramTrend } from '@/hooks/usePrograms'
import { useFieldTrend } from '@/hooks/useFields'
import { useUniversityTrend } from '@/hooks/useUniversities'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { QueryError } from '@/components/QueryError'
import { ProgramSelector } from '@/components/ProgramSelector'
import { FieldSelector } from '@/components/FieldSelector'
import { UniversitySelector } from '@/components/UniversitySelector'
import { PageSkeleton } from '@/components/PageSkeleton'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
} from '@/components/ui/chart'
import { LineChart, Line, XAxis, YAxis, CartesianGrid } from 'recharts'
import { TrendingUp, TrendingDown, Minus } from 'lucide-react'
import type { ChartConfig } from '@/components/ui/chart'
import type { TrendDirection, TrendYearPoint } from '@/lib/types'

const DIRECTION_LABEL: Record<TrendDirection, string> = {
  Growing: 'ზრდა',
  Stable: 'სტაბილური',
  Declining: 'ვარდნა',
}

const DIRECTION_CLASS: Record<TrendDirection, string> = {
  Growing: 'bg-emerald-100 text-emerald-800 border-emerald-200',
  Stable: 'bg-blue-100 text-blue-700 border-blue-200',
  Declining: 'bg-red-100 text-red-800 border-red-200',
}

const countsChartConfig: ChartConfig = {
  firstPriorityCount: { label: 'პირველი პრიორიტეტი', color: 'var(--chart-2)' },
  enrolledCount: { label: 'ჩარიცხული', color: 'var(--chart-1)' },
  announcedPlaces: { label: 'გამოცხადებული ადგილები', color: 'var(--chart-4)' },
}

const feeChartConfig: ChartConfig = {
  annualFee: { label: 'წლიური ფასი (₾)', color: 'var(--chart-3)' },
}

function DirectionIcon({ d }: { d: TrendDirection }) {
  if (d === 'Growing') return <TrendingUp className="h-3.5 w-3.5 text-emerald-600" />
  if (d === 'Declining') return <TrendingDown className="h-3.5 w-3.5 text-red-600" />
  return <Minus className="h-3.5 w-3.5 text-blue-500" />
}

function TrendChart({ points, title, cagr, trendDirection }: {
  points: TrendYearPoint[]
  title: string
  cagr: number
  trendDirection: TrendDirection
}) {
  return (
    <div className="space-y-4">

      <div className="flex items-center gap-3 flex-wrap">
        <Badge variant="outline" className={DIRECTION_CLASS[trendDirection]}>
          <DirectionIcon d={trendDirection} />
          <span className="ml-1">{DIRECTION_LABEL[trendDirection]}</span>
        </Badge>
        <Badge variant="outline" className="text-xs">
          CAGR: {(cagr * 100).toFixed(1)}%
        </Badge>
        <span className="min-w-0 flex-1 truncate text-sm text-muted-foreground" title={title}>{title}</span>
      </div>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-semibold">წლიური მაჩვენებლები</CardTitle>
        </CardHeader>
        <CardContent>
          {points.length === 0 ? (
            <p className="text-sm text-muted-foreground">მონაცემები არ არის</p>
          ) : (
            <ChartContainer config={countsChartConfig} className="h-72">
              <LineChart data={points}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis dataKey="year" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} allowDecimals={false} />
                <ChartTooltip content={<ChartTooltipContent />} />
                <ChartLegend content={<ChartLegendContent />} />
                <Line type="monotone" dataKey="firstPriorityCount" stroke="var(--color-firstPriorityCount)" strokeWidth={2.5} dot={{ r: 4 }} />
                <Line type="monotone" dataKey="enrolledCount" stroke="var(--color-enrolledCount)" strokeWidth={2.5} dot={{ r: 4 }} />
                <Line type="monotone" dataKey="announcedPlaces" stroke="var(--color-announcedPlaces)" strokeWidth={2.5} dot={{ r: 4 }} />
              </LineChart>
            </ChartContainer>
          )}
        </CardContent>
      </Card>

      {points.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-semibold">წლიური ფასი (₾)</CardTitle>
          </CardHeader>
          <CardContent>
            <ChartContainer config={feeChartConfig} className="h-48">
              <LineChart data={points}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis dataKey="year" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} tickFormatter={(v: number) => `${Math.round(v)}₾`} />
                <ChartTooltip content={<ChartTooltipContent />} />
                <Line type="monotone" dataKey="annualFee" stroke="var(--color-annualFee)" strokeWidth={2.5} dot={{ r: 4 }} />
              </LineChart>
            </ChartContainer>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

function SelectPrompt({ text }: { text: string }) {
  return (
    <Card className="border-dashed">
      <CardContent className="py-10 text-center text-sm text-muted-foreground">
        {text}
      </CardContent>
    </Card>
  )
}

function ProgramTrendTab() {
  const [programId, setProgramId] = useState<number | undefined>()
  const { data, isLoading, isError, error, refetch } = useProgramTrend(programId)
  return (
    <div className="space-y-4">
      <ProgramSelector value={programId} onChange={setProgramId} />
      {programId === undefined && <SelectPrompt text="აირჩიეთ პროგრამა ტენდენციის სანახავად" />}
      {programId !== undefined && isLoading && <PageSkeleton />}
      {programId !== undefined && isError && <QueryError error={error} onRetry={() => refetch()} />}
      {data && (
        <TrendChart
          points={data.yearSeries}
          title={data.entityName}
          cagr={data.demandCagr ?? 0}
          trendDirection={data.demandTrendLabel}
        />
      )}
    </div>
  )
}

function FieldTrendTab() {
  const [fieldId, setFieldId] = useState<number | undefined>()
  const { data, isLoading, isError, error, refetch } = useFieldTrend(fieldId)
  return (
    <div className="space-y-4">
      <FieldSelector value={fieldId} onChange={setFieldId} />
      {fieldId === undefined && <SelectPrompt text="აირჩიეთ სფერო ტენდენციის სანახავად" />}
      {fieldId !== undefined && isLoading && <PageSkeleton />}
      {fieldId !== undefined && isError && <QueryError error={error} onRetry={() => refetch()} />}
      {data && (
        <TrendChart
          points={data.yearSeries}
          title={data.entityName}
          cagr={data.demandCagr ?? 0}
          trendDirection={data.demandTrendLabel}
        />
      )}
    </div>
  )
}

function UniversityTrendTab() {
  const [universityId, setUniversityId] = useState<number | undefined>()
  const { data, isLoading, isError, error, refetch } = useUniversityTrend(universityId)
  return (
    <div className="space-y-4">
      <UniversitySelector value={universityId} onChange={setUniversityId} />
      {universityId === undefined && <SelectPrompt text="აირჩიეთ უნივერსიტეტი ტენდენციის სანახავად" />}
      {universityId !== undefined && isLoading && <PageSkeleton />}
      {universityId !== undefined && isError && <QueryError error={error} onRetry={() => refetch()} />}
      {data && (
        <TrendChart
          points={data.yearSeries}
          title={data.entityName}
          cagr={data.demandCagr ?? 0}
          trendDirection={data.demandTrendLabel}
        />
      )}
    </div>
  )
}

function TrendsContent() {
  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-semibold">ტენდენციების ანალიზი</h1>
      <Tabs defaultValue="program">
        <TabsList>
          <TabsTrigger value="program">პროგრამა</TabsTrigger>
          <TabsTrigger value="field">სფერო</TabsTrigger>
          <TabsTrigger value="university">უნივერსიტეტი</TabsTrigger>
        </TabsList>
        <TabsContent value="program" className="pt-5">
          <ProgramTrendTab />
        </TabsContent>
        <TabsContent value="field" className="pt-5">
          <FieldTrendTab />
        </TabsContent>
        <TabsContent value="university" className="pt-5">
          <UniversityTrendTab />
        </TabsContent>
      </Tabs>
    </div>
  )
}

export function TrendsPage() {
  return (
    <ErrorBoundary>
      <TrendsContent />
    </ErrorBoundary>
  )
}
