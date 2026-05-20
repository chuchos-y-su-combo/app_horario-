import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { ProgressBar } from "../ProgressBar";
import { GraduationCap, BookOpen, AlertTriangle, TrendingUp } from "lucide-react";

export function DashboardView() {
  const kpis = [
    { icon: GraduationCap, label: "Docentes Activos", value: "124", color: "text-[#1A6BBF]", bg: "bg-[#1A6BBF]/10" },
    { icon: BookOpen, label: "Asignaturas Totales", value: "68", color: "text-[#003087]", bg: "bg-[#003087]/10" },
    { icon: AlertTriangle, label: "Conflictos Abiertos", value: "12", color: "text-[#E8A020]", bg: "bg-[#E8A020]/10" },
    { icon: TrendingUp, label: "Avance de Horario", value: "78%", color: "text-[#1A7A4A]", bg: "bg-[#1A7A4A]/10" },
  ];

  const scenarios = [
    { name: "Ingeniería Diurna", status: "Activo", badge: "primary", groups: 24, progress: 85 },
    { name: "Ingeniería Nocturna", status: "En validación", badge: "warning", groups: 18, progress: 72 },
    { name: "TAPSI Diurno", status: "Activo", badge: "primary", groups: 16, progress: 90 },
    { name: "TAPSI Nocturno", status: "Pendiente", badge: "inactive", groups: 12, progress: 45 },
  ];

  const scheduleBlocks = [
    { day: 0, hour: 8, duration: 2, subject: "Cálculo I", teacher: "Dr. Ramírez", color: "bg-[#1A6BBF]" },
    { day: 0, hour: 14, duration: 3, subject: "Programación II", teacher: "Ing. López", color: "bg-[#1A6BBF]" },
    { day: 1, hour: 10, duration: 2, subject: "Física III", teacher: "Dr. García", color: "bg-[#003087]" },
    { day: 2, hour: 8, duration: 2, subject: "Base de Datos", teacher: "Msc. Torres", color: "bg-[#1A6BBF]" },
    { day: 3, hour: 15, duration: 2, subject: "Álgebra Lineal", teacher: "Dr. Sánchez", color: "bg-[#003087]" },
    { day: 4, hour: 9, duration: 3, subject: "Ingeniería de Software", teacher: "PhD. Martínez", color: "bg-[#1A6BBF]" },
  ];

  const alerts = [
    { type: "error", message: "Cruce de horario: Dr. Ramírez tiene 2 asignaturas simultáneas el lunes a las 10:00" },
    { type: "warning", message: "Materia 'Circuitos Digitales' no tiene salón asignado" },
    { type: "warning", message: "Ing. López supera su carga contractual por 3 horas" },
  ];

  const days = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* KPIs Row */}
      <div className="grid grid-cols-4 gap-6">
        {kpis.map((kpi, idx) => {
          const Icon = kpi.icon;
          return (
            <Card key={idx} className="flex items-center gap-4">
              <div className={`w-12 h-12 rounded-lg ${kpi.bg} flex items-center justify-center`}>
                <Icon className={kpi.color} size={24} />
              </div>
              <div>
                <p className="text-sm text-[#666666]">{kpi.label}</p>
                <p className="text-2xl font-medium text-[#333333]">{kpi.value}</p>
              </div>
            </Card>
          );
        })}
      </div>

      {/* Scenarios Row */}
      <div className="grid grid-cols-4 gap-6">
        {scenarios.map((scenario, idx) => (
          <Card key={idx}>
            <CardHeader>
              <div className="flex items-center justify-between mb-2">
                <CardTitle className="text-base">{scenario.name}</CardTitle>
                <Badge variant={scenario.badge as any}>{scenario.status}</Badge>
              </div>
              <p className="text-sm text-[#666666]">{scenario.groups} grupos</p>
            </CardHeader>
            <CardContent>
              <ProgressBar
                value={scenario.progress}
                variant={scenario.progress >= 80 ? "success" : scenario.progress >= 60 ? "primary" : "warning"}
              />
              <p className="text-xs text-[#666666] mt-2">{scenario.progress}% completado</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-3 gap-6">
        {/* Calendar - Spans 2 columns */}
        <div className="col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Calendario Semanal Preliminar</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Vista general del horario en construcción (solo lectura)</p>
            </CardHeader>
            <CardContent>
              <div className="border border-[#CCCCCC] rounded overflow-hidden">
                <div className="grid grid-cols-7 bg-[#333333]">
                  <div className="p-2 text-xs text-white text-center border-r border-white/20">Hora</div>
                  {days.map((day) => (
                    <div key={day} className="p-2 text-xs text-white text-center border-r border-white/20 last:border-r-0">
                      {day}
                    </div>
                  ))}
                </div>
                <div className="grid grid-cols-7 relative" style={{ height: "400px" }}>
                  {/* Time labels column */}
                  <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                    {hours.map((hour) => (
                      <div key={hour} className="h-[28.5px] border-b border-[#CCCCCC] px-2 py-1 text-xs text-[#666666]">
                        {hour}:00
                      </div>
                    ))}
                  </div>

                  {/* Days columns */}
                  {days.map((_, dayIdx) => (
                    <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                      {/* Background cells */}
                      {hours.map((hour) => (
                        <div key={hour} className="h-[28.5px] border-b border-[#CCCCCC]" />
                      ))}

                      {/* Schedule blocks for this day */}
                      {scheduleBlocks
                        .filter((block) => block.day === dayIdx)
                        .map((block, idx) => (
                          <div
                            key={idx}
                            className={`absolute ${block.color} text-white p-2 rounded text-xs overflow-hidden left-1 right-1`}
                            style={{
                              top: `${((block.hour - 7) / 14) * 100}%`,
                              height: `${(block.duration / 14) * 100}%`,
                            }}
                          >
                            <div className="font-medium truncate">{block.subject}</div>
                            <div className="text-white/80 truncate text-[10px]">{block.teacher}</div>
                          </div>
                        ))}
                    </div>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Right Panel - Alerts */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Alertas Pendientes</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {alerts.map((alert, idx) => (
                  <div
                    key={idx}
                    className={`p-3 rounded border ${
                      alert.type === "error"
                        ? "bg-[#C0392B]/5 border-[#C0392B]/20"
                        : "bg-[#E8A020]/5 border-[#E8A020]/20"
                    }`}
                  >
                    <div className="flex items-start gap-2">
                      <Badge variant={alert.type as any} className="shrink-0">
                        {alert.type === "error" ? "Error" : "Advertencia"}
                      </Badge>
                      <p className="text-xs text-[#333333] flex-1">{alert.message}</p>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Pendientes Inmediatos</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <div className="flex items-start gap-3">
                  <div className="w-2 h-2 rounded-full bg-[#1A6BBF] mt-1.5 shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm text-[#333333] font-medium">Validar disponibilidad docentes</p>
                    <p className="text-xs text-[#666666]">15 docentes sin cargar horario</p>
                  </div>
                </div>
                <div className="flex items-start gap-3">
                  <div className="w-2 h-2 rounded-full bg-[#E8A020] mt-1.5 shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm text-[#333333] font-medium">Asignar salones faltantes</p>
                    <p className="text-xs text-[#666666]">8 materias pendientes</p>
                  </div>
                </div>
                <div className="flex items-start gap-3">
                  <div className="w-2 h-2 rounded-full bg-[#1A7A4A] mt-1.5 shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm text-[#333333] font-medium">Revisar materias fijas TAPSI</p>
                    <p className="text-xs text-[#666666]">Todas validadas</p>
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
