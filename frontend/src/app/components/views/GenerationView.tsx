import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { ProgressBar } from "../ProgressBar";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Sparkles, CheckCircle, Clock, Loader2, RefreshCw } from "lucide-react";
import { useEffect, useState } from "react";
import { generationService } from "../../../services/generation.service";
import api from "../../../services/api";

const PERIODO_ACTIVO = "2026-1";

const ESCENARIOS_FIJOS = [
  { id: "ING_DIURNA",     nombre: "Ingeniería Diurna" },
  { id: "ING_NOCTURNA",   nombre: "Ingeniería Nocturna" },
  { id: "TAPSI_DIURNA",   nombre: "TAPSI Diurna" },
  { id: "TAPSI_NOCTURNA", nombre: "TAPSI Nocturna" },
];

interface EscenarioStats {
  assigned: number;
}

interface HistoricoRow {
  periodo: string;
  escenario: string;
  total: number;
  estado: string;
}

export function GenerationView() {
  const [statsMap, setStatsMap]                   = useState<Record<string, EscenarioStats>>({});
  const [loadingStats, setLoadingStats]           = useState(true);
  const [errorStats, setErrorStats]               = useState<string | null>(null);
  const [generandoEscenario, setGenerandoEscenario] = useState<string | null>(null);
  const [mensajeGeneracion, setMensajeGeneracion] = useState<{ texto: string; ok: boolean } | null>(null);
  const [historico, setHistorico]                 = useState<HistoricoRow[]>([]);
  const [loadingHistorico, setLoadingHistorico]   = useState(false);

  useEffect(() => {
    cargarPropuestas();
    cargarHistorico();
  }, []);

  // ── Carga stats del periodo activo ────────────────────────────────────────
  const cargarPropuestas = async () => {
    setLoadingStats(true);
    setErrorStats(null);
    try {
      const data: any[] = await generationService.obtenerPropuestas(PERIODO_ACTIVO);
      const map: Record<string, EscenarioStats> = {};
      const list = Array.isArray(data) ? data : [];
      for (const item of list) {
        const esc: string = item.escenario ?? "";
        if (!map[esc]) map[esc] = { assigned: 0 };
        map[esc].assigned++;
      }
      setStatsMap(map);
    } catch {
      setErrorStats("No se pudieron cargar las propuestas del servidor.");
    } finally {
      setLoadingStats(false);
    }
  };

  // ── Carga historial de todos los periodos ────────────────────────────────
  const cargarHistorico = async () => {
    setLoadingHistorico(true);
    try {
      const res = await api.get("/asignaciones/periodos-historicos");
      const periodos: string[] = Array.isArray(res.data) ? res.data : [];

      const rows: HistoricoRow[] = [];
      await Promise.allSettled(
        periodos.map(async (periodo) => {
          try {
            const propuestas = await generationService.obtenerPropuestas(periodo);
            const list = Array.isArray(propuestas) ? propuestas : [];
            for (const esc of ESCENARIOS_FIJOS) {
              const grupo = list.filter((p: any) => p.escenario === esc.id);
              if (grupo.length > 0) {
                rows.push({
                  periodo,
                  escenario: esc.nombre,
                  total: grupo.length,
                  estado: grupo[0]?.estado ?? "Propuesta",
                });
              }
            }
          } catch {}
        })
      );

      rows.sort((a, b) =>
        b.periodo.localeCompare(a.periodo) || a.escenario.localeCompare(b.escenario)
      );
      setHistorico(rows);
    } catch {}
    finally { setLoadingHistorico(false); }
  };

  // ── Generación por escenario individual ──────────────────────────────────
  const handleGenerarEscenario = async (escId: string) => {
    setGenerandoEscenario(escId);
    setMensajeGeneracion(null);
    try {
      const result = await generationService.generarPropuestas({
        periodo: PERIODO_ACTIVO,
        escenarios: [escId],
        semestreIngenieria: 1,          // el backend procesa todos los semestres automáticamente
        borrarPropuestasPrevias: true,
      });
      const nombre = ESCENARIOS_FIJOS.find((e) => e.id === escId)?.nombre ?? escId;
      setMensajeGeneracion({
        ok: true,
        texto: `${nombre}: ${result.totalPropuestasCreadas ?? 0} sesiones generadas, ` +
               `${result.totalAsignaturasNoAsignadas ?? 0} sin ubicar.`,
      });
      await cargarPropuestas();
      await cargarHistorico();
    } catch (err: any) {
      setMensajeGeneracion({
        ok: false,
        texto: "Error: " + (err?.response?.data?.mensaje || err?.message || "Error desconocido"),
      });
    } finally {
      setGenerandoEscenario(null);
    }
  };

  // ── Generación de todos los escenarios ───────────────────────────────────
  const handleGenerarTodos = async () => {
    setGenerandoEscenario("TODOS");
    setMensajeGeneracion(null);
    try {
      const result = await generationService.generarPropuestas({
        periodo: PERIODO_ACTIVO,
        escenarios: [],                 // [] = todos los escenarios en el backend
        semestreIngenieria: 1,
        borrarPropuestasPrevias: true,
      });
      setMensajeGeneracion({
        ok: true,
        texto: `Todos los escenarios: ${result.totalPropuestasCreadas ?? 0} sesiones generadas, ` +
               `${result.totalAsignaturasNoAsignadas ?? 0} sin ubicar.`,
      });
      await cargarPropuestas();
      await cargarHistorico();
    } catch (err: any) {
      setMensajeGeneracion({
        ok: false,
        texto: "Error: " + (err?.response?.data?.mensaje || err?.message || "Error desconocido"),
      });
    } finally {
      setGenerandoEscenario(null);
    }
  };

  const totalAssigned = Object.values(statsMap).reduce((s, e) => s + e.assigned, 0);
  const hayGenerando   = generandoEscenario !== null;

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Generación Automática de Horarios</h1>
          <p className="text-sm text-[#666666] mt-1">
            Motor inteligente de asignación · Período activo: {PERIODO_ACTIVO}
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" onClick={() => { cargarPropuestas(); cargarHistorico(); }} className="gap-2" disabled={hayGenerando}>
            <RefreshCw size={18} />
          </Button>
          <Button className="gap-2" onClick={handleGenerarTodos} disabled={hayGenerando}>
            {generandoEscenario === "TODOS"
              ? <><Loader2 size={18} className="animate-spin" /> Generando todos...</>
              : <><Sparkles size={18} /> Generar todos</>}
          </Button>
        </div>
      </div>

      {/* Mensaje de resultado */}
      {mensajeGeneracion && (
        <div className={`rounded p-3 text-sm border ${
          mensajeGeneracion.ok
            ? "bg-[#1A7A4A]/10 border-[#1A7A4A]/30 text-[#1A7A4A]"
            : "bg-[#C0392B]/10 border-[#C0392B]/30 text-[#C0392B]"
        }`}>
          {mensajeGeneracion.texto}
        </div>
      )}

      {errorStats && (
        <div className="bg-[#C0392B]/10 border border-[#C0392B]/30 text-[#C0392B] rounded p-3 text-sm">
          {errorStats}
        </div>
      )}

      {/* 4 tarjetas fijas de escenarios */}
      <div className="grid grid-cols-4 gap-6">
        {ESCENARIOS_FIJOS.map((esc) => {
          const isGenerandoThis = generandoEscenario === esc.id;
          const stats    = statsMap[esc.id];
          const assigned = stats?.assigned ?? 0;
          const hasData  = assigned > 0;

          return (
            <Card key={esc.id}>
              <CardHeader>
                <div className="flex items-start justify-between mb-3">
                  <CardTitle className="text-base">{esc.nombre}</CardTitle>
                  {loadingStats
                    ? <Loader2 className="animate-spin text-[#1A6BBF]" size={18} />
                    : hasData
                      ? <CheckCircle className="text-[#1A7A4A]" size={20} />
                      : <Clock className="text-[#999999]" size={20} />}
                </div>
                <Badge variant={hasData ? "success" : "inactive"}>
                  {hasData ? "Con propuesta" : "Sin datos"}
                </Badge>
              </CardHeader>

              <CardContent>
                <div className="space-y-3">
                  <ProgressBar
                    value={hasData ? 100 : 0}
                    variant={hasData ? "success" : "warning"}
                  />
                  <div className="flex justify-between text-sm">
                    <span className="text-[#666666]">Sesiones</span>
                    <span className="text-[#333333] font-medium">
                      {loadingStats ? "…" : assigned}
                    </span>
                  </div>

                  {/* Botón generar por escenario */}
                  <div className="pt-2 border-t border-[#CCCCCC]">
                    <button
                      onClick={() => handleGenerarEscenario(esc.id)}
                      disabled={hayGenerando}
                      className="w-full flex items-center justify-center gap-2 px-3 py-2 text-sm font-medium
                        text-[#1A6BBF] border border-[#1A6BBF] rounded hover:bg-[#1A6BBF] hover:text-white
                        transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                    >
                      {isGenerandoThis
                        ? <><Loader2 size={14} className="animate-spin" /> Generando...</>
                        : <><Sparkles size={14} /> Generar</>}
                    </button>
                  </div>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Resumen global (solo si hay propuestas) */}
      {totalAssigned > 0 && !loadingStats && (
        <Card>
          <CardContent className="flex items-center gap-4 py-4">
            <CheckCircle className="text-[#1A7A4A] shrink-0" size={28} />
            <div>
              <p className="text-sm text-[#666666]">Total sesiones en propuesta activa ({PERIODO_ACTIVO})</p>
              <p className="text-2xl font-medium text-[#1A7A4A]">{totalAssigned}</p>
            </div>
            <p className="text-xs text-[#999999] ml-auto">
              Revisa en Ajuste Manual antes de confirmar.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Historial de generaciones */}
      <Card>
        <CardHeader>
          <CardTitle>Historial de Generaciones</CardTitle>
          <p className="text-sm text-[#666666] mt-1">Propuestas registradas por período y escenario</p>
        </CardHeader>
        <CardContent>
          {loadingHistorico ? (
            <div className="flex items-center justify-center py-8 gap-2 text-[#666666]">
              <Loader2 className="animate-spin text-[#1A6BBF]" size={22} />
              <span className="text-sm">Cargando historial...</span>
            </div>
          ) : historico.length === 0 ? (
            <div className="text-center py-8 text-[#999999] text-sm">
              No hay generaciones registradas.
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Período</TableHead>
                  <TableHead>Escenario</TableHead>
                  <TableHead>Total sesiones</TableHead>
                  <TableHead>Estado</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {historico.map((row, idx) => (
                  <TableRow key={idx} striped>
                    <TableCell className="font-medium text-[#333333]">{row.periodo}</TableCell>
                    <TableCell className="text-[#666666]">{row.escenario}</TableCell>
                    <TableCell className="text-[#666666]">{row.total}</TableCell>
                    <TableCell>
                      <Badge variant={
                        row.estado === "Confirmada" ? "success"
                        : row.estado === "Propuesta" ? "primary"
                        : "inactive"
                      }>
                        {row.estado}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
