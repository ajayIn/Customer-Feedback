type Props = {
  value: number
  onChange: (value: number) => void
}

export function NpsScale({ value, onChange }: Props) {
  return (
    <div className="nps-scale">
      <p className="nps-scale__question">
        How likely are you to recommend this store to a friend or colleague?
      </p>
      <div className="nps-scale__buttons" role="radiogroup" aria-label="Recommendation score 0 to 10">
        {Array.from({ length: 11 }, (_, i) => i).map((score) => (
          <button
            key={score}
            type="button"
            role="radio"
            aria-checked={value === score}
            className={`nps-scale__btn ${value === score ? 'nps-scale__btn--on' : ''}`}
            onClick={() => onChange(score)}
          >
            {score}
          </button>
        ))}
      </div>
      <div className="nps-scale__legend">
        <span>0 — Not at all likely</span>
        <span>10 — Extremely likely</span>
      </div>
    </div>
  )
}
