// views/HistoryView.tsx - Versión completa y corregida
import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Download, Eye, Archive, Loader2, FileText, FileSpreadsheet } from "lucide-react";
import { historyService, SemesterHistory, SemesterDetail } from "../../../services/history.service";
import { excelService } from "../../../services/excel.service";

export function HistoryView() {
  const [semesters, setSemesters] = useState<SemesterHistory[]>([]);
  const [selectedPeriodo, setSelectedPeriodo] = useState<string>("");
  const [selectedDetail, setSelectedDetail] = useState<SemesterDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [exportando, setExportando] = useState(false);
  const [estadisticas, setEstadisticas] = useState({
    totalAsignaturas: 0,
    maxDocentes: 0,
    totalConflictos: 0,
    totalExportaciones: 0,
    periodos: 0
  });

  const days = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  const hours = Array.from({ length: 7 }, (_, i) => i + 7);

  // Cargar periodos históricos
  useEffect(() => {
    cargarPeriodos();
    cargarEstadisticas();
  }, []);

  // Cargar detalle cuando se selecciona un periodo
  useEffect(() => {
    if (selectedPeriodo) {
      cargarDetallePeriodo(selectedPeriodo);
    }
  }, [selectedPeriodo]);

  const cargarPeriodos = async () => {
    setLoading(true);
    try {
      const data = await historyService.getPeriodosHistoricos();
      setSemesters(data);
      if (data.length > 0 && !selectedPeriodo) {
        setSelectedPeriodo(data[0].periodo);
      }
    } catch (error) {
      console.error("Error cargando periodos:", error);
    } finally {
      setLoading(false);
    }
  };

  const cargarEstadisticas = async () => {
    try {
      const data = await historyService.getEstadisticasGlobales();
      setEstadisticas(data);
    } catch (error) {
      console.error("Error cargando estadísticas:", error);
    }
  };

  const cargarDetallePeriodo = async (periodo: string) => {
    setLoadingDetail(true);
    try {
      const data = await historyService.getDetallePeriodo(periodo);
      setSelectedDetail(data);
    } catch (error) {
      console.error("Error cargando detalle:", error);
    } finally {
      setLoadingDetail(false);
    }
  };

  // Exportar a PDF
  const handleExportarPDF = async (periodo: string) => {
    setExportando(true);
    try {
      const blob = await historyService.exportarHistorico(periodo, 'pdf');
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Historico_${periodo}.pdf`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error exportando:", error);
      alert("Error al exportar el histórico");
    } finally {
      setExportando(false);
    }
  };

  // Exportar a Excel
  const handleExportarExcel = async (periodo: string) => {
    setExportando(true);
    try {
      const blob = await (excelService as any).exportarHistorial(periodo);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Historico_${periodo}.xlsx`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error exportando:", error);
      alert("Error al exportar el historial");
    } finally {
      setExportando(false);
    }
  };

  // Exportar histórico general
  const handleExportarHistoricoGeneral = async () => {
    if (!selectedPeriodo) return;
    setExportando(true);
    try {
      const blob = await (excelService as any).exportarHistorial(selectedPeriodo);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Historico_${selectedPeriodo}.xlsx`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error exportando:", error);
      alert("Error al exportar el histórico");
    } finally {
      setExportando(false);
    }
  };

  const getStatusBadge = (estado: string) => {
    switch (estado) {
      case "Activo":
        return <Badge variant="success">Activo</Badge>;
      case "Cerrado":
        return <Badge variant="inactive">Cerrado</Badge>;
      case "Archivado":
        return <Badge variant="info">Archivado</Badge>;
      default:
        return <Badge variant="secondary">{estado}</Badge>;
    }
  };

  // Vista previa de horario (tomar primeras asignaciones del detalle)
  type PreviewBlock = {
    day: number;
    hour: number;
    duration: number;
    subject: string;
    color: string;
  };

  const previewBlocks: PreviewBlock[] = selectedDetail?.asignaciones?.slice(0, 6).map(
    (asig: any, idx: number): PreviewBlock => ({
      day: asig.dia - 1,
      hour: parseInt(asig.horaInicio.split(":")[0]),
      duration:
        parseInt(asig.horaFin.split(":")[0]) -
        parseInt(asig.horaInicio.split(":")[0]),
      subject: asig.nombreAsignatura.substring(0, 8),
      color: idx % 2 === 0 ? "bg-[#1A6BBF]" : "bg-[#003087]",
    })
  ) || [];

  if (loading) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <div className="text-center">
          <Loader2 className="w-8 h-8 animate-spin text-[#1A6BBF] mx-auto" />
          <p className="mt-4 text-[#666666]">Cargando historial...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Modo Solo Lectura */}
      <div className="bg-[#1A6BBF]/10 border-l-4 border-[#1A6BBF] p-4 rounded">
        <div className="flex items-center gap-3">
          <Archive className="text-[#1A6BBF]" size={24} />
          <div>
            <p className="text-sm font-medium text-[#333333]">Modo Solo Lectura</p>
            <p className="text-xs text-[#666666]">
              Esta sección contiene información histórica de semestres anteriores. No se pueden realizar modificaciones.
            </p>
          </div>
        </div>
      </div>

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Historial de Semestres</h1>
          <p className="text-sm text-[#666666] mt-1">Consulta de horarios y configuraciones anteriores</p>
        </div>
        <Button 
          className="gap-2" 
          onClick={handleExportarHistoricoGeneral} 
          disabled={!selectedPeriodo || exportando}
        >
          {exportando ? <Loader2 size={20} className="animate-spin" /> : <Download size={20} />}
          Exportar histórico
        </Button>
      </div>

      <div className="grid grid-cols-3 gap-6">
        {/* Tabla de semestres */}
        <div className="col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Semestres Anteriores</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Periodo</TableHead>
                    <TableHead>Escenarios</TableHead>
                    <TableHead>Docentes</TableHead>
                    <TableHead>Asignaturas</TableHead>
                    <TableHead>Exportaciones</TableHead>
                    <TableHead>Estado</TableHead>
                    <TableHead>Acciones</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {semesters.map((semester) => (
                    <TableRow
                      key={semester.periodo}
                      striped
                      className={`transition-colors ${
                        selectedPeriodo === semester.periodo ? "bg-[#1A6BBF]/10" : ""
                      }`}
                    >
                      <TableCell className="font-medium">
                        <button
                          type="button"
                          className="w-full text-left cursor-pointer"
                          onClick={() => setSelectedPeriodo(semester.periodo)}
                        >
                          {semester.periodo}
                        </button>
                      </TableCell>
                      <TableCell>
                        <button
                          type="button"
                          className="w-full text-left cursor-pointer"
                          onClick={() => setSelectedPeriodo(semester.periodo)}
                        >
                          {semester.escenarios}
                        </button>
                      </TableCell>
                      <TableCell>
                        <button
                          type="button"
                          className="w-full text-left cursor-pointer"
                          onClick={() => setSelectedPeriodo(semester.periodo)}
                        >
                          {semester.docentes}
                        </button>
                      </TableCell>
                      <TableCell>
                        <button
                          type="button"
                          className="w-full text-left cursor-pointer"
                          onClick={() => setSelectedPeriodo(semester.periodo)}
                        >
                          {semester.asignaturas}
                        </button>
                      </TableCell>
                      <TableCell>
                        <button
                          type="button"
                          className="w-full text-left cursor-pointer"
                          onClick={() => setSelectedPeriodo(semester.periodo)}
                        >
                          <Badge variant="secondary" className="text-xs">
                            {semester.exportaciones} archivos
                          </Badge>
                        </button>
                      </TableCell>
                      <TableCell>
                        <button
                          type="button"
                          className="w-full text-left cursor-pointer"
                          onClick={() => setSelectedPeriodo(semester.periodo)}
                        >
                          {getStatusBadge(semester.estado)}
                        </button>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleExportarPDF(semester.periodo)}
                            className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]"
                            title="Exportar PDF"
                            disabled={exportando}
                          >
                            <FileText size={16} />
                          </button>
                          <button
                            onClick={() => handleExportarExcel(semester.periodo)}
                            className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A7A4A]"
                            title="Exportar Excel"
                            disabled={exportando}
                          >
                            <FileSpreadsheet size={16} />
                          </button>
                          <button
                            onClick={() => setSelectedPeriodo(semester.periodo)}
                            className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]"
                            title="Ver detalle"
                          >
                            <Eye size={16} />
                          </button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>

        {/* Detalle del semestre seleccionado */}
        <div>
          <Card>
            <CardHeader>
              <CardTitle>Resumen del Semestre</CardTitle>
              {selectedPeriodo && (
                <Badge variant="inactive" className="mt-2">
                  {selectedPeriodo}
                </Badge>
              )}
            </CardHeader>
            <CardContent>
              {loadingDetail ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="w-6 h-6 animate-spin text-[#1A6BBF]" />
                </div>
              ) : selectedDetail ? (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Asignaturas</p>
                      <p className="text-xl font-medium text-[#333333]">{selectedDetail.asignaturas?.length || 0}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Docentes</p>
                      <p className="text-xl font-medium text-[#333333]">{selectedDetail.docentes?.length || 0}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Escenarios</p>
                      <p className="text-xl font-medium text-[#333333]">{selectedDetail.escenarios?.length || 0}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Conflictos</p>
                      <p className="text-xl font-medium text-[#E8A020]">0</p>
                    </div>
                  </div>

                  {/* Vista previa del horario */}
                  <div className="pt-4 border-t border-[#CCCCCC]">
                    <p className="text-xs font-medium text-[#333333] mb-3">Vista previa del horario</p>
                    <div className="border border-[#CCCCCC] rounded overflow-hidden bg-white">
                      <div className="grid grid-cols-7 bg-[#333333]">
                        <div className="p-1 text-[10px] text-white text-center border-r border-white/20">H</div>
                        {days.map((day) => (
                          <div key={day} className="p-1 text-[10px] text-white text-center border-r border-white/20 last:border-r-0">
                            {day}
                          </div>
                        ))}
                      </div>
                      <div className="grid grid-cols-7" style={{ height: "200px" }}>
                        <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                          {hours.map((hour) => (
                            <div key={hour} className="h-[28.5px] border-b border-[#CCCCCC] px-1 text-[9px] text-[#666666]">
                              {hour}
                            </div>
                          ))}
                        </div>

                        {days.map((_, dayIdx) => (
                          <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                            {hours.map((hour) => (
                              <div key={hour} className="h-[28.5px] border-b border-[#CCCCCC]" />
                            ))}

                            {previewBlocks
                              .filter((block) => block.day === dayIdx)
                              .map((block, idx) => (
                                <div
                                  key={idx}
                                  className={`absolute ${block.color} text-white text-[8px] p-1 rounded overflow-hidden flex items-center justify-center font-medium left-0.5 right-0.5`}
                                  style={{
                                    top: `${((block.hour - 7) / 7) * 100}%`,
                                    height: `${(block.duration / 7) * 100}%`,
                                  }}
                                >
                                  {block.subject}
                                </div>
                              ))}
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>

                  <Button variant="secondary" size="sm" className="w-full gap-2">
                    <Eye size={16} />
                    Ver detalle completo
                  </Button>
                </div>
              ) : (
                <p className="text-sm text-[#999999] text-center py-8">
                  Seleccione un semestre para ver el resumen
                </p>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Estadísticas históricas */}
      <Card>
        <CardHeader>
          <CardTitle>Estadísticas Históricas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-4 gap-6">
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#1A6BBF] mb-1">{estadisticas.totalAsignaturas}</p>
              <p className="text-xs text-[#666666]">Total asignaturas históricas</p>
            </div>
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#1A7A4A] mb-1">{estadisticas.maxDocentes}</p>
              <p className="text-xs text-[#666666]">Máximo de docentes</p>
            </div>
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#E8A020] mb-1">{estadisticas.totalConflictos}</p>
              <p className="text-xs text-[#666666]">Conflictos totales resueltos</p>
            </div>
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#333333] mb-1">{estadisticas.totalExportaciones}</p>
              <p className="text-xs text-[#666666]">Documentos generados</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}