import api from './api';

/** Representa un plan de estudios (p.ej. Ingeniería Diurna o TAPSI Nocturna). */
export interface PlanEstudio {
  idPlan: string;
  /** Nombre descriptivo del plan, como "Ingeniería de Sistemas Diurna". */
  nombrePlan: string;
  /** Jornada académica: "Diurna" o "Nocturna". */
  jornada: string;
}

/**
 * Obtiene todos los planes de estudio registrados en el sistema.
 * Se usa en los selectores de filtro de asignaturas y de calendario
 * para que el usuario elija entre los cuatro escenarios disponibles.
 */
export const obtenerPlanes = async (): Promise<PlanEstudio[]> => {
  const response = await api.get('/planes-estudio');
  return response.data;
};
