import { cn } from "../utils/cn";

/** Props del contenedor Table. */
interface TableProps {
  children: React.ReactNode;
  className?: string;
}

/**
 * Contenedor de tabla con scroll horizontal y borde exterior.
 * Componer con TableHeader, TableBody, TableRow, TableHead y TableCell.
 */
export function Table({ children, className }: TableProps) {
  return (
    <div className="w-full overflow-auto border border-[#CCCCCC] rounded">
      <table className={cn("w-full border-collapse", className)}>
        {children}
      </table>
    </div>
  );
}

/** Props del encabezado de tabla. */
interface TableHeaderProps {
  children: React.ReactNode;
  className?: string;
}

/** Encabezado de tabla con fondo oscuro (#333333) y texto blanco. */
export function TableHeader({ children, className }: TableHeaderProps) {
  return (
    <thead className={cn("bg-[#333333] text-white", className)}>
      {children}
    </thead>
  );
}

/** Props del cuerpo de tabla. */
interface TableBodyProps {
  children: React.ReactNode;
  className?: string;
}

/** Cuerpo de la tabla que contiene las filas de datos. */
export function TableBody({ children, className }: TableBodyProps) {
  return <tbody className={cn("", className)}>{children}</tbody>;
}

/** Props de una fila de tabla. */
interface TableRowProps {
  children: React.ReactNode;
  className?: string;
  /** Activa el fondo gris alterno en filas pares para facilitar la lectura. */
  striped?: boolean;
}

/** Fila de tabla con borde inferior. Soporta estilo rayado opcional. */
export function TableRow({ children, className, striped }: TableRowProps) {
  return (
    <tr
      className={cn(
        "border-b border-[#CCCCCC] last:border-b-0",
        striped && "even:bg-[#F5F5F5]",
        className
      )}
    >
      {children}
    </tr>
  );
}

/** Props de una celda de encabezado (th). */
interface TableHeadProps {
  children: React.ReactNode;
  className?: string;
}

/** Celda de encabezado alineada a la izquierda con tipografía semibold. */
export function TableHead({ children, className }: TableHeadProps) {
  return (
    <th
      className={cn(
        "px-4 py-3 text-left font-medium text-sm",
        className
      )}
    >
      {children}
    </th>
  );
}

/** Props de una celda de dato (td). */
interface TableCellProps {
  children: React.ReactNode;
  className?: string;
}

/** Celda de dato con padding estándar y color de texto primario. */
export function TableCell({ children, className }: TableCellProps) {
  return (
    <td className={cn("px-4 py-3 text-sm text-[#333333]", className)}>
      {children}
    </td>
  );
}
