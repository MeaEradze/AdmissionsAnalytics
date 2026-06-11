import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useDashboardSummary(year: number) {
  return useQuery({
    queryKey: ['dashboard', 'summary', year],
    queryFn: () => api.dashboard.summary(year),
  })
}
