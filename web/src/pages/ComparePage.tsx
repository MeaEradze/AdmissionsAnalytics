import { useState } from 'react'
import { useProgramComparison } from '@/hooks/usePrograms'
import { usePrograms } from '@/hooks/usePrograms'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import { useDefaultYear } from '@/hooks/useMeta'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { FallbackYearNote } from '@/components/FallbackYearNote'
import { QueryError } from '@/components/QueryError'
import { YearSelector } from '@/components/YearSelector'
import { PageSkeleton } from '@/components/PageSkeleton'
import { HealthBadge } from '@/components/HealthBadge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
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
  ChartLegend,
  ChartLegendContent,
} from '@/components/ui/chart'
import { RadarChart, Radar, PolarGrid, PolarAngleAxis, PolarRadiusAxis } from 'recharts'
import { GitCompareArrows } from 'lucide-react'
import type { ChartConfig } from '@/components/ui/chart'

const COLORS = [
  'var(--chart-1)',
  'var(--chart-2)',
  'var(--chart-3)',
  'var(--chart-4)',
  'var(--chart-5)',
]

function ProgramMultiSelector({
  selectedIds,
  selectedNames,
  onAdd,
  onRemove,
}: {
  selectedIds: number[]

  selectedNames: Map<number, string>
  onAdd: (id: number, name: string) => void
  onRemove: (id: number) => void
}) {
  const [search, setSearch] = useState('')
  const [open, setOpen] = useState(false)

  const term = useDebouncedValue(search, 300).trim()
  const { data, isFetching } = usePrograms({ search: term || undefined, pageSize: 20 })
  const filtered = (data?.data ?? []).filter((p) => !selectedIds.includes(p.id))

  return (
    <div className="space-y-2">
      <div className="relative w-[280px]">
        <Input
          value={search}
          onChange={(e) => { setSearch(e.target.value); setOpen(true) }}
          onFocus={() => setOpen(true)}
          onBlur={() => setTimeout(() => setOpen(false), 200)}
          placeholder="პროგრამის ძიება..."
          disabled={selectedIds.length >= 5}
        />
        {open && selectedIds.length < 5 && (
          <div className="absolute z-50 mt-1 w-full max-h-60 overflow-y-auto rounded-md border bg-popover shadow-md">
            {filtered.map((p) => (
              <div
                key={p.id}
                className="cursor-pointer px-3 py-2 text-sm hover:bg-accent"
                onMouseDown={() => { onAdd(p.id, p.name); setSearch(''); setOpen(false) }}
              >
                <div className="font-medium truncate">{p.name}</div>
                <div className="text-xs text-muted-foreground">{p.universityName}</div>
              </div>
            ))}
            {filtered.length === 0 && (
              <p className="px-3 py-2 text-sm text-muted-foreground">
                {isFetching
                  ? 'იტვირთება...'
                  : term
                    ? 'პროგრამა ვერ მოიძებნა'
                    : 'ჩაწერეთ პროგრამის ან უნივერსიტეტის სახელი'}
              </p>
            )}
          </div>
        )}
      </div>
      {selectedIds.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {selectedIds.map((id) => (
            <div
              key={id}
              className="flex items-center gap-1 rounded-full border bg-secondary px-3 py-1 text-sm"
            >
              <span className="max-w-[320px] truncate">{selectedNames.get(id) ?? `#${id}`}</span>
              <Button
                variant="ghost"
                size="sm"
                className="h-4 w-4 p-0 hover:bg-destructive/20"
                onClick={() => onRemove(id)}
              >
                ×
              </Button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

function CompareContent() {
  const [selectedIds, setSelectedIds] = useState<number[]>([])
  const [pickedNames, setPickedNames] = useState<Map<number, string>>(new Map())
  const [year, setYear] = useDefaultYear()

  const { data, isLoading, isError, error, refetch } = useProgramComparison(selectedIds, year)

  function addProgram(id: number, name: string) {
    if (selectedIds.length < 5 && !selectedIds.includes(id)) {
      setSelectedIds((prev) => [...prev, id])
      setPickedNames((prev) => new Map(prev).set(id, name))
    }
  }

  function removeProgram(id: number) {
    setSelectedIds((prev) => prev.filter((i) => i !== id))
  }

  const programs = data ?? []

  const selectedNames = new Map(pickedNames)
  for (const p of programs) selectedNames.set(p.programId, p.programName)

  const fallbackItem = programs.find((p) => p.isFallback && p.year !== undefined)

  const radarConfig: ChartConfig = Object.fromEntries(
    programs.map((p, i) => [
      `p${p.programId}`,
      { label: p.programName, color: COLORS[i % COLORS.length] },
    ])
  )

  const radarData = programs.length > 0
    ? [
        { metric: 'ჯანმრთელობა', ...Object.fromEntries(programs.map((p) => [`p${p.programId}`, p.compositeScore])) },
        { metric: 'მოთხოვნა', ...Object.fromEntries(programs.map((p) => [`p${p.programId}`, p.demandScore])) },
        { metric: 'ჩარიცხვა', ...Object.fromEntries(programs.map((p) => [`p${p.programId}`, p.fillRateScore])) },
        { metric: 'ფასი', ...Object.fromEntries(programs.map((p) => [`p${p.programId}`, p.priceScore])) },
      ]
    : []

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold">პროგრამების შედარება</h1>
        <YearSelector value={year} onChange={setYear} />
      </div>

      <ProgramMultiSelector
        selectedIds={selectedIds}
        selectedNames={selectedNames}
        onAdd={addProgram}
        onRemove={removeProgram}
      />

      {selectedIds.length > 0 && isError && (
        <QueryError error={error} onRetry={() => refetch()} />
      )}

      {fallbackItem && (
        <FallbackYearNote shownYear={fallbackItem.year} requestedYear={year} />
      )}

      {selectedIds.length === 0 && (
        <Card className="border-dashed">
          <CardContent className="flex flex-col items-center justify-center gap-3 py-16 text-center">
            <div className="rounded-full bg-muted p-4">
              <GitCompareArrows className="h-7 w-7 text-muted-foreground" />
            </div>
            <p className="text-base font-medium">აირჩიეთ პროგრამები შესადარებლად</p>
            <p className="max-w-sm text-sm text-muted-foreground">
              მოძებნეთ და დაამატეთ 1–5 პროგრამა ზემოთ მოცემული ველის გამოყენებით —
              შედეგად მიიღებთ გვერდ-გვერდ ცხრილს და რადარულ დიაგრამას.
            </p>
          </CardContent>
        </Card>
      )}

      {selectedIds.length > 0 && isLoading && <PageSkeleton />}

      {programs.length > 0 && (
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">გვერდ-გვერდ შედარება</CardTitle>
            </CardHeader>
            <CardContent className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>მეტრიკა</TableHead>
                    {programs.map((p) => (
                      <TableHead key={p.programId} className="min-w-[120px] whitespace-normal align-top">
                        <div className="line-clamp-2 break-words py-1" title={p.programName}>{p.programName}</div>
                      </TableHead>
                    ))}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableRow>
                    <TableCell className="font-medium">ჯანმრთელობა</TableCell>
                    {programs.map((p) => <TableCell key={p.programId}>{p.compositeScore.toFixed(0)}</TableCell>)}
                  </TableRow>
                  <TableRow>
                    <TableCell className="font-medium">მოთხოვნა</TableCell>
                    {programs.map((p) => <TableCell key={p.programId}>{p.demandScore.toFixed(0)}</TableCell>)}
                  </TableRow>
                  <TableRow>
                    <TableCell className="font-medium">ჩარიცხვა</TableCell>
                    {programs.map((p) => <TableCell key={p.programId}>{p.fillRateScore.toFixed(0)}</TableCell>)}
                  </TableRow>
                  <TableRow>
                    <TableCell className="font-medium">ფასი</TableCell>
                    {programs.map((p) => <TableCell key={p.programId}>{p.priceScore.toFixed(0)}</TableCell>)}
                  </TableRow>
                  <TableRow>
                    <TableCell className="font-medium">კონვერსია</TableCell>
                    {programs.map((p) => <TableCell key={p.programId}>{(p.historicalAvgConversion * 100).toFixed(1)}%</TableCell>)}
                  </TableRow>
                  <TableRow>
                    <TableCell className="font-medium">პროგნოზი (ჩარიცხვა)</TableCell>
                    {programs.map((p) => (
                      <TableCell key={p.programId}>{p.forecastPointEstimate.toLocaleString()}</TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <TableCell className="font-medium">კატეგორია</TableCell>
                    {programs.map((p) => (
                      <TableCell key={p.programId}><HealthBadge category={p.category} /></TableCell>
                    ))}
                  </TableRow>
                </TableBody>
              </Table>
            </CardContent>
          </Card>

          {programs.length > 1 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">რადარული შედარება</CardTitle>
              </CardHeader>
              <CardContent>
                <ChartContainer config={radarConfig} className="h-72">
                  <RadarChart data={radarData}>
                    <PolarGrid />
                    <PolarAngleAxis dataKey="metric" tick={{ fontSize: 12 }} />
                    <PolarRadiusAxis />
                    <ChartTooltip content={<ChartTooltipContent />} />
                    <ChartLegend content={<ChartLegendContent />} />
                    {programs.map((p, i) => (
                      <Radar
                        key={p.programId}
                        dataKey={`p${p.programId}`}
                        stroke={COLORS[i % COLORS.length]}
                        fill={COLORS[i % COLORS.length]}
                        fillOpacity={0.15}
                      />
                    ))}
                  </RadarChart>
                </ChartContainer>
              </CardContent>
            </Card>
          )}
        </div>
      )}
    </div>
  )
}

export function ComparePage() {
  return (
    <ErrorBoundary>
      <CompareContent />
    </ErrorBoundary>
  )
}
