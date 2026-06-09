import { useState } from 'react'
import { useProgram, usePrograms } from '@/hooks/usePrograms'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import { Input } from '@/components/ui/input'

interface ProgramSelectorProps {
  value?: number
  onChange: (id: number) => void
  placeholder?: string
}

export function ProgramSelector({ value, onChange, placeholder = 'პროგრამის ძიება...' }: ProgramSelectorProps) {
  const [search, setSearch] = useState('')
  const [open, setOpen] = useState(false)

  const [pickedName, setPickedName] = useState<string | null>(null)

  const term = useDebouncedValue(search, 300).trim()
  const { data, isFetching } = usePrograms({ search: term || undefined, pageSize: 20 })
  const programs = data?.data ?? []

  const { data: selectedDetail } = useProgram(
    value !== undefined && pickedName === null ? value : undefined,
  )
  const selectedLabel =
    pickedName ?? programs.find((p) => p.id === value)?.name ?? selectedDetail?.name ?? ''

  return (
    <div className="relative w-full">
      <Input
        value={open ? search : selectedLabel}
        onChange={(e) => {
          setSearch(e.target.value)
          setOpen(true)
        }}
        onFocus={() => setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 200)}
        placeholder={placeholder}
        className="w-full"
      />
      {open && (
        <div className="absolute z-50 mt-1 w-full max-h-60 overflow-y-auto rounded-md border bg-popover shadow-md">
          {programs.map((p) => (
            <div
              key={p.id}
              className="cursor-pointer px-3 py-2 text-sm hover:bg-accent hover:text-accent-foreground"
              onMouseDown={() => {
                onChange(p.id)
                setPickedName(p.name)
                setSearch('')
                setOpen(false)
              }}
            >
              <div className="font-medium truncate">{p.name}</div>
              <div className="text-xs text-muted-foreground truncate">{p.universityName}</div>
            </div>
          ))}
          {programs.length === 0 && (
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
  )
}
