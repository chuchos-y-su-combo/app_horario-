import { cn } from "../utils/cn";

/** Props del contenedor principal Card. */
interface CardProps {
  children: React.ReactNode;
  className?: string;
}

/** Contenedor de sección con fondo blanco, borde gris y padding estándar. */
export function Card({ children, className }: CardProps) {
  return (
    <div className={cn("bg-white rounded-lg border border-[#CCCCCC] p-5", className)}>
      {children}
    </div>
  );
}

/** Props de la cabecera de una Card. */
interface CardHeaderProps {
  children: React.ReactNode;
  className?: string;
}

/** Cabecera de Card con margen inferior para separar del contenido. */
export function CardHeader({ children, className }: CardHeaderProps) {
  return <div className={cn("mb-4", className)}>{children}</div>;
}

/** Props del título de una Card. */
interface CardTitleProps {
  children: React.ReactNode;
  className?: string;
}

/** Título de Card renderizado como h3 con tipografía de sección. */
export function CardTitle({ children, className }: CardTitleProps) {
  return <h3 className={cn("text-lg font-medium text-[#333333]", className)}>{children}</h3>;
}

/** Props del área de contenido de una Card. */
interface CardContentProps {
  children: React.ReactNode;
  className?: string;
}

/** Área de contenido de Card sin restricciones de padding adicional. */
export function CardContent({ children, className }: CardContentProps) {
  return <div className={cn("", className)}>{children}</div>;
}
