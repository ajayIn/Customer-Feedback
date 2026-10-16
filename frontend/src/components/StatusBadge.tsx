import type { FeedbackStatus } from '../types/api'

const CLASS: Record<FeedbackStatus, string> = {
  New: 'badge badge--new',
  Reviewed: 'badge badge--reviewed',
  Actioned: 'badge badge--actioned',
}

export function StatusBadge({ status }: { status: FeedbackStatus }) {
  return <span className={CLASS[status]}>{status}</span>
}
