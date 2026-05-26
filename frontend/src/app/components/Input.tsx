import { cn } from "../utils/cn";

/**
 * Props del componente Input.
 * Extiende todos los atributos nativos de <input>.
 */
interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  /** Etiqueta visible encima del campo. */
  label?: string;
  /** Mensaje de error que aparece en rojo debajo del campo y cambia el borde a rojo. */
  error?: string;
  className?: string;
}

/**
 * Campo de texto con soporte para label, estado de error y todos los atributos nativos.
 * El anillo de foco cambia de azul a rojo cuando hay un mensaje de error activo.
 */
export function Input({ label, error, className, ...props }: InputProps) {
  return (
    <div className="flex flex-col gap-2">
      {label && (
        <label className="text-sm font-medium text-[#333333]">
          {label}
        </label>
      )}
      <input
        className={cn(
          "px-3 py-2 border border-[#CCCCCC] rounded bg-white text-[#333333] placeholder:text-[#999999]",
          "focus:outline-none focus:ring-2 focus:ring-[#1A6BBF] focus:border-transparent",
          error && "border-[#C0392B] focus:ring-[#C0392B]",
          className
        )}
        {...props}
      />
      {error && (
        <span className="text-sm text-[#C0392B]">{error}</span>
      )}
    </div>
  );
}
