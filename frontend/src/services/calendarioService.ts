import { CalendarioResponse } from '../interfaces/Calendario/CalendarioResponse';
import api from './api';

/**
 * Servicio de consulta del calendario semanal de horarios.
 * Proporciona vistas del horario por programa/jornada y por docente individual,
 * así como acceso a los catálogos de docentes y asignaturas necesarios para los filtros.
 */
export const calendarioService = {
  /**
   * Obtiene la grilla semanal (Lunes–Viernes) del horario de un periodo académico,
   * filtrada opcionalmente por plan de estudios, jornada y número de semestre.
   * Se usa en CalendarView para mostrar el calendario del programa.
   * @param semestre Código del periodo académico, p.ej. "2025-1".
   * @param idPlan ID del plan de estudios (ING_DIURNA, TAPSI_NOCTURNA, etc.). Opcional.
   * @param jornada "Diurna" o "Nocturna". Opcional.
   * @param semestreAsignatura Número de semestre del plan (1–10). 0 o ausente = todos.
   */
  getCalendarioSemanal: async (
    semestre: string,
    idPlan?: string,
    jornada?: string,
    semestreAsignatura?: number,
  ): Promise<CalendarioResponse> => {
    const params = new URLSearchParams();
    params.append('semestre', semestre);
    if (idPlan) params.append('idPlan', idPlan);
    if (jornada) params.append('jornada', jornada);
    if (semestreAsignatura && semestreAsignatura > 0)
      params.append('semestreAsignatura', String(semestreAsignatura));
    const response = await api.get(`/horarios/calendario?${params}`);
    return response.data;
  },

  /**
   * Obtiene la grilla semanal personalizada de un docente concreto.
   * Muestra todas las asignaturas que ese docente tiene asignadas en el periodo,
   * independientemente del plan o jornada.
   * @param idDocente ID del docente.
   * @param semestre Código del periodo académico.
   */
  getCalendarioDocente: async (idDocente: string, semestre: string) => {
    const response = await api.get(`/horarios/calendario/docente/${idDocente}?semestre=${semestre}`);
    return response.data;
  },

  /**
   * Obtiene la lista de todos los docentes para poblar el selector de filtro por docente.
   * Reutiliza el endpoint de profesores del módulo de gestión de docentes.
   */
  getDocentes: async () => {
    const response = await api.get('/profesores');
    return response.data;
  },

  /**
   * Dispara la generación automática de propuestas de horario desde el calendario.
   * Siempre borra las propuestas previas del periodo antes de generar las nuevas.
   * @param periodo Código del periodo académico.
   * @param escenarios Lista de escenarios a generar (p.ej. ["ING_DIURNA", "ING_NOCTURNA"]).
   * @param semestreIngenieria Semestre de Ingeniería a generar (1–10).
   */
  generarPropuestas: async (periodo: string, escenarios: string[], semestreIngenieria: number) => {
    const response = await api.post('/horarios/generar-propuestas', {
      periodo,
      escenarios,
      semestreIngenieria,
      borrarPropuestasPrevias: true
    });
    return response.data;
  },

  /**
   * Obtiene el catálogo completo de asignaturas.
   * Se usa en CalendarView para poblar el selector de filtro por asignatura.
   */
  getAsignaturas: async () => {
    const response = await api.get('/asignaturas');
    return response.data;
  }
};
