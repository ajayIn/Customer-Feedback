import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useDispatch } from 'react-redux'
import { useLoginMutation } from '../app/api'
import { setSession } from '../app/authSlice'
import { PageHeader } from '../components/PageHeader'
import type { ProblemDetails } from '../types/api'

export function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [login, { isLoading }] = useLoginMutation()
  const dispatch = useDispatch()
  const navigate = useNavigate()

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    try {
      const result = await login({ email, password }).unwrap()
      dispatch(
        setSession({
          accessToken: result.accessToken,
          refreshToken: result.refreshToken,
          user: result.user,
        }),
      )
      navigate(result.user.roles.includes('StoreManager') ? '/' : '/feedback/new')
    } catch (err) {
      const problem = err as { data?: ProblemDetails }
      setError(problem.data?.detail ?? 'Invalid email or password. Please try again.')
    }
  }

  return (
    <div className="main--narrow" style={{ maxWidth: 420, margin: '0 auto' }}>
      <PageHeader title="Welcome back" subtitle="Sign in to submit or manage feedback." />
      <section className="card card--auth">
        {error && (
          <div className="validation-summary" role="alert" style={{ marginBottom: '1rem' }}>
            {error}
          </div>
        )}
        <form onSubmit={onSubmit} className="form">
          <div className="field">
            <label>
              Email
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
                autoComplete="email"
                required
              />
            </label>
          </div>
          <div className="field">
            <label>
              Password
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="current-password"
                required
              />
            </label>
          </div>
          <button type="submit" className="btn btn--primary btn--block" disabled={isLoading}>
            {isLoading ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
        <p className="auth-footer">
          Store manager demo: <strong>manager@store.local</strong> / <strong>ChangeMe!123</strong>
        </p>
        <p className="auth-footer">
          New customer? <Link to="/register">Create an account</Link>
        </p>
      </section>
    </div>
  )
}
