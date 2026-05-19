import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { GripVertical, CheckCircle, AlertTriangle, XCircle } from "lucide-react";

export function ManualAdjustmentView() {
  const [selectedSubject, setSelectedSubject] = useState<number | null>(null);

  const pendingSubjects = [
    { id: 1, code: "IS301", name: "Arquitectura de Software", credits: 4, status: "pending", teacher: "Dr. Ramírez" },
    { id: 2, code: "TP201", name: "Redes Avanzadas", credits: 3, status: "conflict", teacher: "Ing. López" },
    { id: 3, code: "MA201", name: "Estadística II", credits: 4, status: "blocked", teacher: "Msc. García" },
    { id: 4, code: "IS401", name: "Sistemas Distribuidos", credits: 4, status: "pending", teacher: "PhD. Torres" },
  ];

  const days = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  const assignedBlocks = [
    { id: 1, day: 0, hour: 8, duration: 2, subject: "Programación I", teacher: "Dr. Ramírez", room: "Lab 301" },
    { id: 2, day: 0, hour: 14, duration: 3, subject: "Base de Datos", teacher: "Ing. Torres", room: "Lab 302" },
    { id: 3, day: 1, hour: 10, duration: 2, subject: "Física III", teacher: "Dr. Sánchez", room: "Lab Física" },
    { id: 4, day: 2, hour: 8, duration: 2, subject: "Álgebra Lineal", teacher: "Msc. Martínez", room: "Aula 203" },
    { id: 5, day: 3, hour: 15, duration: 2, subject: "Redes I", teacher: "Ing. Pérez", room: "Lab 303" },
    { id: 6, day: 4, hour: 9, duration: 3, subject: "Ingeniería Software", teacher: "PhD. Gómez", room: "Aula 301" },
  ];

  const validations = [
    { rule: "Sin cruce docente", status: "success", message: "No hay conflictos de horario" },
    { rule: "Carga contractual", status: "warning", message: "Dr. Ramírez al 92% de capacidad" },
    { rule: "Franja bloqueada", status: "success", message: "No hay bloqueos en esta franja" },
  ];

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Ajuste Manual de Propuesta</h1>
          <p className="text-sm text-[#666666] mt-1">Reasignación de horarios con validación en tiempo real</p>
        </div>
        <Button>Guardar cambios</Button>
      </div>

      <div className="grid grid-cols-12 gap-6">
        {/* Left Panel - Pending Subjects */}
        <div className="col-span-3">
          <Card>
            <CardHeader>
              <CardTitle>Asignaturas Pendientes</CardTitle>
              <p className="text-sm text-[#666666] mt-1">{pendingSubjects.length} por ubicar</p>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                {pendingSubjects.map((subject) => (
                  <div
                    key={subject.id}
                    className={`p-3 rounded border cursor-move transition-all ${
                      selectedSubject === subject.id
                        ? "bg-[#1A6BBF]/10 border-[#1A6BBF]"
                        : "bg-white border-[#CCCCCC] hover:border-[#1A6BBF]"
                    }`}
                    onClick={() => setSelectedSubject(subject.id)}
                  >
                    <div className="flex items-start gap-2">
                      <GripVertical className="text-[#999999] mt-0.5 shrink-0" size={16} />
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <p className="text-sm font-medium text-[#333333] truncate">{subject.code}</p>
                          <Badge
                            variant={
                              subject.status === "pending"
                                ? "warning"
                                : subject.status === "conflict"
                                ? "error"
                                : "inactive"
                            }
                            className="text-xs"
                          >
                            {subject.status === "pending"
                              ? "Diurna"
                              : subject.status === "conflict"
                              ? "Cruce"
                              : "Bloqueada"}
                          </Badge>
                        </div>
                        <p className="text-xs text-[#666666] truncate mb-1">{subject.name}</p>
                        <p className="text-xs text-[#999999]">{subject.teacher}</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Center Panel - Weekly Grid */}
        <div className="col-span-6">
          <Card>
            <CardHeader>
              <CardTitle>Cuadrícula Semanal</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Arrastre las materias para reasignar</p>
            </CardHeader>
            <CardContent>
              <div className="border border-[#CCCCCC] rounded overflow-hidden bg-white">
                <div className="grid grid-cols-7 bg-[#333333]">
                  <div className="p-2 text-xs text-white font-medium text-center border-r border-white/20">
                    Hora
                  </div>
                  {days.map((day) => (
                    <div
                      key={day}
                      className="p-2 text-xs text-white font-medium text-center border-r border-white/20 last:border-r-0"
                    >
                      {day}
                    </div>
                  ))}
                </div>
                <div className="grid grid-cols-7" style={{ minHeight: "600px" }}>
                  {/* Time labels column */}
                  <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                    {hours.map((hour) => (
                      <div
                        key={hour}
                        className="h-[42.85px] border-b border-[#CCCCCC] px-2 py-1 text-xs text-[#666666]"
                      >
                        {hour}:00
                      </div>
                    ))}
                  </div>

                  {/* Days columns */}
                  {days.map((_, dayIdx) => (
                    <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                      {/* Background cells */}
                      {hours.map((hour) => (
                        <div
                          key={hour}
                          className="h-[42.85px] border-b border-[#CCCCCC] hover:bg-[#1A6BBF]/5 transition-colors cursor-pointer"
                        />
                      ))}

                      {/* Assigned blocks for this day */}
                      {assignedBlocks
                        .filter((block) => block.day === dayIdx)
                        .map((block) => (
                          <div
                            key={block.id}
                            className="absolute bg-[#1A6BBF] border-2 border-[#003087] text-white p-2 rounded text-xs overflow-hidden cursor-move hover:shadow-lg transition-shadow left-1 right-1"
                            style={{
                              top: `${((block.hour - 7) / 14) * 100}%`,
                              height: `${(block.duration / 14) * 100}%`,
                            }}
                          >
                            <div className="font-medium truncate mb-0.5">{block.subject}</div>
                            <div className="text-white/90 text-[10px] truncate">{block.teacher}</div>
                            <div className="text-white/80 text-[10px] truncate">
                              {block.hour}:00 - {block.hour + block.duration}:00
                            </div>
                          </div>
                        ))}
                    </div>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Right Panel - Validations */}
        <div className="col-span-3">
          <Card>
            <CardHeader>
              <CardTitle>Validaciones en Tiempo Real</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Estado de reglas institucionales</p>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {validations.map((validation, idx) => (
                  <div key={idx} className="space-y-2">
                    <div className="flex items-center justify-between">
                      <p className="text-sm font-medium text-[#333333]">{validation.rule}</p>
                      {validation.status === "success" ? (
                        <CheckCircle className="text-[#1A7A4A]" size={20} />
                      ) : validation.status === "warning" ? (
                        <AlertTriangle className="text-[#E8A020]" size={20} />
                      ) : (
                        <XCircle className="text-[#C0392B]" size={20} />
                      )}
                    </div>
                    <p className="text-xs text-[#666666]">{validation.message}</p>
                    <div className="h-1 rounded-full bg-[#F5F5F5] overflow-hidden">
                      <div
                        className={`h-full ${
                          validation.status === "success"
                            ? "bg-[#1A7A4A]"
                            : validation.status === "warning"
                            ? "bg-[#E8A020]"
                            : "bg-[#C0392B]"
                        }`}
                        style={{ width: validation.status === "success" ? "100%" : "75%" }}
                      />
                    </div>
                  </div>
                ))}
              </div>

              <div className="mt-6 pt-6 border-t border-[#CCCCCC]">
                <div className="bg-[#1A7A4A]/5 border border-[#1A7A4A]/20 rounded p-3 mb-3">
                  <div className="flex items-center gap-2">
                    <CheckCircle className="text-[#1A7A4A] shrink-0" size={16} />
                    <p className="text-xs text-[#1A7A4A] font-medium">
                      No hay conflictos en esta posición
                    </p>
                  </div>
                </div>

                <div className="space-y-2 text-xs text-[#666666]">
                  <div className="flex justify-between">
                    <span>Asignaturas ubicadas</span>
                    <span className="font-medium text-[#333333]">48 / 60</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Conflictos pendientes</span>
                    <span className="font-medium text-[#E8A020]">3</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Última modificación</span>
                    <span className="font-medium text-[#333333]">Hace 2 min</span>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
