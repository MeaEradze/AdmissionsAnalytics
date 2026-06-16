import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type {
  HealthListParams,
  ProgramListParams,
  UpdateProgramYearRequest,
} from '@/lib/types'

export function usePrograms(params: ProgramListParams = {}) {
  return useQuery({
    queryKey: ['programs', params],
    queryFn: () => api.programs.list(params),
  })
}

export function useProgram(id: number | undefined) {
  return useQuery({
    queryKey: ['program', id],
    queryFn: () => api.programs.get(id!),
    enabled: id !== undefined,
  })
}

export function useUpdateProgramYearStats() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      id,
      year,
      body,
    }: {
      id: number
      year: number
      body: UpdateProgramYearRequest
    }) => api.programs.updateYearStats(id, year, body),

    onSuccess: () => qc.invalidateQueries(),
  })
}

export function useAssignProgramField() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, fieldId }: { id: number; fieldId: number }) =>
      api.programs.assignField(id, fieldId),

    onSuccess: () => qc.invalidateQueries(),
  })
}

export function useProgramHealth(id: number | undefined, year: number) {
  return useQuery({
    queryKey: ['program', id, 'health', year],
    queryFn: () => api.health.program(id!, year),
    enabled: id !== undefined,
  })
}

export function useHealthList(params: HealthListParams = {}) {
  return useQuery({
    queryKey: ['programs', 'health', params],
    queryFn: () => api.health.list(params),
  })
}

export function useProgramForecast(id: number | undefined) {
  return useQuery({
    queryKey: ['program', id, 'forecast'],
    queryFn: () => api.forecast.program(id!),
    enabled: id !== undefined,
  })
}

export function useProgramTrend(id: number | undefined) {
  return useQuery({
    queryKey: ['program', id, 'trend'],
    queryFn: () => api.trend.program(id!),
    enabled: id !== undefined,
  })
}

export function useProgramBenchmark(id: number | undefined, year: number) {
  return useQuery({
    queryKey: ['program', id, 'benchmark', year],
    queryFn: () => api.benchmark.program(id!, year),
    enabled: id !== undefined,
  })
}

export function usePriorityDistribution(id: number | undefined, year: number) {
  return useQuery({
    queryKey: ['program', id, 'priority-distribution', year],
    queryFn: () => api.priority.distribution(id!, year),
    enabled: id !== undefined,
  })
}

export function useProgramConversion(id: number | undefined) {
  return useQuery({
    queryKey: ['program', id, 'conversion'],
    queryFn: () => api.conversion.program(id!),
    enabled: id !== undefined,
  })
}

export function useFeeSensitivity(id: number | undefined) {
  return useQuery({
    queryKey: ['program', id, 'fee-sensitivity'],
    queryFn: () => api.feeSensitivity.program(id!),
    enabled: id !== undefined,
  })
}

export function useProgramCompetition(id: number | undefined, fromYear: number, toYear: number) {
  return useQuery({
    queryKey: ['program', id, 'competition', fromYear, toYear],
    queryFn: () => api.competition.program(id!, fromYear, toYear),
    enabled: id !== undefined,
  })
}

export function useProgramComparison(ids: number[], year: number) {
  return useQuery({
    queryKey: ['programs', 'compare', ids, year],
    queryFn: () => api.compare.programs(ids, year),
    enabled: ids.length > 0,
  })
}
