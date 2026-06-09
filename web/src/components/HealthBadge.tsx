import { Badge } from '@/components/ui/badge'
import type { HealthCategory } from '@/lib/types'

interface HealthBadgeProps {
  category: HealthCategory
}

const LABEL: Record<HealthCategory, string> = {
  Growing: 'მზარდი',
  Stable: 'სტაბილური',
  Risky: 'რისკიანი',
}

const CLASS: Record<HealthCategory, string> = {
  Growing: 'bg-emerald-100 text-emerald-800 border-emerald-200',
  Stable: 'bg-blue-100 text-blue-700 border-blue-200',
  Risky: 'bg-amber-100 text-amber-800 border-amber-200',
}

export function HealthBadge({ category }: HealthBadgeProps) {
  return (
    <Badge variant="outline" className={CLASS[category]}>
      {LABEL[category]}
    </Badge>
  )
}
