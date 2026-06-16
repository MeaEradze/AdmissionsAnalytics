import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useFields } from '@/hooks/useFields'

interface FieldSelectorProps {
  value?: number
  onChange: (id: number) => void
  placeholder?: string
}

export function FieldSelector({ value, onChange, placeholder = 'სფერო' }: FieldSelectorProps) {
  const { data: fields = [] } = useFields()

  return (
    <Select
      value={value !== undefined ? String(value) : ''}
      onValueChange={(v) => onChange(Number(v))}
    >
      <SelectTrigger className="w-full">
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent>
        {fields.map((f) => (
          <SelectItem key={f.id} value={String(f.id)}>
            {f.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
