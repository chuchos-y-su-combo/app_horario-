import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import { ConfirmModal } from "../Modal";
import { Search, Plus, Edit, Trash2, Loader2, X } from "lucide-react";
import api from "../../../services/api";

interface Usuario {
  idUsuario: string;
  nombreCompleto: string;
  correo: string;
  idRol: number;
  nombreRol: string;
}

interface FormUsuario {
  nombreCompleto: string;
  correo: string;
  password: string;
  idRol: number;
}

const ROLES = [
  { value: "1", label: "Administrador" },
  { value: "2", label: "Coordinador" },
];

const permissions = [
  { module: "Dashboard",          admin: "Completo", coordinator: "Completo" },
  { module: "Usuarios y roles",   admin: "Completo", coordinator: "Sin acceso" },
  { module: "Docentes",           admin: "Completo", coordinator: "Completo" },
  { module: "Asignaturas",        admin: "Completo", coordinator: "Completo" },
  { module: "Generación",         admin: "Completo", coordinator: "Completo" },
  { module: "Ajuste manual",      admin: "Completo", coordinator: "Completo" },
  { module: "Franjas bloqueadas", admin: "Completo", coordinator: "Completo" },
  { module: "Calendario",         admin: "Completo", coordinator: "Completo" },
  { module: "Alertas",            admin: "Completo", coordinator: "Completo" },
  { module: "Reportes",           admin: "Completo", coordinator: "Completo" },
  { module: "Historial",          admin: "Completo", coordinator: "Solo lectura" },
];

const FORM_VACIO: FormUsuario = { nombreCompleto: "", correo: "", password: "", idRol: 2 };

export function UsersView() {
  const [users, setUsers] = useState<Usuario[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<Usuario | null>(null);
  const [form, setForm] = useState<FormUsuario>(FORM_VACIO);
  const [guardando, setGuardando] = useState(false);
  const [eliminando, setEliminando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [errorCarga, setErrorCarga] = useState<string | null>(null);

  useEffect(() => {
    cargarUsuarios();
  }, []);

  const cargarUsuarios = async () => {
    setLoading(true);
    setErrorCarga(null);
    try {
      const response = await api.get("/usuarios");
      setUsers(Array.isArray(response.data) ? response.data : []);
    } catch (err: any) {
      const msg = err?.response?.status === 403
        ? "No tienes permiso para ver los usuarios. Solo los administradores pueden acceder."
        : (err?.response?.data?.mensaje || "No se pudo cargar la lista de usuarios. Verifica la conexión.");
      setErrorCarga(msg);
      setUsers([]);
    } finally {
      setLoading(false);
    }
  };

  const abrirCrear = () => {
    setForm(FORM_VACIO);
    setError(null);
    setShowCreateModal(true);
  };

  const abrirEditar = (user: Usuario) => {
    setSelectedUser(user);
    setForm({ nombreCompleto: user.nombreCompleto, correo: user.correo, password: "", idRol: user.idRol });
    setError(null);
    setShowEditModal(true);
  };

  const abrirEliminar = (user: Usuario) => {
    setSelectedUser(user);
    setShowDeleteModal(true);
  };

  const handleCrear = async () => {
    if (!form.nombreCompleto || !form.correo || !form.password) {
      setError("Complete todos los campos obligatorios.");
      return;
    }
    setGuardando(true);
    setError(null);
    try {
      await api.post("/usuarios", {
        nombreCompleto: form.nombreCompleto,
        correo: form.correo,
        password: form.password,
        idRol: form.idRol,
      });
      setShowCreateModal(false);
      cargarUsuarios();
    } catch (err: any) {
      setError(err?.response?.data?.mensaje || "Error al crear el usuario.");
    } finally {
      setGuardando(false);
    }
  };

  const handleEditar = async () => {
    if (!selectedUser || !form.nombreCompleto || !form.correo) {
      setError("Complete todos los campos obligatorios.");
      return;
    }
    setGuardando(true);
    setError(null);
    try {
      await api.put(`/usuarios/${selectedUser.idUsuario}`, {
        nombreCompleto: form.nombreCompleto,
        correo: form.correo,
        idRol: form.idRol,
      });
      setShowEditModal(false);
      cargarUsuarios();
    } catch (err: any) {
      setError(err?.response?.data?.mensaje || "Error al actualizar el usuario.");
    } finally {
      setGuardando(false);
    }
  };

  const handleEliminar = async () => {
    if (!selectedUser) return;
    setEliminando(true);
    try {
      await api.delete(`/usuarios/${selectedUser.idUsuario}`);
      setShowDeleteModal(false);
      setSelectedUser(null);
      cargarUsuarios();
    } catch (err: any) {
      setError(err?.response?.data?.mensaje || "Error al eliminar el usuario.");
    } finally {
      setEliminando(false);
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
        <Button className="gap-2" onClick={abrirCrear}>
          <Plus size={20} />
          Nuevo usuario
        </Button>
      </div>

      {errorCarga && (
        <div className="bg-[#C0392B]/10 border border-[#C0392B]/30 text-[#C0392B] rounded p-3 text-sm">
          {errorCarga}
        </div>
      )}

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
                        <button
                          onClick={() => abrirEditar(user)}
                          className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]"
                        >
                          <Edit size={16} />
                        </button>
                        <button
                          onClick={() => abrirEliminar(user)}
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

      {/* Modal: Crear usuario */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-lg w-full max-w-md p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold text-[#333333]">Nuevo usuario</h2>
              <button onClick={() => setShowCreateModal(false)} className="text-[#999999] hover:text-[#333333]">
                <X size={20} />
              </button>
            </div>
            {error && (
              <div className="mb-3 p-2 bg-[#C0392B]/10 border border-[#C0392B]/30 text-[#C0392B] rounded text-sm">
                {error}
              </div>
            )}
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Nombre completo *</label>
                <Input value={form.nombreCompleto} onChange={(e) => setForm({ ...form, nombreCompleto: e.target.value })} placeholder="Nombre completo" />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Correo electrónico *</label>
                <Input type="email" value={form.correo} onChange={(e) => setForm({ ...form, correo: e.target.value })} placeholder="correo@universidad.edu.co" />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Contraseña * (mín. 8 caracteres)</label>
                <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} placeholder="••••••••" />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Rol</label>
                <Select
                  value={String(form.idRol)}
                  onChange={(e) => setForm({ ...form, idRol: parseInt(e.target.value) })}
                  options={ROLES}
                />
              </div>
            </div>
            <div className="flex gap-3 mt-6 justify-end">
              <button onClick={() => setShowCreateModal(false)} className="px-4 py-2 text-sm font-medium text-[#666666] bg-[#F5F5F5] rounded hover:bg-[#E8E8E8] transition-colors">
                Cancelar
              </button>
              <button onClick={handleCrear} disabled={guardando} className="px-4 py-2 text-sm font-medium text-white bg-[#1A6BBF] rounded hover:bg-[#155BA0] transition-colors disabled:opacity-50 flex items-center gap-2">
                {guardando && <Loader2 size={14} className="animate-spin" />}
                {guardando ? "Creando..." : "Crear"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Editar usuario */}
      {showEditModal && selectedUser && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-lg w-full max-w-md p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold text-[#333333]">Editar usuario</h2>
              <button onClick={() => setShowEditModal(false)} className="text-[#999999] hover:text-[#333333]">
                <X size={20} />
              </button>
            </div>
            {error && (
              <div className="mb-3 p-2 bg-[#C0392B]/10 border border-[#C0392B]/30 text-[#C0392B] rounded text-sm">
                {error}
              </div>
            )}
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Nombre completo *</label>
                <Input value={form.nombreCompleto} onChange={(e) => setForm({ ...form, nombreCompleto: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Correo electrónico *</label>
                <Input type="email" value={form.correo} onChange={(e) => setForm({ ...form, correo: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Rol</label>
                <Select
                  value={String(form.idRol)}
                  onChange={(e) => setForm({ ...form, idRol: parseInt(e.target.value) })}
                  options={ROLES}
                />
              </div>
            </div>
            <div className="flex gap-3 mt-6 justify-end">
              <button onClick={() => setShowEditModal(false)} className="px-4 py-2 text-sm font-medium text-[#666666] bg-[#F5F5F5] rounded hover:bg-[#E8E8E8] transition-colors">
                Cancelar
              </button>
              <button onClick={handleEditar} disabled={guardando} className="px-4 py-2 text-sm font-medium text-white bg-[#1A6BBF] rounded hover:bg-[#155BA0] transition-colors disabled:opacity-50 flex items-center gap-2">
                {guardando && <Loader2 size={14} className="animate-spin" />}
                {guardando ? "Guardando..." : "Guardar cambios"}
              </button>
            </div>
          </div>
        </div>
      )}

      <ConfirmModal
        isOpen={showDeleteModal}
        onClose={() => { setShowDeleteModal(false); setSelectedUser(null); }}
        onConfirm={handleEliminar}
        title="Eliminar usuario"
        message={`¿Está seguro que desea eliminar a "${selectedUser?.nombreCompleto}"? Esta acción no se puede deshacer.`}
        confirmText={eliminando ? "Eliminando..." : "Eliminar"}
        cancelText="Cancelar"
        variant="danger"
      />
    </div>
  );
}
