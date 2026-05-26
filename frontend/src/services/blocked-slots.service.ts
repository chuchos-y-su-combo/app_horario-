import api from "./api";

/**
 * Servicio para la gestión de franjas bloqueadas en el sistema de horarios.
 * Los bloqueos impiden que el generador automático coloque asignaturas
 * en franjas reservadas para eventos institucionales, mantenimientos u otras restricciones.
 */
export const blockedSlotsService = {
  /**
   * Obtiene todos los bloqueos de franja registrados para un periodo académico.
   * Incluye bloqueos globales y bloqueos específicos por asignatura.
   * @param periodo Código del periodo académico, p.ej. "2025-1".
   */
  obtenerBloqueos: async (periodo: string) => {
    const response = await api.get(
      `/asignaturas/bloqueos-franja?periodo=${periodo}`
    );
    return response.data;
  },

  /**
   * Registra un bloqueo de franja para una asignatura específica.
   * Evita que esa asignatura sea agendada en el día y hora indicados.
   * @param idAsignatura ID de la asignatura a bloquear.
   * @param data Datos del bloqueo: periodo, día (1=Lunes…5=Viernes), horas y motivo opcional.
   */
  crearBloqueo: async (
    idAsignatura: string,
    data: {
      periodo: string;
      dia: number;
      horaInicio: string;
      horaFin: string;
      motivo?: string;
    }
  ) => {
    const response = await api.post(
      `/asignaturas/${idAsignatura}/bloqueos-franja`,
      data
    );
    return response.data;
  },

  /**
   * Crea un bloqueo global que aplica a todos los escenarios y asignaturas.
   * Se usa para bloquear franjas institucionales (actos, jornadas de inducción, etc.)
   * que afectan a todo el programa académico en el periodo indicado.
   * @param data Datos del bloqueo global: periodo, día, horas y motivo opcional.
   */
  crearBloqueoGlobal: async (data: {
    periodo: string;
    dia: number;
    horaInicio: string;
    horaFin: string;
    motivo?: string;
  }) => {
    const response = await api.post('/asignaturas/bloqueos-franja', data);
    return response.data;
  },

  /**
   * Elimina permanentemente un bloqueo de franja por su ID.
   * Tras eliminarlo, el generador vuelve a considerar esa franja como disponible.
   * @param idBloqueo ID del bloqueo a eliminar.
   */
  eliminarBloqueo: async (idBloqueo: string) => {
    await api.delete(`/asignaturas/bloqueos-franja/${idBloqueo}`);
  },
};
