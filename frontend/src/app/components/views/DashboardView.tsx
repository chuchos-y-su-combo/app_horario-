import { useEffect, useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { ProgressBar } from "../ProgressBar";
import { GraduationCap, BookOpen, AlertTriangle, TrendingUp, Loader2 } from "lucide-react";
import { obtenerDocentes } from "../../../services/teachersService";
import { subjectService } from "../../../services/subject.service";
import { generationService } from "../../../services/generation.service";

const PERIODO_ACTIVO = "2026-1";

const ESCENARIOS_FIJOS = [
  { key: "ING_DIURNA",     name: "Ing. Diurna" },
  { key: "ING_NOCTURNA",   name: "Ing. Nocturna" },
  { key: "TAPSI_DIURNA",   name: "TAPSI Diurna" },
  { key: "TAPSI_NOCTURNA", name: "TAPSI Nocturna" },
] as const;

interface ScenarioItem {
  id: string;
  name: string;
  status: string;
  badge: string;
  progress: number;
  assigned: number;
  total: number;
  conflicts: number;
}

interface AssignedBlock {
  id: string;
  day: number;
  hour: number;
  duration: number;
  subject: string;
  teacher: string;
}

export function DashboardView() {
  const [docentes, setDocentes] = useState(0);
  const [asignaturas, setAsignaturas] = useState(0);
  const [scenarios, setScenarios] = useState<ScenarioItem[]>(() =>
    ESCENARIOS_FIJOS.map(({ key, name }) => ({
      id: key, name, status: "Sin datos", badge: "inactive",
      progress: 0, assigned: 0, total: 0, conflicts: 0,
    }))
  );
  const [assignedBlocks, setAssignedBlocks] = useState<AssignedBlock[]>([]);
  const [loading, setLoading] = useState(true);

  const days = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  const hours = Array.from({ length: 14 }, (_, i) => i + 7);

  useEffect(() => {
    cargarDatos();
  }, []);

  const cargarDatos = async () => {
    setLoading(true);
    try {
      const [docentesData, asignaturasData, propuestasData] = await Promise.allSettled([
        obtenerDocentes(),
        subjectService.getAll(),
        generationService.obtenerPropuestas(PERIODO_ACTIVO),
      ]);

      if (docentesData.status === "fulfilled") setDocentes(docentesData.value.length);
      if (asignaturasData.status === "fulfilled") setAsignaturas(asignaturasData.value.length);

      if (propuestasData.status === "fulfilled" && Array.isArray(propuestasData.value)) {
        const raw: any[] = propuestasData.value;

        // Una tarjeta fija por escenario, agrupando todas las asignaciones de ese escenario
        const escenarios: ScenarioItem[] = ESCENARIOS_FIJOS.map(({ key, name }) => {
          const grupo = raw.filter((a) => a.escenario === key);
          const total = grupo.length;
          const assigned = grupo.filter((a) => a.dia > 0 && a.horaInicio).length;
          const progress = total > 0 ? Math.round((assigned / total) * 100) : 0;
          const estado = total > 0 ? (grupo[0]?.estado ?? "Propuesta") : "Sin datos";
          const badge =
            estado === "Confirmada" ? "success"
            : estado === "Sin datos" ? "inactive"
            : "primary";
          return { id: key, name, status: estado, badge, progress, assigned, total, conflicts: 0 };
        });
        setScenarios(escenarios);

        const bloques: AssignedBlock[] = [];
        raw.forEach((item) => {
          if (item.dia > 0 && item.horaInicio) {
            const startH = parseInt((item.horaInicio as string).split(":")[0], 10);
            const endH = parseInt(((item.horaFin as string) ?? "").split(":")[0], 10) || startH + 1;
            bloques.push({
              id: item.idAsignacion ?? String(Math.random()),
              day: item.dia - 1,
              hour: startH,
              duration: endH - startH || 1,
              subject: item.nombreAsignatura ?? "",
              teacher: item.nombreDocente ?? "",
            });
          }
        });
        setAssignedBlocks(bloques.slice(0, 20));
      }
    } catch {
      // Errors are handled per-Promise via allSettled; silently degrade
    } finally {
      setLoading(false);
    }
  };

  const totalConflictos = scenarios.reduce((sum, s) => sum + (s.conflicts ?? 0), 0);
  const totalAsignadas = scenarios.reduce((sum, s) => sum + (s.assigned ?? 0), 0);
  const totalAsignaturas_ = scenarios.reduce((sum, s) => sum + (s.total ?? 0), 0);
  const avance = totalAsignaturas_ > 0 ? Math.round((totalAsignadas / totalAsignaturas_) * 100) : 0;

  const kpis = [
    { icon: GraduationCap, label: "Docentes Activos", value: loading ? "..." : String(docentes), color: "text-[#1A6BBF]", bg: "bg-[#1A6BBF]/10" },
    { icon: BookOpen, label: "Asignaturas Totales", value: loading ? "..." : String(asignaturas), color: "text-[#003087]", bg: "bg-[#003087]/10" },
    { icon: AlertTriangle, label: "Conflictos Abiertos", value: loading ? "..." : String(totalConflictos), color: "text-[#E8A020]", bg: "bg-[#E8A020]/10" },
    { icon: TrendingUp, label: "Avance de Horario", value: loading ? "..." : `${avance}%`, color: "text-[#1A7A4A]", bg: "bg-[#1A7A4A]/10" },
  ];

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* KPIs */}
      <div className="grid grid-cols-4 gap-6">
        {kpis.map((kpi, idx) => {
          const Icon = kpi.icon;
          return (
            <Card key={idx} className="flex items-center gap-4">
              <div className={`w-12 h-12 rounded-lg ${kpi.bg} flex items-center justify-center`}>
                {loading ? <Loader2 className={`${kpi.color} animate-spin`} size={24} /> : <Icon className={kpi.color} size={24} />}
              </div>
              <div>
                <p className="text-sm text-[#666666]">{kpi.label}</p>
                <p className="text-2xl font-medium text-[#333333]">{kpi.value}</p>
              </div>
            </Card>
          );
        })}
      </div>

      {/* Escenarios — siempre 4 tarjetas fijas */}
      <div className="grid grid-cols-4 gap-6">
        {scenarios.map((scenario) => (
          <Card key={scenario.id}>
            <CardHeader>
              <div className="flex items-center justify-between mb-2">
                <CardTitle className="text-base">{scenario.name}</CardTitle>
                <Badge variant={scenario.badge as any}>{scenario.status}</Badge>
              </div>
              <p className="text-sm text-[#666666]">
                {scenario.total > 0 ? `${scenario.assigned}/${scenario.total} asignadas` : "Sin propuesta"}
              </p>
            </CardHeader>
            <CardContent>
              {scenario.total > 0 ? (
                <>
                  <ProgressBar
                    value={scenario.progress}
                    variant={scenario.progress >= 80 ? "success" : scenario.progress >= 60 ? "primary" : "warning"}
                  />
                  <p className="text-xs text-[#666666] mt-2">{scenario.progress}% completado</p>
                </>
              ) : (
                <p className="text-xs text-[#999999]">
                  {loading ? "Cargando..." : "Generar desde la sección Generación"}
                </p>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-3 gap-6">
        {/* Calendario */}
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
                    <div key={day} className="p-2 text-xs text-white text-center border-r border-white/20 last:border-r-0">{day}</div>
                  ))}
                </div>
                <div className="grid grid-cols-7 relative" style={{ height: "400px" }}>
                  <div className="border-r border-[#CCCCCC] bg-[#F5F5F5]">
                    {hours.map((hour) => (
                      <div key={hour} className="h-[28.5px] border-b border-[#CCCCCC] px-2 py-1 text-xs text-[#666666]">{hour}:00</div>
                    ))}
                  </div>
                  {days.map((_, dayIdx) => (
                    <div key={dayIdx} className="border-r border-[#CCCCCC] last:border-r-0 relative">
                      {hours.map((hour) => (
                        <div key={hour} className="h-[28.5px] border-b border-[#CCCCCC]" />
                      ))}
                      {assignedBlocks
                        .filter((b) => b.day === dayIdx)
                        .map((b) => (
                          <div
                            key={b.id}
                            className="absolute bg-[#1A6BBF] text-white p-2 rounded text-xs overflow-hidden left-1 right-1"
                            style={{
                              top: `${((b.hour - 7) / 14) * 100}%`,
                              height: `${(b.duration / 14) * 100}%`,
                            }}
                          >
                            <div className="font-medium truncate">{b.subject}</div>
                            <div className="text-white/80 truncate text-[10px]">{b.teacher}</div>
                          </div>
                        ))}
                    </div>
                  ))}
                </div>
              </div>
              {assignedBlocks.length === 0 && !loading && (
                <p className="text-center text-sm text-[#999999] mt-4 py-4">
                  Sin horario generado para mostrar.
                </p>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Panel derecho */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Resumen del Período</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Período activo</span>
                  <span className="font-medium text-[#333333]">{PERIODO_ACTIVO}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Docentes registrados</span>
                  <span className="font-medium text-[#333333]">{docentes}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Asignaturas en catálogo</span>
                  <span className="font-medium text-[#333333]">{asignaturas}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-[#666666]">Escenarios activos</span>
                  <span className="font-medium text-[#333333]">{scenarios.filter(s => s.total > 0).length}</span>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Estado General</CardTitle>
            </CardHeader>
            <CardContent>
              {scenarios.length === 0 ? (
                <p className="text-sm text-[#999999] text-center py-4">
                  Sin propuesta generada
                </p>
              ) : (
                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-[#666666]">Asignaturas ubicadas</span>
                    <span className="font-medium text-[#333333]">{totalAsignadas}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-[#666666]">Conflictos pendientes</span>
                    <Badge variant={totalConflictos > 0 ? "error" : "success"} className="text-xs">
                      {totalConflictos}
                    </Badge>
                  </div>
                  <div className="mt-2">
                    <ProgressBar
                      value={avance}
                      variant={avance >= 80 ? "success" : avance >= 60 ? "primary" : "warning"}
                    />
                    <p className="text-xs text-[#666666] mt-1">{avance}% del horario completado</p>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
