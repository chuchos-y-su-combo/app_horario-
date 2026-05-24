import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { ConfirmModal } from "../Modal";
import { Search, Plus, Edit, Trash2, Loader2 } from "lucide-react";
import api from "../../../services/api";

interface Usuario {
  idUsuario: string;
  nombreCompleto: string;
  correo: string;
  idRol: number;
  nombreRol: string;
}

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

export function UsersView() {
  const [users, setUsers] = useState<Usuario[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<string | null>(null);

  useEffect(() => {
    cargarUsuarios();
  }, []);

  const cargarUsuarios = async () => {
    setLoading(true);
    try {
      const response = await api.get("/usuarios");
      setUsers(Array.isArray(response.data) ? response.data : []);
    } catch (error) {
      console.error("Error cargando usuarios:", error);
    } finally {
      setLoading(false);
    }
  };

  const filteredUsers = users.filter(
    (u) =>
      u.nombreCompleto.toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.correo.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <div className="text-center">
          <Loader2 className="w-8 h-8 animate-spin text-[#1A6BBF] mx-auto" />
          <p className="mt-4 text-[#666666]">Cargando usuarios...</p>
        </div>
      </div>
    );
  }

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
          {filteredUsers.length === 0 ? (
            <div className="text-center py-8 text-[#999999] text-sm">
              {searchTerm ? "No se encontraron usuarios con ese criterio." : "No hay usuarios registrados."}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Usuario</TableHead>
                  <TableHead>Rol</TableHead>
                  <TableHead>Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredUsers.map((user) => (
                  <TableRow key={user.idUsuario} striped>
                    <TableCell>
                      <div>
                        <p className="font-medium text-[#333333]">{user.nombreCompleto}</p>
                        <p className="text-xs text-[#666666]">{user.correo}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.nombreRol === "Administrador" ? "primary" : "secondary"}>
                        {user.nombreRol}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <button className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]">
                          <Edit size={16} />
                        </button>
                        <button
                          onClick={() => { setSelectedUser(user.idUsuario); setShowDeleteModal(true); }}
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
          )}
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
                    <Badge variant={perm.admin === "Completo" ? "success" : perm.admin === "Solo lectura" ? "warning" : "inactive"}>
                      {perm.admin}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={perm.coordinator === "Completo" ? "success" : perm.coordinator === "Solo lectura" ? "warning" : "inactive"}>
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
          console.log("Desactivar usuario:", selectedUser);
          setShowDeleteModal(false);
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
