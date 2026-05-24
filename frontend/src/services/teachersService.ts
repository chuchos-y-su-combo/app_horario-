import api from './api';

export interface Docente {
  idDocente: string;
  identificacion: string;
  nombre: string;
  tipoContrato: string;
  maxAsignaturas: number;
}

export interface AsignaturaHabilitada {
  idAsignatura: string;
  codigo: string;
  nombre: string;
  creditos: number;
  semestre: number;
  fuente: string;
  fechaHabilitacion: string;
}

export const obtenerDocentes = async (): Promise<Docente[]> => {
  const response = await api.get('/profesores');
  return response.data;
};

export const crearDocente = async (data: Partial<Docente>): Promise<Docente> => {
  const response = await api.post('/profesores', data);
  return response.data;
};

export const actualizarDocente = async (id: string, data: Partial<Docente>): Promise<void> => {
  await api.put(`/profesores/${id}`, data);
};

export const eliminarDocente = async (id: string): Promise<void> => {
  await api.delete(`/profesores/${id}`);
};

export const getAsignaturasHabilitadas = async (
  idDocente: string
): Promise<AsignaturaHabilitada[]> => {
  const response = await api.get(`/profesores/${idDocente}/asignaturas-habilitadas`);
  return response.data;
};

export const habilitarAsignatura = async (
  idDocente: string,
  idAsignatura: string
): Promise<AsignaturaHabilitada> => {
  const response = await api.post(`/profesores/${idDocente}/asignaturas-habilitadas`, {
    idAsignatura,
  });
  return response.data;
};

export const desvincularAsignatura = async (
  idDocente: string,
  idAsignatura: string
): Promise<void> => {
  await api.delete(`/profesores/${idDocente}/asignaturas-habilitadas/${idAsignatura}`);
};
