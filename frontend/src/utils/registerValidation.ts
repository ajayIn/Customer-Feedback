export type RegisterForm = {
  displayName: string
  email: string
  password: string
}

export type RegisterFieldErrors = Partial<Record<'displayName' | 'email' | 'password', string>>

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function validateRegisterForm(form: RegisterForm): RegisterFieldErrors {
  const errors: RegisterFieldErrors = {}
  const displayName = form.displayName.trim()
  const email = form.email.trim()
  const password = form.password

  if (!displayName) {
    errors.displayName = 'Display name is required.'
  } else if (displayName.length > 100) {
    errors.displayName = 'Display name must be 100 characters or fewer.'
  }

  if (!email) {
    errors.email = 'Email is required.'
  } else if (!EMAIL_PATTERN.test(email)) {
    errors.email = 'Enter a valid email address (example: you@example.com).'
  }

  if (!password) {
    errors.password = 'Password is required.'
  } else if (password.length < 8) {
    errors.password = 'Password must be at least 8 characters.'
  }

  return errors
}

export function hasFieldErrors(errors: RegisterFieldErrors): boolean {
  return Object.keys(errors).length > 0
}
