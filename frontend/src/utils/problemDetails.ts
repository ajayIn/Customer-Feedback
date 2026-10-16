import type { ProblemDetails } from '../types/api'

export type ParsedProblem = {
  summary: string
  fieldErrors: Record<string, string>
}

function normalizeFieldKey(key: string): string {
  if (!key) return key
  return key.charAt(0).toLowerCase() + key.slice(1)
}

/** Maps RTK Query / fetch errors with RFC 7807 body to user-facing messages. */
export function parseProblemDetails(err: unknown, fallbackSummary: string): ParsedProblem {
  const data = (err as { data?: ProblemDetails })?.data
  if (!data) {
    return { summary: fallbackSummary, fieldErrors: {} }
  }

  const fieldErrors: Record<string, string> = {}
  if (data.errors) {
    for (const [key, messages] of Object.entries(data.errors)) {
      const normalized = normalizeFieldKey(key)
      fieldErrors[normalized] = messages.filter(Boolean).join(' ')
    }
  }

  const summary =
    data.detail?.trim() ||
    data.title?.trim() ||
    (Object.keys(fieldErrors).length > 0 ? 'Please fix the highlighted fields.' : fallbackSummary)

  return { summary, fieldErrors }
}
