import { useEffect, useRef, useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import { ConfirmModal } from "../Modal";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Plus, Trash2, AlertTriangle, Loader2 } from "lucide-react";
import { blockedSlotsService } from "../../../services/blocked-slots.service";

// ─── Types ────────────────────────────────────────────────────────────────────

interface Bloqueo {
  idBloqueo: string;
  idAsignatura: string;
  codigoAsignatura: string;
  nombreAsignatura: string;
  periodo: string;
  dia: number;           // 1=Lunes … 5=Viernes
  diaNombre: string;
  horaInicio: string;    // "HH:mm"
  horaFin: string;       // "HH:mm"
  motivo?: string;
  fechaCreacionUtc: string;
}

/** Bloqueo deduplicado para mostrar en calendario y tabla (uno por franja). */
interface BloqueoUnico {
  key: string;           // dia|horaInicio|horaFin
  ids: string[];         // todos los idBloqueo del grupo
  dia: number;
  diaNombre: string;
  horaInicio: string;
  horaFin: string;
  motivo?: string;
  fechaCreacionUtc: string;
}

interface FormErrors {
  motivo?: string;
  dia?: string;
  horaInicio?: string;
  horaFin?: string;
}

// ─── Constants ────────────────────────────────────────────────────────────────

const PERIODO_ACTIVO = "2026-1";
const DAYS = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes"];
const HOURS = Array.from({ length: 14 }, (_, i) => i + 7); // 7–20

const parseHour = (t: string) => parseInt(t.split(":")[0], 10);
const padH = (h: number) => `${String(h).padStart(2, "0")}:00`;

function formatFecha(iso: string): string {
  try {
    return new Date(iso).toLocaleDateString("es-CO", {
      day: "2-digit", month: "2-digit", year: "numeric",
    });
  } catch {
    return iso;
  }
}

// ─── Component ────────────────────────────────────────────────────────────────

export function BlockedSlotsView() {
  // Data
  const [bloqueos, setBloqueos]         = useState<Bloqueo[]>([]);
  const [loadingData, setLoadingData]   = useState(true);

  // Form
  const [motivo, setMotivo]             = useState("");
  const [dia, setDia]                   = useState("");
  const [horaInicio, setHoraInicio]     = useState("");
  const [horaFin, setHoraFin]           = useState("");
  const [fieldErrors, setFieldErrors]   = useState<FormErrors>({});
  const [guardando, setGuardando]       = useState(false);
  const [formSuccess, setFormSuccess]   = useState<string | null>(null);

  // Conflict warning (after validation, before POST)
  const [conflictWarning, setConflictWarning] = useState<string | null>(null);
  const [pendingCreate, setPendingCreate]     = useState(false);

  // Delete modal — eliminar todos los registros de un grupo (franja)
  const [showDeleteModal, setShowDeleteModal]           = useState(false);
  const [selectedBloqueoKey, setSelectedBloqueoKey]     = useState<string | null>(null);
  const [eliminando, setEliminando]                     = useState(false);

  const formRef = useRef<HTMLDivElement>(null);

  // ── Deduplicar bloqueos: uno por (dia, horaInicio, horaFin) ─────────────────
  const bloqueoUnicos: BloqueoUnico[] = (() => {
    const map = new Map<string, BloqueoUnico>();
    for (const b of bloqueos) {
      const key = `${b.dia}|${b.horaInicio}|${b.horaFin}`;
      if (!map.has(key)) {
        map.set(key, {
          key,
          ids: [b.idBloqueo],
          dia: b.dia,
          diaNombre: b.diaNombre,
          horaInicio: b.horaInicio,
          horaFin: b.horaFin,
          motivo: b.motivo,
          fechaCreacionUtc: b.fechaCreacionUtc,
        });
      } else {
        map.get(key)!.ids.push(b.idBloqueo);
      }
    }
    return Array.from(map.values()).sort(
      (a, b) => a.dia - b.dia || a.horaInicio.localeCompare(b.horaInicio)
    );
  })();

  // ── Load data ───────────────────────────────────────────────────────────────
  useEffect(() => {
    cargarTodo();
  }, []);

  const cargarTodo = async () => {
    setLoadingData(true);
    try {
      const data = await blockedSlotsService.obtenerBloqueos(PERIODO_ACTIVO);
      setBloqueos(Array.isArray(data) ? data : []);
    } catch {
      // silently degrade
    } finally {
      setLoadingData(false);
    }
  };

  // ── Validation ──────────────────────────────────────────────────────────────
  const validar = (): boolean => {
    const errs: FormErrors = {};
    if (!motivo.trim())   errs.motivo     = "El motivo es obligatorio.";
    if (!dia)             errs.dia        = "Selecciona un día.";
    if (!horaInicio)      errs.horaInicio = "Selecciona la hora de inicio.";
    if (!horaFin)         errs.horaFin    = "Selecciona la hora de fin.";
    else if (horaInicio && parseInt(horaFin) <= parseInt(horaInicio)) {
      errs.horaFin = "La hora de fin debe ser posterior a la hora de inicio.";
    }
    setFieldErrors(errs);
    return Object.keys(errs).length === 0;
  };

  // ── Conflict check ──────────────────────────────────────────────────────────
  const verificarConflictos = async (): Promise<string | null> => {
    try {
      const res = await api.get(`/asignaciones/propuestas?periodo=${PERIODO_ACTIVO}`);
      const asignaciones: any[] = Array.isArray(res.data) ? res.data : [];
      const targetDia  = parseInt(dia);
      const startH     = parseInt(horaInicio);
      const endH       = parseInt(horaFin);

      const conflictos = asignaciones.filter((a) => {
        if (a.dia !== targetDia) return false;
        const aStart = parseHour(a.horaInicio ?? "");
        const aEnd   = parseHour(a.horaFin   ?? "");
        // Overlap: ranges intersect if start < other_end AND end > other_start
        return startH < aEnd && endH > aStart && a.estado === "Confirmada";
      });

      if (conflictos.length > 0) {
        const nombres = [...new Set(conflictos.map((c) => c.nombreAsignatura as string))]
          .slice(0, 3)
          .join(", ");
        return `Hay ${conflictos.length} asignación(es) confirmada(s) en esa franja: ${nombres}. El bloqueo aplicará a futuros horarios.`;
      }
    } catch {
      // couldn't check — proceed anyway
    }
    return null;
  };

  // ── Submit (may be called twice if conflict warning is shown) ───────────────
  const handleCrearBloqueo = async (ignorarConflicto = false) => {
    if (!validar()) return;

    // First pass: check conflict, then show warning and ask confirmation
    if (!ignorarConflicto) {
      const warning = await verificarConflictos();
      if (warning) {
        setConflictWarning(warning);
        setPendingCreate(true);
        return;
      }
    }

    // Actually create (global — aplica a todos los escenarios)
    setGuardando(true);
    setFieldErrors({});
    setFormSuccess(null);
    setConflictWarning(null);
    setPendingCreate(false);

    try {
      await blockedSlotsService.crearBloqueoGlobal({
        periodo: PERIODO_ACTIVO,
        dia: parseInt(dia),
        horaInicio: padH(parseInt(horaInicio)),
        horaFin: padH(parseInt(horaFin)),
        motivo: motivo.trim() || undefined,
      });

      // Reset form
      setMotivo("");
      setDia("");
      setHoraInicio("");
      setHoraFin("");

      setFormSuccess(`Bloqueo global creado: ${DAYS[parseInt(dia) - 1]} ${padH(parseInt(horaInicio))}–${padH(parseInt(horaFin))}.`);
      setTimeout(() => setFormSuccess(null), 5000);

      await cargarTodo();
    } catch (err: any) {
      const msg = err?.response?.data?.mensaje || err?.message || "Error al crear el bloqueo.";
      setFieldErrors({ motivo: msg });
    } finally {
      setGuardando(false);
    }
  };

  // ── Delete — elimina todos los registros del grupo (franja global) ──────────
  const handleEliminar = async () => {
    if (!selectedBloqueoKey) return;
    const grupo = bloqueoUnicos.find((b) => b.key === selectedBloqueoKey);
    if (!grupo) return;
    setEliminando(true);
    try {
      await Promise.all(grupo.ids.map((id) => blockedSlotsService.eliminarBloqueo(id)));
      setShowDeleteModal(false);
      setSelectedBloqueoKey(null);
      await cargarTodo();
    } catch (err: any) {
      setFormError(err?.response?.data?.mensaje || "Error al eliminar el bloqueo.");
    } finally {
      setEliminando(false);
    }
  };

  // ── Header button: focus form ───────────────────────────────────────────────
  const focusForm = () => {
    formRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    // Give browsers a moment then try to focus the first input
    setTimeout(() => {
      const first = formRef.current?.querySelector("input, select") as HTMLElement | null;
      first?.focus();
    }, 400);
  };

  // ─── Render ──────────────────────────────────────────────────────────────────

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Franjas Horarias Bloqueadas</h1>
          <p className="text-sm text-[#666666] mt-1">
            Bloqueos globales — ningún escenario puede tener clases en estas franjas · {PERIODO_ACTIVO}
          </p>
        </div>
        <Button className="gap-2" onClick={focusForm}>
          <Plus size={20} />
          Crear bloqueo
        </Button>
      </div>

      {/* Main grid */}
      <div className="grid grid-cols-3 gap-6">
        {/* Calendar */}
        <div className="col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Calendario Semanal – Vista de Bloqueos</CardTitle>
              <p className="text-sm text-[#666666] mt-1">
                Franjas bloqueadas con patrón rayado · {bloqueoUnicos.length} franja(s) bloqueada(s)
              </p>
            </CardHeader>
            <CardContent>
              {loadingData ? (
                <div className="flex items-center justify-center py-12 gap-2 text-[#666666]">
                  <Loader2 className="animate-spin text-[#1A6BBF]" size={22} />
                  <span className="text-sm">Cargando calendario...</span>
                </div>
              ) : (
                <div className="border border-[#CCCCCC] rounded overflow-hidden bg-white">
                  {/* Header row */}
                  <div className="grid grid-cols-6 bg-[#333333]">
                    <div className="p-2 text-xs text-white font-medium text-center border-r border-white/20">Hora</div>
                    {DAYS.map((day) => (
                      <div key={day} className="p-2 text-xs text-white font-medium text-center border-r border-white/20 last:border-r-0">
                        {day}
                      </div>
                    ))}
                  </div>

                  {/* Grid body */}
                  <div className="grid grid-cols-6" style={{ minHeight: "600px" }}>
                    {/* Hour labels */}
                    <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                      {HOURS.map((h) => (
                        <div key={h} className="h-[42.85px] border-b border-[#CCCCCC] px-2 py-1 text-xs text-[#666666]">
                          {h}:00
                        </div>
                      ))}
                    </div>

                    {/* Day columns — dayIdx 0=Lunes, dia backend 1=Lunes */}
                    {DAYS.map((_, dayIdx) => (
                      <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                        {HOURS.map((h) => (
                          <div key={h} className="h-[42.85px] border-b border-[#CCCCCC]" />
                        ))}

                        {bloqueoUnicos
                          .filter((b) => b.dia === dayIdx + 1)   // backend 1-indexed
                          .map((b) => {
                            const startH = parseHour(b.horaInicio);
                            const endH   = parseHour(b.horaFin);
                            const dur    = Math.max(endH - startH, 1);
                            return (
                              <div
                                key={b.key}
                                className="absolute left-1 right-1 rounded overflow-hidden cursor-pointer hover:shadow-lg transition-shadow text-white text-xs p-1.5 border border-[#333333]"
                                style={{
                                  top: `${((startH - 7) / 14) * 100}%`,
                                  height: `${(dur / 14) * 100}%`,
                                  backgroundImage:
                                    "repeating-linear-gradient(45deg,#595959,#595959 8px,#6e6e6e 8px,#6e6e6e 16px)",
                                }}
                                onClick={() => {
                                  setSelectedBloqueoKey(b.key);
                                  setShowDeleteModal(true);
                                }}
                                title={b.motivo ?? "Bloqueo global"}
                              >
                                <div className="font-medium truncate text-[10px]">BLOQUEADO</div>
                                <div className="text-white/80 text-[9px] truncate">{b.horaInicio}–{b.horaFin}</div>
                              </div>
                            );
                          })}
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Form panel */}
        <div ref={formRef}>
          <Card>
            <CardHeader>
              <CardTitle>Nuevo Bloqueo Global</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Aplica a todos los escenarios · {PERIODO_ACTIVO}</p>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">

                {/* Success banner */}
                {formSuccess && (
                  <div className="p-3 bg-[#1A7A4A]/10 border border-[#1A7A4A]/30 text-[#1A7A4A] rounded text-sm">
                    {formSuccess}
                  </div>
                )}

                {/* Conflict warning with confirm */}
                {conflictWarning && pendingCreate && (
                  <div className="p-3 bg-[#E8A020]/10 border border-[#E8A020]/40 rounded">
                    <div className="flex gap-2 mb-2">
                      <AlertTriangle className="text-[#E8A020] shrink-0 mt-0.5" size={16} />
                      <p className="text-xs text-[#666666]">{conflictWarning}</p>
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={() => handleCrearBloqueo(true)}
                        disabled={guardando}
                        className="flex-1 py-1.5 text-xs font-medium bg-[#E8A020] text-white rounded hover:bg-[#C8801A] transition-colors disabled:opacity-50"
                      >
                        {guardando ? "Creando…" : "Crear de todas formas"}
                      </button>
                      <button
                        onClick={() => { setConflictWarning(null); setPendingCreate(false); }}
                        className="flex-1 py-1.5 text-xs font-medium bg-[#F5F5F5] text-[#666666] border border-[#CCCCCC] rounded hover:bg-[#E8E8E8] transition-colors"
                      >
                        Cancelar
                      </button>
                    </div>
                  </div>
                )}

                {/* Motivo */}
                <div>
                  <label className="block text-sm font-medium text-[#333333] mb-1">
                    Motivo <span className="text-[#C0392B]">*</span>
                  </label>
                  <Input
                    placeholder="Ej: Mantenimiento, Evento institucional…"
                    value={motivo}
                    onChange={(e) => { setMotivo(e.target.value); setFieldErrors((p) => ({ ...p, motivo: undefined })); }}
                  />
                  {fieldErrors.motivo && (
                    <p className="mt-1 text-xs text-[#C0392B]">{fieldErrors.motivo}</p>
                  )}
                </div>

                {/* Día */}
                <div>
                  <label className="block text-sm font-medium text-[#333333] mb-1">
                    Día <span className="text-[#C0392B]">*</span>
                  </label>
                  <Select
                    value={dia}
                    onChange={(e) => { setDia(e.target.value); setFieldErrors((p) => ({ ...p, dia: undefined })); }}
                    options={[
                      { value: "", label: "— Seleccionar día —" },
                      ...DAYS.map((d, i) => ({ value: String(i + 1), label: d })), // 1-indexed for backend
                    ]}
                  />
                  {fieldErrors.dia && (
                    <p className="mt-1 text-xs text-[#C0392B]">{fieldErrors.dia}</p>
                  )}
                </div>

                {/* Hora inicio / fin */}
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-sm font-medium text-[#333333] mb-1">
                      Hora inicio <span className="text-[#C0392B]">*</span>
                    </label>
                    <Select
                      value={horaInicio}
                      onChange={(e) => { setHoraInicio(e.target.value); setFieldErrors((p) => ({ ...p, horaInicio: undefined, horaFin: undefined })); }}
                      options={[
                        { value: "", label: "Inicio" },
                        ...HOURS.map((h) => ({ value: String(h), label: `${h}:00` })),
                      ]}
                    />
                    {fieldErrors.horaInicio && (
                      <p className="mt-1 text-xs text-[#C0392B]">{fieldErrors.horaInicio}</p>
                    )}
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-[#333333] mb-1">
                      Hora fin <span className="text-[#C0392B]">*</span>
                    </label>
                    <Select
                      value={horaFin}
                      onChange={(e) => { setHoraFin(e.target.value); setFieldErrors((p) => ({ ...p, horaFin: undefined })); }}
                      options={[
                        { value: "", label: "Fin" },
                        ...HOURS.filter((h) => !horaInicio || h > parseInt(horaInicio)).map((h) => ({
                          value: String(h),
                          label: `${h}:00`,
                        })),
                      ]}
                    />
                    {fieldErrors.horaFin && (
                      <p className="mt-1 text-xs text-[#C0392B]">{fieldErrors.horaFin}</p>
                    )}
                  </div>
                </div>

                {/* Submit */}
                <Button
                  className="w-full"
                  onClick={() => handleCrearBloqueo(false)}
                  disabled={guardando || pendingCreate}
                >
                  {guardando
                    ? <><Loader2 size={14} className="animate-spin mr-2" />Creando…</>
                    : <><Plus size={16} className="mr-2" />Crear bloqueo</>}
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Active blocks table */}
      <Card>
        <CardHeader>
          <CardTitle>Bloqueos Activos</CardTitle>
          <p className="text-sm text-[#666666] mt-1">
            {bloqueoUnicos.length} franja{bloqueoUnicos.length !== 1 ? "s" : ""} bloqueada{bloqueoUnicos.length !== 1 ? "s" : ""} en {PERIODO_ACTIVO}
          </p>
        </CardHeader>
        <CardContent>
          {bloqueoUnicos.length === 0 ? (
            <div className="text-center py-8 text-[#999999] text-sm">
              No hay bloqueos registrados para este período.
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Motivo</TableHead>
                  <TableHead>Día</TableHead>
                  <TableHead>Horario</TableHead>
                  <TableHead>Creado</TableHead>
                  <TableHead>Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {bloqueoUnicos.map((b) => (
                  <TableRow key={b.key} striped>
                    <TableCell className="text-[#666666]">
                      {b.motivo ?? <span className="text-[#AAAAAA] italic">Sin motivo</span>}
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary" className="text-xs">{b.diaNombre}</Badge>
                    </TableCell>
                    <TableCell className="text-[#666666] font-mono text-xs">
                      {b.horaInicio} – {b.horaFin}
                    </TableCell>
                    <TableCell className="text-[#999999] text-xs">
                      {formatFecha(b.fechaCreacionUtc)}
                    </TableCell>
                    <TableCell>
                      <button
                        onClick={() => {
                          setSelectedBloqueoKey(b.key);
                          setShowDeleteModal(true);
                        }}
                        className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#C0392B]"
                        title="Eliminar bloqueo"
                      >
                        <Trash2 size={16} />
                      </button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Delete confirmation */}
      <ConfirmModal
        isOpen={showDeleteModal}
        onClose={() => { setShowDeleteModal(false); setSelectedBloqueoKey(null); }}
        onConfirm={handleEliminar}
        title="Eliminar bloqueo global"
        message="¿Está seguro? Esta acción elimina el bloqueo en todos los escenarios para esa franja horaria."
        confirmText={eliminando ? "Eliminando…" : "Eliminar"}
        cancelText="Cancelar"
        variant="danger"
      />
    </div>
  );
}
