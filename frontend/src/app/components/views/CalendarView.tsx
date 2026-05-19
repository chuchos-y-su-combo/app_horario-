import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Select } from "../Select";
import { Button } from "../Button";
import { Download, User, Home } from "lucide-react";

export function CalendarView() {
  const [scenarioFilter, setScenarioFilter] = useState("");
  const [teacherFilter, setTeacherFilter] = useState("");
  const [roomFilter, setRoomFilter] = useState("");

  const days = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  const scheduleBlocks = [
    { day: 0, hour: 8, duration: 2, subject: "Programación I", teacher: "Dr. Ramírez", room: "Lab 301", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 0, hour: 10, duration: 2, subject: "Cálculo I", teacher: "Msc. López", room: "Aula 205", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 0, hour: 14, duration: 3, subject: "Base de Datos", teacher: "Ing. Torres", room: "Lab 302", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 1, hour: 8, duration: 2, subject: "Fundamentos TAPSI", teacher: "PhD. García", room: "Aula 101", color: "bg-[#003087]", border: "border-[#003087]" },
    { day: 1, hour: 10, duration: 3, subject: "Física III", teacher: "Dr. Sánchez", room: "Lab Física", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 2, hour: 8, duration: 2, subject: "Álgebra Lineal", teacher: "Msc. Martínez", room: "Aula 203", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 2, hour: 15, duration: 2, subject: "Redes I", teacher: "Ing. Pérez", room: "Lab 303", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 3, hour: 9, duration: 2, subject: "Programación II", teacher: "Dr. Ramírez", room: "Lab 301", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 3, hour: 14, duration: 3, subject: "Sistemas Operativos", teacher: "Msc. Castro", room: "Lab 304", color: "bg-[#003087]", border: "border-[#003087]" },
    { day: 4, hour: 8, duration: 2, subject: "Ingeniería Software", teacher: "PhD. Gómez", room: "Aula 301", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 4, hour: 15, duration: 2, subject: "Estructuras de Datos", teacher: "Ing. Díaz", room: "Lab 302", color: "bg-[#1A6BBF]", border: "border-[#003087]" },
    { day: 5, hour: 8, duration: 3, subject: "Proyecto Final", teacher: "Dr. Morales", room: "Aula 401", color: "bg-[#003087]", border: "border-[#003087]" },
    // Bloques bloqueados
    { day: 2, hour: 12, duration: 1, subject: "BLOQUEADO", teacher: "Mantenimiento", room: "", color: "bg-[#595959]", border: "border-[#333333]", pattern: "diagonal-stripes" },
    { day: 4, hour: 12, duration: 1, subject: "BLOQUEADO", teacher: "Evento institucional", room: "", color: "bg-[#595959]", border: "border-[#333333]", pattern: "diagonal-stripes" },
  ];

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Calendario Semanal</h1>
          <p className="text-sm text-[#666666] mt-1">Vista completa del horario académico</p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" className="gap-2">
            <User size={20} />
            Vista docente
          </Button>
          <Button className="gap-2">
            <Download size={20} />
            Exportar PDF
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <Select
              placeholder="Filtrar por escenario"
              value={scenarioFilter}
              onChange={(e) => setScenarioFilter(e.target.value)}
              options={[
                { value: "ing-diurna", label: "Ingeniería Diurna" },
                { value: "ing-nocturna", label: "Ingeniería Nocturna" },
                { value: "tapsi-diurno", label: "TAPSI Diurno" },
                { value: "tapsi-nocturno", label: "TAPSI Nocturno" },
              ]}
              className="flex-1"
            />
            <Select
              placeholder="Filtrar por docente"
              value={teacherFilter}
              onChange={(e) => setTeacherFilter(e.target.value)}
              options={[
                { value: "ramirez", label: "Dr. Carlos Ramírez" },
                { value: "lopez", label: "Msc. María López" },
                { value: "torres", label: "Ing. Juan Torres" },
              ]}
              className="flex-1"
            />
            <Select
              placeholder="Filtrar por aula"
              value={roomFilter}
              onChange={(e) => setRoomFilter(e.target.value)}
              options={[
                { value: "lab301", label: "Lab 301" },
                { value: "aula205", label: "Aula 205" },
                { value: "lab302", label: "Lab 302" },
              ]}
              className="flex-1"
            />
          </div>
        </CardHeader>
        <CardContent>
          <div className="border border-[#CCCCCC] rounded-lg overflow-hidden bg-white">
            <div className="grid grid-cols-7 bg-[#333333]">
              <div className="p-3 text-sm text-white font-medium text-center border-r border-white/20">
                Hora
              </div>
              {days.map((day) => (
                <div key={day} className="p-3 text-sm text-white font-medium text-center border-r border-white/20 last:border-r-0">
                  {day}
                </div>
              ))}
            </div>
            <div className="grid grid-cols-7" style={{ minHeight: "800px" }}>
              {/* Time labels column */}
              <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                {hours.map((hour) => (
                  <div key={hour} className="h-[57px] border-b border-[#CCCCCC] px-3 py-2 text-sm text-[#666666] font-medium">
                    {hour}:00
                  </div>
                ))}
              </div>

              {/* Days columns */}
              {days.map((_, dayIdx) => (
                <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                  {/* Background cells */}
                  {hours.map((hour) => (
                    <div key={hour} className="h-[57px] border-b border-[#CCCCCC]" />
                  ))}

                  {/* Schedule blocks for this day */}
                  {scheduleBlocks
                    .filter((block) => block.day === dayIdx)
                    .map((block, idx) => (
                      <div
                        key={idx}
                        className={`absolute ${block.color} ${block.border} border-2 text-white p-2 rounded text-xs overflow-hidden cursor-pointer hover:shadow-lg transition-shadow left-1 right-1 ${
                          block.pattern === "diagonal-stripes"
                            ? "bg-[repeating-linear-gradient(45deg,#595959,#595959_10px,#666666_10px,#666666_20px)]"
                            : ""
                        }`}
                        style={{
                          top: `${((block.hour - 7) / 14) * 100}%`,
                          height: `${(block.duration / 14) * 100}%`,
                        }}
                      >
                        <div className="font-medium mb-1 truncate">{block.subject}</div>
                        <div className="text-white/90 text-[10px] truncate flex items-center gap-1">
                          <User size={10} />
                          {block.teacher}
                        </div>
                        {block.room && (
                          <div className="text-white/90 text-[10px] truncate flex items-center gap-1 mt-0.5">
                            <Home size={10} />
                            {block.room}
                          </div>
                        )}
                      </div>
                    ))}
                </div>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Leyenda</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-6">
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 bg-[#1A6BBF] border-2 border-[#003087] rounded" />
              <span className="text-sm text-[#666666]">Ingeniería</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 bg-[#003087] border-2 border-[#003087] rounded" />
              <span className="text-sm text-[#666666]">TAPSI</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 bg-[#595959] border-2 border-[#333333] rounded bg-[repeating-linear-gradient(45deg,#595959,#595959_4px,#666666_4px,#666666_8px)]" />
              <span className="text-sm text-[#666666]">Bloqueo</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
