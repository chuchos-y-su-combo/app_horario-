import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import { ProgressBar } from "../ProgressBar";
import { Search, Plus, Eye, Edit, Users, AlertCircle, TrendingUp } from "lucide-react";

export function TeachersListView() {
  const [searchTerm, setSearchTerm] = useState("");
  const [contractFilter, setContractFilter] = useState("");
  const [availabilityFilter, setAvailabilityFilter] = useState("");

  const teachers = [
    {
      id: "1",
      name: "Dr. Carlos Ramírez",
      area: "Ingeniería de Sistemas",
      contract: "TC",
      workload: 20,
      maxWorkload: 25,
      availability: "Validada",
      status: "Activo",
    },
    {
      id: "2",
      name: "Msc. María López",
      area: "Matemáticas",
      contract: "TP",
      workload: 12,
      maxWorkload: 15,
      availability: "Pendiente",
      status: "Activo",
    },
    {
      id: "3",
      name: "Ing. Juan Torres",
      area: "Programación",
      contract: "TC",
      workload: 28,
      maxWorkload: 25,
      availability: "Validada",
      status: "Alerta",
    },
    {
      id: "4",
      name: "PhD. Ana García",
      area: "Física",
      contract: "TC",
      workload: 18,
      maxWorkload: 25,
      availability: "Sin cargar",
      status: "Activo",
    },
    {
      id: "5",
      name: "Msc. Pedro Sánchez",
      area: "Base de Datos",
      contract: "TP",
      workload: 9,
      maxWorkload: 15,
      availability: "Validada",
      status: "Activo",
    },
    {
      id: "6",
      name: "Dr. Laura Martínez",
      area: "Redes",
      contract: "TC",
      workload: 0,
      maxWorkload: 25,
      availability: "Sin cargar",
      status: "Inactivo",
    },
  ];

  const filteredTeachers = teachers.filter((teacher) => {
    const matchesSearch =
      teacher.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      teacher.area.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesContract = !contractFilter || teacher.contract === contractFilter;
    const matchesAvailability = !availabilityFilter || teacher.availability === availabilityFilter;
    return matchesSearch && matchesContract && matchesAvailability;
  });

  const stats = {
    avgWorkload: 14.5,
    withoutAvailability: 2,
    overloaded: 1,
  };

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Gestión de Docentes</h1>
          <p className="text-sm text-[#666666] mt-1">Administración de carga académica y disponibilidad</p>
        </div>
        <Button className="gap-2">
          <Plus size={20} />
          Nuevo docente
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Lista de Docentes</CardTitle>
          <div className="grid grid-cols-3 gap-4 mt-4">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-[#999999]" size={20} />
              <Input
                placeholder="Buscar por nombre o área..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select
              placeholder="Filtrar por contrato"
              value={contractFilter}
              onChange={(e) => setContractFilter(e.target.value)}
              options={[
                { value: "TC", label: "Tiempo completo" },
                { value: "TP", label: "Tiempo parcial" },
              ]}
            />
            <Select
              placeholder="Filtrar por disponibilidad"
              value={availabilityFilter}
              onChange={(e) => setAvailabilityFilter(e.target.value)}
              options={[
                { value: "Validada", label: "Validada" },
                { value: "Pendiente", label: "Pendiente" },
                { value: "Sin cargar", label: "Sin cargar" },
              ]}
            />
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Docente</TableHead>
                <TableHead>Tipo de contrato</TableHead>
                <TableHead>Carga</TableHead>
                <TableHead>Disponibilidad</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead>Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredTeachers.map((teacher) => {
                const workloadPercentage = (teacher.workload / teacher.maxWorkload) * 100;
                const isOverloaded = workloadPercentage > 100;
                const isLowLoad = workloadPercentage < 50;

                return (
                  <TableRow key={teacher.id} striped>
                    <TableCell>
                      <div>
                        <p className="font-medium text-[#333333]">{teacher.name}</p>
                        <p className="text-xs text-[#666666]">{teacher.area}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">
                        {teacher.contract === "TC" ? "Tiempo completo" : "Tiempo parcial"}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        <ProgressBar
                          value={teacher.workload}
                          max={teacher.maxWorkload}
                          variant={isOverloaded ? "error" : isLowLoad ? "warning" : "success"}
                          size="sm"
                        />
                        <p className="text-xs text-[#666666]">
                          {teacher.workload}h / {teacher.maxWorkload}h
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={
                          teacher.availability === "Validada"
                            ? "success"
                            : teacher.availability === "Pendiente"
                            ? "warning"
                            : "inactive"
                        }
                      >
                        {teacher.availability}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={
                          teacher.status === "Activo"
                            ? "success"
                            : teacher.status === "Alerta"
                            ? "error"
                            : "inactive"
                        }
                      >
                        {teacher.status}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <button className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]">
                          <Eye size={16} />
                        </button>
                        <button className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]">
                          <Edit size={16} />
                        </button>
                      </div>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <div className="grid grid-cols-3 gap-6">
        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#1A6BBF]/10 flex items-center justify-center">
              <TrendingUp className="text-[#1A6BBF]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Carga promedio</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.avgWorkload}h</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#E8A020]/10 flex items-center justify-center">
              <AlertCircle className="text-[#E8A020]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Sin disponibilidad</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.withoutAvailability}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#C0392B]/10 flex items-center justify-center">
              <Users className="text-[#C0392B]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Con sobrecarga</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.overloaded}</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
