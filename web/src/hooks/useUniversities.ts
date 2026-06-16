import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { CreateUniversityRequest } from '@/lib/types'

export function useUniversities() {
  return useQuery({
    queryKey: ['universities'],
    queryFn: () => api.universities.list(),
  })
}

export function useCreateUniversity() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateUniversityRequest) => api.universities.create(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['universities'] }),
  })
}

export function useUniversityTrend(id: number | undefined) {
  return useQuery({
    queryKey: ['university', id, 'trend'],
    queryFn: () => api.trend.university(id!),
    enabled: id !== undefined,
  })
}

export function useUniversityPortfolio(id: number | undefined, year: number) {
  return useQuery({
    queryKey: ['university', id, 'portfolio', year],
    queryFn: () => api.portfolioItems.list(id!, year),
    enabled: id !== undefined,
  })
}
