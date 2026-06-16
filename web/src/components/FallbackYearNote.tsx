import { Info } from 'lucide-react'

interface FallbackYearNoteProps {

  shownYear: number | undefined

  requestedYear: number
}

export function FallbackYearNote({ shownYear, requestedYear }: FallbackYearNoteProps) {
  if (shownYear === undefined || shownYear === requestedYear) return null

  return (
    <div className="flex items-center gap-2 rounded-md border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-800">
      <Info className="h-4 w-4 shrink-0" />
      <span>
        {requestedYear} წლის მონაცემები არ არის — ნაჩვენებია {shownYear} წლის მონაცემები.
      </span>
    </div>
  )
}
