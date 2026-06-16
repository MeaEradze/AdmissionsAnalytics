import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useMarketGaps(year: number) {
  return useQuery({
    queryKey: ['market', 'gaps', year],
    queryFn: () => api.market.gaps(year),
  })
}

export function useMarketOverview(year: number) {
  return useQuery({
    queryKey: ['market', 'overview', year],
    queryFn: () => api.market.overview(year),
  })
}
