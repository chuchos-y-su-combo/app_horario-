// views/ReportsView.tsx
import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { ProgressBar } from "../ProgressBar";
import { Select } from "../Select";
import { FileSpreadsheet, FileText, History, Download, Eye, Loader2 } from "lucide-react";
import { reportsService, ReporteCargaDocenteResponse } from "../../../services/reports.service";

export function ReportsView() {
  const [selectedReport, setSelectedReport] = useState<string>("workload");
  const [selectedSemestre, setSelectedSemestre] = useState("2026-1");
  const [reporteData, setReporteData] = useState<ReporteCargaDocenteResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [exportando, setExportando] = useState(false);
  const [exportOptions, setExportOptions] = useState({
    escenarios: ["ING_DIURNA", "ING_NOCTURNA", "TAPSI_DIURNA", "TAPSI_NOCTURNA"],
    incluirDocentes: true,
    mostrarSalones: true,
    incluirCodigos: false,
  });

  const reportTypes = [
    {
      id: "workload",
      title: "Horas asignadas vs contrato",
      description: "Comparativo de carga docente",
      icon: FileSpreadsheet,
      format: "Excel",
      color: "text-[#1A7A4A]",
      bg: "bg-[#1A7A4A]/10",
    },
    {
      id: "schedule",
      title: "Horario semanal",
      description: "Calendario completo por escenario",
      icon: FileText,
      format: "PDF",
      color: "text-[#1A6BBF]",
      bg: "bg-[#1A6BBF]/10",
    },
    {
      id: "conflicts",
      title: "Conflictos y excepciones",
      description: "Registro de auditoría",
      icon: History,
      format: "Excel",
      color: "text-[#E8A020]",
      bg: "bg-[#E8A020]/10",
    },
  ];

  // Cargar reporte cuando se selecciona
  useEffect(() => {
    if (selectedReport === "workload") {
      cargarReporteCarga();
    } else if (selectedReport === "conflicts") {
      cargarConflictos();
    }
  }, [selectedReport, selectedSemestre]);

  const cargarReporteCarga = async () => {
    setLoading(true);
    try {
      const data = await reportsService.getReporteCarga(selectedSemestre);
      setReporteData(data);
    } catch (error) {
      console.error("Error cargando reporte:", error);
    } finally {
      setLoading(false);
    }
  };

  const cargarConflictos = async () => {
    setLoading(true);
    try {
      const data = await reportsService.getConflictos(selectedSemestre);
      console.log("Conflictos:", data);
    } catch (error) {
      console.error("Error cargando conflictos:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleExportarExcel = async () => {
    setExportando(true);
    try {
      const blob = await reportsService.exportarReporteCargaExcel(selectedSemestre);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Reporte_Carga_${selectedSemestre}.xlsx`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error exportando:", error);
      alert("Error al exportar el reporte");
    } finally {
      setExportando(false);
    }
  };

  const handleExportarHorarioPDF = async () => {
    setExportando(true);
    try {
      const blob = await reportsService.exportarHorarioPDF(selectedSemestre, exportOptions.escenarios);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Horario_${selectedSemestre}.pdf`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error exportando:", error);
      alert("Error al exportar el horario PDF");
    } finally {
      setExportando(false);
    }
  };

  const handleExportarConflictosExcel = async () => {
    setExportando(true);
    try {
      const blob = await reportsService.exportarConflictosExcel(selectedSemestre);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Conflictos_${selectedSemestre}.xlsx`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error exportando:", error);
      alert("Error al exportar conflictos");
    } finally {
      setExportando(false);
    }
  };

  const teacherWorkload = reporteData?.docentes?.map(d => ({
    name: d.nombreDocente,
    contract: d.horasContractuales,
    assigned: d.totalHorasSemanales,
    difference: d.diferenciaHoras,
    status: d.estadoCarga === "Excedida" ? "error" : "success"
  })) || [];

  const stats = reporteData ? {
    totalAsignadas: reporteData.docentes.reduce((sum, d) => sum + d.totalHorasSemanales, 0),
    totalContractuales: reporteData.docentes.reduce((sum, d) => sum + d.horasContractuales, 0),
    docentesSobrecarga: reporteData.docentesConCargaExcedida,
  } : { totalAsignadas: 0, totalContractuales: 0, docentesSobrecarga: 0 };

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Header con selector de semestre */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Reportes y Exportación</h1>
          <p className="text-sm text-[#666666] mt-1">Generación de informes y documentos institucionales</p>
        </div>
        <Select
          value={selectedSemestre}
          onChange={(e) => setSelectedSemestre(e.target.value)}
          options={[
            { value: "2026-1", label: "2026-1" },
            { value: "2026-2", label: "2026-2" },
            { value: "2025-1", label: "2025-1" },
            { value: "2025-2", label: "2025-2" },
          ]}
          className="w-32"
        />
      </div>

      {/* Tarjetas de tipos de reporte */}
      <div className="grid grid-cols-3 gap-6">
        {reportTypes.map((report) => {
          const Icon = report.icon;
          return (
            <div
              key={report.id}
              className={`cursor-pointer transition-all ${
                selectedReport === report.id
                  ? "ring-2 ring-[#1A6BBF] shadow-lg"
                  : "hover:shadow-md"
              }`}
              onClick={() => setSelectedReport(report.id)}
            >
              <Card>
                <CardContent className="flex flex-col items-center text-center pt-6">
                  <div className={`w-16 h-16 rounded-lg ${report.bg} flex items-center justify-center mb-4`}>
                    <Icon className={report.color} size={32} />
                  </div>
                  <h3 className="text-base font-medium text-[#333333] mb-1">{report.title}</h3>
                  <p className="text-sm text-[#666666] mb-3">{report.description}</p>
                  <Badge variant="secondary" className="text-xs">
                    {report.format}
                  </Badge>
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
                  <CardTitle>Vista Previa - Horas Asignadas vs Contrato ({selectedSemestre})</CardTitle>
                  <div className="flex gap-2">
                    <Button size="sm" className="gap-2" onClick={handleExportarExcel} disabled={exportando}>
                      {exportando ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
                      Exportar Excel
                    </Button>
                  </div>
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
                    No hay datos de carga docente para el semestre {selectedSemestre}
                  </div>
                ) : (
                  <div className="space-y-4">
                    {teacherWorkload.map((teacher, idx) => {
                      const percentage = (teacher.assigned / teacher.contract) * 100;
                      const isOverloaded = percentage > 100;

                      return (
                        <div key={idx} className="space-y-2">
                          <div className="flex items-center justify-between">
                            <div className="flex-1">
                              <p className="text-sm font-medium text-[#333333]">{teacher.name}</p>
                              <p className="text-xs text-[#666666]">
                                {teacher.assigned}h asignadas de {teacher.contract}h contractuales
                              </p>
                            </div>
                            <Badge variant={isOverloaded ? "error" : "success"} className="ml-4">
                              {isOverloaded ? "Sobrecarga" : "OK"}
                            </Badge>
                          </div>
                          <div className="flex items-center gap-3">
                            <ProgressBar
                              value={teacher.assigned}
                              max={teacher.contract}
                              variant={isOverloaded ? "error" : "success"}
                              size="sm"
                              className="flex-1"
                            />
                            <span className={`text-sm font-medium ${isOverloaded ? "text-[#C0392B]" : "text-[#1A7A4A]"}`}>
                              {isOverloaded ? "-" : "+"}{Math.abs(teacher.difference)}h
                            </span>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}

                {!loading && teacherWorkload.length > 0 && (
                  <div className="mt-6 pt-6 border-t border-[#CCCCCC]">
                    <div className="grid grid-cols-3 gap-4 text-center">
                      <div>
                        <p className="text-sm text-[#666666]">Total asignadas</p>
                        <p className="text-xl font-medium text-[#333333]">{stats.totalAsignadas}h</p>
                      </div>
                      <div>
                        <p className="text-sm text-[#666666]">Total contractuales</p>
                        <p className="text-xl font-medium text-[#333333]">{stats.totalContractuales}h</p>
                      </div>
                      <div>
                        <p className="text-sm text-[#666666]">Docentes sobrecarga</p>
                        <p className="text-xl font-medium text-[#C0392B]">{stats.docentesSobrecarga}</p>
                      </div>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>

          <div>
            <Card>
              <CardHeader>
                <CardTitle>Resumen</CardTitle>
              </CardHeader>
              <CardContent>
                {loading ? (
                  <div className="text-center py-4">
                    <Loader2 className="w-6 h-6 animate-spin text-[#1A6BBF] mx-auto" />
                  </div>
                ) : reporteData ? (
                  <div className="space-y-3">
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666]">Total docentes</p>
                      <p className="text-xl font-medium text-[#333333]">{reporteData.totalDocentes}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666]">Carga completa</p>
                      <p className="text-xl font-medium text-[#1A7A4A]">{reporteData.docentesConCargaCompleta}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666]">Carga parcial</p>
                      <p className="text-xl font-medium text-[#E8A020]">{reporteData.docentesConCargaParcial}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666]">Sin asignaciones</p>
                      <p className="text-xl font-medium text-[#999999]">{reporteData.docentesSinAsignaciones}</p>
                    </div>
                  </div>
                ) : (
                  <p className="text-sm text-[#999999] text-center py-4">Selecciona un semestre</p>
                )}
              </CardContent>
            </Card>
          </div>
        </div>
      )}

      {/* Configuración de exportación de horario PDF */}
      {selectedReport === "schedule" && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Configuración de Exportación - Horario Semanal</CardTitle>
              <Button className="gap-2" onClick={handleExportarHorarioPDF} disabled={exportando}>
                {exportando ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
                Exportar PDF
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-6">
              <div className="space-y-4">
                <div>
                  <label className="text-sm font-medium text-[#333333] mb-2 block">
                    Seleccionar escenario
                  </label>
                  <div className="space-y-2">
                    {["Ingeniería Diurna", "Ingeniería Nocturna", "TAPSI Diurno", "TAPSI Nocturno"].map((scenario) => (
                      <label key={scenario} className="flex items-center gap-2">
                        <input
                          type="checkbox"
                          className="rounded border-[#CCCCCC]"
                          checked={exportOptions.escenarios.includes(scenario.replace(" ", "_").toUpperCase())}
                          onChange={(e) => {
                            const valor = scenario.replace(" ", "_").toUpperCase();
                            if (e.target.checked) {
                              setExportOptions({ ...exportOptions, escenarios: [...exportOptions.escenarios, valor] });
                            } else {
                              setExportOptions({ ...exportOptions, escenarios: exportOptions.escenarios.filter(s => s !== valor) });
                            }
                          }}
                        />
                        <span className="text-sm text-[#333333]">{scenario}</span>
                      </label>
                    ))}
                  </div>
                </div>
                <div>
                  <label className="text-sm font-medium text-[#333333] mb-2 block">
                    Opciones de visualización
                  </label>
                  <div className="space-y-2">
                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        className="rounded border-[#CCCCCC]"
                        checked={exportOptions.incluirDocentes}
                        onChange={(e) => setExportOptions({ ...exportOptions, incluirDocentes: e.target.checked })}
                      />
                      <span className="text-sm text-[#333333]">Incluir nombres de docentes</span>
                    </label>
                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        className="rounded border-[#CCCCCC]"
                        checked={exportOptions.mostrarSalones}
                        onChange={(e) => setExportOptions({ ...exportOptions, mostrarSalones: e.target.checked })}
                      />
                      <span className="text-sm text-[#333333]">Mostrar salones asignados</span>
                    </label>
                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        className="rounded border-[#CCCCCC]"
                        checked={exportOptions.incluirCodigos}
                        onChange={(e) => setExportOptions({ ...exportOptions, incluirCodigos: e.target.checked })}
                      />
                      <span className="text-sm text-[#333333]">Incluir códigos de materia</span>
                    </label>
                  </div>
                </div>
              </div>
              <div className="bg-[#F5F5F5] rounded p-6 text-center flex items-center justify-center">
                <div>
                  <FileText className="text-[#999999] mx-auto mb-3" size={48} />
                  <p className="text-sm text-[#666666]">Vista previa del documento PDF</p>
                  <p className="text-xs text-[#999999] mt-2">
                    El archivo contendrá el calendario semanal completo con los filtros seleccionados
                  </p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Reporte de conflictos */}
      {selectedReport === "conflicts" && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Configuración de Exportación - Conflictos y Excepciones</CardTitle>
              <Button className="gap-2" onClick={handleExportarConflictosExcel} disabled={exportando}>
                {exportando ? <Loader2 size={16} className="animate-spin" /> : <Download size={16} />}
                Exportar Excel
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <p className="text-sm text-[#666666]">
                Este reporte incluye el historial completo de conflictos detectados, excepciones aprobadas
                y su registro de auditoría con usuario y fecha.
              </p>
              <div className="grid grid-cols-3 gap-4 p-4 bg-[#F5F5F5] rounded">
                <div className="text-center">
                  <p className="text-2xl font-medium text-[#C0392B]">-</p>
                  <p className="text-xs text-[#666666]">Errores críticos</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-medium text-[#E8A020]">-</p>
                  <p className="text-xs text-[#666666]">Advertencias</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-medium text-[#1A7A4A]">-</p>
                  <p className="text-xs text-[#666666]">Resueltos</p>
                </div>
              </div>
              {loading && (
                <div className="text-center py-4">
                  <Loader2 className="w-6 h-6 animate-spin text-[#1A6BBF] mx-auto" />
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}