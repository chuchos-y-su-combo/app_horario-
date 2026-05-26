import { X, AlertTriangle } from "lucide-react";
import { cn } from "../utils/cn";
import { Button } from "./Button";

/** Props del componente Modal genérico. */
interface ModalProps {
  /** Controla la visibilidad del modal. Si es false, no se renderiza nada. */
  isOpen: boolean;
  /** Callback invocado al hacer clic en el overlay o en el botón de cierre. */
  onClose: () => void;
  /** Texto del encabezado del modal. */
  title: string;
  children: React.ReactNode;
  /** Nodo opcional para el área de acciones (botones) en el pie del modal. */
  footer?: React.ReactNode;
  /** Ancho máximo del contenedor. Por defecto "md" (max-w-lg). */
  size?: "sm" | "md" | "lg" | "xl";
  className?: string;
}

/**
 * Modal de propósito general con overlay, encabezado, cuerpo con scroll y pie opcional.
 * Hacer clic en el overlay invoca `onClose` para cerrarlo.
 * El contenido tiene altura máxima del 70% de la pantalla con scroll vertical.
 */
export function Modal({
  isOpen,
  onClose,
  title,
  children,
  footer,
  size = "md",
  className,
}: ModalProps) {
  if (!isOpen) return null;

  const sizes = {
    sm: "max-w-md",
    md: "max-w-lg",
    lg: "max-w-2xl",
    xl: "max-w-4xl",
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Overlay semitransparente */}
      <div
        className="absolute inset-0 bg-black/50"
        onClick={onClose}
      />

      {/* Contenido del modal */}
      <div
        className={cn(
          "relative bg-white rounded-lg shadow-lg w-full mx-4",
          sizes[size],
          className
        )}
      >
        {/* Encabezado */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-[#CCCCCC]">
          <h2 className="text-lg font-medium text-[#333333]">{title}</h2>
          <button
            onClick={onClose}
            className="text-[#666666] hover:text-[#333333] transition-colors"
          >
            <X size={20} />
          </button>
        </div>

        {/* Cuerpo con scroll */}
        <div className="px-6 py-4 max-h-[70vh] overflow-y-auto">{children}</div>

        {/* Pie con acciones */}
        {footer && (
          <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-[#CCCCCC]">
            {footer}
          </div>
        )}
      </div>
    </div>
  );
}

/** Props del modal de confirmación de acción destructiva o de advertencia. */
interface ConfirmModalProps {
  isOpen: boolean;
  onClose: () => void;
  /** Callback ejecutado cuando el usuario confirma la acción. */
  onConfirm: () => void;
  title: string;
  /** Mensaje descriptivo de la acción a confirmar. */
  message: string;
  /** Texto del botón de confirmación. Por defecto "Confirmar". */
  confirmText?: string;
  /** Texto del botón de cancelación. Por defecto "Cancelar". */
  cancelText?: string;
  /** Amarillo para advertencias, rojo para acciones irreversibles. Por defecto "warning". */
  variant?: "warning" | "danger";
}

/**
 * Modal de confirmación que muestra un icono de advertencia, un mensaje y dos botones.
 * Llama a `onConfirm` y cierra el modal cuando el usuario acepta.
 */
export function ConfirmModal({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = "Confirmar",
  cancelText = "Cancelar",
  variant = "warning",
}: ConfirmModalProps) {
  const variantColors = {
    warning: "text-[#E8A020]",
    danger: "text-[#C0392B]",
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={title}
      size="sm"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            {cancelText}
          </Button>
          <Button
            variant={variant === "danger" ? "destructive" : "primary"}
            onClick={() => {
              onConfirm();
              onClose();
            }}
          >
            {confirmText}
          </Button>
        </>
      }
    >
      <div className="flex items-start gap-3">
        <AlertTriangle className={cn("mt-0.5", variantColors[variant])} size={24} />
        <p className="text-[#333333] flex-1">{message}</p>
      </div>
    </Modal>
  );
}
