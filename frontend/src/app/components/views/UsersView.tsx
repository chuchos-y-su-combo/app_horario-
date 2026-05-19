import { useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { ConfirmModal } from "../Modal";
import { Search, Plus, Edit, Trash2 } from "lucide-react";

export function UsersView() {
  const [searchTerm, setSearchTerm] = useState("");
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<string | null>(null);

  const users = [
    { id: "1", name: "Carlos Ramírez", email: "carlos.ramirez@autonoma.edu.co", area: "Ingeniería", role: "Administrador", status: "Activo", lastActivity: "Hace 2 horas" },
    { id: "2", name: "María López", email: "maria.lopez@autonoma.edu.co", area: "TAPSI", role: "Coordinador", status: "Activo", lastActivity: "Hace 1 día" },
    { id: "3", name: "Juan Torres", email: "juan.torres@autonoma.edu.co", area: "Ingeniería", role: "Coordinador", status: "Activo", lastActivity: "Hace 3 horas" },
    { id: "4", name: "Ana García", email: "ana.garcia@autonoma.edu.co", area: "TAPSI", role: "Coordinador", status: "Inactivo", lastActivity: "Hace 15 días" },
  ];

  const permissions = [
    { module: "Dashboard", admin: "Completo", coordinator: "Solo lectura" },
    { module: "Usuarios y roles", admin: "Completo", coordinator: "Sin acceso" },
    { module: "Docentes", admin: "Completo", coordinator: "Completo" },
    { module: "Asignaturas", admin: "Completo", coordinator: "Completo" },
    { module: "Generación", admin: "Completo", coordinator: "Solo lectura" },
    { module: "Ajuste manual", admin: "Completo", coordinator: "Completo" },
    { module: "Franjas bloqueadas", admin: "Completo", coordinator: "Completo" },
    { module: "Calendario", admin: "Completo", coordinator: "Solo lectura" },
    { module: "Alertas", admin: "Completo", coordinator: "Completo" },
    { module: "Reportes", admin: "Completo", coordinator: "Solo lectura" },
    { module: "Historial", admin: "Completo", coordinator: "Solo lectura" },
  ];

  const filteredUsers = users.filter(user =>
    user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    user.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleDeleteUser = (userId: string) => {
    setSelectedUser(userId);
    setShowDeleteModal(true);
  };

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Gestión de Usuarios y Roles</h1>
          <p className="text-sm text-[#666666] mt-1">Administración de acceso y permisos del sistema</p>
        </div>
        <Button className="gap-2">
          <Plus size={20} />
          Nuevo usuario
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Usuarios del Sistema</CardTitle>
          <div className="relative mt-4">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-[#999999]" size={20} />
            <Input
              placeholder="Buscar por nombre o correo..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Usuario</TableHead>
                <TableHead>Área</TableHead>
                <TableHead>Rol</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead>Última actividad</TableHead>
                <TableHead>Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredUsers.map((user) => (
                <TableRow key={user.id} striped>
                  <TableCell>
                    <div>
                      <p className="font-medium text-[#333333]">{user.name}</p>
                      <p className="text-xs text-[#666666]">{user.email}</p>
                    </div>
                  </TableCell>
                  <TableCell>{user.area}</TableCell>
                  <TableCell>
                    <Badge variant={user.role === "Administrador" ? "primary" : "secondary"}>
                      {user.role}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={user.status === "Activo" ? "success" : "error"}>
                      {user.status}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-[#666666]">{user.lastActivity}</TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <button className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]">
                        <Edit size={16} />
                      </button>
                      <button
                        onClick={() => handleDeleteUser(user.id)}
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

      <Card>
        <CardHeader>
          <CardTitle>Matriz de Permisos por Rol</CardTitle>
          <p className="text-sm text-[#666666] mt-1">Niveles de acceso por módulo del sistema</p>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Módulo</TableHead>
                <TableHead>Administrador</TableHead>
                <TableHead>Coordinador</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {permissions.map((perm, idx) => (
                <TableRow key={idx} striped>
                  <TableCell className="font-medium">{perm.module}</TableCell>
                  <TableCell>
                    <Badge
                      variant={
                        perm.admin === "Completo"
                          ? "success"
                          : perm.admin === "Solo lectura"
                          ? "warning"
                          : "inactive"
                      }
                    >
                      {perm.admin}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge
                      variant={
                        perm.coordinator === "Completo"
                          ? "success"
                          : perm.coordinator === "Solo lectura"
                          ? "warning"
                          : "inactive"
                      }
                    >
                      {perm.coordinator}
                    </Badge>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <ConfirmModal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        onConfirm={() => {
          console.log("Usuario desactivado:", selectedUser);
        }}
        title="Desactivar usuario"
        message="¿Está seguro que desea desactivar este usuario? Perderá acceso al sistema pero su información se mantendrá en el historial."
        confirmText="Desactivar"
        cancelText="Cancelar"
        variant="warning"
      />
    </div>
  );
}
