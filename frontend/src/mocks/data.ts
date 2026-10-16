import type { Feedback, ReportSummary, UserProfile } from '../types/api'
import { DEFAULT_STORE_ID } from '../types/api'

export const mockManager: UserProfile = {
  id: '22222222-2222-2222-2222-222222222222',
  email: 'manager@store.local',
  displayName: 'Store Manager',
  roles: ['StoreManager'],
}

export const mockCustomer: UserProfile = {
  id: '33333333-3333-3333-3333-333333333333',
  email: 'customer@example.com',
  displayName: 'Demo Customer',
  roles: ['Customer'],
}

const now = new Date().toISOString()
const visit = new Date().toISOString().slice(0, 10)

export const mockState = {
  feedbackItems: [] as Feedback[],
}

mockState.feedbackItems = [
  {
    id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    storeId: DEFAULT_STORE_ID,
    customerId: mockCustomer.id,
    visitDate: visit,
    overall: 5,
    cleanliness: 4,
    staff: 5,
    waitTime: 3,
    productAvailability: 4,
    npsScore: 9,
    category: 'Service',
    comments: 'Friendly staff',
    contactEmail: 'customer@example.com',
    isAnonymous: false,
    status: 'New',
    createdAt: now,
    updatedAt: now,
  },
]

export function getFeedbackItems() {
  return mockState.feedbackItems
}

export function setFeedbackItems(items: Feedback[]) {
  mockState.feedbackItems = items
}

export function buildSummary(items: Feedback[]): ReportSummary {
  const total = items.length
  const averageOverall = total ? items.reduce((s, x) => s + x.overall, 0) / total : 0
  const averageNps = total ? items.reduce((s, x) => s + x.npsScore, 0) / total : 0
  const categories = ['Service', 'Product', 'Facilities', 'Other'] as const
  return {
    totalCount: total,
    averageOverall,
    averageNps,
    byCategory: categories.map((category) => {
      const group = items.filter((x) => x.category === category)
      return {
        category,
        count: group.length,
        averageOverall: group.length ? group.reduce((s, x) => s + x.overall, 0) / group.length : 0,
      }
    }),
    trend: [
      {
        period: visit,
        count: total,
        averageOverall,
        averageNps,
      },
    ],
    statusCounts: {
      new: items.filter((x) => x.status === 'New').length,
      reviewed: items.filter((x) => x.status === 'Reviewed').length,
      actioned: items.filter((x) => x.status === 'Actioned').length,
    },
  }
}
