import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { ProgressBar } from "../ProgressBar";
import { ConfirmModal } from "../Modal";
import { Sparkles, CheckCircle, AlertTriangle, Clock } from "lucide-react";

export function GenerationView() {
  const [showConfirmModal, setShowConfirmModal] = useState(false);

  const scenarios = [
    { id: "1", name: "Ingeniería Diurna", status: "Activo", badge: "primary", progress: 85, assigned: 51, total: 60, conflicts: 3 },
    { id: "2", name: "Ingeniería Nocturna", status: "Validando", badge: "warning", progress: 72, assigned: 43, total: 60, conflicts: 7 },
    { id: "3", name: "TAPSI Diurno", status: "Fijas aplicadas", badge: "success", progress: 90, assigned: 36, total: 40, conflicts: 1 },
    { id: "4", name: "TAPSI Nocturno", status: "Pendiente", badge: "inactive", progress: 45, assigned: 18, total: 40, conflicts: 0 },
  ];

  const constraints = [
    { name: "Disponibilidad docente", status: "success", percentage: 95, description: "Respetando franjas horarias cargadas" },
    { name: "Carga contractual", status: "warning", percentage: 88, description: "3 docentes cerca del límite" },
    { name: "Materias fijas TAPSI", status: "success", percentage: 100, description: "Todas las restricciones aplicadas" },
    { name: "Bloqueos activos", status: "success", percentage: 100, description: "12 franjas bloqueadas respetadas" },
  ];

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

      <Card>
        <CardHeader>
          <CardTitle>Motor de Generación - Validación de Restricciones</CardTitle>
          <p className="text-sm text-[#666666] mt-1">Estado de cumplimiento de reglas institucionales</p>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {constraints.map((constraint, idx) => (
              <div key={idx} className="space-y-2">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div
                      className={`w-2 h-2 rounded-full ${
                        constraint.status === "success" ? "bg-[#1A7A4A]" : "bg-[#E8A020]"
                      }`}
                    />
                    <div>
                      <p className="text-sm font-medium text-[#333333]">{constraint.name}</p>
                      <p className="text-xs text-[#666666]">{constraint.description}</p>
                    </div>
                  </div>
                  <span className="text-sm font-medium text-[#333333]">{constraint.percentage}%</span>
                </div>
                <ProgressBar
                  value={constraint.percentage}
                  variant={constraint.status === "success" ? "success" : "warning"}
                  size="sm"
                />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

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
                  <p className="text-2xl font-medium text-[#1A7A4A]">148</p>
                </div>
                <CheckCircle className="text-[#1A7A4A]" size={32} />
              </div>
              <div className="flex items-center justify-between p-4 bg-[#E8A020]/5 rounded border border-[#E8A020]/20">
                <div>
                  <p className="text-sm text-[#666666]">Conflictos pendientes</p>
                  <p className="text-2xl font-medium text-[#E8A020]">11</p>
                </div>
                <AlertTriangle className="text-[#E8A020]" size={32} />
              </div>
              <div className="pt-4 border-t border-[#CCCCCC]">
                <p className="text-xs text-[#666666] mb-3">
                  El sistema ha generado una propuesta inicial. Se recomienda revisar los conflictos en el módulo de Ajuste Manual antes de aplicar.
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
            <div className="space-y-3">
              {[
                { date: "12 May 2026 - 14:30", status: "Actual", conflicts: 11 },
                { date: "10 May 2026 - 09:15", status: "Anterior", conflicts: 18 },
                { date: "08 May 2026 - 16:45", status: "Descartada", conflicts: 24 },
              ].map((gen, idx) => (
                <div key={idx} className="p-3 border border-[#CCCCCC] rounded hover:bg-[#F5F5F5] transition-colors">
                  <div className="flex items-center justify-between mb-2">
                    <p className="text-sm font-medium text-[#333333]">{gen.date}</p>
                    <Badge variant={gen.status === "Actual" ? "success" : "inactive"} className="text-xs">
                      {gen.status}
                    </Badge>
                  </div>
                  <p className="text-xs text-[#666666]">{gen.conflicts} conflictos detectados</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

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
