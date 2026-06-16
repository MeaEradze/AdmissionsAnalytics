import type {
  CreateFieldRequest,
  CreateUniversityRequest,
  DashboardSummary,
  FeeSensitivity,
  Field,
  FieldCompetition,
  FieldGap,
  FieldTrend,
  HealthListParams,
  HealthListResponse,
  ImportResult,
  MarketOverview,
  PagedResponse,
  PortfolioItem,
  PriorityDistribution,
  ProgramBenchmark,
  ProgramCompetitionTrend,
  ProgramConversion,
  ProgramDetail,
  ProgramForecast,
  ProgramHealth,
  ProgramComparisonItem,
  ProgramListItem,
  ProgramListParams,
  ProgramTrend,
  University,
  UniversityTrend,
  UpdateFieldRequest,
  UpdateProgramYearRequest,
} from './types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5114'

class ApiError extends Error {
  status: number
  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

function problemMessage(raw: string, fallback: string): string {
  if (!raw) return fallback
  try {
    const problem = JSON.parse(raw) as { detail?: string; title?: string }
    return problem.detail ?? problem.title ?? raw
  } catch {
    return raw
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${BASE_URL}${path}`

  const headers = new Headers(init?.headers)
  if (!headers.has('Content-Type') && !(init?.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }
  const res = await fetch(url, { ...init, headers })
  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new ApiError(res.status, problemMessage(text, res.statusText))
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

function buildQuery(params: Record<string, string | number | boolean | undefined>): string {
  const entries = Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== '')
  if (!entries.length) return ''
  return '?' + entries.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`).join('&')
}

const _api = {
  universities: {
    list: () => request<University[]>('/api/universities'),
    create: (body: CreateUniversityRequest) =>
      request<University>('/api/universities', { method: 'POST', body: JSON.stringify(body) }),
  },

  fields: {
    list: () => request<Field[]>('/api/fields'),
    create: (body: CreateFieldRequest) =>
      request<Field>('/api/fields', { method: 'POST', body: JSON.stringify(body) }),
    update: (id: number, body: UpdateFieldRequest) =>
      request<Field>(`/api/fields/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  },

  programs: {
    list: (params: ProgramListParams = {}) =>
      request<PagedResponse<ProgramListItem>>(`/api/programs${buildQuery(params as Record<string, string | number | boolean | undefined>)}`),
    get: (id: number) => request<ProgramDetail>(`/api/programs/${id}`),
    updateYearStats: (id: number, year: number, body: UpdateProgramYearRequest) =>
      request<void>(`/api/programs/${id}/year-stats/${year}`, {
        method: 'PUT',
        body: JSON.stringify(body),
      }),
    assignField: (id: number, fieldId: number) =>
      request<void>(`/api/programs/${id}/field`, {
        method: 'PUT',
        body: JSON.stringify({ fieldId }),
      }),
  },

  competition: {
    field: (fieldId: number, year: number) =>
      request<FieldCompetition>(`/api/fields/${fieldId}/competition${buildQuery({ year })}`),
    program: (id: number, fromYear: number, toYear: number) =>
      request<ProgramCompetitionTrend>(
        `/api/programs/${id}/competition${buildQuery({ fromYear, toYear })}`,
      ),
  },

  health: {
    program: (id: number, year: number) =>
      request<ProgramHealth>(`/api/programs/${id}/health${buildQuery({ year })}`),
    list: (params: HealthListParams = {}) =>
      request<HealthListResponse>(`/api/programs/health${buildQuery(params as Record<string, string | number | boolean | undefined>)}`),
  },

  forecast: {
    program: (id: number) => request<ProgramForecast>(`/api/programs/${id}/forecast`),
  },

  trend: {
    program: (id: number) => request<ProgramTrend>(`/api/programs/${id}/trend`),
    field: (fieldId: number) => request<FieldTrend>(`/api/fields/${fieldId}/trend`),
    university: (id: number) => request<UniversityTrend>(`/api/universities/${id}/trend`),
  },

  benchmark: {
    program: (id: number, year: number) =>
      request<ProgramBenchmark>(`/api/programs/${id}/benchmark${buildQuery({ year })}`),
  },

  market: {
    gaps: (year: number) => request<FieldGap[]>(`/api/market/gaps${buildQuery({ year })}`),
    overview: (year: number) => request<MarketOverview>(`/api/market/overview${buildQuery({ year })}`),
  },

  priority: {
    distribution: (id: number, year: number) =>
      request<PriorityDistribution>(
        `/api/programs/${id}/priority-distribution${buildQuery({ year })}`,
      ),
  },

  conversion: {
    program: (id: number) => request<ProgramConversion>(`/api/programs/${id}/conversion`),
  },

  feeSensitivity: {
    program: (id: number) => request<FeeSensitivity>(`/api/programs/${id}/fee-sensitivity`),
  },

  compare: {
    programs: (ids: number[], year: number) =>
      request<ProgramComparisonItem[]>(
        `/api/programs/compare${buildQuery({ ids: ids.join(','), year })}`,
      ),
  },

  dashboard: {
    summary: (year: number) => request<DashboardSummary>(`/api/dashboard/summary${buildQuery({ year })}`),
  },

  portfolioItems: {
    list: (universityId: number, year: number) =>
      request<PortfolioItem[]>(`/api/universities/${universityId}/portfolio${buildQuery({ year })}`),
  },

  import: {
    enrollments: (file: File, year: number) => {
      const fd = new FormData()
      fd.append('file', file)
      return request<ImportResult>(`/api/import/enrollments${buildQuery({ year })}`, {
        method: 'POST',
        body: fd,
      })
    },
    priorities: (file: File, year: number) => {
      const fd = new FormData()
      fd.append('file', file)
      return request<ImportResult>(`/api/import/priorities${buildQuery({ year })}`, {
        method: 'POST',
        body: fd,
      })
    },
    handbook: (file: File, year: number) => {
      const fd = new FormData()
      fd.append('file', file)
      return request<ImportResult>(`/api/import/handbook${buildQuery({ year })}`, {
        method: 'POST',
        body: fd,
      })
    },
  },

  meta: {
    years: () => request<number[]>('/api/meta/years'),
  },
}

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true'

export const api: typeof _api = USE_MOCK ? (await import('./mockApi')).mockApi : _api

export { ApiError }
