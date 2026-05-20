import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Modal } from "../Modal";
import { AlertCircle, XCircle, CheckCircle, AlertTriangle } from "lucide-react";

export function AlertsView() {
  const [selectedConflict, setSelectedConflict] = useState<number | null>(null);
  const [showExceptionModal, setShowExceptionModal] = useState(false);
  const [exceptionComment, setExceptionComment] = useState("");

  const stats = {
    critical: 5,
    warnings: 8,
    resolved: 24,
  };

  const conflicts = [
    {
      id: 1,
      type: "error",
      title: "Cruce de horario docente",
      description: "Dr. Carlos Ramírez tiene asignadas 2 materias simultáneas",
      subject: "Programación II y Base de Datos - Lunes 10:00",
      validations: [
        { rule: "Docente disponible", status: "error", message: "Conflicto de horario detectado" },
        { rule: "Carga contractual", status: "success", message: "Dentro del límite permitido (18/25h)" },
        { rule: "Materia fija TAPSI", status: "success", message: "No aplica" },
      ],
    },
    {
      id: 2,
      type: "error",
      title: "Sobrecarga docente",
      description: "Ing. Juan Torres supera su carga contractual",
      subject: "Asignadas 28 horas de 25 permitidas",
      validations: [
        { rule: "Docente disponible", status: "success", message: "Disponibilidad validada" },
        { rule: "Carga contractual", status: "error", message: "Excede por 3 horas el límite" },
        { rule: "Materia fija TAPSI", status: "success", message: "No aplica" },
      ],
    },
    {
      id: 3,
      type: "warning",
      title: "Materia sin salón asignado",
      description: "Circuitos Digitales no tiene aula disponible",
      subject: "Grupo 01 - Miércoles 14:00",
      validations: [
        { rule: "Docente disponible", status: "success", message: "Dr. García confirmado" },
        { rule: "Carga contractual", status: "success", message: "Dentro del límite (20/25h)" },
        { rule: "Materia fija TAPSI", status: "warning", message: "Requiere salón específico" },
      ],
    },
    {
      id: 4,
      type: "warning",
      title: "Materia cerca del mínimo de estudiantes",
      description: "Ingeniería de Software Avanzada tiene 13 inscritos",
      subject: "Mínimo requerido: 12 estudiantes",
      validations: [
        { rule: "Docente disponible", status: "success", message: "PhD. Martínez asignado" },
        { rule: "Carga contractual", status: "success", message: "Dentro del límite" },
        { rule: "Materia fija TAPSI", status: "success", message: "No aplica" },
      ],
    },
  ];

  const handleCreateException = () => {
    if (exceptionComment.trim()) {
      console.log("Excepción creada:", { conflictId: selectedConflict, comment: exceptionComment });
      setShowExceptionModal(false);
      setExceptionComment("");
    }
  };

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div>
        <h1 className="text-2xl font-medium text-[#333333]">Alertas y Conflictos</h1>
        <p className="text-sm text-[#666666] mt-1">Gestión de validaciones y excepciones del sistema</p>
      </div>

      <div className="grid grid-cols-3 gap-6">
        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#C0392B]/10 flex items-center justify-center">
              <XCircle className="text-[#C0392B]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Errores críticos</p>
              <p className="text-2xl font-medium text-[#C0392B]">{stats.critical}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#E8A020]/10 flex items-center justify-center">
              <AlertTriangle className="text-[#E8A020]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Advertencias</p>
              <p className="text-2xl font-medium text-[#E8A020]">{stats.warnings}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#1A7A4A]/10 flex items-center justify-center">
              <CheckCircle className="text-[#1A7A4A]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Resueltos</p>
              <p className="text-2xl font-medium text-[#1A7A4A]">{stats.resolved}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-3 gap-6">
        <div className="col-span-2 space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Bandeja de Conflictos</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {conflicts.map((conflict) => (
                  <div
                    key={conflict.id}
                    className={`p-4 rounded border cursor-pointer transition-all ${
                      selectedConflict === conflict.id
                        ? conflict.type === "error"
                          ? "bg-[#C0392B]/5 border-[#C0392B]"
                          : "bg-[#E8A020]/5 border-[#E8A020]"
                        : "bg-white border-[#CCCCCC] hover:border-[#1A6BBF]"
                    }`}
                    onClick={() => setSelectedConflict(conflict.id)}
                  >
                    <div className="flex items-start gap-3">
                      <Badge variant={conflict.type as any} className="shrink-0 mt-0.5">
                        {conflict.type === "error" ? "Error" : "Advertencia"}
                      </Badge>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-[#333333] mb-1">{conflict.title}</p>
                        <p className="text-xs text-[#666666] mb-2">{conflict.description}</p>
                        <div className="flex items-center gap-2">
                          <AlertCircle
                            className={conflict.type === "error" ? "text-[#C0392B]" : "text-[#E8A020]"}
                            size={14}
                          />
                          <p className="text-xs text-[#666666]">{conflict.subject}</p>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        <div>
          <Card>
            <CardHeader>
              <CardTitle>Detalle del Conflicto</CardTitle>
            </CardHeader>
            <CardContent>
              {selectedConflict ? (
                <div className="space-y-4">
                  {conflicts
                    .find((c) => c.id === selectedConflict)
                    ?.validations.map((validation, idx) => (
                      <div key={idx} className="space-y-2">
                        <div className="flex items-center justify-between">
                          <p className="text-sm font-medium text-[#333333]">{validation.rule}</p>
                          <Badge
                            variant={
                              validation.status === "success"
                                ? "success"
                                : validation.status === "error"
                                ? "error"
                                : "warning"
                            }
                            className="text-xs"
                          >
                            {validation.status === "success"
                              ? "✓"
                              : validation.status === "error"
                              ? "✗"
                              : "!"}
                          </Badge>
                        </div>
                        <p className="text-xs text-[#666666]">{validation.message}</p>
                      </div>
                    ))}
                  <div className="pt-4 border-t border-[#CCCCCC]">
                    <div className="bg-[#E8A020]/5 border border-[#E8A020]/20 rounded p-3 mb-3">
                      <div className="flex items-start gap-2">
                        <AlertCircle className="text-[#E8A020] shrink-0 mt-0.5" size={16} />
                        <div>
                          <p className="text-xs font-medium text-[#333333] mb-1">Solo Administrador</p>
                          <p className="text-xs text-[#666666]">
                            Se requiere rol de administrador para crear excepciones
                          </p>
                        </div>
                      </div>
                    </div>
                    <Button
                      variant="secondary"
                      className="w-full"
                      size="sm"
                      onClick={() => setShowExceptionModal(true)}
                    >
                      Crear excepción
                    </Button>
                  </div>
                </div>
              ) : (
                <div className="text-center py-8 text-[#999999] text-sm">
                  Seleccione un conflicto para ver los detalles
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      <Modal
        isOpen={showExceptionModal}
        onClose={() => {
          setShowExceptionModal(false);
          setExceptionComment("");
        }}
        title="Crear Excepción"
        size="md"
        footer={
          <>
            <Button variant="secondary" onClick={() => setShowExceptionModal(false)}>
              Cancelar
            </Button>
            <Button onClick={handleCreateException} disabled={!exceptionComment.trim()}>
              Crear excepción
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div className="bg-[#E8A020]/5 border border-[#E8A020]/20 rounded p-3">
            <div className="flex items-start gap-2">
              <AlertTriangle className="text-[#E8A020] shrink-0 mt-0.5" size={20} />
              <div>
                <p className="text-sm font-medium text-[#333333] mb-1">Advertencia</p>
                <p className="text-xs text-[#666666]">
                  Al crear una excepción, este conflicto será aprobado manualmente y quedará registrado en el
                  sistema de auditoría. Deberá justificar la decisión.
                </p>
              </div>
            </div>
          </div>

          <Input
            label="Comentario de justificación (obligatorio)"
            placeholder="Explique por qué se aprueba esta excepción..."
            value={exceptionComment}
            onChange={(e) => setExceptionComment(e.target.value)}
          />

          <div className="text-xs text-[#666666]">
            Esta acción será registrada en el historial de auditoría con su usuario y timestamp.
          </div>
        </div>
      </Modal>
    </div>
  );
}
