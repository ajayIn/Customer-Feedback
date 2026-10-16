type Props = {
  value: number
}

export function NpsAverageDisplay({ value }: Props) {
  const display = value.toFixed(1)
  const highlight = Math.min(10, Math.max(0, Math.round(value)))

  return (
    <div className="nps-scale nps-scale--summary">
      <p className="nps-scale__question">Average recommendation score (NPS)</p>
      <p className="nps-summary__score" aria-label={`Average NPS ${display}`}>
        {display}
      </p>
      <div className="nps-scale__buttons nps-scale__buttons--readonly" aria-hidden>
        {Array.from({ length: 11 }, (_, i) => i).map((score) => (
          <span
            key={score}
            className={`nps-scale__btn nps-scale__btn--readonly ${score === highlight ? 'nps-scale__btn--on' : ''}`}
          >
            {score}
          </span>
        ))}
      </div>
      <div className="nps-scale__legend">
        <span>0 — Not at all likely</span>
        <span>10 — Extremely likely</span>
      </div>
    </div>
  )
}
