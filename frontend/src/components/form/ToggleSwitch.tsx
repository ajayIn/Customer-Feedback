type Props = {
  label: string
  checked: boolean
  onChange: (checked: boolean) => void
}

export function ToggleSwitch({ label, checked, onChange }: Props) {
  return (
    <label className="toggle">
      <input
        type="checkbox"
        className="toggle__input"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
      />
      <span className="toggle__track" aria-hidden="true">
        <span className="toggle__thumb" />
      </span>
      <span className="toggle__label">{label}</span>
    </label>
  )
}
