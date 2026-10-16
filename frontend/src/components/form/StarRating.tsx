const LABELS: Record<number, string> = {
  1: 'Poor',
  2: 'Fair',
  3: 'Good',
  4: 'Very good',
  5: 'Excellent',
}

type Props = {
  label: string
  value: number
  onChange: (value: number) => void
}

export function StarRating({ label, value, onChange }: Props) {
  return (
    <div className="star-rating" role="radiogroup" aria-label={label}>
      <div className="star-rating__label-row">
        <span className="star-rating__label">{label}</span>
        <span className="star-rating__hint">{LABELS[value] ?? ''}</span>
      </div>
      <div className="star-rating__stars">
        {[1, 2, 3, 4, 5].map((star) => (
          <button
            key={star}
            type="button"
            role="radio"
            aria-checked={value === star}
            className={`star-rating__star ${star <= value ? 'star-rating__star--on' : ''}`}
            onClick={() => onChange(star)}
          >
            ★
          </button>
        ))}
      </div>
    </div>
  )
}
