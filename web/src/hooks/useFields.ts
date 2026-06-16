import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { CreateFieldRequest, UpdateFieldRequest } from '@/lib/types'

export function useFields() {
  return useQuery({
    queryKey: ['fields'],
    queryFn: () => api.fields.list(),
  })
}

export function useCreateField() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateFieldRequest) => api.fields.create(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['fields'] }),
  })
}

export function useUpdateField() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: number; body: UpdateFieldRequest }) =>
      api.fields.update(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['fields'] }),
  })
}

export function useFieldCompetition(fieldId: number | undefined, year: number) {
  return useQuery({
    queryKey: ['field', fieldId, 'competition', year],
    queryFn: () => api.competition.field(fieldId!, year),
    enabled: fieldId !== undefined,
  })
}

export function useFieldTrend(fieldId: number | undefined) {
  return useQuery({
    queryKey: ['field', fieldId, 'trend'],
    queryFn: () => api.trend.field(fieldId!),
    enabled: fieldId !== undefined,
  })
}
