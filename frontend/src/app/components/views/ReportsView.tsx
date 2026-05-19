import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { ProgressBar } from "../ProgressBar";
import { FileSpreadsheet, FileText, History, Download, Eye } from "lucide-react";

export function ReportsView() {
  const [selectedReport, setSelectedReport] = useState<string>("workload");

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

  const teacherWorkload = [
    { name: "Dr. Carlos Ramírez", contract: 25, assigned: 20, difference: 5, status: "success" },
    { name: "Msc. María López", contract: 15, assigned: 12, difference: 3, status: "success" },
    { name: "Ing. Juan Torres", contract: 25, assigned: 28, difference: -3, status: "error" },
    { name: "PhD. Ana García", contract: 25, assigned: 18, difference: 7, status: "success" },
    { name: "Msc. Pedro Sánchez", contract: 15, assigned: 9, difference: 6, status: "success" },
    { name: "Dr. Laura Martínez", contract: 25, assigned: 22, difference: 3, status: "success" },
    { name: "Ing. Roberto Silva", contract: 15, assigned: 15, difference: 0, status: "success" },
    { name: "PhD. Carmen Ruiz", contract: 25, assigned: 24, difference: 1, status: "success" },
  ];

  const recentExports = [
    { id: "1", name: "Horario_Ingenieria_Diurna_2026-1.pdf", type: "PDF", date: "12 May 2026 - 14:30", status: "Completado" },
    { id: "2", name: "Carga_Docente_Mayo.xlsx", type: "Excel", date: "10 May 2026 - 09:15", status: "Completado" },
    { id: "3", name: "Conflictos_Semestre_2026-1.xlsx", type: "Excel", date: "08 May 2026 - 16:45", status: "Completado" },
    { id: "4", name: "Horario_TAPSI_Nocturno_2026-1.pdf", type: "PDF", date: "05 May 2026 - 11:20", status: "Completado" },
  ];

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Reportes y Exportación</h1>
          <p className="text-sm text-[#666666] mt-1">Generación de informes y documentos institucionales</p>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-6">
        {reportTypes.map((report) => {
          const Icon = report.icon;
          return (
            <Card
              key={report.id}
              className={`cursor-pointer transition-all ${
                selectedReport === report.id
                  ? "ring-2 ring-[#1A6BBF] shadow-lg"
                  : "hover:shadow-md"
              }`}
              onClick={() => setSelectedReport(report.id)}
            >
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
          );
        })}
      </div>

      {selectedReport === "workload" && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-2">
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle>Vista Previa - Horas Asignadas vs Contrato</CardTitle>
                  <div className="flex gap-2">
                    <Button variant="secondary" size="sm" className="gap-2">
                      <Eye size={16} />
                      Vista completa
                    </Button>
                    <Button size="sm" className="gap-2">
                      <Download size={16} />
                      Exportar Excel
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
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
                          <span
                            className={`text-sm font-medium ${
                              isOverloaded ? "text-[#C0392B]" : "text-[#1A7A4A]"
                            }`}
                          >
                            {isOverloaded ? "-" : "+"}
                            {Math.abs(teacher.difference)}h
                          </span>
                        </div>
                      </div>
                    );
                  })}
                </div>

                <div className="mt-6 pt-6 border-t border-[#CCCCCC]">
                  <div className="grid grid-cols-3 gap-4 text-center">
                    <div>
                      <p className="text-sm text-[#666666]">Total asignadas</p>
                      <p className="text-xl font-medium text-[#333333]">
                        {teacherWorkload.reduce((sum, t) => sum + t.assigned, 0)}h
                      </p>
                    </div>
                    <div>
                      <p className="text-sm text-[#666666]">Total contractuales</p>
                      <p className="text-xl font-medium text-[#333333]">
                        {teacherWorkload.reduce((sum, t) => sum + t.contract, 0)}h
                      </p>
                    </div>
                    <div>
                      <p className="text-sm text-[#666666]">Docentes sobrecarga</p>
                      <p className="text-xl font-medium text-[#C0392B]">
                        {teacherWorkload.filter((t) => t.status === "error").length}
                      </p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div>
            <Card>
              <CardHeader>
                <CardTitle>Exportaciones Recientes</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {recentExports.map((exp) => (
                    <div key={exp.id} className="p-3 border border-[#CCCCCC] rounded hover:bg-[#F5F5F5] transition-colors">
                      <div className="flex items-start gap-3">
                        {exp.type === "PDF" ? (
                          <FileText className="text-[#C0392B] shrink-0 mt-0.5" size={20} />
                        ) : (
                          <FileSpreadsheet className="text-[#1A7A4A] shrink-0 mt-0.5" size={20} />
                        )}
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-[#333333] truncate mb-1">
                            {exp.name}
                          </p>
                          <p className="text-xs text-[#666666] mb-2">{exp.date}</p>
                          <Badge variant="success" className="text-xs">
                            {exp.status}
                          </Badge>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      )}

      {selectedReport === "schedule" && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Configuración de Exportación - Horario Semanal</CardTitle>
              <Button className="gap-2">
                <Download size={16} />
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
                    {["Ingeniería Diurna", "Ingeniería Nocturna", "TAPSI Diurno", "TAPSI Nocturno"].map(
                      (scenario) => (
                        <label key={scenario} className="flex items-center gap-2">
                          <input type="checkbox" className="rounded border-[#CCCCCC]" defaultChecked />
                          <span className="text-sm text-[#333333]">{scenario}</span>
                        </label>
                      )
                    )}
                  </div>
                </div>
                <div>
                  <label className="text-sm font-medium text-[#333333] mb-2 block">
                    Opciones de visualización
                  </label>
                  <div className="space-y-2">
                    <label className="flex items-center gap-2">
                      <input type="checkbox" className="rounded border-[#CCCCCC]" defaultChecked />
                      <span className="text-sm text-[#333333]">Incluir nombres de docentes</span>
                    </label>
                    <label className="flex items-center gap-2">
                      <input type="checkbox" className="rounded border-[#CCCCCC]" defaultChecked />
                      <span className="text-sm text-[#333333]">Mostrar salones asignados</span>
                    </label>
                    <label className="flex items-center gap-2">
                      <input type="checkbox" className="rounded border-[#CCCCCC]" />
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

      {selectedReport === "conflicts" && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Configuración de Exportación - Conflictos y Excepciones</CardTitle>
              <Button className="gap-2">
                <Download size={16} />
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
                  <p className="text-2xl font-medium text-[#C0392B]">5</p>
                  <p className="text-xs text-[#666666]">Errores críticos</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-medium text-[#E8A020]">8</p>
                  <p className="text-xs text-[#666666]">Advertencias</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-medium text-[#1A7A4A]">24</p>
                  <p className="text-xs text-[#666666]">Resueltos</p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
