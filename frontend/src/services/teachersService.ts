import api from './api';

export const obtenerDocentes = async () => {
  const response = await api.get('/profesores');
  return response.data;
};

export const crearDocente = async (data: any) => {
  const response = await api.post('/profesores', data);
  return response.data;
};

export const actualizarDocente = async (id: string, data: any) => {
  await api.put(`/profesores/${id}`, data);
};

export const eliminarDocente = async (id: string) => {
  await api.delete(`/profesores/${id}`);
};