import api from './api';

export interface PlanEstudio {
  idPlan: string;
  nombrePlan: string;
  jornada: string;
}

export const obtenerPlanes = async (): Promise<PlanEstudio[]> => {
  const response = await api.get('/planes-estudio');
  return response.data;
};
