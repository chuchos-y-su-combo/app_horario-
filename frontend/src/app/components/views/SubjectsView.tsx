// views/SubjectsView.tsx
import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import { ConfirmModal } from "../Modal";
import { Search, Plus, Edit, Trash2, BookOpen, AlertTriangle, Loader2, RefreshCw } from "lucide-react";
import { subjectService, Subject, CreateSubjectRequest } from "../../../services/subject.service";

type PlanEstudio = {
  idPlan: string;
  nombrePlan: string;
};

export function SubjectsView() {
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [planes, setPlanes] = useState<PlanEstudio[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [planFilter, setPlanFilter] = useState("");
  const [semesterFilter, setSemesterFilter] = useState("");
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [selectedSubject, setSelectedSubject] = useState<Subject | null>(null);
  const [creando, setCreando] = useState(false);
  const [eliminando, setEliminando] = useState(false);
  
  const [formData, setFormData] = useState<CreateSubjectRequest>({
    idPlan: "",
    codigo: "",
    nombre: "",
    creditos: 3,
    semestre: 1,
    minEstudiantes: 15,
    esFijaTapsi: false,
    esOpcionalTapsiDiurna: false,
  });

  useEffect(() => {
    cargarDatos();
  }, []);

const cargarDatos = async () => {
    setLoading(true);
    try {
        const asignaturasData = await subjectService.getAll();
        setSubjects(asignaturasData);
        setPlanes([]);
    } catch (error) {
        console.error("Error cargando datos:", error);
    } finally {
        setLoading(false);
    }
};

  const handleCreate = async () => {
    if (!formData.idPlan || !formData.codigo || !formData.nombre) {
      alert("Complete todos los campos obligatorios");
      return;
    }
    
    setCreando(true);
    try {
      await subjectService.create(formData);
      setShowCreateModal(false);
      setFormData({
        idPlan: "",
        codigo: "",
        nombre: "",
        creditos: 3,
        semestre: 1,
        minEstudiantes: 15,
        esFijaTapsi: false,
        esOpcionalTapsiDiurna: false,
      });
      cargarDatos();
    } catch (error: any) {
      alert(error.response?.data?.message || "Error al crear la asignatura");
    } finally {
      setCreando(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedSubject) return;
    
    setEliminando(true);
    try {
      await subjectService.delete(selectedSubject.idAsignatura);
      setShowDeleteModal(false);
      setSelectedSubject(null);
      cargarDatos();
    } catch (error: any) {
      alert(error.response?.data?.message || "Error al eliminar la asignatura");
    } finally {
      setEliminando(false);
    }
  };

  const getNombrePlan = (idPlan: string) => {
    const plan = planes.find(p => p.idPlan === idPlan);
    return plan?.nombrePlan || idPlan;
  };

  const filteredSubjects = subjects.filter((subject) => {
    const matchesSearch =
      subject.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
      subject.codigo.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesPlan = !planFilter || subject.idPlan === planFilter;
    const matchesSemester = !semesterFilter || subject.semestre.toString() === semesterFilter;
    return matchesSearch && matchesPlan && matchesSemester;
  });

  const stats = {
    total: subjects.length,
    totalIngenieria: subjects.filter(s => {
      const plan = planes.find(p => p.idPlan === s.idPlan);
      return plan?.nombrePlan?.toLowerCase().includes("ingenieria");
    }).length,
    totalTAPSI: subjects.filter(s => {
      const plan = planes.find(p => p.idPlan === s.idPlan);
      return plan?.nombrePlan?.toLowerCase().includes("tapsi");
    }).length,
    totalFijas: subjects.filter(s => s.esFijaTapsi).length,
    totalOpcionales: subjects.filter(s => s.esOpcionalTapsiDiurna).length,
    belowMinimum: subjects.filter(s => s.minEstudiantes < 15).length,
  };

  if (loading) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <div className="text-center">
          <Loader2 className="w-8 h-8 animate-spin text-[#1A6BBF] mx-auto" />
          <p className="mt-4 text-[#666666]">Cargando asignaturas...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Gestión de Asignaturas</h1>
          <p className="text-sm text-[#666666] mt-1">Administración de materias y planes de estudio</p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" onClick={cargarDatos} className="gap-2">
            <RefreshCw size={20} />
            Actualizar
          </Button>
          <Button onClick={() => setShowCreateModal(true)} className="gap-2">
            <Plus size={20} />
            Nueva asignatura
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-4 gap-6">
        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#1A6BBF]/10 flex items-center justify-center">
              <BookOpen className="text-[#1A6BBF]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Total Asignaturas</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.total}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#1A6BBF]/10 flex items-center justify-center">
              <BookOpen className="text-[#1A6BBF]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Total Ingeniería</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.totalIngenieria}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#003087]/10 flex items-center justify-center">
              <BookOpen className="text-[#003087]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Total TAPSI</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.totalTAPSI}</p>
              <p className="text-xs text-[#666666]">{stats.totalFijas} fijas, {stats.totalOpcionales} opcionales</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#E8A020]/10 flex items-center justify-center">
              <AlertTriangle className="text-[#E8A020]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Bajo mínimo</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.belowMinimum}</p>
              <p className="text-xs text-[#666666]">(menos de 15 estudiantes)</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabla de asignaturas */}
      <Card>
        <CardHeader>
          <CardTitle>Catálogo de Asignaturas</CardTitle>
          <div className="grid grid-cols-3 gap-4 mt-4">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-[#999999]" size={20} />
              <Input
                placeholder="Buscar por nombre o código..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select
              placeholder="Filtrar por plan de estudios"
              value={planFilter}
              onChange={(e) => setPlanFilter(e.target.value)}
              options={[
                { value: "", label: "Todos" },
                ...planes.map(p => ({ value: p.idPlan, label: p.nombrePlan }))
              ]}
            />
            <Select
              placeholder="Filtrar por semestre"
              value={semesterFilter}
              onChange={(e) => setSemesterFilter(e.target.value)}
              options={[
                { value: "", label: "Todos" },
                ...Array.from({ length: 10 }, (_, i) => ({
                  value: String(i + 1),
                  label: `Semestre ${i + 1}`,
                }))
              ]}
            />
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Código</TableHead>
                <TableHead>Nombre</TableHead>
                <TableHead>Créditos</TableHead>
                <TableHead>Semestre</TableHead>
                <TableHead>Plan</TableHead>
                <TableHead>Mín. estudiantes</TableHead>
                <TableHead>Tipo</TableHead>
                <TableHead>Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredSubjects.map((subject) => (
                <TableRow key={subject.idAsignatura} striped>
                  <TableCell className="font-medium">{subject.codigo}</TableCell>
                  <TableCell>{subject.nombre}</TableCell>
                  <TableCell>{subject.creditos}</TableCell>
                  <TableCell>{subject.semestre}</TableCell>
                  <TableCell className="text-[#666666] text-xs">{getNombrePlan(subject.idPlan)}</TableCell>
                  <TableCell>{subject.minEstudiantes}</TableCell>
                  <TableCell>
                    {subject.esFijaTapsi ? (
                      <Badge variant="info">Fija TAPSI</Badge>
                    ) : subject.esOpcionalTapsiDiurna ? (
                      <Badge variant="warning">Opcional Diurna</Badge>
                    ) : (
                      <Badge variant="secondary">Flexible</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <button className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]">
                        <Edit size={16} />
                      </button>
                      <button
                        onClick={() => {
                          setSelectedSubject(subject);
                          setShowDeleteModal(true);
                        }}
                        className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#C0392B]"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Modal de nueva asignatura */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-lg w-full max-w-lg p-6">
            <h2 className="text-lg font-semibold text-[#333333] mb-4">Nueva asignatura</h2>
            <div className="space-y-4 max-h-96 overflow-y-auto">
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Plan de estudios *</label>
                <Select
                  placeholder="Seleccionar plan"
                  value={formData.idPlan}
                  onChange={(e) => setFormData({ ...formData, idPlan: e.target.value })}
                  options={planes.map(p => ({ value: p.idPlan, label: p.nombrePlan }))}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Código *</label>
                <Input
                  value={formData.codigo}
                  onChange={(e) => setFormData({ ...formData, codigo: e.target.value.toUpperCase() })}
                  placeholder="Ej: IS101"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Nombre *</label>
                <Input
                  value={formData.nombre}
                  onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                  placeholder="Nombre de la asignatura"
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-[#333333] mb-1">Créditos</label>
                  <Input
                    type="number"
                    value={formData.creditos}
                    onChange={(e) => setFormData({ ...formData, creditos: parseInt(e.target.value) })}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-[#333333] mb-1">Semestre</label>
                  <Input
                    type="number"
                    value={formData.semestre}
                    onChange={(e) => setFormData({ ...formData, semestre: parseInt(e.target.value) })}
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Mínimo estudiantes</label>
                <Input
                  type="number"
                  value={formData.minEstudiantes}
                  onChange={(e) => setFormData({ ...formData, minEstudiantes: parseInt(e.target.value) })}
                />
              </div>
              <div className="flex gap-4">
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={formData.esFijaTapsi}
                    onChange={(e) => setFormData({ ...formData, esFijaTapsi: e.target.checked })}
                    className="rounded border-[#CCCCCC]"
                  />
                  <span className="text-sm text-[#333333]">Marcar como fija TAPSI</span>
                </label>
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={formData.esOpcionalTapsiDiurna}
                    onChange={(e) => setFormData({ ...formData, esOpcionalTapsiDiurna: e.target.checked })}
                    className="rounded border-[#CCCCCC]"
                  />
                  <span className="text-sm text-[#333333]">Marcar como opcional diurna</span>
                </label>
              </div>
            </div>
            <div className="flex gap-3 mt-6 justify-end">
              <button
                onClick={() => setShowCreateModal(false)}
                className="px-4 py-2 text-sm font-medium text-[#666666] bg-[#F5F5F5] rounded hover:bg-[#E8E8E8] transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={handleCreate}
                className="px-4 py-2 text-sm font-medium text-white bg-[#1A6BBF] rounded hover:bg-[#155BA0] transition-colors"
              >
                {creando ? "Creando..." : "Crear"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de eliminación */}
      <ConfirmModal
        isOpen={showDeleteModal}
        onClose={() => {
          setShowDeleteModal(false);
          setSelectedSubject(null);
        }}
        onConfirm={handleDelete}
        title="Eliminar asignatura"
        message={`¿Está seguro que desea eliminar "${selectedSubject?.nombre}"? Esta acción puede afectar el historial de horarios anteriores. Se recomienda inactivarla en lugar de eliminarla.`}
        confirmText={eliminando ? "Eliminando..." : "Eliminar"}
        cancelText="Cancelar"
        variant="danger"
      />
    </div>
  );
}