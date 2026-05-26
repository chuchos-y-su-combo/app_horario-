import { useCallback, useEffect, useRef, useState } from "react";
import { DndProvider, useDrag, useDrop } from "react-dnd";
import { HTML5Backend } from "react-dnd-html5-backend";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Select } from "../Select";
import { GripVertical, Loader2, BookOpen, CheckCircle, AlertTriangle } from "lucide-react";
import { manualAdjustmentService } from "../../../services/manual-adjustment.service";
import api from "../../../services/api";

// ─── Types ────────────────────────────────────────────────────────────────────

/** Identificador del tipo de elemento arrastrable para react-dnd. */
const DRAG_TYPE = "ASIGNACION";

/** Asignación de una sesión de clase: puede estar ubicada (con día y hora) o pendiente (sin ubicar). */
interface Asignacion {
  id: string;
  idDocente: string;
  idAsignatura: string;
  codigoAsignatura: string;
  nombreAsignatura: string;
  nombreDocente: string;
  /** Día de la semana (1=Lunes … 5=Viernes). null si la asignación no tiene día asignado aún. */
  dia: number | null;
  /** Hora de inicio en formato "HH:MM". null si sin ubicar. */
  horaInicio: string | null;
  /** Hora de fin en formato "HH:MM". null si sin ubicar. */
  horaFin: string | null;
  /** Estado de la asignación: "Propuesta", "AsignadaManual", "Confirmada" o "Cancelada". */
  estado: string;
  /** Escenario al que pertenece, p.ej. "ING_DIURNA". */
  escenario: string;
}

/** Datos transportados por react-dnd al arrastrar una asignación hacia una celda de la cuadrícula. */
interface DragItem {
  id: string;
  idDocente: string;
  nombreAsignatura: string;
  nombreDocente: string;
  /** Duración estimada del bloque en horas (por defecto 2). */
  duracion: number;
  /** Día de origen si el bloque ya estaba ubicado; null si venía del panel pendiente. */
  fromDia: number | null;
}

/** Franja de disponibilidad horaria de un docente (proveniente del endpoint /profesores/{id}/disponibilidad). */
interface DisponibilidadItem {
  /** Día de la semana (1=Lunes … 5=Viernes). */
  diaSemana: number;
  horaInicio: string;
  horaFin: string;
}

/** Estado del toast de notificación temporal (desaparece a los 4 s). */
interface ToastState {
  message: string;
  type: "success" | "error";
}

// ─── Constants ────────────────────────────────────────────────────────────────

const DAYS = ["Lun", "Mar", "Mié", "Jue", "Vie"];
const HOURS = Array.from({ length: 14 }, (_, i) => i + 7); // 07 – 20
const ESCENARIOS_OPCIONES = [
  { value: "",              label: "Todos los escenarios" },
  { value: "ING_DIURNA",   label: "Ingeniería Diurna" },
  { value: "ING_NOCTURNA", label: "Ingeniería Nocturna" },
  { value: "TAPSI_DIURNA", label: "TAPSI Diurna" },
  { value: "TAPSI_NOCTURNA", label: "TAPSI Nocturna" },
];

// ─── Helpers ─────────────────────────────────────────────────────────────────

/** Extrae la hora entera de una cadena "HH:MM"; devuelve 7 si la cadena es null. */
const parseHour = (hora: string | null) =>
  hora ? parseInt(hora.split(":")[0], 10) : 7;

/** Calcula la duración en horas entre dos franjas; mínimo 1 hora; por defecto 2 si falta alguna. */
const calcDuration = (inicio: string | null, fin: string | null) => {
  if (!inicio || !fin) return 2;
  return Math.max(parseHour(fin) - parseHour(inicio), 1);
};

/** Formatea un número de hora como cadena "HH:00" para enviarla al backend. */
const padHour = (h: number) => `${String(h).padStart(2, "0")}:00`;

// ─── DraggablePendingCard ─────────────────────────────────────────────────────

/**
 * Tarjeta arrastrable para asignaturas sin ubicar en el panel izquierdo.
 * Al arrastrarla hacia la cuadrícula, transporta los datos del DragItem al DroppableCell destino.
 */
function DraggablePendingCard({
  asignacion,
  isSelected,
  onClick,
}: {
  asignacion: Asignacion;
  isSelected: boolean;
  onClick: () => void;
}) {
  const [{ isDragging }, drag] = useDrag<DragItem, unknown, { isDragging: boolean }>(
    () => ({
      type: DRAG_TYPE,
      item: {
        id: asignacion.id,
        idDocente: asignacion.idDocente,
        nombreAsignatura: asignacion.nombreAsignatura,
        nombreDocente: asignacion.nombreDocente,
        duracion: 2,
        fromDia: null,
      },
      collect: (m) => ({ isDragging: m.isDragging() }),
    }),
    [asignacion]
  );

  return (
    <div
      ref={drag}
      style={{ opacity: isDragging ? 0.4 : 1 }}
      className={`p-3 rounded border cursor-grab active:cursor-grabbing transition-all ${
        isSelected
          ? "bg-[#1A6BBF]/10 border-[#1A6BBF]"
          : "bg-white border-[#CCCCCC] hover:border-[#1A6BBF]"
      }`}
      onClick={onClick}
    >
      <div className="flex items-start gap-2">
        <GripVertical className="text-[#999999] mt-0.5 shrink-0" size={16} />
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <p className="text-sm font-medium text-[#333333] truncate">{asignacion.codigoAsignatura}</p>
            <Badge variant="warning" className="text-xs">Pendiente</Badge>
          </div>
          <p className="text-xs text-[#666666] truncate mb-1">{asignacion.nombreAsignatura}</p>
          <p className="text-xs text-[#999999]">{asignacion.nombreDocente}</p>
        </div>
      </div>
    </div>
  );
}

// ─── DraggableGridBlock ───────────────────────────────────────────────────────

/**
 * Bloque de horario posicionado absolutamente en la cuadrícula semanal.
 * Es arrastrable para reubicar la asignación a otra celda; al soltarse llama al handleDrop del padre.
 */
function DraggableGridBlock({
  asignacion,
  style,
  isSelected,
  onClick,
}: {
  asignacion: Asignacion;
  style: React.CSSProperties;
  isSelected: boolean;
  onClick: () => void;
}) {
  const dur = calcDuration(asignacion.horaInicio, asignacion.horaFin);

  const [{ isDragging }, drag] = useDrag<DragItem, unknown, { isDragging: boolean }>(
    () => ({
      type: DRAG_TYPE,
      item: {
        id: asignacion.id,
        idDocente: asignacion.idDocente,
        nombreAsignatura: asignacion.nombreAsignatura,
        nombreDocente: asignacion.nombreDocente,
        duracion: dur,
        fromDia: asignacion.dia,
      },
      collect: (m) => ({ isDragging: m.isDragging() }),
    }),
    [asignacion, dur]
  );

  return (
    <div
      ref={drag}
      style={{ ...style, opacity: isDragging ? 0.35 : 1 }}
      className={`absolute text-white p-1.5 rounded text-xs overflow-hidden cursor-grab active:cursor-grabbing transition-shadow left-1 right-1 ${
        isSelected
          ? "bg-[#003087] border-2 border-[#1A6BBF] shadow-lg"
          : "bg-[#1A6BBF] border border-[#003087] hover:shadow-md"
      }`}
      onClick={onClick}
    >
      <div className="font-medium truncate">{asignacion.nombreAsignatura}</div>
      <div className="text-white/90 text-[10px] truncate">{asignacion.nombreDocente}</div>
      <div className="text-white/75 text-[10px]">
        {asignacion.horaInicio} – {asignacion.horaFin}
      </div>
    </div>
  );
}

// ─── DroppableCell ────────────────────────────────────────────────────────────

/**
 * Celda de la cuadrícula semanal que acepta soltar bloques arrastrados.
 * Cada celda corresponde a un par (día, hora); al soltar llama a onDrop con los datos del item y la posición.
 */
function DroppableCell({
  dayIdx,
  hour,
  onDrop,
}: {
  dayIdx: number;
  hour: number;
  onDrop: (item: DragItem, day: number, hour: number) => void;
}) {
  const handleDrop = useCallback(
    (item: DragItem) => onDrop(item, dayIdx + 1, hour),
    [onDrop, dayIdx, hour]
  );

  const [{ isOver, canDrop }, drop] = useDrop<DragItem, void, { isOver: boolean; canDrop: boolean }>(
    () => ({
      accept: DRAG_TYPE,
      drop: handleDrop,
      collect: (m) => ({
        isOver: m.isOver(),
        canDrop: m.canDrop(),
      }),
    }),
    [handleDrop]
  );

  return (
    <div
      ref={drop}
      className={`h-[42.85px] border-b border-[#CCCCCC] transition-colors ${
        isOver && canDrop
          ? "bg-[#1A6BBF]/20 ring-1 ring-inset ring-[#1A6BBF]"
          : canDrop
          ? "hover:bg-[#1A6BBF]/5"
          : ""
      }`}
    />
  );
}

// ─── Toast ────────────────────────────────────────────────────────────────────

/**
 * Notificación temporal (toast) que se auto-cierra después de 4 segundos.
 * Muestra un mensaje de éxito (verde) o error (rojo) sobre el contenido de la página.
 */
function Toast({ toast, onClose }: { toast: ToastState; onClose: () => void }) {
  useEffect(() => {
    const t = setTimeout(onClose, 4000);
    return () => clearTimeout(t);
  }, [onClose]);

  return (
    <div
      className={`fixed bottom-6 right-6 z-50 flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg text-sm font-medium transition-all ${
        toast.type === "success"
          ? "bg-[#1A7A4A] text-white"
          : "bg-[#C0392B] text-white"
      }`}
    >
      {toast.type === "success"
        ? <CheckCircle size={18} />
        : <AlertTriangle size={18} />}
      {toast.message}
    </div>
  );
}

// ─── Main view (inner — requires DndProvider above) ───────────────────────────

/**
 * Lógica principal del módulo de ajuste manual.
 * Gestiona el estado de todas las asignaciones del periodo seleccionado,
 * el selector de periodo, el filtro por escenario, los caches de disponibilidad
 * y habilitaciones de docentes, y el handler de drag-and-drop que llama a la API.
 * Separado del componente público para poder envolverse en DndProvider sin ciclos.
 */
function ManualAdjustmentInner() {
  const [asignaciones, setAsignaciones] = useState<Asignacion[]>([]);
  const [loading, setLoading]           = useState(true);
  const [error, setError]               = useState<string | null>(null);
  const [selectedId, setSelectedId]     = useState<string | null>(null);
  const [escenarioFiltro, setEscenarioFiltro] = useState("");
  const [toast, setToast]               = useState<ToastState | null>(null);
  const [dropping, setDropping]         = useState(false);

  // Period selector
  const [periodos, setPeriodos]                 = useState<string[]>([]);
  const [periodoSeleccionado, setPeriodoSeleccionado] = useState<string>("");
  const [loadingPeriodos, setLoadingPeriodos]   = useState(true);

  // Disponibilidad cache: idDocente → DisponibilidadItem[]
  const dispCache = useRef<Map<string, DisponibilidadItem[]>>(new Map());

  // Habilitados cache: idDocente → string[] (idAsignatura[])
  const habCache = useRef<Map<string, string[]>>(new Map());

  const showToast = useCallback((message: string, type: "success" | "error") => {
    setToast({ message, type });
  }, []);

  // ── Load periods ────────────────────────────────────────────────────────────
  useEffect(() => {
    (async () => {
      setLoadingPeriodos(true);
      try {
        const res = await api.get("/asignaciones/periodos-historicos");
        const list: string[] = Array.isArray(res.data) ? res.data : [];
        list.sort((a, b) => b.localeCompare(a)); // newest first
        setPeriodos(list);
        if (list.length > 0) setPeriodoSeleccionado(list[0]);
      } catch {
        // silently degrade — no periods available
      } finally {
        setLoadingPeriodos(false);
      }
    })();
  }, []);

  // ── Load asignaciones when period changes ───────────────────────────────────
  useEffect(() => {
    cargarAsignaciones();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [periodoSeleccionado]);

  /**
   * Carga las asignaciones del periodo seleccionado (o todas si no hay periodo)
   * y las normaliza al tipo local Asignacion para el estado del componente.
   */
  const cargarAsignaciones = async () => {
    setLoading(true);
    setError(null);
    try {
      let raw: any[];
      if (periodoSeleccionado) {
        raw = await manualAdjustmentService.obtenerPropuestas(periodoSeleccionado);
      } else {
        raw = await manualAdjustmentService.obtenerAsignaciones();
      }
      const list: any[] = Array.isArray(raw) ? raw : [];
      // El backend retorna 'idAsignacion' (camelCase) — lo mapeamos a 'id' para el frontend
      const mapped: Asignacion[] = list.map((a) => ({
        id: a.idAsignacion ?? a.id ?? "",
        idDocente: a.idDocente ?? "",
        idAsignatura: a.idAsignatura ?? "",
        codigoAsignatura: a.codigoAsignatura ?? "",
        nombreAsignatura: a.nombreAsignatura ?? "",
        nombreDocente: a.nombreDocente ?? "",
        dia: a.dia > 0 ? a.dia : null,
        horaInicio: a.horaInicio || null,
        horaFin: a.horaFin || null,
        estado: a.estado ?? "",
        escenario: a.escenario ?? "",
      }));
      setAsignaciones(mapped);
    } catch (err: any) {
      setError(err?.response?.data?.mensaje || "Error al cargar asignaciones");
    } finally {
      setLoading(false);
    }
  };

  /**
   * Obtiene y cachea las franjas de disponibilidad de un docente para evitar
   * llamadas repetidas durante la misma sesión de drag-and-drop.
   * Si el endpoint falla, retorna [] (modo permisivo: permite el drop).
   */
  const obtenerDisponibilidad = async (idDocente: string): Promise<DisponibilidadItem[]> => {
    if (dispCache.current.has(idDocente)) {
      return dispCache.current.get(idDocente)!;
    }
    try {
      const res = await api.get(`/profesores/${idDocente}/disponibilidad`);
      const list: DisponibilidadItem[] = Array.isArray(res.data) ? res.data : [];
      dispCache.current.set(idDocente, list);
      return list;
    } catch {
      return []; // no disponibilidades found → will block drop
    }
  };

  /**
   * Obtiene y cachea los IDs de asignaturas para las que un docente está habilitado.
   * Si el endpoint falla o devuelve lista vacía, retorna [] (modo permisivo: no bloquea el drop).
   */
  const obtenerHabilitados = async (idDocente: string): Promise<string[]> => {
    if (habCache.current.has(idDocente)) {
      return habCache.current.get(idDocente)!;
    }
    try {
      const res = await api.get(`/profesores/${idDocente}/habilitados`);
      const list: string[] = Array.isArray(res.data) ? res.data : [];
      habCache.current.set(idDocente, list);
      return list;
    } catch {
      return []; // sin datos → permisivo (no bloquea)
    }
  };

  /**
   * Verifica si el docente tiene disponibilidad para la franja propuesta.
   * Comprueba que la franja [horaInicio, horaInicio+duracion) quede completamente dentro
   * de alguna de sus disponibilidades declaradas para el día indicado.
   * Si la lista de disponibilidades está vacía, retorna true (modo permisivo).
   */
  const validarDisponibilidad = (
    disponibilidades: DisponibilidadItem[],
    dia: number,
    horaInicio: number,
    duracion: number
  ): boolean => {
    if (disponibilidades.length === 0) return true; // no data → allow (permissive fallback)
    const horaFin = horaInicio + duracion;
    return disponibilidades.some((d) => {
      if (d.diaSemana !== dia) return false;
      const dStart = parseInt(d.horaInicio.split(":")[0], 10);
      const dEnd   = parseInt(d.horaFin.split(":")[0], 10);
      return horaInicio >= dStart && horaFin <= dEnd;
    });
  };

  /**
   * Handler central de drag-and-drop: valida disponibilidad del docente y habilitación
   * de la asignatura, luego llama a PATCH /asignaciones/{id}/ajustar para persistir el cambio.
   * Muestra un toast de éxito o error según el resultado. Debounceado con el flag `dropping`.
   */
  const handleDrop = useCallback(
    async (item: DragItem, targetDay: number, targetHour: number) => {
      if (dropping) return;
      setDropping(true);

      const horaInicio = padHour(targetHour);
      const horaFin    = padHour(targetHour + item.duracion);

      try {
        // 1. Validate disponibilidad
        const disp = await obtenerDisponibilidad(item.idDocente);
        const ok   = validarDisponibilidad(disp, targetDay, targetHour, item.duracion);

        if (!ok) {
          const dayName = DAYS[targetDay - 1] ?? `día ${targetDay}`;
          showToast(
            `${item.nombreDocente} no tiene disponibilidad el ${dayName} a las ${horaInicio}`,
            "error"
          );
          return;
        }

        // 2. Validate habilitados: el docente debe ofertar la asignatura
        const asignacion = asignaciones.find((a) => a.id === item.id);
        if (asignacion?.idAsignatura) {
          const habilitados = await obtenerHabilitados(item.idDocente);
          if (habilitados.length > 0 && !habilitados.includes(asignacion.idAsignatura)) {
            showToast(
              `${item.nombreDocente} no oferta la asignatura "${asignacion.nombreAsignatura}"`,
              "error"
            );
            return;
          }
        }

        // 3. Call PATCH
        await manualAdjustmentService.ajustarAsignacion(item.id, {
          dia: targetDay,
          horaInicio,
          horaFin,
        });

        // 3. Refresh grid
        await cargarAsignaciones();
        showToast(`${item.nombreAsignatura} reubicada`, "success");
      } catch (err: any) {
        const msg =
          err?.response?.data?.mensaje ||
          err?.response?.data?.message ||
          err?.message ||
          "Error al reubicar la asignación";
        showToast(msg, "error");
      } finally {
        setDropping(false);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dropping, showToast]
  );

  // ── Derived state ───────────────────────────────────────────────────────────
  const asignacionesFiltradas = escenarioFiltro
    ? asignaciones.filter((a) => a.escenario === escenarioFiltro)
    : asignaciones;

  const pendingSubjects = asignacionesFiltradas.filter((a) => !a.dia);
  const assignedBlocks  = asignacionesFiltradas.filter((a) => !!a.dia);

  const periodoOptions = [
    { value: "", label: "Todos los periodos" },
    ...periodos.map((p) => ({ value: p, label: p })),
  ];

  if (loading && asignaciones.length === 0) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <div className="text-center">
          <Loader2 className="w-8 h-8 animate-spin text-[#1A6BBF] mx-auto" />
          <p className="mt-4 text-[#666666]">Cargando asignaciones...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Toast */}
      {toast && <Toast toast={toast} onClose={() => setToast(null)} />}

      {/* Overlay durante drop */}
      {dropping && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/10 pointer-events-none">
          <div className="bg-white rounded-lg shadow-lg px-4 py-2 flex items-center gap-2 text-sm text-[#333333]">
            <Loader2 className="animate-spin text-[#1A6BBF]" size={16} />
            Guardando...
          </div>
        </div>
      )}

      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Ajuste Manual de Propuesta</h1>
          <p className="text-sm text-[#666666] mt-1">
            Arrastra las asignaturas a la celda de día/hora deseada
          </p>
        </div>
        <div className="flex items-center gap-3 flex-wrap">
          {/* Period selector */}
          <Select
            value={periodoSeleccionado}
            onChange={(e) => {
              dispCache.current.clear();
              habCache.current.clear();
              setPeriodoSeleccionado(e.target.value);
            }}
            options={loadingPeriodos ? [{ value: "", label: "Cargando periodos..." }] : periodoOptions}
            className="w-44"
          />
          {/* Escenario filter */}
          <Select
            value={escenarioFiltro}
            onChange={(e) => setEscenarioFiltro(e.target.value)}
            options={ESCENARIOS_OPCIONES}
            className="w-52"
          />
          <Button
            variant="secondary"
            onClick={cargarAsignaciones}
            disabled={loading || dropping}
          >
            {loading ? <Loader2 className="animate-spin" size={16} /> : "Actualizar"}
          </Button>
        </div>
      </div>

      {error && (
        <div className="bg-[#C0392B]/10 border border-[#C0392B]/30 text-[#C0392B] rounded p-3 text-sm">
          {error}
        </div>
      )}

      <div className="grid grid-cols-12 gap-6">
        {/* ── Panel izquierdo — Asignaturas sin ubicar ─────────────────────── */}
        <div className="col-span-3">
          <Card>
            <CardHeader>
              <CardTitle>Asignaturas Pendientes</CardTitle>
              <p className="text-sm text-[#666666] mt-1">{pendingSubjects.length} por ubicar</p>
            </CardHeader>
            <CardContent>
              {pendingSubjects.length === 0 ? (
                <div className="text-center py-8 border-2 border-dashed border-[#CCCCCC] rounded">
                  <BookOpen className="mx-auto text-[#CCCCCC] mb-2" size={24} />
                  <p className="text-sm text-[#999999]">
                    {asignaciones.length === 0
                      ? "Sin propuesta activa"
                      : "Sin asignaturas pendientes"}
                  </p>
                </div>
              ) : (
                <div className="space-y-2">
                  {pendingSubjects.map((a) => (
                    <DraggablePendingCard
                      key={a.id}
                      asignacion={a}
                      isSelected={selectedId === a.id}
                      onClick={() => setSelectedId(a.id)}
                    />
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* ── Panel central — Cuadrícula semanal ───────────────────────────── */}
        <div className="col-span-6">
          <Card>
            <CardHeader>
              <CardTitle>Cuadrícula Semanal</CardTitle>
              <p className="text-sm text-[#666666] mt-1">
                Arrastra bloques para reubicarlos · Validación de disponibilidad automática
              </p>
            </CardHeader>
            <CardContent>
              <div className="border border-[#CCCCCC] rounded overflow-hidden bg-white">
                {/* Header row */}
                <div className="grid grid-cols-6 bg-[#333333]">
                  <div className="p-2 text-xs text-white font-medium text-center border-r border-white/20">
                    Hora
                  </div>
                  {DAYS.map((day) => (
                    <div
                      key={day}
                      className="p-2 text-xs text-white font-medium text-center border-r border-white/20 last:border-r-0"
                    >
                      {day}
                    </div>
                  ))}
                </div>

                {/* Grid body */}
                <div className="grid grid-cols-6" style={{ minHeight: "600px" }}>
                  {/* Hour labels column */}
                  <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                    {HOURS.map((hour) => (
                      <div
                        key={hour}
                        className="h-[42.85px] border-b border-[#CCCCCC] px-2 py-1 text-xs text-[#666666]"
                      >
                        {hour}:00
                      </div>
                    ))}
                  </div>

                  {/* Day columns */}
                  {DAYS.map((_, dayIdx) => (
                    <div
                      key={dayIdx}
                      className="border-r border-[#CCCCCC] last:border-r-0 relative"
                    >
                      {/* Drop cells (one per hour) */}
                      {HOURS.map((hour) => (
                        <DroppableCell
                          key={hour}
                          dayIdx={dayIdx}
                          hour={hour}
                          onDrop={handleDrop}
                        />
                      ))}

                      {/* Assigned blocks (draggable, absolutely positioned) */}
                      {assignedBlocks
                        .filter((a) => a.dia === dayIdx + 1)
                        .map((a) => {
                          const startH = parseHour(a.horaInicio);
                          const dur    = calcDuration(a.horaInicio, a.horaFin);
                          return (
                            <DraggableGridBlock
                              key={a.id}
                              asignacion={a}
                              style={{
                                top:    `${((startH - 7) / 14) * 100}%`,
                                height: `${(dur / 14) * 100}%`,
                              }}
                              isSelected={selectedId === a.id}
                              onClick={() =>
                                setSelectedId((prev) => (prev === a.id ? null : a.id))
                              }
                            />
                          );
                        })}
                    </div>
                  ))}
                </div>
              </div>

              {assignedBlocks.length === 0 && !loading && (
                <p className="text-center text-sm text-[#999999] mt-4 py-4">
                  No hay asignaciones ubicadas para el período seleccionado.
                </p>
              )}
            </CardContent>
          </Card>
        </div>

        {/* ── Panel derecho — Resumen + info del bloque seleccionado ────────── */}
        <div className="col-span-3 space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Resumen</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Estado de la propuesta</p>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Período</span>
                  <span className="font-medium text-[#333333]">{periodoSeleccionado || "Todos"}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Ubicadas</span>
                  <span className="font-medium text-[#1A7A4A]">{assignedBlocks.length}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Sin ubicar</span>
                  <span className="font-medium text-[#E8A020]">{pendingSubjects.length}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Total</span>
                  <span className="font-medium text-[#333333]">{asignacionesFiltradas.length}</span>
                </div>
              </div>

              {asignaciones.length === 0 && !loading && (
                <div className="mt-4 p-3 bg-[#F5F5F5] border border-[#CCCCCC] rounded text-xs text-[#666666]">
                  Sin propuesta activa. Ve a Generación para crear una.
                </div>
              )}
            </CardContent>
          </Card>

          {/* Detail card for selected block */}
          {selectedId && (() => {
            const sel = asignaciones.find((a) => a.id === selectedId);
            if (!sel) return null;
            return (
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Detalle</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2 text-sm">
                    <p className="font-medium text-[#333333]">{sel.nombreAsignatura}</p>
                    <p className="text-[#666666]">{sel.codigoAsignatura}</p>
                    <p className="text-[#666666]">👤 {sel.nombreDocente}</p>
                    {sel.dia ? (
                      <p className="text-[#666666]">
                        📅 {DAYS[(sel.dia ?? 1) - 1]} · {sel.horaInicio} – {sel.horaFin}
                      </p>
                    ) : (
                      <Badge variant="warning">Sin ubicar</Badge>
                    )}
                    <p className="text-xs text-[#999999]">Escenario: {sel.escenario}</p>
                  </div>
                </CardContent>
              </Card>
            );
          })()}

          {/* DnD instructions */}
          <Card>
            <CardContent className="py-4">
              <p className="text-xs text-[#666666] leading-relaxed">
                <strong>Cómo usar:</strong> Arrastra una asignatura pendiente (panel izquierdo) o un bloque
                ya ubicado directamente a la celda de día y hora que desees. La disponibilidad del docente se
                valida automáticamente antes de guardar.
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

// ─── Public export — wraps with DndProvider ────────────────────────────────────

/**
 * Vista pública de ajuste manual de horarios.
 * Envuelve ManualAdjustmentInner con el DndProvider de react-dnd (HTML5Backend),
 * que habilita la API de drag-and-drop nativa del navegador en toda la vista.
 */
export function ManualAdjustmentView() {
  return (
    <DndProvider backend={HTML5Backend}>
      <ManualAdjustmentInner />
    </DndProvider>
  );
}
