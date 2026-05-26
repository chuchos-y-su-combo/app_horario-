import api from "./api";

/**
 * Servicio para el módulo de ajuste manual de asignaciones.
 * Permite consultar, reasignar, cancelar y ajustar franjas horarias
 * de las asignaciones generadas automáticamente o creadas manualmente.
 */
export const manualAdjustmentService = {
  /**
   * Obtiene todas las asignaciones del sistema (cualquier estado y periodo).
   * Se usa como fuente de datos para la tabla de ajuste manual.
   */
  obtenerAsignaciones: async () => {
    const response = await api.get("/asignaciones");
    return response.data;
  },

  /**
   * Obtiene las asignaciones en estado "Propuesta" para un periodo específico.
   * Las propuestas son el resultado de la generación automática antes de ser confirmadas.
   * @param periodo Código del periodo académico, p.ej. "2025-1".
   */
  obtenerPropuestas: async (periodo: string) => {
    const response = await api.get(
      `/asignaciones/propuestas?periodo=${periodo}`
    );
    return response.data;
  },

  /**
   * Ajusta parcialmente una asignación existente: permite cambiar el docente,
   * la asignatura, el día o la franja horaria sin recrear la asignación completa.
   * @param idAsignacion ID de la asignación a modificar.
   * @param data Campos a actualizar (todos opcionales; solo se envían los que cambian).
   */
  ajustarAsignacion: async (
    idAsignacion: string,
    data: {
      idDocente?: string;
      idAsignatura?: string;
      dia?: number;
      horaInicio?: string;
      horaFin?: string;
      periodo?: string;
    }
  ) => {
    const response = await api.patch(
      `/asignaciones/${idAsignacion}/ajustar`,
      data
    );
    return response.data;
  },

  /**
   * Marca una asignación como "Cancelada", retirándola del horario activo.
   * La asignación permanece en base de datos para el historial de cambios.
   * @param idAsignacion ID de la asignación a cancelar.
   */
  cancelarAsignacion: async (idAsignacion: string) => {
    const response = await api.patch(
      `/asignaciones/${idAsignacion}/cancelar`
    );
    return response.data;
  },

  /**
   * Asigna o actualiza únicamente el día y la franja horaria de una asignación,
   * sin modificar el docente ni la asignatura.
   * Útil para mover un bloque de horario a otro día sin cambiar el resto.
   * @param idAsignacion ID de la asignación a reposicionar.
   * @param data Día (1=Lunes … 5=Viernes) y opcionalmente horas de inicio y fin.
   */
  asignarDia: async (
    idAsignacion: string,
    data: {
      dia: number;
      horaInicio?: string;
      horaFin?: string;
    }
  ) => {
    const response = await api.patch(
      `/asignaciones/${idAsignacion}/asignar-dia`,
      data
    );
    return response.data;
  },
};
