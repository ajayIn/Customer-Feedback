import { Navigate } from 'react-router-dom'
import { useSelector } from 'react-redux'
import type { Role } from '../types/api'
import { selectAuth } from '../app/authSlice'

export function RequireAuth({ role, children }: { role?: Role; children: React.ReactNode }) {
  const auth = useSelector(selectAuth)
  if (!auth.accessToken || !auth.user) {
    return <Navigate to="/login" replace />
  }
  if (role && !auth.user.roles.includes(role)) {
    const fallback = auth.user.roles.includes('StoreManager') ? '/' : '/feedback/new'
    return <Navigate to={fallback} replace />
  }
  return <>{children}</>
}
