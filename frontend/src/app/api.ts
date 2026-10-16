import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type { Feedback, PagedResult, ReportSummary, TokenResponse, UserProfile } from '../types/api'
import type { RootState } from './store'

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export type RegisterBody = { email: string; password: string; displayName: string }
export type LoginBody = { email: string; password: string }

export type CreateFeedbackBody = {
  storeId: string
  visitDate: string
  overall: number
  cleanliness: number
  staff: number
  waitTime: number
  productAvailability: number
  npsScore: number
  category: string
  comments?: string
  contactEmail?: string
  isAnonymous: boolean
}

export const api = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({
    baseUrl,
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.accessToken
      if (token) {
        headers.set('Authorization', `Bearer ${token}`)
      }
      return headers
    },
  }),
  tagTypes: ['Feedback', 'Report'],
  endpoints: (builder) => ({
    register: builder.mutation<UserProfile, RegisterBody>({
      query: (body) => ({ url: '/api/identity/register', method: 'POST', body }),
    }),
    login: builder.mutation<TokenResponse, LoginBody>({
      query: (body) => ({ url: '/api/identity/login', method: 'POST', body }),
    }),
    me: builder.query<UserProfile, void>({
      query: () => '/api/identity/me',
    }),
    createFeedback: builder.mutation<Feedback, CreateFeedbackBody>({
      query: (body) => ({ url: '/api/feedback', method: 'POST', body }),
      invalidatesTags: ['Feedback', 'Report'],
    }),
    myFeedback: builder.query<PagedResult<Feedback>, { page?: number }>({
      query: ({ page = 1 }) => `/api/feedback/mine?page=${page}`,
      providesTags: ['Feedback'],
    }),
    managerFeedback: builder.query<
      PagedResult<Feedback>,
      { page?: number; category?: string; status?: string }
    >({
      query: ({ page = 1, category, status }) => {
        const params = new URLSearchParams({ page: String(page) })
        if (category) params.set('category', category)
        if (status) params.set('status', status)
        return `/api/feedback?${params}`
      },
      providesTags: ['Feedback'],
    }),
    patchCategory: builder.mutation<Feedback, { id: string; category: string }>({
      query: ({ id, category }) => ({
        url: `/api/feedback/${id}/category`,
        method: 'PATCH',
        body: { category },
      }),
      invalidatesTags: ['Feedback', 'Report'],
    }),
    patchStatus: builder.mutation<Feedback, { id: string; status: string }>({
      query: ({ id, status }) => ({
        url: `/api/feedback/${id}/status`,
        method: 'PATCH',
        body: { status },
      }),
      invalidatesTags: ['Feedback', 'Report'],
    }),
    reportSummary: builder.query<ReportSummary, { storeId?: string }>({
      query: ({ storeId }) => {
        const params = storeId ? `?storeId=${storeId}` : ''
        return `/api/reports/summary${params}`
      },
      providesTags: ['Report'],
    }),
  }),
})

export const {
  useRegisterMutation,
  useLoginMutation,
  useMeQuery,
  useCreateFeedbackMutation,
  useMyFeedbackQuery,
  useManagerFeedbackQuery,
  usePatchCategoryMutation,
  usePatchStatusMutation,
  useReportSummaryQuery,
} = api
