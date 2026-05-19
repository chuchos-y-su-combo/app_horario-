import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import { Modal, ConfirmModal } from "../Modal";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Plus, Trash2, AlertTriangle } from "lucide-react";

export function BlockedSlotsView() {
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedBlock, setSelectedBlock] = useState<number | null>(null);
  const [formData, setFormData] = useState({
    reason: "",
    day: "",
    startTime: "",
    endTime: "",
    scope: "",
  });
  const [timeError, setTimeError] = useState("");

  const days = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  const blockedSlots = [
    { id: 1, day: 2, startHour: 12, duration: 1, reason: "Mantenimiento general", scope: "Todos los escenarios", impact: 4 },
    { id: 2, day: 4, startHour: 12, duration: 1, reason: "Evento institucional", scope: "Todos los escenarios", impact: 5 },
    { id: 3, day: 3, startHour: 18, duration: 2, reason: "Asamblea docente", scope: "Ingeniería Nocturna", impact: 2 },
  ];

  const activeBlocks = [
    { id: 1, reason: "Mantenimiento general", day: "Miércoles", time: "12:00 - 13:00", scope: "Todos los escenarios", created: "10 May 2026" },
    { id: 2, reason: "Evento institucional", day: "Viernes", time: "12:00 - 13:00", scope: "Todos los escenarios", created: "08 May 2026" },
    { id: 3, reason: "Asamblea docente", day: "Jueves", time: "18:00 - 20:00", scope: "Ingeniería Nocturna", created: "05 May 2026" },
    { id: 4, reason: "Reunión de coordinación", day: "Lunes", time: "16:00 - 17:00", scope: "TAPSI Diurno", created: "03 May 2026" },
  ];

  const handleCreateBlock = () => {
    if (!formData.reason || !formData.day || !formData.startTime || !formData.endTime || !formData.scope) {
      return;
    }

    const start = parseInt(formData.startTime);
    const end = parseInt(formData.endTime);

    if (end <= start) {
      setTimeError("La hora de fin debe ser posterior a la hora de inicio");
      return;
    }

    setTimeError("");
    console.log("Bloqueo creado:", formData);
    setShowCreateModal(false);
    setFormData({ reason: "", day: "", startTime: "", endTime: "", scope: "" });
  };

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Franjas Horarias Bloqueadas</h1>
          <p className="text-sm text-[#666666] mt-1">Gestión de bloqueos institucionales y restricciones de horario</p>
        </div>
        <Button className="gap-2" onClick={() => setShowCreateModal(true)}>
          <Plus size={20} />
          Crear bloqueo
        </Button>
      </div>

      <div className="grid grid-cols-3 gap-6">
        <div className="col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Calendario Semanal - Vista de Bloqueos</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Franjas bloqueadas se muestran con patrón rayado</p>
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
                        <div key={hour} className="h-[42.85px] border-b border-[#CCCCCC]" />
                      ))}

                      {/* Blocked slots for this day */}
                      {blockedSlots
                        .filter((block) => block.day === dayIdx)
                        .map((block) => (
                          <div
                            key={block.id}
                            className="absolute bg-[#595959] border-2 border-[#333333] text-white p-2 rounded text-xs overflow-hidden cursor-pointer hover:shadow-lg transition-shadow left-1 right-1"
                            style={{
                              top: `${((block.startHour - 7) / 14) * 100}%`,
                              height: `${(block.duration / 14) * 100}%`,
                              backgroundImage:
                                "repeating-linear-gradient(45deg, #595959, #595959 10px, #666666 10px, #666666 20px)",
                            }}
                            onClick={() => setSelectedBlock(block.id)}
                          >
                            <div className="font-medium truncate mb-0.5">BLOQUEADO</div>
                            <div className="text-white/90 text-[10px] truncate">{block.reason}</div>
                            <div className="text-white/80 text-[10px] truncate mt-1">
                              {block.startHour}:00 - {block.startHour + block.duration}:00
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

        <div>
          <Card>
            <CardHeader>
              <CardTitle>Formulario de Bloqueo</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <Input
                  label="Motivo del bloqueo"
                  placeholder="Ej: Mantenimiento, Evento institucional..."
                  value={formData.reason}
                  onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                />

                <Select
                  label="Día de la semana"
                  placeholder="Seleccionar día"
                  value={formData.day}
                  onChange={(e) => setFormData({ ...formData, day: e.target.value })}
                  options={days.map((day, idx) => ({ value: String(idx), label: day }))}
                />

                <div className="grid grid-cols-2 gap-3">
                  <Select
                    label="Hora inicio"
                    placeholder="Inicio"
                    value={formData.startTime}
                    onChange={(e) => {
                      setFormData({ ...formData, startTime: e.target.value });
                      setTimeError("");
                    }}
                    options={hours.map((h) => ({ value: String(h), label: `${h}:00` }))}
                    error={timeError}
                  />

                  <Select
                    label="Hora fin"
                    placeholder="Fin"
                    value={formData.endTime}
                    onChange={(e) => {
                      setFormData({ ...formData, endTime: e.target.value });
                      setTimeError("");
                    }}
                    options={hours.map((h) => ({ value: String(h), label: `${h}:00` }))}
                    error={timeError}
                  />
                </div>

                {timeError && (
                  <div className="bg-[#C0392B]/10 border border-[#C0392B]/20 rounded p-3">
                    <p className="text-sm text-[#C0392B]">{timeError}</p>
                  </div>
                )}

                <Select
                  label="Aplicar a"
                  placeholder="Seleccionar escenario"
                  value={formData.scope}
                  onChange={(e) => setFormData({ ...formData, scope: e.target.value })}
                  options={[
                    { value: "all", label: "Todos los escenarios" },
                    { value: "ing-diurna", label: "Ingeniería Diurna" },
                    { value: "ing-nocturna", label: "Ingeniería Nocturna" },
                    { value: "tapsi-diurno", label: "TAPSI Diurno" },
                    { value: "tapsi-nocturno", label: "TAPSI Nocturno" },
                  ]}
                />

                <Button
                  className="w-full"
                  onClick={handleCreateBlock}
                  disabled={
                    !formData.reason || !formData.day || !formData.startTime || !formData.endTime || !formData.scope
                  }
                >
                  Crear bloqueo
                </Button>

                {selectedBlock && (
                  <div className="pt-4 border-t border-[#CCCCCC]">
                    <div className="bg-[#E8A020]/5 border border-[#E8A020]/20 rounded p-3 mb-3">
                      <div className="flex items-start gap-2">
                        <AlertTriangle className="text-[#E8A020] shrink-0 mt-0.5" size={16} />
                        <p className="text-xs text-[#666666]">
                          Este bloqueo afecta {blockedSlots.find((b) => b.id === selectedBlock)?.impact} asignaturas
                          existentes
                        </p>
                      </div>
                    </div>
                    <Button
                      variant="destructive"
                      size="sm"
                      className="w-full"
                      onClick={() => setShowDeleteModal(true)}
                    >
                      Eliminar bloqueo seleccionado
                    </Button>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Bloqueos Activos</CardTitle>
          <p className="text-sm text-[#666666] mt-1">{activeBlocks.length} franjas bloqueadas</p>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Motivo</TableHead>
                <TableHead>Día</TableHead>
                <TableHead>Horario</TableHead>
                <TableHead>Aplicación</TableHead>
                <TableHead>Creado</TableHead>
                <TableHead>Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {activeBlocks.map((block) => (
                <TableRow key={block.id} striped>
                  <TableCell className="font-medium">{block.reason}</TableCell>
                  <TableCell>{block.day}</TableCell>
                  <TableCell>{block.time}</TableCell>
                  <TableCell>
                    <Badge variant="secondary" className="text-xs">
                      {block.scope}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-[#666666] text-xs">{block.created}</TableCell>
                  <TableCell>
                    <button
                      onClick={() => {
                        setSelectedBlock(block.id);
                        setShowDeleteModal(true);
                      }}
                      className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#C0392B]"
                    >
                      <Trash2 size={16} />
                    </button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <ConfirmModal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        onConfirm={() => {
          console.log("Bloqueo eliminado:", selectedBlock);
          setShowDeleteModal(false);
          setSelectedBlock(null);
        }}
        title="Eliminar bloqueo"
        message="¿Está seguro que desea eliminar este bloqueo? Esta acción puede afectar las asignaciones existentes en esa franja horaria."
        confirmText="Eliminar"
        cancelText="Cancelar"
        variant="danger"
      />
    </div>
  );
}
