export type Role = 'Customer' | 'StoreManager'

export type UserProfile = {
  id: string
  email: string
  displayName: string
  roles: Role[]
}

export type TokenResponse = {
  accessToken: string
  refreshToken: string
  expiresIn: number
  user: UserProfile
}

export type FeedbackCategory = 'Service' | 'Product' | 'Facilities' | 'Other'
export type FeedbackStatus = 'New' | 'Reviewed' | 'Actioned'

export type Feedback = {
  id: string
  storeId: string
  customerId: string | null
  visitDate: string
  overall: number
  cleanliness: number
  staff: number
  waitTime: number
  productAvailability: number
  npsScore: number
  category: FeedbackCategory
  comments: string | null
  contactEmail: string | null
  isAnonymous: boolean
  status: FeedbackStatus
  createdAt: string
  updatedAt: string
}

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export type ReportSummary = {
  totalCount: number
  averageOverall: number
  averageNps: number
  byCategory: { category: FeedbackCategory; count: number; averageOverall: number }[]
  trend: { period: string; count: number; averageOverall: number; averageNps: number }[]
  statusCounts: { new: number; reviewed: number; actioned: number }
}

export type ProblemDetails = {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

export const DEFAULT_STORE_ID = '11111111-1111-1111-1111-111111111111'
