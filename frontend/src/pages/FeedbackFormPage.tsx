import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useSelector } from 'react-redux'
import { useCreateFeedbackMutation } from '../app/api'
import { selectAuth } from '../app/authSlice'
import { CategoryChips } from '../components/form/CategoryChips'
import { FormSection } from '../components/form/FormSection'
import { NpsScale } from '../components/form/NpsScale'
import { StarRating } from '../components/form/StarRating'
import { ToggleSwitch } from '../components/form/ToggleSwitch'
import { PageHeader } from '../components/PageHeader'
import { DEFAULT_STORE_ID, type FeedbackCategory } from '../types/api'

const categories: FeedbackCategory[] = ['Service', 'Product', 'Facilities', 'Other']

const RATING_FIELDS: { key: 'overall' | 'cleanliness' | 'staff' | 'waitTime' | 'productAvailability'; label: string }[] = [
  { key: 'overall', label: 'Overall experience' },
  { key: 'cleanliness', label: 'Cleanliness' },
  { key: 'staff', label: 'Staff helpfulness' },
  { key: 'waitTime', label: 'Wait time' },
  { key: 'productAvailability', label: 'Product availability' },
]

export function FeedbackFormPage() {
  const accountEmail = useSelector(selectAuth).user?.email ?? ''
  const [createFeedback, { isLoading, isSuccess }] = useCreateFeedbackMutation()
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState({
    visitDate: new Date().toISOString().slice(0, 10),
    overall: 5,
    cleanliness: 5,
    staff: 5,
    waitTime: 3,
    productAvailability: 4,
    npsScore: 8,
    category: 'Service' as FeedbackCategory,
    comments: '',
    isAnonymous: false,
  })

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    try {
      await createFeedback({
        storeId: DEFAULT_STORE_ID,
        visitDate: form.visitDate,
        overall: form.overall,
        cleanliness: form.cleanliness,
        staff: form.staff,
        waitTime: form.waitTime,
        productAvailability: form.productAvailability,
        npsScore: form.npsScore,
        category: form.category,
        comments: form.comments || undefined,
        contactEmail: form.isAnonymous ? undefined : accountEmail || undefined,
        isAnonymous: form.isAnonymous,
      }).unwrap()
    } catch {
      setError('Could not submit feedback. Check your ratings and try again.')
    }
  }

  return (
    <div className="main--narrow" style={{ maxWidth: 640, margin: '0 auto' }}>
      <PageHeader
        title="Share your experience"
        subtitle="Your feedback helps us improve Downtown Store."
      />

      {isSuccess && (
        <div className="success-banner" role="status">
          Thank you — your feedback was submitted.{' '}
          <Link to="/feedback/mine">View my submissions</Link>
        </div>
      )}

      <section className="card">
        {error && (
          <div className="validation-summary" role="alert" style={{ marginBottom: '1rem' }}>
            {error}
          </div>
        )}

        <form onSubmit={onSubmit} className="form">
          <FormSection title="Visit details" description="When did you visit and what was it about?">
            <div className="field">
              <label>
                Visit date
                <input
                  type="date"
                  value={form.visitDate}
                  max={new Date().toISOString().slice(0, 10)}
                  onChange={(e) => setForm({ ...form, visitDate: e.target.value })}
                  required
                />
              </label>
            </div>
            <div>
              <span className="star-rating__label" style={{ display: 'block', marginBottom: '0.5rem' }}>
                Category
              </span>
              <CategoryChips
                options={categories}
                value={form.category}
                onChange={(category) => setForm({ ...form, category })}
              />
            </div>
          </FormSection>

          <FormSection title="Your ratings" description="Tap the stars for each area (1–5).">
            {RATING_FIELDS.map(({ key, label }) => (
              <StarRating
                key={key}
                label={label}
                value={form[key]}
                onChange={(value) => setForm({ ...form, [key]: value })}
              />
            ))}
          </FormSection>

          <FormSection title="Recommendation" description="Net Promoter Score (NPS)">
            <NpsScale
              value={form.npsScore}
              onChange={(npsScore) => setForm({ ...form, npsScore })}
            />
          </FormSection>

          <FormSection title="Comments & contact">
            <div className="field">
              <label>
                Additional comments
                <textarea
                  value={form.comments}
                  placeholder="What went well? What could we improve?"
                  onChange={(e) => setForm({ ...form, comments: e.target.value })}
                  rows={4}
                />
              </label>
            </div>
            <ToggleSwitch
              label="Submit anonymously"
              checked={form.isAnonymous}
              onChange={(isAnonymous) => setForm({ ...form, isAnonymous })}
            />
            {!form.isAnonymous && accountEmail && (
              <div className="field">
                <label>
                  Contact email
                  <input type="email" value={accountEmail} readOnly autoComplete="email" />
                </label>
                <p className="field-hint">Taken from your account so we can follow up if needed.</p>
              </div>
            )}
          </FormSection>

          <button type="submit" className="btn btn--primary btn--block" disabled={isLoading || isSuccess}>
            {isLoading ? 'Submitting…' : 'Submit feedback'}
          </button>
        </form>
      </section>
    </div>
  )
}
