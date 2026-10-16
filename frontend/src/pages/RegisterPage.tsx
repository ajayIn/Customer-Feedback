import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useRegisterMutation } from '../app/api'
import { PageHeader } from '../components/PageHeader'
import { parseProblemDetails } from '../utils/problemDetails'
import {
  hasFieldErrors,
  validateRegisterForm,
  type RegisterFieldErrors,
} from '../utils/registerValidation'

export function RegisterPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [summaryError, setSummaryError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<RegisterFieldErrors>({})
  const [register, { isLoading }] = useRegisterMutation()
  const navigate = useNavigate()

  function clearFieldError(field: keyof RegisterFieldErrors) {
    setFieldErrors((prev) => {
      if (!prev[field]) return prev
      const next = { ...prev }
      delete next[field]
      return next
    })
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setSummaryError(null)

    const clientErrors = validateRegisterForm({ displayName, email, password })
    if (hasFieldErrors(clientErrors)) {
      setFieldErrors(clientErrors)
      setSummaryError('Please correct the errors below before creating your account.')
      return
    }

    setFieldErrors({})
    try {
      await register({
        email: email.trim(),
        password,
        displayName: displayName.trim(),
      }).unwrap()
      navigate('/login')
    } catch (err) {
      const status = (err as { status?: number })?.status
      const { summary, fieldErrors: serverFields } = parseProblemDetails(
        err,
        'We could not create your account. Please try again.',
      )
      const nextFields = { ...serverFields }
      if (status === 409) {
        nextFields.email = summary
      }
      setFieldErrors(nextFields)
      setSummaryError(
        Object.keys(nextFields).length > 0
          ? 'Please correct the errors below before creating your account.'
          : summary,
      )
    }
  }

  return (
    <div className="main--narrow" style={{ maxWidth: 420, margin: '0 auto' }}>
      <PageHeader title="Create account" subtitle="Register as a customer to submit store feedback." />
      <section className="card card--auth">
        {summaryError && (
          <div className="validation-summary" role="alert" aria-live="polite">
            {summaryError}
          </div>
        )}

        <form onSubmit={onSubmit} className="form" noValidate style={{ marginTop: summaryError ? '1rem' : 0 }}>
          <div className={`field ${fieldErrors.displayName ? 'field-invalid' : ''}`}>
            <label>
              Display name
              <input
                value={displayName}
                onChange={(e) => {
                  setDisplayName(e.target.value)
                  clearFieldError('displayName')
                  setSummaryError(null)
                }}
                aria-invalid={Boolean(fieldErrors.displayName)}
                autoComplete="name"
              />
              {fieldErrors.displayName && <span className="field-error">{fieldErrors.displayName}</span>}
            </label>
          </div>

          <div className={`field ${fieldErrors.email ? 'field-invalid' : ''}`}>
            <label>
              Email
              <input
                type="email"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value)
                  clearFieldError('email')
                  setSummaryError(null)
                }}
                aria-invalid={Boolean(fieldErrors.email)}
                autoComplete="email"
              />
              {fieldErrors.email && <span className="field-error">{fieldErrors.email}</span>}
            </label>
          </div>

          <div className={`field ${fieldErrors.password ? 'field-invalid' : ''}`}>
            <label>
              Password
              <input
                type="password"
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value)
                  clearFieldError('password')
                  setSummaryError(null)
                }}
                aria-invalid={Boolean(fieldErrors.password)}
                autoComplete="new-password"
              />
              {fieldErrors.password ? (
                <span className="field-error">{fieldErrors.password}</span>
              ) : (
                <span className="field-hint">At least 8 characters.</span>
              )}
            </label>
          </div>

          <button type="submit" className="btn btn--primary btn--block" disabled={isLoading}>
            {isLoading ? 'Creating account…' : 'Create account'}
          </button>
        </form>
        <p className="auth-footer">
          Already registered? <Link to="/login">Sign in</Link>
        </p>
      </section>
    </div>
  )
}
