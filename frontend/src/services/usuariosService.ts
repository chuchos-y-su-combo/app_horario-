import api from './api';

/**
 * Obtiene la lista de todos los usuarios del sistema con sus datos y roles.
 * Solo accesible por usuarios con rol Administrador.
 */
export const obtenerUsuarios = async () => {
  const response = await api.get('/usuarios');
  return response.data;
};

/**
 * Crea un nuevo usuario con los datos indicados (nombre, correo, contraseña, rol).
 * El backend valida que el correo no esté duplicado y que el rol exista.
 * @param data Datos del nuevo usuario (tipado como any para flexibilidad según el formulario).
 */
export const crearUsuario = async (data: any) => {
  const response = await api.post('/usuarios', data);
  return response.data;
};

/**
 * Actualiza parcialmente un usuario existente (p.ej. cambio de rol o nombre).
 * No modifica la contraseña; para eso existe el flujo de recuperación de contraseña.
 * @param id ID del usuario a actualizar.
 * @param data Campos a actualizar.
 */
export const actualizarUsuario = async (id: string, data: any) => {
  await api.put(`/usuarios/${id}`, data);
};

/**
 * Elimina permanentemente un usuario del sistema.
 * Operación irreversible; requiere confirmar en el modal antes de llamar este método.
 * @param id ID del usuario a eliminar.
 */
export const eliminarUsuario = async (id: string) => {
  await api.delete(`/usuarios/${id}`);
};
