import { useEffect, useState } from 'react'
import { useHealthList } from '@/hooks/usePrograms'
import { useDefaultYear } from '@/hooks/useMeta'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { QueryError } from '@/components/QueryError'
import { YearSelector } from '@/components/YearSelector'
import { FieldSelector } from '@/components/FieldSelector'
import { UniversitySelector } from '@/components/UniversitySelector'
import { HealthBadge } from '@/components/HealthBadge'
import { PageSkeleton } from '@/components/PageSkeleton'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
} from '@/components/ui/chart'
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts'
import { TrendingUp, TrendingDown, Minus, Heart } from 'lucide-react'
import type { ChartConfig } from '@/components/ui/chart'
import type { HealthCategory, ProgramHealth } from '@/lib/types'

const CATEGORY_OPTIONS: { label: string; value: HealthCategory | 'all' }[] = [
  { label: 'ყველა', value: 'all' },
  { label: 'მზარდი', value: 'Growing' },
  { label: 'სტაბილური', value: 'Stable' },
  { label: 'რისკიანი', value: 'Risky' },
]

const donutConfig: ChartConfig = {
  Growing: { label: 'მზარდი', color: 'var(--chart-1)' },
  Stable: { label: 'სტაბილური', color: 'var(--chart-2)' },
  Risky: { label: 'რისკიანი', color: 'var(--chart-3)' },
}

const subScoreConfig: ChartConfig = {
  score: { label: 'ქულა', color: 'var(--chart-2)' },
}

const donutColors: Record<string, string> = {
  Growing: 'var(--chart-1)',
  Stable: 'var(--chart-2)',
  Risky: 'var(--chart-3)',
}

function HealthDetailDialog({
  program,
  open,
  onOpenChange,
}: {
  program: ProgramHealth | null
  open: boolean
  onOpenChange: (v: boolean) => void
}) {
  if (!program) return null

  const subScores = [
    { name: 'მოთხოვნა', score: program.demandScore },
    { name: 'ჩარიცხვა', score: program.fillRateScore },
    { name: 'პრიორიტეტი', score: program.priorityQualityScore },
    { name: 'ფასი', score: program.priceScore },
  ]

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle className="text-base leading-snug">{program.programName}</DialogTitle>
          <p className="text-xs text-muted-foreground">{program.universityName}</p>
        </DialogHeader>
        <div className="space-y-4">
          <div className="flex items-center gap-3">
            <div className="rounded-full bg-primary/10 px-4 py-2">
              <span className="text-3xl font-bold text-primary">{program.compositeScore.toFixed(0)}</span>
            </div>
            <HealthBadge category={program.category} />
          </div>

          <ChartContainer config={subScoreConfig} className="h-44">
            <BarChart data={subScores} layout="vertical">
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis type="number" domain={[0, 100]} tick={{ fontSize: 11 }} />
              <YAxis dataKey="name" type="category" width={80} tick={{ fontSize: 11 }} />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Bar dataKey="score" fill="var(--color-score)" radius={[0, 3, 3, 0]} />
            </BarChart>
          </ChartContainer>

          <div className="grid grid-cols-2 gap-3 text-sm border-t pt-3">
            <div>
              <p className="text-xs text-muted-foreground">ჩარიცხული</p>
              <p className="font-semibold tabular-nums">{program.enrolledCount}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">განცხადებული ადგილები</p>
              <p className="font-semibold tabular-nums">{program.announcedPlaces}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">I პრიორიტეტი</p>
              <p className="font-semibold tabular-nums">{program.firstPriorityCount}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">საფასური</p>
              <p className="font-semibold tabular-nums">{program.annualFee.toLocaleString()} ₾</p>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}

function CategoryIcon({ category }: { category: HealthCategory }) {
  if (category === 'Growing') return <TrendingUp className="h-3 w-3 text-emerald-600" />
  if (category === 'Risky') return <TrendingDown className="h-3 w-3 text-amber-600" />
  return <Minus className="h-3 w-3 text-blue-500" />
}

const PAGE_SIZE = 20

function HealthContent() {
  const [year, setYear] = useDefaultYear()
  const [fieldId, setFieldId] = useState<number | undefined>()
  const [universityId, setUniversityId] = useState<number | undefined>()
  const [category, setCategory] = useState<HealthCategory | 'all'>('all')
  const [selected, setSelected] = useState<ProgramHealth | null>(null)
  const [page, setPage] = useState(1)

  const [minFeeText, setMinFeeText] = useState('')
  const [maxFeeText, setMaxFeeText] = useState('')
  const [feeRange, setFeeRange] = useState<{ min?: number; max?: number }>({})

  useEffect(() => {
    const handle = setTimeout(() => {
      const min = minFeeText === '' ? undefined : Number(minFeeText)
      const max = maxFeeText === '' ? undefined : Number(maxFeeText)
      setFeeRange({
        min: min !== undefined && !Number.isNaN(min) && min >= 0 ? min : undefined,
        max: max !== undefined && !Number.isNaN(max) && max >= 0 ? max : undefined,
      })
    }, 400)
    return () => clearTimeout(handle)
  }, [minFeeText, maxFeeText])

  useEffect(() => setPage(1), [year, fieldId, universityId, category, feeRange])

  const { data, isLoading, isError, error, refetch } = useHealthList({
    year,
    fieldId,
    universityId,
    category: category === 'all' ? undefined : category,
    minFee: feeRange.min,
    maxFee: feeRange.max,
    page,
    pageSize: PAGE_SIZE,
  })

  const programs = data?.data ?? []
  const total = data?.total ?? 0
  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE))

  const summary = data?.summary ?? {
    total: programs.length,
    growingCount: programs.filter((p) => p.category === 'Growing').length,
    stableCount: programs.filter((p) => p.category === 'Stable').length,
    riskyCount: programs.filter((p) => p.category === 'Risky').length,
    averageScore: programs.length > 0
      ? programs.reduce((s, p) => s + p.compositeScore, 0) / programs.length
      : 0,
  }

  const donutData = [
    { name: 'Growing', label: 'მზარდი', value: summary.growingCount },
    { name: 'Stable', label: 'სტაბილური', value: summary.stableCount },
    { name: 'Risky', label: 'რისკიანი', value: summary.riskyCount },
  ].filter((d) => d.value > 0)

  return (
    <div className="p-6 space-y-5">

      <div className="flex items-start justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold">ჯანმრთელობის ინდექსი</h1>
        <div className="flex gap-2 flex-wrap">
          <Select value={category} onValueChange={(v) => setCategory(v as HealthCategory | 'all')}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="კატეგორია" />
            </SelectTrigger>
            <SelectContent>
              {CATEGORY_OPTIONS.map((o) => (
                <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <FieldSelector value={fieldId} onChange={setFieldId} />
          <UniversitySelector value={universityId} onChange={setUniversityId} />
          <YearSelector value={year} onChange={setYear} />
          <Input
            type="number"
            min={0}
            placeholder="ფასი — დან (₾)"
            className="w-[130px]"
            value={minFeeText}
            onChange={(e) => setMinFeeText(e.target.value)}
          />
          <Input
            type="number"
            min={0}
            placeholder="ფასი — მდე (₾)"
            className="w-[130px]"
            value={maxFeeText}
            onChange={(e) => setMaxFeeText(e.target.value)}
          />
        </div>
      </div>

      {isLoading && <PageSkeleton />}

      {isError && <QueryError error={error} onRetry={() => refetch()} />}

      {!isLoading && !isError && summary.total > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">

          <div className="md:col-span-2 grid grid-cols-2 gap-4">
            <div className="rounded-lg border bg-card p-4 flex items-start gap-3">
              <div className="rounded-md bg-emerald-100 p-2 shrink-0">
                <TrendingUp className="h-4 w-4 text-emerald-700" />
              </div>
              <div>
                <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">მზარდი</p>
                <p className="text-2xl font-bold text-emerald-700">{summary.growingCount}</p>
                <p className="text-xs text-muted-foreground">
                  {`${((summary.growingCount / summary.total) * 100).toFixed(0)}%`}
                </p>
              </div>
            </div>

            <div className="rounded-lg border bg-card p-4 flex items-start gap-3">
              <div className="rounded-md bg-amber-100 p-2 shrink-0">
                <TrendingDown className="h-4 w-4 text-amber-700" />
              </div>
              <div>
                <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">რისკიანი</p>
                <p className="text-2xl font-bold text-amber-700">{summary.riskyCount}</p>
                <p className="text-xs text-muted-foreground">
                  {`${((summary.riskyCount / summary.total) * 100).toFixed(0)}%`}
                </p>
              </div>
            </div>

            <div className="rounded-lg border bg-card p-4 flex items-start gap-3">
              <div className="rounded-md bg-blue-100 p-2 shrink-0">
                <Minus className="h-4 w-4 text-blue-700" />
              </div>
              <div>
                <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">სტაბილური</p>
                <p className="text-2xl font-bold text-blue-700">{summary.stableCount}</p>
                <p className="text-xs text-muted-foreground">
                  {`${((summary.stableCount / summary.total) * 100).toFixed(0)}%`}
                </p>
              </div>
            </div>

            <div className="rounded-lg border bg-card p-4 flex items-start gap-3">
              <div className="rounded-md bg-primary/10 p-2 shrink-0">
                <Heart className="h-4 w-4 text-primary" />
              </div>
              <div>
                <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">საშუალო ქულა</p>
                <p className="text-2xl font-bold text-primary">{summary.averageScore.toFixed(1)}</p>
                <p className="text-xs text-muted-foreground">{summary.total} პროგრამა</p>
              </div>
            </div>
          </div>

          {donutData.length > 0 && (
            <Card className="flex flex-col">
              <CardHeader className="pb-0">
                <CardTitle className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                  განაწილება
                </CardTitle>
              </CardHeader>
              <CardContent className="flex flex-1 items-center">
                <ChartContainer config={donutConfig} className="h-48">
                  <PieChart>
                    <ChartTooltip content={<ChartTooltipContent />} />
                    <Pie
                      data={donutData}
                      dataKey="value"
                      nameKey="label"
                      cx="50%"
                      cy="50%"
                      innerRadius={35}
                      outerRadius={60}
                      paddingAngle={3}
                    >
                      {donutData.map((d, i) => (
                        <Cell key={i} fill={donutColors[d.name]} />
                      ))}
                    </Pie>
                    <ChartLegend content={<ChartLegendContent />} />
                  </PieChart>
                </ChartContainer>
              </CardContent>
            </Card>
          )}
        </div>
      )}

      {!isLoading && !isError && summary.total === 0 && (
        <p className="text-muted-foreground">მონაცემები არ არის</p>
      )}

      {programs.length > 0 && (
        <Card>
          <CardHeader className="pb-0">
            <CardTitle className="text-sm font-semibold">
              პროგრამები — დააჭირეთ დეტალებისთვის
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0 mt-2">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="pl-5 w-[30%]">პროგრამა</TableHead>
                  <TableHead className="w-[26%]">უნივერსიტეტი</TableHead>
                  <TableHead className="w-[14%]">სფერო</TableHead>
                  <TableHead className="text-right">ქულა</TableHead>
                  <TableHead>კატეგორია</TableHead>
                  <TableHead className="text-right">ჩარიცხული</TableHead>
                  <TableHead className="text-right">I პრიორიტეტი</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {programs.map((p) => (
                  <TableRow
                    key={p.programId}
                    className="cursor-pointer hover:bg-muted/40"
                    onClick={() => setSelected(p)}
                  >
                    <TableCell className="pl-5 whitespace-normal break-words">
                      <div className="flex items-start gap-2">
                        <span className="mt-0.5 shrink-0"><CategoryIcon category={p.category} /></span>
                        <span className="font-medium text-sm line-clamp-3" title={p.programName}>
                          {p.programName}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell className="whitespace-normal break-words text-sm text-muted-foreground">
                      {p.universityName}
                    </TableCell>
                    <TableCell className="whitespace-normal break-words text-sm text-muted-foreground">
                      {p.fieldName}
                    </TableCell>
                    <TableCell className="text-right tabular-nums font-semibold">
                      {p.compositeScore.toFixed(0)}
                    </TableCell>
                    <TableCell><HealthBadge category={p.category} /></TableCell>
                    <TableCell className="text-right tabular-nums">{p.enrolledCount}</TableCell>
                    <TableCell className="text-right tabular-nums">{p.firstPriorityCount}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {totalPages > 1 && (
              <div className="flex items-center justify-between border-t px-5 py-3">
                <p className="text-xs text-muted-foreground tabular-nums">
                  ნაჩვენებია {(page - 1) * PAGE_SIZE + 1}–{Math.min(page * PAGE_SIZE, total)} / {total}
                </p>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => setPage((p) => p - 1)}
                  >
                    წინა
                  </Button>
                  <span className="text-xs text-muted-foreground tabular-nums">
                    {page} / {totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= totalPages}
                    onClick={() => setPage((p) => p + 1)}
                  >
                    შემდეგი
                  </Button>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      <HealthDetailDialog
        program={selected}
        open={selected !== null}
        onOpenChange={(v) => { if (!v) setSelected(null) }}
      />
    </div>
  )
}

export function HealthPage() {
  return (
    <ErrorBoundary>
      <HealthContent />
    </ErrorBoundary>
  )
}
