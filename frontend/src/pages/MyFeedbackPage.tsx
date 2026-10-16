import { Link } from 'react-router-dom'
import { useMyFeedbackQuery } from '../app/api'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'

export function MyFeedbackPage() {
  const { data, isLoading, error } = useMyFeedbackQuery({})

  if (isLoading) return <p className="loading-state">Loading your submissions…</p>
  if (error) return <p className="error">Could not load your feedback.</p>

  return (
    <section>
      <PageHeader title="My submissions" subtitle="Track status of feedback you have sent." />
      <div className="card">
        {!data?.items.length ? (
          <p className="muted" style={{ textAlign: 'center', padding: '1.5rem 0' }}>
            No feedback yet.{' '}
            <Link to="/feedback/new">Submit your first review</Link>
          </p>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Visit date</th>
                  <th>Overall</th>
                  <th>NPS</th>
                  <th>Category</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td>{item.visitDate}</td>
                    <td><span className="rating-pill">★ {item.overall}</span></td>
                    <td>{item.npsScore}</td>
                    <td>{item.category}</td>
                    <td><StatusBadge status={item.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  )
}
