import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import { ProgressBar } from "../ProgressBar";
import { Sparkles, CheckCircle, AlertTriangle, Clock, Loader2 } from "lucide-react";
import { useEffect, useState } from "react";
import { generationService } from "../../../services/generation.service";

const ESCENARIOS_FIJOS = [
  { id: "ING_DIURNA",    nombre: "Ingeniería Diurna" },
  { id: "ING_NOCTURNA",  nombre: "Ingeniería Nocturna" },
  { id: "TAPSI_DIURNA",  nombre: "TAPSI Diurna" },
  { id: "TAPSI_NOCTURNA",nombre: "TAPSI Nocturna" },
];

interface EscenarioStats {
  assigned: number;
  total: number;
}

export function GenerationView() {
  const [statsMap, setStatsMap] = useState<Record<string, EscenarioStats>>({});
  const [loadingStats, setLoadingStats] = useState(true);
  const [errorStats, setErrorStats] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [generando, setGenerando] = useState(false);
  const [periodo, setPeriodo] = useState("2026-1");
  const [semestreIngenieria, setSemestreIngenieria] = useState("1");
  const [mensajeGeneracion, setMensajeGeneracion] = useState<string | null>(null);

  useEffect(() => {
    cargarPropuestas();
  }, []);

  const cargarPropuestas = async () => {
    setLoadingStats(true);
    setErrorStats(null);
    try {
      const data: any[] = await generationService.obtenerPropuestas("2026-1");
      const map: Record<string, EscenarioStats> = {};
      const list = Array.isArray(data) ? data : [];
      for (const item of list) {
        const esc: string = item.escenario ?? "";
        if (!map[esc]) map[esc] = { assigned: 0, total: 0 };
        map[esc].assigned++;
      }
      setStatsMap(map);
    } catch {
      setErrorStats("No se pudieron cargar las propuestas del servidor.");
    } finally {
      setLoadingStats(false);
    }
  };

  const handleGenerar = async () => {
    setGenerando(true);
    setMensajeGeneracion(null);
    try {
      const result = await generationService.generarPropuestas({
        periodo,
        escenarios: [],
        semestreIngenieria: parseInt(semestreIngenieria, 10),
        borrarPropuestasPrevias: true,
      });
      setMensajeGeneracion(
        `Generación completada: ${result.totalPropuestasCreadas ?? 0} propuestas creadas, ` +
        `${result.totalAsignaturasNoAsignadas ?? 0} sin ubicar.`
      );
      await cargarPropuestas();
      setShowModal(false);
    } catch (err: any) {
      setMensajeGeneracion(
        "Error: " + (err?.response?.data?.mensaje || err?.message || "Error desconocido")
      );
    } finally {
      setGenerando(false);
    }
  };

  const totalAssigned = Object.values(statsMap).reduce((s, e) => s + e.assigned, 0);

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Generación Automática de Horarios</h1>
          <p className="text-sm text-[#666666] mt-1">Motor inteligente de asignación con validación de restricciones</p>
        </div>
        <Button className="gap-2" onClick={() => setShowModal(true)}>
          <Sparkles size={20} />
          Generar horarios
        </Button>
      </div>

      {mensajeGeneracion && (
        <div className={`rounded p-3 text-sm ${mensajeGeneracion.startsWith("Error") ? "bg-[#C0392B]/10 border border-[#C0392B]/30 text-[#C0392B]" : "bg-[#1A7A4A]/10 border border-[#1A7A4A]/30 text-[#1A7A4A]"}`}>
          {mensajeGeneracion}
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
          if (loadingStats) {
            return (
              <Card key={esc.id}>
                <CardHeader>
                  <CardTitle className="text-base">{esc.nombre}</CardTitle>
                </CardHeader>
                <CardContent className="flex items-center justify-center h-28">
                  <Loader2 className="animate-spin text-[#1A6BBF]" size={24} />
                </CardContent>
              </Card>
            );
          }

          const stats = statsMap[esc.id];
          const assigned = stats?.assigned ?? 0;
          const hasData = assigned > 0;

          return (
            <Card key={esc.id}>
              <CardHeader>
                <div className="flex items-start justify-between mb-3">
                  <CardTitle className="text-base">{esc.nombre}</CardTitle>
                  {hasData
                    ? <CheckCircle className="text-[#1A7A4A]" size={20} />
                    : <Clock className="text-[#999999]" size={20} />
                  }
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
                    <span className="text-[#333333] font-medium">{assigned}</span>
                  </div>
                  <div className="pt-3 border-t border-[#CCCCCC]">
                    <div className="flex justify-between text-sm">
                      <span className="text-[#666666]">Estado</span>
                      <Badge variant={hasData ? "success" : "inactive"} className="text-xs">
                        {hasData ? "Propuesta activa" : "Pendiente"}
                      </Badge>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Resultado preliminar */}
      {totalAssigned > 0 && (
        <div className="grid grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Resultado Preliminar</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center justify-between p-4 bg-[#1A7A4A]/5 rounded border border-[#1A7A4A]/20">
                  <div>
                    <p className="text-sm text-[#666666]">Sesiones ubicadas</p>
                    <p className="text-2xl font-medium text-[#1A7A4A]">{totalAssigned}</p>
                  </div>
                  <CheckCircle className="text-[#1A7A4A]" size={32} />
                </div>
                <div className="pt-4 border-t border-[#CCCCCC]">
                  <p className="text-xs text-[#666666]">
                    Propuesta generada. Revisa en Ajuste Manual antes de confirmar.
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Historial de Generaciones</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-center py-8 text-[#999999] text-sm">
                No hay generaciones anteriores registradas.
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Modal de generación */}
      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-lg w-full max-w-md p-6">
            <h2 className="text-lg font-semibold text-[#333333] mb-4">Generar horarios</h2>
            <p className="text-sm text-[#666666] mb-4">
              Se generarán propuestas para los 4 escenarios. Las propuestas previas serán reemplazadas.
            </p>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Período académico</label>
                <Select
                  value={periodo}
                  onChange={(e) => setPeriodo(e.target.value)}
                  options={[
                    { value: "2026-1", label: "2026-1" },
                    { value: "2026-2", label: "2026-2" },
                    { value: "2025-2", label: "2025-2" },
                  ]}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Semestre Ingeniería</label>
                <Select
                  value={semestreIngenieria}
                  onChange={(e) => setSemestreIngenieria(e.target.value)}
                  options={Array.from({ length: 10 }, (_, i) => ({
                    value: String(i + 1),
                    label: `Semestre ${i + 1}`,
                  }))}
                />
              </div>
            </div>
            <div className="flex gap-3 mt-6 justify-end">
              <button
                onClick={() => setShowModal(false)}
                disabled={generando}
                className="px-4 py-2 text-sm font-medium text-[#666666] bg-[#F5F5F5] rounded hover:bg-[#E8E8E8] transition-colors disabled:opacity-50"
              >
                Cancelar
              </button>
              <button
                onClick={handleGenerar}
                disabled={generando}
                className="px-4 py-2 text-sm font-medium text-white bg-[#1A6BBF] rounded hover:bg-[#155BA0] transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {generando && <Loader2 size={16} className="animate-spin" />}
                {generando ? "Generando..." : "Generar"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
