import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { CircleHelp } from 'lucide-react'

interface StatCardProps {
  icon: React.ReactNode
  label: string
  value: string | number
  description?: string

  sub?: React.ReactNode

  iconColor?: string

  layout?: 'vertical' | 'horizontal'
}

export function StatCard({
  icon,
  label,
  value,
  description,
  sub,
  iconColor = 'text-primary',
  layout = 'vertical',
}: StatCardProps) {
  if (layout === 'horizontal') {
    return (
      <Card className="py-0">
        <CardContent className="flex items-center gap-3 p-4">
          <span
            className={`flex size-11 shrink-0 items-center justify-center rounded-lg bg-primary/10 [&_svg]:size-5 ${iconColor}`}
          >
            {icon}
          </span>
          <div className="min-w-0 flex-1">
            <p className="text-xs font-medium text-muted-foreground leading-tight truncate">
              {label}
            </p>
            <p className="text-2xl font-bold tracking-tight tabular-nums leading-tight">{value}</p>
            {sub && <p className="mt-0.5 text-xs text-muted-foreground leading-snug">{sub}</p>}
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card className="relative gap-0 py-4">
      <CardHeader className="px-4 pb-1.5">
        <div className="flex items-start justify-between gap-2">
          <div className="flex items-center gap-2 min-w-0">
            <span className={`shrink-0 ${iconColor}`}>{icon}</span>
            <CardTitle className="text-xs font-medium text-muted-foreground leading-tight">
              {label}
            </CardTitle>
          </div>
          {description && (
            <Popover>
              <PopoverTrigger asChild>
                <button
                  className="shrink-0 text-muted-foreground/60 hover:text-muted-foreground transition-colors mt-0.5"
                  aria-label="დამატებითი ინფორმაცია"
                >
                  <CircleHelp className="h-3.5 w-3.5" />
                </button>
              </PopoverTrigger>
              <PopoverContent className="w-64 text-sm" side="top" align="end">
                <p className="text-muted-foreground leading-relaxed">{description}</p>
              </PopoverContent>
            </Popover>
          )}
        </div>
      </CardHeader>
      <CardContent className="px-4">
        <p className="text-3xl font-bold tracking-tight tabular-nums">{value}</p>
        {sub && <p className="mt-1 text-xs text-muted-foreground leading-snug">{sub}</p>}
      </CardContent>
    </Card>
  )
}
