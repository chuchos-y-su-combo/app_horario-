// views/CalendarView.tsx - Corregido
import { useState, useEffect, useRef } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Select } from "../Select";
import { Button } from "../Button";
import { Download, User, Home, AlertCircle, RefreshCw, Upload, Loader2 } from "lucide-react";
import { calendarioService } from "../../../services/calendarioService";
import { excelService } from "../../../services/excel.service";

export function CalendarView() {
  const [scenarioFilter, setScenarioFilter] = useState("");
  const [teacherFilter, setTeacherFilter] = useState("");
  const [roomFilter, setRoomFilter] = useState("");
  const [calendario, setCalendario] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedSemestre, setSelectedSemestre] = useState("2026-1");
  const [docentes, setDocentes] = useState<any[]>([]);
  const [generandoPropuestas, setGenerandoPropuestas] = useState(false);
  const [importando, setImportando] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const days = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  useEffect(() => {
    cargarDocentes();
  }, []);

  useEffect(() => {
    cargarCalendario();
  }, [selectedSemestre, scenarioFilter]);

  const cargarDocentes = async () => {
    try {
      const data = await calendarioService.getDocentes();
      setDocentes(data || []);
    } catch (err) {
      console.error("Error cargando docentes:", err);
    }
  };

  const cargarCalendario = async () => {
    setLoading(true);
    setError(null);
    try {
      let idPlan: string | undefined;
      let jornada: string | undefined;

      switch (scenarioFilter) {
        case "ing-diurna":
          jornada = "Diurna";
          break;
        case "ing-nocturna":
          jornada = "Nocturna";
          break;
        case "tapsi-diurno":
          jornada = "Diurna";
          break;
        case "tapsi-nocturno":
          jornada = "Nocturna";
          break;
      }

      const data = await calendarioService.getCalendarioSemanal(selectedSemestre, idPlan, jornada);
      setCalendario(data);
    } catch (err: any) {
      setError(err?.response?.data?.message || "No se pudo cargar el calendario");
    } finally {
      setLoading(false);
    }
  };

  const handleGenerarPropuestas = async () => {
    setGenerandoPropuestas(true);
    try {
      await calendarioService.generarPropuestas(selectedSemestre, ["ING_DIURNA", "ING_NOCTURNA", "TAPSI_DIURNA", "TAPSI_NOCTURNA"], 1);
      alert("Propuestas generadas exitosamente");
      cargarCalendario();
    } catch (err: any) {
      alert(err?.response?.data?.message || "Error al generar propuestas");
    } finally {
      setGenerandoPropuestas(false);
    }
  };

  // Importar currículo desde Excel
  const handleImportarCurriculo = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setImportando(true);
    try {
      const result = await excelService.importarCurriculo(file);
      alert(`Importación completada: ${result.procesadas} registros procesados. Errores: ${result.errores?.length || 0}`);
      if (result.errores?.length > 0) {
        console.error('Errores:', result.errores);
      }
      // Recargar datos después de importar
      cargarCalendario();
    } catch (error: any) {
      console.error('Error importando:', error);
      alert(error?.response?.data?.message || 'Error al importar el archivo');
    } finally {
      setImportando(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
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
    if (nombrePlan.toLowerCase().includes("ingenieria")) return "bg-[#1A6BBF]";
    if (nombrePlan.toLowerCase().includes("tapsi")) return "bg-[#003087]";
    return "bg-[#1A6BBF]";
  };

  const scheduleBlocks = calendario?.dias?.flatMap((dia: any) =>
    (dia.bloques || []).map((bloque: any) => ({
      ...bloque,
      day: dia.numeroDia - 1,
      hour: horaStringToNumber(bloque.horaInicio),
      duration: calcularDuracion(bloque.horaInicio, bloque.horaFin),
      color: getColorPorPlan(bloque.nombrePlan),
      room: bloque.escenario
    }))
  ).filter((block: any) => {
    if (teacherFilter && !block.nombreDocente?.toLowerCase().includes(teacherFilter.toLowerCase())) return false;
    if (roomFilter && !block.escenario?.toLowerCase().includes(roomFilter.toLowerCase())) return false;
    return true;
  }) || [];

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

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Calendario Semanal</h1>
          <p className="text-sm text-[#666666] mt-1">Total bloques: {scheduleBlocks.length}</p>
        </div>
        <div className="flex gap-3 flex-wrap">
          {/* Botón de importar currículo */}
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleImportarCurriculo}
            accept=".xlsx,.xls"
            className="hidden"
          />
          <Button 
            variant="secondary" 
            onClick={() => fileInputRef.current?.click()}
            disabled={importando}
            className="gap-2"
          >
            {importando ? <Loader2 size={20} className="animate-spin" /> : <Upload size={20} />}
            Importar Currículo
          </Button>
          
          <Button 
            variant="secondary" 
            onClick={cargarCalendario}
            className="gap-2"
          >
            <RefreshCw size={20} />
          </Button>
          
          <Button 
            variant="secondary" 
            onClick={handleGenerarPropuestas} 
            disabled={generandoPropuestas}
          >
            {generandoPropuestas ? "Generando..." : "Generar propuestas"}
          </Button>
          
          {/* Botón de exportar - deshabilitado hasta implementar endpoint */}
          <Button 
            variant="secondary"
            disabled
            className="gap-2 opacity-50 cursor-not-allowed"
          >
            <Download size={20} />
            Exportar (Próximamente)
          </Button>
        </div>
      </div>

      {/* Filtros */}
      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-4">
            <Select value={selectedSemestre} onChange={(e) => setSelectedSemestre(e.target.value)} options={[
              { value: "2026-1", label: "2026-1" }, { value: "2026-2", label: "2026-2" },
              { value: "2025-1", label: "2025-1" }, { value: "2025-2", label: "2025-2" }
            ]} />
            <Select value={scenarioFilter} onChange={(e) => setScenarioFilter(e.target.value)} options={[
              { value: "", label: "Todos" }, { value: "ing-diurna", label: "Ingeniería Diurna" },
              { value: "ing-nocturna", label: "Ingeniería Nocturna" }, { value: "tapsi-diurno", label: "TAPSI Diurno" },
              { value: "tapsi-nocturno", label: "TAPSI Nocturno" }
            ]} />
            <Select value={teacherFilter} onChange={(e) => setTeacherFilter(e.target.value)} options={[
              { value: "", label: "Todos" }, ...docentes.map((d: any) => ({ value: d.nombre, label: d.nombre }))
            ]} />
            <Select value={roomFilter} onChange={(e) => setRoomFilter(e.target.value)} options={[{ value: "", label: "Todos" }]} />
          </div>
        </CardHeader>
      </Card>

      {/* Calendario */}
      <Card>
        <CardContent className="p-0 overflow-x-auto">
          {error ? (
            <div className="p-6 text-center">
              <AlertCircle size={48} className="mx-auto text-red-500 mb-4" />
              <p>{error}</p>
              <Button onClick={cargarCalendario} className="mt-4">Reintentar</Button>
            </div>
          ) : scheduleBlocks.length === 0 ? (
            <div className="p-6 text-center">
              <p>No hay horarios asignados. Genera propuestas primero.</p>
              <div className="flex gap-3 mt-4 justify-center">
                <Button onClick={handleGenerarPropuestas} disabled={generandoPropuestas}>
                  {generandoPropuestas ? "Generando..." : "Generar propuestas"}
                </Button>
              </div>
            </div>
          ) : (
            <div className="border rounded-lg overflow-hidden min-w-[800px]">
              <div className="grid grid-cols-7 bg-[#333333]">
                <div className="p-3 text-white text-center border-r">Hora</div>
                {days.map(day => <div key={day} className="p-3 text-white text-center border-r">{day}</div>)}
              </div>
              <div className="grid grid-cols-7" style={{ minHeight: "600px" }}>
                <div className="border-r bg-[#F5F5F5]">
                  {hours.map(hour => <div key={hour} className="h-[57px] border-b px-3 py-2 text-sm">{hour}:00</div>)}
                </div>
                {days.map((_, dayIdx) => (
                  <div key={dayIdx} className="border-r relative">
                    {hours.map(hour => <div key={hour} className="h-[57px] border-b bg-white" />)}
                    {scheduleBlocks.filter((b: any) => b.day === dayIdx).map((block: any, idx: number) => (
                      <div key={idx} className={`absolute ${block.color} text-white p-1 rounded text-xs left-0.5 right-0.5`}
                        style={{ top: `${((block.hour - 7) / 14) * 100}%`, height: `${(block.duration / 14) * 100}%`, minHeight: "30px" }}>
                        <div className="font-medium truncate text-[11px]">{block.nombreAsignatura}</div>
                        <div className="text-white/80 text-[9px] flex items-center gap-0.5">
                          <User size={8} /> {block.nombreDocente?.split(" ")[0]}
                        </div>
                        <div className="text-white/60 text-[8px]">{block.horaInicio} - {block.horaFin}</div>
                      </div>
                    ))}
                  </div>
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Leyenda */}
      <Card>
        <CardHeader><CardTitle>Leyenda</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-6">
            <div className="flex items-center gap-2"><div className="w-6 h-6 bg-[#1A6BBF] rounded" /><span>Ingeniería</span></div>
            <div className="flex items-center gap-2"><div className="w-6 h-6 bg-[#003087] rounded" /><span>TAPSI</span></div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}