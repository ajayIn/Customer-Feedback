import { createSlice, type PayloadAction } from '@reduxjs/toolkit'
import type { UserProfile } from '../types/api'

const REFRESH_KEY = 'cf_refresh_token'

type AuthState = {
  accessToken: string | null
  user: UserProfile | null
}

const loadRefresh = () => localStorage.getItem(REFRESH_KEY)

const initialState: AuthState = {
  accessToken: null,
  user: null,
}

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setSession(
      state,
      action: PayloadAction<{ accessToken: string; refreshToken: string; user: UserProfile }>,
    ) {
      state.accessToken = action.payload.accessToken
      state.user = action.payload.user
      localStorage.setItem(REFRESH_KEY, action.payload.refreshToken)
    },
    clearSession(state) {
      state.accessToken = null
      state.user = null
      localStorage.removeItem(REFRESH_KEY)
    },
  },
})

export const { setSession, clearSession } = authSlice.actions
export const selectAuth = (state: { auth: AuthState }) => state.auth
export const selectRefreshToken = () => loadRefresh()
export default authSlice.reducer
