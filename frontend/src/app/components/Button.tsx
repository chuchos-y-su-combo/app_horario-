import { cn } from "../utils/cn";

/**
 * Props del componente Button.
 * Extiende todos los atributos nativos de <button>.
 */
interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  children: React.ReactNode;
  /** Estilo visual: azul primario, borde secundario o rojo destructivo. */
  variant?: "primary" | "secondary" | "destructive";
  /** Tamaño del padding y fuente. Por defecto "md". */
  size?: "sm" | "md" | "lg";
  className?: string;
}

/**
 * Botón reutilizable con soporte para variantes de color y tamaños.
 * Se deshabilita visualmente (opacidad + cursor) cuando `disabled` es true.
 */
export function Button({
  children,
  variant = "primary",
  size = "md",
  className,
  ...props
}: ButtonProps) {
  const variants = {
    primary: "bg-[#1A6BBF] text-white hover:bg-[#003087] active:bg-[#003087]",
    secondary: "border border-[#333333] bg-transparent text-[#333333] hover:bg-[#F5F5F5]",
    destructive: "bg-[#C0392B] text-white hover:bg-[#A0291B]",
  };

  const sizes = {
    sm: "px-3 py-1.5 text-sm",
    md: "px-4 py-2 text-base",
    lg: "px-6 py-3 text-base",
  };

  return (
    <button
      className={cn(
        "rounded font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed",
        variants[variant],
        sizes[size],
        className
      )}
      {...props}
    >
      {children}
    </button>
  );
}
