import { useState } from 'react'
import {
  useManagerFeedbackQuery,
  usePatchCategoryMutation,
  usePatchStatusMutation,
} from '../app/api'
import { PageHeader } from '../components/PageHeader'
import type { FeedbackCategory, FeedbackStatus } from '../types/api'

const categories: FeedbackCategory[] = ['Service', 'Product', 'Facilities', 'Other']
const statuses: FeedbackStatus[] = ['New', 'Reviewed', 'Actioned']

export function ManagerFeedbackPage() {
  const [categoryFilter, setCategoryFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const { data, isLoading } = useManagerFeedbackQuery({
    category: categoryFilter || undefined,
    status: statusFilter || undefined,
  })
  const [patchCategory] = usePatchCategoryMutation()
  const [patchStatus] = usePatchStatusMutation()

  if (isLoading) return <p className="loading-state">Loading feedback…</p>

  return (
    <section>
      <PageHeader title="Manage feedback" subtitle="Filter, recategorize, and update workflow status." />
      <div className="card">
        <div className="filters">
          <div className="field" style={{ flex: 1, minWidth: 140 }}>
            <label>
              Category
              <select value={categoryFilter} onChange={(e) => setCategoryFilter(e.target.value)}>
                <option value="">All categories</option>
                {categories.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            </label>
          </div>
          <div className="field" style={{ flex: 1, minWidth: 140 }}>
            <label>
              Status
              <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
                <option value="">All statuses</option>
                {statuses.map((s) => <option key={s} value={s}>{s}</option>)}
              </select>
            </label>
          </div>
        </div>
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Visit</th>
                <th>Overall</th>
                <th>Category</th>
                <th>Status</th>
                <th>Comments</th>
              </tr>
            </thead>
            <tbody>
              {data?.items.map((item) => (
                <tr key={item.id}>
                  <td>{item.visitDate}</td>
                  <td><span className="rating-pill">★ {item.overall}</span></td>
                  <td>
                    <select
                      value={item.category}
                      onChange={(e) => patchCategory({ id: item.id, category: e.target.value })}
                    >
                      {categories.map((c) => <option key={c} value={c}>{c}</option>)}
                    </select>
                  </td>
                  <td>
                    <select
                      value={item.status}
                      onChange={(e) => patchStatus({ id: item.id, status: e.target.value })}
                    >
                      {statuses.map((s) => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </td>
                  <td className="muted" style={{ maxWidth: 200 }}>{item.comments ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {!data?.items.length && <p className="muted" style={{ marginTop: '1rem' }}>No feedback matches your filters.</p>}
      </div>
    </section>
  )
}
