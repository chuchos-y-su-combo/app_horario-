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

const DRAG_TYPE = "ASIGNACION";

interface Asignacion {
  id: string;
  idDocente: string;
  idAsignatura: string;
  codigoAsignatura: string;
  nombreAsignatura: string;
  nombreDocente: string;
  dia: number | null;
  horaInicio: string | null;
  horaFin: string | null;
  estado: string;
  escenario: string;
}

interface DragItem {
  id: string;
  idDocente: string;
  nombreAsignatura: string;
  nombreDocente: string;
  duracion: number;   // hours
  fromDia: number | null;
}

interface DisponibilidadItem {
  diaSemana: number;
  horaInicio: string;
  horaFin: string;
}

interface ToastState {
  message: string;
  type: "success" | "error";
}

// ─── Constants ────────────────────────────────────────────────────────────────

const DAYS = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
const HOURS = Array.from({ length: 14 }, (_, i) => i + 7); // 07 – 20
const ESCENARIOS_OPCIONES = [
  { value: "",              label: "Todos los escenarios" },
  { value: "ING_DIURNA",   label: "Ingeniería Diurna" },
  { value: "ING_NOCTURNA", label: "Ingeniería Nocturna" },
  { value: "TAPSI_DIURNA", label: "TAPSI Diurna" },
  { value: "TAPSI_NOCTURNA", label: "TAPSI Nocturna" },
];

// ─── Helpers ─────────────────────────────────────────────────────────────────

const parseHour = (hora: string | null) =>
  hora ? parseInt(hora.split(":")[0], 10) : 7;

const calcDuration = (inicio: string | null, fin: string | null) => {
  if (!inicio || !fin) return 2;
  return Math.max(parseHour(fin) - parseHour(inicio), 1);
};

const padHour = (h: number) => `${String(h).padStart(2, "0")}:00`;

// ─── DraggablePendingCard ─────────────────────────────────────────────────────

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

  const cargarAsignaciones = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: Asignacion[];
      if (periodoSeleccionado) {
        data = await manualAdjustmentService.obtenerPropuestas(periodoSeleccionado);
      } else {
        data = await manualAdjustmentService.obtenerAsignaciones();
      }
      setAsignaciones(Array.isArray(data) ? data : []);
    } catch (err: any) {
      setError(err?.response?.data?.mensaje || "Error al cargar asignaciones");
    } finally {
      setLoading(false);
    }
  };

  // ── Disponibilidad (cached per docente) ────────────────────────────────────
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

  // ── Drop handler ────────────────────────────────────────────────────────────
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
            `${item.nombreDocente} no disponible el ${dayName} a las ${horaInicio}`,
            "error"
          );
          return;
        }

        // 2. Call PATCH
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
                <div className="grid grid-cols-7 bg-[#333333]">
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
                <div className="grid grid-cols-7" style={{ minHeight: "600px" }}>
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

export function ManualAdjustmentView() {
  return (
    <DndProvider backend={HTML5Backend}>
      <ManualAdjustmentInner />
    </DndProvider>
  );
}
