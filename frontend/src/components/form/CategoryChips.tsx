import type { FeedbackCategory } from '../../types/api'

type Props = {
  options: FeedbackCategory[]
  value: FeedbackCategory
  onChange: (value: FeedbackCategory) => void
}

export function CategoryChips({ options, value, onChange }: Props) {
  return (
    <div className="chip-group" role="radiogroup" aria-label="Feedback category">
      {options.map((option) => (
        <button
          key={option}
          type="button"
          role="radio"
          aria-checked={value === option}
          className={`chip ${value === option ? 'chip--on' : ''}`}
          onClick={() => onChange(option)}
        >
          {option}
        </button>
      ))}
    </div>
  )
}
