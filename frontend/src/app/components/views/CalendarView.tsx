// CalendarView.tsx - Versión completa y corregida
import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Select } from "../Select";
import { Button } from "../Button";
import { Download, User, Home, AlertCircle, RefreshCw } from "lucide-react";
import { calendarioService } from "../../../services/calendarioService";

interface PlanEstudio {
  idPlan: string;
  nombrePlan: string;
  jornada: string;
}

interface BloqueCalendario {
  idAsignacion: string;
  horaInicio: string;
  horaFin: string;
  nombreAsignatura: string;
  codigoAsignatura: string;
  nombreDocente: string;
  escenario: string;
  jornada: string;
  nombrePlan: string;
  idPlan: string;
  estado: string;
}

interface DiaCalendario {
  numeroDia: number;
  nombreDia: string;
  bloques: BloqueCalendario[];
}

interface CalendarioData {
  semestre: string;
  idPlanFiltro?: string;
  jornadaFiltro?: string;
  dias: DiaCalendario[];
}

export function CalendarView() {
  const [scenarioFilter, setScenarioFilter] = useState("");
  const [teacherFilter, setTeacherFilter] = useState("");
  const [roomFilter, setRoomFilter] = useState("");
  const [calendario, setCalendario] = useState<CalendarioData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedSemestre, setSelectedSemestre] = useState("2026-1");
  const [planes, setPlanes] = useState<PlanEstudio[]>([]);
  const [idPlanIngenieria, setIdPlanIngenieria] = useState<string>("");
  const [idPlanTapsi, setIdPlanTapsi] = useState<string>("");
  const [docentes, setDocentes] = useState<{ idDocente: string; nombre: string }[]>([]);
  const [escenarios, setEscenarios] = useState<string[]>([]);
  const [generandoPropuestas, setGenerandoPropuestas] = useState(false);

  const days = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  // Cargar planes de estudio al inicio
  useEffect(() => {
    cargarPlanes();
    cargarDocentes();
    cargarEscenarios();
  }, []);

  const cargarPlanes = async () => {
    try {
      const data = await calendarioService.getPlanesEstudio();
      setPlanes(data);
      
      const planIng = data.find((p: PlanEstudio) => 
        p.nombrePlan.toLowerCase().includes("ingenieria") || 
        p.nombrePlan.toLowerCase().includes("ingeniería")
      );
      const planTapsi = data.find((p: PlanEstudio) => 
        p.nombrePlan.toLowerCase().includes("tapsi")
      );
      
      if (planIng) setIdPlanIngenieria(planIng.idPlan);
      if (planTapsi) setIdPlanTapsi(planTapsi.idPlan);
      
      console.log("Planes cargados:", { planIng, planTapsi });
    } catch (err) {
      console.error("Error cargando planes:", err);
    }
  };

  const cargarDocentes = async () => {
    try {
      const data = await calendarioService.getDocentes();
      setDocentes(data);
    } catch (err) {
      console.error("Error cargando docentes:", err);
    }
  };

  const cargarEscenarios = async () => {
    try {
      const data = await calendarioService.getEscenarios();
      setEscenarios(data);
    } catch (err) {
      console.error("Error cargando escenarios:", err);
    }
  };

  // Cargar calendario cuando cambien los filtros
  useEffect(() => {
    cargarCalendario();
  }, [selectedSemestre, scenarioFilter]);

  const cargarCalendario = async () => {
    setLoading(true);
    setError(null);
    try {
      let idPlan: string | undefined;
      let jornada: string | undefined;

      switch (scenarioFilter) {
        case "ing-diurna":
          idPlan = idPlanIngenieria || undefined;
          jornada = "Diurna";
          break;
        case "ing-nocturna":
          idPlan = idPlanIngenieria || undefined;
          jornada = "Nocturna";
          break;
        case "tapsi-diurno":
          idPlan = idPlanTapsi || undefined;
          jornada = "Diurna";
          break;
        case "tapsi-nocturno":
          idPlan = idPlanTapsi || undefined;
          jornada = "Nocturna";
          break;
      }

      console.log("Consultando calendario con:", { semestre: selectedSemestre, idPlan, jornada });

      const data = await calendarioService.getCalendarioSemanal(
        selectedSemestre,
        idPlan,
        jornada
      );
      
      setCalendario(data);
      
      if (!data?.dias?.some((d: DiaCalendario) => d.bloques?.length > 0)) {
        setError("No hay horarios asignados para este periodo. Genera propuestas primero.");
      }
    } catch (err) {
      console.error("Error cargando calendario:", err);
      setError("No se pudo cargar el calendario. Verifica que el backend esté corriendo.");
    } finally {
      setLoading(false);
    }
  };

  const handleGenerarPropuestas = async () => {
    setGenerandoPropuestas(true);
    try {
      await calendarioService.generarPropuestas(
        selectedSemestre,
        ["ING_DIURNA", "ING_NOCTURNA", "TAPSI_DIURNA", "TAPSI_NOCTURNA"],
        1
      );
      alert("Propuestas generadas exitosamente");
      cargarCalendario();
    } catch (err) {
      console.error("Error generando propuestas:", err);
      alert("Error al generar propuestas");
    } finally {
      setGenerandoPropuestas(false);
    }
  };

  const horaStringToNumber = (horaStr: string): number => {
    if (!horaStr) return 0;
    const [hora] = horaStr.split(":");
    return parseInt(hora, 10);
  };

  const calcularDuracion = (horaInicio: string, horaFin: string): number => {
    if (!horaInicio || !horaFin) return 1;
    const inicio = horaStringToNumber(horaInicio);
    const fin = horaStringToNumber(horaFin);
    return Math.max(fin - inicio, 1);
  };

  const getColorPorPlan = (nombrePlan: string): string => {
    if (!nombrePlan) return "bg-[#1A6BBF]";
    if (nombrePlan.toLowerCase().includes("ingenieria") || nombrePlan.toLowerCase().includes("ingeniería")) {
      return "bg-[#1A6BBF]";
    }
    if (nombrePlan.toLowerCase().includes("tapsi")) {
      return "bg-[#003087]";
    }
    return "bg-[#1A6BBF]";
  };

  const bloquesFiltrados = () => {
    if (!calendario?.dias) return [];
    
    const todosBloques = calendario.dias.flatMap((dia: DiaCalendario) => 
      (dia.bloques || []).map((bloque: BloqueCalendario) => ({
        ...bloque,
        day: dia.numeroDia - 1,
        hour: horaStringToNumber(bloque.horaInicio),
        duration: calcularDuracion(bloque.horaInicio, bloque.horaFin),
        color: getColorPorPlan(bloque.nombrePlan),
        room: bloque.escenario
      }))
    );

    return todosBloques.filter((bloque: any) => {
      if (teacherFilter && !bloque.nombreDocente?.toLowerCase().includes(teacherFilter.toLowerCase())) {
        return false;
      }
      if (roomFilter && !bloque.escenario?.toLowerCase().includes(roomFilter.toLowerCase())) {
        return false;
      }
      return true;
    });
  };

  const scheduleBlocks = bloquesFiltrados();

  const exportarPDF = () => {
    // Implementar exportación a PDF
    console.log("Exportando a PDF...");
    alert("Función de exportación en desarrollo");
  };

  if (loading) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#003087] mx-auto"></div>
          <p className="mt-4 text-[#666666]">Cargando calendario...</p>
        </div>
      </div>
    );
  }

  if (error && !calendario) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <Card className="max-w-md">
          <CardContent className="pt-6">
            <div className="text-center">
              <AlertCircle size={48} className="mx-auto text-red-500 mb-4" />
              <p className="text-[#666666]">{error}</p>
              <div className="flex gap-3 mt-4 justify-center">
                <Button onClick={cargarCalendario} className="mt-4">
                  <RefreshCw size={16} className="mr-2" />
                  Reintentar
                </Button>
                <Button 
                  variant="secondary" 
                  onClick={handleGenerarPropuestas}
                  disabled={generandoPropuestas}
                  className="mt-4"
                >
                  {generandoPropuestas ? "Generando..." : "Generar propuestas"}
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Calendario Semanal</h1>
          <p className="text-sm text-[#666666] mt-1">
            {calendario?.semestre || selectedSemestre} - Total bloques: {scheduleBlocks.length}
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" className="gap-2" onClick={cargarCalendario}>
            <RefreshCw size={20} />
            Actualizar
          </Button>
          <Button variant="secondary" className="gap-2" onClick={handleGenerarPropuestas} disabled={generandoPropuestas}>
            {generandoPropuestas ? "Generando..." : "Generar propuestas"}
          </Button>
          <Button className="gap-2" onClick={exportarPDF}>
            <Download size={20} />
            Exportar PDF
          </Button>
        </div>
      </div>

      {/* Filtros */}
      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-4">
            <Select
              placeholder="Seleccionar semestre"
              value={selectedSemestre}
              onChange={(e) => setSelectedSemestre(e.target.value)}
              options={[
                { value: "2026-1", label: "2026-1" },
                { value: "2026-2", label: "2026-2" },
                { value: "2025-1", label: "2025-1" },
                { value: "2025-2", label: "2025-2" },
              ]}
              className="w-full"
            />
            <Select
              placeholder="Filtrar por escenario"
              value={scenarioFilter}
              onChange={(e) => setScenarioFilter(e.target.value)}
              options={[
                { value: "", label: "Todos" },
                { value: "ing-diurna", label: "Ingeniería Diurna" },
                { value: "ing-nocturna", label: "Ingeniería Nocturna" },
                { value: "tapsi-diurno", label: "TAPSI Diurno" },
                { value: "tapsi-nocturno", label: "TAPSI Nocturno" },
              ]}
              className="w-full"
            />
            <Select
              placeholder="Filtrar por docente"
              value={teacherFilter}
              onChange={(e) => setTeacherFilter(e.target.value)}
              options={[
                { value: "", label: "Todos" },
                ...docentes.map(d => ({ value: d.nombre, label: d.nombre }))
              ]}
              className="w-full"
            />
            <Select
              placeholder="Filtrar por escenario/aula"
              value={roomFilter}
              onChange={(e) => setRoomFilter(e.target.value)}
              options={[
                { value: "", label: "Todos" },
                ...escenarios.map(e => ({ value: e, label: e }))
              ]}
              className="w-full"
            />
          </div>
        </CardHeader>
      </Card>

      {/* Calendario */}
      <Card>
        <CardContent className="p-0 overflow-x-auto">
          <div className="border border-[#CCCCCC] rounded-lg overflow-hidden bg-white min-w-[800px]">
            {/* Header de días */}
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
            
            {/* Cuerpo del calendario */}
            <div className="grid grid-cols-7" style={{ minHeight: "700px" }}>
              {/* Columna de horas */}
              <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                {hours.map((hour) => (
                  <div key={hour} className="h-[57px] border-b border-[#CCCCCC] px-3 py-2 text-sm text-[#666666] font-medium">
                    {hour}:00
                  </div>
                ))}
              </div>

              {/* Columnas de días */}
              {days.map((_, dayIdx) => (
                <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                  {/* Celdas de fondo */}
                  {hours.map((hour) => (
                    <div key={hour} className="h-[57px] border-b border-[#CCCCCC] bg-white" />
                  ))}

                  {/* Bloques de horario */}
                  {scheduleBlocks
                    .filter((block: any) => block.day === dayIdx)
                    .map((block: any, idx: number) => (
                      <div
                        key={idx}
                        className={`absolute ${block.color} text-white p-1 rounded text-xs overflow-hidden cursor-pointer hover:shadow-lg transition-shadow left-0.5 right-0.5 border-l-4 border-l-white/30`}
                        style={{
                          top: `${((block.hour - 7) / 14) * 100}%`,
                          height: `${(block.duration / 14) * 100}%`,
                          minHeight: "30px",
                        }}
                        title={`${block.nombreAsignatura} - ${block.nombreDocente}`}
                      >
                        <div className="font-medium truncate text-[11px]">{block.nombreAsignatura}</div>
                        <div className="text-white/80 text-[9px] truncate flex items-center gap-0.5">
                          <User size={8} />
                          {block.nombreDocente?.split(" ")[0]}
                        </div>
                        {block.room && (
                          <div className="text-white/70 text-[8px] truncate flex items-center gap-0.5 mt-0.5">
                            <Home size={7} />
                            {block.room}
                          </div>
                        )}
                        <div className="text-white/60 text-[8px] truncate mt-0.5">
                          {block.horaInicio} - {block.horaFin}
                        </div>
                      </div>
                    ))}
                </div>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Leyenda */}
      <Card>
        <CardHeader>
          <CardTitle>Leyenda</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap items-center gap-6">
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 bg-[#1A6BBF] rounded" />
              <span className="text-sm text-[#666666]">Ingeniería</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 bg-[#003087] rounded" />
              <span className="text-sm text-[#666666]">TAPSI</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 bg-[#E8A020] rounded" />
              <span className="text-sm text-[#666666]">Electiva</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}