import { http, HttpResponse } from 'msw'
import type { Feedback, TokenResponse, UserProfile } from '../types/api'
import { DEFAULT_STORE_ID } from '../types/api'
import { buildSummary, getFeedbackItems, mockCustomer, mockManager, setFeedbackItems } from './data'

let currentUser: UserProfile | null = null

function problem(status: number, title: string, detail: string) {
  return HttpResponse.json({ title, detail, status }, { status })
}

export const handlers = [
  http.post('/api/identity/register', async ({ request }) => {
    const body = (await request.json()) as { email: string; displayName: string }
    const user: UserProfile = {
      id: crypto.randomUUID(),
      email: body.email,
      displayName: body.displayName,
      roles: ['Customer'],
    }
    return HttpResponse.json(user, { status: 201 })
  }),

  http.post('/api/identity/login', async ({ request }) => {
    const body = (await request.json()) as { email: string; password: string }
    const isManager = body.email === 'manager@store.local'
    const user = isManager ? mockManager : { ...mockCustomer, email: body.email }
    currentUser = user
    const tokens: TokenResponse = {
      accessToken: `mock-token-${user.roles[0]}`,
      refreshToken: 'mock-refresh',
      expiresIn: 900,
      user,
    }
    return HttpResponse.json(tokens)
  }),

  http.get('/api/identity/me', () => {
    if (!currentUser) {
      return problem(401, 'Unauthorized', 'Authentication is required.')
    }
    return HttpResponse.json(currentUser)
  }),

  http.post('/api/feedback', async ({ request }) => {
    if (!currentUser?.roles.includes('Customer')) {
      return problem(403, 'Forbidden', 'Customers only.')
    }
    const body = (await request.json()) as Record<string, unknown>
    const now = new Date().toISOString()
    const item: Feedback = {
      id: crypto.randomUUID(),
      storeId: String(body.storeId ?? DEFAULT_STORE_ID),
      customerId: body.isAnonymous ? null : currentUser.id,
      visitDate: String(body.visitDate),
      overall: Number(body.overall),
      cleanliness: Number(body.cleanliness),
      staff: Number(body.staff),
      waitTime: Number(body.waitTime),
      productAvailability: Number(body.productAvailability),
      npsScore: Number(body.npsScore),
      category: body.category as Feedback['category'],
      comments: (body.comments as string) ?? null,
      contactEmail: body.isAnonymous ? null : (body.contactEmail as string) ?? null,
      isAnonymous: Boolean(body.isAnonymous),
      status: 'New',
      createdAt: now,
      updatedAt: now,
    }
    setFeedbackItems([item, ...getFeedbackItems()])
    return HttpResponse.json(item, { status: 201 })
  }),

  http.get('/api/feedback/mine', () => {
    if (!currentUser?.roles.includes('Customer')) {
      return problem(403, 'Forbidden', 'Customers only.')
    }
    const items = getFeedbackItems().filter((x) => x.customerId === currentUser!.id)
    return HttpResponse.json({ items, page: 1, pageSize: 20, totalCount: items.length })
  }),

  http.get('/api/feedback', () => {
    if (!currentUser?.roles.includes('StoreManager')) {
      return problem(403, 'Forbidden', 'Managers only.')
    }
    return HttpResponse.json({
      items: getFeedbackItems(),
      page: 1,
      pageSize: 20,
      totalCount: getFeedbackItems().length,
    })
  }),

  http.patch('/api/feedback/:id/category', async ({ params, request }) => {
    const body = (await request.json()) as { category: string }
    setFeedbackItems(getFeedbackItems().map((x) =>
      x.id === params.id
        ? { ...x, category: body.category as Feedback['category'], updatedAt: new Date().toISOString() }
        : x,
    ))
    const item = getFeedbackItems().find((x) => x.id === params.id)
    return item ? HttpResponse.json(item) : problem(404, 'Not found', 'Feedback was not found.')
  }),

  http.patch('/api/feedback/:id/status', async ({ params, request }) => {
    const body = (await request.json()) as { status: string }
    setFeedbackItems(getFeedbackItems().map((x) =>
      x.id === params.id
        ? { ...x, status: body.status as Feedback['status'], updatedAt: new Date().toISOString() }
        : x,
    ))
    const item = getFeedbackItems().find((x) => x.id === params.id)
    return item ? HttpResponse.json(item) : problem(404, 'Not found', 'Feedback was not found.')
  }),

  http.get('/api/reports/summary', () => {
    if (!currentUser?.roles.includes('StoreManager')) {
      return problem(403, 'Forbidden', 'Managers only.')
    }
    return HttpResponse.json(buildSummary(getFeedbackItems()))
  }),
]
