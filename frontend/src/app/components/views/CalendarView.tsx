import { useState, useEffect, useMemo } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Select } from "../Select";
import { Button } from "../Button";
import { Download, User, AlertCircle, RefreshCw, Loader2 } from "lucide-react";
import { calendarioService } from "../../../services/calendarioService";

// ─── Types ────────────────────────────────────────────────────────────────────

interface Props {
  onNavigate?: (view: string, state?: Record<string, string>) => void;
}

interface Bloque {
  idAsignacion: string;
  idDocente: string;
  horaInicio: string;
  horaFin: string;
  nombreAsignatura: string;
  codigoAsignatura: string;
  nombreDocente: string;
  semestreAsignatura: number;
  escenario: string;
  jornada: string;
  nombrePlan: string;
  idPlan: string;
  estado: string;
  // computed:
  day: number;      // 0-indexed
  hour: number;     // integer hour
  duration: number; // hours
}

interface Docente {
  idProfesor: string;
  nombre: string;
}

// ─── Constants ────────────────────────────────────────────────────────────────

const DAYS = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];

const ESCENARIOS = [
  { value: "",              label: "Todos los escenarios",  jornada: undefined,   maxSemestre: 12 },
  { value: "ING_DIURNA",   label: "Ingeniería Diurna",     jornada: "Diurna",    maxSemestre: 10 },
  { value: "ING_NOCTURNA", label: "Ingeniería Nocturna",   jornada: "Nocturna",  maxSemestre: 12 },
  { value: "TAPSI_DIURNA", label: "TAPSI Diurna",          jornada: "Diurna",    maxSemestre: 4  },
  { value: "TAPSI_NOCTURNA", label: "TAPSI Nocturna",      jornada: "Nocturna",  maxSemestre: 4  },
] as const;

const PERIODOS = [
  { value: "2026-1", label: "2026-1" },
  { value: "2026-2", label: "2026-2" },
  { value: "2025-2", label: "2025-2" },
  { value: "2025-1", label: "2025-1" },
];

// ─── Helpers ─────────────────────────────────────────────────────────────────

const toHour = (t: string) => (t ? parseInt(t.split(":")[0], 10) : 0);
const durHours = (inicio: string, fin: string) =>
  Math.max(toHour(fin) - toHour(inicio), 1);

// ─── Component ────────────────────────────────────────────────────────────────

export function CalendarView({ onNavigate }: Props) {
  const [periodo, setPeriodo]                   = useState("2026-1");
  const [scenarioFilter, setScenarioFilter]     = useState("");
  const [teacherFilter, setTeacherFilter]       = useState("");
  const [semestreFilter, setSemestreFilter]     = useState("");   // "" = all; "1" … "12"

  const [rawBlocks, setRawBlocks]               = useState<Bloque[]>([]);
  const [docentes, setDocentes]                 = useState<Docente[]>([]);
  const [loading, setLoading]                   = useState(false);
  const [error, setError]                       = useState<string | null>(null);

  // ── Derived: jornada + hour range ──────────────────────────────────────────
  const escenarioMeta = ESCENARIOS.find((e) => e.value === scenarioFilter) ?? ESCENARIOS[0];
  const isNocturna    = escenarioMeta.jornada === "Nocturna";
  const isDiurna      = escenarioMeta.jornada === "Diurna";

  const startHour  = isNocturna ? 18 : 7;
  const endHour    = isDiurna   ? 18 : 23;           // exclusive upper bound
  const numSlots   = endHour - startHour;
  const horasRango = Array.from({ length: numSlots }, (_, i) => i + startHour);

  const ROW_H_PX   = 57;   // pixels per 1-hour slot (same as before)

  // ── Semester options for selected plan ─────────────────────────────────────
  const semestreOpciones = useMemo(() => {
    const max = escenarioMeta.maxSemestre;
    return [
      { value: "", label: "Todos los semestres" },
      ...Array.from({ length: max }, (_, i) => ({
        value: String(i + 1),
        label: `Semestre ${i + 1}`,
      })),
    ];
  }, [escenarioMeta]);

  // Reset semestre when plan changes (selected semester may exceed new max)
  useEffect(() => {
    const max = escenarioMeta.maxSemestre;
    if (semestreFilter && parseInt(semestreFilter) > max) setSemestreFilter("");
  }, [scenarioFilter, escenarioMeta.maxSemestre, semestreFilter]);

  // ── Load docentes once ──────────────────────────────────────────────────────
  useEffect(() => {
    calendarioService.getDocentes()
      .then((data) => setDocentes(Array.isArray(data) ? data : []))
      .catch(() => {/* silently degrade */});
  }, []);

  // ── Load calendar when filters change ──────────────────────────────────────
  useEffect(() => {
    cargarCalendario();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [periodo, scenarioFilter, teacherFilter, semestreFilter]);

  const cargarCalendario = async () => {
    setLoading(true);
    setError(null);
    try {
      let dias: any[];

      if (teacherFilter) {
        // ── Teacher selected: use dedicated endpoint (queries by IdDocente, not name) ──
        const resp = await calendarioService.getCalendarioDocente(teacherFilter, periodo);
        dias = resp?.dias ?? [];
      } else {
        // ── General calendar with server-side jornada + semestre filters ──
        const semestreNum = semestreFilter ? parseInt(semestreFilter) : undefined;
        const resp = await calendarioService.getCalendarioSemanal(
          periodo,
          undefined,
          escenarioMeta.jornada,
          semestreNum
        );
        dias = resp?.dias ?? [];
      }

      // Flatten all bloques and attach computed display fields
      const blocks: Bloque[] = dias.flatMap((dia: any) =>
        (dia.bloques ?? []).map((b: any): Bloque => ({
          ...b,
          day:      (dia.numeroDia ?? 1) - 1,   // 0-indexed for grid
          hour:     toHour(b.horaInicio ?? ""),
          duration: durHours(b.horaInicio ?? "", b.horaFin ?? ""),
        }))
      );
      setRawBlocks(blocks);
    } catch (err: any) {
      setError(
        err?.response?.data?.mensaje ||
        err?.response?.data?.message ||
        "No se pudo cargar el calendario."
      );
    } finally {
      setLoading(false);
    }
  };

  // ── Client-side filters (escenario + semestre when teacher selected) ────────
  const scheduleBlocks = useMemo(() => {
    let filtered = rawBlocks;

    // Escenario: always filter client-side (server sends all jornada; need plan precision)
    if (scenarioFilter) {
      filtered = filtered.filter((b) => b.escenario === scenarioFilter);
    }

    // Semestre: when teacher selected, server didn't filter → apply client-side
    if (teacherFilter && semestreFilter) {
      filtered = filtered.filter(
        (b) => b.semestreAsignatura === parseInt(semestreFilter)
      );
    }

    // Restrict to visible hour range (blocks outside range won't be shown)
    filtered = filtered.filter(
      (b) => b.hour >= startHour && b.hour < endHour
    );

    return filtered;
  }, [rawBlocks, scenarioFilter, teacherFilter, semestreFilter, startHour, endHour]);

  // ── Block color ─────────────────────────────────────────────────────────────
  const blockColor = (b: Bloque) => {
    if (b.escenario?.startsWith("TAPSI")) return "bg-[#003087]";
    if (b.escenario?.includes("NOCTURNA")) return "bg-[#1A4A8A]";
    return "bg-[#1A6BBF]";
  };

  // ─── Render ──────────────────────────────────────────────────────────────────

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Calendario Semanal</h1>
          <p className="text-sm text-[#666666] mt-1">
            {loading ? "Cargando…" : `${scheduleBlocks.length} bloque${scheduleBlocks.length !== 1 ? "s" : ""} visibles`}
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" onClick={cargarCalendario} disabled={loading} className="gap-2">
            {loading ? <Loader2 size={18} className="animate-spin" /> : <RefreshCw size={18} />}
          </Button>
          <Button
            className="gap-2"
            onClick={() =>
              onNavigate?.("reportes", {
                semestre: periodo,
                ...(teacherFilter ? { idDocente: teacherFilter } : {}),
              })
            }
          >
            <Download size={18} />
            Exportar
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
            {/* Período académico */}
            <div>
              <label className="block text-xs text-[#666666] mb-1">Período</label>
              <Select
                value={periodo}
                onChange={(e) => setPeriodo(e.target.value)}
                options={PERIODOS}
              />
            </div>

            {/* Plan / Escenario */}
            <div>
              <label className="block text-xs text-[#666666] mb-1">Plan / Escenario</label>
              <Select
                value={scenarioFilter}
                onChange={(e) => {
                  setScenarioFilter(e.target.value);
                  setSemestreFilter(""); // reset semester when plan changes
                }}
                options={ESCENARIOS.map(({ value, label }) => ({ value, label }))}
              />
            </div>

            {/* Docente — by ID, uses dedicated endpoint */}
            <div>
              <label className="block text-xs text-[#666666] mb-1">Docente</label>
              <Select
                value={teacherFilter}
                onChange={(e) => setTeacherFilter(e.target.value)}
                options={[
                  { value: "", label: "Todos los docentes" },
                  ...docentes.map((d) => ({ value: d.idProfesor, label: d.nombre })),
                ]}
              />
            </div>

            {/* Semestre de la asignatura — options depend on selected plan */}
            <div>
              <label className="block text-xs text-[#666666] mb-1">Semestre</label>
              <Select
                value={semestreFilter}
                onChange={(e) => setSemestreFilter(e.target.value)}
                options={semestreOpciones}
              />
            </div>
          </div>

          {/* Active filters summary */}
          {(scenarioFilter || teacherFilter || semestreFilter) && (
            <div className="mt-3 flex flex-wrap gap-2 text-xs text-[#1A6BBF]">
              {scenarioFilter && (
                <span className="px-2 py-0.5 bg-[#1A6BBF]/10 rounded">
                  {ESCENARIOS.find((e) => e.value === scenarioFilter)?.label}
                </span>
              )}
              {teacherFilter && (
                <span className="px-2 py-0.5 bg-[#1A6BBF]/10 rounded">
                  {docentes.find((d) => d.idProfesor === teacherFilter)?.nombre ?? "Docente"}
                </span>
              )}
              {semestreFilter && (
                <span className="px-2 py-0.5 bg-[#1A6BBF]/10 rounded">
                  Semestre {semestreFilter}
                </span>
              )}
              <button
                onClick={() => { setScenarioFilter(""); setTeacherFilter(""); setSemestreFilter(""); }}
                className="px-2 py-0.5 text-[#C0392B] hover:underline"
              >
                Limpiar filtros
              </button>
            </div>
          )}
        </CardHeader>
      </Card>

      {/* Calendar grid */}
      <Card>
        <CardContent className="p-0 overflow-x-auto">
          {error ? (
            <div className="p-8 text-center">
              <AlertCircle size={40} className="mx-auto text-[#C0392B] mb-3" />
              <p className="text-[#C0392B] text-sm">{error}</p>
              <Button onClick={cargarCalendario} className="mt-4">Reintentar</Button>
            </div>
          ) : loading ? (
            <div className="flex items-center justify-center py-16 gap-3 text-[#666666]">
              <Loader2 className="animate-spin text-[#1A6BBF]" size={28} />
              <span>Cargando calendario...</span>
            </div>
          ) : scheduleBlocks.length === 0 ? (
            <div className="p-8 text-center">
              <p className="text-[#666666]">No hay bloques para los filtros seleccionados.</p>
              <p className="text-sm text-[#999999] mt-1">
                {rawBlocks.length > 0
                  ? `(${rawBlocks.length} bloque${rawBlocks.length !== 1 ? "s" : ""} excluido${rawBlocks.length !== 1 ? "s" : ""} por los filtros)`
                  : "Genera propuestas desde la sección de Generación."}
              </p>
            </div>
          ) : (
            <div className="border rounded-lg overflow-hidden min-w-[800px]">
              {/* Header row */}
              <div className="grid grid-cols-7 bg-[#333333]">
                <div className="p-3 text-white text-center text-xs font-medium border-r border-white/20">Hora</div>
                {DAYS.map((day) => (
                  <div key={day} className="p-3 text-white text-center text-xs font-medium border-r border-white/20 last:border-r-0">
                    {day}
                  </div>
                ))}
              </div>

              {/* Body */}
              <div
                className="grid grid-cols-7"
                style={{ minHeight: `${numSlots * ROW_H_PX}px` }}
              >
                {/* Hour labels */}
                <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                  {horasRango.map((h) => (
                    <div
                      key={h}
                      className="border-b border-[#CCCCCC] px-3 py-2 text-xs text-[#666666]"
                      style={{ height: ROW_H_PX }}
                    >
                      {h}:00
                    </div>
                  ))}
                  {/* Jornada label at end */}
                  <div className="px-3 py-1 text-[9px] text-[#AAAAAA]">
                    {isDiurna ? "18:00" : isNocturna ? "23:00" : "23:00"}
                  </div>
                </div>

                {/* Day columns */}
                {DAYS.map((_, dayIdx) => (
                  <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                    {horasRango.map((h) => (
                      <div
                        key={h}
                        className="border-b border-[#CCCCCC] bg-white"
                        style={{ height: ROW_H_PX }}
                      />
                    ))}

                    {scheduleBlocks
                      .filter((b) => b.day === dayIdx)
                      .map((block, idx) => {
                        // Position relative to the visible range
                        const topPct     = ((block.hour - startHour) / numSlots) * 100;
                        const heightPct  = (block.duration / numSlots) * 100;
                        return (
                          <div
                            key={idx}
                            title={`${block.nombreAsignatura}\n${block.nombreDocente}\n${block.horaInicio}–${block.horaFin}\nSem. ${block.semestreAsignatura}`}
                            className={`absolute ${blockColor(block)} text-white p-1 rounded text-xs left-0.5 right-0.5 overflow-hidden`}
                            style={{
                              top: `${topPct}%`,
                              height: `${Math.max(heightPct, 4)}%`,
                              minHeight: "28px",
                            }}
                          >
                            <div className="font-medium truncate text-[10px]">{block.nombreAsignatura}</div>
                            <div className="text-white/80 text-[9px] flex items-center gap-0.5 truncate">
                              <User size={7} /> {block.nombreDocente?.split(" ")[0]}
                            </div>
                            <div className="text-white/60 text-[8px]">
                              {block.horaInicio}–{block.horaFin}
                              {block.semestreAsignatura > 0 && ` · S${block.semestreAsignatura}`}
                            </div>
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

      {/* Jornada range info */}
      <div className="text-xs text-[#999999] text-center">
        {isDiurna
          ? "Jornada diurna · 07:00 – 18:00"
          : isNocturna
          ? "Jornada nocturna · 18:30 – 22:30"
          : "Rango completo · 07:00 – 23:00"}
      </div>

      {/* Leyenda */}
      <Card>
        <CardHeader><CardTitle>Leyenda</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-6 flex-wrap text-sm">
            <div className="flex items-center gap-2">
              <div className="w-5 h-5 rounded bg-[#1A6BBF]" />
              <span className="text-[#666666]">Ingeniería Diurna</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-5 h-5 rounded bg-[#1A4A8A]" />
              <span className="text-[#666666]">Ingeniería Nocturna</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-5 h-5 rounded bg-[#003087]" />
              <span className="text-[#666666]">TAPSI</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
