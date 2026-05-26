import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { ProgressBar } from "../ProgressBar";
import { Select } from "../Select";
import { FileSpreadsheet, FileText, History, Download, Loader2 } from "lucide-react";
import { reportsService, ReporteCargaDocenteResponse } from "../../../services/reports.service";
import { obtenerDocentes } from "../../../services/teachersService";
import { obtenerPlanes, PlanEstudio } from "../../../services/planesService";
import api from "../../../services/api";

/** Datos mínimos de un docente para los selectores de filtro en la vista de reportes. */
interface Docente { idDocente: string; nombre: string; }

const reportTypes = [
  { id: "workload",  title: "Horas asignadas vs contrato", description: "Comparativo de carga docente",    icon: FileSpreadsheet, format: "Excel", color: "text-[#1A7A4A]", bg: "bg-[#1A7A4A]/10" },
  { id: "schedule",  title: "Exportar horarios",           description: "Calendario completo por escenario", icon: FileText,       format: "Excel", color: "text-[#1A6BBF]", bg: "bg-[#1A6BBF]/10" },
  { id: "conflicts", title: "Conflictos y excepciones",    description: "Registro de auditoría",             icon: History,        format: "Excel", color: "text-[#E8A020]", bg: "bg-[#E8A020]/10" },
];

/** Props de ReportsView. Si CalendarView redirige aquí con filtros, se pre-seleccionan semestre y docente. */
interface Props {
  /** Código de periodo a preseleccionar en el selector de semestre (p.ej. "2026-1"). */
  initialSemestre?: string;
  /** ID del docente a preseleccionar en el filtro de exportación por docente. */
  initialDocenteId?: string;
}

/**
 * Vista de reportes y exportaciones del sistema.
 * Muestra tres secciones: reporte de carga docente (con barra de progreso por docente),
 * exportación de horarios (por escenario, por docente o por plan) y registro de conflictos.
 * Acepta filtros iniciales provenientes de CalendarView para pre-seleccionar el tab correcto.
 */
export function ReportsView({ initialSemestre, initialDocenteId }: Props) {
  const [selectedReport, setSelectedReport] = useState<string>(initialDocenteId ? "schedule" : "workload");
  const [selectedSemestre, setSelectedSemestre] = useState(initialSemestre ?? "2026-1");
  const [reporteData, setReporteData] = useState<ReporteCargaDocenteResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [exportando, setExportando] = useState<string | null>(null);
  const [docentes, setDocentes] = useState<Docente[]>([]);
  const [planes, setPlanes] = useState<PlanEstudio[]>([]);
  const [docenteSeleccionado, setDocenteSeleccionado] = useState(initialDocenteId ?? "");
  const [planSeleccionado, setPlanSeleccionado] = useState("");

  useEffect(() => {
    cargarFiltros();
  }, []);

  useEffect(() => {
    if (selectedReport === "workload") cargarReporteCarga();
  }, [selectedReport, selectedSemestre]);

  /** Carga en paralelo docentes y planes de estudio para los selectores de filtro de exportación. */
  const cargarFiltros = async () => {
    try {
      const [docs, plans] = await Promise.allSettled([obtenerDocentes(), obtenerPlanes()]);
      if (docs.status === "fulfilled") setDocentes(docs.value || []);
      if (plans.status === "fulfilled") setPlanes(plans.value || []);
    } catch { /* graceful */ }
  };

  /** Solicita al backend el reporte de carga docente del semestre seleccionado y actualiza el estado local. */
  const cargarReporteCarga = async () => {
    setLoading(true);
    try {
      const data = await reportsService.getReporteCarga(selectedSemestre);
      setReporteData(data);
    } catch { /* silent */ } finally { setLoading(false); }
  };

  /**
   * Fuerza la descarga de un Blob en el navegador/Electron con el nombre de archivo indicado.
   * Crea un anchor temporal en el DOM, hace clic programáticamente y lo elimina.
   */
  const descargarBlob = (blob: Blob, nombreArchivo: string) => {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = nombreArchivo;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  };

  /**
   * Exporta el horario en formato Excel según el tipo seleccionado:
   * - "todos": una hoja por cada uno de los cuatro escenarios.
   * - "docente": horario individual del docente seleccionado, o una hoja por docente si se eligió "__ALL__".
   * - "plan": una hoja por semestre del plan de estudios seleccionado.
   */
  const exportarHorario = async (tipo: "todos" | "docente" | "plan") => {
    setExportando(tipo);
    try {
      const params = new URLSearchParams({ periodo: selectedSemestre });

      if (tipo === "docente") {
        if (docenteSeleccionado === "__ALL__") {
          // Una hoja por cada docente
          params.set("idDocente", "__ALL__");
        } else if (docenteSeleccionado) {
          params.set("idDocente", docenteSeleccionado);
        }
      }

      if (tipo === "plan" && planSeleccionado) {
        params.set("idPlan", planSeleccionado);
        params.set("porSemestre", "true"); // Una hoja por semestre
      }

      const response = await api.get(`/horarios/exportar?${params}`, { responseType: "blob" });
      const sufijo =
        tipo === "todos" ? "4_Horarios" :
        tipo === "docente" ? (docenteSeleccionado === "__ALL__" ? "Todos_Docentes" : "Por_Docente") :
        "Por_Plan";
      descargarBlob(response.data, `Horarios_${sufijo}_${selectedSemestre}.xlsx`);
    } catch (err: any) {
      alert("Error al exportar: " + (err?.response?.data?.message || err?.message));
    } finally {
      setExportando(null);
    }
  };

  /** Descarga el reporte de carga docente del semestre seleccionado en formato Excel (.xlsx). */
  const exportarReporteCarga = async () => {
    setExportando("carga");
    try {
      const blob = await reportsService.exportarReporteCargaExcel(selectedSemestre);
      descargarBlob(blob, `Reporte_Carga_${selectedSemestre}.xlsx`);
    } catch { alert("Error al exportar el reporte."); } finally { setExportando(null); }
  };

  const teacherWorkload = reporteData?.docentes?.map(d => ({
    name: d.nombreDocente,
    contract: d.horasContractuales,
    assigned: d.totalHorasSemanales,
    difference: d.diferenciaHoras,
    isOverloaded: d.estadoCarga === "Excedida"
  })) || [];

  const stats = reporteData
    ? {
        totalAsignadas: reporteData.docentes.reduce((s, d) => s + d.totalHorasSemanales, 0),
        totalContractuales: reporteData.docentes.reduce((s, d) => s + d.horasContractuales, 0),
        docentesSobrecarga: reporteData.docentesConCargaExcedida,
      }
    : { totalAsignadas: 0, totalContractuales: 0, docentesSobrecarga: 0 };

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Reportes y Exportación</h1>
          <p className="text-sm text-[#666666] mt-1">Generación de informes y documentos institucionales</p>
        </div>
        <Select
          value={selectedSemestre}
          onChange={(e) => setSelectedSemestre(e.target.value)}
          options={[
            { value: "2026-1", label: "2026-1" }, { value: "2026-2", label: "2026-2" },
            { value: "2025-1", label: "2025-1" }, { value: "2025-2", label: "2025-2" },
          ]}
          className="w-32"
        />
      </div>

      {/* Tarjetas de tipo */}
      <div className="grid grid-cols-3 gap-6">
        {reportTypes.map((report) => {
          const Icon = report.icon;
          return (
            <div
              key={report.id}
              className={`cursor-pointer transition-all ${selectedReport === report.id ? "ring-2 ring-[#1A6BBF] shadow-lg" : "hover:shadow-md"}`}
              onClick={() => setSelectedReport(report.id)}
            >
              <Card>
                <CardContent className="flex flex-col items-center text-center pt-6">
                  <div className={`w-16 h-16 rounded-lg ${report.bg} flex items-center justify-center mb-4`}>
                    <Icon className={report.color} size={32} />
                  </div>
                  <h3 className="text-base font-medium text-[#333333] mb-1">{report.title}</h3>
                  <p className="text-sm text-[#666666] mb-3">{report.description}</p>
                  <Badge variant="secondary" className="text-xs">{report.format}</Badge>
                </CardContent>
              </Card>
            </div>
          );
        })}
      </div>

      {/* Reporte de carga docente */}
      {selectedReport === "workload" && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-2">
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle>Vista Previa — Horas Asignadas vs Contrato ({selectedSemestre})</CardTitle>
                  <Button size="sm" className="gap-2" onClick={exportarReporteCarga} disabled={exportando === "carga"}>
                    {exportando === "carga" ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
                    Exportar Excel
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                {loading ? (
                  <div className="text-center py-8">
                    <Loader2 className="w-8 h-8 animate-spin text-[#1A6BBF] mx-auto" />
                    <p className="mt-2 text-[#666666]">Cargando datos...</p>
                  </div>
                ) : teacherWorkload.length === 0 ? (
                  <div className="text-center py-8 text-[#666666]">
                    No hay datos de carga docente para {selectedSemestre}.
                  </div>
                ) : (
                  <div className="space-y-4">
                    {teacherWorkload.map((teacher, idx) => {
                      const pct = (teacher.assigned / teacher.contract) * 100;
                      return (
                        <div key={idx} className="space-y-2">
                          <div className="flex items-center justify-between">
                            <div className="flex-1">
                              <p className="text-sm font-medium text-[#333333]">{teacher.name}</p>
                              <p className="text-xs text-[#666666]">{teacher.assigned}h de {teacher.contract}h contractuales</p>
                            </div>
                            <Badge variant={teacher.isOverloaded ? "error" : "success"} className="ml-4">
                              {teacher.isOverloaded ? "Sobrecarga" : "OK"}
                            </Badge>
                          </div>
                          <div className="flex items-center gap-3">
                            <ProgressBar value={teacher.assigned} max={teacher.contract} variant={teacher.isOverloaded ? "error" : "success"} size="sm" className="flex-1" />
                            <span className={`text-sm font-medium ${teacher.isOverloaded ? "text-[#C0392B]" : "text-[#1A7A4A]"}`}>
                              {teacher.isOverloaded ? "-" : "+"}{Math.abs(teacher.difference)}h
                            </span>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
                {!loading && teacherWorkload.length > 0 && (
                  <div className="mt-6 pt-6 border-t border-[#CCCCCC] grid grid-cols-3 gap-4 text-center">
                    <div><p className="text-sm text-[#666666]">Total asignadas</p><p className="text-xl font-medium text-[#333333]">{stats.totalAsignadas}h</p></div>
                    <div><p className="text-sm text-[#666666]">Total contractuales</p><p className="text-xl font-medium text-[#333333]">{stats.totalContractuales}h</p></div>
                    <div><p className="text-sm text-[#666666]">Sobrecarga</p><p className="text-xl font-medium text-[#C0392B]">{stats.docentesSobrecarga}</p></div>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
          <div>
            <Card>
              <CardHeader><CardTitle>Resumen</CardTitle></CardHeader>
              <CardContent>
                {loading ? (
                  <div className="text-center py-4"><Loader2 className="w-6 h-6 animate-spin text-[#1A6BBF] mx-auto" /></div>
                ) : reporteData ? (
                  <div className="space-y-3">
                    <div className="p-3 bg-[#F5F5F5] rounded"><p className="text-xs text-[#666666]">Total docentes</p><p className="text-xl font-medium text-[#333333]">{reporteData.totalDocentes}</p></div>
                    <div className="p-3 bg-[#F5F5F5] rounded"><p className="text-xs text-[#666666]">Carga completa</p><p className="text-xl font-medium text-[#1A7A4A]">{reporteData.docentesConCargaCompleta}</p></div>
                    <div className="p-3 bg-[#F5F5F5] rounded"><p className="text-xs text-[#666666]">Carga parcial</p><p className="text-xl font-medium text-[#E8A020]">{reporteData.docentesConCargaParcial}</p></div>
                    <div className="p-3 bg-[#F5F5F5] rounded"><p className="text-xs text-[#666666]">Sin asignaciones</p><p className="text-xl font-medium text-[#999999]">{reporteData.docentesSinAsignaciones}</p></div>
                  </div>
                ) : (
                  <p className="text-sm text-[#999999] text-center py-4">Selecciona un semestre</p>
                )}
              </CardContent>
            </Card>
          </div>
        </div>
      )}

      {/* Exportar horarios */}
      {selectedReport === "schedule" && (
        <Card>
          <CardHeader>
            <CardTitle>Exportar Horarios — {selectedSemestre}</CardTitle>
            <p className="text-sm text-[#666666] mt-1">Descarga el horario semanal en formato Excel (cuadrícula por día y hora)</p>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-3 gap-6">
              {/* Los 4 horarios */}
              <div className="p-5 border border-[#CCCCCC] rounded-lg space-y-3">
                <div className="flex items-center gap-3 mb-2">
                  <div className="w-10 h-10 rounded-lg bg-[#1A6BBF]/10 flex items-center justify-center">
                    <FileText className="text-[#1A6BBF]" size={20} />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-[#333333]">Los 4 horarios</p>
                    <p className="text-xs text-[#666666]">Una hoja por escenario</p>
                  </div>
                </div>
                <Button
                  className="w-full gap-2"
                  onClick={() => exportarHorario("todos")}
                  disabled={exportando !== null}
                >
                  {exportando === "todos" ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
                  {exportando === "todos" ? "Descargando..." : "Descargar Excel"}
                </Button>
              </div>

              {/* Por docente */}
              <div className="p-5 border border-[#CCCCCC] rounded-lg space-y-3">
                <div className="flex items-center gap-3 mb-2">
                  <div className="w-10 h-10 rounded-lg bg-[#1A7A4A]/10 flex items-center justify-center">
                    <FileText className="text-[#1A7A4A]" size={20} />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-[#333333]">Por docente</p>
                    <p className="text-xs text-[#666666]">
                      {docenteSeleccionado === "__ALL__" ? "Una hoja por docente" : "Horario individual del docente"}
                    </p>
                  </div>
                </div>
                <Select
                  placeholder="Seleccionar docente"
                  value={docenteSeleccionado}
                  onChange={(e) => setDocenteSeleccionado(e.target.value)}
                  options={[
                    { value: "", label: "Seleccionar docente..." },
                    { value: "__ALL__", label: "Todos los docentes" },
                    ...docentes.map((d) => ({ value: d.idDocente, label: d.nombre })),
                  ]}
                />
                <Button
                  className="w-full gap-2"
                  onClick={() => exportarHorario("docente")}
                  disabled={!docenteSeleccionado || exportando !== null}
                >
                  {exportando === "docente" ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
                  {exportando === "docente" ? "Descargando..." : "Descargar Excel"}
                </Button>
              </div>

              {/* Por plan */}
              <div className="p-5 border border-[#CCCCCC] rounded-lg space-y-3">
                <div className="flex items-center gap-3 mb-2">
                  <div className="w-10 h-10 rounded-lg bg-[#E8A020]/10 flex items-center justify-center">
                    <FileText className="text-[#E8A020]" size={20} />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-[#333333]">Por plan de estudios</p>
                    <p className="text-xs text-[#666666]">Una hoja por semestre del plan</p>
                  </div>
                </div>
                <Select
                  placeholder="Seleccionar plan"
                  value={planSeleccionado}
                  onChange={(e) => setPlanSeleccionado(e.target.value)}
                  options={[
                    { value: "", label: "Seleccionar plan..." },
                    ...planes.map((p) => ({ value: p.idPlan, label: p.nombrePlan })),
                  ]}
                />
                <Button
                  className="w-full gap-2"
                  onClick={() => exportarHorario("plan")}
                  disabled={!planSeleccionado || exportando !== null}
                >
                  {exportando === "plan" ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
                  {exportando === "plan" ? "Descargando..." : "Descargar Excel"}
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Conflictos */}
      {selectedReport === "conflicts" && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Conflictos y Excepciones — {selectedSemestre}</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <p className="text-sm text-[#666666]">
                Historial completo de conflictos detectados y excepciones aprobadas para el período seleccionado.
              </p>
              <div className="grid grid-cols-3 gap-4 p-4 bg-[#F5F5F5] rounded">
                <div className="text-center"><p className="text-2xl font-medium text-[#C0392B]">—</p><p className="text-xs text-[#666666]">Errores críticos</p></div>
                <div className="text-center"><p className="text-2xl font-medium text-[#E8A020]">—</p><p className="text-xs text-[#666666]">Advertencias</p></div>
                <div className="text-center"><p className="text-2xl font-medium text-[#1A7A4A]">—</p><p className="text-xs text-[#666666]">Resueltos</p></div>
              </div>
              <p className="text-sm text-[#999999] text-center py-2">
                Ve a la sección de Alertas para ver el detalle de conflictos activos.
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
