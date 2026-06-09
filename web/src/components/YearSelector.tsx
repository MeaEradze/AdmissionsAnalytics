import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useYears } from '@/hooks/useMeta'

interface YearSelectorProps {
  value: number
  onChange: (year: number) => void
}

const FALLBACK_YEARS = [2025, 2024, 2023]

export function YearSelector({ value, onChange }: YearSelectorProps) {
  const { data } = useYears()
  const years = data && data.length > 0 ? data : FALLBACK_YEARS

  const options = years.includes(value) ? years : [value, ...years]

  return (
    <Select value={String(value)} onValueChange={(v) => onChange(Number(v))}>
      <SelectTrigger className="w-[120px]">
        <SelectValue placeholder="წელი" />
      </SelectTrigger>
      <SelectContent>
        {options.map((y) => (
          <SelectItem key={y} value={String(y)}>
            {y} წ.
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
