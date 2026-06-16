import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'

export function useYears() {
  return useQuery({
    queryKey: ['meta', 'years'],
    queryFn: () => api.meta.years(),
  })
}

const FALLBACK_YEAR = 2025

export function useDefaultYear(): [number, (year: number) => void] {
  const { data: years } = useYears()
  const [picked, setPicked] = useState<number | null>(null)
  const year = picked ?? years?.[0] ?? FALLBACK_YEAR
  return [year, setPicked]
}
