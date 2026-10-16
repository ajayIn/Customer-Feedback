import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useReportSummaryQuery } from '../app/api'
import { FormSection } from '../components/form/FormSection'
import { NpsAverageDisplay } from '../components/form/NpsAverageDisplay'
import { StarRatingDisplay } from '../components/form/StarRatingDisplay'
import { PageHeader } from '../components/PageHeader'
import { StatusBadge } from '../components/StatusBadge'
import { DEFAULT_STORE_ID } from '../types/api'

function DashboardShell({ children }: { children: ReactNode }) {
  return (
    <div className="main--narrow page-stack">
      <PageHeader
        title="Store summary"
        subtitle="Downtown Store · live reporting from customer feedback"
      />
      <section className="card">{children}</section>
    </div>
  )
}

export function ManagerDashboardPage() {
  const { data, isLoading, error } = useReportSummaryQuery({ storeId: DEFAULT_STORE_ID })

  if (isLoading) {
    return (
      <DashboardShell>
        <p className="loading-state" style={{ margin: 0 }}>Loading summary…</p>
      </DashboardShell>
    )
  }

  if (error || !data) {
    return (
      <DashboardShell>
        <div className="validation-summary" role="alert">
          Could not load reporting summary. Check that Reporting and RabbitMQ are running, then refresh.
        </div>
      </DashboardShell>
    )
  }

  const workflow = [
    { status: 'New' as const, count: data.statusCounts.new },
    { status: 'Reviewed' as const, count: data.statusCounts.reviewed },
    { status: 'Actioned' as const, count: data.statusCounts.actioned },
  ]

  return (
    <DashboardShell>
      <div className="form">
        <FormSection
          title="At a glance"
          description="Aggregated scores across all submissions for this store."
        >
          <div className="summary-kpis">
            <div className="summary-kpi">
              <span className="summary-kpi__label">Total feedback</span>
              <strong className="summary-kpi__value">{data.totalCount}</strong>
            </div>
          </div>
          <StarRatingDisplay label="Overall experience" value={data.averageOverall} />
          <NpsAverageDisplay value={data.averageNps} />
        </FormSection>

        <FormSection title="Workflow" description="How submissions are moving through review.">
          <div className="workflow-grid">
            {workflow.map(({ status, count }) => (
              <div key={status} className="workflow-card">
                <StatusBadge status={status} />
                <strong className="workflow-card__count">{count}</strong>
                <span className="workflow-card__hint">submissions</span>
              </div>
            ))}
          </div>
        </FormSection>

        <FormSection title="By category" description="Volume and average rating for each topic.">
          {data.byCategory.length === 0 ? (
            <p className="muted" style={{ margin: 0 }}>No category breakdown yet.</p>
          ) : (
            <div className="category-breakdown">
              {data.byCategory.map((row) => (
                <div key={row.category} className="category-breakdown__row">
                  <div className="category-breakdown__meta">
                    <span className="chip chip--static">{row.category}</span>
                    <span className="category-breakdown__count">
                      {row.count} {row.count === 1 ? 'submission' : 'submissions'}
                    </span>
                  </div>
                  <StarRatingDisplay
                    label="Avg overall"
                    value={row.averageOverall}
                    hint={row.averageOverall.toFixed(1)}
                  />
                </div>
              ))}
            </div>
          )}
        </FormSection>

        <FormSection title="Trend" description="Recent periods — count and averages.">
          {data.trend.length === 0 ? (
            <p className="muted" style={{ margin: 0 }}>No trend data yet.</p>
          ) : (
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Period</th>
                    <th>Count</th>
                    <th>Overall</th>
                    <th>NPS</th>
                  </tr>
                </thead>
                <tbody>
                  {data.trend.map((row) => (
                    <tr key={row.period}>
                      <td>{row.period}</td>
                      <td>{row.count}</td>
                      <td>
                        <span className="rating-pill">★ {row.averageOverall.toFixed(1)}</span>
                      </td>
                      <td>{row.averageNps.toFixed(1)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </FormSection>

        <Link to="/manager/feedback" className="btn btn--primary btn--block">
          Manage feedback
        </Link>
      </div>
    </DashboardShell>
  )
}
