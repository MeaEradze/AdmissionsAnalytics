import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useImportEnrollments() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ file, year }: { file: File; year: number }) =>
      api.import.enrollments(file, year),
    onSuccess: () => qc.invalidateQueries(),
  })
}

export function useImportPriorities() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ file, year }: { file: File; year: number }) =>
      api.import.priorities(file, year),
    onSuccess: () => qc.invalidateQueries(),
  })
}

export function useImportHandbook() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ file, year }: { file: File; year: number }) =>
      api.import.handbook(file, year),
    onSuccess: () => qc.invalidateQueries(),
  })
}
