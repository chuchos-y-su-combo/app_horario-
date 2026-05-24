import { useEffect, useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { GripVertical, Loader2, BookOpen } from "lucide-react";
import { manualAdjustmentService } from "../../../services/manual-adjustment.service";

interface Asignacion {
  id: string;
  idAsignatura: string;
  codigoAsignatura: string;
  nombreAsignatura: string;
  nombreDocente: string;
  dia: number | null;
  horaInicio: string | null;
  horaFin: string | null;
  estado: string;
}

export function ManualAdjustmentView() {
  const [asignaciones, setAsignaciones] = useState<Asignacion[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);

  const days = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  useEffect(() => {
    cargarAsignaciones();
  }, []);

  const cargarAsignaciones = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await manualAdjustmentService.obtenerAsignaciones();
      setAsignaciones(Array.isArray(data) ? data : []);
    } catch (err: any) {
      setError(err.response?.data?.mensaje || "Error al cargar asignaciones");
    } finally {
      setLoading(false);
    }
  };

  const guardarCambios = async () => {
    setGuardando(true);
    try {
      for (const a of assignedBlocks) {
        await manualAdjustmentService.ajustarAsignacion(a.id, {
          dia: a.dia,
          horaInicio: a.horaInicio ?? undefined,
          horaFin: a.horaFin ?? undefined,
        });
      }
      await cargarAsignaciones();
    } catch (err: any) {
      setError(err.response?.data?.mensaje || "Error al guardar cambios");
    } finally {
      setGuardando(false);
    }
  };

  const pendingSubjects = asignaciones.filter((a) => a.dia === null);
  const assignedBlocks = asignaciones.filter((a) => a.dia !== null);

  const parseHour = (hora: string | null) => {
    if (!hora) return 7;
    return parseInt(hora.split(":")[0], 10);
  };

  const calcDuration = (inicio: string | null, fin: string | null) => {
    if (!inicio || !fin) return 1;
    return parseHour(fin) - parseHour(inicio) || 1;
  };

  if (loading) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <div className="text-center">
          <Loader2 className="w-8 h-8 animate-spin text-[#1A6BBF] mx-auto" />
          <p className="mt-4 text-[#666666]">Cargando asignaciones...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Ajuste Manual de Propuesta</h1>
          <p className="text-sm text-[#666666] mt-1">Reasignación de horarios con validación en tiempo real</p>
        </div>
        <Button onClick={guardarCambios} disabled={guardando || assignedBlocks.length === 0}>
          {guardando ? <Loader2 className="animate-spin mr-2" size={16} /> : null}
          Guardar cambios
        </Button>
      </div>

      {error && (
        <div className="bg-[#C0392B]/10 border border-[#C0392B]/30 text-[#C0392B] rounded p-3 text-sm">
          {error}
        </div>
      )}

      <div className="grid grid-cols-12 gap-6">
        {/* Panel izquierdo — Asignaturas sin ubicar */}
        <div className="col-span-3">
          <Card>
            <CardHeader>
              <CardTitle>Asignaturas Pendientes</CardTitle>
              <p className="text-sm text-[#666666] mt-1">{pendingSubjects.length} por ubicar</p>
            </CardHeader>
            <CardContent>
              {pendingSubjects.length === 0 ? (
                <div className="text-center py-8 border-2 border-dashed border-[#CCCCCC] rounded">
                  <BookOpen className="mx-auto text-[#CCCCCC] mb-2" size={24} />
                  <p className="text-sm text-[#999999]">Sin asignaturas pendientes</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {pendingSubjects.map((a) => (
                    <div
                      key={a.id}
                      className={`p-3 rounded border cursor-pointer transition-all ${
                        selectedId === a.id
                          ? "bg-[#1A6BBF]/10 border-[#1A6BBF]"
                          : "bg-white border-[#CCCCCC] hover:border-[#1A6BBF]"
                      }`}
                      onClick={() => setSelectedId(a.id)}
                    >
                      <div className="flex items-start gap-2">
                        <GripVertical className="text-[#999999] mt-0.5 shrink-0" size={16} />
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <p className="text-sm font-medium text-[#333333] truncate">{a.codigoAsignatura}</p>
                            <Badge variant="warning" className="text-xs">Pendiente</Badge>
                          </div>
                          <p className="text-xs text-[#666666] truncate mb-1">{a.nombreAsignatura}</p>
                          <p className="text-xs text-[#999999]">{a.nombreDocente}</p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Panel central — Cuadrícula semanal */}
        <div className="col-span-6">
          <Card>
            <CardHeader>
              <CardTitle>Cuadrícula Semanal</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Propuesta actual de horarios</p>
            </CardHeader>
            <CardContent>
              <div className="border border-[#CCCCCC] rounded overflow-hidden bg-white">
                <div className="grid grid-cols-7 bg-[#333333]">
                  <div className="p-2 text-xs text-white font-medium text-center border-r border-white/20">Hora</div>
                  {days.map((day) => (
                    <div key={day} className="p-2 text-xs text-white font-medium text-center border-r border-white/20 last:border-r-0">
                      {day}
                    </div>
                  ))}
                </div>
                <div className="grid grid-cols-7" style={{ minHeight: "600px" }}>
                  <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                    {hours.map((hour) => (
                      <div key={hour} className="h-[42.85px] border-b border-[#CCCCCC] px-2 py-1 text-xs text-[#666666]">
                        {hour}:00
                      </div>
                    ))}
                  </div>
                  {days.map((_, dayIdx) => (
                    <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                      {hours.map((hour) => (
                        <div key={hour} className="h-[42.85px] border-b border-[#CCCCCC] hover:bg-[#1A6BBF]/5 transition-colors" />
                      ))}
                      {assignedBlocks
                        .filter((a) => a.dia === dayIdx)
                        .map((a) => {
                          const startH = parseHour(a.horaInicio);
                          const dur = calcDuration(a.horaInicio, a.horaFin);
                          return (
                            <div
                              key={a.id}
                              className="absolute bg-[#1A6BBF] border-2 border-[#003087] text-white p-2 rounded text-xs overflow-hidden cursor-pointer hover:shadow-lg transition-shadow left-1 right-1"
                              style={{
                                top: `${((startH - 7) / 14) * 100}%`,
                                height: `${(dur / 14) * 100}%`,
                              }}
                              onClick={() => setSelectedId(a.id)}
                            >
                              <div className="font-medium truncate mb-0.5">{a.nombreAsignatura}</div>
                              <div className="text-white/90 text-[10px] truncate">{a.nombreDocente}</div>
                              <div className="text-white/80 text-[10px]">
                                {a.horaInicio} - {a.horaFin}
                              </div>
                            </div>
                          );
                        })}
                    </div>
                  ))}
                </div>
              </div>

              {assignedBlocks.length === 0 && !loading && (
                <div className="text-center py-8 text-[#999999] text-sm mt-4">
                  No hay asignaciones en la propuesta actual. Genera un horario primero.
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Panel derecho — Resumen */}
        <div className="col-span-3">
          <Card>
            <CardHeader>
              <CardTitle>Resumen</CardTitle>
              <p className="text-sm text-[#666666] mt-1">Estado de la propuesta</p>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Asignaturas ubicadas</span>
                  <span className="font-medium text-[#333333]">{assignedBlocks.length}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Sin ubicar</span>
                  <span className="font-medium text-[#E8A020]">{pendingSubjects.length}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Total asignaciones</span>
                  <span className="font-medium text-[#333333]">{asignaciones.length}</span>
                </div>
              </div>

              {asignaciones.length === 0 && (
                <div className="mt-6 p-3 bg-[#F5F5F5] border border-[#CCCCCC] rounded text-xs text-[#666666]">
                  No hay propuesta activa. Ve a Generación para crear una propuesta de horarios.
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
