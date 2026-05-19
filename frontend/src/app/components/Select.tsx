import { cn } from "../utils/cn";

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  options: Array<{ value: string; label: string }>;
  placeholder?: string;
  className?: string;
}

export function Select({
  label,
  error,
  options,
  placeholder = "Seleccionar...",
  className,
  ...props
}: SelectProps) {
  return (
    <div className="flex flex-col gap-2">
      {label && (
        <label className="text-sm font-medium text-[#333333]">
          {label}
        </label>
      )}
      <select
        className={cn(
          "px-3 py-2 border border-[#CCCCCC] rounded bg-white text-[#333333]",
          "focus:outline-none focus:ring-2 focus:ring-[#1A6BBF] focus:border-transparent",
          error && "border-[#C0392B] focus:ring-[#C0392B]",
          className
        )}
        {...props}
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error && (
        <span className="text-sm text-[#C0392B]">{error}</span>
      )}
    </div>
  );
}
