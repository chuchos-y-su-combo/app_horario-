import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Download, Eye, Archive } from "lucide-react";

export function HistoryView() {
  const [selectedSemester, setSelectedSemester] = useState<number>(1);

  const semesters = [
    {
      id: 1,
      period: "2026-1",
      scenarios: 4,
      teachers: 124,
      exports: 12,
      status: "Cerrado",
      assigned: 240,
      conflicts: 8,
    },
    {
      id: 2,
      period: "2025-2",
      scenarios: 4,
      teachers: 118,
      exports: 15,
      status: "Archivado",
      assigned: 235,
      conflicts: 12,
    },
    {
      id: 3,
      period: "2025-1",
      scenarios: 4,
      teachers: 115,
      exports: 14,
      status: "Archivado",
      assigned: 228,
      conflicts: 10,
    },
    {
      id: 4,
      period: "2024-2",
      scenarios: 4,
      teachers: 110,
      exports: 13,
      status: "Archivado",
      assigned: 220,
      conflicts: 15,
    },
  ];

  const selectedSemesterData = semesters.find((s) => s.id === selectedSemester);

  const days = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  const previewBlocks = [
    { day: 0, hour: 8, duration: 2, subject: "Prog I", color: "bg-[#1A6BBF]" },
    { day: 0, hour: 14, duration: 3, subject: "BD", color: "bg-[#1A6BBF]" },
    { day: 1, hour: 10, duration: 2, subject: "Física", color: "bg-[#003087]" },
    { day: 2, hour: 8, duration: 2, subject: "Álgebra", color: "bg-[#1A6BBF]" },
    { day: 3, hour: 15, duration: 2, subject: "Redes", color: "bg-[#003087]" },
    { day: 4, hour: 9, duration: 3, subject: "Ing. SW", color: "bg-[#1A6BBF]" },
  ];

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="bg-[#1A6BBF]/10 border-l-4 border-[#1A6BBF] p-4 rounded">
        <div className="flex items-center gap-3">
          <Archive className="text-[#1A6BBF]" size={24} />
          <div>
            <p className="text-sm font-medium text-[#333333]">Modo Solo Lectura</p>
            <p className="text-xs text-[#666666]">
              Esta sección contiene información histórica de semestres anteriores. No se pueden realizar
              modificaciones.
            </p>
          </div>
        </div>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Historial de Semestres</h1>
          <p className="text-sm text-[#666666] mt-1">Consulta de horarios y configuraciones anteriores</p>
        </div>
        <Button className="gap-2">
          <Download size={20} />
          Exportar histórico
        </Button>
      </div>

      <div className="grid grid-cols-3 gap-6">
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
                    <TableHead>Exportaciones</TableHead>
                    <TableHead>Estado</TableHead>
                    <TableHead>Acciones</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {semesters.map((semester) => (
                    <TableRow
                      key={semester.id}
                      striped
                      className={`cursor-pointer transition-colors ${
                        selectedSemester === semester.id ? "bg-[#1A6BBF]/10" : ""
                      }`}
                      onClick={() => setSelectedSemester(semester.id)}
                    >
                      <TableCell className="font-medium">{semester.period}</TableCell>
                      <TableCell>{semester.scenarios}</TableCell>
                      <TableCell>{semester.teachers}</TableCell>
                      <TableCell>
                        <Badge variant="secondary" className="text-xs">
                          {semester.exports} archivos
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant={semester.status === "Cerrado" ? "inactive" : "info"}>
                          {semester.status}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <button className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]">
                          <Eye size={16} />
                        </button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>

        <div>
          <Card>
            <CardHeader>
              <CardTitle>Resumen del Semestre</CardTitle>
              {selectedSemesterData && (
                <Badge variant="inactive" className="mt-2">
                  {selectedSemesterData.period}
                </Badge>
              )}
            </CardHeader>
            <CardContent>
              {selectedSemesterData ? (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Asignaturas</p>
                      <p className="text-xl font-medium text-[#333333]">{selectedSemesterData.assigned}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Docentes</p>
                      <p className="text-xl font-medium text-[#333333]">{selectedSemesterData.teachers}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Escenarios</p>
                      <p className="text-xl font-medium text-[#333333]">{selectedSemesterData.scenarios}</p>
                    </div>
                    <div className="p-3 bg-[#F5F5F5] rounded">
                      <p className="text-xs text-[#666666] mb-1">Conflictos</p>
                      <p className="text-xl font-medium text-[#E8A020]">{selectedSemesterData.conflicts}</p>
                    </div>
                  </div>

                  <div className="pt-4 border-t border-[#CCCCCC]">
                    <p className="text-xs font-medium text-[#333333] mb-3">Vista previa del horario</p>
                    <div className="border border-[#CCCCCC] rounded overflow-hidden bg-white">
                      <div className="grid grid-cols-7 bg-[#333333]">
                        <div className="p-1 text-[10px] text-white text-center border-r border-white/20">H</div>
                        {days.map((day) => (
                          <div
                            key={day}
                            className="p-1 text-[10px] text-white text-center border-r border-white/20 last:border-r-0"
                          >
                            {day}
                          </div>
                        ))}
                      </div>
                      <div className="grid grid-cols-7" style={{ height: "200px" }}>
                        {/* Time labels column */}
                        <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                          {hours.slice(0, 7).map((hour) => (
                            <div
                              key={hour}
                              className="h-[28.5px] border-b border-[#CCCCCC] px-1 text-[9px] text-[#666666]"
                            >
                              {hour}
                            </div>
                          ))}
                        </div>

                        {/* Days columns */}
                        {days.map((_, dayIdx) => (
                          <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                            {/* Background cells */}
                            {hours.slice(0, 7).map((hour) => (
                              <div key={hour} className="h-[28.5px] border-b border-[#CCCCCC]" />
                            ))}

                            {/* Preview blocks for this day */}
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

                  <div className="pt-4 border-t border-[#CCCCCC]">
                    <p className="text-xs font-medium text-[#333333] mb-2">Archivos generados</p>
                    <div className="space-y-2">
                      <div className="flex items-center justify-between text-xs">
                        <span className="text-[#666666]">Horarios PDF</span>
                        <span className="text-[#333333]">4 archivos</span>
                      </div>
                      <div className="flex items-center justify-between text-xs">
                        <span className="text-[#666666]">Carga docente Excel</span>
                        <span className="text-[#333333]">3 archivos</span>
                      </div>
                      <div className="flex items-center justify-between text-xs">
                        <span className="text-[#666666]">Reportes de conflictos</span>
                        <span className="text-[#333333]">5 archivos</span>
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

      <Card>
        <CardHeader>
          <CardTitle>Estadísticas Históricas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-4 gap-6">
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#1A6BBF] mb-1">
                {semesters.reduce((sum, s) => sum + s.assigned, 0)}
              </p>
              <p className="text-xs text-[#666666]">Total asignaturas históricas</p>
            </div>
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#1A7A4A] mb-1">
                {Math.max(...semesters.map((s) => s.teachers))}
              </p>
              <p className="text-xs text-[#666666]">Máximo de docentes</p>
            </div>
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#E8A020] mb-1">
                {semesters.reduce((sum, s) => sum + s.conflicts, 0)}
              </p>
              <p className="text-xs text-[#666666]">Conflictos totales resueltos</p>
            </div>
            <div className="text-center p-4 bg-[#F5F5F5] rounded">
              <p className="text-2xl font-medium text-[#333333] mb-1">
                {semesters.reduce((sum, s) => sum + s.exports, 0)}
              </p>
              <p className="text-xs text-[#666666]">Documentos generados</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
