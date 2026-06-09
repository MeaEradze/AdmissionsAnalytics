import { AlertTriangle, RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'

interface QueryErrorProps {

  error?: unknown
  onRetry?: () => void
}

export function QueryError({ error, onRetry }: QueryErrorProps) {
  const detail = error instanceof Error && error.message ? error.message : null

  return (
    <Card className="border-destructive/30">
      <CardContent className="flex flex-col items-center justify-center gap-3 py-10 text-center">
        <div className="rounded-full bg-destructive/10 p-3">
          <AlertTriangle className="h-6 w-6 text-destructive" />
        </div>
        <p className="text-base font-medium">მონაცემების ჩატვირთვა ვერ მოხერხდა</p>
        {detail && (
          <p className="max-w-md text-sm text-muted-foreground break-words">{detail}</p>
        )}
        <p className="text-xs text-muted-foreground">
          შეამოწმეთ, რომ სერვერი გაშვებულია, და სცადეთ ხელახლა.
        </p>
        {onRetry && (
          <Button variant="outline" size="sm" onClick={onRetry}>
            <RotateCcw className="mr-1.5 h-3.5 w-3.5" />
            ხელახლა ცდა
          </Button>
        )}
      </CardContent>
    </Card>
  )
}
