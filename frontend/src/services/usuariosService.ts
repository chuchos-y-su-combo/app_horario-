import api from './api';

export const obtenerUsuarios = async () => {
  const response = await api.get('/usuarios');
  return response.data;
};

export const crearUsuario = async (data: any) => {
  const response = await api.post('/usuarios', data);
  return response.data;
};

export const actualizarUsuario = async (id: string, data: any) => {
  await api.put(`/usuarios/${id}`, data);
};

export const eliminarUsuario = async (id: string) => {
  await api.delete(`/usuarios/${id}`);
};