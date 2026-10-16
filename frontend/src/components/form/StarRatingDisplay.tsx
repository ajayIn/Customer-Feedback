const LABELS: Record<number, string> = {
  1: 'Poor',
  2: 'Fair',
  3: 'Good',
  4: 'Very good',
  5: 'Excellent',
}

type Props = {
  label: string
  /** 1–5; decimals are rounded for star fill */
  value: number
  hint?: string
}

export function StarRatingDisplay({ label, value, hint }: Props) {
  const rounded = Math.min(5, Math.max(1, Math.round(value)))
  const displayHint = hint ?? `${value.toFixed(1)} · ${LABELS[rounded] ?? ''}`

  return (
    <div className="star-rating" aria-label={`${label}: ${value.toFixed(1)} out of 5`}>
      <div className="star-rating__label-row">
        <span className="star-rating__label">{label}</span>
        <span className="star-rating__hint">{displayHint}</span>
      </div>
      <div className="star-rating__stars" aria-hidden>
        {[1, 2, 3, 4, 5].map((star) => (
          <span
            key={star}
            className={`star-rating__star star-rating__star--readonly ${star <= rounded ? 'star-rating__star--on' : ''}`}
          >
            ★
          </span>
        ))}
      </div>
    </div>
  )
}
