import api from './api';

/** Datos de un docente tal como los devuelve el backend (normalizado desde idProfesor → idDocente). */
export interface Docente {
  idDocente: string;
  identificacion: string;
  nombre: string;
  /** TC = Tiempo Completo · TP = Tiempo Parcial */
  tipoContrato: string;
  /** Número máximo de asignaturas distintas que puede recibir en un periodo. */
  maxAsignaturas: number;
}

/** Asignatura para la que un docente está habilitado según su currículo importado. */
export interface AsignaturaHabilitada {
  idAsignatura: string;
  codigo: string;
  nombre: string;
  creditos: number;
  semestre: number;
  /** Origen de la habilitación: normalmente "Excel" tras importar el currículo. */
  fuente: string;
  fechaHabilitacion: string;
}

/**
 * Obtiene la lista completa de docentes.
 * Normaliza el campo `idProfesor` del backend al nombre `idDocente` del frontend.
 */
export const obtenerDocentes = async (): Promise<Docente[]> => {
  const response = await api.get('/profesores');
  return (response.data as any[]).map((p) => ({
    idDocente: p.idProfesor,
    identificacion: p.identificacion,
    nombre: p.nombre,
    tipoContrato: p.tipoContrato,
    maxAsignaturas: p.maxAsignaturas,
  }));
};

/** Crea un nuevo docente con los datos básicos de contrato. */
export const crearDocente = async (data: Partial<Docente>): Promise<Docente> => {
  const response = await api.post('/profesores', data);
  return response.data;
};

/** Actualiza los datos de contrato de un docente existente. */
export const actualizarDocente = async (id: string, data: Partial<Docente>): Promise<void> => {
  await api.put(`/profesores/${id}`, data);
};

/** Elimina permanentemente un docente y todas sus disponibilidades y habilitaciones. */
export const eliminarDocente = async (id: string): Promise<void> => {
  await api.delete(`/profesores/${id}`);
};

/** Obtiene las asignaturas para las que el docente está habilitado según su currículo. */
export const getAsignaturasHabilitadas = async (
  idDocente: string
): Promise<AsignaturaHabilitada[]> => {
  const response = await api.get(`/profesores/${idDocente}/asignaturas-habilitadas`);
  return response.data;
};

/**
 * Habilita manualmente una asignatura para un docente.
 * Registra la habilitación con fuente "Manual".
 */
export const habilitarAsignatura = async (
  idDocente: string,
  idAsignatura: string
): Promise<AsignaturaHabilitada> => {
  const response = await api.post(`/profesores/${idDocente}/asignaturas-habilitadas`, {
    idAsignatura,
  });
  return response.data;
};

/** Desvincula una asignatura del currículo de un docente. */
export const desvincularAsignatura = async (
  idDocente: string,
  idAsignatura: string
): Promise<void> => {
  await api.delete(`/profesores/${idDocente}/asignaturas-habilitadas/${idAsignatura}`);
};
