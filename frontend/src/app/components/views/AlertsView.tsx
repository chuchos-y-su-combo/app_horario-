import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Modal } from "../Modal";
import { AlertCircle, XCircle, CheckCircle, AlertTriangle, Loader2 } from "lucide-react";
import api from "../../../services/api";

const PERIODO_ACTIVO = "2026-1";

interface ConflictoValidacion {
  rule: string;
  status: "success" | "error" | "warning";
  message: string;
}

interface Conflicto {
  id: string;
  type: "error" | "warning";
  title: string;
  description: string;
  subject: string;
  validations: ConflictoValidacion[];
}

export function AlertsView() {
  const [conflicts, setConflicts] = useState<Conflicto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedConflict, setSelectedConflict] = useState<string | null>(null);
  const [showExceptionModal, setShowExceptionModal] = useState(false);
  const [exceptionComment, setExceptionComment] = useState("");

  useEffect(() => {
    cargarConflictos();
  }, []);

  const cargarConflictos = async () => {
    setLoading(true);
    try {
      const response = await api.get(`/reportes/conflictos?periodo=${PERIODO_ACTIVO}`);
      const data = Array.isArray(response.data) ? response.data : [];
      const mapped: Conflicto[] = data.map((item: any) => ({
        id: item.id ?? item.idConflicto ?? String(Math.random()),
        type: item.tipo === "Error" || item.nivel === "Critico" ? "error" : "warning",
        title: item.titulo ?? item.descripcion ?? "Conflicto",
        description: item.detalle ?? item.descripcion ?? "",
        subject: item.asignatura ?? item.referencia ?? "",
        validations: [],
      }));
      setConflicts(mapped);
    } catch {
      setConflicts([]);
    } finally {
      setLoading(false);
    }
  };

  const stats = {
    critical: conflicts.filter((c) => c.type === "error").length,
    warnings: conflicts.filter((c) => c.type === "warning").length,
    resolved: 0,
  };

  const selectedConflictData = conflicts.find((c) => c.id === selectedConflict);

  const handleCreateException = () => {
    if (exceptionComment.trim()) {
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
              <p className="text-2xl font-medium text-[#C0392B]">{loading ? "..." : stats.critical}</p>
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
              <p className="text-2xl font-medium text-[#E8A020]">{loading ? "..." : stats.warnings}</p>
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
        <div className="col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Bandeja de Conflictos</CardTitle>
            </CardHeader>
            <CardContent>
              {loading ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="animate-spin text-[#1A6BBF]" size={24} />
                </div>
              ) : conflicts.length === 0 ? (
                <div className="text-center py-12 border-2 border-dashed border-[#CCCCCC] rounded">
                  <CheckCircle className="mx-auto text-[#1A7A4A] mb-3" size={32} />
                  <p className="text-sm font-medium text-[#333333]">Sin conflictos detectados</p>
                  <p className="text-xs text-[#999999] mt-1">No hay alertas pendientes para el período {PERIODO_ACTIVO}.</p>
                </div>
              ) : (
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
                          {conflict.subject && (
                            <div className="flex items-center gap-2">
                              <AlertCircle
                                className={conflict.type === "error" ? "text-[#C0392B]" : "text-[#E8A020]"}
                                size={14}
                              />
                              <p className="text-xs text-[#666666]">{conflict.subject}</p>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        <div>
          <Card>
            <CardHeader>
              <CardTitle>Detalle del Conflicto</CardTitle>
            </CardHeader>
            <CardContent>
              {selectedConflictData ? (
                <div className="space-y-4">
                  <div>
                    <p className="text-sm font-medium text-[#333333] mb-1">{selectedConflictData.title}</p>
                    <p className="text-xs text-[#666666]">{selectedConflictData.description}</p>
                  </div>
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
        onClose={() => { setShowExceptionModal(false); setExceptionComment(""); }}
        title="Crear Excepción"
        size="md"
        footer={
          <>
            <Button variant="secondary" onClick={() => setShowExceptionModal(false)}>Cancelar</Button>
            <Button onClick={handleCreateException} disabled={!exceptionComment.trim()}>Crear excepción</Button>
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
                  Al crear una excepción, este conflicto será aprobado manualmente y quedará registrado en el sistema de auditoría.
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
