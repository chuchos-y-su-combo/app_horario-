import { cn } from "../utils/cn";

/**
 * Props del componente Select.
 * Extiende todos los atributos nativos de <select>.
 */
interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  /** Etiqueta visible encima del selector. */
  label?: string;
  /** Mensaje de error que aparece en rojo debajo del selector. */
  error?: string;
  /** Lista de opciones a mostrar. Cada opción tiene un value y un label visible. */
  options: Array<{ value: string; label: string }>;
  /** Texto de la opción vacía por defecto. Por defecto "Seleccionar...". */
  placeholder?: string;
  className?: string;
}

/**
 * Selector desplegable con soporte para label, placeholder, error y opciones tipadas.
 * La primera opción siempre es el placeholder con value="" para representar "sin selección".
 */
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
