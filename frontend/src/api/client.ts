import type { RowSummary, SaveResponse, SettingChange, SettingsCatalog, SettingsResponse } from './types'

/** Error carrying the API's ProblemDetails (title, detail and per-field errors). */
export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors: Record<string, string[]>

  constructor(message: string, status: number, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

const base = import.meta.env.VITE_API_BASE ?? ''

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(base + path, {
      ...init,
      headers: { 'Content-Type': 'application/json', ...init?.headers },
    })
  } catch {
    throw new ApiError('Sunucuya ulaşılamadı. API çalışıyor mu?', 0)
  }

  if (response.ok) return (await response.json()) as T

  let problem: { title?: string; detail?: string; errors?: Record<string, string[]> } = {}
  try {
    problem = await response.json()
  } catch {
    /* body is not JSON */
  }
  const message = [problem.title, problem.detail].filter(Boolean).join(' – ') || `İstek başarısız (${response.status})`
  throw new ApiError(message, response.status, problem.errors ?? {})
}

export const api = {
  catalog: () => request<SettingsCatalog>('/api/settings/catalog'),
  rows: () => request<RowSummary[]>('/api/settings/rows'),
  get: (row: number) => request<SettingsResponse>(`/api/settings?row=${row}`),
  save: (row: number, changes: SettingChange[]) =>
    request<SaveResponse>(`/api/settings?row=${row}`, {
      method: 'PUT',
      body: JSON.stringify({ changes }),
    }),
}
