import { useEffect, useState } from 'react'

import { Input } from '@/components/ui/input'

interface YearSelectorProps {
  value: number
  onChange: (year: number) => void
}

const MIN_YEAR = 1900
const MAX_YEAR = 2100

export function YearSelector({ value, onChange }: YearSelectorProps) {
  const [text, setText] = useState(String(value))

  // Keep the input in sync when the year is changed elsewhere (e.g. default year loads).
  useEffect(() => {
    setText(String(value))
  }, [value])

  const commit = (raw: string) => {
    const year = Number(raw)
    if (Number.isInteger(year) && year >= MIN_YEAR && year <= MAX_YEAR) {
      onChange(year)
    } else {
      // Invalid entry: revert the field back to the current valid year.
      setText(String(value))
    }
  }

  return (
    <Input
      type="number"
      inputMode="numeric"
      min={MIN_YEAR}
      max={MAX_YEAR}
      className="w-[120px]"
      placeholder="წელი"
      value={text}
      onChange={(e) => {
        setText(e.target.value)
        // Commit immediately once a full, plausible year is typed.
        const year = Number(e.target.value)
        if (Number.isInteger(year) && year >= MIN_YEAR && year <= MAX_YEAR) {
          onChange(year)
        }
      }}
      onBlur={(e) => commit(e.target.value)}
    />
  )
}
