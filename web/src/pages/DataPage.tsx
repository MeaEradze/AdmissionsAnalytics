import { useState, useRef } from 'react'
import { toast } from 'sonner'
import { useImportEnrollments, useImportPriorities, useImportHandbook } from '@/hooks/useImport'
import { useUpdateProgramYearStats, useAssignProgramField, useProgram } from '@/hooks/usePrograms'
import { useCreateField } from '@/hooks/useFields'
import { useDefaultYear } from '@/hooks/useMeta'
import { ErrorBoundary } from '@/components/ErrorBoundary'
import { ProgramSelector } from '@/components/ProgramSelector'
import { FieldSelector } from '@/components/FieldSelector'
import { YearSelector } from '@/components/YearSelector'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { FileSpreadsheet, FileText, UploadCloud } from 'lucide-react'
import type { ImportResult, UpdateProgramYearRequest } from '@/lib/types'

function errorText(error: unknown, fallback: string): string {
  return error instanceof Error && error.message ? error.message : fallback
}

function ImportSection({
  title,
  description,
  icon,
  accept,
  onImport,
  isPending,
  result,
}: {
  title: string
  description: string
  icon: React.ReactNode

  accept: string
  onImport: (file: File, year: number) => void
  isPending: boolean
  result: ImportResult | null
}) {
  const [year, setYear] = useDefaultYear()
  const [fileName, setFileName] = useState<string | null>(null)
  const fileRef = useRef<HTMLInputElement>(null)

  function handleSubmit() {
    const file = fileRef.current?.files?.[0]
    if (!file) { toast.error('გთხოვთ ფაილი ატვირთოთ'); return }
    onImport(file, year)
  }

  return (
    <Card className="flex flex-col">
      <CardHeader className="pb-3">
        <div className="flex items-center gap-2">
          <span className="text-primary">{icon}</span>
          <CardTitle className="text-sm font-semibold">{title}</CardTitle>
        </div>
        <p className="text-xs text-muted-foreground leading-snug">{description}</p>
      </CardHeader>
      <CardContent className="flex flex-1 flex-col gap-3">
        <button
          type="button"
          className="flex flex-col items-center justify-center gap-1.5 rounded-lg border-2 border-dashed border-muted px-3 py-5 text-center transition-colors hover:border-primary"
          onClick={() => fileRef.current?.click()}
        >
          <UploadCloud className="h-5 w-5 text-muted-foreground" />
          <p className="text-xs text-muted-foreground break-all">
            {fileName ?? 'ფაილის ასარჩევად დააჭირეთ'}
          </p>
        </button>
        <input
          ref={fileRef}
          type="file"
          className="hidden"
          accept={accept}
          onChange={(e) => setFileName(e.target.files?.[0]?.name ?? null)}
        />
        <div className="flex items-center gap-2">
          <YearSelector value={year} onChange={setYear} />
          <Button onClick={handleSubmit} disabled={isPending} className="flex-1">
            {isPending ? 'ატვირთვა...' : 'ატვირთვა'}
          </Button>
        </div>

        {result && (
          <div className="rounded-lg border p-3 space-y-1 text-xs">
            <p>წაკითხული: <span className="font-semibold">{result.rowsRead}</span></p>
            <p>შემოტანილი: <span className="font-semibold">{result.rowsImported}</span></p>
            {result.errors.length > 0 && (
              <div>
                <p className="text-destructive font-medium">შეცდომები ({result.errors.length}):</p>
                <ul className="list-disc pl-4 text-muted-foreground max-h-24 overflow-y-auto">
                  {result.errors.map((e, i) => <li key={i}>{e}</li>)}
                </ul>
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

const REQUIRED_FIELDS: [keyof UpdateProgramYearRequest, string][] = [
  ['announcedPlaces', 'განცხ. ადგილები'],
  ['enrolledCount', 'ჩარიცხული'],
  ['firstPriorityCount', 'I პრიორ.'],
  ['annualFee', 'საფასური (₾)'],
]

function EditYearStatsSection() {
  const [programId, setProgramId] = useState<number | undefined>()
  const [year, setYear] = useDefaultYear()
  const [dialogOpen, setDialogOpen] = useState(false)
  const { data: program } = useProgram(programId)
  const updateMutation = useUpdateProgramYearStats()

  const existingStats = program?.yearStats.find((s) => s.year === year)

  const [form, setForm] = useState<UpdateProgramYearRequest>({
    announcedPlaces: 0,
    enrolledCount: 0,
    firstPriorityCount: 0,
    totalPriorityCount: undefined,
    annualFee: 0,
    grantFullCount: undefined,
    grantPartialCount: undefined,
  })

  function openDialog() {
    if (existingStats) {
      setForm({
        announcedPlaces: existingStats.announcedPlaces,
        enrolledCount: existingStats.enrolledCount,
        firstPriorityCount: existingStats.firstPriorityCount,
        totalPriorityCount: existingStats.totalPriorityCount,
        annualFee: existingStats.annualFee,
        grantFullCount: existingStats.grantFullCount,
        grantPartialCount: existingStats.grantPartialCount,
      })
    } else {
      setForm({
        announcedPlaces: 0,
        enrolledCount: 0,
        firstPriorityCount: 0,
        totalPriorityCount: undefined,
        annualFee: 0,
        grantFullCount: undefined,
        grantPartialCount: undefined,
      })
    }
    setDialogOpen(true)
  }

  function handleSave() {
    if (!programId) return

    const missing = REQUIRED_FIELDS.filter(([key]) => {
      const v = form[key]
      return v === undefined || Number.isNaN(v)
    })
    if (missing.length > 0) {
      toast.error(`შეავსეთ სავალდებულო ველები: ${missing.map(([, label]) => label).join(', ')}`)
      return
    }

    const negative = Object.values(form).some((v) => typeof v === 'number' && v < 0)
    if (negative) {
      toast.error('მნიშვნელობები არ შეიძლება იყოს უარყოფითი')
      return
    }

    updateMutation.mutate(
      { id: programId, year, body: form },
      {
        onSuccess: () => { toast.success('მონაცემები განახლდა'); setDialogOpen(false) },
        onError: (e) => toast.error(errorText(e, 'შეცდომა განახლებისას')),
      }
    )
  }

  function setField(key: keyof UpdateProgramYearRequest, value: string) {
    const num = Number(value)
    setForm((prev) => ({ ...prev, [key]: value === '' ? undefined : num }))
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">პროგრამის წლიური სტატისტიკის რედაქტირება</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex gap-3 flex-wrap items-end">
          <ProgramSelector value={programId} onChange={setProgramId} />
          <YearSelector value={year} onChange={setYear} />
          <Button onClick={openDialog} disabled={!programId} variant="outline">
            რედაქტირება
          </Button>
        </div>

        {program && (
          <p className="text-sm text-muted-foreground">
            {program.name} — {program.university.name}
          </p>
        )}
      </CardContent>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {program?.name} — {year} წ.
            </DialogTitle>
          </DialogHeader>
          <div className="grid grid-cols-2 gap-3 text-sm">
            {(
              [
                ['announcedPlaces', 'განცხ. ადგილები'],
                ['enrolledCount', 'ჩარიცხული'],
                ['firstPriorityCount', 'I პრიორ.'],
                ['totalPriorityCount', 'ჯამ. პრიორ.'],
                ['annualFee', 'საფასური (₾)'],
                ['grantFullCount', 'სრული გრანტი'],
                ['grantPartialCount', 'ნაწ. გრანტი'],
              ] as [keyof UpdateProgramYearRequest, string][]
            ).map(([key, label]) => (
              <div key={key} className="space-y-1">
                <label className="text-xs text-muted-foreground">{label}</label>
                <Input
                  type="number"
                  value={form[key] ?? ''}
                  onChange={(e) => setField(key, e.target.value)}
                />
              </div>
            ))}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>გაუქმება</Button>
            <Button onClick={handleSave} disabled={updateMutation.isPending}>
              {updateMutation.isPending ? 'შენახვა...' : 'შენახვა'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  )
}

function ReferenceDataSection() {
  const createField = useCreateField()
  const assignField = useAssignProgramField()

  const [fieldName, setFieldName] = useState('')
  const [fieldCode, setFieldCode] = useState('')
  const [assignProgramId, setAssignProgramId] = useState<number | undefined>()
  const [assignFieldId, setAssignFieldId] = useState<number | undefined>()

  function handleCreateField() {
    if (!fieldName.trim()) { toast.error('შეიყვანეთ სფეროს სახელი'); return }
    createField.mutate(
      { name: fieldName.trim(), code: fieldCode.trim() || undefined },
      {
        onSuccess: () => { toast.success('სფერო დაემატა'); setFieldName(''); setFieldCode('') },
        onError: (e) => toast.error(errorText(e, 'შეცდომა სფეროს დამატებისას')),
      }
    )
  }

  function handleAssign() {
    if (assignProgramId === undefined || assignFieldId === undefined) {
      toast.error('აირჩიეთ პროგრამა და სფერო')
      return
    }
    assignField.mutate(
      { id: assignProgramId, fieldId: assignFieldId },
      {
        onSuccess: () => toast.success('პროგრამა მიეკუთვნა სფეროს'),
        onError: (e) => toast.error(errorText(e, 'შეცდომა მიკუთვნებისას')),
      }
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">ცნობარების მართვა</CardTitle>
        <p className="text-xs text-muted-foreground">
          სფეროებს წყარო ფაილები არ ქმნის — დაამატეთ აქედან და მიაკუთვნეთ პროგრამები სფეროებს.
        </p>
      </CardHeader>
      <CardContent className="grid gap-6 md:grid-cols-2">
        <div className="space-y-2">
          <p className="text-sm font-medium">ახალი სფერო</p>
          <Input
            placeholder="სახელი (მაგ. სამედიცინო მეცნიერებები)"
            value={fieldName}
            onChange={(e) => setFieldName(e.target.value)}
          />
          <Input
            placeholder="კოდი (არასავალდებულო)"
            value={fieldCode}
            onChange={(e) => setFieldCode(e.target.value)}
          />
          <Button
            onClick={handleCreateField}
            disabled={createField.isPending}
            variant="outline"
            className="w-full"
          >
            {createField.isPending ? 'დამატება...' : 'დამატება'}
          </Button>
        </div>

        <div className="space-y-2">
          <p className="text-sm font-medium">პროგრამის მიკუთვნება სფეროზე</p>
          <ProgramSelector value={assignProgramId} onChange={setAssignProgramId} />
          <FieldSelector value={assignFieldId} onChange={setAssignFieldId} />
          <Button
            onClick={handleAssign}
            disabled={assignField.isPending}
            variant="outline"
            className="w-full"
          >
            {assignField.isPending ? 'მინიჭება...' : 'მინიჭება'}
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

function DataContent() {
  const enrollMut = useImportEnrollments()
  const priorMut = useImportPriorities()
  const handbookMut = useImportHandbook()

  const [enrollResult, setEnrollResult] = useState<ImportResult | null>(null)
  const [priorResult, setPriorResult] = useState<ImportResult | null>(null)
  const [handbookResult, setHandbookResult] = useState<ImportResult | null>(null)

  function handleEnroll(file: File, year: number) {
    enrollMut.mutate(
      { file, year },
      {
        onSuccess: (r) => { setEnrollResult(r); toast.success('ჩარიცხვები შემოტანილია') },
        onError: (e) => toast.error(errorText(e, 'შეცდომა ჩარიცხვების შემოტანისას')),
      }
    )
  }

  function handlePriorities(file: File, year: number) {
    priorMut.mutate(
      { file, year },
      {
        onSuccess: (r) => { setPriorResult(r); toast.success('პრიორიტეტები შემოტანილია') },
        onError: (e) => toast.error(errorText(e, 'შეცდომა პრიორიტეტების შემოტანისას')),
      }
    )
  }

  function handleHandbook(file: File, year: number) {
    handbookMut.mutate(
      { file, year },
      {
        onSuccess: (r) => { setHandbookResult(r); toast.success('ცნობარი შემოტანილია') },
        onError: (e) => toast.error(errorText(e, 'შეცდომა ცნობარის შემოტანისას')),
      }
    )
  }

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-semibold">მონაცემთა მართვა</h1>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <ImportSection
          title="ჩარიცხულთა სია"
          description="Excel ფაილი ჩარიცხულების ჩამონათვალით (.xlsx)"
          icon={<FileSpreadsheet className="h-4 w-4" />}
          accept=".xlsx"
          onImport={handleEnroll}
          isPending={enrollMut.isPending}
          result={enrollResult}
        />

        <ImportSection
          title="პრიორიტეტების არჩევანი"
          description="Excel ფაილი პრიორიტეტების მიხედვით (.xlsx)"
          icon={<FileSpreadsheet className="h-4 w-4" />}
          accept=".xlsx"
          onImport={handlePriorities}
          isPending={priorMut.isPending}
          result={priorResult}
        />

        <ImportSection
          title="ცნობარი"
          description="PDF ფაილი — აბიტურიენტებისათვის ცნობარი (.pdf)"
          icon={<FileText className="h-4 w-4" />}
          accept=".pdf"
          onImport={handleHandbook}
          isPending={handbookMut.isPending}
          result={handbookResult}
        />
      </div>

      <EditYearStatsSection />

      <ReferenceDataSection />
    </div>
  )
}

export function DataPage() {
  return (
    <ErrorBoundary>
      <DataContent />
    </ErrorBoundary>
  )
}
