import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import { ConfirmModal } from "../Modal";
import { Search, Plus, Edit, Trash2, BookOpen, AlertTriangle } from "lucide-react";

export function SubjectsView() {
  const [searchTerm, setSearchTerm] = useState("");
  const [planFilter, setPlanFilter] = useState("");
  const [semesterFilter, setSemesterFilter] = useState("");
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  const subjects = [
    { id: "1", code: "IS101", name: "Programación I", credits: 4, semester: 1, plan: "Ingeniería de Sistemas", minStudents: 15, type: "Flexible", groups: 3 },
    { id: "2", code: "IS201", name: "Base de Datos", credits: 4, semester: 3, plan: "Ingeniería de Sistemas", minStudents: 15, type: "Flexible", groups: 2 },
    { id: "3", code: "TP101", name: "Fundamentos TAPSI", credits: 3, semester: 1, plan: "TAPSI", minStudents: 20, type: "Fija", groups: 4 },
    { id: "4", code: "MA101", name: "Cálculo Diferencial", credits: 4, semester: 1, plan: "Ingeniería de Sistemas", minStudents: 15, type: "Flexible", groups: 3 },
    { id: "5", code: "TP301", name: "Sistemas Operativos", credits: 3, semester: 5, plan: "TAPSI", minStudents: 20, type: "Fija", groups: 2 },
    { id: "6", code: "IS401", name: "Ingeniería de Software II", credits: 4, semester: 7, plan: "Ingeniería de Sistemas", minStudents: 12, type: "Flexible", groups: 1 },
  ];

  const filteredSubjects = subjects.filter((subject) => {
    const matchesSearch =
      subject.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      subject.code.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesPlan = !planFilter || subject.plan === planFilter;
    const matchesSemester = !semesterFilter || subject.semester.toString() === semesterFilter;
    return matchesSearch && matchesPlan && matchesSemester;
  });

  const stats = {
    totalIngenieria: subjects.filter(s => s.plan.includes("Ingeniería")).length,
    totalTAPSI: subjects.filter(s => s.plan === "TAPSI").length,
    totalFijas: subjects.filter(s => s.type === "Fija").length,
    belowMinimum: 0,
  };

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Gestión de Asignaturas</h1>
          <p className="text-sm text-[#666666] mt-1">Administración de materias y planes de estudio</p>
        </div>
        <Button className="gap-2">
          <Plus size={20} />
          Nueva asignatura
        </Button>
      </div>

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
                { value: "Ingeniería de Sistemas", label: "Ingeniería de Sistemas" },
                { value: "TAPSI", label: "TAPSI" },
              ]}
            />
            <Select
              placeholder="Filtrar por semestre"
              value={semesterFilter}
              onChange={(e) => setSemesterFilter(e.target.value)}
              options={Array.from({ length: 10 }, (_, i) => ({
                value: String(i + 1),
                label: `Semestre ${i + 1}`,
              }))}
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
                <TableRow key={subject.id} striped>
                  <TableCell className="font-medium">{subject.code}</TableCell>
                  <TableCell>{subject.name}</TableCell>
                  <TableCell>{subject.credits}</TableCell>
                  <TableCell>{subject.semester}</TableCell>
                  <TableCell className="text-[#666666] text-xs">{subject.plan}</TableCell>
                  <TableCell>{subject.minStudents}</TableCell>
                  <TableCell>
                    <Badge variant={subject.type === "Fija" ? "info" : "secondary"}>
                      {subject.type}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <button className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]">
                        <Edit size={16} />
                      </button>
                      <button
                        onClick={() => setShowDeleteModal(true)}
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

      <div className="grid grid-cols-4 gap-6">
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
              <p className="text-xs text-[#666666]">{stats.totalFijas} fijas</p>
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
            </div>
          </CardContent>
        </Card>
      </div>

      <ConfirmModal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        onConfirm={() => console.log("Asignatura eliminada")}
        title="Eliminar asignatura"
        message="¿Está seguro que desea eliminar esta asignatura? Esta acción puede afectar el historial de horarios anteriores. Se recomienda inactivarla en lugar de eliminarla."
        confirmText="Eliminar"
        cancelText="Cancelar"
        variant="danger"
      />
    </div>
  );
}
