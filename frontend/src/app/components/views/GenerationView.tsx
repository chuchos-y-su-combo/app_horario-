import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { ProgressBar } from "../ProgressBar";
import { ConfirmModal } from "../Modal";
import { Sparkles, CheckCircle, AlertTriangle, Clock } from "lucide-react";
import { useEffect, useState } from "react";
import { generationService } from "../../../services/generation.service";

export function GenerationView() {
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [scenarios, setScenarios] = useState<any[]>([]);

  useEffect(() => {
    cargarPropuestas();
  }, []);

  const cargarPropuestas = async () => {
    try {
      const response = await generationService.obtenerPropuestas("2026-1");

      const propuestasTransformadas = response.map((item: any) => ({
        id: item.id,
        name: item.nombreEscenario,
        status: item.estado,
        badge:
          item.estado === "Activo"
            ? "primary"
            : item.estado === "Validando"
            ? "warning"
            : item.estado === "Fijas aplicadas"
            ? "success"
            : "inactive",
        progress: item.progreso,
        assigned: item.asignadas,
        total: item.total,
        conflicts: item.conflictos,
      }));

      setScenarios(propuestasTransformadas);
    } catch (error) {
      console.error("Error cargando propuestas", error);
    }
  };


  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Generación Automática de Horarios</h1>
          <p className="text-sm text-[#666666] mt-1">Motor inteligente de asignación con validación de restricciones</p>
        </div>
        <Button className="gap-2" onClick={() => setShowConfirmModal(true)}>
          <Sparkles size={20} />
          Generar horarios
        </Button>
      </div>

      <div className="grid grid-cols-4 gap-6">
        {scenarios.map((scenario) => {
          const statusIcons = {
            Activo: CheckCircle,
            Validando: Clock,
            "Fijas aplicadas": CheckCircle,
            Pendiente: AlertTriangle,
          };
          const Icon = statusIcons[scenario.status as keyof typeof statusIcons];

          return (
            <Card key={scenario.id}>
              <CardHeader>
                <div className="flex items-start justify-between mb-3">
                  <CardTitle className="text-base">{scenario.name}</CardTitle>
                  <Icon
                    className={
                      scenario.badge === "success"
                        ? "text-[#1A7A4A]"
                        : scenario.badge === "warning"
                        ? "text-[#E8A020]"
                        : scenario.badge === "primary"
                        ? "text-[#1A6BBF]"
                        : "text-[#999999]"
                    }
                    size={20}
                  />
                </div>
                <Badge variant={scenario.badge as any}>{scenario.status}</Badge>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  <ProgressBar
                    value={scenario.progress}
                    variant={scenario.progress >= 80 ? "success" : scenario.progress >= 60 ? "primary" : "warning"}
                  />
                  <div className="flex justify-between text-sm">
                    <span className="text-[#666666]">Progreso</span>
                    <span className="text-[#333333] font-medium">{scenario.progress}%</span>
                  </div>
                  <div className="pt-3 border-t border-[#CCCCCC] space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-[#666666]">Asignadas</span>
                      <span className="text-[#333333]">{scenario.assigned} / {scenario.total}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-[#666666]">Conflictos</span>
                      <Badge variant={scenario.conflicts > 0 ? "error" : "success"} className="text-xs">
                        {scenario.conflicts}
                      </Badge>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {scenarios.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Motor de Generación - Estado de Restricciones</CardTitle>
            <p className="text-sm text-[#666666] mt-1">Estado de cumplimiento de reglas institucionales</p>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[
                { name: "Disponibilidad docente", description: "Franjas horarias cargadas por importación Excel" },
                { name: "Carga contractual", description: "Horas asignadas vs. límite por tipo de contrato" },
                { name: "Materias fijas TAPSI", description: "Restricciones de plan TAPSI aplicadas" },
                { name: "Bloqueos activos", description: "Franjas bloqueadas respetadas" },
              ].map((item, idx) => {
                const scenario = scenarios[0];
                const pct = scenario ? Math.min(100, Math.round((scenario.assigned / Math.max(scenario.total, 1)) * 100)) : 0;
                return (
                  <div key={idx} className="space-y-2">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <div className={`w-2 h-2 rounded-full ${pct >= 80 ? "bg-[#1A7A4A]" : "bg-[#E8A020]"}`} />
                        <div>
                          <p className="text-sm font-medium text-[#333333]">{item.name}</p>
                          <p className="text-xs text-[#666666]">{item.description}</p>
                        </div>
                      </div>
                      <span className="text-sm font-medium text-[#333333]">{pct}%</span>
                    </div>
                    <ProgressBar value={pct} variant={pct >= 80 ? "success" : "warning"} size="sm" />
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      )}

      {scenarios.length > 0 && (
        <div className="grid grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Resultado Preliminar</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center justify-between p-4 bg-[#1A7A4A]/5 rounded border border-[#1A7A4A]/20">
                  <div>
                    <p className="text-sm text-[#666666]">Asignaturas ubicadas</p>
                    <p className="text-2xl font-medium text-[#1A7A4A]">
                      {scenarios.reduce((s, e) => s + (e.assigned ?? 0), 0)}
                    </p>
                  </div>
                  <CheckCircle className="text-[#1A7A4A]" size={32} />
                </div>
                <div className="flex items-center justify-between p-4 bg-[#E8A020]/5 rounded border border-[#E8A020]/20">
                  <div>
                    <p className="text-sm text-[#666666]">Conflictos pendientes</p>
                    <p className="text-2xl font-medium text-[#E8A020]">
                      {scenarios.reduce((s, e) => s + (e.conflicts ?? 0), 0)}
                    </p>
                  </div>
                  <AlertTriangle className="text-[#E8A020]" size={32} />
                </div>
                <div className="pt-4 border-t border-[#CCCCCC]">
                  <p className="text-xs text-[#666666] mb-3">
                    Propuesta generada. Revisa los conflictos en Ajuste Manual antes de confirmar.
                  </p>
                  <Button variant="secondary" className="w-full">
                    Ver detalles de generación
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Historial de Generaciones</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-center py-8 text-[#999999] text-sm">
                No hay generaciones anteriores registradas.
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      <ConfirmModal
        isOpen={showConfirmModal}
        onClose={() => setShowConfirmModal(false)}
        onConfirm={() => console.log("Generación iniciada")}
        title="Confirmar generación de horarios"
        message="¿Está seguro que desea iniciar la generación automática? Esta acción creará una nueva propuesta de horarios que quedará como versión oficial preliminar. La versión actual se guardará en el historial."
        confirmText="Generar"
        cancelText="Cancelar"
        variant="warning"
      />
    </div>
  );
}
