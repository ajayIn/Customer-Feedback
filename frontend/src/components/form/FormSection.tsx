import type { ReactNode } from 'react'

export function FormSection({ title, description, children }: {
  title: string
  description?: string
  children: ReactNode
}) {
  return (
    <section className="form-section">
      <div className="form-section__head">
        <h2 className="form-section__title">{title}</h2>
        {description && <p className="form-section__desc">{description}</p>}
      </div>
      <div className="form-section__body">{children}</div>
    </section>
  )
}
