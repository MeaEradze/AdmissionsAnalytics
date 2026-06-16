import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useUniversities } from '@/hooks/useUniversities'

interface UniversitySelectorProps {
  value?: number
  onChange: (id: number) => void
  placeholder?: string
}

export function UniversitySelector({ value, onChange, placeholder = 'უნივერსიტეტი' }: UniversitySelectorProps) {
  const { data: universities = [] } = useUniversities()

  return (
    <Select
      value={value !== undefined ? String(value) : ''}
      onValueChange={(v) => onChange(Number(v))}
    >
      <SelectTrigger className="w-[340px]">
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent>
        {universities.map((u) => (
          <SelectItem key={u.id} value={String(u.id)}>
            {u.shortName ?? u.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
