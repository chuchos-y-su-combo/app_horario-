import api from "./api";

/**
 * Servicio de generación y confirmación de propuestas de horario.
 * Agrupa los endpoints del módulo de generación automática.
 */
export const generationService = {
  /**
   * Dispara la generación automática de propuestas para los escenarios indicados.
   * Si `borrarPropuestasPrevias` es true, elimina las propuestas anteriores del periodo
   * antes de generar las nuevas.
   * @returns Resumen con propuestas creadas, asignaturas no asignadas y mensajes de diagnóstico.
   */
  generarPropuestas: async (data: {
    periodo: string;
    escenarios: string[];
    semestreIngenieria: number;
    borrarPropuestasPrevias: boolean;
  }) => {
    const response = await api.post("/horarios/generar-propuestas", data);
    return response.data;
  },

  /**
   * Devuelve todas las asignaciones en estado "Propuesta" para el periodo indicado.
   * Se usa en la vista de Generación para revisar el resultado antes de confirmar.
   */
  obtenerPropuestas: async (periodo: string) => {
    const response = await api.get(`/asignaciones/propuestas?periodo=${periodo}`);
    return response.data;
  },

  /**
   * Confirma un lote de propuestas cambiando su estado a "Confirmada".
   * @param idsAsignacion Lista de IDs de asignaciones a confirmar.
   * @returns Resultado de la confirmación con conteo de exitosas y fallidas.
   */
  confirmarAsignaciones: async (idsAsignacion: string[]) => {
    const response = await api.post("/asignaciones/confirmar", {
      idsAsignacion,
    });
    return response.data;
  },
};
